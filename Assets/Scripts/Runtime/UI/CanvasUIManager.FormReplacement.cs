using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    private GameObject _formReplacementPanel;
    private Text _formReplacementTitle;
    private Transform _formSlotContainer;

    private GameObject _legacySelectionPanel;
    private Text _legacySelectionTitle;
    private Transform _legacyContainer;

    private string _pendingFormId;
    private List<LegacyAbility> _pendingLegacies;
    private string _pendingSourceFormId;
    private LegacyAbility _pendingNewLegacy;

    private GameObject _legacyReplacePanel;
    private Text _legacyReplaceTitle;
    private Transform _legacyReplaceContainer;

    GameObject MakePopupContainer(GameObject overlay, float widthPct, float heightPct)
    {
        var container = new GameObject("Container", typeof(RectTransform), typeof(Image), typeof(Outline));
        container.transform.SetParent(overlay.GetComponent<RectTransform>(), false);
        container.GetComponent<Image>().color = DarkItemBg;
        var ol = container.GetComponent<Outline>();
        ol.effectColor = new Color(0f, 1f, 0.816f, 0.5f);
        ol.effectDistance = new Vector2(2, 2);
        var cRT = container.GetComponent<RectTransform>();
        float hw = widthPct / 2f;
        float hh = heightPct / 2f;
        cRT.anchorMin = new Vector2(0.5f - hw, 0.5f - hh);
        cRT.anchorMax = new Vector2(0.5f + hw, 0.5f + hh);
        cRT.offsetMin = Vector2.zero;
        cRT.offsetMax = Vector2.zero;
        return container;
    }

    // ==================== 形态替换面板 ====================
    public void BuildFormReplacementPanel()
    {
        _formReplacementPanel = Panel("FormReplacement", OvlBgDense);
        _formReplacementPanel.SetActive(false);

        var container = MakePopupContainer(_formReplacementPanel, 0.42f, 0.30f);
        var vl = AddVL(container, 20, 12);

        Spacer(vl, 8);

        _formReplacementTitle = TxtGo(vl.transform, "形态槽已满！", 20, Cyan);
        _formReplacementTitle.alignment = TextAnchor.MiddleCenter;
        _formReplacementTitle.fontStyle = FontStyle.Bold;

        var hintTxt = TxtGo(vl.transform, "选择要替换的形态（本命无法替换）", 13, Dim);
        hintTxt.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 6);

        _formSlotContainer = new GameObject("FormSlots", typeof(RectTransform), typeof(HorizontalLayoutGroup)).transform;
        _formSlotContainer.SetParent(vl.transform, false);
        _formSlotContainer.gameObject.AddComponent<LayoutElement>().preferredHeight = 100;
        var hl = _formSlotContainer.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 12;
        hl.childAlignment = TextAnchor.MiddleCenter;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = false;

        Spacer(vl, 6);

        var cancelBtn = BtnGo(vl.transform, "取消", 14, Dim, 40);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => HideFormReplacementPanel());

        EventBus.Register<string>(EventTypes.FormSlotFull, OnFormSlotFull);
    }

    void OnFormSlotFull(string newFormId)
    {
        _pendingFormId = newFormId;
        ShowFormReplacementPanel();
    }

    void ShowFormReplacementPanel()
    {
        ClearFormSlotButtons();

        var player = GameManager.Instance.Player;
        for (int i = 0; i < player.ownedForms.Count; i++)
        {
            var formId = player.ownedForms[i];
            bool isPrimary = (i == FormManager.PRIMARY_FORM_SLOT);

            var btnGo = new GameObject($"FormSlot_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(_formSlotContainer, false);

            var btn = btnGo.GetComponent<Button>();
            btn.interactable = !isPrimary;

            var img = btnGo.GetComponent<Image>();
            img.color = isPrimary ? new Color(0.2f, 0.4f, 0.3f) : new Color(0.15f, 0.1f, 0.25f);

            var le = btnGo.AddComponent<LayoutElement>();
            le.preferredWidth = 90;
            le.preferredHeight = 90;
            le.minWidth = 90;
            le.minHeight = 90;

            // Icon container positioned in upper portion of button
            var iconContainer = new GameObject("IconContainer", typeof(RectTransform));
            iconContainer.transform.SetParent(btnGo.transform, false);
            var iconCRT = iconContainer.GetComponent<RectTransform>();
            iconCRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconCRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconCRT.pivot = new Vector2(0.5f, 0.5f);
            iconCRT.anchoredPosition = Vector2.up * 12;
            iconCRT.sizeDelta = new Vector2(48, 48);

            var tex = LoadTex($"Icons/Monsters/{formId}");
            if (tex != null)
            {
                // 使用BuildAspectIcon保持宽高比
                BuildAspectIcon(iconContainer.transform, tex, 0);
            }

            var nameTxt = TxtGo(btnGo.transform, GetMonsterDisplayName(formId), 11, isPrimary ? SuccessGreen : Bright);
            nameTxt.alignment = TextAnchor.MiddleCenter;
            nameTxt.rectTransform.anchoredPosition = Vector2.down * 18;

            if (isPrimary)
            {
                var lockGo = new GameObject("Lock", typeof(RectTransform), typeof(Text));
                lockGo.transform.SetParent(btnGo.transform, false);
                var lockTxt = lockGo.GetComponent<Text>();
                lockTxt.text = "本命";
                lockTxt.font = F();
                lockTxt.fontSize = 9;
                lockTxt.color = SuccessGreen;
                lockTxt.alignment = TextAnchor.MiddleCenter;
            }
            else
            {
                int slotIdx = i;
                btn.onClick.AddListener(() => OnFormSlotSelected(slotIdx));
            }
        }

        _formReplacementPanel.SetActive(true);
    }

    void ClearFormSlotButtons()
    {
        foreach (Transform child in _formSlotContainer)
            Destroy(child.gameObject);
    }

    void OnFormSlotSelected(int slotIndex)
    {
        HideFormReplacementPanel();
        bool success = FormManager.Instance.ReplaceForm(slotIndex, _pendingFormId);
        if (success)
        {
            Analytics.Track("FormReplaced", ("Slot", slotIndex), ("FormId", _pendingFormId));
        }
    }

    void HideFormReplacementPanel()
    {
        _formReplacementPanel.SetActive(false);
        _pendingFormId = null;
    }

    // ==================== 遗产选择面板 ====================
    public void BuildLegacySelectionPanel()
    {
        _legacySelectionPanel = Panel("LegacySelection", OvlBgDense);
        _legacySelectionPanel.SetActive(false);

        var container = MakePopupContainer(_legacySelectionPanel, 0.42f, 0.35f);
        var vl = AddVL(container, 18, 10);

        Spacer(vl, 6);

        _legacySelectionTitle = TxtGo(vl.transform, "获得遗产！", 20, Purp);
        _legacySelectionTitle.alignment = TextAnchor.MiddleCenter;
        _legacySelectionTitle.fontStyle = FontStyle.Bold;

        Spacer(vl, 4);

        _legacyContainer = new GameObject("LegacyList", typeof(RectTransform), typeof(VerticalLayoutGroup)).transform;
        _legacyContainer.SetParent(vl.transform, false);
        var lg = _legacyContainer.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 8;
        lg.childForceExpandWidth = true;
        lg.childForceExpandHeight = false;

        Spacer(vl, 6);

        var cancelBtn = BtnGo(vl.transform, "放弃", 14, Dim, 38);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => HideLegacySelectionPanel());

        EventBus.Register<List<LegacyAbility>, string>(EventTypes.LegacySelection, OnLegacySelection);
    }

    void OnLegacySelection(List<LegacyAbility> abilities, string sourceFormId)
    {
        _pendingLegacies = abilities;
        _pendingSourceFormId = sourceFormId;
        ShowLegacySelectionPanel();
    }

    void ShowLegacySelectionPanel()
    {
        ClearLegacyButtons();

        var sourceName = GetMonsterDisplayName(_pendingSourceFormId);
        _legacySelectionTitle.text = $"获得遗产！\n来自: {sourceName}";

        foreach (var ability in _pendingLegacies)
        {
            var btnGo = new GameObject($"Legacy_{ability.id}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(_legacyContainer, false);

            var btn = btnGo.GetComponent<Button>();
            var capturedAbility = ability;
            btn.onClick.AddListener(() => OnLegacySelected(capturedAbility));

            var img = btnGo.GetComponent<Image>();
            img.color = new Color(0.15f, 0.1f, 0.25f);
            btn.targetGraphic = img;

            btnGo.AddComponent<LayoutElement>().preferredHeight = 60;

            var hl = btnGo.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 10;
            hl.padding = new RectOffset(12, 12, 8, 8);
            hl.childAlignment = TextAnchor.MiddleLeft;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = true;

            Color rc = ability.rarity == 2 ? Purp : ability.rarity == 1 ? Gold : Bright;
            var iconTxt = TxtGo(btnGo.transform, ability.icon, 18, rc);
            iconTxt.alignment = TextAnchor.MiddleCenter;

            var infoCol = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup));
            infoCol.transform.SetParent(btnGo.transform, false);
            infoCol.AddComponent<LayoutElement>().flexibleWidth = 1;
            var iv = infoCol.GetComponent<VerticalLayoutGroup>();
            iv.spacing = 2;
            iv.childAlignment = TextAnchor.MiddleLeft;
            iv.childForceExpandHeight = false;
            iv.childForceExpandWidth = true;

            var nameTxt = TxtGo(infoCol.transform, $"{ability.name} [{ability.RarityLabel()}]", 14, rc);
            nameTxt.alignment = TextAnchor.MiddleLeft;

            if (IsBeginnerLegacy(ability))
            {
                var recBadge = TxtGo(infoCol.transform, "★ 推荐新手", 10, new Color(1f, 0.7f, 0.2f));
                recBadge.alignment = TextAnchor.MiddleLeft;
            }

            var descTxt = TxtGo(infoCol.transform, ability.description, 11, Dim);
            descTxt.alignment = TextAnchor.MiddleLeft;
        }

        _legacySelectionPanel.SetActive(true);
    }

    void ClearLegacyButtons()
    {
        foreach (Transform child in _legacyContainer)
            Destroy(child.gameObject);
    }

    void OnLegacySelected(LegacyAbility ability)
    {
        bool added = LegacyManager.Instance.AddLegacy(ability);

        if (!added)
        {
            _pendingNewLegacy = ability;
            ShowLegacyReplacePanel();
            return;
        }

        HideLegacySelectionPanel();
        CompleteGameSystem.Instance?.AddCombatLog($"<color=#bb88ff>★ 获得遗产: {ability.icon} {ability.name}</color>");
    }

    // ==================== 遗产替换面板 ====================
    void ShowLegacyReplacePanel()
    {
        if (_legacyReplacePanel == null) BuildLegacyReplacePanel();

        foreach (Transform child in _legacyReplaceContainer)
            Destroy(child.gameObject);

        _legacyReplaceTitle.text = $"遗产已满！选择替换\n新: {_pendingNewLegacy.icon} {_pendingNewLegacy.name}\n{_pendingNewLegacy.description}";

        var equipped = LegacyManager.Instance.GetEquippedLegacies();
        for (int i = 0; i < equipped.Count; i++)
        {
            var leg = equipped[i];
            int idx = i;

            var btnGo = new GameObject($"LegReplace_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(_legacyReplaceContainer, false);

            var btn = btnGo.GetComponent<Button>();
            btn.onClick.AddListener(() => OnLegacyReplaceSlotSelected(idx));

            var img = btnGo.GetComponent<Image>();
            img.color = new Color(0.12f, 0.08f, 0.2f);
            btn.targetGraphic = img;

            btnGo.AddComponent<LayoutElement>().preferredHeight = 55;

            var hl = btnGo.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 8;
            hl.padding = new RectOffset(12, 12, 6, 6);
            hl.childAlignment = TextAnchor.MiddleLeft;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = true;

            Color rc = leg.rarity == 2 ? Purp : leg.rarity == 1 ? Gold : Bright;
            var iconTxt = TxtGo(btnGo.transform, leg.icon, 16, rc);
            iconTxt.alignment = TextAnchor.MiddleCenter;

            var infoCol = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup));
            infoCol.transform.SetParent(btnGo.transform, false);
            infoCol.AddComponent<LayoutElement>().flexibleWidth = 1;
            var iv = infoCol.GetComponent<VerticalLayoutGroup>();
            iv.spacing = 2;
            iv.childAlignment = TextAnchor.MiddleLeft;
            iv.childForceExpandHeight = false;
            iv.childForceExpandWidth = true;

            TxtGo(infoCol.transform, $"{leg.name} [{leg.RarityLabel()}]", 13, rc).alignment = TextAnchor.MiddleLeft;
            TxtGo(infoCol.transform, leg.description, 10, Dim).alignment = TextAnchor.MiddleLeft;
        }

        _legacyReplacePanel.SetActive(true);
    }

    void BuildLegacyReplacePanel()
    {
        _legacyReplacePanel = Panel("LegacyReplace", OvlBgDense);
        _legacyReplacePanel.SetActive(false);

        var container = MakePopupContainer(_legacyReplacePanel, 0.42f, 0.32f);
        var vl = AddVL(container, 18, 10);

        Spacer(vl, 6);

        _legacyReplaceTitle = TxtGo(vl.transform, "", 16, Purp);
        _legacyReplaceTitle.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 4);

        _legacyReplaceContainer = new GameObject("ReplaceList", typeof(RectTransform), typeof(VerticalLayoutGroup)).transform;
        _legacyReplaceContainer.SetParent(vl.transform, false);
        var lg = _legacyReplaceContainer.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 6;
        lg.childForceExpandWidth = true;
        lg.childForceExpandHeight = false;

        Spacer(vl, 6);

        var cancelBtn = BtnGo(vl.transform, "取消", 14, Dim, 38);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            _legacyReplacePanel.SetActive(false);
            _pendingNewLegacy = null;
        });
    }

    void OnLegacyReplaceSlotSelected(int index)
    {
        _legacyReplacePanel.SetActive(false);
        HideLegacySelectionPanel();

        LegacyManager.Instance.ReplaceLegacy(index, _pendingNewLegacy);
        CompleteGameSystem.Instance?.AddCombatLog($"<color=#bb88ff>★ 遗产替换: {_pendingNewLegacy.icon} {_pendingNewLegacy.name}</color>");
        _pendingNewLegacy = null;
    }

    void HideLegacySelectionPanel()
    {
        _legacySelectionPanel.SetActive(false);
        _pendingLegacies = null;
        _pendingSourceFormId = null;
    }

    string GetMonsterDisplayName(string monsterId)
    {
        if (GameDataImporter.MonsterDefinitions == null) return monsterId;
        var monster = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == monsterId);
        return !string.IsNullOrEmpty(monster.name) ? monster.name : monsterId;
    }

    bool IsBeginnerLegacy(LegacyAbility ability)
    {
        var beginnerTypes = new HashSet<string> { "AttackBonus", "lifesteal", "regen", "DefenseBonus" };
        return beginnerTypes.Contains(ability.effectType);
    }
}
