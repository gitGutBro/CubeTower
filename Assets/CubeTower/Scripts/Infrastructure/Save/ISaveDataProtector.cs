namespace CubeTower.Infrastructure.Save
{
    /// <summary>
    /// Transforms save bytes before they reach the disk (encryption, signing).
    /// Implementations must be thread-safe: they are called from a background thread.
    /// </summary>
    public interface ISaveDataProtector
    {
        byte[] Protect(byte[] data);

        /// <summary>Returns false if the data was modified, corrupted or has an unknown format.</summary>
        bool TryUnprotect(byte[] protectedData, out byte[] data);
    }
}
