using UnityEngine;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public enum GameMode
    {
        Drag,
        Draw
    }

    [Header("Mode Controllers")]
    [SerializeField] private DragController dragController;
    [SerializeField] private ShapeDrawer shapeDrawer;

    [Header("UI Elements")]
    [SerializeField] private Image dragIcon;
    [SerializeField] private Image drawIcon;
    [SerializeField] private Button dragButton;
    [SerializeField] private Button drawButton;

    [Header("Cursors")]
    [SerializeField] private Texture2D dragCursor;
    [SerializeField] private Texture2D drawCursor;
    [SerializeField] private Texture2D animalDragCursor;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    [Header("UI State")]
    [SerializeField] private float activeAlpha = 1f;
    [SerializeField] private float inactiveAlpha = 0.5f;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseMinScale = 0.9f;
    [SerializeField] private float pulseMaxScale = 1.1f;

    private GameMode currentMode;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (dragController == null)
            dragController = FindObjectOfType<DragController>();
        if (shapeDrawer == null)
            shapeDrawer = FindObjectOfType<ShapeDrawer>();

        if (dragButton != null)
            dragButton.onClick.AddListener(() => SwitchMode(GameMode.Drag));
        if (drawButton != null)
            drawButton.onClick.AddListener(() => SwitchMode(GameMode.Draw));

        SwitchMode(GameMode.Drag);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchMode(GameMode.Drag);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchMode(GameMode.Draw);
        }
    }

    public void SetDragCursor()
    {
        Cursor.SetCursor(dragCursor, cursorHotspot, CursorMode.Auto);
    }

    public void SetDrawCursor()
    {
        Cursor.SetCursor(drawCursor, cursorHotspot, CursorMode.Auto);
    }

    public void SetAnimalDragCursor()
    {
        Cursor.SetCursor(animalDragCursor, cursorHotspot, CursorMode.Auto);
    }

    private void SwitchMode(GameMode newMode)
    {
        currentMode = newMode;

        dragController.enabled = (currentMode == GameMode.Drag);
        shapeDrawer.enabled = (currentMode == GameMode.Draw);

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        if (currentMode == GameMode.Drag)
        {
            SetDragCursor();
            dragIcon.color = new Color(dragIcon.color.r, dragIcon.color.g, dragIcon.color.b, activeAlpha);
            drawIcon.color = new Color(drawIcon.color.r, drawIcon.color.g, drawIcon.color.b, inactiveAlpha);
            pulseCoroutine = StartCoroutine(Pulse(dragIcon.rectTransform));
            if (drawIcon != null) drawIcon.rectTransform.localScale = Vector3.one;
        }
        else // Draw mode
        {
            SetDrawCursor();
            drawIcon.color = new Color(drawIcon.color.r, drawIcon.color.g, drawIcon.color.b, activeAlpha);
            dragIcon.color = new Color(dragIcon.color.r, dragIcon.color.g, dragIcon.color.b, inactiveAlpha);
            pulseCoroutine = StartCoroutine(Pulse(drawIcon.rectTransform));
            if (dragIcon != null) dragIcon.rectTransform.localScale = Vector3.one;
        }
    }

    private System.Collections.IEnumerator Pulse(RectTransform target)
    {
        if (target == null) yield break;
        while (true)
        {
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1) / 2f);
            target.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }
}
