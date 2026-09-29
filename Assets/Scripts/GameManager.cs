using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game")]
    public float gameDuration = 60f;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text timeText;
    public Slider healthSlider; // เปลี่ยนจาก TMP_Text เป็น Slider
    public TMP_Text messageText;        // ข้อความแจ้งเตือนระหว่างเล่น (เก็บไว้ ไม่ต้องปิดมัน)
    public TMP_Text ammoText;
    public TMP_Text reserveAmmoText;

    [Header("Game Over / Win UI Panel")]
    public GameObject gameOverPanel;     // ลาก Panel ป๊อปอัปมาใส่
    public TMP_Text finalScoreText;    // Text แสดงผลคะแนนในป๊อปอัปโดยเฉพาะ
    public string mainMenuSceneName = "MainMenu"; // ชื่อ Scene หน้าเมนูหลักของคุณ

    private int score = 0;
    private float timeLeft;
    private bool isGameOver = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip winClip;
    public AudioClip loseClip;

    public bool IsGameOver
    {
        get
        {
            return isGameOver;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        timeLeft = gameDuration;
        score = 0;
        isGameOver = false;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // ซ่อน Panel จบเกมตอนเริ่มเล่น
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        UpdateScoreText();
        UpdateTimeText();
    }

    private void Update()
    {
        if (isGameOver)
        {
            CheckRestartInput();
            return;
        }

        UpdateTimer();
    }

    private void UpdateTimer()
    {
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            WinGame();
        }

        UpdateTimeText();
    }

    private void UpdateTimeText()
    {
        if (timeText != null)
        {
            timeText.text = Mathf.CeilToInt(timeLeft).ToString();

            
        }
    }

    public void AddScore(int amount)
    {
        if (isGameOver)
            return;

        score += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "Score: " + score;
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;      // กำหนดค่าเลือดสูงสุด (เช่น 3)
            healthSlider.value = currentHealth;     // อัปเดตเลือดปัจจุบันตามที่ลดหรือเพิ่ม
        }
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        PlaySound(loseClip);

        // แสดง UI หน้าจอจบเกมแบบแพ้
        ShowEndGameUI("GAME OVER", "Score: " + score);
    }

    private void WinGame()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        PlaySound(winClip);

        // แสดง UI หน้าจอจบเกมแบบชนะ
        ShowEndGameUI("YOU SURVIVED!", "Score: " + score);
    }

    private void ShowEndGameUI(string title, string details)
    {
        // 1. เปิด Panel ป๊อปอัปขึ้นมา
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 2. แสดงข้อความเฉพาะในกล่องป๊อปอัป (Final Score Text) เท่านั้น
        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(true);
            finalScoreText.text = title + "\n" + details;
        }

        // (ตัดคำสั่งปิด messageText ออกไปแล้ว ตัวอื่นจึงไม่ถูกปิดเกะกะครับ)
    }

    private void CheckRestartInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SetAmmo(int currentAmmo, int magazineSize, int reserveAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text =
                "Ammo: " +
                currentAmmo +
                "/" +
                magazineSize;
        }

        if (reserveAmmoText != null)
        {
            reserveAmmoText.text =
                "magazine: " +
                reserveAmmo;
        }
    }

    // ฟังก์ชันสำหรับแสดงข้อความแจ้งเตือนบนจอ (เช่น Press R to Reload)
    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = message;
        }
    }

    // ฟังก์ชันสำหรับซ่อนข้อความแจ้งเตือน
    public void HideMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayShootSound()
    {
        PlaySound(shootSound);
    }
}