using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class ProjectSetupWizard : EditorWindow
{
    [MenuItem("Tools/Setup Project")]
    public static void ShowWindow()
    {
        GetWindow<ProjectSetupWizard>("Project Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Parasite Tower Setup Wizard", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("1. Create All ScriptableObjects"))
        {
            CreateAllScriptableObjects();
        }

        if (GUILayout.Button("2. Create Scenes"))
        {
            CreateScenes();
        }

        if (GUILayout.Button("3. Configure Build Settings"))
        {
            ConfigureBuildSettings();
        }

        if (GUILayout.Button("4. Run All Setup"))
        {
            RunCompleteSetup();
        }

        GUILayout.Space(20);
        GUILayout.Label("After setup, open BootstrapScene.unity and press Play!", EditorStyles.helpBox);
    }

    private void RunCompleteSetup()
    {
        CreateAllScriptableObjects();
        CreateScenes();
        ConfigureBuildSettings();
        EditorUtility.DisplayDialog("Complete", "All setup complete! Open BootstrapScene.unity and press Play!", "OK");
    }

    private void CreateAllScriptableObjects()
    {
        CreateGameConfig();
        CreateMonsterData();
        CreateClassData();
        CreateFormData();
        CreateFragmentData();
        CreateShopItemData();
        CreateAchievementData();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CreateGameConfig()
    {
        var config = CreateScriptableObject<GameConfig>("Data/GameConfig");
        if (config != null)
        {
            config.enemyHpScaling = 0.1f;
            config.startingHp = 100;
            config.startingAttack = 10;
            config.startingDefense = 5;
            EditorUtility.SetDirty(config);
        }
    }

    private void CreateMonsterData()
    {
        CreateMonster("Slime", 50, 10, 5, false);
        CreateMonster("Goblin", 60, 12, 6, false);
        CreateMonster("Skeleton", 70, 14, 8, false);
        CreateMonster("Orc", 80, 16, 10, false);
        CreateMonster("Troll", 100, 20, 15, false);
        CreateMonster("Dragon", 200, 30, 20, true);
    }

    private void CreateMonster(string name, int hp, int atk, int def, bool isBoss)
    {
        var monster = CreateScriptableObject<MonsterData>($"Data/Monsters/{name}");
        if (monster != null)
        {
            monster.monsterId = name.ToLower();
            monster.monsterName = name;
            monster.baseHp = hp;
            monster.baseAttack = atk;
            monster.baseDefense = def;
            monster.isBoss = isBoss;
            monster.minGold = 10;
            monster.maxGold = 20;
            monster.fragmentDropChance = 0.3f;
            EditorUtility.SetDirty(monster);
        }
    }

    private void CreateClassData()
    {
        CreateClass("Warrior", "High HP and Defense", 150, 15, 12);
        CreateClass("Mage", "High Attack", 80, 25, 5);
        CreateClass("Rogue", "Balanced stats", 100, 18, 8);
    }

    private void CreateClass(string name, string desc, int hp, int atk, int def)
    {
        var cls = CreateScriptableObject<ClassData>($"Data/Classes/{name}");
        if (cls != null)
        {
            cls.classId = name.ToLower();
            cls.className = name;
            cls.quote = desc;
            cls.baseHp = hp;
            cls.baseAttack = atk;
            cls.baseDefense = def;
            EditorUtility.SetDirty(cls);
        }
    }

    private void CreateFormData()
    {
        CreateForm("Normal Form", "The basic form", 0, 0);
        CreateForm("Power Form", "Increased attack", 0, 5);
        CreateForm("Defense Form", "Increased defense", 5, 0);
    }

    private void CreateForm(string name, string desc, int defBonus, int atkBonus)
    {
        var form = CreateScriptableObject<FormData>($"Data/Forms/{name}");
        if (form != null)
        {
            form.formId = name.ToLower().Replace(" ", "_");
            form.formName = name;
            form.description = desc;
            form.defenseBonus = defBonus;
            form.attackBonus = atkBonus;
            EditorUtility.SetDirty(form);
        }
    }

    private void CreateFragmentData()
    {
        CreateFragment("Strength Shard", "+5 Attack", 5, 0, 0);
        CreateFragment("Defense Shard", "+5 Defense", 0, 5, 0);
        CreateFragment("Health Shard", "+20 HP", 0, 0, 20);
    }

    private void CreateFragment(string name, string desc, int atk, int def, int hp)
    {
        var frag = CreateScriptableObject<FragmentData>($"Data/Fragments/{name}");
        if (frag != null)
        {
            frag.fragmentId = name.ToLower().Replace(" ", "_");
            frag.fragmentName = name;
            frag.description = desc;
            frag.attackBonus = atk;
            frag.defenseBonus = def;
            frag.hpBonus = hp;
            EditorUtility.SetDirty(frag);
        }
    }

    private void CreateShopItemData()
    {
        CreateShopItem("Health Potion", "Restore 30 HP", 20, ShopItemCategory.Survival);
        CreateShopItem("Attack Boost", "+10 Attack", 30, ShopItemCategory.Growth);
        CreateShopItem("Defense Boost", "+10 Defense", 30, ShopItemCategory.Growth);
    }

    private void CreateShopItem(string name, string desc, int cost, ShopItemCategory category)
    {
        var item = CreateScriptableObject<ShopItemData>($"Data/ShopItems/{name}");
        if (item != null)
        {
            item.itemId = name.ToLower().Replace(" ", "_");
            item.itemName = name;
            item.description = desc;
            item.basePrice = cost;
            item.category = category;
            EditorUtility.SetDirty(item);
        }
    }

    private void CreateAchievementData()
    {
        // Full achievement list from AchievementManager
        CreateAchievement("first_kill", "初猎", "击杀第一只怪物", "⚔", AchievementType.Kill, 1, "atk", 1, "攻击+1");
        CreateAchievement("first_possess", "寄生觉醒", "首次成功附身", "★", AchievementType.Possession, 1, "possessBonus", 0.05f, "附身成功率+5%");
        CreateAchievement("floor10", "深入", "到达第10层", "◄", AchievementType.Floor, 10, null, 0, null);
        CreateAchievement("floor25", "中途觉醒", "到达第25层", "⚡", AchievementType.Floor, 25, "maxHp", 20, "最大生命+20");
        CreateAchievement("floor50", "登顶", "到达第50层", "♛", AchievementType.Floor, 50, "atk", 3, "攻击+3");
        CreateAchievement("possess5", "收集者", "附身5种不同生物", "♦", AchievementType.PossessionCount, 5, null, 0, null);
        CreateAchievement("possess10", "百变怪", "附身10种不同生物", "↔", AchievementType.PossessionCount, 10, "def", 2, "防御+2");
        CreateAchievement("no_death", "不死传说", "不死亡通关25层", "☠", AchievementType.NoDeath, 25, "evoPoints", 100, "起始进化点+100");
        CreateAchievement("titan_end", "不可移动的永恒", "达成泰坦结局", "■", AchievementType.Ending, 1, null, 0, null);
        CreateAchievement("ghost_end", "不存在的自由", "达成幽灵结局", "◎", AchievementType.Ending, 1, null, 0, null);
        CreateAchievement("swarm_end", "增殖的混沌", "达成虫群结局", "†", AchievementType.Ending, 1, null, 0, null);
        CreateAchievement("blood_end", "永恒的饥渴", "达成血族结局", "♥", AchievementType.Ending, 1, null, 0, null);
        CreateAchievement("mech_end", "超越肉体", "达成机甲结局", "⚙", AchievementType.Ending, 1, null, 0, null);
        CreateAchievement("hidden_end", "递归的观察者", "达成隐藏结局", "◈", AchievementType.Ending, 1, "pollution", -10, "起始污染-10");
        CreateAchievement("defend10", "铁壁", "单场战斗防御10次", "◆", AchievementType.Defend, 10, null, 0, null);
        CreateAchievement("switch3", "形态大师", "单场战斗切换形态3次", "↩", AchievementType.FormSwitch, 3, null, 0, null);
        CreateAchievement("pollution0", "纯净", "通关25层时污染为0", "✨", AchievementType.PureRun, 25, null, 0, null);
    }

    private void CreateAchievement(string id, string name, string desc, string iconStr, AchievementType type, int target, string bonusStat, float bonusValue, string bonusDesc)
    {
        var ach = CreateScriptableObject<AchievementData>($"Data/Achievements/{name}");
        if (ach != null)
        {
            ach.achievementId = id;
            ach.name = name;
            ach.description = desc;
            ach.icon = iconStr;
            ach.target = target;
            ach.type = type;
            
            if (bonusStat != null)
            {
                AchievementBonus bonus = new AchievementBonus();
                bonus.stat = bonusStat;
                bonus.value = bonusValue;
                bonus.description = bonusDesc;
                ach.bonus = bonus;
            }
            
            EditorUtility.SetDirty(ach);
        }
    }

    private T CreateScriptableObject<T>(string path) where T : ScriptableObject
    {
        string fullPath = $"Assets/Resources/{path}.asset";
        string directory = Path.GetDirectoryName(fullPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(fullPath))
        {
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        T obj = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(obj, fullPath);
        return obj;
    }

    private void CreateScenes()
    {
        CreateBootstrapScene();
        CreateGameScene();
        EditorSceneManager.SaveOpenScenes();
    }

    private void CreateBootstrapScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        var cameraObj = new GameObject("Main Camera");
        var camera = cameraObj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        var eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        var bootstrapObj = new GameObject("Bootstrap");
        var bootstrap = bootstrapObj.AddComponent<Bootstrap>();
        bootstrap.enableDebugMode = true;
        bootstrap.autoStartGame = false;

        var testObj = new GameObject("GameTestHarness");
        var testHarness = testObj.AddComponent<GameTestHarness>();
        testHarness.autoStartTest = false;
        testHarness.showDebugUI = true;

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/BootstrapScene.unity");
        Debug.Log("Created BootstrapScene");
    }

    private void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        var cameraObj = new GameObject("Main Camera");
        var camera = cameraObj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        var eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        var testObj = new GameObject("GameTestHarness");
        var testHarness = testObj.AddComponent<GameTestHarness>();
        testHarness.autoStartTest = true;
        testHarness.showDebugUI = true;

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
        Debug.Log("Created GameScene");
    }

    private void ConfigureBuildSettings()
    {
        var scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/BootstrapScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build settings configured");
    }
}
