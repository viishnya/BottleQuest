using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class TimeOut : MonoBehaviour
{
    public Button butStart;
    public TextMeshProUGUI wait;

    void Update()
    {
        if (PlayerPrefs.HasKey("DateTime"))
        {
            string stored = PlayerPrefs.GetString("DateTime");
            DateTime old = DateTime.ParseExact(stored, "u", CultureInfo.InvariantCulture);
            TimeSpan timePassed = DateTime.UtcNow - old;
            int secondPassed = 7200 - (int)timePassed.TotalSeconds;
            if (secondPassed > 0)
            {
                butStart.gameObject.SetActive(false);
                string wtext = "";
                if (secondPassed / 3600 > 0) wtext += (secondPassed / 3600).ToString() + ":";
                if ((secondPassed % 3600) / 60 > 0 || secondPassed / 3600 > 0) wtext += ((secondPassed % 3600) / 60).ToString() + ":";
                wtext += (secondPassed % 60).ToString();
                wait.text = wtext;
                wait.gameObject.SetActive(true);
            }
            else
            {
                butStart.gameObject.SetActive(true);
                wait.gameObject.SetActive(false);
            }
        }
    }
}
