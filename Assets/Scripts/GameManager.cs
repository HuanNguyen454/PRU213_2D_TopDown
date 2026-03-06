using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private SaveManager saveManager;
    [SerializeField] private GameObject playerPrefab;
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

        if (player == null)
        {
            GameObject newPlayer = Instantiate(playerPrefab);
            player = newPlayer.GetComponent<Player_Health>();
        }

        player.transform.position = new Vector2(
            data.playerPosX,
            data.playerPosY
        );

        player.TakeDamage(player.CurrentHP - data.playerHP);
        var cam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        if (cam != null)
        {
            cam.Follow = player.transform;
            cam.LookAt = player.transform;
        }
    }
    private void OnApplicationQuit()
    {
        SaveGame();
    }
}