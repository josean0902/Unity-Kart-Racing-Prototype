using System;
using UnityEngine;

public class KartTimeTracker : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private RaceManager raceManager;

    public float RaceTime {  get; private set; }
    public float LapTime {  get; private set; }

    private bool timerEnabled;

    private void Start()
    {
        timerEnabled = false;
        RaceTime = 0f;
        LapTime = 0f;

        SubscribeToEvents();

    }

    private void Update()
    {
        if (!timerEnabled)
            return;

        RaceTime += Time.deltaTime;
        LapTime += Time.deltaTime;
    }

    private void OnDestroy()
    {
        UnSubscribeFromEvents();
    }

    public void ResetLapTime() => LapTime = 0;

    private void SubscribeToEvents()
    {
        if (raceManager == null)
            return;

        raceManager.OnRaceStarted += StartRace;
    }

    private void UnSubscribeFromEvents()
    {
        if (raceManager == null)
            return;

        raceManager.OnRaceStarted -= StartRace;
    }

    private void StartRace() => timerEnabled = true;

}
