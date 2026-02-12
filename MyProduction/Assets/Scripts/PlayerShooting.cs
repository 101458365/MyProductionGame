using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Auto Shooting")]
    [SerializeField] private GameObject autoProjectilePrefab;
    [SerializeField] private float autoProjectileSpeed = 10f;
    [SerializeField] private float autoTargetRange = 15f;
    [SerializeField] private float autoFireRate = 0.5f;
    [SerializeField] private Transform autoFirePoint;

    [Header("Melee Attack")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeAngle = 90f;
    [SerializeField] private float meleeCooldown = 0.5f;

    private Camera mainCamera;
    private MeleeVisual meleeVisual;
    private float nextAutoFireTime = 0f;
    private float nextMeleeTime = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;
        meleeVisual = GetComponent<MeleeVisual>();
    }

    private void Update()
    {
        // Auto-shoot at nearest enemy
        if (Time.time >= nextAutoFireTime)
        {
            ShootAtNearestEnemy();
        }

        // Left click - melee attack towards mouse
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextMeleeTime)
        {
            MeleeAttack();
        }
    }

    private void ShootAtNearestEnemy()
    {
        GameObject nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            Vector2 direction = (nearestEnemy.transform.position - transform.position).normalized;
            FireProjectile(direction);
            nextAutoFireTime = Time.time + autoFireRate;
        }
    }

    private void MeleeAttack()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0;
        Vector2 attackDirection = (mouseWorldPos - transform.position).normalized;

        if (meleeVisual != null)
        {
            meleeVisual.Thrust();
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Vector2 toEnemy = enemy.transform.position - transform.position;
            float distance = toEnemy.magnitude;

            if (distance <= meleeRange)
            {
                float angle = Vector2.Angle(attackDirection, toEnemy.normalized);

                if (angle <= meleeAngle / 2)
                {
                    Destroy(enemy);
                }
            }
        }

        nextMeleeTime = Time.time + meleeCooldown;
    }

    private void FireProjectile(Vector2 direction)
    {
        Vector3 spawnPos = autoFirePoint != null ? autoFirePoint.position : transform.position;
        GameObject projectile = Instantiate(autoProjectilePrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * autoProjectileSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float closestDistance = autoTargetRange;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}