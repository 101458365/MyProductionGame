using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DismantleAbility : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject dismantleSlashPrefab;
    [SerializeField] private GameObject aoeExplosionPrefab;  // same prefab as PlayerShooting uses

    [Header("Settings")]
    [SerializeField] private float baseCooldown = 1f;
    [SerializeField] private float targetRange = 20f;
    [SerializeField] private float damageMultiplier = 0.5f;

    [Header("Multi-Slash")]
    [SerializeField] private int slashesPerTarget = 3;
    [SerializeField] private float minAngleBetweenSlashes = 10f;
    [SerializeField] private float maxAngleBetweenSlashes = 90f;

    [Header("Scaling")]
    [SerializeField] private float cooldownReductionPerStack = 0.1f;
    [SerializeField] private float minCooldown = 0.25f;
    [SerializeField] private float damagePerStack = 0.15f;

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

        int targetCount = playerLevel != null ? playerLevel.BaseProjectileCount : 1;
        List<GameObject> targets = FindNearestEnemies(targetCount);
        if (targets.Count == 0) return;

        float totalDamage = CalculateDamage();
        float damagePerSlash = totalDamage / slashesPerTarget;

        // Get AOE stacks from PlayerStats
        int aoStacks = (playerStats != null && playerStats.HasAOE) ? playerStats.AOEStacks : 0;
        GameObject aoePrefab = (aoStacks > 0) ? aoeExplosionPrefab : null;

        foreach (GameObject target in targets)
            StartCoroutine(SlashBurstRoutine(target, damagePerSlash, aoePrefab, aoStacks));

        Debug.Log($"[Dismantle] Fired {slashesPerTarget} slashes at {targets.Count} target(s). " +
                  $"{damagePerSlash:F2} dmg each. AOE stacks: {aoStacks}");
    }

    private IEnumerator SlashBurstRoutine(GameObject target, float damagePerSlash,
                                          GameObject aoePrefab, int aoStacks)
    {
        float angleBetweenSlashes = Random.Range(minAngleBetweenSlashes, maxAngleBetweenSlashes);
        float totalSpread = angleBetweenSlashes * (slashesPerTarget - 1);
        float startAngle = -totalSpread / 2f;

        for (int i = 0; i < slashesPerTarget; i++)
        {
            if (target == null) yield break;

            float angleOffset = startAngle + (angleBetweenSlashes * i);

            GameObject slashObj = Instantiate(dismantleSlashPrefab, transform.position, Quaternion.identity);
            slashObj.GetComponent<DismantleSlash>()?.Initialise(
                target, damagePerSlash, transform.position, angleOffset, aoePrefab, aoStacks);

            yield return new WaitForSeconds(0.08f);
        }
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
        Debug.Log($"[Dismantle] Stacks: {stacks}");
    }

    public bool IsUnlocked => isUnlocked;
}