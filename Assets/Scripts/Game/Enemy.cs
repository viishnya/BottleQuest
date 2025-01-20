using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Services.CloudSave;

public class Enemy : MonoBehaviour
{
    public int health;
    public float speed;
    public int damage;
    public float startTimeBtwAttack;
    public float startStopTime;
    public float normalSpeed;

    private float stopTime;
    private Player player;
    private Animator anim;
    private GameManage game;
    private float timeBtwAttack;
    private float delta;

    private void Start()
    {
        anim = GetComponent<Animator>();
        player = FindObjectOfType<Player>();
        game = FindObjectOfType<GameManage>();
        normalSpeed = speed;
    }

    private async void Update()
    {
        if(stopTime <= 0)
        {
            speed = normalSpeed;
        }
        else
        {
            speed = 0;
            stopTime -= Time.deltaTime;
        }
        if(player.transform.position.x > transform.position.x)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        if (health <= 0)
        {
            Destroy(gameObject);
            SpawnEnemy.nowEnemies--;
            Player.Score += PlayerPrefs.GetInt("BottleCount") + 1;
            int t = PlayerPrefs.GetInt("BestScore");
            PlayerPrefs.SetInt("BestScore", Math.Max(t, Player.Score));
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> { { "BestScore", (Math.Max(t, Player.Score)).ToString() } });
            SpawnEnemy.numberEnemies++;
        }
    }

    public void TakeDamage(int damage)
    {
        stopTime = startStopTime;
        health -= damage;
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if (timeBtwAttack <= 0)
            {
                timeBtwAttack = startTimeBtwAttack;
                anim.SetTrigger("enemyAttack");
            }
            else
            {
                timeBtwAttack -= Time.deltaTime;
            }
        }
    }
    public void OnEnemyAttack()
    {
        player.health -= damage;
    }
}
