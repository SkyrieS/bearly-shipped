using UnityEngine;

public class Flag : MonoBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private LayerMask terrainMask;
    [SerializeField] private AnimalMovementController movementController;

    private GameObject ghostInstance;

    public void OnBeginDrag()
    {
        ghostInstance = Instantiate(ghostPrefab, transform.position, Quaternion.identity);
        ghostInstance.SetActive(true);
    }

    public void OnUpdateDrag(Camera camera)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, terrainMask))
        {
            ghostInstance.transform.position = hit.point;
        }
    }

    public void OnEndDrag()
    {
        transform.position = ghostInstance.transform.position;
        Destroy(ghostInstance);

        if (movementController != null)
        {
            movementController.RecalculateStopPoints();
        }
    }
}