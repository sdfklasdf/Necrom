using System;
namespace Necrom.Core.Domain
{
    // Provisional progression coefficients for AWU-19; revisit during balance approval.
    public static class WaveDifficulty
    {
        public static bool IsBoss(int wave) => wave > 0 && wave % 10 == 0;
        public static int EnemyCount(int wave,int normalCount)
        {
            if(wave<1 || normalCount<1)throw new ArgumentOutOfRangeException();
            return IsBoss(wave)?1:normalCount;
        }
        public static int Scale(int baseValue,int wave,bool bossHp=false)
        {
            if(baseValue<1 || wave<1)throw new ArgumentOutOfRangeException();
            double factor=Math.Pow(1.1,Math.Min(wave-1,500));
            if(bossHp && IsBoss(wave))factor*=5;
            double value=baseValue*factor;
            return value>=int.MaxValue?int.MaxValue:Math.Max(1,(int)Math.Round(value,MidpointRounding.AwayFromZero));
        }
    }
}
