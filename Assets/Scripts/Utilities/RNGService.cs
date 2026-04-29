public static class RNGService
{
    private static System.Random _rng = new System.Random();
    public static int Next(int max) => _rng.Next(max);
    public static int Next(int min, int max) => _rng.Next(min, max);
    public static float NextFloat() => (float)_rng.NextDouble();
    public static void SetSeed(int seed) => _rng = new System.Random(seed);
}