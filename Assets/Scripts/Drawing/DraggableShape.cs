using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(Rigidbody))]
public class DraggableShape : MonoBehaviour
{
    public MeshCollider MeshCollider { get; private set; }
    public Rigidbody Rigidbody { get; private set; }

    void Awake()
    {
        MeshCollider = GetComponent<MeshCollider>();
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.linearDamping = 1f;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void OnBeginDrag()
    {
        Rigidbody.useGravity = false;
    }

    public void OnEndDrag()
    {
        Rigidbody.useGravity = true;
    }
}