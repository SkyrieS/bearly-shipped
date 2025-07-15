using UnityEngine;

public class DragController : MonoBehaviour
{
    private IDragMode currentMode;
    public Camera mainCamera;

    void OnDisable()
    {
        ForceEndDrag();
    }

    public void ForceEndDrag()
    {
        if (currentMode != null && currentMode.IsDragging)
        {
            GameModeManager.Instance.SetDragCursor();
            currentMode.EndDrag();
            currentMode = null;
        }
    }

    void Update()
    {
        if (!this.enabled)
        {
            if (currentMode != null && currentMode.IsDragging)
            {
                currentMode.EndDrag();
                currentMode = null;
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var target = hit.collider.gameObject;
                if (target != null)
                {
                    if (target.GetComponent<Animal>() != null)
                        currentMode = new AnimalDragMode();
                    else if (target.GetComponent<DraggableShape>() != null)
                        currentMode = new ShapeDragMode();
                    else if (target.GetComponent<Flag>() != null)
                        currentMode = new FlagDragMode();

                    if (currentMode != null)
                    {
                        GameModeManager.Instance.SetAnimalDragCursor();
                        currentMode.BeginDrag(target, mainCamera);
                    }
                }
            }
        }

        if (Input.GetMouseButton(0) && currentMode != null && currentMode.IsDragging)
        {
            currentMode.UpdateDrag(mainCamera);
        }

        if (Input.GetMouseButtonUp(0) && currentMode != null && currentMode.IsDragging)
        {
            GameModeManager.Instance.SetDragCursor();
            currentMode.EndDrag();
            currentMode = null;
        }
    }

    void FixedUpdate()
    {
        if (!this.enabled) return;

        if (currentMode != null && currentMode.IsDragging)
        {
            currentMode.FixedUpdateDrag(mainCamera);
        }
    }
}