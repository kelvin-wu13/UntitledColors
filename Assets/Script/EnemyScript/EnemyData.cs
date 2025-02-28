using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    public GameObject enemyPrefab;
    public float health;
    public float damage;
    public float chargeTime;
    public float stunTime;
    public float knockBackForce;
    public float knockBackDuration;
    public float moveSpeed;
}