    using UnityEngine;

public class ShapeDragMode : IDragMode
{
    private float forceAmount = 500;

    private DraggableShape shape;
    private bool isDragging;

    private float selectionDistance;

    public bool IsDragging => isDragging;

    public void BeginDrag(GameObject target, Camera camera)
    {
        shape = target.GetComponent<DraggableShape>();
        if (shape != null)
        {
            isDragging = true;
            selectionDistance = Vector3.Distance(camera.transform.position, target.transform.position);
            shape.OnBeginDrag();
        }
    }

    public void UpdateDrag(Camera camera)
    {
        if (isDragging && shape != null)
        {
            HandleScrollInput();
        }
    }

    public void FixedUpdateDrag(Camera camera)
    {
        if (isDragging && shape != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = selectionDistance;
            Vector3 worldPosition = camera.ScreenToWorldPoint(mousePosition);
            shape.Rigidbody.linearVelocity = (worldPosition - shape.transform.position) * forceAmount * Time.deltaTime;

            float keyRotationSpeed = 90f * Time.deltaTime; // degrees per second

            Quaternion rotationDelta = Quaternion.identity;

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                rotationDelta = Quaternion.AngleAxis(-keyRotationSpeed, Vector3.up) * rotationDelta;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                rotationDelta = Quaternion.AngleAxis(keyRotationSpeed, Vector3.up) * rotationDelta;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                rotationDelta = Quaternion.AngleAxis(-keyRotationSpeed, Vector3.right) * rotationDelta;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                rotationDelta = Quaternion.AngleAxis(keyRotationSpeed, Vector3.right) * rotationDelta;
            }

            if (rotationDelta != Quaternion.identity)
            {
                shape.Rigidbody.MoveRotation(shape.Rigidbody.rotation * rotationDelta);
            }
        }
    }

    public void EndDrag()
    {
        if (isDragging && shape != null)
        {
            shape.OnEndDrag();
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