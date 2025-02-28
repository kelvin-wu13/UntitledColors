using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float damage;
    public float lifetime = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CrimsonCharger enemy = other.GetComponent<CrimsonCharger>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}
