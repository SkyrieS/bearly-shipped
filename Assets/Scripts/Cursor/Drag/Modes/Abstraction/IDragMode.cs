using UnityEngine;

public interface IDragMode
{
    void BeginDrag(GameObject target, Camera camera);
    void UpdateDrag(Camera camera);
    void FixedUpdateDrag(Camera camera);
    void EndDrag();
    bool IsDragging { get; }
}