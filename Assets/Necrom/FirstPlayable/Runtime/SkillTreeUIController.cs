using System;
using System.Collections.Generic;
using System.Linq;
using Necrom.Core.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // AWU-13: UI-only prototype; skill effects and production persistence are separate work.
    [DisallowMultipleComponent]
    public sealed class SkillTreeUIController : MonoBehaviour
    {
        [SerializeField] private int debugSpOnFirstOpen = 100;
        private const string SaveKey = "NECROM_SKILL_TREE_V1";
        [Serializable] private sealed class SkillSave { public int version = 1; public int availableSp; public SkillLevel[] levels; }
        [Serializable] private sealed class SkillLevel { public string id; public int level; }
        private SkillTreeManager manager;
        private PermanentDeckCombatSpawner deckSpawner;
        private SkillTreeCatalog catalog;
        private Canvas canvas;
        private GameObject popup;
        private Text spLabel;
        private bool granted;
        private readonly List<NodeView> views = new List<NodeView>();

        private sealed class NodeView
        {
            public SkillNode Skill;
            public Button Button;
            public Image Background;
            public Text Label;
        }

        private void Start()
        {
            try
            {
                var asset = Resources.Load<TextAsset>("skill_tree_v1");
                if (asset == null) throw new InvalidOperationException("Resources/skill_tree_v1.json missing.");
                catalog = JsonUtility.FromJson<SkillTreeCatalog>(asset.text);
                manager = new SkillTreeManager(catalog);
                LoadState();
                if (!enabled) return;
                deckSpawner = GetComponent<PermanentDeckCombatSpawner>();
                if(deckSpawner==null)throw new InvalidOperationException("Permanent deck spawner missing");
                deckSpawner.BindSkillManager(manager);
                Build();
            }
            catch (Exception ex)
            {
                Debug.LogError("[AWU-13] Skill tree failed to initialize: " + ex);
                enabled = false;
            }
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 min, Vector2 max,
            Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }

        private static Text TextAt(Transform parent, string name, string value, int size,
            Vector2 anchor, Vector2 position, Vector2 dimensions)
        {
            var rect = Rect(parent, name, anchor, anchor, position, dimensions);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = size; text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Wrap;
            return text;
        }

        private static Button MakeButton(Transform parent, string name, string caption,
            Vector2 anchor, Vector2 position, Vector2 dimensions, Action action)
        {
            var rect = Rect(parent, name, anchor, anchor, position, dimensions);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(.18f, .35f, .55f, 1f);
            var button = rect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => action());
            TextAt(rect, "Caption", caption, 15, new Vector2(.5f,.5f), Vector2.zero,
                dimensions - new Vector2(4,4));
            return button;
        }

        private void Build()
        {
            var root = new GameObject("SkillTreeCanvas", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true; canvas.sortingOrder = 30020;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844);
            scaler.matchWidthOrHeight = .5f;
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero; rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero; rootRect.offsetMax = Vector2.zero;
            MakeButton(root.transform, "OpenSkillTree", "스킬 트리",
                new Vector2(1,1), new Vector2(-95,-173), new Vector2(165,44), Open);

            var panel = Rect(root.transform, "SkillTreeFullscreen", Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            popup = panel.gameObject;
            var backdrop = popup.AddComponent<Image>();
            backdrop.color = new Color(.025f,.04f,.09f,.985f);
            TextAt(panel, "Title", "네크로맨서 스킬 트리", 23,
                new Vector2(.5f,1), new Vector2(0,-39), new Vector2(360,45));
            spLabel = TextAt(panel, "SkillPoints", "", 18,
                new Vector2(.5f,1), new Vector2(0,-87), new Vector2(340,35));
            MakeButton(panel, "CloseSkillTree", "닫기", new Vector2(.5f,0),
                new Vector2(0,44), new Vector2(180,45), Close);

            var viewport = Rect(panel,"SkillTreeViewport",new Vector2(.04f,.15f),
                new Vector2(.96f,.84f),Vector2.zero,Vector2.zero);
            viewport.gameObject.AddComponent<Image>().color = new Color(.08f,.11f,.20f,.8f);
            var mask = viewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = true;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            var content = Rect(viewport,"SkillTreeContent",new Vector2(0,1),
                new Vector2(1,1),Vector2.zero,new Vector2(0,760));
            content.pivot = new Vector2(.5f,1);
            scroll.viewport = viewport; scroll.content = content;
            scroll.vertical = true; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var branches = new [] { "Destruction","Summoning","CurseCC" };
            var headings = new [] { "파괴 마법","소환수 강화","저주 / CC" };
            for (int c = 0; c < branches.Length; c++)
            {
                float x = (c - 1) * 116f;
                TextAt(content,"Branch"+c,headings[c],16,new Vector2(.5f,1),
                    new Vector2(x,-30),new Vector2(112,36));
                for (int tier=1;tier<=7;tier++)
                {
                    SkillNode skill = null;
                    foreach (var candidate in catalog.skills)
                        if (candidate.branch == branches[c] && candidate.tier == tier)
                        {
                            if (skill != null) throw new InvalidOperationException("Duplicate branch tier");
                            skill = candidate;
                        }
                    if (skill == null) throw new InvalidOperationException("Missing branch tier " + branches[c] + tier);
                    var picked = skill;
                    var btn = MakeButton(content,skill.id,"",new Vector2(.5f,1),
                        new Vector2(x,-84-(tier-1)*90),new Vector2(108,78),
                        () => LevelUp(picked.id));
                    views.Add(new NodeView {Skill=picked,Button=btn,
                        Background=btn.GetComponent<Image>(),
                        Label=btn.GetComponentInChildren<Text>()});
                }
            }
            popup.SetActive(false);
        }

        public void Open()
        {
            if (!enabled || popup == null || manager == null) return;
            // Once per component lifetime: closing/reopening cannot repeatedly mint SP.
            if (!granted)
            {
                manager.GrantSp(Mathf.Max(0,debugSpOnFirstOpen));
                granted = true;
                SaveState();
                Debug.Log("[AWU-14] Granted initial debug SP once: " + debugSpOnFirstOpen);
            }
            popup.SetActive(true);
            Refresh();
        }

        public void Close() { if (popup != null) popup.SetActive(false); }

        private void LevelUp(string id)
        {
            if (manager == null || !manager.TryLevelUp(id)) return;
            SaveState();
            if(deckSpawner!=null)deckSpawner.RefreshSkillBonuses();
            Refresh();
        }

        private void Refresh()
        {
            spLabel.text = "남은 SP: " + manager.AvailableSp + "  (테스트 전용)";
            foreach (var view in views)
            {
                int level = manager.LevelOf(view.Skill.id);
                bool maxed = level >= view.Skill.maxLevel;
                bool allowed = manager.CanLevelUp(view.Skill.id);
                bool ultimate = view.Skill.tier == 7;
                string title = ultimate
                    ? (view.Skill.branch == "Destruction" ? "그림자 군단" :
                       view.Skill.branch == "Summoning" ? "망자의 융합" : "영혼 폭주")
                    : "티어 " + view.Skill.tier;
                view.Label.text = (ultimate ? "★ " : "") + title + "\nLv " + level +
                    "/" + view.Skill.maxLevel + "\n" +
                    (maxed ? "마스터" : allowed ? "레벨업 ("+view.Skill.spPerLevel+"SP)" : "잠김 / SP 부족");
                view.Button.interactable = allowed;
                // Gray indicates disabled; gold-purple marks accessible ultimates.
                view.Background.color = !allowed
                    ? new Color(.30f,.30f,.33f,1f)
                    : ultimate ? new Color(.58f,.30f,.70f,1f)
                    : new Color(.16f,.40f,.64f,1f);
            }
        }

        private void LoadState()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;
            try
            {
                var save = JsonUtility.FromJson<SkillSave>(PlayerPrefs.GetString(SaveKey));
                if (save == null || save.version != 1 || save.levels == null)
                    throw new InvalidOperationException("Unsupported skill save");
                var levels = new Dictionary<string,int>(StringComparer.Ordinal);
                foreach (var entry in save.levels)
                    if (entry == null || string.IsNullOrEmpty(entry.id) || !levels.TryAdd(entry.id,entry.level))
                        throw new InvalidOperationException("Duplicate or missing saved skill id");
                manager.RestoreState(save.availableSp,levels);
                granted = true;
                Debug.Log("[AWU-14] Skill tree save restored. SP=" + manager.AvailableSp);
            }
            catch (Exception ex)
            {
                // Preserve the original bytes for recovery. Never silently overwrite a corrupt save.
                Debug.LogError("[AWU-14] Skill tree save invalid; original preserved: " + ex);
                enabled = false;
            }
        }

        private void SaveState()
        {
            if (manager == null || !granted) return;
            var save = new SkillSave { version = 1, availableSp = manager.AvailableSp,
                levels = manager.SnapshotLevels().Select(p => new SkillLevel { id=p.Key, level=p.Value }).ToArray() };
            PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool paused) { if (paused) SaveState(); }
        private void OnApplicationQuit() { SaveState(); }
        private void OnDestroy() { if (canvas != null) Destroy(canvas.gameObject); }
    }
}
