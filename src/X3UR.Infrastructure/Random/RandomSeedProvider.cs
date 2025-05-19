using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X3UR.Domain.Interfaces;

namespace X3UR.Infrastructure.Random;
public class RandomSeedProvider : ISeedProvider {
    private readonly System.Random _rng;

    public RandomSeedProvider() {
        _rng = new System.Random();
    }

    public int GetSeed() => _rng.Next();
}