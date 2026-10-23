using System;

namespace CubeTower.Core.Common
{
    public sealed class GuidIdGenerator : IIdGenerator
    {
        public string Next() => 
            Guid.NewGuid().ToString("N");
    }
}
