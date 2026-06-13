using System;
using UnityEngine;

public class KartRaceProgress : MonoBehaviour
{

    public event Action<KartRaceProgress, int> OnLapCompleted;

    public event Action OnRaceFinished;

    [Header("References")]
    [SerializeField] private KartTrackFollower follower;

    public int CurrentLap { get; private set; }
    public float Progress { get; private set; }

    private int previousTrackPoint;
    private bool updateProgress = true;

    private bool raceStarted;

    private void Update()
    {
        if (!updateProgress)
            return;

        UpdateLap();
        UpdateProgress();
    }

    public void FinishRace()
    {
        updateProgress = false;
        OnRaceFinished?.Invoke();
    }

    private void UpdateLap()
    {
        int current = follower.CurrentTrackPoint;
        int last = follower.TrackPointsCount - 1;


        bool crossedFinishLine =
            previousTrackPoint == last &&
            current == 0;

        if (!raceStarted && crossedFinishLine)
        {
            raceStarted = true;
            previousTrackPoint = current;
            return;
        }

        if (crossedFinishLine)
        {
            CurrentLap++;
            OnLapCompleted?.Invoke(this, CurrentLap);
        }

        previousTrackPoint = current;
    }

    private void UpdateProgress()
    {
        if (!raceStarted)
            return;

        Progress =
            CurrentLap *
            follower.TrackPointsCount +
            follower.GetTrackProgress();
    }
}