using UnityEngine;

[System.Obsolete("Use Bootstrap instead. This class is kept only for scene compatibility.")]
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private bool _initializeOnStart = true;

    void Start()
    {
        if (!_initializeOnStart) return;

        if (Bootstrap.Instance != null)
        {

            return;
        }


        var go = new GameObject("Bootstrap");
        go.AddComponent<Bootstrap>();
    }
}
