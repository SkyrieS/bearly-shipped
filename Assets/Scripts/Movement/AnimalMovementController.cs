using UnityEngine;
using System.Collections.Generic;

public class AnimalMovementController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stopRadius = 3f;
    [SerializeField] private GameObject stopArea;
    [SerializeField] private float fallThreshold = -10f;

    private List<Rigidbody> animalRigidbodies = new List<Rigidbody>();

    private Dictionary<Rigidbody, Vector3> animalStopPoints = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, Vector3> animalStartPoints = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, Animal> rigidbodyToAnimal = new Dictionary<Rigidbody, Animal>();
    private Dictionary<Rigidbody, bool> animalReachedState = new Dictionary<Rigidbody, bool>();

    private float stopPointThreshold = 0.2f;

    private void Start()
    {
        RecalculateStopPoints(true);
    }

    public void RemoveAnimal(Animal animal)
    {
        var rb = animal.Rigidbody;
        if (rb != null && animalRigidbodies.Contains(rb))
        {
            animalRigidbodies.Remove(rb);
            animalStopPoints.Remove(rb);
            rigidbodyToAnimal.Remove(rb);
            animalReachedState.Remove(rb);
        }
    }

    public void RecalculateStopPoints(bool teleport = false)
    {
        animalRigidbodies.Clear();
        animalStopPoints.Clear();
        rigidbodyToAnimal.Clear();
        animalReachedState.Clear();

        var animals = FindObjectsByType<Animal>(FindObjectsSortMode.None);

        List<Rigidbody> sortedAnimals = new List<Rigidbody>();
        foreach (var animal in animals)
        {
            var rb = animal.Rigidbody;
            if (rb != null)
            {
                sortedAnimals.Add(rb);
                rigidbodyToAnimal[rb] = animal;
            }
        }
        sortedAnimals.Sort((a, b) =>
            Vector3.Distance(a.position, target.position).CompareTo(
            Vector3.Distance(b.position, target.position)));

        int animalCount = sortedAnimals.Count;
        if (animalCount == 0) return;

        List<Vector3> stopPoints = new List<Vector3>();

        stopPoints.Add(target.position);

        int innerRingCount = Mathf.Min(4, animalCount - 1);
        float innerRadius = stopRadius * 0.5f;
        for (int i = 0; i < innerRingCount; i++)
        {
            float angle = (360f / 4) * i * Mathf.Deg2Rad;
            stopPoints.Add(target.position + new Vector3(
                Mathf.Cos(angle) * innerRadius,
                0f,
                Mathf.Sin(angle) * innerRadius
            ));
        }

        int outerRingCount = animalCount - 1 - innerRingCount;
        for (int i = 0; i < outerRingCount; i++)
        {
            float angle = (360f / Mathf.Max(outerRingCount, 1)) * i * Mathf.Deg2Rad;
            stopPoints.Add(target.position + new Vector3(
                Mathf.Cos(angle) * stopRadius * 0.85f,
                0f,
                Mathf.Sin(angle) * stopRadius * 0.85f
            ));
        }

        for (int i = 0; i < animalCount; i++)
        {
            Rigidbody animalRb = sortedAnimals[i];
            Vector3 stopPoint = stopPoints[i];

            animalRigidbodies.Add(animalRb);
            animalStopPoints[animalRb] = stopPoint;
            
            if (teleport)
            {
                animalRb.position = stopPoint;
                animalStartPoints[animalRb] = stopPoint;
            }

        }
    }

    private void FixedUpdate()
    {
        foreach (Rigidbody animalRb in animalRigidbodies)
        {
            if (animalRb == null)
                continue;

            if (rigidbodyToAnimal.TryGetValue(animalRb, out Animal animal) && !animal.isPicked && animalRb.position.y < fallThreshold)
            {
                if (animalStartPoints.TryGetValue(animalRb, out Vector3 resetPosition))
                {
                    animalRb.position = resetPosition;
                    animalRb.linearVelocity = Vector3.zero;
                    animalRb.angularVelocity = Vector3.zero;
                    PopupManager.Instance.ShowPopup("Fall animal" , "Animals that have fallen off the map return to their starting position", 5f);
                }
                continue;
            }

            MoveAnimalTowardsStopPoint(animalRb);
        }
    }

    private void Update()
    {
        UpdateStopAreaIndicator();
    }

    private void MoveAnimalTowardsStopPoint(Rigidbody animalRb)
    {
        if (target == null || !animalStopPoints.ContainsKey(animalRb)) return;

        Vector3 stopPoint = animalStopPoints[animalRb];
        Vector3 toStopPoint = stopPoint - animalRb.position;
        Vector2 toStopPointXZ = new Vector2(toStopPoint.x, toStopPoint.z);
        float distanceXZ = toStopPointXZ.magnitude;

        bool reached = distanceXZ <= stopPointThreshold;

        if (rigidbodyToAnimal.TryGetValue(animalRb, out var animal))
        {
            if (animal.isPicked)
                return;

            if (reached)
            {
                animal.OnReachedTarget();
            }
            else if (!reached)
            {
                animal.OnStartedMovingToTarget();
            }
        }

        if (reached)
            return;

        Vector3 direction = toStopPoint.normalized;

        Vector3 directionXZ = new Vector3(direction.x, 0f, direction.z);
        if (directionXZ.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionXZ);
            animalRb.rotation = Quaternion.Slerp(animalRb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        animalRb.MovePosition(animalRb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    private void UpdateStopAreaIndicator()
    {
        if (stopArea != null && target != null)
        {
            stopArea.transform.position = target.position;

            float diameter = stopRadius * 2f;
            stopArea.transform.localScale = new Vector3(diameter, 0.01f, diameter);
        }
    }
}
