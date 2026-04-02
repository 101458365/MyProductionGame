using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DismantleSlash : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float minSlashDistance = 1f;
    [SerializeField] private float maxSlashDistance = 3f;
    [SerializeField] private float slashSpeed = 6f;
    [SerializeField] private float lifetime = 1f;

    [Header("Hit Detection")]
    [SerializeField] private float hitRadius = 0.4f;

    private GameObject primaryTarget;
    private float damage;
    private Vector3 destination;
    private float timer = 0f;
    private Vector3 originalScale;

    // AOE
    private GameObject aoeExplosionPrefab;
    private int aoeStacks = 0;

    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        originalScale = transform.localScale;
    }

    public void Initialise(GameObject slashTarget, float slashDamage, Vector3 spawnerPosition,
                           float angleOffset = 0f, GameObject aoePrefab = null, int aoeStackCount = 0)
    {
        primaryTarget = slashTarget;
        damage = slashDamage;
        aoeExplosionPrefab = aoePrefab;
        aoeStacks = aoeStackCount;

        if (primaryTarget == null)
        {
            Destroy(gameObject, lifetime);
            return;
        }

        Vector3 toEnemy = (primaryTarget.transform.position - spawnerPosition).normalized;
        Vector3 baseSlashDir = new Vector3(-toEnemy.y, toEnemy.x, 0f).normalized;
        Vector3 slashDir = Quaternion.Euler(0f, 0f, angleOffset) * baseSlashDir;

        float slashDistance = Random.Range(minSlashDistance, maxSlashDistance);
        transform.position = primaryTarget.transform.position - slashDir * (slashDistance * 0.5f);
        destination = primaryTarget.transform.position + slashDir * (slashDistance * 0.5f);

        float angle = Mathf.Atan2(slashDir.y, slashDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        transform.localScale = originalScale;

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

            CheckHits();

            if (spriteRenderer != null)
            {
                float t = timer / lifetime;
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(1f, 0f, t * 1.5f);
                spriteRenderer.color = c;
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

    private void CheckHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            GameObject enemy = hit.gameObject;
            if (hitEnemies.Contains(enemy)) continue;

            hitEnemies.Add(enemy);

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"[Dismantle] Hit {enemy.name} for {damage} damage.");

                // Proc AOE if player has AOE item stacks
                if (aoeExplosionPrefab != null && aoeStacks > 0)
                {
                    GameObject explosion = Instantiate(aoeExplosionPrefab, enemy.transform.position, Quaternion.identity);
                    explosion.GetComponent<AOEExplosion>()?.Initialise(damage, aoeStacks);
                }
            }
        }
    }
}