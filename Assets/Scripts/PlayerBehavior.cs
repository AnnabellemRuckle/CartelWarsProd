using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
public class PlayerBehavior : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotateSpeed = 75f;
    public float jumpVelocity = 5f;
    public float duckedHeight = 0.5f;
    public float distanceToGround = 0.1f;
    public LayerMask groundLayer;
    public GameObject gunshotEffect;
    public GameObject hitPoint;
    public Transform gunSpawnPoint;
    private float vInput;
    private float hInput;
    private float originalHeight;
    private Rigidbody _rb;
    private CapsuleCollider _col;
    public delegate void JumpingEvent();
    public event JumpingEvent playerJump;
    private bool isShooting = false;
    public int maxHealth = 3;
    private int currentHealth;
    public int enemyDamage = 1;
    public TextMeshProUGUI healthText;
    public AudioSource gunshotAudio;
    public AudioClip gunshotSoundClip;
    public AudioClip damageSoundClip;
    public GameObject bloodSplatterUI;
    public TextMeshProUGUI gameOverText;
    private bool hasDied = false;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();
        originalHeight = _col.height;
        playerJump += PerformJump;
        currentHealth = maxHealth;

        gunshotAudio = gameObject.AddComponent<AudioSource>();
        gunshotAudio.clip = gunshotSoundClip;

        bloodSplatterUI.SetActive(false);
    }

    void Update()
    {
        vInput = Input.GetAxis("Vertical") * moveSpeed;
        hInput = Input.GetAxis("Horizontal") * rotateSpeed;

        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            playerJump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartShooting();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopShooting();
        }

        if (Input.GetKey(KeyCode.S))
        {
            Duck();
        }
        else
        {
            ReleaseDuck();
        }
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth.ToString();
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void FixedUpdate()
    {
        Vector3 forwardMovement = this.transform.forward * vInput * Time.fixedDeltaTime;

        if (_col.height == duckedHeight)
        {
            forwardMovement = Vector3.zero;
        }

        _rb.MovePosition(this.transform.position + forwardMovement);

        if (Mathf.Abs(hInput) > 0.01f)
        {
            Vector3 rotation = Vector3.up * hInput * rotateSpeed * Time.fixedDeltaTime;
            this.transform.Rotate(rotation);
        }

        _rb.angularVelocity = Vector3.zero;

        if (isShooting)
        {
            Shoot();
        }
    }

    private bool IsGrounded()
    {
        Vector3 capsuleBottom = new Vector3(_col.bounds.center.x, _col.bounds.min.y, _col.bounds.center.z);
        bool grounded = Physics.CheckCapsule(_col.bounds.center, capsuleBottom, distanceToGround, groundLayer, QueryTriggerInteraction.Ignore);

        return grounded;
    }

    private void PerformJump()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, jumpVelocity, _rb.velocity.z);
    }

    private void Duck()
    {
        _col.height = duckedHeight;
    }

    public bool IsDucking()
    {
        return _col.height == duckedHeight;
    }

    private void ReleaseDuck()
    {
        _col.height = originalHeight;
    }

    private void StartShooting()
    {
        isShooting = true;
        gunshotEffect.SetActive(true);
        gunshotAudio.clip = gunshotSoundClip;
        gunshotAudio.Play();
    }

    private void StopShooting()
    {
        isShooting = false;
        gunshotEffect.SetActive(false);
    }

    private void Shoot()
    {
        if (gunshotEffect != null && gunSpawnPoint != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(gunSpawnPoint.position, transform.TransformDirection(Vector3.forward), out hit))
            {
                EnemyBehavior enemy = hit.collider.GetComponent<EnemyBehavior>();
                if (enemy != null)
                {
                    Debug.DrawRay(gunSpawnPoint.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    GameObject a = Instantiate(gunshotEffect, gunSpawnPoint.position, Quaternion.identity);
                    GameObject b = Instantiate(hitPoint, hit.point, Quaternion.identity);
                    Destroy(a, 1);
                    Destroy(b, 1);
                    int damage = Random.Range(1, 4); 
                    enemy.TakeDamage(damage);
                }
            }
        }
    }

    private void Die()
    {
        if (!hasDied) 
        {
            hasDied = true;
            Debug.Log("Player is dead.");
            isShooting = false;
            GetComponent<PlayerBehavior>().enabled = false;
            gameOverText.text = "GAME OVER";

            StartCoroutine(WaitAndLoadScene());
        }

    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        gunshotAudio.clip = damageSoundClip;
        gunshotAudio.Play();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DisplayBloodSplatter());
        }
    }

    private IEnumerator DisplayBloodSplatter()
    {
        bloodSplatterUI.SetActive(true);
        yield return new WaitForSeconds(1.5f); 
        bloodSplatterUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(enemyDamage);
        }
    }

    IEnumerator WaitAndLoadScene()
    {
        yield return new WaitForSeconds(5f); 
        LoadMainScene();
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene("Mainland"); 
    }
}
