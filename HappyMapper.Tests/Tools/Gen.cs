using System;

namespace HappyMapper.Tests.Tools
{
    public static class Gen
    {
        public static int Int(int max = 100)
        {
            Random = new Random();
            return Random.Next(max);
        }

        private static Random Random { get; set; }
    }
}