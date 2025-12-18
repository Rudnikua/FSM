using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillManager : MonoBehaviour
{
    public static KillManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private GameObject winScreen;

    [SerializeField] private int killsToWin = 10;

    private int killCount = 0;
    private bool isGameWon = false;
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }
    }
    private void Start() {
        if (isGameWon) return;
        if (winScreen != null)
            winScreen.SetActive(false);

        UpdateKillText();
    }
    public void AddKill() {
        killCount++;

        UpdateKillText();

        if (killCount >= killsToWin && !isGameWon) {
            WinGame();
        }
    }

    private void UpdateKillText() {
        if (killCountText != null) {
            killCountText.text = $"Kills: " + killCount.ToString();
        }
    }

    private void WinGame() {
        isGameWon = true;

        if (winScreen != null) {
            winScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void OnMainMenuButton() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
