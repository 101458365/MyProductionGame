using UnityEngine;

public class MagnetPickup : MonoBehaviour
{
    [SerializeField] private float magnetRange = 5f;
    [SerializeField] private float magnetSpeed = 12f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    private Transform player;
    private Vector3 startPos;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPos = transform.position;
        Destroy(gameObject, 30f);
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= magnetRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                magnetSpeed * Time.deltaTime);
        }
        else
        {
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        XPPickup.ActivateMagnet(12f, 2.5f, 2.5f);
        GoldPickup.ActivateMagnet(12f, 2.5f, 2.5f);

        Destroy(gameObject);
    }
}