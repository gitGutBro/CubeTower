namespace CubeTower.Infrastructure.Save.Crypto
{
    /// <summary>
    /// Secrets of save encryption.
    /// This is NOT real security: these strings are compiled into the build
    /// (with IL2CPP they end up in global-metadata.dat in plain text) and the source is shared with the project.
    /// The goal is to stop casual manual editing and to detect corrupted files.
    /// In a real project the master secret is injected by CI at build time and is not kept in git.
    /// </summary>
    internal static class SaveCryptoSecrets
    {
        public const string MasterSecret = "CubeTower-7c1f4e2a9b8d4f61a3e5c0d2b6f8a9e1";

        /// <summary>Appended to the application identifier to form the PBKDF2 salt (min 8 bytes).</summary>
        public const string SaltSuffix = ".cubetower.save.v1";
        
        public const int Pbkdf2Iterations = 100_000;
        public const int KeySizeBytes = 32;
    }
}
