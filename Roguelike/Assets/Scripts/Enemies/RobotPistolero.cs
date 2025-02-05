using System.Collections;
using UnityEngine;

public class RobotPistolero : MonoBehaviour, IDamageable
{
    public float maxHP = 20.0f;
    private float currentHP;
    public float speed = 2.0f;
    public bool horizontal = false;
    public GameObject bulletPrefab;
    public Transform shootPoint;
    private bool movingRight = true;
    private float direction = 1f;
    private bool canShoot = true;
    private GameObject playerG;
    private float expValue = 20.0f;

    private bool shootmode = false;
    private bool invencible = false;

    private SpriteRenderer spriteRenderer;

    public static event System.Action OnEnemyDeath;

    // Referencias para la animación y el sprite
    // public Animator animator;
    public SpriteRenderer spriteR;

    public Door door;

    private void Awake()
    {
        playerG = GameObject.FindGameObjectWithTag("Player");
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Move();
        CheckForPlayer();
    }

    private void Move()
    {
        if (!shootmode)
        {
            if (horizontal)
            {
                transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
            }
            else
            {
                transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            movingRight = !movingRight;
            transform.position += new Vector3(0, direction * 0.1f, 0); // Que no se atore en la pared.
            Flip(); // Voltear el sprite al chocar con una pared
        }
    }

    private void CheckForPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(shootPoint.position, transform.right, 10f);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            shootmode = true;
            if (canShoot)
            {
                StartCoroutine(Shoot());
            }
        }
        else
        {
            shootmode = false;
        }
    }

    void OnDrawGizmos()
    {
        if (shootPoint != null)
        {
            Gizmos.color = Color.red; // Establece el color del Gizmo a rojo
            Vector3 direction = transform.right * 10f; // Ajusta la longitud de tu raycast según sea necesario
            Gizmos.DrawRay(shootPoint.position, direction);
        }
    }

    IEnumerator Shoot()
    {
        canShoot = false;

        // Obtener una bala del pool en lugar de instanciar una nueva
        GameObject bullet = BulletPool.Instance.GetBullet();

        // Configurar la posición y la velocidad de la bala
        bullet.transform.position = shootPoint.position;

        // GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = transform.right * 10f; // Ajusta la velocidad según sea necesario
        yield return new WaitForSeconds(3);
        canShoot = true;
    }

    public void ChangeHP(float amount)
    {
        if (!invencible)
        {
            invencible = true;
            currentHP += amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            if (currentHP <= 0)
                HandleDeath();

            StartCoroutine(Vencible());
        }
    }

    // Modificar la escala en lugar de rotarlo
    void Flip()
    {
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
        direction *= -1;
    }

    public void HandleDeath()
    {
        // Reproducir animación de muerte
        // if (animator != null)
        // {
        //     animator.SetTrigger("Die");
        // }

        // Desactivar el sprite para que desaparezca visualmente
        if (spriteR != null)
        {
            spriteR.enabled = false;
        }

        playerG.GetComponent<Movement>().AddEXP(expValue);

        if (door != null)
            door.locked = false;

        // Destruir el objeto después de un pequeño retraso
        Destroy(gameObject);
        OnEnemyDeath?.Invoke();
    }

    private IEnumerator Vencible()
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
            yield return new WaitForSeconds(0.2f);
        }

        invencible = false;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
    }
}