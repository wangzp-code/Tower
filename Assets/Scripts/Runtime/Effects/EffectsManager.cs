using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EffectsManager : SingletonBase<EffectsManager>
{
    [Header("特效容器")]
    private GameObject _effectsContainer;
    private Camera _mainCamera;

    private Canvas _flashCanvas;
    private Image _flashImage;
    private readonly System.Collections.Generic.Queue<GameObject> _hitMarkerPool = new System.Collections.Generic.Queue<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        InitializeEffects();
    }

    void InitializeEffects()
    {
        _effectsContainer = new GameObject("EffectsContainer");
        _effectsContainer.transform.SetParent(transform);
        _mainCamera = Camera.main;
    }

    public void PlayHitEffect(Vector3 position, bool isCrit = false)
    {
        if (isCrit)
        {
            ScreenEffectsManager.Instance?.FlashCrit();
            Shake(0.2f, 0.2f);
            CreateHitMarker(position, new Color(0.9f, 0.8f, 0.3f), "CRIT!");
        }
        else
        {
            ScreenEffectsManager.Instance?.FlashDamage();
            Shake(0.1f, 0.1f);
            CreateHitMarker(position, new Color(0.9f, 0.4f, 0.2f), "HIT");
        }
    }

    public void PlayHealEffect(Vector3 position)
    {
        ScreenEffectsManager.Instance?.FlashHeal();
        CreateHitMarker(position, new Color(0.2f, 0.8f, 0.5f), "+HP");
    }

    public void PlayPossessEffect(Vector3 from, Vector3 to)
    {
        StartCoroutine(PossessAnimation(from, to));
    }

    IEnumerator PossessAnimation(Vector3 from, Vector3 to)
    {
        CreateHitMarker(from, new Color(0.7f, 0.3f, 0.95f), "POSSESS");
        ScreenEffectsManager.Instance?.FlashPossess();
        
        yield return new WaitForSeconds(0.3f);
        
        CreateHitMarker(to, new Color(0.7f, 0.3f, 0.95f), "SUCCESS");
        Shake(0.15f, 0.15f);
    }

    public void PlayEvolutionEffect(Vector3 position)
    {
        StartCoroutine(EvolutionAnimation(position));
    }

    IEnumerator EvolutionAnimation(Vector3 position)
    {
        ScreenEffectsManager.Instance?.FlashHeal();
        
        for (int i = 0; i < 3; i++)
        {
            CreateHitMarker(position + Random.insideUnitSphere * 0.5f, 
                new Color(1f, 0.75f, 0.3f), "EVOLVE");
            yield return new WaitForSeconds(0.2f);
        }
        
        CreateHitMarker(position, new Color(1f, 0.75f, 0.3f), "LEVEL UP!");
        Shake(0.2f, 0.15f);
    }

    public void PlayPollutionEffect(float pollution)
    {
        if (PollutionOverlay.Instance != null)
        {
            PollutionOverlay.Instance.UpdatePollution(pollution);
        }
    }

    public void Shake(float duration = 0.15f, float magnitude = 0.1f)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = _mainCamera != null ? _mainCamera.transform.localPosition : Vector3.zero;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float shakeMultiplier = 1f - t;
            
            float x = Random.Range(-1f, 1f) * magnitude * shakeMultiplier;
            float y = Random.Range(-1f, 1f) * magnitude * shakeMultiplier;
            
            if (_mainCamera != null)
            {
                _mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            }
            
            yield return null;
        }
        
        if (_mainCamera != null)
        {
            _mainCamera.transform.localPosition = originalPos;
        }
    }

    public void TriggerGlitch(float intensity = 0.5f, float duration = 0.3f)
    {
        StartCoroutine(GlitchCoroutine(intensity, duration));
    }

    IEnumerator GlitchCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            if (Random.value < 0.3f)
            {
                float offsetX = (Random.value - 0.5f) * intensity * 50f;
                float offsetY = (Random.value - 0.5f) * intensity * 10f;
                
                if (_mainCamera != null)
                {
                    _mainCamera.rect = new Rect(offsetX / Screen.width, offsetY / Screen.height, 
                        1f, 1f);
                }
            }
            
            yield return null;
        }
        
        if (_mainCamera != null)
        {
            _mainCamera.rect = new Rect(0, 0, 1, 1);
        }
    }

    public void FlashScreen(Color color, float duration = 0.15f)
    {
        StartCoroutine(FlashCoroutine(color, duration));
    }

    IEnumerator FlashCoroutine(Color color, float duration)
    {
        if (_flashCanvas == null)
        {
            GameObject flashGo = new GameObject("FlashOverlay");
            flashGo.transform.SetParent(_effectsContainer.transform);
            RectTransform rt = flashGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _flashCanvas = flashGo.AddComponent<Canvas>();
            _flashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _flashCanvas.sortingOrder = 999;
            _flashImage = flashGo.AddComponent<Image>();
        }

        _flashCanvas.gameObject.SetActive(true);
        _flashImage.color = new Color(color.r, color.g, color.b, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Sin(t * Mathf.PI) * color.a;
            _flashImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        _flashCanvas.gameObject.SetActive(false);
    }

    void CreateHitMarker(Vector3 worldPosition, Color color, string text)
    {
        if (_mainCamera == null) return;

        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(worldPosition);

        GameObject markerGo;
        if (_hitMarkerPool.Count > 0)
        {
            markerGo = _hitMarkerPool.Dequeue();
            markerGo.SetActive(true);
            markerGo.GetComponent<RectTransform>().position = screenPosition;
            Text textComp = markerGo.GetComponentInChildren<Text>();
            if (textComp != null) { textComp.text = text; textComp.color = color; }
            CanvasGroup cg = markerGo.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }
        else
        {
            markerGo = new GameObject("HitMarker");
            markerGo.transform.SetParent(_effectsContainer.transform);
            RectTransform rt = markerGo.AddComponent<RectTransform>();
            rt.position = screenPosition;
            rt.sizeDelta = new Vector2(100, 50);
            Canvas canvas = markerGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            Text textComp = markerGo.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 24;
            textComp.color = color;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontStyle = FontStyle.Bold;
            Outline outline = markerGo.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, 2f);
            markerGo.AddComponent<CanvasGroup>();
        }

        StartCoroutine(AnimateHitMarker(markerGo));
    }

    IEnumerator AnimateHitMarker(GameObject marker)
    {
        RectTransform rt = marker.GetComponent<RectTransform>();
        CanvasGroup cg = marker.GetComponent<CanvasGroup>();
        Vector3 startPos = rt.position;

        float elapsed = 0f;
        float duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.position = startPos + Vector3.up * t * 50f;
            cg.alpha = 1f - t;
            yield return null;
        }

        marker.SetActive(false);
        _hitMarkerPool.Enqueue(marker);
    }

    protected override void OnDestroy()
    {
        StopAllCoroutines();
        if (_mainCamera != null)
        {
            _mainCamera.rect = new Rect(0, 0, 1, 1);
        }
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        StopAllCoroutines();
        if (_mainCamera != null)
        {
            _mainCamera.rect = new Rect(0, 0, 1, 1);
        }
        base.OnDisable();
    }
}