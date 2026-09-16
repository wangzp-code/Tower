using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action onTrigger;
    public Action onStart;
    public Action onEnd;
    public float firstDelay = 0.3f;
    public float repeatInterval = 0.12f;

    bool _held;
    float _timer;
    bool _firstFired;

    public void OnPointerDown(PointerEventData eventData)
    {
        var btn = GetComponent<Button>();
        if (btn != null && !btn.interactable) return;
        
        _held = true;
        _timer = 0f;
        _firstFired = false;
        onTrigger?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _held = false;
        onEnd?.Invoke();
    }

    void Update()
    {
        if (!_held) return;

        _timer += Time.deltaTime;

        if (!_firstFired)
        {
            if (_timer >= firstDelay)
            {
                _firstFired = true;
                _timer = 0f;
                onStart?.Invoke();
                onTrigger?.Invoke();
            }
        }
        else
        {
            if (_timer >= repeatInterval)
            {
                _timer = 0f;
                onTrigger?.Invoke();
            }
        }
    }

    void OnDisable()
    {
        _held = false;
    }
}
