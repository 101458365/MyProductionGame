using UnityEngine;
using System.Collections;

public class DismantleSlash : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float slashDistance = 4f;
    [SerializeField] private float slashSpeed = 6f;
    [SerializeField] private float lifetime = 1f;

    private GameObject target;
    private float damage;
    private Vector3 destination;
    private bool hasDealtDamage = false;
    private float timer = 0f;
    private Vector3 originalScale;

    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        originalScale = transform.localScale;
    }

    // spawnerPosition = player position, used to calculate the slash angle
    public void Initialise(GameObject slashTarget, float slashDamage, Vector3 spawnerPosition)
    {
        target = slashTarget;
        damage = slashDamage;

        if (target == null)
        {
            Destroy(gameObject, lifetime);
            return;
        }

        // Calculate direction BEFORE repositioning the slash
        // Using player position avoids the zero-vector problem from spawning at the enemy
        Vector3 toEnemy = (target.transform.position - spawnerPosition).normalized;
        Vector3 slashDir = new Vector3(-toEnemy.y, toEnemy.x, 0f).normalized;

        // Start on one side of the enemy, destination on the other side
        transform.position = target.transform.position - slashDir * (slashDistance * 0.5f);
        destination = target.transform.position + slashDir * (slashDistance * 0.5f);

        float angle = Mathf.Atan2(slashDir.y, slashDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        transform.localScale = originalScale;

        Debug.Log($"[Slash] Start: {transform.position} Dest: {destination} Dir: {slashDir}");

        StartCoroutine(SlashRoutine());
    }

    private IEnumerator SlashRoutine()
    {
        while (timer < lifetime)
        {
            timer += Time.deltaTime;

            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                slashSpeed * Time.deltaTime
            );

            if (spriteRenderer != null)
            {
                float t = timer / lifetime;
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(1f, 0f, t * 1.5f);
                spriteRenderer.color = c;
            }

            if (!hasDealtDamage && target != null)
            {
                if (Vector2.Distance(transform.position, target.transform.position) < 0.5f)
                    DealDamage();
            }

            yield return null;
        }

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            yield return new WaitForSeconds(trailRenderer.time);
        }

        Destroy(gameObject);
    }

    private void DealDamage()
    {
        hasDealtDamage = true;
        if (target == null) return;

        target.GetComponent<EnemyHealth>()?.TakeDamage(damage);
        CameraShake.Instance?.Shake(0.08f, 0.1f);
        Debug.Log($"[Dismantle] Hit {target.name} for {damage} damage.");
    }
}