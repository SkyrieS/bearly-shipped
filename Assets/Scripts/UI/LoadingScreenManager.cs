using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Tooltip("Assign loading screen canvas prefabs here (one per type)")]
    public List<GameObject> loadingScreenObjects;

    [Header("Transition Elements")]
    public Camera loadingCamera;
    public Camera mainSceneCamera;
    public CanvasGroup yellowFadeGroup;
    public CanvasGroup clickTextGroup;
    public List<GameObject> sceneManagers;

    private GameObject currentLoadingScreen;

    [Tooltip("Fade duration in seconds")]
    public float fadeDuration = 0.7f;


    private bool waitingForClick = false;
    private System.Action onContinueAction;

    void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        FinishAndLoadNext("Level 1", null, 0);
    }

    void Update()
    {
        if (waitingForClick && Input.GetMouseButtonDown(0))
        {
            waitingForClick = false;
            onContinueAction?.Invoke();
        }
    }

    public void FinishAndLoadNext(string nextSceneName, string currentSceneName, int loadingScreenIndex = 0)
    {
        StartCoroutine(FinishAndLoadNextRoutine(nextSceneName, currentSceneName, loadingScreenIndex));
    }

    private IEnumerator FinishAndLoadNextRoutine(string nextSceneName, string currentSceneName, int loadingScreenIndex, bool firstLoad = false)
    {
        // Enable yellow fade
        if (firstLoad)
        {
            yellowFadeGroup.gameObject.SetActive(true);
            yellowFadeGroup.alpha = 1f;
        }
        else
        {
            yellowFadeGroup.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(yellowFadeGroup, 0f, 1f, fadeDuration));
        }
  
        // Switch to loading camera
        if (mainSceneCamera != null) mainSceneCamera.enabled = false;
        if (loadingCamera != null) loadingCamera.enabled = true;

        SetManagersActive(false);

        currentLoadingScreen = loadingScreenObjects[loadingScreenIndex];
        currentLoadingScreen.SetActive(true);

        // Fade out yellow
        if (yellowFadeGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(yellowFadeGroup, 1f, 0f, fadeDuration));
            yellowFadeGroup.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(currentSceneName)) 
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentSceneName);
            while (!asyncUnload.isDone)
            {
                yield return null;
            }
        }

        // Load next scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextSceneName));

        if (clickTextGroup != null)
        {
            clickTextGroup.gameObject.SetActive(true);
            StartCoroutine(FadeCanvasGroup(clickTextGroup, 0f, 1f, 1f));
        }

        // Wait for click to continue
        waitingForClick = true;
        bool clicked = false;
        onContinueAction = () => { clicked = true; };
        while (!clicked) yield return null;
        onContinueAction = null;

        yellowFadeGroup.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(yellowFadeGroup, 0f, 1f, fadeDuration));

        if (currentLoadingScreen != null)
            currentLoadingScreen.SetActive(false);

        clickTextGroup.gameObject.SetActive(false);

        if (mainSceneCamera != null) mainSceneCamera.enabled = true;
        if (loadingCamera != null) loadingCamera.enabled = false;

        yield return StartCoroutine(FadeCanvasGroup(yellowFadeGroup, 1f, 0f, fadeDuration));
        yellowFadeGroup.gameObject.SetActive(false);


        SetManagersActive(true);
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

    private void SetManagersActive(bool active)
    {
        if (sceneManagers == null) return;
        foreach (var go in sceneManagers)
        {
            if (go != null) go.SetActive(active);
        }
    }
}
