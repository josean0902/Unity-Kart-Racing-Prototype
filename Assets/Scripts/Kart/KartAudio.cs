using System;
using UnityEngine;

public class KartAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KartController kartController;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private AudioSource driftAudioSource;
    [SerializeField] private AudioSource boostExplosionAudioSource;
    [SerializeField] private AudioSource boostNoiseAudioSource;
    [SerializeField] private AudioSource crashAudioSource;

    [Header("Engine")]
    [SerializeField] private float basePitch = 0.5f;
    [SerializeField] private float maxPitch = 1.5f;
    [SerializeField] private float pitchSpeed = 5f;

    private void Start()
    {
        SubscribeToEvents();
    }
        
    private void Update()
    {
        UpdateMotorPitch();
    }

    private void OnDestroy()
    {
        UnSubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (kartController != null)
        {
            kartController.OnDriftStarted += EnableDriftAudio;
            kartController.OnDriftEnded += DisableDriftAudio;

            kartController.OnBoostStarted += EnableBoostAudio;
            kartController.OnBoostEnded += DisableBoostAudio;

            kartController.OnCrash += PlayCrashAudio;
        }
    }

    private void UnSubscribeFromEvents()
    {
        if (kartController != null)
        {
            kartController.OnDriftStarted -= EnableDriftAudio;
            kartController.OnDriftEnded -= DisableDriftAudio;

            kartController.OnBoostStarted -= EnableBoostAudio;
            kartController.OnBoostEnded -= DisableBoostAudio;

            kartController.OnCrash -= PlayCrashAudio;
        }
    }

    private void EnableDriftAudio(KartDriftSide side) => driftAudioSource?.Play();
    private void DisableDriftAudio() => driftAudioSource?.Stop();

    private void EnableBoostAudio()
    {
        boostExplosionAudioSource?.Play();
        boostNoiseAudioSource?.Play();
    }
    private void DisableBoostAudio() => boostNoiseAudioSource?.Stop();

    private void PlayCrashAudio(Vector3 surfacePosition, Vector3 surfaceNormal)
    {
        if (engineAudioSource == null)
            return;

        crashAudioSource.transform.position = surfacePosition;
        crashAudioSource.Play();
    }

    private void UpdateMotorPitch()
    {
        if (engineAudioSource == null || kartController == null)
            return;

        float targetPitch = basePitch +
            (maxPitch - basePitch) *
            kartController.CurrentSpeedNormalized;

        engineAudioSource.pitch = 
            Mathf.Lerp(engineAudioSource.pitch,
                targetPitch,
                pitchSpeed * Time.deltaTime
            );
    }
}
