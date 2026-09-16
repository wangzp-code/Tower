using UnityEngine;

public class ConfigInitializer : MonoBehaviour
{
    void Awake()
    {
        EnsureManagerExists<DataConfigManager>();
        EnsureManagerExists<ShopConfigManager>();
    }

    void EnsureManagerExists<T>() where T : MonoBehaviour
    {
        if (FindObjectOfType<T>() == null)
        {
            GameObject managerObj = new GameObject(typeof(T).Name);
            managerObj.AddComponent<T>();
            DontDestroyOnLoad(managerObj);
        }
    }
}
