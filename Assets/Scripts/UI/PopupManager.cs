using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("Popup UI Elements")]
    public Canvas popupCanvas; // Assign a world-space or screen-space canvas in inspector
    public GameObject popupPanel; // Assign a panel GameObject in inspector
    public Text popupText; // Assign a Text UI element in inspector
    public Button closeButton; // Assign a Button UI element in inspector

    private HashSet<string> shownTypes = new HashSet<string>();
    private string currentType = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (closeButton != null)
            closeButton.onClick.AddListener(HidePopup);
        HidePopup();
    }

    public void ShowPopup(string type, string message)
    {
        if (shownTypes.Contains(type)) return;
        shownTypes.Add(type);
        currentType = type;
        if (popupPanel != null) popupPanel.SetActive(true);
        if (popupText != null) popupText.text = message;
        if (popupCanvas != null) popupCanvas.enabled = true;
    }

    public void HidePopup()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        if (popupCanvas != null) popupCanvas.enabled = false;
        currentType = null;
    }

    public bool HasShown(string type)
    {
        return shownTypes.Contains(type);
    }

    // Optionally, reset shown types (for debugging or replay)
    public void ResetShownTypes()
    {
        shownTypes.Clear();
    }
}
