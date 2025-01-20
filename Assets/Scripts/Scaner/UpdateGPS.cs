using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Globalization;
using TMPro;
using System;

public class UpdateGPS : MonoBehaviour
{
    public TextMeshProUGUI coordinates;
    public TextAsset garbageCans;
    public GameObject Main;
    public GameObject Scaner;
    public GameObject BG;
    public GameObject Canvas;

    private string[] cans;

    public void Start()
    {
        cans = garbageCans.text.Split('\n');
        bool flag = false;
        for (int i = 0; i < cans.Length; i++)
        {
            string[] xy = cans[i].Split(' ');
            float tx = (Mathf.Round(GPS.Instance.latitude * 10000)), ty = (Mathf.Round(GPS.Instance.longitude * 10000));
            if ((tx == float.Parse(xy[0], CultureInfo.InvariantCulture.NumberFormat)) && (ty == float.Parse(xy[1], CultureInfo.InvariantCulture.NumberFormat)))
            {
                flag = true;
                break;
            }
        }
        if (flag)
        {
            SetDateTime();
            Main.SetActive(false);
            Canvas.SetActive(true);
            BG.SetActive(false);
            Scaner.GetComponent<RunInferenceYOLO>().Start();
        }
        else if (GPS.Instance.latitude == 0.0f && GPS.Instance.longitude == 0.0f)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Ќевозможно определить местоположение устройства", "Ok");
        }
        else
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "ѕохоже, вы не находитесь р€дом с баком", "Ok");
        }
        coordinates.text = (Mathf.Round(GPS.Instance.latitude * 10000) / 10000).ToString() + ' ' + (Mathf.Round(GPS.Instance.longitude * 10000) / 10000).ToString();
    }

    private void SetDateTime()
    {
        DateTime value = DateTime.UtcNow;
        string convertedToString = value.ToString("u", CultureInfo.InvariantCulture);
        PlayerPrefs.SetString("DateTime", convertedToString);
    }
}
