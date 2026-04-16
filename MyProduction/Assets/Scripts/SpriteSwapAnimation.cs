using UnityEngine;

public class SpriteSwapAnimation : MonoBehaviour
{
    [Header("Frames")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 12f;
    [SerializeField] private bool destroyOnFinish = true;

    [Header("Scale")]
    [SerializeField] private bool scaleUp = true;
    [SerializeField] private float startScale = 0.3f;
    [SerializeField] private float endScale = 1.5f;

    private SpriteRenderer sr;
    private float timer = 0f;
    private int currentFrame = 0;
    private float frameDuration;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        frameDuration = 1f / frameRate;

        if (frames != null && frames.Length > 0 && sr != null)
        {
            sr.sprite = frames[0];
            transform.localScale = Vector3.one * startScale;
        }
    }

    // Called by AOEExplosion after it calculates finalRadius
    // so the animation scales up to match the actual explosion size
    public void SetScale(float start, float end)
    {
        startScale = start;
        endScale = end;
        transform.localScale = Vector3.one * startScale;
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (destroyOnFinish)
                    Destroy(gameObject);
                else
                    currentFrame = 0;
                return;
            }

            if (sr != null)
                sr.sprite = frames[currentFrame];
        }

        if (scaleUp && frames.Length > 1)
        {
            float t = (float)currentFrame / (frames.Length - 1);
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);
        }
    }
}