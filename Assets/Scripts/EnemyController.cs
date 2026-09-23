using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Combat")]
    public int maxHealth = 1;
    public int touchDamage = 1;
    public int scoreValue = 10;
    private int currentHealth;
    private Rigidbody rb;
    private Transform player;
    private bool isDead = false;
    public GameObject ammo;
    public GameObject healthPack;

    [Range(0f, 100f)]
    public float ammoDropChance = 15f;

    [Range(0f, 100f)]
    public float healthPackDropChance = 10f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (player == null)
            return;

        MoveTowardPlayer();
    }

    private void MoveTowardPlayer()
    {
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        direction.Normalize();

        Vector3 newPosition =
            rb.position +
            direction *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(newPosition);

        Quaternion rotation =
            Quaternion.LookRotation(direction);

        rb.MoveRotation(rotation);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            Drop();
        }

    }

    private void Die()
    {
        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(
                scoreValue
            );
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
            return;

        if (other.TryGetComponent<PlayerHealth>(
            out PlayerHealth playerHealth))
        {
            playerHealth.TakeDamage(
                touchDamage
            );

            Destroy(gameObject);
        }
    }

    private void Drop()
    {
        float randomValue = Random.Range(0f, 100f);
        Debug.Log("Random Value: " + randomValue);
        if (randomValue < ammoDropChance)
        {
            Instantiate(
                ammo,
                transform.position,
                Quaternion.identity
            );
        }
        else if (randomValue < ammoDropChance + healthPackDropChance)
        {
            Instantiate(
                healthPack,
                transform.position,
                Quaternion.identity
            );
        }
    }
}

