using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeVisual : MonoBehaviour
{
    [Header("Melee Visual")]
    [SerializeField] private GameObject meleeSprite; 
    [SerializeField] private float baseDistance = 0.5f; 
    [SerializeField] private float thrustDistance = 1.5f; 
    [SerializeField] private float thrustSpeed = 15f; 

    private Camera mainCamera;
    private Vector3 targetPosition;
    private bool isThrusting = false;
    private float thrustTimer = 0f;
    private float thrustDuration = 0.2f;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        meleeSprite.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (isThrusting)
        {
            thrustTimer += Time.deltaTime;

            if (thrustTimer < thrustDuration)
            {
                targetPosition = transform.position + (Vector3)direction * thrustDistance;
            }
            else
            {
                isThrusting = false;
                thrustTimer = 0f;
            }
        }
        else
        {
            targetPosition = transform.position + (Vector3)direction * baseDistance;
        }

        meleeSprite.transform.position = Vector3.Lerp(meleeSprite.transform.position, targetPosition, Time.deltaTime * thrustSpeed);
    }

    public void Thrust()
    {
        isThrusting = true;
        thrustTimer = 0f;
    }
}