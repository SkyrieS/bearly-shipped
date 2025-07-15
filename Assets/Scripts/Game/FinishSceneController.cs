using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinishSceneController : MonoBehaviour
{
    [Header("Truck and Gift")]
    public GameObject truck;
    public GameObject gift;
    public Transform truckStart;
    public Transform truckEnd;
    public float truckMoveDuration = 3f;
    public float giftDropDelay = 2f;
    public GameObject giftLift;
    public float liftHeight = 2f;
    public float liftSpeed = 2f;

    [Header("Animals Confetti")]
    public List<GameObject> animalPrefabs;
    public Transform confettiOrigin;
    public int animalCount = 10;
    public float confettiForce = 8f;
    public float confettiSpread = 2f;

    [Header("UI Elements")]
    public GameObject exitButtonGroup;
    public float exitButtonDelay = 5f;
    public Button exitButton;

    [Header("3D Happy Birthday Text")]
    public Transform happyBirthdayText;
    public float textPopDuration = 0.7f;
    private Vector3 textOriginalScale;
    private BoxCollider happyBirthdayCollider;
    public Vector3 textStartOffset = new Vector3(0, 0.2f, 0); // Offset inside box
    public Vector3 textEndOffset = new Vector3(0, 1.2f, 0);   // Offset above box
    private Vector3 textStartPos;
    private Vector3 textEndPos;
    public AnimationCurve textScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float textMoveDuration = 0.5f;
    public float textSmallScale = 0.3f;

    [Header("Present Drop Animation")]
    public float presentDropHeight = 3f;
    public float presentDropDuration = 0.7f;

    [Header("Hint")]
    public Transform hintText; // Assign a TextMeshPro or TextMeshProUGUI object in the inspector
    public Vector3 hintOffset = new Vector3(0, 1.5f, 0);

    private bool liftActive = false;
    private bool confettiThrown = false;
    private float liftTargetY;

    private Rigidbody giftLiftRb;
    private Collider giftLiftCollider;
    private bool isDraggingLift = false;
    public float liftForce = 20f;
    public float extraLiftForce = 100f;
    public float liftRaycastDistance = 10f;

    private bool lidClicked = false;
    public float initialLiftForce = 30f;
    public float delayBeforeExtraForce = 0.5f;

    private Queue<GameObject> animalPool = new Queue<GameObject>();
    public int poolSize = 30;
    public float confettiEmitDuration = 1f;
    public int animalsPerBurst = 2;
    public float animalDeactivateDelay = 3f;

    void Awake()
    {
        // Create animal pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = animalPrefabs[Random.Range(0, animalPrefabs.Count)];
            GameObject animal = Instantiate(prefab, confettiOrigin.position, Quaternion.identity);
            animal.SetActive(false);
            animalPool.Enqueue(animal);
        }
        gameObject.SetActive(false);
        LoadingScreenManager.Instance.sceneManagers.Add(this.gameObject);
    }

    void Start()
    {
        exitButtonGroup.SetActive(false);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        giftLiftRb = giftLift.GetComponent<Rigidbody>();
        // Get any collider on the giftLift
        // Setup 3D text
        if (happyBirthdayText != null)
        {
            textOriginalScale = happyBirthdayText.localScale;
            happyBirthdayText.localScale = Vector3.zero;
            happyBirthdayCollider = happyBirthdayText.GetComponent<BoxCollider>();
            if (happyBirthdayCollider == null)
            {
                happyBirthdayCollider = happyBirthdayText.gameObject.AddComponent<BoxCollider>();
            }
            // Set start and end positions relative to the gift
            textStartPos = gift.transform.position + textStartOffset;
            textEndPos = gift.transform.position + textEndOffset;
            happyBirthdayText.position = textStartPos;
            happyBirthdayText.gameObject.SetActive(false);
        }
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // Move truck
        truck.transform.position = truckStart.position;
        float t = 0;
        // Start gift drop after delay, in parallel
        StartCoroutine(GiftDropAfterDelay(giftDropDelay));
        while (t < truckMoveDuration)
        {
            truck.transform.position = Vector3.Lerp(truckStart.position, truckEnd.position, t / truckMoveDuration);
            t += Time.deltaTime;
            yield return null;
        }
        truck.transform.position = truckEnd.position;
    }

    IEnumerator GiftDropAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Drop gift animation
        Vector3 dropStart = gift.transform.position + Vector3.up * presentDropHeight;
        Vector3 dropEnd = gift.transform.position;
        gift.transform.position = dropStart;
        gift.SetActive(true);
        float t = 0;
        while (t < presentDropDuration)
        {
            float lerp = t / presentDropDuration;
            gift.transform.position = Vector3.Lerp(dropStart, dropEnd, lerp);
            t += Time.deltaTime;
            yield return null;
        }
        gift.transform.position = dropEnd;

        // Enable lift and show hint
        liftActive = true;
        if (hintText != null)
        {
            hintText.gameObject.SetActive(true);
        }
        gift.SetActive(true);
        giftLiftRb.isKinematic = false;
        giftLiftRb.useGravity = true;
    }

    void Update()
    {
        if (liftActive && !confettiThrown && !lidClicked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, liftRaycastDistance))
                {
                    if (hit.collider.gameObject.CompareTag("GiftLift"))
                    {
                        lidClicked = true;
                        if (hintText != null) hintText.gameObject.SetActive(false);
                        ApplyExtraLiftAndConfetti();
                    }
                }
            }
        }
    }

    void ApplyExtraLiftAndConfetti()
    {
        Vector3 randomUp = (Vector3.up + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f))).normalized;
        giftLiftRb.AddForce(randomUp * extraLiftForce, ForceMode.Impulse);
        liftActive = false;
        StartCoroutine(ThrowConfetti());
    }

    IEnumerator ThrowConfetti()
    {
        confettiThrown = true;
        float emitInterval = confettiEmitDuration / (poolSize / animalsPerBurst);
        int emitted = 0;
        while (emitted < poolSize)
        {
            for (int i = 0; i < animalsPerBurst && emitted < poolSize; i++)
            {
                if (animalPool.Count > 0)
                {
                    GameObject animal = animalPool.Dequeue();
                    animal.transform.position = confettiOrigin.position;
                    animal.SetActive(true);
                    Rigidbody rb = animal.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        Vector3 dir = (Vector3.up + Random.insideUnitSphere * confettiSpread).normalized;
                        rb.AddForce(dir * confettiForce, ForceMode.Impulse);
                    }
                }
                emitted++;
            }
            yield return new WaitForSeconds(emitInterval);
        }
        StartCoroutine(ShowHappyBirthday());
    }

    IEnumerator ShowHappyBirthday()
    {
        // Show and animate 3D text from inside the box
        if (happyBirthdayText != null)
        {
            happyBirthdayText.gameObject.SetActive(true);
            // Phase 1: Move up while small
            float t = 0;
            Vector3 smallScale = textOriginalScale * textSmallScale;
            while (t < textMoveDuration)
            {
                float lerp = t / textMoveDuration;
                happyBirthdayText.localScale = smallScale;
                happyBirthdayText.position = Vector3.Lerp(textStartPos, textEndPos, lerp);
                t += Time.deltaTime;
                yield return null;
            }
            happyBirthdayText.position = textEndPos;
            happyBirthdayText.localScale = smallScale;
            // Phase 2: Scale up at final position with curve
            t = 0;
            while (t < textPopDuration)
            {
                float lerp = t / textPopDuration;
                float curveValue = textScaleCurve.Evaluate(lerp);
                happyBirthdayText.localScale = Vector3.LerpUnclamped(smallScale, textOriginalScale, curveValue);
                t += Time.deltaTime;
                yield return null;
            }
            happyBirthdayText.localScale = textOriginalScale;
        }
        // Wait for exit button group
        yield return new WaitForSeconds(exitButtonDelay);
        exitButtonGroup.SetActive(true);
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }
}
