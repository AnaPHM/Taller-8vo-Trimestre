using System.Collections;
using UnityEngine;

public class RobotLuchador : MonoBehaviour, IDamageable
{
    public float maxHP = 20.0f;
    public float attackDamage = 20.0f;
    public float speed = 3.0f;
    private Transform player;
    public float detectionRange = 10.0f;
    public float attackDistance = 2.5f;
    private float currentHP;
    private bool isAttacking = false;
    private bool canAttack = true;
    private GameObject playerG;
    private float expValue = 20.0f;

    private bool invencible = false;

    private SpriteRenderer spriteRenderer;

    public static event System.Action OnEnemyDeath;
    
    public AudioClip attackRL;

    // Referencias para la animacion y el sprite
    public Animator animator;
    public SpriteRenderer spriteR;
    public GameObject attackHitbox;

    [SerializeField] private ParticleSystem attackParticlePrefab; // Prefab de part�culas de ataque
    [SerializeField] private Transform attackOrigin; // Posici�n de origen del ataque

    public Door door;

    // Referencia al AudioSource
    public AudioSource audioSource;

    private void Awake()
    {
        currentHP = maxHP;
        playerG = GameObject.FindGameObjectWithTag("Player");
        player = playerG.transform;
        attackHitbox.SetActive(false);
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Obtener el componente AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);


            if (distanceToPlayer < detectionRange && distanceToPlayer > attackDistance && canAttack)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                transform.position += (Vector3)direction * speed * Time.deltaTime;
                // Determinar la direcci�n del movimiento para las animaciones
                UpdateAnimationDirection(direction);
            }
            else if (distanceToPlayer <= attackDistance && canAttack)
                StartCoroutine(ActivateHitbox());

        }
    }

    IEnumerator ActivateHitbox()
    {
        canAttack = false;
        isAttacking = true;

        audioSource.PlayOneShot(attackRL);

        // Establece el Trigger para iniciar la animaci�n de ataque
        if (animator != null)
        {
            animator.SetTrigger("Attacking");
        }

        yield return new WaitForSeconds(0.5f);

        // Instanciar part�culas en el origen del ataque
        if (attackParticlePrefab != null && attackOrigin != null)
        {
            ParticleSystem particles = Instantiate(attackParticlePrefab, attackOrigin.position, Quaternion.identity);
            Vector3 attackDirection = (player.position - attackOrigin.position).normalized;

            // Ajustar rotaci�n de part�culas hacia el jugador
            if (attackDirection != Vector3.zero)
            {
                particles.transform.rotation = Quaternion.LookRotation(Vector3.forward, attackDirection);
            }

            particles.Play();
            Destroy(particles.gameObject, particles.main.duration + 0.1f);
        }

        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(1.0f);

        attackHitbox.SetActive(false);
        yield return new WaitForSeconds(1.0f);

        isAttacking = false;
        canAttack = true;
    }

    public void ChangeHP(float amount)
    {
        if (!invencible)
        {
            invencible = true;
            currentHP += amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            // Reproducir el sonido de da�o
            if (audioSource != null && amount < 0) // Solo reproducir si el da�o es negativo (es decir, el enemigo est� siendo da�ado)
            {
                audioSource.Play();
            }

            if (currentHP <= 0)
                HandleDeath();

            StartCoroutine(Vencible());
        }
    }

    public void HandleDeath()
    {
        // MarcoAntonio
        // Reproducir animacion de muerte
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

        // Destruir el objeto despu�s de un peque�o retraso 
        //quite el retraso para hacer test al spawn lo devolvemos cuando tengamos animaciones de muertes.
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

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
    }

    void UpdateAnimationDirection(Vector2 direction)
    {
        // Si el enemigo est� atacando, reproduce �nicamente la animaci�n de ataque
        if (isAttacking)
        {
            animator.Play("Enemy_Attack");
            return;
        }

        // Determinar si el enemigo se est� moviendo
        bool isMoving = direction != Vector2.zero;

        // Actualizar par�metros del Animator
        animator.SetBool("EnemyMove", isMoving);

        if (isMoving)
        {
            // Comprobar direcci�n de movimiento y reproducir la animaci�n correspondiente
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Movimiento lateral (izquierda o derecha)
                if (direction.x > 0)
                {
                    // Movimiento hacia la derecha
                    animator.Play("Enemy_Right_Walk");
                    spriteRenderer.flipX = false;
                }
                else if (direction.x < 0)
                {
                    // Movimiento hacia la izquierda
                    animator.Play("Enemy_Left_Walk");
                    spriteRenderer.flipX = false;
                }
            }
            //else if (direction.y > 0)
            //{
            //    // Movimiento hacia arriba
            //    animator.Play("Enemy_Walk_Back");
            //    //spriteRenderer.flipX = false; // No voltear el sprite
            //}
            //else if (direction.y < 0)
            //{
            //    // Movimiento hacia abajo
            //    animator.Play("Enemy_Front_Walk");
            //    //spriteRenderer.flipX = false; // No voltear el sprite
            //}
        }
        else
        {
            animator.Play("Enemy_Idle");
            spriteRenderer.flipX = true;
        }
    }
}