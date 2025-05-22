namespace X3UR.Domain.Interfaces;
public interface IRandomProvider {
    int Next();
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    double NextDouble();
    void Reseed(int seed);
}