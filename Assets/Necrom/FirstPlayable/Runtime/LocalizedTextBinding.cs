using System;
using Necrom.Core.Domain;
using UnityEngine;
using UnityEngine.UI;
namespace Necrom.FirstPlayable.Runtime {
 [DisallowMultipleComponent] [RequireComponent(typeof(Text))]
 public sealed class LocalizedTextBinding : MonoBehaviour {
  [SerializeField] string localizationKey;
  LocalizationManager subscribed;
  public string Key=>localizationKey;
  public static LocalizedTextBinding Attach(Text target,string key){
   if(target==null)throw new ArgumentNullException(nameof(target));
   if(string.IsNullOrWhiteSpace(key))throw new ArgumentException("Localization key required.");
   var binding=target.GetComponent<LocalizedTextBinding>()??target.gameObject.AddComponent<LocalizedTextBinding>();
   binding.Unsubscribe();binding.localizationKey=key;
   if(binding.isActiveAndEnabled)binding.Subscribe();
   binding.Refresh();return binding;
  }
  void Subscribe(){
   if(string.IsNullOrEmpty(localizationKey))return;
   Unsubscribe();subscribed=LocalizationRuntime.Manager;subscribed.LanguageChanged+=Refresh;Refresh();
  }
  void Unsubscribe(){if(subscribed!=null)subscribed.LanguageChanged-=Refresh;subscribed=null;}
  void Refresh(){if(!string.IsNullOrEmpty(localizationKey))GetComponent<Text>().text=LocalizationRuntime.Manager.Get(localizationKey);}
  void OnEnable()=>Subscribe();
  void OnDisable()=>Unsubscribe();
  void OnDestroy()=>Unsubscribe();
 }
}
