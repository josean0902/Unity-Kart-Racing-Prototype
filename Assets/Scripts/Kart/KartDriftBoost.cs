using System;
using UnityEngine;

public class KartDriftBoost : MonoBehaviour
{
    public event Action<int> OnBoostLevelChanged;
    public event Action<int, float> OnReleaseBoost;
    public event Action OnDrifCanceled;

    [System.Serializable]
    public struct BoostData
    {
        public float ChargeTime;
        public float BoostTime;
    }

    [field:Header("Drift and Boost")]
    [field:SerializeField] 
    public float MinDriftAngle { get; private set; } = 5.0f;
    [field: SerializeField]
    public float BoostMultiplier { get; private set; } = 1.5f;
    [SerializeField] private float normalGrip = 2.0f;
    [SerializeField] private float driftGrip = 0.5f;
    [SerializeField] private float gripFactor = 1.0f;

    [SerializeField]
    private BoostData[] boostLevels = new BoostData[]
    {
        new BoostData
        {
            ChargeTime = 0.8f,
            BoostTime = 0.6f
        },
        new BoostData
        {
            ChargeTime = 2.0f,
            BoostTime = 1.6f
        },
        new BoostData
        {
            ChargeTime = 4.5f,
            BoostTime = 2.5f
        }
    };

    public float CurrentGripFactor { get; internal set; }
    public bool Drifting { get; private set; }

    private int currentLevel;

    private float driftTimer;

    public void StartDrift()
    {
        Drifting = true;
        driftTimer = 0f;
        SetBoostLevel(0);
    }

    public void UpdateDrift()
    {

        float targetGrip = Drifting ? driftGrip : normalGrip;

        CurrentGripFactor = Mathf.Lerp(
            CurrentGripFactor,
            targetGrip,
            gripFactor * Time.deltaTime
        );

        if (!Drifting) 
            return;

        driftTimer += Time.deltaTime;

        int newLevel = CalculateLevelFromTime(driftTimer);
        SetBoostLevel(newLevel);
    }

    public float ReleaseBoost()
    {
        Drifting = false;

        if (currentLevel <= 0)
        {
            CancelDrift();
            return 0f;
        }

        int index = Mathf.Clamp(currentLevel - 1, 0, boostLevels.Length - 1);
        float duration = boostLevels[index].BoostTime;

        OnReleaseBoost?.Invoke(currentLevel, duration);

        return duration;
    }

    public void CancelDrift()
    {
        Drifting = false;
        driftTimer = 0f;
        SetBoostLevel(0);

        OnDrifCanceled?.Invoke();
    }

    public void GetCurrentBoostLevelNormalized(
        out int currentBoostLevel,
        out float currentBoostLevelProgress)
    {

        currentBoostLevel = currentLevel;

        if (currentLevel >= boostLevels.Length)
        {
            currentBoostLevelProgress = 1.0f;
            return;
        }

        float previousChargeTime = 0.0f;
        if (currentLevel - 1 >= 0)
        {
            previousChargeTime = boostLevels[currentBoostLevel - 1].ChargeTime;
        }

        float currentChargeTime = boostLevels[currentBoostLevel].ChargeTime;

        float chargeTimeDifference = currentChargeTime - previousChargeTime;
        float currentProgress = driftTimer - previousChargeTime;

        currentBoostLevelProgress = currentProgress / chargeTimeDifference;
    }

    private void SetBoostLevel(int newLevel)
    {
        if (currentLevel == newLevel) return;

        currentLevel = newLevel;
        OnBoostLevelChanged?.Invoke(currentLevel);
    }

    private int CalculateLevelFromTime(float time)
    {
        int level = 0;

        for (int i = 0; i < boostLevels.Length; i++)
        {
            if (time >= boostLevels[i].ChargeTime)
                level = i + 1;
        }

        return level;
    }
}