using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CubeTower.Infrastructure.Save;
using CubeTower.Infrastructure.Save.Crypto;
using NUnit.Framework;

namespace CubeTower.Tests.EditMode
{
    public sealed class SaveFileTests
    {
        private static AesHmacSaveDataProtector _protector;
        private string _directory;
        private string _path;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Key derivation is deliberately slow (PBKDF2): derive once for all tests.
            _protector = AesHmacSaveDataProtector.CreateInitialized("com.test.cubetower");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => 
            _protector.Dispose();

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "cubetower-tests-" + Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_directory, "save.dat");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }

        [Test]
        public void Protector_RoundTrips_AndHidesPlainText()
        {
            byte[] plain = Encoding.UTF8.GetBytes("{\"secret\":\"tower\"}");

            byte[] protectedBytes = _protector.Protect(plain);

            Assert.That(Encoding.UTF8.GetString(protectedBytes).Contains("tower"), Is.False);
            Assert.That(_protector.TryUnprotect(protectedBytes, out byte[] restored), Is.True);
            Assert.That(restored, Is.EqualTo(plain));
        }

        [Test]
        public void Protector_UsesRandomIv()
        {
            byte[] plain = Encoding.UTF8.GetBytes("same data");

            Assert.That(_protector.Protect(plain), Is.Not.EqualTo(_protector.Protect(plain)));
        }

        [Test]
        public void Protector_RejectsTamperedOrTruncatedData()
        {
            byte[] protectedBytes = _protector.Protect(Encoding.UTF8.GetBytes("data"));

            byte[] tampered = protectedBytes.ToArray();
            tampered[tampered.Length / 2] ^= 0x01;
            byte[] truncated = protectedBytes.Take(protectedBytes.Length - 1).ToArray();

            Assert.That(_protector.TryUnprotect(tampered, out _), Is.False);
            Assert.That(_protector.TryUnprotect(truncated, out _), Is.False);
            Assert.That(_protector.TryUnprotect(Encoding.UTF8.GetBytes("plain json"), out _), Is.False);
        }

        [Test]
        public void FileSaver_WritesAndReadsEncryptedState()
        {
            JsonFileGameStateSaver saver = new JsonFileGameStateSaver(_protector, new SilentLog(), _path);
            Dictionary<string, string> state = new Dictionary<string, string> { ["tower"] = "{\"v\":1}" };

            saver.SaveImmediate(state);
            IReadOnlyDictionary<string, string> loaded = saver.Load();

            Assert.That(loaded["tower"], Is.EqualTo("{\"v\":1}"));
            Assert.That(File.Exists(_path + ".tmp"), Is.False, "Temp file must be cleaned up.");
        }

        [Test]
        public void FileSaver_MovesBrokenFileToBackup_AndStartsEmpty()
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(_path, "garbage");
            SilentLog log = new();
            JsonFileGameStateSaver saver = new(_protector, log, _path);

            IReadOnlyDictionary<string, string> loaded = saver.Load();

            Assert.That(loaded, Is.Empty);
            Assert.That(File.Exists(_path), Is.False);
            Assert.That(Directory.GetFiles(_directory, "*.bak"), Has.Length.EqualTo(1));
            Assert.That(log.Warnings, Is.Not.Empty);
        }

        [Test]
        public void FileSaver_MissingFile_GivesEmptyState()
        {
            JsonFileGameStateSaver saver = new(_protector, new SilentLog(), _path);

            Assert.That(saver.Load(), Is.Empty);
        }
    }
}
