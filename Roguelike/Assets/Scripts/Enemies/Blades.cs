using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blades : MonoBehaviour
{
    [SerializeField]
    private float attackDamage = 20.0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("Player"))
            {
                Debug.Log("Get Wrecked");
                GameObject enemy = collision.gameObject;
                enemy.GetComponent<IDamageable>()?.ChangeHP(-attackDamage);
            }
        }
    }
}
