using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Leaderboards;

public class LeaderboardsMenu : Panel
{
    [SerializeField] private int playersPerPage = 7;
    [SerializeField] private LeaderboardsPlayerItem playerItemPrefab = null;
    [SerializeField] private RectTransform playersContainer = null;
    [SerializeField] public TextMeshProUGUI pageText = null;
    [SerializeField] private Button nextButton = null;
    [SerializeField] private Button prevButton = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button addScoreButton = null;

    private int currentPage = 1;
    private int totalPages = 0;

    public override void Open()
    {
        pageText.text = "-";
        nextButton.interactable = false;
        prevButton.interactable = false;
        base.Open();
        ClearPlayersList();
        currentPage = 1;
        totalPages = 0;
        LoadPlayers(1);
    }

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        ClearPlayersList();
        closeButton.onClick.AddListener(ClosePanel);
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
        addScoreButton.onClick.AddListener(AddScore);
        base.Initialize();
    }

    public void AddScore()
    {
        AddScoreAsync(PlayerPrefs.GetInt("BestScore"));
    }

    private async void AddScoreAsync(int score)
    {
        addScoreButton.interactable = false;

        try
        {
            var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync("leaderboard", score);
            LoadPlayers(currentPage);
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }

        addScoreButton.interactable = true;
    }

    private async void LoadPlayers(int page)
    {
        nextButton.interactable = false;
        prevButton.interactable = false;

        try
        {
            GetScoresOptions options = new GetScoresOptions();
            options.Offset = (page - 1) * playersPerPage;
            options.Limit = playersPerPage;
            var scores = await LeaderboardsService.Instance.GetScoresAsync("leaderboard", options);
            ClearPlayersList();
            for (int i = 0; i < scores.Results.Count; i++)
            {
                LeaderboardsPlayerItem item = Instantiate(playerItemPrefab, playersContainer);
                item.Initialize(scores.Results[i]);
            }
            totalPages = Mathf.CeilToInt(scores.Total / scores.Limit);
            currentPage = page;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
        pageText.text = currentPage.ToString() + "/" + (totalPages + 1).ToString();
        nextButton.interactable = currentPage <= totalPages;
        prevButton.interactable = currentPage > 1;
    }

    private void NextPage()
    {
        if (currentPage > totalPages)
        {
            LoadPlayers(1);
        }
        else
        {
            LoadPlayers(currentPage + 1);
        }
    }

    private void PrevPage()
    {
        if (currentPage - 1 <= 0)
        {
            LoadPlayers(totalPages);
        }
        else
        {
            LoadPlayers(currentPage - 1);
        }
    }

    private void ClosePanel()
    {
        Close();
        PanelManager.Open("main");
    }

    private void ClearPlayersList()
    {
        LeaderboardsPlayerItem[] items = playersContainer.GetComponentsInChildren<LeaderboardsPlayerItem>();
        if(items != null)
        {
            for(int i = 0; i < items.Length; i++)
            {
                Destroy(items[i].gameObject);
            }
        }
    }
}
