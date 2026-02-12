using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Auto Shooting")]
    [SerializeField] private GameObject autoProjectilePrefab;
    [SerializeField] private float autoProjectileSpeed = 10f;
    [SerializeField] private float autoTargetRange = 15f;
    [SerializeField] private float autoFireRate = 0.5f; // Time between auto shots
    [SerializeField] private Transform autoFirePoint; // Where auto projectile spawns

    [Header("Manual Shooting")]
    [SerializeField] private GameObject manualProjectilePrefab;
    [SerializeField] private float manualProjectileSpeed = 15f;
    [SerializeField] private float manualFireRate = 0.2f; // Time between manual shots
    [SerializeField] private Transform manualFirePoint; // Where manual projectile spawns

    private Camera mainCamera;
    private float nextAutoFireTime = 0f;
    private float nextManualFireTime = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Auto-shoot at nearest enemy (runs independently)
        if (Time.time >= nextAutoFireTime)
        {
            ShootAtNearestEnemy();
        }

        // Left click - shoot at mouse position (independent from auto-shoot)
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextManualFireTime)
        {
            ShootAtMouse();
        }
    }

    private void ShootAtNearestEnemy()
    {
        GameObject nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            Vector2 direction = (nearestEnemy.transform.position - transform.position).normalized;
            FireProjectile(autoProjectilePrefab, autoProjectileSpeed, direction, autoFirePoint);
            nextAutoFireTime = Time.time + autoFireRate;
        }
    }

    private void ShootAtMouse()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0;

        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        FireProjectile(manualProjectilePrefab, manualProjectileSpeed, direction, manualFirePoint);
        nextManualFireTime = Time.time + manualFireRate;
    }

    private void FireProjectile(GameObject prefab, float speed, Vector2 direction, Transform firePoint)
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject projectile = Instantiate(prefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // Rotate projectile to face direction
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
}