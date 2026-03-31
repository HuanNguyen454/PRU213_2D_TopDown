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
        // 1. Tìm Player (Đợi tối đa 10 frame nếu chưa thấy)
        GameObject playerObj = null;
        int retryCount = 0;
        while (playerObj == null && retryCount < 10)
        {
            yield return null; // Chờ frame tiếp theo
            var playerHealth = GameManager.Instance.GetCurrentPlayer();
            if (playerHealth != null) playerObj = playerHealth.gameObject;
            retryCount++;
        }

        if (playerObj == null)
        {
            Debug.LogWarning("CompanionManager: Không tìm thấy Player sau khi load cảnh!");
            yield break;
        }

        // 2. Spawn hoặc Lấy con chó hiện tại
        if (activeDog == null)
        {
            activeDog = Instantiate(dogPrefab, playerObj.transform.position, Quaternion.identity);
            DontDestroyOnLoad(activeDog);
        }
        else
        {
            // Nếu chó đã có, đảm bảo nó được bật lên
            activeDog.SetActive(true);
        }

        // 3. GIẢI PHÁP: Ép chó bám đuôi Player trong 5 frame đầu tiên 🔥
        // Việc này đảm bảo dù Player có bị GameManager dịch chuyển đi đâu (đến Portal),
        // con chó cũng sẽ đi theo ngay lập tức, không bị "bỏ rơi".
        int followFrames = 5;
        while (followFrames > 0)
        {
            if (activeDog != null && playerObj != null)
            {
                // Dịch chuyển chó đến vị trí Player ngay lập tức
                activeDog.transform.position = playerObj.transform.position;

                // Tạm thời tắt script Follow để tránh xung đột vật lý
                var followScript = activeDog.GetComponent<CompanionFollow>();
                if (followScript != null) followScript.enabled = false;
            }

            yield return null; // Chờ frame tiếp theo
            followFrames--;
        }

        // 4. Bật lại script Follow sau khi đã ổn định vị trí
        if (activeDog != null)
        {
            var followScript = activeDog.GetComponent<CompanionFollow>();
            if (followScript != null) followScript.enabled = true;

            Debug.Log("CompanionManager: Đã ổn định vị trí chó tại Scene mới.");
        }
    }

    public void UnlockCompanion(Vector3 position)
    {
        isUnlocked = true;
        StartCoroutine(SpawnOrRepositionRoutine());
    }
}