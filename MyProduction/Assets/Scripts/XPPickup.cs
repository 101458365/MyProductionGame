using UnityEngine;

public class XPPickup : MonoBehaviour
{
    [SerializeField] private int xpAmount = 5;
    [SerializeField] private float magnetRange = 5f;
    [SerializeField] private float magnetSpeed = 15f;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Magnetic collection
        if (distanceToPlayer <= magnetRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerLevel playerLevel = collision.GetComponent<PlayerLevel>();
            if (playerLevel != null)
            {
                playerLevel.AddXP(xpAmount);
                Destroy(gameObject);
            }
        }
    }

    public void SetXPAmount(int amount)
    {
        xpAmount = amount;
    }
}