using StarterAssets;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathMenu : MonoBehaviour {
    [SerializeField] private GameObject playerDeathMenuUI;
    private void Start() {
        if (playerDeathMenuUI == null) {
            Debug.LogError("[DeathMenu] Death Menu UI is not assigned in the inspector.");
        }
        playerDeathMenuUI.SetActive(false);

        if (HealthSystem.Instance != null) HealthSystem.Instance.OnPlayerDeath += HealthSystem_OnPlayerDeath;
    }
    private void HealthSystem_OnPlayerDeath(object sender, System.EventArgs e) {
        ShowGameOver();
    }
    public void OnMainMenuButton() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowGameOver() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerDeathMenuUI.SetActive(true);
    }

    private void OnDestroy() {
        if (HealthSystem.Instance != null) HealthSystem.Instance.OnPlayerDeath -= HealthSystem_OnPlayerDeath;
    }
}
