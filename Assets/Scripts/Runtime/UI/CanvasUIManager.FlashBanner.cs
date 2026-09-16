using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public partial class CanvasUIManager
{
    GameObject _flashBanner;
    Text _flashText;
    CanvasGroup _flashCG;
    Queue<(string text, Color color, float duration)> _flashQueue = new Queue<(string, Color, float)>();
    bool _flashPlaying;

    void BuildFlashBanner()
    {
        _flashBanner = new GameObject("FlashBanner", typeof(RectTransform), typeof(CanvasGroup));
        _flashBanner.transform.SetParent(_root, false);
        _flashCG = _flashBanner.GetComponent<CanvasGroup>();
        _flashCG.alpha = 0;
        _flashCG.blocksRaycasts = false;
        _flashCG.interactable = false;

        var rt = _flashBanner.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.42f);
        rt.anchorMax = new Vector2(1, 0.58f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var txtGo = new GameObject("Txt", typeof(RectTransform), typeof(Text), typeof(Outline), typeof(Shadow));
        txtGo.transform.SetParent(_flashBanner.transform, false);
        Stretch(txtGo);

        _flashText = txtGo.GetComponent<Text>();
        _flashText.font = F();
        _flashText.fontSize = 28;
        _flashText.alignment = TextAnchor.MiddleCenter;
        _flashText.fontStyle = FontStyle.Bold;
        _flashText.raycastTarget = false;
        _flashText.horizontalOverflow = HorizontalWrapMode.Wrap;

        var outline = txtGo.GetComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.9f);
        outline.effectDistance = new Vector2(2, 2);

        var shadow = txtGo.GetComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.6f);
        shadow.effectDistance = new Vector2(3, -3);

        _flashBanner.SetActive(false);
    }

    public void ShowFlashBanner(string text, Color color, float duration = 1.2f)
    {
        if (_flashBanner == null) BuildFlashBanner();

        _flashQueue.Enqueue((text, color, duration));
        if (!_flashPlaying)
            StartCoroutine(PlayFlashQueue());
    }

    IEnumerator PlayFlashQueue()
    {
        _flashPlaying = true;
        while (_flashQueue.Count > 0)
        {
            var (text, color, duration) = _flashQueue.Dequeue();

            _flashText.text = text;
            _flashText.color = color;
            _flashBanner.SetActive(true);
            _flashBanner.transform.SetAsLastSibling();

            var rt = _flashBanner.GetComponent<RectTransform>();
            rt.localScale = new Vector3(0.6f, 0.6f, 1f);

            float fadeIn = 0.15f;
            float hold = duration - 0.45f;
            float fadeOut = 0.3f;

            float t = 0;
            while (t < fadeIn)
            {
                t += Time.deltaTime;
                float p = t / fadeIn;
                _flashCG.alpha = p;
                rt.localScale = Vector3.Lerp(new Vector3(0.6f, 0.6f, 1), Vector3.one, p);
                yield return null;
            }
            _flashCG.alpha = 1;
            rt.localScale = Vector3.one;

            yield return new WaitForSeconds(Mathf.Max(0.1f, hold));

            t = 0;
            while (t < fadeOut)
            {
                t += Time.deltaTime;
                float p = t / fadeOut;
                _flashCG.alpha = 1f - p;
                float s = 1f + p * 0.15f;
                rt.localScale = new Vector3(s, s, 1);
                yield return null;
            }

            _flashCG.alpha = 0;
            _flashBanner.SetActive(false);

            if (_flashQueue.Count > 0)
                yield return new WaitForSeconds(0.15f);
        }
        _flashPlaying = false;
    }

    void RegisterFlashEvents()
    {
        EventBus.Register(EventTypes.PollutionTierUp, OnFlashPollutionTierUp);
        EventBus.Register(EventTypes.PollutionTierDown, OnFlashPollutionTierDown);
        EventBus.Register<LegacyAbility>(EventTypes.LegacyAdded, OnFlashLegacyAdded);
        EventBus.Register<int, LegacyAbility>(EventTypes.LegacyReplaced, OnFlashLegacyReplaced);
    }

    void OnFlashPollutionTierUp()
    {
        var player = GameManager.Instance?.Player;
        if (player == null) return;
        var tier = PollutionPassiveSystem.GetPollutionTier(player.pollution);
        ShowFlashBanner($"{tier.icon} {tier.label}", tier.color, 1.5f);
    }

    void OnFlashPollutionTierDown()
    {
        var player = GameManager.Instance?.Player;
        if (player == null) return;
        var tier = PollutionPassiveSystem.GetPollutionTier(player.pollution);
        ShowFlashBanner($"{tier.icon} {tier.label}", tier.color, 1.2f);
    }

    void OnFlashLegacyAdded(LegacyAbility leg)
    {
        Color c = leg.rarity == 2 ? Purp : leg.rarity == 1 ? Gold : Bright;
        ShowFlashBanner($"{leg.icon} {leg.name}", c, 1.0f);
    }

    void OnFlashLegacyReplaced(int idx, LegacyAbility leg)
    {
        Color c = leg.rarity == 2 ? Purp : leg.rarity == 1 ? Gold : Bright;
        ShowFlashBanner($"{leg.icon} {leg.name}", c, 1.0f);
    }

    public void ShowPickupAnimation(string icon, Vector2Int mapPos)
    {
        StartCoroutine(PlayPickupAnimation(icon, mapPos));
    }

    IEnumerator PlayPickupAnimation(string icon, Vector2Int mapPos)
    {
        var pickupGo = new GameObject("PickupAnim", typeof(RectTransform), typeof(Text), typeof(CanvasGroup));
        pickupGo.transform.SetParent(_root, false);
        
        var rt = pickupGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        var mapCellSize = 36f;
        var mapOffsetX = Screen.width * 0.008f + 18f;
        var mapOffsetY = Screen.height * 0.304f + 18f;
        
        float startX = mapOffsetX + mapPos.x * (mapCellSize + 1);
        float startY = mapOffsetY + mapPos.y * (mapCellSize + 1);
        
        rt.position = new Vector3(startX, startY, 0);
        rt.sizeDelta = new Vector2(40, 40);
        
        var txt = pickupGo.GetComponent<Text>();
        txt.font = F();
        txt.fontSize = 24;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = icon;
        txt.color = Color.white;
        txt.fontStyle = FontStyle.Bold;
        txt.raycastTarget = false;
        
        var cg = pickupGo.GetComponent<CanvasGroup>();
        cg.alpha = 1;
        cg.blocksRaycasts = false;
        
        pickupGo.transform.SetAsLastSibling();
        
        float duration = 0.6f;
        float t = 0;
        
        Vector3 endPos = new Vector3(Screen.width / 2f, Screen.height * 0.85f, 0);
        
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float eased = 1 - Mathf.Pow(1 - p, 3);
            
            rt.position = Vector3.Lerp(new Vector3(startX, startY, 0), endPos, eased);
            
            float scale = 1f + p * 0.5f;
            rt.localScale = Vector3.one * scale;
            
            float alpha = 1f - p * 0.3f;
            cg.alpha = alpha;
            
            float rotate = p * 360f;
            rt.localRotation = Quaternion.Euler(0, 0, rotate);
            
            yield return null;
        }
        
        Destroy(pickupGo);
        
        ShowFlashBanner($"{icon} 获得物品", Gold, 1.0f);
    }
}
