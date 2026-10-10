using Necrom.Core.Domain;
using UnityEngine;
namespace Necrom.FirstPlayable.Runtime {
 public static class LocalizationRuntime {
  static LocalizationManager manager;
  public static LocalizationManager Manager {
   get {
    if(manager==null){
     var source=Resources.Load<TextAsset>("Localization/necro-localization");
     if(source==null)throw new System.InvalidOperationException("Localization JSON missing.");
     manager=new LocalizationManager(JsonUtility.FromJson<LocalizationTableData>(source.text));
    }
    return manager;
   }
  }
  public static bool SetLanguage(string language)=>Manager.TrySetLanguage(language);
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void Reset(){manager=null;}
 }
}
