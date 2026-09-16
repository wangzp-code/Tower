using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GestureInputManager : MonoBehaviour
{
    public static GestureInputManager Instance { get; private set; }

    [Header("Gesture Settings")]
    [Tooltip("Minimum swipe distance in pixels")]
    public float minSwipeDistance = 50f;
    
    [Tooltip("Maximum time for a swipe gesture")]
    public float maxSwipeTime = 0.5f;
    
    [Tooltip("Minimum hold time for long press")]
    public float longPressDuration = 0.5f;
    
    [Tooltip("Time for pre-feedback before action")]
    public float preFeedbackDelay = 0.1f;
    
    [Tooltip("Edge swipe threshold from screen edge")]
    public float edgeSwipeThreshold = 50f;

    [Header("Vibration Settings")]
    public bool enableVibration = true;
    public float lightVibrationDuration = 0.05f;
    public float mediumVibrationDuration = 0.1f;
    public float heavyVibrationDuration = 0.2f;

    public event System.Action OnSingleTap;
    public event System.Action<Vector2> OnSwipeLeft;
    public event System.Action<Vector2> OnSwipeRight;
    public event System.Action<Vector2> OnSwipeUp;
    public event System.Action<Vector2> OnSwipeDown;
    public event System.Action<Vector2> OnLongPress;
    public event System.Action<float> OnPinch;
    public event System.Action OnDoubleTap;
    public event System.Action OnEdgeSwipe;

    private List<Touch> activeTouches = new List<Touch>();
    private Dictionary<int, TouchData> touchDataDictionary = new Dictionary<int, TouchData>();
    
    private Vector2 lastTapPosition;
    private float lastTapTime;
    private float doubleTapThreshold = 0.3f;
    
    private bool isProcessingGesture = false;
    private bool isPreFeedbackActive = false;
    private float preFeedbackStartTime;
    private System.Action pendingAction;

    private struct TouchData
    {
        public Vector2 startPosition;
        public Vector2 currentPosition;
        public Vector2 delta;
        public float startTime;
        public bool isLongPress;
        public bool isProcessed;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        #if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
        #else
        HandleTouchInput();
        #endif
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int touchId = 0;
            touchDataDictionary[touchId] = new TouchData
            {
                startPosition = Input.mousePosition,
                currentPosition = Input.mousePosition,
                startTime = Time.time,
                isLongPress = false,
                isProcessed = false
            };
        }

        if (Input.GetMouseButton(0))
        {
            int touchId = 0;
            if (touchDataDictionary.TryGetValue(touchId, out TouchData data))
            {
                data.currentPosition = Input.mousePosition;
                data.delta = data.currentPosition - data.startPosition;
                touchDataDictionary[touchId] = data;

                CheckLongPress(data, touchId);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            int touchId = 0;
            if (touchDataDictionary.TryGetValue(touchId, out TouchData data))
            {
                if (!data.isLongPress)
                {
                    ProcessTapOrSwipe(data);
                }
                touchDataDictionary.Remove(touchId);
            }
        }
    }

    private void HandleTouchInput()
    {
        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchDataDictionary[touch.fingerId] = new TouchData
                    {
                        startPosition = touch.position,
                        currentPosition = touch.position,
                        startTime = Time.time,
                        isLongPress = false,
                        isProcessed = false
                    };
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (touchDataDictionary.TryGetValue(touch.fingerId, out TouchData data))
                    {
                        data.currentPosition = touch.position;
                        data.delta = data.currentPosition - data.startPosition;
                        touchDataDictionary[touch.fingerId] = data;

                        CheckLongPress(data, touch.fingerId);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (touchDataDictionary.TryGetValue(touch.fingerId, out TouchData endData))
                    {
                        if (!endData.isLongPress)
                        {
                            ProcessTapOrSwipe(endData);
                        }
                        touchDataDictionary.Remove(touch.fingerId);
                    }
                    break;
            }
        }

        HandlePinchGesture();
    }

    private void CheckLongPress(TouchData data, int touchId)
    {
        if (data.isLongPress || data.isProcessed) return;

        float holdTime = Time.time - data.startTime;
        
        if (holdTime >= longPressDuration)
        {
            data.isLongPress = true;
            data.isProcessed = true;
            touchDataDictionary[touchId] = data;
            
            TriggerPreFeedback(() => 
            {
                OnLongPress?.Invoke(data.startPosition);
                Vibrate(mediumVibrationDuration);
            });
        }
        else if (holdTime >= preFeedbackDelay && !isPreFeedbackActive)
        {
            TriggerPreFeedback(null);
        }
    }

    private void ProcessTapOrSwipe(TouchData data)
    {
        if (data.isProcessed) return;

        float timeElapsed = Time.time - data.startTime;
        
        if (timeElapsed > maxSwipeTime)
        {
            return;
        }

        float distance = data.delta.magnitude;

        if (distance < minSwipeDistance)
        {
            CheckDoubleTap(data.startPosition);
        }
        else
        {
            Vector2 direction = data.delta.normalized;
            
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                if (direction.x > 0)
                {
                    TriggerPreFeedback(() => OnSwipeRight?.Invoke(data.delta));
                }
                else
                {
                    TriggerPreFeedback(() => OnSwipeLeft?.Invoke(data.delta));
                }
            }
            else
            {
                if (direction.y > 0)
                {
                    TriggerPreFeedback(() => OnSwipeUp?.Invoke(data.delta));
                }
                else
                {
                    TriggerPreFeedback(() => OnSwipeDown?.Invoke(data.delta));
                }
            }
            
            Vibrate(lightVibrationDuration);
        }

        data.isProcessed = true;
    }

    private void CheckDoubleTap(Vector2 position)
    {
        float timeSinceLastTap = Time.time - lastTapTime;
        float distanceFromLastTap = Vector2.Distance(position, lastTapPosition);

        if (timeSinceLastTap < doubleTapThreshold && distanceFromLastTap < 50f)
        {
            TriggerPreFeedback(() => OnDoubleTap?.Invoke());
            Vibrate(heavyVibrationDuration);
            lastTapTime = 0;
        }
        else
        {
            lastTapPosition = position;
            lastTapTime = Time.time;
            TriggerPreFeedback(() => OnSingleTap?.Invoke());
        }
    }

    private void HandlePinchGesture()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.touches[0];
            Touch touch1 = Input.touches[1];

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevDistance = Vector2.Distance(touch0PrevPos, touch1PrevPos);
            float currDistance = Vector2.Distance(touch0.position, touch1.position);

            float delta = currDistance - prevDistance;
            
            if (Mathf.Abs(delta) > 5f)
            {
                OnPinch?.Invoke(delta);
            }
        }
    }

    private void TriggerPreFeedback(System.Action action)
    {
        if (isPreFeedbackActive) return;

        isPreFeedbackActive = true;
        pendingAction = action;
        preFeedbackStartTime = Time.time;

        Invoke(nameof(ExecutePendingAction), preFeedbackDelay);
    }

    private void ExecutePendingAction()
    {
        if (pendingAction != null)
        {
            pendingAction.Invoke();
        }
        
        isPreFeedbackActive = false;
        pendingAction = null;
    }

    private void Vibrate(float duration)
    {
        if (!enableVibration) return;
        
        #if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
        #endif
    }

    public bool IsTouchNearEdge(Touch touch)
    {
        return touch.position.x < edgeSwipeThreshold ||
               touch.position.x > Screen.width - edgeSwipeThreshold ||
               touch.position.y < edgeSwipeThreshold ||
               touch.position.y > Screen.height - edgeSwipeThreshold;
    }

    public bool IsProcessingGesture()
    {
        return isProcessingGesture || isPreFeedbackActive;
    }

    public void CancelPendingAction()
    {
        CancelInvoke(nameof(ExecutePendingAction));
        isPreFeedbackActive = false;
        pendingAction = null;
    }
}