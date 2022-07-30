using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Text = TMPro.TextMeshProUGUI;

public class LeaderboardElement : MonoBehaviour
{
    [SerializeField] private Text rankText, usernameText, valueText;
    
    public void Initialize(int rank, string name, string value, bool highlight=false)
    {
        if (rank > 0)
            rankText.text = rank.ToString() + ".";
        usernameText.text = name;
        valueText.text = value;
        if (highlight)
        {
            rankText.text = $"<color=orange>{rankText.text}</color>";
            usernameText.text = $"<color=orange>{usernameText.text}</color>";
            valueText.text = $"<color=orange>{valueText.text}</color>";
        }
    }
}