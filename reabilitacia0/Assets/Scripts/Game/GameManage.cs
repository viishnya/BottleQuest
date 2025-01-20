using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManage : MonoBehaviour
{
    public GameObject Game;
    public GameObject MainMenu;
    public GameObject PausePanel;
    public TextMeshProUGUI textScore;
    public Button play;

    private Player player;

    public void Pause()
    {
        Time.timeScale = 0;
        textScore.gameObject.SetActive(false);
        play.gameObject.SetActive(true);
        PausePanel.SetActive(true);
    }

    public void Play()
    {
        Time.timeScale = 1;
        textScore.gameObject.SetActive(true);
        play.gameObject.SetActive(false);
        PausePanel.SetActive(false);
    }

    public void Exit()
    {
        Time.timeScale = 1;
        player = FindObjectOfType<Player>();

        player.health = player.thealth;
        Player.Score = 0;
        player.transform.position = player.start;
        SpawnEnemy.nowEnemies = 0;
        SpawnEnemy.numberEnemies = 4;
        foreach (var obj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(obj);
        }

        Game.SetActive(false);
        PausePanel.SetActive(false);
        MainMenu.SetActive(true);
    }
}
