using System;

namespace Player.Data
{
    [Flags]
    public enum StationaryEffect
    {
        None = 0,
        Wind = 1,
        Shockwave = 2,
        Smoke = 4
    }
}