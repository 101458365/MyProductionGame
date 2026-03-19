using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerShooting : MonoBehaviour
{
    [Header("Auto Shooting")]
    [SerializeField] private GameObject autoProjectilePrefab;
    [SerializeField] private float autoProjectileSpeed = 10f;
    [SerializeField] private float autoTargetRange     = 15f;
    [SerializeField] private float autoFireRate        = 0.5f;
    [SerializeField] private Transform autoFirePoint;

    [Header("Melee Attack")]
    [SerializeField] private float meleeRange    = 2f;
    [SerializeField] private float meleeAngle    = 90f;
    [SerializeField] private float meleeCooldown = 0.5f;

    [Header("AOE")]
    [SerializeField] private GameObject aoeExplosionPrefab;

    private Camera       mainCamera;
    private MeleeVisual  meleeVisual;
    private PlayerLevel  playerLevel;
    private PlayerStats  playerStats;
    private float        nextAutoFireTime = 0f;
    private float        nextMeleeTime   = 0f;

    private void Awake()
    {
        mainCamera  = Camera.main;
        meleeVisual = GetComponent<MeleeVisual>();
        playerLevel = GetComponent<PlayerLevel>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (Time.time >= nextAutoFireTime)
            ShootAtNearestEnemy();

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextMeleeTime)
            MeleeAttack();
    }

    private void ShootAtNearestEnemy()
    {
        int projectileCount = playerLevel != null ? playerLevel.BaseProjectileCount : 1;
        List<GameObject> nearestEnemies = FindNearestEnemies(projectileCount);

        if (nearestEnemies.Count == 0) return;

        int projectilesFired = 0;
        foreach (GameObject enemy in nearestEnemies)
        {
            Vector2 direction = (enemy.transform.position - transform.position).normalized;
            FireSingleProjectile(direction);
            projectilesFired++;
        }

        // Extra projectiles spread around single target
        if (nearestEnemies.Count == 1 && projectilesFired < projectileCount)
        {
            GameObject target    = nearestEnemies[0];
            Vector2 baseDirection = (target.transform.position - transform.position).normalized;
            int remaining         = projectileCount - projectilesFired;

            for (int i = 0; i < remaining; i++)
            {
                float angle   = (i + 1) * 10f * (i % 2 == 0 ? 1 : -1);
                Vector2 dir   = Quaternion.Euler(0, 0, angle) * baseDirection;
                FireSingleProjectile(dir);
            }
        }

        float finalFireRate = autoFireRate;
        if (playerLevel != null) finalFireRate /= playerLevel.BaseAttackSpeed;
        if (playerStats != null) finalFireRate /= playerStats.AttackSpeedMultiplier;
        nextAutoFireTime = Time.time + finalFireRate;
    }

    private List<GameObject> FindNearestEnemies(int count)
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        var enemyDistances      = new List<(GameObject enemy, float distance)>();

        foreach (GameObject enemy in allEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= autoTargetRange)
                enemyDistances.Add((enemy, distance));
        }

        enemyDistances.Sort((a, b) => a.distance.CompareTo(b.distance));

        var result = new List<GameObject>();
        int take   = Mathf.Min(count, enemyDistances.Count);
        for (int i = 0; i < take; i++)
            result.Add(enemyDistances[i].enemy);

        return result;
    }

    private void FireSingleProjectile(Vector2 direction)
    {
        Vector3 spawnPos   = autoFirePoint != null ? autoFirePoint.position : transform.position;
        GameObject projObj = Instantiate(autoProjectilePrefab, spawnPos, Quaternion.identity);

        Projectile proj = projObj.GetComponent<Projectile>();
        if (proj != null)
        {
            float totalDamage = playerLevel != null ? playerLevel.BaseDamage : 1f;
            Debug.Log($"Total Projectile Damage: {totalDamage}");
            proj.SetDamage(totalDamage);

            // Pass AOE data if the player has AOE stacks
            if (playerStats != null && playerStats.HasAOE && aoeExplosionPrefab != null)
            {
                float aoeDamage = totalDamage * (playerStats != null ? playerStats.DamageMultiplier : 1f);
                proj.SetAOE(aoeExplosionPrefab, playerStats.AOEStacks, aoeDamage);
            }
        }

        Rigidbody2D rb = projObj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = direction * autoProjectileSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projObj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void MeleeAttack()
    {
        Vector3 mouseWorldPos    = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z          = 0;
        Vector2 attackDirection  = (mouseWorldPos - transform.position).normalized;

        meleeVisual?.Thrust();

        float meleeDamage = 2f;
        if (playerLevel != null) meleeDamage *= playerLevel.BaseDamage;
        if (playerStats != null) meleeDamage *= playerStats.DamageMultiplier;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Vector2 toEnemy = enemy.transform.position - transform.position;
            float distance  = toEnemy.magnitude;

            if (distance <= meleeRange)
            {
                float angle = Vector2.Angle(attackDirection, toEnemy.normalized);
                if (angle <= meleeAngle / 2)
                {
                    EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(meleeDamage);

                        // Trigger AOE at the enemy's position on melee hit
                        if (playerStats != null && playerStats.HasAOE && aoeExplosionPrefab != null)
                        {
                            float aoeDamage    = meleeDamage;
                            GameObject explosion = Instantiate(aoeExplosionPrefab,
                                                               enemy.transform.position,
                                                               Quaternion.identity);
                            AOEExplosion aoe   = explosion.GetComponent<AOEExplosion>();
                            aoe?.Initialise(aoeDamage, playerStats.AOEStacks);
                        }
                    }
                }
            }
        }

        float finalMeleeCooldown = meleeCooldown;
        if (playerLevel != null) finalMeleeCooldown /= playerLevel.BaseAttackSpeed;
        if (playerStats != null) finalMeleeCooldown /= playerStats.AttackSpeedMultiplier;
        nextMeleeTime = Time.time + finalMeleeCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
