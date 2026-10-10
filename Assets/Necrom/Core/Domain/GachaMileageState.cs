using System;
namespace Necrom.Core.Domain
{
    // Legacy remains the existing UI policy until separate persistence/UI migration is approved.
    public enum GachaGuaranteeMode { LegacyRandomPity=0, SelectionMileage=1 }
    [Serializable]
    public sealed class GachaMileageState
    {
        public int schemaVersion=1;
        public long successfulDraws;
        public long redeemedTickets;
        // Snapshot only. Not trusted currency, persistence or server authority.
    }
    public sealed class MileageSelection
    {
        public string Category { get; }
        public string TraitId { get; }
        public MileageSelection(string category,string traitId)
        {
            if ((category!="Origin" && category!="Class") || !TftTaxonomy.IsCanonical(category,traitId))
                throw new ArgumentException("Choose a canonical Origin or Class (Joker/aliases not eligible).");
            Category=category; TraitId=traitId;
        }
        public MileageSelection(TftOrigin origin) : this("Origin",origin.ToString()) {}
        public MileageSelection(TftClass job) : this("Class",job.ToString()) {}
    }
}
