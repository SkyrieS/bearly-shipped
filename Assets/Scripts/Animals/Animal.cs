using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Animal : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform holdPoint;
    public Transform HoldPoint => holdPoint;
    public Rigidbody Rigidbody => GetComponent<Rigidbody>();

    [Header("Effects")]
    [SerializeField] private AudioClip disableSound;
    [SerializeField] private ParticleSystem disableParticles;
    [SerializeField] private AudioSource audioSource;

    [Header("Truck Entry")]
    [SerializeField] private Vector3 truckFrontOffset = new Vector3(0, 0, -0.7f);
    [SerializeField] private float moveToFrontDuration = 0.3f;
    [SerializeField] private float moveIntoTruckDuration = 0.5f;

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
        PopupManager.Instance.HidePopup("Animals");
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

    private IEnumerator EnterTruckSequence(Vector3 targetPosition)
    {
        isPicked = true;

        Vector3 frontPosition = targetPosition + truckFrontOffset;
        float t = 0f;
        Vector3 startPosition = transform.position;
        while (t < moveToFrontDuration)
        {
            transform.position = Vector3.Lerp(startPosition, frontPosition, t / moveToFrontDuration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = frontPosition;

        t = 0f;
        while (t < moveIntoTruckDuration)
        {
            transform.position = Vector3.Lerp(frontPosition, targetPosition, t / moveIntoTruckDuration);
            t += Time.deltaTime;
            yield return null;
        }
        Rigidbody.position = targetPosition;

        if (disableSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(disableSound);
            else
                AudioSource.PlayClipAtPoint(disableSound, transform.position);
        }
        if (disableParticles != null)
        {
            disableParticles.Play();
        }

        gameObject.SetActive(false);
    }
}