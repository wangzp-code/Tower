using UnityEngine;

public class GameModeManager : SingletonBase<GameModeManager>
{
    private void Awake()
    {
        base.Awake();
    }

    public void StartGameMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Classic:
                StartClassicMode();
                break;
            case GameMode.Short:
                StartShortMode();
                break;
            case GameMode.Expedition:
                StartExpeditionMode();
                break;
            case GameMode.Daily:
                StartDailyChallenge();
                break;
            case GameMode.Weekly:
                StartWeeklyChallenge();
                break;
        }
    }

    private void StartClassicMode()
    {
        GameManager.Instance.CurrentMode = GameMode.Classic;
        GameManager.Instance.MaxFloor = ConfigManager.Instance.GetGameConfig().classicModeFloorCount;
        GameManager.Instance.StartNewGame("Classic");
    }

    private void StartShortMode()
    {
        GameManager.Instance.CurrentMode = GameMode.Short;
        GameManager.Instance.MaxFloor = ConfigManager.Instance.GetGameConfig().shortModeFloorCount;
        GameManager.Instance.StartNewGame("Short");
    }

    private void StartExpeditionMode()
    {
        GameManager.Instance.CurrentMode = GameMode.Expedition;
        GameManager.Instance.MaxFloor = ConfigManager.Instance.GetGameConfig().expeditionModeFloorCount;
        GameManager.Instance.StartNewGame("Expedition");
    }

    private void StartDailyChallenge()
    {
        GameManager.Instance.CurrentMode = GameMode.Daily;
        GameManager.Instance.StartNewGame("Daily");
    }

    private void StartWeeklyChallenge()
    {
        GameManager.Instance.CurrentMode = GameMode.Weekly;
        GameManager.Instance.StartNewGame("Weekly");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
