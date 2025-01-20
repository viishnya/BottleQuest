using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using System.Globalization;
using System;

public class MainMenu : Panel
{

    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] private Button logoutButton = null;
    [SerializeField] private Button leaderboardsButton = null;
    [SerializeField] private Button playButton = null;

    public GameObject Main;
    public GameObject Game;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        logoutButton.onClick.AddListener(SignOut);
        leaderboardsButton.onClick.AddListener(Leaderboards);
        playButton.onClick.AddListener(Play);
        base.Initialize();
    }

    public override void Open()
    {
        UpdatePlayerNameUI();
        base.Open();
    }

    private void SignOut()
    {
        MenuManager.Singleton.SignOut();
    }

    private void UpdatePlayerNameUI()
    {
        nameText.text = AuthenticationService.Instance.PlayerName;
    }

    private void Leaderboards()
    {
        Close();
        PanelManager.Open("leaderboards");
    }

    private void Play()
    {
        Main.SetActive(false);
        Game.SetActive(true);
    }
}
