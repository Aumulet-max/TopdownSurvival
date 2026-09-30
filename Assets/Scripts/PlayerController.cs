using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootCooldown = 0.2f;

    [Header("Dash UI")]
    public UnityEngine.UI.Image dashCooldownBar; // หลอดคูลดาวน์แบบวิ่งจากซ้ายไปขวา

    [Header("Audio")]
    public AudioClip reloadSound;
    public AudioClip dashSound;
    private AudioSource playerAudio;

    Rigidbody rb;
    private Camera mainCamera;
    private Vector3 moveDirection;
    private Quaternion targetRotation;
    private float nextShootTime;

    public int magazineSize = 30;
    public int currentAmmo = 30;
    public int reserveAmmo = 5;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    public float dashforce = 200f;
    public float dashCooldown = 3f;
    private bool canDash = true;
    private float dashCooldownTimer = 0f; // ตัวแปรสำหรับจับเวลาคูลดาวน์

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        targetRotation = transform.rotation;
        playerAudio = GetComponent<AudioSource>();
        CheckAmmoWarning();
    }

    private void Update()
    {
        // ถ้าเกมจบแล้ว ไม่รับ Input
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            moveDirection = Vector3.zero;
            if (GameManager.Instance != null)
                GameManager.Instance.HideMessage();
            return;
        }

        ReadMovementInput();
        AimAtMouse();
        ReadShootingInput();
        CheckAmmoWarning();
        UpdateDashCooldownUI(); // อัปเดตหลอดคูลดาวน์ Dash ทุกเฟรม
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    private void ReadMovementInput()
    {
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && canDash)
        {
            Debug.Log("Dashing");
            Dash();
        }

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        moveDirection = input.normalized;
    }

    private void MovePlayer()
    {
        Vector3 velocity = moveDirection * moveSpeed;

        rb.linearVelocity = new Vector3(
            velocity.x,
            rb.linearVelocity.y, // คงค่าแรงโน้มถ่วงแนวแกน Y เดิมไว้
            velocity.z
        );
    }

    private void AimAtMouse()
    {
        if (Mouse.current == null || mainCamera == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 lookDirection = hitPoint - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                targetRotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    private void RotatePlayer()
    {
        rb.MoveRotation(targetRotation);
    }

    private void ReadShootingInput()
    {
        if (Mouse.current == null || Keyboard.current == null)
            return;

        // ยิงกระสุนด้วยคลิกซ้าย (และต้องไม่อยู่ระหว่างรีโหลด)
        if (Mouse.current.leftButton.isPressed &&
            Time.time >= nextShootTime &&
            !isReloading)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                nextShootTime = Time.time + shootCooldown;
            }
        }

        // รีโหลดด้วยปุ่ม R
        if (Keyboard.current.rKey.wasPressedThisFrame && !isReloading)
        {
            if (reserveAmmo > 0 && currentAmmo < magazineSize)
            {
                Reload();
            }
        }
    }

    private void Shoot()
    {
        currentAmmo--;
        FireBullet();

        GameManager.Instance.SetAmmo(
            currentAmmo,
            magazineSize,
            reserveAmmo
        );
    }

    private void FireBullet()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayShootSound();
        }
    }

    private void Reload()
    {
        isReloading = true;
        reserveAmmo--;
        currentAmmo = magazineSize;

        if (playerAudio != null && reloadSound != null)
            playerAudio.PlayOneShot(reloadSound);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetAmmo(currentAmmo, magazineSize, reserveAmmo);
        }

        Invoke(nameof(FinishReload), reloadTime);
    }

    public void FinishReload()
    {
        isReloading = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ammo"))
        {
            Debug.Log("Picked up ammo");
            reserveAmmo++;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetAmmo(currentAmmo, magazineSize, reserveAmmo);
            }
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("health"))
        {
            Debug.Log("Picked up health");
            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null && playerHealth.HasHealth())
            {
                playerHealth.TakeDamage(-1); // Heal 1 health
            }
            Destroy(other.gameObject);
        }
    }

    private void CheckAmmoWarning()
    {
        if (GameManager.Instance == null) return;

        if (currentAmmo == 0 && reserveAmmo > 0 && !isReloading)
        {
            GameManager.Instance.ShowMessage("PRESS R TO RELOAD");
        }
        else
        {
            GameManager.Instance.HideMessage();
        }
    }

    private void Dash()
    {
        if (canDash)
        {
            canDash = false;
            dashCooldownTimer = dashCooldown; // เริ่มต้นนับเวลาคูลดาวน์

            Vector3 direction = transform.forward;
            rb.AddForce(direction * dashforce, ForceMode.Impulse);

            if (playerAudio != null && dashSound != null)
                playerAudio.PlayOneShot(dashSound);

            Invoke(nameof(ResetDash), dashCooldown);
        }
    }

    private void ResetDash()
    {
        canDash = true;
    }

    private void UpdateDashCooldownUI()
    {
        if (dashCooldownBar == null) return;

        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            dashCooldownBar.fillAmount = 1f - (dashCooldownTimer / dashCooldown);
        }
        else
        {
            dashCooldownBar.fillAmount = 1f;
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;
        // Set the "Speed" parameter based on the player's movement
        float speed = moveDirection.magnitude;
        animator.SetFloat("Speed", speed);
    }
}
