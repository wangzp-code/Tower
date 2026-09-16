using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public int tileSize = 40;

    [Header("References")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    private Vector3 targetPosition;
    private bool isMoving;
    private TileMapGenerator tileMap;

    private void Awake()
    {
        targetPosition = transform.position;
    }

    private void Start()
    {
        tileMap = FindObjectOfType<TileMapGenerator>();
    }

    private void Update()
    {
        if (GameManager.Instance.IsPlaying())
        {
            HandleInput();
            HandleMovement();
        }
    }

    private void HandleInput()
    {
        if (isMoving) return;

        int moveX = 0;
        int moveY = 0;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) moveY = 1;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) moveY = -1;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) moveX = -1;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) moveX = 1;

        if (moveX != 0 || moveY != 0)
        {
            TryMove(moveX, moveY);
        }
    }

    private void TryMove(int dx, int dy)
    {
        Vector3 newPosition = targetPosition + new Vector3(dx * tileSize, dy * tileSize, 0f);
        if (tileMap != null && tileMap.IsWalkable(newPosition))
        {
            targetPosition = newPosition;
            isMoving = true;
            EventBus.Emit(EventTypes.PlayerStatsChanged);
            CheckCollisions();
        }
    }

    private void CheckCollisions()
    {
        var collider = Physics2D.OverlapCircle(transform.position, tileSize / 3f);
        if (collider != null)
        {
            var monster = collider.GetComponent<MonsterBase>();
            if (monster != null)
            {
                GameManager.Instance.StartCombat(monster);
            }
        }
    }

    private void HandleMovement()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }
}
