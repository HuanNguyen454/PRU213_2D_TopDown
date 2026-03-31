using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverCanvas;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Scene";
    [SerializeField] private string menuSceneName = "Menu";

    private Player_Health playerHealth;
    private bool isGameOver = false;

    private void Awake()
    {
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerSpawned += HandlePlayerSpawned;

            var p = GameManager.Instance.GetCurrentPlayer();
            if (p != null) HandlePlayerSpawned(p);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerSpawned -= HandlePlayerSpawned;

        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerSpawned(Player_Health hp)
    {
        // unsubscribe player cũ
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;

        playerHealth = hp;

        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        Debug.Log("GAME OVER TRIGGERED");

        if (isGameOver) return;
        isGameOver = true;

        Time.timeScale = 0f;

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.ResetPlayer();

        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.ResetPlayer();

        SceneManager.LoadScene(menuSceneName);
    }
}