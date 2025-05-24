using X3UR.Domain.Interfaces;

namespace X3UR.Infrastructure.Randoms;
public class RandomProvider : IRandomProvider {
    private Random _rnd;

    public RandomProvider(ISeedProvider seedProvider) {
        _rnd = CreateRandomFromSeed(seedProvider.Seed);
        seedProvider.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(seedProvider.Seed))
                _rnd = CreateRandomFromSeed(seedProvider.Seed);
        };
    }

    private static Random CreateRandomFromSeed(long seed64) {
        int seed32 = unchecked((int)(seed64 ^ (seed64 >> 32)));
        return new Random(seed32);
    }

    public int Next() => _rnd.Next();
    public int Next(int maxValue) => _rnd.Next(maxValue);
    public int Next(int minValue, int max) => _rnd.Next(minValue, max);
    public double NextDouble() => _rnd.NextDouble();
    public void Reseed(long seed) => _rnd = new Random(seed.GetHashCode());
}