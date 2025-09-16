using System.Collections;
using UnityEngine;

public class Flag : MonoBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private LayerMask terrainMask;
    [SerializeField] private AnimalMovementController movementController;

    private GameObject ghostInstance;

    private bool hasShownPopup = false;


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
        PopupManager.Instance.HidePopup("Flag");
        if(!hasShownPopup)
        {
            StartCoroutine(ShowPopup(2f));
        }

        transform.position = ghostInstance.transform.position;
        Destroy(ghostInstance);

        if (movementController != null)
        {
            movementController.RecalculateStopPoints();
        }
    }

    IEnumerator ShowPopup(float delay)
    {
        hasShownPopup = true;
        yield return new WaitForSeconds(delay);
        PopupManager.Instance.ShowPopup("End truck", "Lead animals to the truck", 0f);
    }
}