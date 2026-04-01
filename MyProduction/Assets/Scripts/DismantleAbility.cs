using UnityEngine;
using System.Collections.Generic;

public class DismantleAbility : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject dismantleSlashPrefab;

    [Header("Settings")]
    [SerializeField] private float baseCooldown = 1f;
    [SerializeField] private float targetRange = 20f;
    [SerializeField] private float damageMultiplier = 0.5f;

    [Header("Scaling")]
    [SerializeField] private float cooldownReductionPerStack = 0.1f; // 10% per stack
    [SerializeField] private float minCooldown = 0.25f; // cap
    [SerializeField] private float damagePerStack = 0.15f; // +15% damage per stack

    private bool isUnlocked = false;
    private float cooldownTimer = 0f;
    private int stacks = 0;

    private PlayerLevel playerLevel;
    private PlayerStats playerStats;

    private void Awake()
    {
        playerLevel = GetComponent<PlayerLevel>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!isUnlocked) return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            FireDismantle();

            // Cooldown scaling (multiplicative)
            float finalCooldown = baseCooldown * Mathf.Pow(1f - cooldownReductionPerStack, stacks);
            cooldownTimer = Mathf.Max(minCooldown, finalCooldown);
        }
    }

    private void FireDismantle()
    {
        if (dismantleSlashPrefab == null)
        {
            Debug.LogWarning("[DismantleAbility] dismantleSlashPrefab not assigned!");
            return;
        }

        int slashCount = playerLevel != null ? playerLevel.BaseProjectileCount : 1;

        List<GameObject> targets = FindNearestEnemies(slashCount);
        if (targets.Count == 0) return;

        float damage = CalculateDamage();

        for (int i = 0; i < slashCount; i++)
        {
            GameObject target = targets[i % targets.Count];

            GameObject slashObj = Instantiate(
                dismantleSlashPrefab,
                transform.position,
                Quaternion.identity
            );

            DismantleSlash slash = slashObj.GetComponent<DismantleSlash>();
            slash?.Initialise(target, damage, transform.position);
        }

        Debug.Log($"[Dismantle] Fired {slashCount} slash(es) for {damage} damage each. (Stacks: {stacks})");
    }

    private float CalculateDamage()
    {
        float playerDamage = playerLevel != null ? playerLevel.BaseDamage : 1f;
        float itemMult = playerStats != null ? playerStats.DamageMultiplier : 1f;

        float stackMult = 1f + (stacks * damagePerStack);

        return playerDamage * itemMult * damageMultiplier * stackMult;
    }

    private List<GameObject> FindNearestEnemies(int count)
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag("Enemy");
        var distances = new List<(GameObject obj, float dist)>();

        foreach (GameObject e in all)
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d <= targetRange)
                distances.Add((e, d));
        }

        distances.Sort((a, b) => a.dist.CompareTo(b.dist));

        var result = new List<GameObject>();
        int take = Mathf.Min(count, distances.Count);
        for (int i = 0; i < take; i++)
            result.Add(distances[i].obj);

        return result;
    }

    public void AddStack()
    {
        stacks++;

        if (!isUnlocked)
        {
            isUnlocked = true;
            cooldownTimer = 0.5f;
            Debug.Log("[Dismantle] Ability unlocked!");
        }

        Debug.Log($"[Dismantle] Stacks increased to {stacks}");
    }

    public bool IsUnlocked => isUnlocked;
}