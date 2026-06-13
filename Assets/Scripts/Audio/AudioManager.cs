using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private KartRaceProgress playerRaceProgress;
    [SerializeField] private RaceManager raceManager;

    [Header("Properties")]
    [SerializeField] private float lastLapPitch = 1.1f;
    [SerializeField] private float raceEndedPitch = .9f;

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnSubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if(playerRaceProgress != null)
            playerRaceProgress.OnLapCompleted += CheckForFinalLap;
    }

    private void UnSubscribeFromEvents()
    {
        if (playerRaceProgress != null)
            playerRaceProgress.OnLapCompleted -= CheckForFinalLap;
    }

    private void CheckForFinalLap(KartRaceProgress progress, int lap)
    {
        if (lap + 1 == raceManager.MaxLaps)
            backgroundAudioSource.pitch = lastLapPitch;

        if(lap == raceManager.MaxLaps)
            backgroundAudioSource.pitch = raceEndedPitch;
    }
}
