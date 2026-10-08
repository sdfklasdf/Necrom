using System;
using System.Collections.Generic;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    // Attach via code to future permanent-deck runtime ownership, never to Raise entities.
    public sealed class PermanentDeckStatBridge : MonoBehaviour
    {
        private readonly Dictionary<string,EffectiveCombatStats> activeStats = new Dictionary<string,EffectiveCombatStats>();
        private StatCalculator calculator;
        private PermanentMonsterRoster roster;
        public void Initialize(StatCalculator statCalculator,PermanentMonsterRoster permanentRoster) {
            if(calculator!=null)throw new InvalidOperationException("Bridge already initialized");
            calculator=statCalculator??throw new ArgumentNullException(nameof(statCalculator));
            roster=permanentRoster??throw new ArgumentNullException(nameof(permanentRoster));
        }
        public void ReplaceCalculator(StatCalculator replacement)
        {
            if(calculator==null)throw new InvalidOperationException("Bridge not initialized");
            calculator=replacement??throw new ArgumentNullException(nameof(replacement));
        }
        public EffectiveCombatStats RegisterPermanentEntity(string monsterId) {
            if(calculator==null)throw new InvalidOperationException("Bridge not initialized");
            if(!roster.Owns(monsterId) || activeStats.ContainsKey(monsterId))
                throw new InvalidOperationException("Unowned or duplicate permanent entity");
            var deck=new List<string>();
            for(int slot=0;slot<roster.UnlockedSlots;slot++) {
                var id=roster.GetDeckSlot(slot);if(id!=null)deck.Add(id);
            }
            var result=calculator.Calculate(monsterId,deck);
            activeStats.Add(monsterId,result);
            return result;
        }
        public EffectiveCombatStats RecalculatePermanentEntity(string monsterId)
        {
            if(calculator==null || roster==null || !activeStats.ContainsKey(monsterId))
                throw new InvalidOperationException("Permanent entity not registered");
            var deck=new List<string>();
            for(int slot=0;slot<roster.UnlockedSlots;slot++) {
                var id=roster.GetDeckSlot(slot);if(id!=null)deck.Add(id);
            }
            var next=calculator.Calculate(monsterId,deck);
            activeStats[monsterId]=next;
            return next;
        }
        public void UnregisterPermanentEntity(string monsterId)=>activeStats.Remove(monsterId);
        public void ResetCombatRegistration()=>activeStats.Clear();
        // Explicit separate combat adapter will consume these stats when permanent units are actually spawned.
    }
}
