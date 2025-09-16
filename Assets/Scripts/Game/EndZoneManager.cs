using System.Collections;
using TMPro;
using UnityEngine;

public class EndZoneManager : MonoBehaviour
{
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private Transform truckTargetPoint;
    [SerializeField] private AnimalMovementController animalMovementController;
    [SerializeField] private DragController dragController;

    [SerializeField] private string sceneToLoad;
    [SerializeField] private string currentScene;
    [SerializeField] private int loadScreenIndex;

    [Header("Truck Animation")]
    [SerializeField] private GameObject truck;
    [SerializeField] private Animator truckDoorAnimator;
    [SerializeField] private Transform truckMoveEndPoint;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float truckMoveDuration = 2f;
    [SerializeField] private string truckDoorCloseTrigger = "Close";


    private int score = 0;
    private int totalAnimals;
    private bool truckSequenceStarted = false;

    private void Awake()
    {
        var animals = FindObjectsByType<Animal>(FindObjectsSortMode.None);
        totalAnimals = animals.Length;
        UpdateScoreText();
    }

    private void OnTriggerEnter(Collider other)
    {
        var animal = other.GetComponent<Animal>();
        if (animal != null && !animal.IsEnteringTruck())
        {
            PopupManager.Instance.HidePopup("End truck");

            score++;
            UpdateScoreText();

            if (animalMovementController != null)
            {
                animalMovementController.RemoveAnimal(animal);
            }

            if (dragController != null)
            {
                dragController.ForceEndDrag();
            }

            animal.EnterTruck(truckTargetPoint.position);

            if (!truckSequenceStarted && score >= totalAnimals)
            {
                truckSequenceStarted = true;
                StartCoroutine(CloseTruckAndMoveForward());
            }
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{score}/{totalAnimals}";
        }
    }

    public int GetScore() => score;

    private IEnumerator CloseTruckAndMoveForward()
    {
        audioSource.Play();

        yield return new WaitForSeconds(0.5f);

        if (truckDoorAnimator != null)
        {
            truckDoorAnimator.SetTrigger(truckDoorCloseTrigger);
            yield return new WaitForSeconds(2f);
        }
        if (truck != null && truckMoveEndPoint != null)
        {
            Vector3 start = truck.transform.position;
            Vector3 end = truckMoveEndPoint.position;
            float t = 0f;
            while (t < truckMoveDuration)
            {
                float normalized = t / truckMoveDuration;
                float eased = normalized * normalized;
                truck.transform.position = Vector3.Lerp(start, end, eased);
                t += Time.deltaTime;
                yield return null;
            }
            truck.transform.position = end;
        }
        StartCoroutine(StopAudio());
        LoadingScreenManager.Instance.FinishAndLoadNext(sceneToLoad, currentScene, loadScreenIndex);
    }

    private IEnumerator StopAudio()
    {
        float t = 0f;
        while (t < 3)
        {
            audioSource.volume = Mathf.Lerp(1f, 0, t);
            t += Time.deltaTime;
            yield return null;
        }
    }
}