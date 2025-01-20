using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.CloudSave;
using System;
using Unity.Services.Authentication;

public class LoadScore : MonoBehaviour
{
    public TextMeshProUGUI displayScore;
    public TextMeshProUGUI nameText;

    private void Start()
    {
        Load();
    }

    public async void Load()
    {
        var client = CloudSaveService.Instance.Data;
        var query2 = await client.LoadAsync(new HashSet<string> { "BottleCount" });
        if (query2.TryGetValue("BottleCount", out var key2))
        {
            PlayerPrefs.SetInt("BottleCount", Int32.Parse(query2["BottleCount"]));
        }
        else
        {
            PlayerPrefs.SetInt("BottleCount", 0);
        }

        var query1 = await client.LoadAsync(new HashSet<string> { "BestScore" });
        if (query1.TryGetValue("BestScore", out var key1))
        {
            PlayerPrefs.SetInt("BestScore", Int32.Parse(query1["BestScore"]));
        }
        else
        {
            PlayerPrefs.SetInt("BestScore", 0);
        }

        displayScore.text = PlayerPrefs.GetInt("BottleCount").ToString() + " : " + PlayerPrefs.GetInt("BestScore").ToString();
        if (PlayerPrefs.HasKey("Name") && PlayerPrefs.GetString("Name") != "")
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(PlayerPrefs.GetString("Name"));
            string name = AuthenticationService.Instance.PlayerName;
            nameText.text = name.Substring(0, name.LastIndexOf('#'));
        }
    }
}
