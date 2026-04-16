using UnityEngine;
using System.Collections;

public class GoldPickup : MonoBehaviour
{
    [SerializeField] private int goldAmount = 3;

    [Header("Magnet")]
    [SerializeField] private float magnetRange = 6f;
    [SerializeField] private float minSpeed = 6f;
    [SerializeField] private float maxSpeed = 35f;

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

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(goldAmount);
        }

        Destroy(gameObject);
    }

    public void SetGoldAmount(int amount)
    {
        goldAmount = amount;
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