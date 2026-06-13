using System;
using System.Collections;
using UnityEngine;

public class KartView : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private KartController controller;
    [SerializeField] private KartDriftBoost drift;

    [Header("Drift VFX")]
    [SerializeField] private TrailRenderer[] skidmarkList;
    [SerializeField] private ParticleSystem[] sparksParticleList;


    [Header("Boost VFX")]
    [SerializeField] private ParticleSystem[] boostParticleList;
    [SerializeField]
    private Color[] vfxColorArray = new Color[]
                                    {
                                        new Color(0.31f, 0.67f, 1.0f),
                                        new Color(1.0f, 0.55f, 0.0f),
                                        new Color(0.67f, 0.27f, 1.0f)
                                    };
    [SerializeField] float intensity = 2.5f;

    [Header("Crash")]
    [SerializeField] private ParticleSystem crashParticleSystem;

    [Header("Hop")]
    [SerializeField] private float hopHeight = 0.35f;
    [SerializeField] private float hopTime = 0.18f;

    [Header("Rotation")]
    [SerializeField] private float yawSpeed = 10.0f;
    [SerializeField] private float driftYawOffset = 20.0f;

    [Header("Anticipation")]
    [SerializeField] private float stretchSquashSpeed = 10.0f;
    [SerializeField] private float stretchSquashIncrement = 0.1f;

    private MaterialPropertyBlock materialPropertyBlock;

    private Coroutine hopCorroutine;

    private bool isSubscribed;

    private float currentYaw;
    private float targetYaw;

    private Vector3 targetScale;

    private void Start()
    {
        SubscribeToEvents();

        materialPropertyBlock = new MaterialPropertyBlock();

        targetScale = Vector3.one;
    }

    private void Update()
    {
        HandleYaw();

        HandleScale();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (isSubscribed)
            return;

        if (controller == null || drift == null)
            return;

        controller.OnDriftStarted += StartDriftEffect;
        controller.OnDriftEnded += EndDriftEffect;
        controller.OnHoldOn += HopAnimation;
        controller.OnCrash += HandleCrash;

        drift.OnBoostLevelChanged += HandleBoostLevelChanged;
        drift.OnReleaseBoost += HandleBoost;

        isSubscribed = true;
    }

    private void UnsubscribeFromEvents()
    {
        if (!isSubscribed)
            return;

        if (controller != null)
        {
            controller.OnDriftStarted -= StartDriftEffect;
            controller.OnDriftEnded -= EndDriftEffect;
            controller.OnHoldOn -= HopAnimation;
            controller.OnCrash -= HandleCrash;
        }

        if (drift != null)
        {
            drift.OnBoostLevelChanged -= HandleBoostLevelChanged;
            drift.OnReleaseBoost -= HandleBoost;
        }

        isSubscribed = false;
    }

    private void StartDriftEffect(KartDriftSide side)
    {
        SetDriftVFX(true);
        if (side == KartDriftSide.Left)
            targetYaw = -driftYawOffset;
        else
            targetYaw = driftYawOffset;
    }

    private void EndDriftEffect()
    {
        SetDriftVFX(false);
        targetYaw = 0.0f;
    }

    private void SetDriftVFX(bool enable)
    {
        foreach (TrailRenderer skidmark in skidmarkList)
            skidmark.emitting = enable;

        foreach (ParticleSystem sparkParticle in sparksParticleList)
        {
            if (enable)
                sparkParticle.Play();
            else
            {
                sparkParticle.Stop();
            }
        }
    }

    private void HopAnimation()
    {
        if (hopCorroutine != null)
            return;

        hopCorroutine = StartCoroutine(nameof(HopAnimationCoroutine));
    }

    public IEnumerator HopAnimationCoroutine()
    {
        float hopTimer = 0f;

        Vector3 startPos = transform.localPosition;

        while (hopTimer < hopTime)
        {
            hopTimer += Time.deltaTime;

            float t = hopTimer / hopTime;

            float yOffset = 4f * hopHeight * t * (1f - t);

            transform.localPosition =
                startPos + Vector3.up * yOffset;

            yield return null;
        }

        transform.localPosition = startPos;

        hopCorroutine = null;
    }

    private void HandleCrash(Vector3 point, Vector3 normal)
    {
        EndDriftEffect();

        float yOffset = 1.0f;

        crashParticleSystem.transform.position = point - Vector3.up * yOffset;
        crashParticleSystem.transform.forward = normal;
        crashParticleSystem.Play();
    }

    private void HandleBoostLevelChanged(int boostLevel)
    {
        targetScale =
            Vector3.one +
            new Vector3(stretchSquashIncrement, 0, -stretchSquashIncrement) * boostLevel;

        foreach (var sparksParticle in sparksParticleList)
        {
            ChangeColor(sparksParticle.GetComponent<ParticleSystemRenderer>(), boostLevel);
        }
    }

    private void ChangeColor(ParticleSystemRenderer particleRenderer, int colorIndex)
    {
        particleRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor("_EmissionColor", vfxColorArray[colorIndex] * intensity);
        particleRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void HandleBoost(int boostLevel, float boostDuration)
    {
        targetScale = 
            Vector3.one +
            new Vector3(-stretchSquashIncrement, 0, stretchSquashIncrement) * boostLevel;

        Invoke(nameof(ResetScale), boostDuration);

        foreach (ParticleSystem boostParticle in boostParticleList)
        {
            boostParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = boostParticle.main;
            main.duration = boostDuration;

            ChangeColor(
                boostParticle.GetComponent<ParticleSystemRenderer>(),
                boostLevel);

            boostParticle.Play();
        }
    }

    private void ResetScale()
    {
        targetScale = Vector3.one;
    }

    private void HandleYaw()
    {
        currentYaw = Mathf.Lerp(
                    currentYaw,
                    targetYaw,
                    yawSpeed * Time.deltaTime
                );

        transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    private void HandleScale()
    {
        transform.localScale = 
            Vector3.Lerp(
                transform.localScale, 
                targetScale,
                stretchSquashSpeed * Time.deltaTime
            );
    }

}
