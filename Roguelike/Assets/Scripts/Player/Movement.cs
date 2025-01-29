using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class Movement : MonoBehaviour, IDamageable, IExperience
{
    [SerializeField]
    private float speed = 5.0f;
    [SerializeField]
    private float dashSpeedMultiplier = 3f; // Multiplicador de la velocidad de dash puede que este algo alto.
    [SerializeField]
    private float dashDuration = 0.2f;
    [SerializeField]
    private float dashCooldown = 1.5f;
    [SerializeField]
    private float attackDuration = 1f;
    [SerializeField]
    private float secundaryCooldown = 6f;
    [SerializeField]
    private GameObject hitbox;
    [SerializeField]
    private BoxCollider2D hit;
    [SerializeField]
    private GameObject fishPrefab;
    [SerializeField]
    private Transform shootPoint;
    [SerializeField]
    private Transition_Manager transition_Manager;
    [SerializeField]
    private Canvas pause;

    //MarcoAntonio
    [SerializeField] private ParticleSystem levelUpEffect; //Efecto de particulas
    [SerializeField] private Image secondaryAbilityImage; //Imagen de habilidad secundaria
    [SerializeField] private Sprite secondaryAbilityActive; //Sprite no tachado
    [SerializeField] private Sprite secondaryAbilityInactive; //Sprite tachado
    [SerializeField] private Sprite secondaryAbilityCooldown; //Sprite en tonos grises

    [SerializeField] private ParticleSystem attackParticlePrefab;
    [SerializeField] private Transform hitboxCenter;
    //---------------------------

    private Transform me;
    private Vector2 movementInput;
    private Vector2 lastDirection = Vector2.right;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;
    private float exp = 0.0f;
    private int level = 0;
    private bool secundaryactivate = false;
    private bool canShoot = true;
    private bool ispaused;


    private float hp;
    private float maxHp = 100f;
    private bool invencible = false;
    public ProgressBar Pbh;
    public ProgressBar Pbe;

    bool attacking = false;

    private SpriteRenderer spriteRenderer;

    public Image interact;

    //MarcoAntonio
    //Agregar el controlador de animacion y asignar el GameOver screen desde el inspector
    public Animator animator;

    private bool death = false;
    //----------------------------------------

    public AudioSource audioSource;
    public AudioClip attack;
    public AudioClip zoom;
    public AudioClip shoot;

    void Start()
    {
        me = transform;
        InitializeValues();
    }

    void InitializeValues()
    {
        hp = maxHp;
        exp = 0;
        Pbh.BarValue = hp;
        Pbe.BarValue = exp;

        pause.gameObject.SetActive(false);
        spriteRenderer = GetComponent<SpriteRenderer>();
        isDashing = false;
        secundaryactivate = false;
        death = false;

        interact.gameObject.SetActive(false);

        //MarcoAntonio
        if (levelUpEffect)
        {
            var main = levelUpEffect.main;
            main.loop = false;
            main.playOnAwake = false;
        }
        UpdateSecondaryAbilityImage();
        //--------------------------
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        if (death) { return; }
        movementInput = context.ReadValue<Vector2>().normalized;
        if (movementInput != Vector2.zero)
        {
            lastDirection = movementInput; // Guarda la última dirección no cero
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (death) { return; }
        if (context.performed && Time.time >= nextDashTime && !isDashing)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        audioSource.PlayOneShot(zoom);
        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (death) { return; }
        if (context.performed)
        {
            if (!attacking)
                StartCoroutine(ActivateHitbox());
        }
    }

    public void OnSecundario(InputAction.CallbackContext context)
    {
        if (death) { return; }
        if (secundaryactivate)
        {
            if (context.performed && canShoot && !attacking)
                StartCoroutine(Shoot());
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (death) { return; }
        if (context.performed)
        {
            ispaused = !ispaused;

            if (ispaused)
            {
                pause.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
            if (!ispaused)
            {
                pause.gameObject.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }

    public void ButtonPause()
    {
        if (death) { return; }

        ispaused = !ispaused;

        if (ispaused)
        {
            pause.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }
        if (!ispaused)
        {
            pause.gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void MainMenuTime()
    {
        ispaused = !ispaused;
        pause.gameObject.SetActive(false);
        transition_Manager.LoadMainMenu();
        Time.timeScale = 1f;
    }

    public void ChangeHP(float amount)
    {
        if (!invencible)
        {
            invencible = true;
            hp += amount;
            hp = Mathf.Clamp(hp, 0, maxHp);

            if (hp <= 0)
            {
                HandleDeath();
            }

            StartCoroutine(Vencible());
        }
    }

    public void HandleDeath()
    {
        Debug.Log("El personaje ha muerto.");

        //Reproduce la animacion de muerte
        //animator.SetTrigger("Die");


        spriteRenderer.enabled = false;

        death = true;

        StartCoroutine(ShowGameOverScreen());

    }

    private IEnumerator ShowGameOverScreen()
    {
        //Ajustar el tiempo para que se vea la animacion de muerte completa.
        yield return new WaitForSeconds(2.0f);

        transition_Manager.LoadGameover();
    }

    private IEnumerator ActivateHitbox()
    {
        attacking = true;
        hit.enabled = true;

        audioSource.PlayOneShot(attack);

        //Instanciar particulas en el centro del hitbox
        if (attackParticlePrefab != null && hitboxCenter != null)
        {
            ParticleSystem particles = Instantiate(attackParticlePrefab, hitboxCenter.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration); // Destruir después de la duración del efecto
        }
        //

        yield return new WaitForSeconds(attackDuration);
        hit.enabled = false;
        attacking = false;
    }


    IEnumerator Shoot()
    {
        canShoot = false;

        //MarcoAntonio-----------------------
        UpdateSecondaryAbilityImage(true); //Cambia la imagen a gris
        //--------------------------

        audioSource.PlayOneShot(shoot);
        GameObject fish = Instantiate(fishPrefab, shootPoint.position, Quaternion.identity);
        fish.GetComponent<Rigidbody2D>().velocity = transform.right * 10f;
        yield return new WaitForSeconds(secundaryCooldown);
        canShoot = true;

        //MarcoAntonio-----------------------
        UpdateSecondaryAbilityImage(); //Restaura la imagen
        //--------------------------
    }


    private IEnumerator Vencible()
    {
        //Gracias por el comentario profe la neta no se me habia ocurrido jajajaja
        for (int i = 0; i < 6; i++)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
            yield return new WaitForSeconds(0.2f);
        }

        invencible = false;

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
    }

    public void AddEXP(float amount)
    {
        if (level == 3) return;

        exp += amount;

        if (exp >= 100)
        {
            exp = 0;
            LvlUp();
        }
    }


    public void LvlUp()
    {
        if (levelUpEffect && !levelUpEffect.isPlaying)
        {
            levelUpEffect.Play();
        }
        if (level == 0)
        {
            secundaryactivate = true;
            level = 1;
            UpdateSecondaryAbilityImage();
            return;
        }

        if (level == 1)
        {
            dashCooldown /= 2f;
            level = 2;
            return;
        }

        if (level == 2)
        {
            secundaryCooldown /= 2f;
            level = 3;
            exp = 100.0f;
            return;
        }

        if (level == 3)
            return;


    }

    //Marco Antonio
    void UpdateSecondaryAbilityImage(bool isOnCooldown = false)
    {
        if (!secundaryactivate)
            secondaryAbilityImage.sprite = secondaryAbilityInactive;
        else if (isOnCooldown)
            secondaryAbilityImage.sprite = secondaryAbilityCooldown;
        else
            secondaryAbilityImage.sprite = secondaryAbilityActive;
    }
    //-------------------

    void Update()
    {
        Pbh.BarValue = hp;
        Pbe.BarValue = exp;
        //Movimiento y Dash
        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                isDashing = false;
            }
            me.position += (Vector3)(lastDirection * speed * dashSpeedMultiplier * Time.deltaTime);
        }
        else
        {
            me.position += (Vector3)(movementInput * speed * Time.deltaTime);
        }

        // Actualizar rotación del personaje
        if (movementInput != Vector2.zero)
        {
            lastDirection = movementInput;
            UpdateRotation();
        }

        // Actualizar Animator
        UpdateAnimator();
    }

    void UpdateRotation()
    {
        if (lastDirection == Vector2.right)
            me.rotation = Quaternion.Euler(0, 0, 0);
        else if (lastDirection == Vector2.up)
            me.rotation = Quaternion.Euler(0, 0, 90);
        else if (lastDirection == Vector2.down)
            me.rotation = Quaternion.Euler(0, 0, -90);
        else if (lastDirection == Vector2.left)
            me.rotation = Quaternion.Euler(0, 180, 0);
    }

    void UpdateAnimator()
    {
        // Determinar si el personaje se está moviendo
        bool isMoving = movementInput != Vector2.zero;

        // Actualizar parámetros del Animator
        //animator.SetBool("IsMoving", isMoving);
        animator.SetBool("Attack", attacking);

        //Si el personaje esta atacando, se reproduce la animacion de ataque
        if (attacking)
        {

            if (movementInput.x > 0)
            {
                // Ataque hacia la derecha
                animator.Play("Player_Attack");
                spriteRenderer.flipX = true;
            }
            else if (movementInput.x < 0)
            {
                // Ataque hacia la izquierda
                animator.Play("Player_Attack");
                spriteRenderer.flipX = true;
            }
            else if (lastDirection.y > 0)
            {
                animator.Play("Player_Attack");
                spriteRenderer.flipX = true;
            }
            else if (lastDirection.y < 0)
            {
                animator.Play("Player_Attack");
                spriteRenderer.flipX = true;
            }

            //
            if (lastDirection.x > 0)
            {
                // Ataque hacia la derecha
                animator.Play("Player_Attack");
                spriteRenderer.flipX = true;
            }
            else if (lastDirection.x < 0)
            {
                // Ataque hacia la izquierda
                animator.Play("Player_Attack");
                spriteRenderer.flipX = true;
            }
            else if (movementInput.y > 0)
            {
                // Ataque hacia arriba
                animator.Play("Player_Attack");
                spriteRenderer.flipX = true;
            }
            else if (movementInput.y < 0)
            {
                // Ataque hacia abajo
                animator.Play("Player_Attack");
                spriteRenderer.flipX = true;
            }
        }
        else
        {
            // Si no está atacando, reproducir animación de caminar
            animator.SetBool("IsMoving", isMoving);

            if (isMoving)
            {
                if (Mathf.Abs(movementInput.x) > Mathf.Abs(movementInput.y))
                {
                    // Movimiento lateral (derecha o izquierda)
                    if (movementInput.x > 0)
                    {
                        // Movimiento hacia la derecha
                        animator.Play("Player_Side_Walk");
                        spriteRenderer.flipX = true;
                    }
                    else if (movementInput.x < 0)
                    {
                        // Movimiento hacia la izquierda
                        animator.Play("Player_Side_Walk");
                        spriteRenderer.flipX = true;
                    }
                }
                else if (movementInput.y > 0)
                {
                    // Movimiento hacia arriba
                    animator.Play("Player_Back_Walk");
                    spriteRenderer.flipX = false;
                }
                else if (movementInput.y < 0)
                {
                    // Movimiento hacia abajo
                    animator.Play("Player_Front_Walk");
                    spriteRenderer.flipX = false;
                }
            }
            else
            {
                //// Si no se mueve, reproducir animación de idle según la última dirección
                //if (lastDirection.x > 0)
                //{
                //    animator.Play("Player_Idle");
                //    spriteRenderer.flipX = false; // Asegúrate de que el sprite no esté volteado
                //}
                //else if (lastDirection.x < 0)
                //{
                //    animator.Play("Player_Idle");
                //    spriteRenderer.flipX = true; // Voltear el sprite hacia la izquierda
                //}
                //else if (lastDirection.y > 0)
                //{
                //    animator.Play("Player_Idle");
                //}
                //else if (lastDirection.y < 0)
                //{
                //    animator.Play("Player_Idle");
                //}

                // Si no se mueve, se puede poner una animación de descanso o idle si lo prefieres
                animator.Play("Player_Idle");
                spriteRenderer.flipX = false;
            }
        }
    }
}
