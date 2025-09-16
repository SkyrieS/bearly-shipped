using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 90f;
    public float positionLerpSpeed = 8f;
    public float rotationLerpSpeed = 8f;

    [Header("Movement Bounds")]
    public Vector2 minBounds = new Vector2(-10, -10);
    public Vector2 maxBounds = new Vector2(10, 10);

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Awake()
    {
        if(LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.MainSceneCamera = gameObject.GetComponent<Camera>();
            LoadingScreenManager.Instance.MainSceneListener = gameObject.GetComponent<AudioListener>();
        }
    }

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        Vector3 move = (right * moveX + forward * moveZ).normalized * moveSpeed * Time.deltaTime;
        targetPosition += move;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);

        float rotate = 0f;
        if (Input.GetKey(KeyCode.Q)) rotate -= 1f;
        if (Input.GetKey(KeyCode.E)) rotate += 1f;

        if (rotate != 0f)
        {
            targetRotation = Quaternion.Euler(0, rotate * rotationSpeed * Time.deltaTime, 0) * targetRotation;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, positionLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }
}