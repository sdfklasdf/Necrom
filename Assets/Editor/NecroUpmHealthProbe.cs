using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

// Explicit read-only health check. No install/remove/resolve or asset purchase.
[InitializeOnLoad]
public static class NecroUpmHealthProbe {
 static ListRequest list;
 static SearchRequest search;
 static string Root => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
 static string Output => Path.Combine(Root, "docs/evidence/UPM-Recovery-20261011/health.txt");
 static NecroUpmHealthProbe() { EditorApplication.update += Tick; }
 static void Tick() {
  if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
  try {
   if (list != null) {
    if (!list.IsCompleted) return;
    if (list.Status != StatusCode.Success) throw new Exception("List: " + list.Error.message);
    File.AppendAllText(Output, "ONLINE_LIST PASS count=" + list.Result.Count() + "\n"
     + string.Join("\n", list.Result.Select(p => p.name + "@" + p.version)) + "\n");
    list = null; search = Client.Search("com.unity.ugui", false); return;
   }
   if (search != null) {
    if (!search.IsCompleted) return;
    if (search.Status != StatusCode.Success) throw new Exception("Search: " + search.Error.message);
    if (!search.Result.Any(p => p.name == "com.unity.ugui")) throw new Exception("Registry package absent");
    File.AppendAllText(Output, "LIVE_REGISTRY_SEARCH PASS com.unity.ugui\n");
    search = null;
    bool opened = EditorApplication.ExecuteMenuItem("Window/Package Management/Package Manager");
    File.AppendAllText(Output, "Package Manager menu opened=" + opened + "\n");
    return;
   }
   var job = Path.Combine(Root, "Library/upm-health-job.txt");
   if (!File.Exists(job)) return;
   try { File.Delete(job); } catch (IOException) { return; } // writer still owns trigger; retry next tick
   Directory.CreateDirectory(Path.GetDirectoryName(Output));
   File.WriteAllText(Output, DateTime.Now.ToString("o") + " Unity=" + Application.unityVersion + "\n");
   list = Client.List(false, true);
  } catch (Exception e) {
   list = null; search = null;
   File.AppendAllText(Output, "FAIL " + e + "\n"); Debug.LogException(e);
  }
 }
}
