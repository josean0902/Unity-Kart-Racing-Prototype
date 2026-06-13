using System;
using System.Collections;
using UnityEngine;

public partial class KartController : MonoBehaviour
{

    public event Action OnHoldOn;

    public event Action<KartDriftSide> OnDriftStarted;
    public event Action OnDriftEnded;

    public event Action<Vector3, Vector3> OnCrash;

    public event Action OnBoostStarted;
    public event Action OnBoostEnded;

    [Header("References")]
    [SerializeField] private PlayerInput input;
    [SerializeField] private KartRaceProgress raceProgress;
    [SerializeField] private KartTrackFollower trackFollower;
    [SerializeField] private KartGroundSensor groundSensor;
    [SerializeField] private KartDriftBoost drift;
    [SerializeField] private RaceManager raceManager;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 60.0f;
    [SerializeField] private float acceleration = 30.0f;

    [Header("Rotation")]
    [SerializeField] private float snapToFloorRotationSpeed = 10;
    [SerializeField] private float pointToTargetRotationSpeed = 2;

    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.5f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float raycastDistance = 2.0f;
    [SerializeField] private float speedPenaltyMultiplier = 0.9f;

    public float CurrentSpeedNormalized => GetFinalSpeed() / maxSpeed;

    private bool raceStarted;

    private float currentSpeed;
    private Vector3 currentMoveDir;
    private Vector3 desiredDirection;

    private KartDriftSide currentDriftSide;

    private Coroutine boostCoroutine;
    private bool boosting;

    private void Start()
    {
        currentMoveDir = transform.forward;

        SubscribeToEvents();
    }

    private void Update()
    {
        if (!raceStarted)
            return;

        UpdateSpeed();

        Vector3 target = trackFollower.GetLookAheadPoint(currentSpeed);

        SnapToGround();

        HandleRotation(target);

        CheckCollision();

        UpdateMovement(target);

        drift.UpdateDrift();
        CheckDriftCondition();

        trackFollower.UpdateTrackPointIndex();

#if UNITY_EDITOR

        // Visual debug
        Debug.DrawLine(transform.position, target, Color.green);
        float moveDirLenght = 10.0f;
        Debug.DrawLine(transform.position, transform.position + currentMoveDir * moveDirLenght, Color.red);

#endif

    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (input != null)
        {
            input.OnHoldPressed += TryStartDrift;
            input.OnHoldReleased += EndDrift;
        }

        if (raceManager != null)
            raceManager.OnRaceStarted += OnRaceStarted;

        if (raceProgress != null)
            raceProgress.OnRaceFinished += HandleRaceFinished;
    }

    private void UnsubscribeFromEvents()
    {
        if (input != null)
        {
            input.OnHoldPressed -= TryStartDrift;
            input.OnHoldReleased -= EndDrift;
        }

        if (raceManager != null)
            raceManager.OnRaceStarted -= OnRaceStarted;

        if (raceProgress != null)
            raceProgress.OnRaceFinished -= HandleRaceFinished;
    }

    private void TryStartDrift()
    {
        OnHoldOn?.Invoke();

        if (!CanDrift())
            return;

        currentDriftSide = GetDriftDirection();
        OnDriftStarted?.Invoke(currentDriftSide);

        drift.StartDrift();
    }

    private bool CanDrift() => Vector3.Angle(desiredDirection, currentMoveDir) > drift.MinDriftAngle;

    private KartDriftSide GetDriftDirection()
    {
        Vector3 cross =
             Vector3.Cross(currentMoveDir, desiredDirection);

        float side =
            Vector3.Dot(cross, groundSensor.GroundHit.normal);

        if (side > 0f)
            return KartDriftSide.Right;

        return KartDriftSide.Left;

    }

    private void EndDrift()
    {
        if (!drift.Drifting)
            return;

        float boostDuration = drift.ReleaseBoost();

        OnDriftEnded?.Invoke();

        if (boostDuration > 0)
        {
            if (boostCoroutine != null)
            {
                StopCoroutine(boostCoroutine);
                boostCoroutine = null;
            }

            boostCoroutine = StartCoroutine(ApplyBoost(boostDuration));
        }
    }

    private IEnumerator ApplyBoost(float boostDuration)
    {
        boosting = true;
        OnBoostStarted?.Invoke();

        yield return new WaitForSeconds(boostDuration);

        boosting = false;
        OnBoostEnded?.Invoke();
    }

    private void HandleRaceFinished()
    {
        if (input == null)
            return;

        input.enabled = false;
    }

    private void OnRaceStarted()
    {
        raceStarted = true;
    }

    private void UpdateSpeed()
    {
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
    }

    private void SnapToGround()
    {
        Vector3 position = transform.position;
        position.y = groundSensor.GroundHit.point.y;
        transform.position = position;
    }

    private void HandleRotation(Vector3 target)
    {
        AlignToGround();
        AlignToTarget(target);
    }

    private void AlignToGround()
    {
        Quaternion groundRotation = Quaternion.FromToRotation(transform.up, groundSensor.GroundHit.normal) * transform.rotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            groundRotation,
            snapToFloorRotationSpeed * Time.deltaTime
        );
    }

    private void AlignToTarget(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        Vector3 direction = Vector3.ProjectOnPlane(toTarget, groundSensor.GroundHit.normal);
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            pointToTargetRotationSpeed * Time.deltaTime
        );
    }

    private void CheckCollision()
    {

        float yOffset = 2.0f;
#if UNITY_EDITOR

        Vector3 origin = transform.position + Vector3.up * yOffset;
        Debug.DrawLine(origin, origin + currentMoveDir * raycastDistance, Color.red);

#endif
        if (Physics.SphereCast(
            transform.position + Vector3.up * yOffset,
            collisionRadius, currentMoveDir,
            out RaycastHit hitInfo, raycastDistance, collisionMask))
        {
            currentMoveDir = Vector3.ProjectOnPlane(currentMoveDir, hitInfo.normal).normalized;
            drift.CancelDrift();
            currentSpeed *= speedPenaltyMultiplier;
            OnDriftEnded?.Invoke();
            OnCrash?.Invoke(hitInfo.point, hitInfo.normal);
        }
    }

    private void UpdateMovement(Vector3 target)
    {
        desiredDirection =
            Vector3.ProjectOnPlane(target - transform.position, groundSensor.GroundHit.normal);

        float gripFactor = drift.CurrentGripFactor;
        currentMoveDir = Vector3.Slerp(
            currentMoveDir,
            desiredDirection,
            gripFactor * Time.deltaTime).normalized;

        transform.position += currentMoveDir * GetFinalSpeed() * Time.deltaTime;
    }

    private float GetFinalSpeed()
    {
        float speed = currentSpeed;

        if (boosting)
            speed *= drift.BoostMultiplier;

        return speed;
    }

    private void CheckDriftCondition()
    {
        if (!drift.Drifting)
            return;

        if (currentDriftSide != GetDriftDirection())
            EndDrift();
    }
}