using UnityEngine;
using System.Collections.Generic;

public class ParasiteShaderManager : MonoBehaviour
{
    public static ParasiteShaderManager Instance { get; private set; }

    [Header("Shader Materials")]
    public Material dissolveMaterial;
    public Material pollutionMaterial;
    public Material uiBreakdownMaterial;

    [Header("Noise Textures")]
    public Texture2D dissolveNoise;
    public Texture2D pollutionNoise;
    public Texture2D uiNoise;

    [Header("Animation Settings")]
    public float dissolveDuration = 1.2f;
    public float pollutionSpeed = 1.0f;
    public float uiBreakdownSpeed = 2.0f;

    private Dictionary<string, Coroutine> activeAnimations = new Dictionary<string, Coroutine>();
    private List<Renderer> affectedRenderers = new List<Renderer>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMaterials();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeMaterials()
    {
        if (dissolveMaterial != null)
        {
            dissolveMaterial.SetTexture("_NoiseTex", dissolveNoise);
            dissolveMaterial.SetFloat("_DissolveAmount", 0);
        }
        
        if (pollutionMaterial != null)
        {
            pollutionMaterial.SetTexture("_NoiseTex", pollutionNoise);
            pollutionMaterial.SetFloat("_PollutionAmount", 0);
        }
        
        if (uiBreakdownMaterial != null)
        {
            uiBreakdownMaterial.SetTexture("_NoiseTex", uiNoise);
            uiBreakdownMaterial.SetFloat("_BreakdownAmount", 0);
        }
    }

    public void StartPossessionAnimation(Renderer targetRenderer, System.Action onComplete = null)
    {
        if (targetRenderer == null || dissolveMaterial == null)
        {
            onComplete?.Invoke();
            return;
        }

        string key = "possession_" + targetRenderer.GetInstanceID();
        if (activeAnimations.ContainsKey(key))
        {
            StopCoroutine(activeAnimations[key]);
        }

        Material originalMaterial = targetRenderer.material;
        targetRenderer.material = new Material(dissolveMaterial);
        affectedRenderers.Add(targetRenderer);

        activeAnimations[key] = StartCoroutine(AnimateDissolve(targetRenderer.material, true, () =>
        {
            activeAnimations.Remove(key);
            targetRenderer.material = originalMaterial;
            affectedRenderers.Remove(targetRenderer);
            onComplete?.Invoke();
        }));
    }

    public void StartReleaseAnimation(Renderer targetRenderer, System.Action onComplete = null)
    {
        if (targetRenderer == null || dissolveMaterial == null)
        {
            onComplete?.Invoke();
            return;
        }

        string key = "release_" + targetRenderer.GetInstanceID();
        if (activeAnimations.ContainsKey(key))
        {
            StopCoroutine(activeAnimations[key]);
        }

        targetRenderer.material = new Material(dissolveMaterial);
        targetRenderer.material.SetFloat("_DissolveAmount", 1);
        affectedRenderers.Add(targetRenderer);

        activeAnimations[key] = StartCoroutine(AnimateDissolve(targetRenderer.material, false, () =>
        {
            activeAnimations.Remove(key);
            affectedRenderers.Remove(targetRenderer);
            onComplete?.Invoke();
        }));
    }

    private System.Collections.IEnumerator AnimateDissolve(Material mat, bool dissolveIn, System.Action onComplete)
    {
        float startValue = dissolveIn ? 0 : 1;
        float endValue = dissolveIn ? 1 : 0;
        float elapsed = 0;

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveDuration);
            t = Mathf.SmoothStep(0, 1, t);
            
            float value = Mathf.Lerp(startValue, endValue, t);
            mat.SetFloat("_DissolveAmount", value);
            
            yield return null;
        }

        mat.SetFloat("_DissolveAmount", endValue);
        onComplete?.Invoke();
    }

    public void SetPollutionLevel(float level)
    {
        level = Mathf.Clamp01(level);
        
        if (pollutionMaterial != null)
        {
            pollutionMaterial.SetFloat("_PollutionAmount", level);
            pollutionMaterial.SetFloat("_DistortionAmount", level * 0.15f);
        }

        UpdateAllRendererPollution(level);
    }

    private void UpdateAllRendererPollution(float level)
    {
        foreach (var renderer in affectedRenderers)
        {
            if (renderer.material.HasProperty("_PollutionAmount"))
            {
                renderer.material.SetFloat("_PollutionAmount", level);
            }
        }
    }

    public void StartUIBreakdownEffect(float intensity = 1.0f)
    {
        if (uiBreakdownMaterial == null) return;

        string key = "ui_breakdown";
        if (activeAnimations.ContainsKey(key))
        {
            StopCoroutine(activeAnimations[key]);
        }

        activeAnimations[key] = StartCoroutine(AnimateUIBreakdown(intensity));
    }

    private System.Collections.IEnumerator AnimateUIBreakdown(float intensity)
    {
        float elapsed = 0;
        float duration = 0.5f * intensity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            uiBreakdownMaterial.SetFloat("_BreakdownAmount", Mathf.Sin(t * Mathf.PI) * intensity);
            
            yield return null;
        }

        uiBreakdownMaterial.SetFloat("_BreakdownAmount", 0);
        activeAnimations.Remove("ui_breakdown");
    }

    public void ApplyDissolveToRenderer(Renderer renderer, float amount)
    {
        if (renderer == null || dissolveMaterial == null) return;

        if (!affectedRenderers.Contains(renderer))
        {
            renderer.material = new Material(dissolveMaterial);
            affectedRenderers.Add(renderer);
        }

        renderer.material.SetFloat("_DissolveAmount", Mathf.Clamp01(amount));
    }

    public void RestoreRenderer(Renderer renderer)
    {
        if (affectedRenderers.Contains(renderer))
        {
            Destroy(renderer.material);
            affectedRenderers.Remove(renderer);
        }
    }

    private void OnDestroy()
    {
        foreach (var renderer in affectedRenderers)
        {
            Destroy(renderer.material);
        }
        
        foreach (var coroutine in activeAnimations.Values)
        {
            StopCoroutine(coroutine);
        }
    }
}