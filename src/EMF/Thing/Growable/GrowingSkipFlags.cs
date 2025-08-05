using System;

namespace EMF
{
    [Flags]
    public enum GrowingSkipFlags
    {
        none,
        Walls = 2,
        Doors = 4,
        Floors = 8,
        Power = 16,
        Furniture = 32,
        Other = 64,
        All = 128
    }
}