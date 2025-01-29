using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHit : MonoBehaviour
{
    [SerializeField]
    private float attackDamage = 10.0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("Enemy"))
            {
                Debug.Log("hit");
                GameObject enemy = collision.gameObject;
                enemy.GetComponent<IDamageable>()?.ChangeHP(-attackDamage);
            }
        }
    }
}
