using UnityEditor;
using UnityEngine;

namespace Necrom.EditorTools
{
    // Prevent the broken Visual Studio integration from launching when scripts are opened.
    [InitializeOnLoad]
    internal static class NecromExternalEditorFallback
    {
        static NecromExternalEditorFallback()
        {
#if UNITY_EDITOR_WIN
            const string editorPath = @"C:\Windows\System32\notepad.exe";
            if (System.IO.File.Exists(editorPath) &&
                EditorPrefs.GetString("kScriptsDefaultApp", "") != editorPath)
            {
                EditorPrefs.SetString("kScriptsDefaultApp", editorPath);
                Debug.Log("NECROM: external script editor set to Windows Notepad (Visual Studio disabled).");
            }
#endif
        }
    }
}
