using UnityEngine;

public class EliteEnemy : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private float scaleMultiplier = 1.5f;
    [SerializeField] private Color eliteTint = new Color(1f, 0.6f, 0f);   // orange glow
    [SerializeField] private GameObject auraObject;   // optional child glow sprite

    [Header("Stats Multiplier")]
    [SerializeField] private float eliteGoldMultiplier = 3f;

    private void Awake()
    {
        // Enlarge the enemy
        transform.localScale *= scaleMultiplier;

        // Tint the sprite
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = eliteTint;

        // Enable aura if assigned
        if (auraObject != null)
            auraObject.SetActive(true);

        // Mark EnemyHealth as elite so it uses elite stats/drops
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
            health.SetElite(true, eliteGoldMultiplier);
    }
}
