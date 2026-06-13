using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private TextMeshProUGUI positionText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI timeText;

    public void Setup(string position, string racerName, string time)
    {
        positionText.text = position;
        nameText.text = racerName;
        timeText.text = time;
    }
}