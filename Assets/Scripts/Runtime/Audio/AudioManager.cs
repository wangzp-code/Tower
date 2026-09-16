using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : SingletonBase<AudioManager>
{
    const int SfxPoolSize = 5;
    const float CrossfadeDuration = 2f;
    const float DefaultDuckFactor = 0.3f;
    const float SfxRateLimitWindow = 0.05f;

    float _masterVol = 1f;
    float _bgmVol = 0.7f;
    float _sfxVol = 1f;

    AudioSource _bgmSourceA;
    AudioSource _bgmSourceB;
    AudioSource _activeBgmSource;
    AudioSource _uiSfxSource;
    AudioSource _sfxPrimary;
    AudioSource[] _sfxPool;
    int _sfxPoolIdx;

    readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
    readonly Dictionary<string, float> _lastPlayTimes = new Dictionary<string, float>();
    readonly List<float> _duckRequests = new List<float>();
    Coroutine _bgmCoroutine;
    bool _bgmPlaying;
    Coroutine _duckCoroutine;

    static readonly HashSet<string> UiSfxNames = new HashSet<string>
    {
        "click", "error"
    };

    static readonly HashSet<string> RateLimitedSfx = new HashSet<string>
    {
        "move", "hit"
    };

    static readonly Dictionary<string, float> SfxRateLimits = new Dictionary<string, float>
    {
        { "move", 0.06f },
        { "hit", 0.04f }
    };

    static readonly Dictionary<CompleteGameSystem.RunScreen, int> ScreenBgmMap
        = new Dictionary<CompleteGameSystem.RunScreen, int>
        {
            { CompleteGameSystem.RunScreen.MainMenu, 0 },
            { CompleteGameSystem.RunScreen.CharacterSelect, 0 },
            { CompleteGameSystem.RunScreen.Exploration, 1 },
            { CompleteGameSystem.RunScreen.Combat, 2 },
            { CompleteGameSystem.RunScreen.Victory, 0 },
            { CompleteGameSystem.RunScreen.Defeat, 2 },
            { CompleteGameSystem.RunScreen.RunEnd, 0 },
            { CompleteGameSystem.RunScreen.GameOver, 0 },
            { CompleteGameSystem.RunScreen.Ending, 0 },
            { CompleteGameSystem.RunScreen.StageTransition, 1 },
        };

    void Awake()
    {
        base.Awake();
        LoadVolumeSettings();
        InitAudioSources();
        GenerateAllClips();
    }

    void LoadVolumeSettings()
    {
        _masterVol = PlayerPrefs.GetFloat("PT_MasterVol", 1f);
        _bgmVol = PlayerPrefs.GetFloat("PT_BGMVol", 0.7f);
        _sfxVol = PlayerPrefs.GetFloat("PT_SFXVol", 1f);
        AudioListener.volume = _masterVol;
    }

    void InitAudioSources()
    {
        _bgmSourceA = gameObject.AddComponent<AudioSource>();
        _bgmSourceA.loop = true;
        _bgmSourceA.playOnAwake = false;
        _bgmSourceA.volume = 0f;

        _bgmSourceB = gameObject.AddComponent<AudioSource>();
        _bgmSourceB.loop = true;
        _bgmSourceB.playOnAwake = false;
        _bgmSourceB.volume = 0f;

        _activeBgmSource = _bgmSourceA;

        _uiSfxSource = gameObject.AddComponent<AudioSource>();
        _uiSfxSource.loop = false;
        _uiSfxSource.playOnAwake = false;
        _uiSfxSource.volume = _sfxVol;

        _sfxPool = new AudioSource[SfxPoolSize];
        for (int i = 0; i < SfxPoolSize; i++)
        {
            _sfxPool[i] = gameObject.AddComponent<AudioSource>();
            _sfxPool[i].loop = false;
            _sfxPool[i].playOnAwake = false;
            _sfxPool[i].volume = _sfxVol;
        }
        _sfxPrimary = _sfxPool[0];

        bgmSource = _bgmSourceA;
        sfxSource = _sfxPrimary;
    }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM")]
    public AudioClip[] bgmTracks;

    [Header("SFX")]
    public AudioClip[] sfxClips;

    #region Procedural Clip Generation

    static float SoftLimit(float sample, float ceiling = 0.95f)
    {
        if (Mathf.Abs(sample) <= ceiling) return sample;
        float sign = Mathf.Sign(sample);
        float overflow = Mathf.Abs(sample) - ceiling;
        return sign * (ceiling + overflow * 0.15f);
    }

    static AudioClip MakeTone(string name, float freq, float duration, float volume = 0.15f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Lerp(volume, 0.01f, t / duration);
            float s = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope;
            data[i] = SoftLimit(s);
        }
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip MakeSweep(string name, float f1, float f2, float duration, float volume = 0.2f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float ratio = t / duration;
            float freq = f1 * Mathf.Pow(f2 / f1, ratio);
            float envelope = Mathf.Lerp(volume, 0.01f, ratio);
            float s = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope;
            data[i] = SoftLimit(s);
        }
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip MakeChord(string name, float[] freqs, float duration, float volume = 0.1f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        float[] data = new float[samples];
        float perNote = volume / freqs.Length;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Lerp(1f, 0.01f, t / duration);
            float sum = 0f;
            foreach (float f in freqs)
                sum += Mathf.Sin(2f * Mathf.PI * f * t);
            float s = sum * perNote * envelope;
            data[i] = SoftLimit(s);
        }
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip MakeNoise(string name, float duration, float volume = 0.1f)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Lerp(volume, 0.01f, t / duration);
            float s = (Random.value * 2f - 1f) * envelope;
            data[i] = SoftLimit(s);
        }
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip ConcatClips(string name, params (AudioClip clip, float delayBefore)[] parts)
    {
        int sampleRate = 44100;
        int totalSamples = 0;
        foreach (var (clip, delay) in parts)
            totalSamples = Mathf.Max(totalSamples, Mathf.CeilToInt(delay * sampleRate) + clip.samples);

        var result = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
        float[] data = new float[totalSamples];
        foreach (var (clip, delay) in parts)
        {
            float[] partData = new float[clip.samples];
            clip.GetData(partData, 0);
            int offset = Mathf.CeilToInt(delay * sampleRate);
            for (int i = 0; i < partData.Length && offset + i < totalSamples; i++)
                data[offset + i] += partData[i];
        }
        for (int i = 0; i < data.Length; i++)
            data[i] = SoftLimit(data[i]);
        result.SetData(data, 0);
        return result;
    }

    void GenerateAllClips()
    {
        _clips["hit"] = MakeTone("hit", 200, 0.05f);
        _clips["crit"] = MakeSweep("crit", 400, 800, 0.15f);
        _clips["heal"] = MakeChord("heal", new[] { 600f, 750f }, 0.1f);
        _clips["death"] = MakeSweep("death", 200, 50, 0.4f);
        _clips["possess"] = ConcatClips("possess",
            (MakeSweep("p1", 200, 800, 0.3f), 0f),
            (MakeSweep("p2", 800, 400, 0.5f), 0.3f));
        _clips["move"] = MakeTone("move", 300, 0.03f);
        _clips["trait"] = MakeTone("trait", 500, 0.06f);
        _clips["comboUp"] = MakeChord("comboUp", new[] { 500f, 750f, 1000f }, 0.12f);
        _clips["comboBreak"] = MakeSweep("comboBreak", 600, 100, 0.2f);
        _clips["pickup"] = MakeChord("pickup", new[] { 800f, 1000f, 1200f }, 0.08f);
        _clips["boss"] = ConcatClips("boss",
            (MakeChord("b1", new[] { 100f, 150f, 200f }, 0.5f), 0f),
            (MakeSweep("b2", 150, 400, 0.3f), 0.3f),
            (MakeChord("b3", new[] { 200f, 300f, 400f }, 0.4f), 0.6f));
        _clips["levelUp"] = ConcatClips("levelUp",
            (MakeChord("l1", new[] { 400f, 500f, 600f }, 0.1f), 0f),
            (MakeChord("l2", new[] { 600f, 750f, 900f }, 0.15f), 0.12f),
            (MakeChord("l3", new[] { 800f, 1000f, 1200f }, 0.2f), 0.26f));
        _clips["shop"] = ConcatClips("shop",
            (MakeChord("s1", new[] { 600f, 800f }, 0.06f), 0f),
            (MakeTone("s2", 1000, 0.08f), 0.08f));
        _clips["floorUp"] = MakeSweep("floorUp", 300, 800, 0.25f);
        _clips["floorDown"] = MakeSweep("floorDown", 800, 300, 0.25f);
        _clips["alert"] = ConcatClips("alert",
            (MakeTone("a1", 600, 0.05f), 0f),
            (MakeTone("a2", 800, 0.05f), 0.08f));
        _clips["error"] = ConcatClips("error",
            (MakeTone("e1", 150, 0.1f), 0f),
            (MakeTone("e2", 100, 0.15f), 0.12f));
        _clips["defend"] = ConcatClips("defend",
            (MakeChord("d1", new[] { 400f, 500f }, 0.08f), 0f),
            (MakeNoise("d2", 0.05f), 0f));
        _clips["evade"] = MakeSweep("evade", 600, 1200, 0.1f);
        _clips["click"] = MakeTone("click", 800, 0.02f, 0.08f);
        _clips["evolve"] = ConcatClips("evolve",
            (MakeChord("ev1", new[] { 300f, 450f }, 0.15f), 0f),
            (MakeChord("ev2", new[] { 450f, 600f }, 0.15f), 0.15f),
            (MakeChord("ev3", new[] { 600f, 900f, 1200f }, 0.3f), 0.3f));
        _clips["possession_success"] = ConcatClips("possession_success",
            (MakeChord("ps1", new[] { 400f, 600f, 800f }, 0.12f), 0f),
            (MakeSweep("ps2", 500, 1000, 0.3f), 0.12f),
            (MakeChord("ps3", new[] { 600f, 900f, 1200f }, 0.2f), 0.4f));
        _clips["possessFail"] = ConcatClips("possessFail",
            (MakeSweep("pf1", 400, 100, 0.25f), 0f),
            (MakeNoise("pf2", 0.15f), 0.05f));
    }

    #endregion

    #region BGM

    static readonly string[] BgmTrackNames = {
        "Audio/bgm_dark_theme",
        "Audio/bgm_dungeon",
        "Audio/bgm_creepy"
    };

    AudioClip[] _bgmClips;
    int _bgmTrackIdx;

    void LoadBGMClips()
    {
        if (_bgmClips != null) return;
        _bgmClips = new AudioClip[BgmTrackNames.Length];
        for (int i = 0; i < BgmTrackNames.Length; i++)
        {
            _bgmClips[i] = Resources.Load<AudioClip>(BgmTrackNames[i]);
            if (_bgmClips[i] == null)
                Debug.LogWarning($"[AudioManager] BGM clip not found: {BgmTrackNames[i]}");
        }
    }

    IEnumerator BGMCrossfadeLoop()
    {
        LoadBGMClips();
        yield return null;

        while (_bgmPlaying)
        {
            if (_bgmClips == null || _bgmClips.Length == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            var currentClip = _bgmClips[_bgmTrackIdx];
            if (currentClip == null)
            {
                _bgmTrackIdx = (_bgmTrackIdx + 1) % _bgmClips.Length;
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            var src = _activeBgmSource;
            bool alreadyPlaying = src != null && src.isPlaying && src.clip == currentClip;

            if (!alreadyPlaying)
            {
                src.clip = currentClip;
                src.volume = 0f;
                src.Play();
                yield return FadeBgmVolume(src, GetEffectiveBGMVolume(), CrossfadeDuration);
            }

            float holdTime = Mathf.Max(0.5f, currentClip.length - CrossfadeDuration * 2f);
            float elapsed = 0f;
            while (elapsed < holdTime && _bgmPlaying && src.clip == currentClip)
            {
                elapsed += Time.deltaTime;
                src.volume = GetEffectiveBGMVolume();
                yield return null;
            }

            if (!_bgmPlaying) break;

            yield return FadeBgmVolume(src, 0f, CrossfadeDuration);

            src.Stop();
            _bgmTrackIdx = (_bgmTrackIdx + 1) % _bgmClips.Length;
        }
    }

    float GetEffectiveBGMVolume()
    {
        float minDuck = 1f;
        foreach (float f in _duckRequests)
            if (f < minDuck) minDuck = f;
        return _bgmVol * minDuck;
    }

    public void DuckBGM(float factor = DefaultDuckFactor, float duration = -1f)
    {
        _duckRequests.Add(factor);
        if (duration > 0f)
        {
            if (_duckCoroutine != null)
            {
                StopCoroutine(_duckCoroutine);
                _duckCoroutine = null;
            }
            _duckCoroutine = StartCoroutine(DuckTimeoutCoroutine(factor, duration));
        }
    }

    public void RestoreBGM(float factor = -1f)
    {
        if (factor < 0f)
        {
            _duckRequests.Clear();
        }
        else
        {
            _duckRequests.Remove(factor);
        }
        if (_duckRequests.Count == 0 && _duckCoroutine != null)
        {
            StopCoroutine(_duckCoroutine);
            _duckCoroutine = null;
        }
    }

    IEnumerator DuckTimeoutCoroutine(float factor, float duration)
    {
        yield return new WaitForSeconds(duration);
        _duckRequests.Remove(factor);
        _duckCoroutine = null;
    }

    IEnumerator FadeBgmVolume(AudioSource src, float targetVol, float duration)
    {
        float startVol = src.volume;
        float t = 0f;
        while (t < duration && src != null)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            src.volume = Mathf.Lerp(startVol, targetVol, p);
            yield return null;
        }
        if (src != null) src.volume = targetVol;
    }

    public void StartBGM()
    {
        if (_bgmPlaying) return;
        _bgmPlaying = true;
        _bgmTrackIdx = 0;
        _duckRequests.Clear();
        _bgmCoroutine = StartCoroutine(BGMCrossfadeLoop());
    }

    public void StopBGM()
    {
        _bgmPlaying = false;
        if (_bgmCoroutine != null)
        {
            StopCoroutine(_bgmCoroutine);
            _bgmCoroutine = null;
        }
        if (_duckCoroutine != null)
        {
            StopCoroutine(_duckCoroutine);
            _duckCoroutine = null;
        }
        if (_bgmSourceA != null) _bgmSourceA.Stop();
        if (_bgmSourceB != null) _bgmSourceB.Stop();
    }

    public void PlayBGM(int trackIndex)
    {
        if (_bgmClips == null) LoadBGMClips();
        if (_bgmClips != null && trackIndex >= 0 && trackIndex < _bgmClips.Length && _bgmClips[trackIndex] != null)
        {
            _bgmTrackIdx = trackIndex;
            CrossfadeToTrack(trackIndex);
        }
        else if (bgmTracks != null && trackIndex >= 0 && trackIndex < bgmTracks.Length && bgmSource != null)
        {
            bgmSource.clip = bgmTracks[trackIndex];
            bgmSource.Play();
        }
        else
        {
            StartBGM();
        }
    }

    public void SetScreenBGM(CompleteGameSystem.RunScreen screen)
    {
        if (ScreenBgmMap.TryGetValue(screen, out int trackIdx))
        {
            PlayBGM(trackIdx);
        }
    }

    void CrossfadeToTrack(int targetIdx)
    {
        var oldSrc = _activeBgmSource;
        var newSrc = oldSrc == _bgmSourceA ? _bgmSourceB : _bgmSourceA;

        var clip = _bgmClips != null && targetIdx < _bgmClips.Length ? _bgmClips[targetIdx] : null;
        if (clip == null) return;

        if (_bgmCoroutine != null)
        {
            StopCoroutine(_bgmCoroutine);
            _bgmCoroutine = null;
        }

        _activeBgmSource = newSrc;
        _bgmTrackIdx = targetIdx;
        _bgmCoroutine = StartCoroutine(CrossfadeCoroutine(oldSrc, newSrc, clip));
    }

    IEnumerator CrossfadeCoroutine(AudioSource oldSrc, AudioSource newSrc, AudioClip newClip)
    {
        _bgmPlaying = true;

        if (oldSrc != null && oldSrc.isPlaying)
        {
            yield return FadeBgmVolume(oldSrc, 0f, CrossfadeDuration);
            oldSrc.Stop();
        }

        newSrc.clip = newClip;
        newSrc.volume = 0f;
        newSrc.Play();
        yield return FadeBgmVolume(newSrc, GetEffectiveBGMVolume(), CrossfadeDuration);

        _bgmCoroutine = StartCoroutine(BGMCrossfadeLoop());
    }

    #endregion

    #region SFX Playback

    AudioSource GetAvailableSfxSource()
    {
        for (int i = 0; i < _sfxPool.Length; i++)
        {
            int idx = (_sfxPoolIdx + i) % _sfxPool.Length;
            if (_sfxPool[idx] != null && !_sfxPool[idx].isPlaying)
            {
                _sfxPoolIdx = (idx + 1) % _sfxPool.Length;
                return _sfxPool[idx];
            }
        }
        var src = _sfxPool[_sfxPoolIdx];
        _sfxPoolIdx = (_sfxPoolIdx + 1) % _sfxPool.Length;
        return src;
    }

    bool IsRateLimited(string sfxName)
    {
        if (!RateLimitedSfx.Contains(sfxName)) return false;
        float limit = SfxRateLimits.TryGetValue(sfxName, out float v) ? v : SfxRateLimitWindow;
        if (_lastPlayTimes.TryGetValue(sfxName, out float lastTime))
        {
            if (Time.time - lastTime < limit) return true;
        }
        return false;
    }

    void RecordPlayTime(string sfxName)
    {
        if (RateLimitedSfx.Contains(sfxName))
            _lastPlayTimes[sfxName] = Time.time;
    }

    float RandomPitch()
    {
        return 0.88f + Random.value * 0.24f;
    }

    public void PlaySFX(string sfxName)
    {
        PlaySFX(sfxName, 1f, -1f);
    }

    public void PlaySFX(string sfxName, float volumeScale, float pitch = -1f)
    {
        if (IsRateLimited(sfxName)) return;

        if (_clips.TryGetValue(sfxName, out var clip))
        {
            AudioSource src;
            if (UiSfxNames.Contains(sfxName))
            {
                src = _uiSfxSource;
            }
            else
            {
                src = GetAvailableSfxSource();
            }

            if (src != null)
            {
                float vol = _sfxVol * Mathf.Clamp(volumeScale, 0f, 2f);
                float p = pitch > 0f ? pitch : RandomPitch();
                src.pitch = p;
                src.PlayOneShot(clip, vol);
                RecordPlayTime(sfxName);
            }
            return;
        }

        if (sfxClips != null)
        {
            foreach (var c in sfxClips)
            {
                if (c != null && c.name == sfxName)
                {
                    AudioSource src = UiSfxNames.Contains(sfxName) ? _uiSfxSource : GetAvailableSfxSource();
                    if (src != null)
                    {
                        float vol = _sfxVol * Mathf.Clamp(volumeScale, 0f, 2f);
                        float p = pitch > 0f ? pitch : RandomPitch();
                        src.pitch = p;
                        src.PlayOneShot(c, vol);
                        RecordPlayTime(sfxName);
                    }
                    return;
                }
            }
        }
    }

    public void PlaySFXIfExists(string sfxName)
    {
        if (_clips.ContainsKey(sfxName))
            PlaySFX(sfxName);
    }

    #endregion

    #region Volume Control

    public void SetMasterVolume(float volume)
    {
        _masterVol = Mathf.Clamp01(volume);
        AudioListener.volume = _masterVol;
        PlayerPrefs.SetFloat("PT_MasterVol", _masterVol);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float volume)
    {
        _bgmVol = Mathf.Clamp01(volume);
        float eff = GetEffectiveBGMVolume();
        if (_bgmSourceA != null) _bgmSourceA.volume = _bgmSourceA.isPlaying ? eff : 0f;
        if (_bgmSourceB != null) _bgmSourceB.volume = _bgmSourceB.isPlaying ? eff : 0f;
        if (bgmSource != null) bgmSource.volume = eff;
        PlayerPrefs.SetFloat("PT_BGMVol", _bgmVol);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVol = Mathf.Clamp01(volume);
        if (_sfxPool != null)
        {
            foreach (var src in _sfxPool)
                if (src != null) src.volume = _sfxVol;
        }
        if (_uiSfxSource != null) _uiSfxSource.volume = _sfxVol;
        if (sfxSource != null) sfxSource.volume = _sfxVol;
        PlayerPrefs.SetFloat("PT_SFXVol", _sfxVol);
        PlayerPrefs.Save();
    }

    public float GetMasterVolume() => _masterVol;
    public float GetBGMVolume() => _bgmVol;
    public float GetSFXVolume() => _sfxVol;

    #endregion

    protected override void OnDestroy()
    {
        StopBGM();
        StopAllCoroutines();
        if (_bgmSourceA != null) _bgmSourceA.Stop();
        if (_bgmSourceB != null) _bgmSourceB.Stop();
        if (_uiSfxSource != null) _uiSfxSource.Stop();
        if (_sfxPool != null)
        {
            foreach (var src in _sfxPool)
                if (src != null) src.Stop();
        }
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        _bgmPlaying = false;
        if (_bgmCoroutine != null)
        {
            StopCoroutine(_bgmCoroutine);
            _bgmCoroutine = null;
        }
        if (_duckCoroutine != null)
        {
            StopCoroutine(_duckCoroutine);
            _duckCoroutine = null;
        }
        if (_bgmSourceA != null) _bgmSourceA.Stop();
        if (_bgmSourceB != null) _bgmSourceB.Stop();
        if (_uiSfxSource != null) _uiSfxSource.Stop();
        if (_sfxPool != null)
        {
            foreach (var src in _sfxPool)
                if (src != null) src.Stop();
        }
        base.OnDisable();
    }
}