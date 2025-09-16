using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("Popup UI Elements")]
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private RectTransform popupParent;
    [SerializeField] private float fadeDuration = 0.3f;

    private HashSet<string> shownTypes = new HashSet<string>();
    private List<PopupInstance> activePopups = new List<PopupInstance>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowPopup(string type, string message, float duration = 0f)
    {
        if (shownTypes.Contains(type)) 
            return;
        shownTypes.Add(type);
        if (popupPrefab == null || popupParent == null) return;

        GameObject popupGO = Instantiate(popupPrefab, popupParent);
        popupGO.SetActive(true);

        TextMeshProUGUI txt = popupGO.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.text = message;

        PopupInstance instance = new PopupInstance(type, popupGO);
        activePopups.Add(instance);

        CanvasGroup cg = popupGO.GetComponent<CanvasGroup>();
        if (cg == null) cg = popupGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        instance.fadeCoroutine = StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, fadeDuration));
        if (duration > 0f)
        {
            instance.coroutine = StartCoroutine(AutoHide(instance, duration));
        }
    }

    private IEnumerator AutoHide(PopupInstance instance, float duration)
    {
        yield return new WaitForSeconds(duration);
        HidePopup(instance);
    }

    public void HidePopup(string type)
    {
        PopupInstance instance = activePopups.Find(p => p.type == type);
        if (instance != null)
        {
            HidePopup(instance);
        }
    }

    public void HidePopup(PopupInstance instance)
    {
        if (instance == null || instance.popupGO == null) return;
        if (instance.coroutine != null) StopCoroutine(instance.coroutine);
        if (instance.fadeCoroutine != null) StopCoroutine(instance.fadeCoroutine);
        activePopups.Remove(instance);
        CanvasGroup cg = instance.popupGO.GetComponent<CanvasGroup>();
        if (cg == null) cg = instance.popupGO.AddComponent<CanvasGroup>();
        StartCoroutine(FadeAndDestroy(instance, cg));
    }

    private IEnumerator FadeAndDestroy(PopupInstance instance, CanvasGroup cg)
    {
        yield return StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, fadeDuration));
        Destroy(instance.popupGO);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        cg.alpha = to;
    }

    public bool HasShown(string type)
    {
        return shownTypes.Contains(type);
    }

    public void ResetShownTypes()
    {
        shownTypes.Clear();
    }

    public class PopupInstance
    {
        public string type;
        public GameObject popupGO;
        public Coroutine coroutine;
        public Coroutine fadeCoroutine;
        public PopupInstance(string type, GameObject popupGO)
        {
            this.type = type;
            this.popupGO = popupGO;
            this.coroutine = null;
            this.fadeCoroutine = null;
        }
    }
}
