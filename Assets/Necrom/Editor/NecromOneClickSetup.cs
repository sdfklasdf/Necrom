using System;
using Necrom.FirstPlayable.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Necrom.EditorTools
{
    public static class NecromOneClickSetup
    {
        [MenuItem("네크로맨서 게임 세팅")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play mode before setting up UI.");
                return;
            }
            var font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                EditorUtility.DisplayDialog("TMP 폰트 필요", "TextMesh Pro Essential Resources를 먼저 설치하세요.", "확인");
                return;
            }
            var root = GameObject.Find("NecromDemoUI");
            if (root == null) root = new GameObject("NecromDemoUI");
            var canvas = root.GetComponent<Canvas>() ?? root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            if (root.GetComponent<CanvasScaler>() == null) root.AddComponent<CanvasScaler>();
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            if (root.GetComponent<GraphicRaycaster>() == null) root.AddComponent<GraphicRaycaster>();

            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var popup = Panel(root.transform, "OfflinePopup", new Vector2(0, 40), new Vector2(350, 220),
                new Color(.13f,.17f,.28f,.97f));
            var amount = Label(popup.transform, "OfflineAmountText", "오프라인 보상", new Vector2(0,45), new Vector2(320,60),font,27);
            var claim = MakeButton(popup.transform, "ClaimButton", "보상 받기", new Vector2(0,-57),font);
            var coins = Label(root.transform, "CoinsText", "Coins: 0", new Vector2(0,270),new Vector2(340,55),font,27);
            var result = Label(root.transform, "ResultText", "몬스터 소환 준비!", new Vector2(0,-175),new Vector2(340,90),font,22);
            var draw = MakeButton(root.transform, "DrawButton", "몬스터 뽑기 (100 코인)", new Vector2(0,-290),font);
            var system = GameObject.Find("NecromDemoGameSystems");
            if(system == null) system = new GameObject("NecromDemoGameSystems");
            var manager = system.GetComponent<UIManager>() ?? system.AddComponent<UIManager>();
            var so = new SerializedObject(manager);
            so.FindProperty("offlinePopup").objectReferenceValue = popup;
            so.FindProperty("offlineAmountText").objectReferenceValue = amount;
            so.FindProperty("coinsText").objectReferenceValue = coins;
            so.FindProperty("resultText").objectReferenceValue = result;
            so.FindProperty("claimButton").objectReferenceValue = claim;
            so.FindProperty("drawButton").objectReferenceValue = draw;
            so.ApplyModifiedPropertiesWithoutUndo();
            popup.SetActive(false);
            EditorSceneManager.MarkSceneDirty(root.scene);
            EditorSceneManager.SaveScene(root.scene);
            Selection.activeGameObject = system;
            Debug.Log("NECROM UI SETUP COMPLETE: Canvas, popup, buttons, UIManager references saved.");
            EditorUtility.DisplayDialog("네크로맨서 UI 세팅 완료", "씬에 UI와 버튼 연결을 저장했습니다. Play를 누르세요.", "확인");
        }

        static GameObject Panel(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {
            var go = Child(parent,name);
            var rect = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f);
            rect.sizeDelta=size;rect.anchoredPosition=position;
            var image=go.GetComponent<Image>() ?? go.AddComponent<Image>();
            image.color=color;
            return go;
        }
        static GameObject Child(Transform parent,string name)
        {
            var t=parent.Find(name);
            if(t!=null) return t.gameObject;
            var go=new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);
            return go;
        }
        static TMP_Text Label(Transform parent,string name,string caption,Vector2 position,Vector2 size,TMP_FontAsset font,int fontSize)
        {
            var go=Child(parent,name);
            var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.anchoredPosition=position;rect.sizeDelta=size;
            var label=go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
            label.font=font;label.fontSize=fontSize;label.text=caption;
            label.alignment=TextAlignmentOptions.Center;label.color=Color.white;
            label.raycastTarget=false;
            return label;
        }
        static Button MakeButton(Transform parent,string name,string caption,Vector2 position,TMP_FontAsset font)
        {
            var go=Panel(parent,name,position,new Vector2(300,58),new Color(.19f,.63f,.39f,1));
            var button=go.GetComponent<Button>() ?? go.AddComponent<Button>();
            button.targetGraphic=go.GetComponent<Image>();
            Label(go.transform,"Label",caption,Vector2.zero,new Vector2(290,54),font,23);
            return button;
        }
    }
}