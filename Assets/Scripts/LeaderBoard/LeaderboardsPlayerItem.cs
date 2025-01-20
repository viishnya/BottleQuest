using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Leaderboards.Models;
using UnityEngine.UI;
using System;
using Unity.Services.CloudSave;
using System.Drawing;

public class LeaderboardsPlayerItem : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] public TextMeshProUGUI scoreText = null;

    private LeaderboardEntry player = null;


    public void Initialize(LeaderboardEntry player)
    {
        this.player = player;
        nameText.text = (player.Rank + 1).ToString() + ". " + player.PlayerName.Substring(0, player.PlayerName.LastIndexOf('#'));
        scoreText.text = player.Score.ToString();
    }
}
