using UnityEngine;

public class AnimalDragMode : IDragMode
{
    private float forceAmount = 500;

    private Animal animal;
    private bool isDragging;

    private float selectionDistance;

    public bool IsDragging => isDragging;

    public void BeginDrag(GameObject target, Camera camera)
    {
        animal = target.GetComponent<Animal>();
        if (animal != null)
        {
            isDragging = true;
            selectionDistance = Vector3.Distance(camera.transform.position, animal.HoldPoint.position);
            animal.OnBeginDrag();
        }
    }

    public void UpdateDrag(Camera camera)
    {
        if (isDragging && animal != null)
        {
            HandleScrollInput();
        }
    }

    public void FixedUpdateDrag(Camera camera)
    {
        if (isDragging && animal != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = selectionDistance;
            Vector3 worldPosition = camera.ScreenToWorldPoint(mousePosition);
            animal.Rigidbody.linearVelocity = (worldPosition - animal.HoldPoint.position) * forceAmount * Time.deltaTime;
        }
    }

    public void EndDrag()
    {
        if (isDragging && animal != null)
        {
            animal.OnEndDrag();
            isDragging = false;
        }
    }

    private void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        selectionDistance += scroll * 2f;
        selectionDistance = Mathf.Clamp(selectionDistance, 1f, 20f);
    }
}