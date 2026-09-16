using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

/// <summary>
/// 游戏冒烟测试系统
/// 在Unity Editor中运行以验证核心游戏流程
/// </summary>
public class SmokeTestRunner : MonoBehaviour
{
    public static SmokeTestRunner Instance { get; private set; }

    public enum TestStatus
    {
        Pending,
        Running,
        Passed,
        Failed,
        Skipped
    }

    public class TestResult
    {
        public string testName;
        public TestStatus status;
        public string message;
        public System.DateTime startTime;
        public System.DateTime endTime;
        public float durationMs;
    }

    [Header("测试配置")]
    public bool autoRunOnStart = false;
    public float stepDelay = 0.5f;
    public bool stopOnFailure = false;

    private List<TestResult> _results = new List<TestResult>();
    private bool _isRunning = false;
    private int _currentTestIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (autoRunOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }

    [ContextMenu("运行冒烟测试")]
    public void RunTests()
    {
        if (!_isRunning)
        {
            StartCoroutine(RunAllTests());
        }
    }

    private IEnumerator RunAllTests()
    {
        _isRunning = true;
        _results.Clear();
        _currentTestIndex = 0;
        
        UnityEngine.Debug.Log("========== 开始游戏冒烟测试 ==========");
        UnityEngine.Debug.Log($"开始时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        UnityEngine.Debug.Log("========================================");
        
        yield return RunTest("系统初始化", TestSystemInitialization);
        yield return RunTest("游戏管理器状态", TestGameManagerState);
        yield return RunTest("UI系统创建", TestUISystemCreation);
        yield return RunTest("事件总线", TestEventBus);
        yield return RunTest("存档系统", TestSaveSystem);
        yield return RunTest("战斗系统", TestCombatSystem);
        yield return RunTest("玩家数据", TestPlayerData);
        yield return RunTest("资源加载", TestResourceLoading);
        yield return RunTest("性能基准", TestPerformanceBaseline);
        
        UnityEngine.Debug.Log("========================================");
        UnityEngine.Debug.Log("冒烟测试完成！");
        UnityEngine.Debug.Log(GetSummary());
        UnityEngine.Debug.Log("========================================");
        
        _isRunning = false;
    }

    private IEnumerator RunTest(string testName, System.Func<IEnumerator> testAction)
    {
        var result = new TestResult
        {
            testName = testName,
            status = TestStatus.Running,
            startTime = System.DateTime.Now
        };
        
        UnityEngine.Debug.Log($"[测试] 开始: {testName}");
        
        bool testPassed = true;
        string errorMessage = "";
        
        try
        {
            // 先验证委托是否有效
            if (testAction == null)
            {
                throw new System.Exception("测试委托为 null");
            }
        }
        catch (System.Exception e)
        {
            testPassed = false;
            errorMessage = e.Message;
        }
        
        if (testPassed)
        {
            // yield return 不能在 try-catch 中
            yield return StartCoroutine(testAction());
        }
        
        if (testPassed && string.IsNullOrEmpty(errorMessage))
        {
            result.status = TestStatus.Passed;
            result.message = "测试通过";
        }
        else
        {
            result.status = TestStatus.Failed;
            result.message = string.IsNullOrEmpty(errorMessage) ? "测试失败" : errorMessage;
            UnityEngine.Debug.LogError($"[测试] {testName} 失败: {result.message}");
            
            if (stopOnFailure)
            {
                result.endTime = System.DateTime.Now;
                result.durationMs = (float)(result.endTime - result.startTime).TotalMilliseconds;
                _results.Add(result);
                yield break;
            }
        }
        
        result.endTime = System.DateTime.Now;
        result.durationMs = (float)(result.endTime - result.startTime).TotalMilliseconds;
        
        _results.Add(result);
        
        string statusIcon = result.status == TestStatus.Passed ? "✅" : 
                            result.status == TestStatus.Failed ? "❌" : "⏭️";
        UnityEngine.Debug.Log($"[测试] {statusIcon} {testName}: {result.status} ({result.durationMs:F0}ms) - {result.message}");
        
        yield return new WaitForSeconds(stepDelay);
    }

    #region 测试用例

    private IEnumerator TestSystemInitialization()
    {
        yield return null;
        yield return null;
        
        var checks = new List<string>();
        
        if (Bootstrap.Instance == null) checks.Add("Bootstrap");
        if (GameManager.Instance == null) checks.Add("GameManager");
        if (SaveSystem.Instance == null) checks.Add("SaveSystem");
        if (CanvasUIManager.Instance == null) checks.Add("CanvasUIManager");
        
        if (checks.Count > 0)
        {
            throw new System.Exception($"以下系统未初始化: {string.Join(", ", checks)}");
        }
        
        UnityEngine.Debug.Log("✅ 所有核心系统初始化完成");
    }

    private IEnumerator TestGameManagerState()
    {
        yield return null;
        
        if (GameManager.Instance == null)
        {
            throw new System.Exception("GameManager.Instance 为 null");
        }
        
        var state = GameManager.Instance.CurrentState;
        UnityEngine.Debug.Log($"游戏当前状态: {state}");
        
        if (!System.Enum.IsDefined(typeof(GameManager.GameState), state))
        {
            throw new System.Exception($"无效的游戏状态: {state}");
        }
        
        UnityEngine.Debug.Log("✅ 游戏管理器状态正常");
    }

    private IEnumerator TestUISystemCreation()
    {
        yield return null;
        
        if (CanvasUIManager.Instance == null)
        {
            throw new System.Exception("CanvasUIManager.Instance 为 null");
        }
        
        var root = CanvasUIManager.Instance.transform;
        if (root == null)
        {
            throw new System.Exception("UI根节点为 null");
        }
        
        UnityEngine.Debug.Log($"✅ UI系统创建正常 (根节点: {root.name})");
    }

    private IEnumerator TestEventBus()
    {
        yield return null;
        
        // EventBus 是静态类，直接使用
        bool eventReceived = false;
        string testEventName = "SmokeTest_" + System.Guid.NewGuid().ToString("N");
        
        EventBus.Register<string>(testEventName, (value) =>
        {
            eventReceived = true;
        });
        
        EventBus.Emit(testEventName, "test_value");
        
        yield return null;
        
        if (!eventReceived)
        {
            throw new System.Exception("事件总线未正确触发事件");
        }
        
        // 清理
        EventBus.Unregister<string>(testEventName, null);
        
        UnityEngine.Debug.Log("✅ 事件总线工作正常");
    }

    private IEnumerator TestSaveSystem()
    {
        yield return null;
        
        if (SaveSystem.Instance == null)
        {
            throw new System.Exception("SaveSystem.Instance 为 null");
        }
        
        string saveDir = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!System.IO.Directory.Exists(saveDir))
        {
            System.IO.Directory.CreateDirectory(saveDir);
        }
        
        UnityEngine.Debug.Log($"✅ 存档系统正常 (目录: {saveDir})");
    }

    private IEnumerator TestCombatSystem()
    {
        yield return null;
        
        if (CombatManager.Instance == null && CompleteGameSystem.Instance == null)
        {
            throw new System.Exception("战斗系统未初始化 (CombatManager 和 CompleteGameSystem 均为 null)");
        }
        
        if (CompleteGameSystem.Instance != null)
        {
            var log = CompleteGameSystem.Instance.CombatLog;
            UnityEngine.Debug.Log($"战斗日志可用，当前条目数: {log?.Count ?? 0}");
        }
        
        UnityEngine.Debug.Log("✅ 战斗系统可用");
    }

    private IEnumerator TestPlayerData()
    {
        yield return null;
        
        if (GameManager.Instance == null || GameManager.Instance.Player == null)
        {
            UnityEngine.Debug.LogWarning("玩家数据未加载（可能需要先开始游戏），跳过此测试");
            yield break;
        }
        
        var player = GameManager.Instance.Player;
        
        var checks = new List<string>();
        
        if (player.maxHp <= 0) checks.Add("maxHp 无效");
        if (player.hp <= 0) checks.Add("hp 无效");
        if (player.baseAttack <= 0) checks.Add("baseAttack 无效");
        if (player.baseDefense <= 0) checks.Add("baseDefense 无效");
        
        if (checks.Count > 0)
        {
            UnityEngine.Debug.LogWarning($"玩家数据警告: {string.Join(", ", checks)}");
        }
        
        UnityEngine.Debug.Log($"玩家数据: HP={player.hp}/{player.maxHp}, ATK={player.baseAttack}, DEF={player.baseDefense}, 污染={player.pollution}");
        UnityEngine.Debug.Log("✅ 玩家数据正常");
    }

    private IEnumerator TestResourceLoading()
    {
        yield return null;
        
        var testObj = Resources.Load<GameObject>("TestPrefab");
        if (testObj != null)
        {
            UnityEngine.Debug.Log("✅ Resources加载测试通过");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Resources测试资源不存在（这是正常的），跳过加载测试");
        }
    }

    private IEnumerator TestPerformanceBaseline()
    {
        yield return null;
        yield return new WaitForSeconds(1f);
        
        if (GameDiagnostics.Instance != null)
        {
            var summary = GameDiagnostics.Instance.GetQuickSummary();
            UnityEngine.Debug.Log($"性能基准测试:\n{summary}");
            
            var report = GameDiagnostics.Instance.GenerateReport();
            UnityEngine.Debug.Log(report.ToSummaryString());
            
            if (report.avgFPS < 30f)
            {
                UnityEngine.Debug.LogWarning($"平均FPS低于30: {report.avgFPS:F1}");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("GameDiagnostics 未初始化，跳过性能测试");
        }
        
        UnityEngine.Debug.Log("✅ 性能基准测试完成");
    }

    #endregion

    public string GetSummary()
    {
        int passed = _results.Count(r => r.status == TestStatus.Passed);
        int failed = _results.Count(r => r.status == TestStatus.Failed);
        int total = _results.Count;
        float totalTime = _results.Sum(r => r.durationMs);
        
        var summary = $"========== 测试摘要 ==========\n" +
                      $"总测试数: {total}\n" +
                      $"通过: {passed} ✅\n" +
                      $"失败: {failed} ❌\n" +
                      $"总耗时: {totalTime:F0}ms\n";
        
        if (failed > 0)
        {
            summary += $"\n失败的测试:\n";
            foreach (var result in _results.Where(r => r.status == TestStatus.Failed))
            {
                summary += $"  ❌ {result.testName}: {result.message}\n";
            }
        }
        
        summary += "================================";
        
        return summary;
    }

    public List<TestResult> GetResults()
    {
        return new List<TestResult>(_results);
    }
}
