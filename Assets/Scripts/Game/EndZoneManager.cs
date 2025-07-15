using UnityEngine;
using TMPro;
using System.Collections;

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
    [SerializeField] private GameObject truck; // Assign the truck GameObject
    [SerializeField] private Animator truckDoorAnimator; // Assign if using Animator for door
    [SerializeField] private Transform truckMoveEndPoint; // Where the truck should move after closing
    [SerializeField] private float truckMoveDuration = 2f;
    [SerializeField] private string truckDoorCloseTrigger = "Close"; // Animator trigger name

    private int score = 0;
    private int totalAnimals;
    private bool truckSequenceStarted = false;

    private void Awake()
    {
        // Find all animals to get the total count
        var animals = FindObjectsOfType<Animal>();
        totalAnimals = animals.Length;
        UpdateScoreText();

        if (animalMovementController == null)
        {
            animalMovementController = FindObjectOfType<AnimalMovementController>();
        }

        if (dragController == null)
        {
            dragController = FindObjectOfType<DragController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var animal = other.GetComponent<Animal>();
        if (animal != null && !animal.IsEnteringTruck())
        {
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
        // Close the truck door (if animator is assigned)
        if (truckDoorAnimator != null)
        {
            truckDoorAnimator.SetTrigger(truckDoorCloseTrigger);
            // Wait for door close animation (assume 1s, adjust as needed)
            yield return new WaitForSeconds(2f);
        }
        // Move the truck forward
        if (truck != null && truckMoveEndPoint != null)
        {
            Vector3 start = truck.transform.position;
            Vector3 end = truckMoveEndPoint.position;
            float t = 0f;
            while (t < truckMoveDuration)
            {
                float normalized = t / truckMoveDuration;
                float eased = normalized * normalized; // Ease-in (acceleration)
                truck.transform.position = Vector3.Lerp(start, end, eased);
                t += Time.deltaTime;
                yield return null;
            }
            truck.transform.position = end;
        }
        LoadingScreenManager.Instance.FinishAndLoadNext(sceneToLoad, currentScene, loadScreenIndex);
    }
}