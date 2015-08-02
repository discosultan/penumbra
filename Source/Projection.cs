using System;

namespace Penumbra
{
    [Flags]
    public enum Projections
    {
        SpriteBatch                 = 1 << 0,
        OriginCenter_XRight_YUp     = 1 << 1,
        OriginBottomLeft_XRight_YUp = 1 << 2,
        Custom                      = 1 << 3
    }
}
