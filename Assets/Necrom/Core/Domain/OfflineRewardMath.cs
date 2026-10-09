using System;
namespace Necrom.Core.Domain
{
    public static class OfflineRewardMath
    {
        public const int MaxSeconds=28800;
        public static int Elapsed(long start,long end) =>
            start<=0 || end<=start ? 0 : (int)Math.Min(MaxSeconds,end-start);
        public static long Gold(int seconds)=>Math.Max(0,seconds/60)*100L;
        public static long Diamonds(int seconds)=>Math.Max(0,seconds/60)*10L;
    }
}
