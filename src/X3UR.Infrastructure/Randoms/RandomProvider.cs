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

    private Random CreateRandomFromSeed(long seed64) {
        int seed32 = unchecked((int)(seed64 ^ (seed64 >> 32)));
        List<int> ints = new List<int>();
        return new Random(seed32);
    }

    public int Next() => _rnd.Next();
    public int Next(int maxValue) => _rnd.Next(maxValue);
    public int Next(int minValue, int max) => _rnd.Next(minValue, max);
    public double NextDouble() => _rnd.NextDouble();
    public void Reseed(long seed) => _rnd = new Random(seed.GetHashCode());
    public IEnumerable<T> Shuffle<T>(IEnumerable<T> source) {
        var list = new List<T>(source);
        int n = list.Count;
        for (int i = 0; i < n; i++) {
            int j = _rnd.Next(i, n);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}