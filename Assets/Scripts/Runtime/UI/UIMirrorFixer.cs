using UnityEngine;
using UnityEngine.UI;

// UIMirrorFixer:运行时保留类定义,避免 prefab/场景引用丢失脚本
// 但运行时不再执行 FindObjectsOfType 扫描,仅保留空实现
public class UIMirrorFixer : MonoBehaviour
{
    void Awake()
    {
        // 运行时不再执行任何修复逻辑
    }

    void Start()
    {
        // 运行时不再执行任何修复逻辑
    }

    void OnDisable()
    {
    }

    void OnDestroy()
    {
    }

#if UNITY_EDITOR
    [ContextMenu("Fix UI Mirroring Now")]
    public void FixNow()
    {
        FixAllCanvasScale();
        ResetCameraProjection();
    }

    void FixAllCanvasScale()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            RectTransform rt = canvas.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector3 scale = rt.localScale;
                bool fixedScale = false;

                if (scale.x < 0) { scale.x = Mathf.Abs(scale.x); fixedScale = true; }
                if (scale.y < 0) { scale.y = Mathf.Abs(scale.y); fixedScale = true; }
                if (scale.z < 0) { scale.z = Mathf.Abs(scale.z); fixedScale = true; }

                if (fixedScale) rt.localScale = scale;
            }
        }
    }

    void ResetCameraProjection()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            cam.ResetProjectionMatrix();
            cam.ResetWorldToCameraMatrix();
        }

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.ResetProjectionMatrix();
            mainCam.ResetWorldToCameraMatrix();
        }
    }
#endif
}
