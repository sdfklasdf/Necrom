using System;
namespace Necrom.Core.Domain {
 public sealed class LocalizedContentNames {
  private readonly LocalizationManager localization;
  public LocalizedContentNames(LocalizationManager localization){this.localization=localization??throw new ArgumentNullException(nameof(localization));}
  public string ResolveTraitName(TftTraitDefinition trait){
   if(trait==null)throw new ArgumentNullException(nameof(trait));
   var key=string.IsNullOrEmpty(trait.localizationKey)?"trait."+trait.category+"."+trait.id+".name":trait.localizationKey;
   return localization.Get(key);
  }
  public string ResolveMonsterName(MonsterEntry monster){
   if(monster==null)throw new ArgumentNullException(nameof(monster));
   var key="monster."+monster.id+".name";
   return localization.HasKey(key)?localization.Get(key):monster.name;
  }
 }
}
