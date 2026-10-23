using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CubeTower.Application.Lifecycle;
using Cysharp.Threading.Tasks;

namespace CubeTower.Infrastructure.Save.Crypto
{
    /// <summary>
    /// AES-256-CBC + HMAC-SHA256 (encrypt-then-MAC).
    /// AES-GCM would be simpler, but AesGcm is not available in .NET Standard 2.1 / Unity Mono.
    /// The MAC rejects modified or truncated files before they reach the JSON parser.
    /// <para>File format: [magic "CTSV" 4b][version 1b][IV 16b][ciphertext][HMAC 32b].</para>
    /// <para>
    /// Thread safety: keys are immutable after initialization; Aes/HMAC objects are not
    /// thread-safe, so they are created per call and disposed with <c>using</c>.
    /// </para>
    /// </summary>
    public sealed class AesHmacSaveDataProtector : ISaveDataProtector, IAsyncInitializable, IDisposable
    {
        private const byte FormatVersion = 1;
        private const int IvSize = 16;
        private const int MacSize = 32;
        private const int BlockSize = 16;

        private static readonly byte[] Magic = { (byte)'C', (byte)'T', (byte)'S', (byte)'V' };
        private static readonly int HeaderSize = Magic.Length + 1;

        private readonly string _salt;
        private byte[] _encryptionKey;
        private byte[] _macKey;
        private bool _isDisposed;

        /// <param name="salt">Application identifier. Read it on the main thread: Unity APIs are not thread-safe.</param>
        public AesHmacSaveDataProtector(string salt) => 
            _salt = salt ?? string.Empty;

        public bool IsInitialized => _encryptionKey != null;

        /// <summary>Creates a ready-to-use protector synchronously. For editor tools and tests.</summary>
        public static AesHmacSaveDataProtector CreateInitialized(string salt)
        {
            AesHmacSaveDataProtector protector = new(salt);
            protector.SetKeys(DeriveKeys(protector._salt));
            return protector;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;

            // PBKDF2 with 100k iterations takes up to a second on weak phones.
            // It is pure CPU work on immutable input, so it runs on the thread pool;
            // the continuation returns to the main thread (configureAwait: true by default).
            string salt = _salt;
            byte[] keys = await UniTask.RunOnThreadPool(
                () => DeriveKeys(salt), cancellationToken: cancellationToken);
            SetKeys(keys);
        }

        public byte[] Protect(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            EnsureReady();

            using Aes aes = CreateAes();
            aes.GenerateIV();

            byte[] cipher;
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                cipher = encryptor.TransformFinalBlock(data, 0, data.Length);
            }

            byte[] result = new byte[HeaderSize + IvSize + cipher.Length + MacSize];
            Buffer.BlockCopy(Magic, 0, result, 0, Magic.Length);
            result[Magic.Length] = FormatVersion;
            Buffer.BlockCopy(aes.IV, 0, result, HeaderSize, IvSize);
            Buffer.BlockCopy(cipher, 0, result, HeaderSize + IvSize, cipher.Length);

            byte[] mac = ComputeMac(result, result.Length - MacSize);
            Buffer.BlockCopy(mac, 0, result, result.Length - MacSize, MacSize);
            return result;
        }

        public bool TryUnprotect(byte[] protectedData, out byte[] data)
        {
            data = null;
            EnsureReady();

            if (protectedData == null || protectedData.Length < HeaderSize + IvSize + BlockSize + MacSize)
                return false;

            for (int i = 0; i < Magic.Length; i++)
                if (protectedData[i] != Magic[i])
                    return false;

            if (protectedData[Magic.Length] != FormatVersion)
                return false;

            int macOffset = protectedData.Length - MacSize;
            byte[] expectedMac = ComputeMac(protectedData, macOffset);
            
            if (!FixedTimeEquals(expectedMac, protectedData, macOffset))
                return false;

            byte[] iv = new byte[IvSize];
            Buffer.BlockCopy(protectedData, HeaderSize, iv, 0, IvSize);
            int cipherOffset = HeaderSize + IvSize;
            int cipherLength = macOffset - cipherOffset;

            try
            {
                using Aes aes = CreateAes();
                aes.IV = iv;
                
                using ICryptoTransform decryptor = aes.CreateDecryptor();
                data = decryptor.TransformFinalBlock(protectedData, cipherOffset, cipherLength);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            // Do not leave key material in memory longer than needed.
            if (_encryptionKey != null)
                Array.Clear(_encryptionKey, 0, _encryptionKey.Length);

            if (_macKey != null)
                Array.Clear(_macKey, 0, _macKey.Length);
        }

        private static byte[] DeriveKeys(string salt)
        {
            byte[] password = Encoding.UTF8.GetBytes(SaveCryptoSecrets.MasterSecret);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt + SaveCryptoSecrets.SaltSuffix);

            using Rfc2898DeriveBytes kdf = new
                (password, saltBytes, SaveCryptoSecrets.Pbkdf2Iterations, HashAlgorithmName.SHA256);
            
            // One master derivation, split into two independent keys: AES and HMAC.
            return kdf.GetBytes(SaveCryptoSecrets.KeySizeBytes * 2);
        }

        /// <summary>Constant-time comparison: does not leak how many MAC bytes matched.</summary>
        private static bool FixedTimeEquals(byte[] expected, byte[] buffer, int offset)
        {
            int difference = 0;
            
            for (int i = 0; i < expected.Length; i++)
                difference |= expected[i] ^ buffer[offset + i];

            return difference == 0;
        }

        private void SetKeys(byte[] keys)
        {
            byte[] encryptionKey = new byte[SaveCryptoSecrets.KeySizeBytes];
            byte[] macKey = new byte[SaveCryptoSecrets.KeySizeBytes];
            Buffer.BlockCopy(keys, 0, encryptionKey, 0, SaveCryptoSecrets.KeySizeBytes);
            Buffer.BlockCopy(keys, SaveCryptoSecrets.KeySizeBytes, macKey, 0, SaveCryptoSecrets.KeySizeBytes);
            Array.Clear(keys, 0, keys.Length);

            _macKey = macKey;
            _encryptionKey = encryptionKey;
        }

        private Aes CreateAes()
        {
            Aes aes = Aes.Create();
            aes.KeySize = SaveCryptoSecrets.KeySizeBytes * 8;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _encryptionKey;
            return aes;
        }

        private byte[] ComputeMac(byte[] buffer, int count)
        {
            using HMACSHA256 hmac = new(_macKey);
            return hmac.ComputeHash(buffer, 0, count);
        }

        private void EnsureReady()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(AesHmacSaveDataProtector));

            if (!IsInitialized)
                throw new InvalidOperationException("Save encryption keys are not initialized yet.");
        }
    }
}
