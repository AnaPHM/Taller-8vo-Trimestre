using UnityEngine;

public class Fish : MonoBehaviour
{
    [SerializeField]
    private float attackDamage = 20.0f;

    //TODO: No destruir peces usar un pool.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("Player"))
                return;

            if (collision.CompareTag("Enemy"))
            {
                Debug.Log("hit");
                GameObject enemy = collision.gameObject;
                enemy.GetComponent<IDamageable>()?.ChangeHP(-attackDamage);
                Destroy(this.gameObject);
            }
            else Destroy(this.gameObject);
        }
    }
}
