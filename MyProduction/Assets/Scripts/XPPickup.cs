using UnityEngine;
using System.Collections;

public class XPPickup : MonoBehaviour
{
    [SerializeField] private int xpAmount = 5;

    [Header("Magnet")]
    [SerializeField] private float magnetRange = 5f;
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 30f;

    private Transform player;

    private static float rangeMultiplier = 1f;
    private static float speedMultiplier = 1f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Destroy(gameObject, 30f);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        float effectiveRange = magnetRange * rangeMultiplier;

        if (distance <= effectiveRange)
        {
            float t = 1f - (distance / effectiveRange);
            float speed = Mathf.Lerp(minSpeed, maxSpeed, t) * speedMultiplier;

            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerLevel playerLevel = collision.GetComponent<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.AddXP(xpAmount);
        }

        Destroy(gameObject);
    }

    public void SetXPAmount(int amount)
    {
        xpAmount = amount;
    }

    public static void ActivateMagnet(float rangeMult, float speedMult, float duration)
    {
        rangeMultiplier = rangeMult;
        speedMultiplier = speedMult;
        InstanceRunner.Run(ResetMagnet(duration));
    }

    private static IEnumerator ResetMagnet(float duration)
    {
        yield return new WaitForSeconds(duration);
        rangeMultiplier = 1f;
        speedMultiplier = 1f;
    }
}