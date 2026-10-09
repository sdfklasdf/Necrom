using Necrom.Core.Domain;
using NUnit.Framework;
namespace Necrom.Core.Tests {
 public class WaveDifficultyAwu19Tests {
  [Test] public void LinearWaveIndexCompoundsByTenPercent() {
   Assert.That(WaveDifficulty.Scale(100,1),Is.EqualTo(100));
   Assert.That(WaveDifficulty.Scale(100,2),Is.EqualTo(110));
   Assert.That(WaveDifficulty.Scale(100,3),Is.EqualTo(121));
  }
  [Test] public void BossAtMultiplesOfTenIsSingleAndFiveTimesHp() {
   foreach(var wave in new[]{10,20,30}) {
    Assert.That(WaveDifficulty.IsBoss(wave),Is.True);
    Assert.That(WaveDifficulty.EnemyCount(wave,2),Is.EqualTo(1));
    Assert.That(WaveDifficulty.Scale(100,wave,true),Is.EqualTo(
      WaveDifficulty.Scale(100,wave)*5).Within(3));
   }
   Assert.That(WaveDifficulty.EnemyCount(11,2),Is.EqualTo(2));
  }
  [Test] public void ExtremeWavesClampWithoutOverflow() {
   Assert.That(WaveDifficulty.Scale(10,10000),Is.EqualTo(int.MaxValue));
   Assert.Throws<System.ArgumentOutOfRangeException>(()=>WaveDifficulty.Scale(10,0));
  }
 }
}
