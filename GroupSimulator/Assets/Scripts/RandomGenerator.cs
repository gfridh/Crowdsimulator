using System;

public class RandomGenerator {

    private Random randomGenerator;

    public RandomGenerator(int seed) {
        randomGenerator = new Random(seed);
    }

    public int Range(int minValue, int maxValue) {
        return randomGenerator.Next(minValue, maxValue);
    }

    public float Range(float minValue, float maxValue) {
        return (float)randomGenerator.NextDouble() * (maxValue - minValue) + minValue;
    }
}
