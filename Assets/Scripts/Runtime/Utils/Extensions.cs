using UnityEngine;

public static class Extensions
{
    #region Vector Extensions
    public static Vector3 GetGridPosition(this Vector3 worldPos, int tileSize)
    {
        return new Vector3(
            Mathf.FloorToInt(worldPos.x / tileSize) * tileSize,
            Mathf.FloorToInt(worldPos.y / tileSize) * tileSize,
            0
        );
    }

    public static Vector3 SnapToGrid(this Vector3 pos, int gridSize)
    {
        pos.x = Mathf.RoundToInt(pos.x / gridSize) * gridSize;
        pos.y = Mathf.RoundToInt(pos.y / gridSize) * gridSize;
        return pos;
    }
    #endregion

    #region Color Extensions
    public static Color WithAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    public static string ToHex(this Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }
    #endregion

    #region Array Extensions
    public static T RandomElement<T>(this T[] array)
    {
        if (array.Length == 0) return default(T);
        return array[Random.Range(0, array.Length)];
    }

    public static T RandomElement<T>(this System.Collections.Generic.List<T> list)
    {
        if (list.Count == 0) return default(T);
        return list[Random.Range(0, list.Count)];
    }
    #endregion

    #region Float Extensions
    public static bool Approximately(this float a, float b, float threshold = 0.001f)
    {
        return Mathf.Abs(a - b) < threshold;
    }
    #endregion
}
