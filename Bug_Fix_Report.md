# Bug Fix Report - Parasite Tower

## Date
2026-06-02

## Summary of Issues Found and Fixed

### 1. Critical: Bootstrap.cs - EventBus.Instance Error
**File**: `Assets/Scripts/Runtime/Core/Bootstrap.cs`

**Issue**: 
- Attempting to create EventBus as a MonoBehaviour component
- EventBus is a static class and should not be instantiated
- Trying to access `EventBus.Instance` which doesn't exist

**Fix Applied**:
```csharp
// Before
private void InitializeCoreSystems()
{
    EnsureManager<GameManager>();
    EnsureManager<EventBus>();  // ❌ Wrong - EventBus is static
    EnsureManager<ConfigManager>();
    EnsureManager<SaveSystem>();
    EnsureManager<SceneLoader>();
}

// After
private void InitializeCoreSystems()
{
    EnsureManager<GameManager>();
    // EventBus is static, no need to create as component ✓
    EnsureManager<ConfigManager>();
    EnsureManager<SaveSystem>();
    EnsureManager<SceneLoader>();
}

// Also updated LogSystemStatus
Debug.Log($"EventBus: OK (static class)");
```

---

### 2. Critical: CombatManager - Missing Singleton Pattern
**File**: `Assets/Scripts/Runtime/Combat/CombatManager.cs`

**Issue**:
- No singleton implementation
- Bootstrap trying to access `CombatManager.Instance` which didn't exist

**Fix Applied**:
```csharp
// Added singleton pattern
private static CombatManager _instance;
public static CombatManager Instance
{
    get
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<CombatManager>();
            if (_instance == null)
            {
                var go = new GameObject("CombatManager");
                _instance = go.AddComponent<CombatManager>();
                DontDestroyOnLoad(go);
            }
        }
        return _instance;
    }
}

// Added Awake method
private void Awake()
{
    if (_instance != null && _instance != this)
    {
        Destroy(gameObject);
        return;
    }
    _instance = this;
    DontDestroyOnLoad(gameObject);
}
```

---

### 3. Missing: Complete EventTypes Definitions
**File**: `Assets/Scripts/Runtime/Core/EventBus.cs`

**Issue**:
- Many event types missing that are used in other systems

**Fix Applied**:
Added missing event types:
```csharp
public const string ShopGenerated = "ShopGenerated";
public const string ItemPurchased = "ItemPurchased";
public const string RewardClaimed = "RewardClaimed";
public const string FormUnlocked = "FormUnlocked";
public const string EventTriggered = "EventTriggered";
public const string EventCompleted = "EventCompleted";
public const string FloorEntered = "FloorEntered";
public const string FloorExited = "FloorExited";
public const string PlayerStatsChanged = "PlayerStatsChanged";
public const string GameSaved = "GameSaved";
public const string GameLoaded = "GameLoaded";
```

---

### 4. Important: Unity Project Version Configuration
**Files Created**:
- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/TagManager.asset`
- `ProjectSettings/QualitySettings.asset`
- `ProjectSettings/InputManager.asset`

**Configuration**:
```
m_EditorVersion: 2022.3.62f2c1
```

---

## How to Handle the "Corrupted Library Detected" Error

### What to Do:
1. **Click "Rebuild Library"** in the Unity popup
   - This is completely normal when project settings are modified externally
   - Unity will rebuild its internal cache

2. **If that doesn't work**:
   - Close Unity completely
   - Delete the `Library` folder manually
   - Reopen the project in Unity
   - Unity will regenerate the Library folder

### Why This Happened:
- We created and modified ProjectSettings files directly
- Unity's internal cache became out of sync
- Rebuilding the Library is the standard solution

---

## Files Modified/Created

### Modified Files:
1. `Assets/Scripts/Runtime/Core/Bootstrap.cs` - Fixed EventBus handling
2. `Assets/Scripts/Runtime/Combat/CombatManager.cs` - Added singleton pattern
3. `Assets/Scripts/Runtime/Core/EventBus.cs` - Added complete event types
4. `Assets/Scripts/Runtime/Utils/Constants.cs` (previously modified)

### Created Files:
1. `ProjectSettings/ProjectVersion.txt` - Unity 2022.3.62f2c1
2. `ProjectSettings/TagManager.asset` - Tags & layers config
3. `ProjectSettings/QualitySettings.asset` - Quality settings
4. `ProjectSettings/InputManager.asset` - Input configuration
5. `UNITY_VERSION.md` - Version info documentation
6. `SceneSetupGuide.md` - Scene setup guide
7. `BUG_FIX_REPORT.md` - This file

---

## Remaining Tasks (to be done in Unity Editor)

### 1. Add Singleton Patterns to Other Managers (Recommended)
The following Managers should also have singleton patterns for consistency:
- TraitManager
- CurseManager
- AchievementManager
- TutorialManager
- NegotiateManager
- ShopManager
- RewardManager
- FloorManager
- FormManager
- FragmentManager
- ClassManager
- PollutionSystem
- UIManager
- AudioManager
- VFXManager

**Pattern to use**:
```csharp
private static [ManagerName] _instance;
public static [ManagerName] Instance
{
    get
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<[ManagerName]>();
            if (_instance == null)
            {
                var go = new GameObject("[ManagerName]");
                _instance = go.AddComponent<[ManagerName]>();
                DontDestroyOnLoad(go);
            }
        }
        return _instance;
    }
}

private void Awake()
{
    if (_instance != null && _instance != this)
    {
        Destroy(gameObject);
        return;
    }
    _instance = this;
    DontDestroyOnLoad(gameObject);
}
```

---

## Testing Steps (After Rebuilding Library)

1. **Open Project in Unity 2022.3.62f2c1**
   - Click "Rebuild Library" if prompted

2. **Load BootstrapScene**
   - Open `Assets/Scenes/BootstrapScene.unity`

3. **Enter Play Mode**
   - Check Console for initialization logs
   - Verify no errors appear

4. **Verify Systems Initialized**
   - Look for "=== Starting Bootstrap ===" log
   - Look for "=== All Systems Initialized ===" log
   - Look for "=== System Status Report ==="

---

## Known Issues & Workarounds

### Issue: Some Managers Still Missing Singleton
- **Status**: Needs attention
- **Priority**: Medium
- **Workaround**: Bootstrap should still create them dynamically

### Issue: Scenes Not in Build Settings
- **Status**: Needs manual configuration
- **Priority**: Low
- **Workaround**: Add scenes to Build Settings in Unity Editor

---

## Verification Checklist

- [ ] Click "Rebuild Library" in Unity
- [ ] Project opens without errors
- [ ] Console shows Bootstrap initialization logs
- [ ] No red errors in Console
- [ ] GameManager.Instance accessible
- [ ] CombatManager.Instance accessible
- [ ] EventBus.Emit works correctly
- [ ] All Manager singletons accessible

---

## Notes

- The most critical bugs have been fixed
- Bootstrap should now initialize all systems correctly
- CombatManager now has proper singleton pattern
- EventBus is properly handled as static class
- All required event types are defined

Next step: Open project in Unity, rebuild Library, and test!
