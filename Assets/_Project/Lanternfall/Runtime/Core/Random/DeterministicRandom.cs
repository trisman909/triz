using System;

namespace Lanternfall.Core.Random
{

/// <summary>Fixed SplitMix64 stream for reproducible runs.</summary>
public sealed class DeterministicRandom
{
    private ulong _state;

    public DeterministicRandom(ulong seed) => _state = seed;

    public ulong NextUInt64()
    {
        _state += 0x9E3779B97F4A7C15UL;
        ulong value = _state;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }

    public int NextInt(int maximum)
    {
        if (maximum <= 0) throw new ArgumentOutOfRangeException(nameof(maximum));
        return (int)(NextUInt64() % (uint)maximum);
    }

    public float NextFloat() => (NextUInt64() >> 40) * (1f / 16777216f);
}

public static class SeedDerivation
{
    public static ulong Derive(ulong master, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
        ulong hash = 14695981039346656037UL ^ master;
        foreach (char character in name)
        {
            hash ^= character;
            hash *= 1099511628211UL;
        }
        hash ^= hash >> 33;
        hash *= 0xff51afd7ed558ccdUL;
        return hash ^ (hash >> 33);
    }
}

}
