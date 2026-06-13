using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [Header("PlayerUI")]
    [SerializeField] private Transform playerUI;

    [Header("Countdown")]
    [SerializeField] private Transform countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Time")]
    [SerializeField] private KartTimeTracker playerLapTimeTracker;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private Transform[] lapPanelList;

    [Header("Coins")]
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Drift and Boost")]
    [SerializeField] private KartDriftBoost drift;
    [SerializeField] private Image boostMeterFill;
    [SerializeField] private Image boostMeterFillBackground;
    [SerializeField]
    private Color[] boostMeterColorArray = new Color[]
                                    {
                                        new Color(1.0f, 1.0f, 1.0f),
                                        new Color(0.31f, 0.67f, 1.0f),
                                        new Color(1.0f, 0.55f, 0.0f),
                                        new Color(0.67f, 0.27f, 1.0f)
                                    };

    [Header("Position")]
    [SerializeField] private RaceManager raceManager;
    [SerializeField] private KartRaceProgress playerProgress;
    [SerializeField] private TextMeshProUGUI positionText;
    [SerializeField] private TextMeshProUGUI lapText;
    [SerializeField] private Transform centeredLapPanel;

    [Header("EndUI")]
    [SerializeField] private Transform endUI;
    [SerializeField] private Button resetButton;

    [Header("LeaderBoard")]
    [SerializeField] private Transform leaderboardListPanel;
    [SerializeField] private Transform leaderboardEntryPrefab;

    private Coroutine decreaseCurrentBoostCoroutine;

    private bool stopUpdateUI = false;

    private void Start()
    {
        lapText.text = 0 + "/" + raceManager.MaxLaps;

        countdownText.text = $"{raceManager.StartTime}";

        SubscribeToEvents();
    }

    private void Update()
    {
        if (stopUpdateUI)
            return;

        UpdateBoostMeter();

        UpdateTotalTime();

        UpdatePlayerPosition();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (drift != null) {
            drift.OnReleaseBoost += DecreaseCurrentBoost;
            drift.OnDrifCanceled += ResetBoostMeter;
        }

        if (coinManager != null)
            coinManager.OnCoinCollected += UpdateCoinsUI;

        if (playerProgress != null) {
            playerProgress.OnLapCompleted += UpdatePlayerLap;
            playerProgress.OnRaceFinished += HandleFinish;
        }

        if (raceManager != null)
        {
            raceManager.OnCountDownChanged += HandleCountDown;
            raceManager.OnRacerFinished += HandleRacerFinished;
        }

        if (resetButton != null)
            resetButton.onClick.AddListener(RestartScene);
    }

    private void UnsubscribeFromEvents()
    {
        if (drift != null)
        {
            drift.OnReleaseBoost -= DecreaseCurrentBoost;
            drift.OnDrifCanceled -= ResetBoostMeter;
        }

        if (coinManager != null)
            coinManager.OnCoinCollected -= UpdateCoinsUI;

        if (playerProgress != null)
        {
            playerProgress.OnLapCompleted -= UpdatePlayerLap;
            playerProgress.OnRaceFinished -= HandleFinish;
        }

        if (raceManager != null)
        {
            raceManager.OnCountDownChanged -= HandleCountDown;
            raceManager.OnRacerFinished -= HandleRacerFinished;
        }

    }

    private void DecreaseCurrentBoost(int currentLevel, float duration)
    {
        if (decreaseCurrentBoostCoroutine != null)
        {
            StopCoroutine(decreaseCurrentBoostCoroutine);
            decreaseCurrentBoostCoroutine = null;
        }

        decreaseCurrentBoostCoroutine = StartCoroutine(DecreaseCurrentBoostCoroutine(currentLevel, duration));
    }

    public IEnumerator DecreaseCurrentBoostCoroutine(int currentLevel, float duration)
    {

        boostMeterFill.color = boostMeterColorArray[currentLevel - 1];
        boostMeterFillBackground.color = Color.white;

        float timer = 0.0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            boostMeterFill.fillAmount = 1 - (timer / duration);
            yield return null;
        }

        ResetBoostMeter();
    }

    private void ResetBoostMeter()
    {
        boostMeterFill.fillAmount = 0.0f;
        boostMeterFillBackground.color = Color.white;
    }

    private void UpdateCoinsUI(int coins)
    {
        string text = "";

        if (coins < 10)
            text += "0";

        text += coins;

        coinText.text = text;
    }

    private void UpdatePlayerLap(KartRaceProgress _, int lap)
    {
        GameObject lapTimerPanel = lapPanelList[lap - 1].gameObject;
        lapTimerPanel.SetActive(true);
        TextMeshProUGUI lapTimerText = lapTimerPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (lapTimerText != null)
        {
            lapTimerText.text =
                $"<color=yellow>L{lap}</color>:{FormatTime(playerLapTimeTracker.LapTime)}";
            playerLapTimeTracker.ResetLapTime();
        }

        string text = playerProgress.CurrentLap + "/" + raceManager.MaxLaps;

        lapText.text = text;

        if (centeredLapPanel == null)
            return;

        TextMeshProUGUI centeredLapText = 
            centeredLapPanel.GetComponentInChildren<TextMeshProUGUI>();

        if (centeredLapText != null)
        {
            centeredLapText.text = text;
        }

        centeredLapPanel.gameObject.SetActive(true);

        float hideTime = 1.0f;
        Invoke(nameof(HideCenteredLapPanel), hideTime);
    }

    private void HideCenteredLapPanel() => centeredLapPanel.gameObject.SetActive(false);

    private void HandleFinish()
    {
        stopUpdateUI = true;

        endUI.gameObject.SetActive(true);
    }

    private void HandleCountDown(int time)
    {
        if (time == 0)
        {
            countdownText.text = $"GO";
            float hideTime = 1.0f;
            Invoke(nameof(HideCountDownPanel), hideTime);

        }
        else
            countdownText.text = $"{time}";
    }

    private void HandleRacerFinished(KartRaceProgress kartRaceProgress)
    {
        AddToLeaderBoard(kartRaceProgress);
    }

    private void AddToLeaderBoard(KartRaceProgress kartRaceProgress)
    {
        Transform leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboardListPanel);
        if (leaderboardEntry.TryGetComponent(out LeaderboardEntryUI leaderboardEntryUI))
        {
            float time = 0.0f;

            if (kartRaceProgress.TryGetComponent(out KartTimeTracker lapTimeTracker))
            {
                int position = raceManager.GetPosition(kartRaceProgress);

                time = lapTimeTracker.RaceTime;

                leaderboardEntryUI.Setup(
                    GetOrdinal(position),
                    kartRaceProgress.name,
                    FormatTime(time)
                 );
            }

        }
    }

    private void RestartScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private string GetOrdinal(int number)
    {
        if (number <= 0)
            return number.ToString();

        return number switch
        {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            _ => number + "th"
        };
    }

    private void UpdateTotalTime()
    {
        if (playerLapTimeTracker == null)
            return;

        totalTimeText.text = FormatTime(playerLapTimeTracker.RaceTime);

    }

    private void UpdateBoostMeter()
    {
        if (!drift.Drifting)
            return;

        if (decreaseCurrentBoostCoroutine != null)
        {
            StopCoroutine(decreaseCurrentBoostCoroutine);
            decreaseCurrentBoostCoroutine = null;
        }

        drift.GetCurrentBoostLevelNormalized(
            out int currentBoostLevel,
            out float currentBoostLevelProgress);

        if (currentBoostLevel < 0 || currentBoostLevel >= boostMeterColorArray.Length)
            return;

        boostMeterFill.color = boostMeterColorArray[currentBoostLevel];
        boostMeterFill.fillAmount = currentBoostLevelProgress;

        if (currentBoostLevel - 1 < 0)
            boostMeterFillBackground.color = Color.white;
        else
            boostMeterFillBackground.color = boostMeterColorArray[currentBoostLevel - 1];
    }

    private void UpdatePlayerPosition()
    {
        int position = raceManager.GetPosition(playerProgress);

        positionText.text = GetOrdinal(position) + "/" + GetOrdinal(raceManager.RacersCount);
    }

    private void HideCountDownPanel()
    {
        countdownPanel.gameObject.SetActive(false);
    }

    private static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);

        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }
}
