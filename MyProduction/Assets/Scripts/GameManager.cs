using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI gameOverStatsText;

    [Header("Game Stats")]
    private int enemiesKilled = 0;
    private float survivalTime = 0f;
    private bool isGameOver = false;
    private bool isPaused = false;

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

    private void Update()
    {
        if (!isGameOver)
        {
            survivalTime += Time.deltaTime;
        }

        // Pause input
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void AddKill()
    {
        enemiesKilled++;
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);

            if (gameOverStatsText != null)
            {
                int minutes = Mathf.FloorToInt(survivalTime / 60f);
                int seconds = Mathf.FloorToInt(survivalTime % 60f);

                gameOverStatsText.text = $"Time Survived: {minutes:00}:{seconds:00}\n" +
                                        $"Enemies Killed: {enemiesKilled}";
            }
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        // For now, just quit application
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Getters
    public int EnemiesKilled => enemiesKilled;
    public float SurvivalTime => survivalTime;
}