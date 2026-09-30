using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นสำหรับการเปลี่ยนฉาก

public class UiManager : MonoBehaviour
{
    // ฟังก์ชันสำหรับกดเพื่อเล่นเกม (ไปหน้าเกม)
    public void PlayGame()
    {
        SceneManager.LoadScene("Maingame"); // เปลี่ยน "GameScene" เป็นชื่อ Scene เกมของคุณ
    }


    // ฟังก์ชันสำหรับกดเพื่อออกจากเกม
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit(); // ปิดเกม (ทำงานเมื่อ Build ออกมาแล้ว)
    }

}