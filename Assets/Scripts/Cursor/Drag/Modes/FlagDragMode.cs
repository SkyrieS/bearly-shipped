using UnityEngine;

public class FlagDragMode : IDragMode
{
    private Flag flag;
    private bool isDragging;

    public bool IsDragging => isDragging;

    public void BeginDrag(GameObject target, Camera camera)
    {
        flag = target.GetComponent<Flag>();
        if (flag != null)
        {
            isDragging = true;
            flag.OnBeginDrag();
        }
    }

    public void UpdateDrag(Camera camera)
    {
        if (isDragging && flag != null)
        {
            flag.OnUpdateDrag(camera);
        }
    }

    public void FixedUpdateDrag(Camera camera)
    {
    }


    public void EndDrag()
    {
        if (isDragging && flag != null)
        {
            flag.OnEndDrag();
            isDragging = false;
        }
    }
}