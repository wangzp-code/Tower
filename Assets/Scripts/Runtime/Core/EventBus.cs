using UnityEngine;
using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<string, Delegate> events = new Dictionary<string, Delegate>();
    private static readonly List<string> _dirtyEvents = new List<string>();

    #region Event Registration
    public static void Register<T>(string eventName, Action<T> callback)
    {
        if (callback == null) return;

        if (!events.ContainsKey(eventName))
        {
            events[eventName] = callback;
        }
        else
        {
            var existing = events[eventName];
            if (existing is Action<T> existingAction)
            {
                events[eventName] = existingAction + callback;
            }
            else
            {
                Debug.LogError($"[EventBus] Type mismatch for event '{eventName}': " +
                    $"expected Action<{typeof(T).Name}>, got {existing.GetType().Name}");
            }
        }
    }

    public static void Unregister<T>(string eventName, Action<T> callback)
    {
        if (callback == null || !events.ContainsKey(eventName)) return;

        var existing = events[eventName];
        if (existing is Action<T> existingAction)
        {
            existingAction -= callback;
            if (existingAction == null)
            {
                events.Remove(eventName);
            }
            else
            {
                events[eventName] = existingAction;
            }
        }
    }

    public static void Register(string eventName, Action callback)
    {
        if (callback == null) return;

        if (!events.ContainsKey(eventName))
        {
            events[eventName] = callback;
        }
        else
        {
            var existing = events[eventName];
            if (existing is Action existingAction)
            {
                events[eventName] = existingAction + callback;
            }
            else
            {
                Debug.LogError($"[EventBus] Type mismatch for event '{eventName}': " +
                    $"expected Action, got {existing.GetType().Name}");
            }
        }
    }

    public static void Unregister(string eventName, Action callback)
    {
        if (callback == null || !events.ContainsKey(eventName)) return;

        var existing = events[eventName];
        if (existing is Action existingAction)
        {
            existingAction -= callback;
            if (existingAction == null)
            {
                events.Remove(eventName);
            }
            else
            {
                events[eventName] = existingAction;
            }
        }
    }

    public static void Register<T1, T2>(string eventName, Action<T1, T2> callback)
    {
        if (callback == null) return;

        if (!events.ContainsKey(eventName))
        {
            events[eventName] = callback;
        }
        else
        {
            var existing = events[eventName];
            if (existing is Action<T1, T2> existingAction)
            {
                events[eventName] = existingAction + callback;
            }
            else
            {
                Debug.LogError($"[EventBus] Type mismatch for event '{eventName}': " +
                    $"expected Action<{typeof(T1).Name},{typeof(T2).Name}>, got {existing.GetType().Name}");
            }
        }
    }

    public static void Unregister<T1, T2>(string eventName, Action<T1, T2> callback)
    {
        if (callback == null || !events.ContainsKey(eventName)) return;

        var existing = events[eventName];
        if (existing is Action<T1, T2> existingAction)
        {
            existingAction -= callback;
            if (existingAction == null)
            {
                events.Remove(eventName);
            }
            else
            {
                events[eventName] = existingAction;
            }
        }
    }

    public static void Register<T1, T2, T3>(string eventName, Action<T1, T2, T3> callback)
    {
        if (callback == null) return;

        if (!events.ContainsKey(eventName))
        {
            events[eventName] = callback;
        }
        else
        {
            var existing = events[eventName];
            if (existing is Action<T1, T2, T3> existingAction)
            {
                events[eventName] = existingAction + callback;
            }
            else
            {
                Debug.LogError($"[EventBus] Type mismatch for event '{eventName}': " +
                    $"expected Action<{typeof(T1).Name},{typeof(T2).Name},{typeof(T3).Name}>, got {existing.GetType().Name}");
            }
        }
    }

    public static void Unregister<T1, T2, T3>(string eventName, Action<T1, T2, T3> callback)
    {
        if (callback == null || !events.ContainsKey(eventName)) return;

        var existing = events[eventName];
        if (existing is Action<T1, T2, T3> existingAction)
        {
            existingAction -= callback;
            if (existingAction == null)
            {
                events.Remove(eventName);
            }
            else
            {
                events[eventName] = existingAction;
            }
        }
    }
    #endregion

    #region Event Emission
    public static void Emit(string eventName)
    {
        if (!events.TryGetValue(eventName, out Delegate evt)) return;

        var action = evt as Action;
        if (action == null)
        {
            LogTypeMismatch(eventName, typeof(Action), evt.GetType());
            return;
        }

        CleanupDeadDelegates(eventName, action);

        foreach (var d in action.GetInvocationList())
        {
            try
            {
                if (d.Target is UnityEngine.Object obj && obj == null) continue;
                ((Action)d).Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventBus] Exception in event '{eventName}': {e.Message}");
            }
        }
    }

    public static void Emit<T>(string eventName, T param)
    {
        if (!events.TryGetValue(eventName, out Delegate evt)) return;

        var action = evt as Action<T>;
        if (action == null)
        {
            LogTypeMismatch(eventName, typeof(Action<T>), evt.GetType());
            return;
        }

        CleanupDeadDelegates(eventName, action);

        foreach (var d in action.GetInvocationList())
        {
            try
            {
                if (d.Target is UnityEngine.Object obj && obj == null) continue;
                ((Action<T>)d).Invoke(param);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventBus] Exception in event '{eventName}': {e.Message}");
            }
        }
    }

    public static void Emit<T1, T2>(string eventName, T1 param1, T2 param2)
    {
        if (!events.TryGetValue(eventName, out Delegate evt)) return;

        var action = evt as Action<T1, T2>;
        if (action == null)
        {
            LogTypeMismatch(eventName, typeof(Action<T1, T2>), evt.GetType());
            return;
        }

        CleanupDeadDelegates(eventName, action);

        foreach (var d in action.GetInvocationList())
        {
            try
            {
                if (d.Target is UnityEngine.Object obj && obj == null) continue;
                ((Action<T1, T2>)d).Invoke(param1, param2);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventBus] Exception in event '{eventName}': {e.Message}");
            }
        }
    }

    public static void Emit<T1, T2, T3>(string eventName, T1 param1, T2 param2, T3 param3)
    {
        if (!events.TryGetValue(eventName, out Delegate evt)) return;

        var action = evt as Action<T1, T2, T3>;
        if (action == null)
        {
            LogTypeMismatch(eventName, typeof(Action<T1, T2, T3>), evt.GetType());
            return;
        }

        CleanupDeadDelegates(eventName, action);

        foreach (var d in action.GetInvocationList())
        {
            try
            {
                if (d.Target is UnityEngine.Object obj && obj == null) continue;
                ((Action<T1, T2, T3>)d).Invoke(param1, param2, param3);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventBus] Exception in event '{eventName}': {e.Message}");
            }
        }
    }

    private static void CleanupDeadDelegates(string eventName, Delegate del)
    {
        var invocationList = del.GetInvocationList();
        bool hasDeadRefs = false;

        foreach (var d in invocationList)
        {
            if (d.Target is UnityEngine.Object obj && obj == null)
            {
                hasDeadRefs = true;
                break;
            }
        }

        if (!hasDeadRefs) return;

        _dirtyEvents.Add(eventName);
    }

    public static void CleanupAllDeadReferences()
    {
        for (int i = _dirtyEvents.Count - 1; i >= 0; i--)
        {
            string eventName = _dirtyEvents[i];
            if (!events.TryGetValue(eventName, out var del)) continue;

            var cleanList = new List<Delegate>();
            foreach (var d in del.GetInvocationList())
            {
                if (d.Target is UnityEngine.Object obj && obj == null) continue;
                cleanList.Add(d);
            }

            if (cleanList.Count == 0)
            {
                events.Remove(eventName);
            }
            else
            {
                var rebuilt = cleanList[0];
                for (int j = 1; j < cleanList.Count; j++)
                {
                    rebuilt = Delegate.Combine(rebuilt, cleanList[j]);
                }
                events[eventName] = rebuilt;
            }
            _dirtyEvents.RemoveAt(i);
        }
    }

    private static void LogTypeMismatch(string eventName, Type expected, Type actual)
    {
        Debug.LogError($"[EventBus] Type mismatch for event '{eventName}': " +
            $"expected {expected.Name}, got {actual.Name}. Event not emitted.");
    }
    #endregion

    #region Event Management
    public static void ClearAll()
    {
        events.Clear();
        _dirtyEvents.Clear();
    }

    public static void ClearEvent(string eventName)
    {
        if (events.ContainsKey(eventName))
        {
            events.Remove(eventName);
        }
    }

    public static bool HasEvent(string eventName)
    {
        return events.ContainsKey(eventName);
    }

    public static int GetSubscriberCount(string eventName)
    {
        if (!events.TryGetValue(eventName, out var del)) return 0;
        return del.GetInvocationList().Length;
    }
    #endregion
}

public static class EventTypes
{
    public const string GameStateChanged = "GameStateChanged";
    public const string CombatVictory = "CombatVictory";
    public const string CombatDefeat = "CombatDefeat";
    public const string FloorChanged = "FloorChanged";
    public const string PollutionChanged = "PollutionChanged";
    public const string FormSwitched = "FormSwitched";
    public const string FragmentEquipped = "FragmentEquipped";
    public const string AchievementUnlocked = "AchievementUnlocked";
    public const string CombatStarted = "CombatStarted";
    public const string PlayerDamaged = "PlayerDamaged";
    public const string CombatLogUpdated = "CombatLogUpdated";
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

    public const string RunStart = "RunStart";
    public const string RunEnd = "RunEnd";
    public const string RunVictory = "RunVictory";
    public const string RunDefeat = "RunDefeat";

    public const string CombatEnd = "CombatEnd";
    public const string ShowHint = "ShowHint";
    public const string ShowButtonGuide = "ShowButtonGuide";
    public const string TurnStart = "TurnStart";
    public const string TurnEnd = "TurnEnd";

    public const string PlayerAttack = "PlayerAttack";
    public const string PlayerDefend = "PlayerDefend";
    public const string PlayerUseSkill = "PlayerUseSkill";
    public const string PlayerHeal = "PlayerHeal";

    public const string PossessStart = "PossessStart";
    public const string PossessEnd = "PossessEnd";
    public const string HostDeath = "HostDeath";
    public const string HostDiscover = "HostDiscover";

    public const string FormLost = "FormLost";
    public const string FormGain = "FormGain";
    public const string FormAdded = "FormAdded";
    public const string FormSlotFull = "FormSlotFull";
    public const string FormReplaced = "FormReplaced";

    public const string LegacySelection = "LegacySelection";
    public const string LegacyAdded = "LegacyAdded";
    public const string LegacyReplaced = "LegacyReplaced";
    public const string LegacyRemoved = "LegacyRemoved";
    public const string LegacyActivated = "LegacyActivated";
    public const string LegacyUnlocked = "LegacyUnlocked";

    public const string PollutionTierUp = "PollutionTierUp";
    public const string PollutionTierDown = "PollutionTierDown";
    public const string PollutionMax = "PollutionMax";

    public const string EndingReached = "EndingReached";

    public const string MonsterKill = "MonsterKill";
    public const string EliteKill = "EliteKill";
    public const string BossKill = "BossKill";
    public const string FloorClear = "FloorClear";

    public const string BossPhaseChange = "BossPhaseChange";
    public const string BossDefeat = "BossDefeat";
    public const string BossEnrage = "BossEnrage";

    public const string FloorEventStart = "FloorEventStart";
    public const string FloorEventComplete = "FloorEventComplete";
    public const string FloorEventFailed = "FloorEventFailed";

    public const string ModuleUnlocked = "ModuleUnlocked";
}