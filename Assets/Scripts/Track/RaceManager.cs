using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public event Action OnRaceStarted;

    public event Action<KartRaceProgress> OnRacerFinished;

    public event Action<int> OnCountDownChanged;

    [SerializeField] private List<KartRaceProgress> raceProgressList;
    [field: SerializeField]
    public int MaxLaps { get; private set; } = 3;
    [field: SerializeField] 
    public int StartTime { get; private set; } = 3;

    public int RacersCount => raceProgressList.Count;

    private void Start()
    {
        SubscribeToEvents();

        StartCoroutine(StartRaceRoutine());
    }

    private void Update()
    {
        UpdatePositions();
    }

    private void OnDestroy()
    {
        UnSubscribeFromEvents();
    }

    public int GetPosition(KartRaceProgress target)
    {
        int index = raceProgressList.IndexOf(target);

        if (index == -1)
            return -1;

        return index + 1;
    }

    private void SubscribeToEvents()
    {
        foreach (var raceProgress in raceProgressList)
        {
            raceProgress.OnLapCompleted += CheckRaceFinished;
        }
    }

    private void UnSubscribeFromEvents()
    {
        foreach (var raceProgress in raceProgressList)
        {
            raceProgress.OnLapCompleted -= CheckRaceFinished;
        }
    }

    private void CheckRaceFinished(KartRaceProgress raceProgress, int lapCompleted)
    {
        if (lapCompleted != MaxLaps)
            return;

        raceProgress.FinishRace();

        OnRacerFinished?.Invoke(raceProgress);
    }

    private IEnumerator StartRaceRoutine()
    {
        int count = Mathf.CeilToInt(StartTime);

        while (count > 0)
        {
            OnCountDownChanged?.Invoke(count);

            yield return new WaitForSeconds(1f);

            count--;
        }

        OnCountDownChanged?.Invoke(0);

        OnRaceStarted?.Invoke();
    }

    private void UpdatePositions()
    {
        raceProgressList = raceProgressList
            .OrderByDescending(r => r.Progress)
            .ToList();
    }
}