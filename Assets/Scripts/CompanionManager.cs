using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanionManager : MonoBehaviour
{
    public static CompanionManager Instance { get; private set; }

    [Header("Settings")]
    public GameObject dogPrefab; // Kéo Prefab con chó vào đây
    public bool isUnlocked = false; // Trạng thái đã nhận chó chưa

    private GameObject activeDog;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isUnlocked)
        {
            // Đợi 1 frame để GameManager spawn Player xong rồi mới spawn Chó
            StartCoroutine(SpawnOrRepositionRoutine());
        }
    }

    private System.Collections.IEnumerator SpawnOrRepositionRoutine()
    {
        yield return null; // Chờ Player xuất hiện

        // Lấy Player từ GameManager của bạn
        var player = GameManager.Instance.GetCurrentPlayer();
        if (player == null) yield break;

        Vector3 spawnPos = player.transform.position;

        if (activeDog == null)
        {
            activeDog = Instantiate(dogPrefab, spawnPos, Quaternion.identity);
            DontDestroyOnLoad(activeDog);
        }
        else
        {
            activeDog.transform.position = spawnPos;
            activeDog.SetActive(true);
        }
    }

    public void UnlockCompanion(Vector3 position)
    {
        isUnlocked = true;
        StartCoroutine(SpawnOrRepositionRoutine());
    }
}