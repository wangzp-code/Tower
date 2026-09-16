public static class GameConstants
{
    #region Tags & Layers
    public const string PLAYER_TAG = "Player";
    public const string MONSTER_TAG = "Monster";
    public const string TILE_MAP_LAYER = "TileMap";
    #endregion

    #region Paths
    public const string SAVE_PATH = "Saves/";
    public const string DATA_PATH = "Data/";
    public const string PREFAB_PATH = "Prefabs/";
    #endregion

    #region Events
    public const string ON_PLAYER_MOVE = "OnPlayerMove";
    public const string ON_MONSTER_SPAWN = "OnMonsterSpawn";
    public const string ON_FLOOR_CHANGE = "OnFloorChange";
    public const string ON_COMBAT_START = "OnCombatStart";
    public const string ON_COMBAT_END = "OnCombatEnd";
    #endregion

    #region Animation
    public const float MOVE_DURATION = 0.2f;
    public const float ATTACK_DURATION = 0.3f;
    public const float DAMAGE_DURATION = 0.15f;
    #endregion

    #region Combat
    public const int MAX_COMBO = 99;
    public const float DEFAULT_CRIT = 0.15f;
    public const float DEFAULT_CRIT_MULT = 1.5f;
    #endregion
}
