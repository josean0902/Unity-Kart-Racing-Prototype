using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform lookAt;
    [SerializeField] private KartDriftBoost driftBoost;
    [SerializeField] private KartRaceProgress kartRaceProgress;

    [Header("Position Follow")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 5f, -8f);
    [SerializeField] private float positionSmooth = 8f;
    [SerializeField] private LayerMask collisionMask;

    [Header("Look At")]
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0, 1f, 0);
    [SerializeField] private float rotationSmooth = 10f;

    [Header("Fov")]
    [SerializeField] private float normalFov = 60.0f;
    [SerializeField] private float boostFovIncrement = 10.0f;
    [SerializeField] private float fovChangeSpeed = 10f;

    [Header("Race Finished Settings")]
    [SerializeField] private Vector3 finishCameraOffset = new Vector3(0, 5f, 20f);
    [SerializeField] private Vector3 finishLookAtOffset = new Vector3(0, 1f, 0);

    private Camera kartCamera;

    private Vector3 targetOffset;
    private Vector3 targetLookAt;

    private float targetFov;
    private Coroutine fovCoroutine;

    private void Start()
    {
        Setup();

        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnSubscribeFromEvents();
    }

    private void LateUpdate()
    {
        if (lookAt == null) 
            return;

        HandlePosition();
        HandleLookAt();

        HandleFov();
    }

    private void Setup()
    {
        targetOffset = cameraOffset;
        targetLookAt = lookAtOffset;

        targetFov = normalFov;
        kartCamera = GetComponent<Camera>();
    }

    private void SubscribeToEvents()
    {
        if (driftBoost != null)
            driftBoost.OnReleaseBoost += ChangeFov;

        if (kartRaceProgress != null)
            kartRaceProgress.OnRaceFinished += SetupEndGameCamera;
    }

    private void UnSubscribeFromEvents()
    {
        if (driftBoost != null)
            driftBoost.OnReleaseBoost -= ChangeFov;

        if (kartRaceProgress != null)
            kartRaceProgress.OnRaceFinished -= SetupEndGameCamera;
    }

    private void ChangeFov(int boostLevel, float duration)
    {
        if (fovCoroutine != null)
        {
            StopCoroutine(fovCoroutine);
            fovCoroutine = null;
        }

        fovCoroutine = StartCoroutine(ChangeFovCoroutine(boostLevel, duration));
    }

    private IEnumerator ChangeFovCoroutine(int boostLevel, float time)
    {
        targetFov = normalFov + boostFovIncrement * boostLevel;

        yield return new WaitForSeconds(time);

        targetFov = normalFov;
    }

    private void SetupEndGameCamera()
    {
        targetOffset = finishCameraOffset;
        targetLookAt = finishLookAtOffset;
    }

    private void HandlePosition()
    {
        Vector3 targetPosition =
        lookAt.position + lookAt.TransformDirection(targetOffset);

        Vector3 direction = targetPosition - lookAt.position;
        float distance = direction.magnitude;

        Vector3 directionNormalized = direction / distance;

        if (Physics.Raycast(
            lookAt.position, directionNormalized,
            out RaycastHit hitInfo, distance,
            collisionMask))
        {
            float positionOffset = 0.2f;
            targetPosition = hitInfo.point - directionNormalized * positionOffset;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmooth * Time.deltaTime
        );
    }

    private void HandleLookAt()
    {
        Vector3 lookTarget = lookAt.position + lookAt.TransformDirection(targetLookAt);

        Quaternion targetRotation = Quaternion.LookRotation(
            lookTarget - transform.position,
            Vector3.up
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmooth * Time.deltaTime
        );
    }

    private void HandleFov()
    {
        kartCamera.fieldOfView =
            Mathf.Lerp(
                kartCamera.fieldOfView,
                targetFov,
                fovChangeSpeed * Time.deltaTime
            );
    }

}