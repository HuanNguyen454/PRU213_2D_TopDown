using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Player_Health playerHealth;
    [SerializeField] private GameObject gameOverCanvas;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Scene";
    [SerializeField] private string menuSceneName = "Menu";

    private bool isGameOver = false;

    private void Awake()
    {
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        if (isGameOver) return;
        isGameOver = true;

        Time.timeScale = 0f;
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
