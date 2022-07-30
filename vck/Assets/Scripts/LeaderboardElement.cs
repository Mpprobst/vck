using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Text = TMPro.TextMeshProUGUI;

public class LeaderboardElement : MonoBehaviour
{
    [SerializeField] private Text rankText, usernameText, valueText;
    
    public void Initialize(int rank, string name, string value)
    {
        rankText.text = rank.ToString() + ".";
        usernameText.text = name;
        valueText.text = value;
    }
}
