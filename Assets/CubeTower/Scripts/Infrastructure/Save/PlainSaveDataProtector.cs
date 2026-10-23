namespace CubeTower.Infrastructure.Save
{
    /// <summary>
    /// No protection: the save file is plain JSON. Handy for debugging.
    /// </summary>
    public sealed class PlainSaveDataProtector : ISaveDataProtector
    {
        public byte[] Protect(byte[] data) => 
            data;

        public bool TryUnprotect(byte[] protectedData, out byte[] data)
        {
            data = protectedData;
            return protectedData != null;
        }
    }
}
