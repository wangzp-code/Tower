using UnityEngine;
using System.Collections.Generic;

public enum BattleAnimationType
{
    PossessionTransition,
    EvolutionBurst,
    DeathReplay,
    AttackHit,
    DefenseShield,
    SkillActivation,
    PollutionCorruption,
    VictoryCelebration
}

public class TimelineEvent
{
    public float time;
    public string eventType;
    public Dictionary<string, object> parameters;
    public System.Action<TimelineEvent> callback;

    public TimelineEvent(float time, string eventType, Dictionary<string, object> parameters = null)
    {
        this.time = time;
        this.eventType = eventType;
        this.parameters = parameters ?? new Dictionary<string, object>();
    }
}

public class BattleAnimationManager : SingletonBase<BattleAnimationManager>
{
    [Header("Animation Settings")]
    public float defaultAnimationDuration = 1.2f;
    public float timeFreezeDuration = 0.3f;
    public float cameraShakeIntensity = 0.1f;

    [Header("Effect Prefabs")]
    public GameObject possessionBurst;
    public GameObject evolutionExplosion;
    public GameObject hitEffect;
    public GameObject shieldEffect;

    [Header("Audio Clips")]
    public AudioClip possessionSound;
    public AudioClip evolutionSound;
    public AudioClip hitSound;
    public AudioClip shieldSound;

    private List<TimelineEvent> currentTimeline = new List<TimelineEvent>();
    private float animationStartTime;
    private bool isAnimating = false;
    private Camera mainCamera;

    private void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    public void PlayAnimation(BattleAnimationType type, Transform target = null, System.Action onComplete = null)
    {
        if (isAnimating)
        {
            StopCurrentAnimation();
        }

        isAnimating = true;
        animationStartTime = Time.time;

        switch (type)
        {
            case BattleAnimationType.PossessionTransition:
                PlayPossessionTransition(target, onComplete);
                break;
            case BattleAnimationType.EvolutionBurst:
                PlayEvolutionBurst(target, onComplete);
                break;
            case BattleAnimationType.AttackHit:
                PlayAttackHit(target, onComplete);
                break;
            case BattleAnimationType.DefenseShield:
                PlayDefenseShield(target, onComplete);
                break;
            case BattleAnimationType.PollutionCorruption:
                PlayPollutionCorruption(target, onComplete);
                break;
        }
    }

    private void PlayPossessionTransition(Transform target, System.Action onComplete)
    {
        currentTimeline = new List<TimelineEvent>
        {
            new TimelineEvent(0.0f, "dissolve_start"),
            new TimelineEvent(0.3f, "camera_shake", new Dictionary<string, object> { { "intensity", cameraShakeIntensity } }),
            new TimelineEvent(0.5f, "target_highlight", new Dictionary<string, object> { { "target", target } }),
            new TimelineEvent(0.8f, "screen_flash", new Dictionary<string, object> { { "color", Color.black } }),
            new TimelineEvent(0.85f, "effect_burst", new Dictionary<string, object> { { "prefab", possessionBurst }, { "position", target.position } }),
            new TimelineEvent(0.9f, "play_sound", new Dictionary<string, object> { { "clip", possessionSound } }),
            new TimelineEvent(1.0f, "ui_update"),
            new TimelineEvent(1.2f, "animation_end")
        };

        StartCoroutine(RunTimeline(onComplete));
    }

    private void PlayEvolutionBurst(Transform target, System.Action onComplete)
    {
        currentTimeline = new List<TimelineEvent>
        {
            new TimelineEvent(0.0f, "time_freeze", new Dictionary<string, object> { { "duration", timeFreezeDuration } }),
            new TimelineEvent(0.1f, "camera_shake", new Dictionary<string, object> { { "intensity", cameraShakeIntensity * 2 } }),
            new TimelineEvent(0.2f, "effect_explosion", new Dictionary<string, object> { { "prefab", evolutionExplosion }, { "position", target.position } }),
            new TimelineEvent(0.25f, "play_sound", new Dictionary<string, object> { { "clip", evolutionSound } }),
            new TimelineEvent(0.5f, "time_unfreeze"),
            new TimelineEvent(0.8f, "ui_update"),
            new TimelineEvent(1.0f, "animation_end")
        };

        StartCoroutine(RunTimeline(onComplete));
    }

    private void PlayAttackHit(Transform target, System.Action onComplete)
    {
        currentTimeline = new List<TimelineEvent>
        {
            new TimelineEvent(0.0f, "camera_shake", new Dictionary<string, object> { { "intensity", cameraShakeIntensity * 0.5f } }),
            new TimelineEvent(0.1f, "effect_hit", new Dictionary<string, object> { { "prefab", hitEffect }, { "position", target.position } }),
            new TimelineEvent(0.1f, "play_sound", new Dictionary<string, object> { { "clip", hitSound } }),
            new TimelineEvent(0.3f, "animation_end")
        };

        StartCoroutine(RunTimeline(onComplete));
    }

    private void PlayDefenseShield(Transform target, System.Action onComplete)
    {
        currentTimeline = new List<TimelineEvent>
        {
            new TimelineEvent(0.0f, "effect_shield", new Dictionary<string, object> { { "prefab", shieldEffect }, { "position", target.position } }),
            new TimelineEvent(0.0f, "play_sound", new Dictionary<string, object> { { "clip", shieldSound } }),
            new TimelineEvent(0.5f, "animation_end")
        };

        StartCoroutine(RunTimeline(onComplete));
    }

    private void PlayPollutionCorruption(Transform target, System.Action onComplete)
    {
        currentTimeline = new List<TimelineEvent>
        {
            new TimelineEvent(0.0f, "pollution_start"),
            new TimelineEvent(0.5f, "ui_breakdown", new Dictionary<string, object> { { "intensity", 0.5f } }),
            new TimelineEvent(1.0f, "animation_end")
        };

        StartCoroutine(RunTimeline(onComplete));
    }

    private System.Collections.IEnumerator RunTimeline(System.Action onComplete)
    {
        float elapsed = 0;
        int eventIndex = 0;

        while (eventIndex < currentTimeline.Count)
        {
            elapsed = Time.time - animationStartTime;

            if (elapsed >= currentTimeline[eventIndex].time)
            {
                ExecuteEvent(currentTimeline[eventIndex]);
                eventIndex++;
            }

            yield return null;
        }

        isAnimating = false;
        onComplete?.Invoke();
    }

    private void ExecuteEvent(TimelineEvent timelineEvent)
    {
        switch (timelineEvent.eventType)
        {
            case "dissolve_start":
                StartDissolveEffect();
                break;
            case "camera_shake":
                float intensity = (float)timelineEvent.parameters["intensity"];
                StartCameraShake(intensity);
                break;
            case "target_highlight":
                Transform target = (Transform)timelineEvent.parameters["target"];
                HighlightTarget(target);
                break;
            case "screen_flash":
                Color color = (Color)timelineEvent.parameters["color"];
                FlashScreen(color);
                break;
            case "effect_burst":
            case "effect_explosion":
            case "effect_hit":
            case "effect_shield":
                GameObject prefab = (GameObject)timelineEvent.parameters["prefab"];
                Vector3 position = (Vector3)timelineEvent.parameters["position"];
                PlayEffectAtPosition(prefab, position);
                break;
            case "play_sound":
                AudioClip clip = (AudioClip)timelineEvent.parameters["clip"];
                PlaySound(clip);
                break;
            case "time_freeze":
                float duration = (float)timelineEvent.parameters["duration"];
                StartTimeFreeze(duration);
                break;
            case "time_unfreeze":
                UnfreezeTime();
                break;
            case "ui_update":
                UpdateUI();
                break;
            case "ui_breakdown":
                float uiIntensity = (float)timelineEvent.parameters["intensity"];
                TriggerUIBreakdown(uiIntensity);
                break;
            case "pollution_start":
                StartPollutionEffect();
                break;
            case "animation_end":
                break;
        }
    }

    private void StartDissolveEffect()
    {
        if (ParasiteShaderManager.Instance != null)
        {
            ParasiteShaderManager.Instance.SetPollutionLevel(1f);
        }
    }

    private void StartCameraShake(float intensity)
    {
        if (mainCamera != null)
        {
            StartCoroutine(ShakeCamera(intensity, 0.2f));
        }
    }

    private System.Collections.IEnumerator ShakeCamera(float intensity, float duration)
    {
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * intensity;
            float y = UnityEngine.Random.Range(-1f, 1f) * intensity;
            mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.localPosition = originalPos;
    }

    private void HighlightTarget(Transform target)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.cyan * 2);
        }
    }

    private void FlashScreen(Color color)
    {
        ScreenFlashEffect.Instance?.Flash(color, 0.1f);
    }

    private void PlayEffectAtPosition(GameObject prefab, Vector3 position)
    {
        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            
            MonoBehaviour[] components = instance.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                comp.enabled = true;
            }
            
            Destroy(instance, 2f);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip.name);
        }
    }

    private void StartTimeFreeze(float duration)
    {
        Time.timeScale = 0.1f;
        Invoke(nameof(UnfreezeTime), duration);
    }

    private void UnfreezeTime()
    {
        Time.timeScale = 1f;
    }

    private void UpdateUI()
    {
        GameManager.Instance?.NotifyPlayerStatsChanged();
    }

    private void TriggerUIBreakdown(float intensity)
    {
        if (ParasiteShaderManager.Instance != null)
        {
            ParasiteShaderManager.Instance.StartUIBreakdownEffect(intensity);
        }
    }

    private void StartPollutionEffect()
    {
        if (ParasiteShaderManager.Instance != null)
        {
            ParasiteShaderManager.Instance.SetPollutionLevel(1f);
        }
    }

    public void StopCurrentAnimation()
    {
        StopAllCoroutines();
        isAnimating = false;
        Time.timeScale = 1f;
        
        if (ParasiteShaderManager.Instance != null)
        {
            ParasiteShaderManager.Instance.SetPollutionLevel(0f);
        }
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }

    protected override void OnDestroy()
    {
        StopAllCoroutines();
        CancelInvoke();
        isAnimating = false;
        Time.timeScale = 1f;
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        StopAllCoroutines();
        CancelInvoke();
        isAnimating = false;
        Time.timeScale = 1f;
        base.OnDisable();
    }
}