using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.5f;

    [Header("Safety")]
    // Enemies beyond this distance from the world origin get destroyed
    // Prevents the NaN velocity and invalid AABB errors
    [SerializeField] private float maxDistanceFromOrigin = 200f;

    private Transform player;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start()
    {
        if (DifficultyManager.Instance != null)
            moveSpeed *= DifficultyManager.Instance.SpeedMultiplier;
    }

    private void FixedUpdate()
    {
        // Despawn if too far from origin — prevents floating point breakdown
        if (transform.position.magnitude > maxDistanceFromOrigin)
        {
            Debug.LogWarning($"[EnemyMovement] Enemy too far from origin ({transform.position.magnitude:F0} units), despawning.");
            Destroy(gameObject);
            return;
        }

        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        // Guard against NaN — can happen if enemy and player are at the exact same position
        if (float.IsNaN(direction.x) || float.IsNaN(direction.y))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopDistance)
            rb.linearVelocity = direction * moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }
}