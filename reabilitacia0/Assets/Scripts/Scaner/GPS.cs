using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using TMPro;

public class GPS : MonoBehaviour
{
    public static GPS Instance { set; get; }

    public GameObject Scaner;
    public float latitude;
    public float longitude;

    public void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera)) Permission.RequestUserPermission(Permission.Camera);
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(StartLocationService());
    }

    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Ќа устройстве отключена геолокаци€", "Ok");
            Scaner.GetComponent<UpdateGPS>().Start();
            yield break;
        }

        Input.location.Start();
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Ќевозможно определить местоположение устройства", "Ok");
            Scaner.GetComponent<UpdateGPS>().Start();
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Ќевозможно определить местоположение устройства", "Ok");
            Scaner.GetComponent<UpdateGPS>().Start();
            yield break;
        }

        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;

        Scaner.GetComponent<UpdateGPS>().Start();
        yield break;
    }
}
