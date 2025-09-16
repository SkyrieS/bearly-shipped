using UnityEngine;

public class SetCamera : MonoBehaviour
{
    private void Awake()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.MainSceneCamera = gameObject.GetComponent<Camera>();
            LoadingScreenManager.Instance.MainSceneListener = gameObject.GetComponent<AudioListener>();
        }
    }
}
