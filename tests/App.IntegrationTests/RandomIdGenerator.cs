using System;

namespace App.IntegrationTests
{
    public static class RandomIdGenerator
    {
        static Random rng;
        static RandomIdGenerator()
        {
            rng = new Random();
        }

        public static int GetId() => rng.Next(1,int.MaxValue-1);
    }
}