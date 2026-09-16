using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : SingletonBase<SceneLoader>
{

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public UnityEngine.UI.Slider loadingSlider;

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (loadingSlider != null)
            {
                loadingSlider.value = progress;
            }

            yield return null;
        }

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }

    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }
}
