using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Animal : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform holdPoint;
    public Transform HoldPoint => holdPoint;
    public Rigidbody Rigidbody => GetComponent<Rigidbody>();

    private readonly List<string> animationList = new List<string> { "Idle_A", "Idle_B", "Idle_C" };

    private bool reachedTarget = false;
    public bool isPicked = false;
    private bool isEnteringTruck = false;

    public void OnBeginDrag()
    {
        isPicked = true;
        Rigidbody.useGravity = false;
        reachedTarget = false;
        Rigidbody.isKinematic = false;

        if (animator.HasState(0, Animator.StringToHash("Walk")))
        {
            animator.Play("Walk");
        }
    }

    public void OnEndDrag()
    {
        isPicked = false;
        Rigidbody.useGravity = true;
    }

    public void OnReachedTarget()
    {
        if(reachedTarget || isPicked) 
            return;

        reachedTarget = true;

        string currentIdleAnimation = animationList[Random.Range(0, animationList.Count)];
        if (animator.HasState(0, Animator.StringToHash(currentIdleAnimation)))
        {
            animator.Play(currentIdleAnimation);
        }

        Rigidbody.isKinematic = true;
    }

    public void OnStartedMovingToTarget()
    {
        if (!reachedTarget || isPicked)
            return;

        reachedTarget = false;

        if (animator.HasState(0, Animator.StringToHash("Walk")))
        {
            animator.Play("Walk");
        }

        Rigidbody.isKinematic = false;
    }

    public bool IsEnteringTruck()
    {
        return isEnteringTruck;
    }

    public void EnterTruck(Vector3 targetPosition)
    {
        if (isEnteringTruck) return;
        isEnteringTruck = true;

        StartCoroutine(EnterTruckSequence(targetPosition));
    }

    private System.Collections.IEnumerator EnterTruckSequence(Vector3 targetPosition)
    {
        // Disable physics interactions
        Rigidbody.isKinematic = false;

        // Animate towards the truck
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Rigidbody.position = targetPosition;

        // Disable the animal
        gameObject.SetActive(false);
    }
}