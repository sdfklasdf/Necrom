using System;

namespace Necrom.Core.Domain
{
    // Account EXP state has no Unity dependency, allowing deterministic domain tests.
    public sealed class AccountProgression
    {
        public int Level { get; private set; }
        public long Experience { get; private set; }
        public const int BaseEnemyExperience = 10;
        public AccountProgression(int level = 1, long experience = 0)
        {
            if (level < 1 || experience < 0 || experience >= RequiredExperience(level))
                throw new ArgumentOutOfRangeException(nameof(experience), "Invalid account progression snapshot.");
            Level = level;
            Experience = experience;
        }
        public static long RequiredExperience(int level)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            var value = 100.0 * Math.Pow(1.15, Math.Min(level - 1, 400));
            return value >= long.MaxValue ? long.MaxValue : Math.Max(100, (long)Math.Ceiling(value));
        }
        public static long ExperienceForKill(int wave)
        {
            if (wave < 1) throw new ArgumentOutOfRangeException(nameof(wave));
            return WaveDifficulty.Scale(BaseEnemyExperience, wave);
        }
        // Returns the number of SP to credit; excess EXP carries into the next level.
        public int GrantExperience(long amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return 0;
            Experience = amount > long.MaxValue - Experience ? long.MaxValue : Experience + amount;
            int gained = 0;
            while (Level < int.MaxValue && Experience >= RequiredExperience(Level))
            {
                Experience -= RequiredExperience(Level);
                Level++;
                gained++;
            }
            if (Level == int.MaxValue && Experience >= RequiredExperience(Level))
                Experience = RequiredExperience(Level) - 1;
            return gained;
        }
        public double Percent => Math.Min(1.0, (double)Experience / RequiredExperience(Level));
    }
}
