using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyData enemyData;

    private void Start()
    {
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        GameObject enemyPrefab = enemyData.enemyPrefab;
        Instantiate(enemyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }
}