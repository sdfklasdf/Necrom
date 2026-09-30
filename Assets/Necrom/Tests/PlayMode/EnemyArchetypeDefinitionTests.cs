using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class EnemyArchetypeDefinitionTests
    {
        [UnityTest]
        public IEnumerator DefinitionCarriesIdentityRoleAndCombatHealth()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition"))
                .FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, "Enemy archetype definition type must exist.");

            var value = Activator.CreateInstance(type, "enemy.skeleton.guard", "frontline.guard", 120);
            Assert.That(type.GetProperty("ArchetypeId")?.GetValue(value), Is.EqualTo("enemy.skeleton.guard"));
            Assert.That(type.GetProperty("RoleId")?.GetValue(value), Is.EqualTo("frontline.guard"));
            Assert.That(type.GetProperty("MaxHealth")?.GetValue(value), Is.EqualTo(120));

            yield return null;
        }
    }
}
