using UnityEngine;

#if UNITY_EDITOR
public class GameTestHarness : MonoBehaviour
{
    public static GameTestHarness Instance { get; private set; }

    [Header("Test Options")]
    public bool autoStartTest = true;
    public float testDelay = 0.5f;

    [Header("Debug Display")]
    public bool showDebugUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureBootstrap();
    }

    private void EnsureBootstrap()
    {
        if (Bootstrap.Instance != null) return;

        GameObject bootstrapObj = new GameObject("Bootstrap");
        var bootstrap = bootstrapObj.AddComponent<Bootstrap>();
        bootstrap.enableDebugMode = true;
        bootstrap.autoStartGame = false;
    }

    private void Start()
    {
        if (autoStartTest)
        {
            Invoke(nameof(RunSmokeTest), testDelay);
        }
    }

    public void RunSmokeTest()
    {









        if (CompleteGameSystem.Instance != null)
        {

        }
    }
}
#endif
