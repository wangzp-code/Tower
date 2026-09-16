using UnityEngine;

public class GameFXInitializer : MonoBehaviour
{
    void Awake()
    {
        if (ScreenEffectsManager.Instance == null)
        {
            GameObject fxGo = new GameObject("ScreenEffectsManager");
            fxGo.AddComponent<ScreenEffectsManager>();
            DontDestroyOnLoad(fxGo);
        }

        if (ScreenEffectsManager.Instance == null && PollutionOverlay.Instance == null)
        {
            GameObject pollGo = new GameObject("PollutionOverlay");
            pollGo.AddComponent<PollutionOverlay>();
            DontDestroyOnLoad(pollGo);
        }
    }
}
