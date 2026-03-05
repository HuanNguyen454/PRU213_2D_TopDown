using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private SaveManager saveManager;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void SaveGame()
    {
        Player_Health player = FindObjectOfType<Player_Health>();

        if (player != null)
        {
            saveManager.SaveGame(player);
        }
    }

    public void LoadGame()
    {
        GameData data = saveManager.LoadGame();

        if (data == null) return;

        SceneManager.LoadScene(data.currentScene);

        StartCoroutine(LoadPlayerData(data));
    }

    private System.Collections.IEnumerator LoadPlayerData(GameData data)
    {
        yield return null;

        Player_Health player = FindObjectOfType<Player_Health>();

        if (player != null)
        {
            player.TakeDamage(player.CurrentHP - data.playerHP);

            player.transform.position = new Vector2(
                data.playerPosX,
                data.playerPosY
            );
        }
    }
}