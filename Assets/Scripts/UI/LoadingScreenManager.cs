using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Tooltip("Assign loading screen canvas prefabs here (one per type)")]
    [SerializeField] private List<GameObject> loadingScreenObjects;

    [Header("Transition Elements")]
    [SerializeField] private Camera loadingCamera;
    [SerializeField] private AudioListener loadingListener;
    [SerializeField] private Camera mainSceneCamera;
    [SerializeField] private AudioListener mainSceneListener;
    [SerializeField] private CanvasGroup yellowFadeGroup;
    [SerializeField] private CanvasGroup clickTextGroup;
    [SerializeField] private List<GameObject> sceneManagers;

    public Camera MainSceneCamera { get => mainSceneCamera; set => mainSceneCamera = value; }
    public AudioListener MainSceneListener { get => mainSceneListener; set => mainSceneListener = value; }
    public List<GameObject> SceneManagers => sceneManagers;

    [Tooltip("Fade duration in seconds")]
    [SerializeField] private float fadeDuration = 0.7f;

    private GameObject currentLoadingScreen;

    private bool waitingForClick = false;
    private Action onContinueAction;

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
  
        if (mainSceneCamera != null) mainSceneCamera.enabled = false;
        if (loadingCamera != null) loadingCamera.enabled = true;
        if (mainSceneListener != null) mainSceneListener.enabled = false;
        if (loadingListener != null) loadingListener.enabled = true;

        SetManagersActive(false);

        currentLoadingScreen = loadingScreenObjects[loadingScreenIndex];
        currentLoadingScreen.SetActive(true);

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

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextSceneName));

        if (nextSceneName != "FinishScene")
        {
            clickTextGroup.gameObject.SetActive(true);
            StartCoroutine(FadeCanvasGroup(clickTextGroup, 0f, 1f, 1f));

            waitingForClick = true;
            bool clicked = false;
            onContinueAction = () => { clicked = true; };
            while (!clicked) yield return null;
            onContinueAction = null;
        }
        else
        {
            yield return new WaitForSeconds(5f);
        }

        yellowFadeGroup.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(yellowFadeGroup, 0f, 1f, fadeDuration));

        if (currentLoadingScreen != null)
            currentLoadingScreen.SetActive(false);

        clickTextGroup.gameObject.SetActive(false);

        if (mainSceneCamera != null) mainSceneCamera.enabled = true;
        if (loadingCamera != null) loadingCamera.enabled = false;
        if (mainSceneListener != null) mainSceneListener.enabled = true;
        if (loadingListener != null) loadingListener.enabled = false;

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
