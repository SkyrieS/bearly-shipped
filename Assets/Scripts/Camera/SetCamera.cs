using UnityEngine;

public class SetCamera : MonoBehaviour
{
    private void Awake()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.mainSceneCamera = gameObject.GetComponent<Camera>();
        }
    }
}
