using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Necrom.ContentDraft {
 [Serializable] public sealed class CharacterVisualEntry {
 public string characterId,assetKey;
 public GameObject prefab;
 public bool isDummy=true;
 }
 [CreateAssetMenu(menuName="NECRO Draft/Character Visual Catalog")]
 public sealed class CharacterVisualCatalogAsset : ScriptableObject {
 public CharacterCatalogDraftAsset catalog;
 public CharacterVisualEntry[] entries=Array.Empty<CharacterVisualEntry>();
 public string[] Validate() {
 var errors=new List<string>();
 if(catalog==null)return new[]{"Character catalog missing."};
 var data=catalog.ReadDraft();errors.AddRange(data.Validate());
 var profiles=data.profiles ?? Array.Empty<Necrom.Core.Domain.CharacterContentProfile>();
 var ids=new HashSet<string>(StringComparer.Ordinal);
 var keys=new HashSet<string>(StringComparer.Ordinal);
 if(entries==null)return new[]{"Visual entries missing."};
 foreach(var entry in entries) {
 if(entry==null){errors.Add("Null visual entry.");continue;}
 var profile=profiles.FirstOrDefault(p=>p!=null&&p.characterId==entry.characterId);
 if(string.IsNullOrWhiteSpace(entry.characterId)||!ids.Add(entry.characterId)||profile==null)errors.Add("Unknown/duplicate visual ID.");
 if(string.IsNullOrWhiteSpace(entry.assetKey)||!keys.Add(entry.assetKey)||profile==null||profile.assetStatus!="MAPPED"||profile.assetKey!=entry.assetKey)errors.Add("Visual key mismatch.");
 if(entry.prefab==null){errors.Add("Prefab missing: "+entry.characterId);continue;}
 var binding=entry.prefab.GetComponent<CharacterVisualBinding>();
 var sprite=entry.prefab.GetComponent<SpriteRenderer>();
 if(binding==null||binding.characterId!=entry.characterId||binding.isDummy!=entry.isDummy)errors.Add("Prefab identity mismatch: "+entry.characterId);
 if(sprite==null||sprite.sprite==null)errors.Add("Sprite missing: "+entry.characterId);
 }
 foreach(var p in profiles.Where(p=>p!=null&&p.assetStatus=="MAPPED"))if(!ids.Contains(p.characterId))errors.Add("Mapped profile lacks visual: "+p.characterId);
 return errors.ToArray();
 }
 public GameObject ResolvePrefab(string characterId) {
 var matches=(entries??Array.Empty<CharacterVisualEntry>()).Where(e=>e!=null&&e.characterId==characterId).ToArray();
 if(matches.Length!=1||Validate().Length!=0)throw new InvalidOperationException("Invalid visual catalog or unknown ID: "+characterId);
 return matches[0].prefab;
 }
 }
}
