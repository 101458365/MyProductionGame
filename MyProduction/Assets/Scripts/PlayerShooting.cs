using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
    private PlayerLevel playerLevel;
    private PlayerStats playerStats;
    private float nextAutoFireTime = 0f;
    private float nextMeleeTime = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;
        meleeVisual = GetComponent<MeleeVisual>();
        playerLevel = GetComponent<PlayerLevel>();
        playerStats = GetComponent<PlayerStats>();
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
        // Get projectile count
        int projectileCount = 1;
        if (playerLevel != null)
        {
            projectileCount = playerLevel.BaseProjectileCount;
        }

        // Find multiple nearest enemies
        List<GameObject> nearestEnemies = FindNearestEnemies(projectileCount);

        if (nearestEnemies.Count > 0)
        {
            int projectilesFired = 0;

            // Fire one projectile at each unique enemy
            foreach (GameObject enemy in nearestEnemies)
            {
                Vector2 direction = (enemy.transform.position - transform.position).normalized;
                FireSingleProjectile(direction);
                projectilesFired++;
            }

            // If we have extra projectiles and only 1 enemy, fire remaining at that enemy in a spread
            if (nearestEnemies.Count == 1 && projectilesFired < projectileCount)
            {
                GameObject target = nearestEnemies[0];
                Vector2 baseDirection = (target.transform.position - transform.position).normalized;

                // Fire remaining projectiles in a slight spread
                int remaining = projectileCount - projectilesFired;
                for (int i = 0; i < remaining; i++)
                {
                    float angle = (i + 1) * 10f * (i % 2 == 0 ? 1 : -1); // Alternate left/right: +10, -10, +20, -20...
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * baseDirection;
                    FireSingleProjectile(direction);
                }
            }

            // Apply attack speed multiplier to fire rate
            float finalFireRate = autoFireRate;
            if (playerLevel != null)
            {
                finalFireRate /= playerLevel.BaseAttackSpeed;
            }
            if (playerStats != null)
            {
                finalFireRate /= playerStats.AttackSpeedMultiplier;
            }

            nextAutoFireTime = Time.time + finalFireRate;
        }
    }

    private List<GameObject> FindNearestEnemies(int count)
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> nearestEnemies = new List<GameObject>();

        // Create list of enemies with distances
        List<(GameObject enemy, float distance)> enemyDistances = new List<(GameObject, float)>();

        foreach (GameObject enemy in allEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= autoTargetRange)
            {
                enemyDistances.Add((enemy, distance));
            }
        }

        // Sort by distance (closest first)
        enemyDistances.Sort((a, b) => a.distance.CompareTo(b.distance));

        // Take the closest 'count' enemies
        int enemiesToTake = Mathf.Min(count, enemyDistances.Count);
        for (int i = 0; i < enemiesToTake; i++)
        {
            nearestEnemies.Add(enemyDistances[i].enemy);
        }

        return nearestEnemies;
    }

    private void FireSingleProjectile(Vector2 direction)
    {
        Vector3 spawnPos = autoFirePoint != null ? autoFirePoint.position : transform.position;
        GameObject projectile = Instantiate(autoProjectilePrefab, spawnPos, Quaternion.identity);

        // Set damage on projectile
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            float totalDamage = 1f; // Base projectile damage

            // Apply base damage from leveling up
            if (playerLevel != null)
            {
                totalDamage = playerLevel.BaseDamage;
            }

            // Apply damage multiplier from items
            if (playerStats != null)
            {
                totalDamage = playerLevel.BaseDamage;
            }
            Debug.Log($"Total Projectile Damage: {totalDamage}");
            proj.SetDamage(totalDamage);
        }

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * autoProjectileSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
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

        // Calculate melee damage
        float meleeDamage = 2f; // Base melee damage
        if (playerLevel != null)
        {
            meleeDamage *= playerLevel.BaseDamage;
        }
        if (playerStats != null)
        {
            meleeDamage *= playerStats.DamageMultiplier;
        }

        foreach (GameObject enemy in enemies)
        {
            Vector2 toEnemy = enemy.transform.position - transform.position;
            float distance = toEnemy.magnitude;

            if (distance <= meleeRange)
            {
                float angle = Vector2.Angle(attackDirection, toEnemy.normalized);

                if (angle <= meleeAngle / 2)
                {
                    EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(meleeDamage);
                    }
                }
            }
        }

        // Apply attack speed multiplier to melee cooldown
        float finalMeleeCooldown = meleeCooldown;
        if (playerLevel != null)
        {
            finalMeleeCooldown /= playerLevel.BaseAttackSpeed;
        }
        if (playerStats != null)
        {
            finalMeleeCooldown /= playerStats.AttackSpeedMultiplier;
        }

        nextMeleeTime = Time.time + finalMeleeCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}