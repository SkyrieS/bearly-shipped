using UnityEngine;

public class DragController : MonoBehaviour
{
    public string HintToStart = "Drag flag to guide the animals";
    public string HintType = "Flag";

    public bool enableFlagDrag = true;
    public bool enableAnimalDrag = true;
    public bool enableShapeDrag = true;

    private IDragMode currentMode;
    public Camera mainCamera;

    [Header("Drag Sounds")]
    public AudioSource audioSource;
    public AudioClip pickSound;
    public AudioClip releaseSound;

    void Start()
    {
        PopupManager.Instance.ShowPopup(HintType, HintToStart, 0f);
    }

    void OnDisable()
    {
        ForceEndDrag();
    }

    public void ForceEndDrag()
    {
        if (currentMode != null && currentMode.IsDragging)
        {
            GameModeManager.Instance.SetDragCursor();
            if (audioSource != null && releaseSound != null)
                audioSource.PlayOneShot(releaseSound);
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
                if (audioSource != null && releaseSound != null)
                    audioSource.PlayOneShot(releaseSound);
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
                    if (enableAnimalDrag && target.GetComponent<Animal>() != null)
                        currentMode = new AnimalDragMode();
                    else if (enableShapeDrag && target.GetComponent<DraggableShape>() != null)
                        currentMode = new ShapeDragMode();
                    else if (enableFlagDrag && target.GetComponent<Flag>() != null)
                        currentMode = new FlagDragMode();

                    if (currentMode != null)
                    {
                        GameModeManager.Instance.SetAnimalDragCursor();
                        currentMode.BeginDrag(target, mainCamera);
                        if (audioSource != null && pickSound != null)
                            audioSource.PlayOneShot(pickSound);
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
            if (audioSource != null && releaseSound != null)
                audioSource.PlayOneShot(releaseSound);
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