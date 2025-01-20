using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnEnemy;
    [SerializeField] private Transform[] spawnPoint;

    public int startNumberEnemies;
    public float startSpawnerInterval;
    static public int numberEnemies;
    static public int nowEnemies;
    public GameObject Spawner;

    private float spawnerInterval;
    private int randEnemy;
    private int randPoint;

    void Start()
    {
        numberEnemies = startNumberEnemies;
    }

    void Update()
    {
        if(spawnerInterval <= 0 && nowEnemies < numberEnemies)
        {
            randEnemy = Random.Range(0, spawnEnemy.Length);
            randPoint = Random.Range(0, spawnPoint.Length);
            Instantiate(spawnEnemy[randEnemy], spawnPoint[randPoint].transform.position, Quaternion.identity, Spawner.transform);
            spawnerInterval = startSpawnerInterval;
            nowEnemies++;
        }
        else
        {
            spawnerInterval -= Time.deltaTime;
        }
    }
}
