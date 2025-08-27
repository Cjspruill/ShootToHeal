using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject gameOverPanel;

    [SerializeField] PlayerController playerController;
    [SerializeField] Health playerHealth;
    [SerializeField] GameObject pausePanel;

    InputSystem_Actions playerInput;

    bool gameIsPaused = false;

    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI shootToHealText;
    [SerializeField] TextMeshProUGUI fireRateText;
    [SerializeField] TextMeshProUGUI bulletDamageText;
    [SerializeField] TextMeshProUGUI moveSpeedText;
    [SerializeField] TextMeshProUGUI rotationSpeedText;
    [SerializeField] TextMeshProUGUI cameraViewDistanceText;
    [SerializeField] TextMeshProUGUI enemyDetectionRangeText;
    [SerializeField] TextMeshProUGUI sprintTimeText;
    [SerializeField] TextMeshProUGUI sprintMultiplierText;
    [SerializeField] TextMeshProUGUI sprintCoolDownText;
    [SerializeField] TextMeshProUGUI xpText;

    private void OnEnable()
    {
        playerInput = new InputSystem_Actions();
        playerInput.UI.Enable();
        playerInput.UI.Cancel.performed += OnCancelPerformed;
        GameManager.OnGameOver += ShowGameOverPanel;
    }

    private void OnDisable()
    {
        playerInput.UI.Disable();
        playerInput.UI.Cancel.performed -= OnCancelPerformed;
        GameManager.OnGameOver -= ShowGameOverPanel;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateTexts();
    }

    public void OnCancelPerformed(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.levelEnded) return;

        gameIsPaused = !gameIsPaused;
        pausePanel.SetActive(gameIsPaused);

        if (gameIsPaused)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    void UpdateTexts()
    {
        healthText.text = "Player Health: " + playerHealth.GetRoundedHealth() + " / " + playerHealth.GetRoundedMaxHealth();
        shootToHealText.text = "Player Shoot To Heal: " + Mathf.Round(playerController.GetShootToHeal * 100f) / 100f;
        fireRateText.text = "Player Fire Rate: " + Mathf.Round(playerController.GetFireRate * 100f) / 100f;
        bulletDamageText.text = "Player Bullet Damage: " + Mathf.Round(playerController.GetBulletDamage * 100f) / 100f;
        moveSpeedText.text = "Player Move Speed: " + Mathf.Round(playerController.GetMoveSpeed * 100f) / 100f;
        rotationSpeedText.text = "Player Rotation Speed: " + Mathf.Round(playerController.GetRotationSpeed * 100f) / 100f;
        cameraViewDistanceText.text = "Player Camera View Distance: " + Mathf.Round(playerController.GetCameraViewDistance * 100f) / 100f;
        enemyDetectionRangeText.text = "Player Enemy Detection Range: " + Mathf.Round(playerController.GetEnemyDetectionRange * 100f) / 100f;
        sprintTimeText.text = "Player Sprint Time: " + Mathf.Round(playerController.GetSprintTime * 100f) / 100f;
        sprintMultiplierText.text = "Player Sprint Multiplier: " + Mathf.Round(playerController.GetSprintMultiplier * 100f) / 100f;
        sprintCoolDownText.text = "Player Sprint Cooldown: " + Mathf.Round(playerController.GetSprintCooldown * 100f) / 100f;

        if (GameManager.Instance.showXP)
            xpText.text = "Player XP: " + playerController.GetXp;
        else
            xpText.text = "";
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }

    public void PauseButtonClicked()
    {
        if (GameManager.Instance.levelEnded) return;

        gameIsPaused = !gameIsPaused;
        pausePanel.SetActive(gameIsPaused);

        if (gameIsPaused)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

  
}