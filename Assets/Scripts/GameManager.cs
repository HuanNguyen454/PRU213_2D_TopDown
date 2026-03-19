using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<Player_Health> OnPlayerSpawned;

    [Header("References")]
    [SerializeField] private SaveManager saveManager;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string playerTag = "Player";

    private Player_Health currentPlayer;
    private bool isLoadingFromSave = false;
    // NEW: lưu portal ID tạm thời khi chuyển scene
    private string pendingPortalID = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsurePlayerExists();

        // đặt vị trí theo portal nếu có
        StartCoroutine(HandleSpawnAfterLoad());

        BindCameraToPlayer();
    }

    private IEnumerator HandleSpawnAfterLoad()
    {
        yield return null;

        var p = GetCurrentPlayer();
        if (p == null) yield break;

        // nếu có portal → spawn theo portal
        if (!string.IsNullOrEmpty(pendingPortalID))
        {
            var spawnPoints = FindObjectsOfType<SpawnPoint>();

            foreach (var sp in spawnPoints)
            {
                if (sp.portalID == pendingPortalID)
                {
                    p.transform.position = sp.transform.position;
                    pendingPortalID = "";
                    yield break;
                }
            }
        }

        // chỉ load save khi thực sự load game
        if (isLoadingFromSave && saveManager != null)
        {
            GameData data = saveManager.LoadGame();
            if (data != null)
            {
                p.transform.position = new Vector2(data.playerPosX, data.playerPosY);
                TrySetHP(p, data.playerHP);
            }

            isLoadingFromSave = false; // reset
        }
    }

    // =============================
    // SAVE / LOAD
    // =============================

    public void SaveGame()
    {
        var p = GetCurrentPlayer();
        if (p == null || saveManager == null) return;

        saveManager.SaveGame(p);
    }

    // NEW: save kèm portal
    public void SaveGameWithPortal(string portalID)
    {
        var p = GetCurrentPlayer();
        if (p == null || saveManager == null) return;

        pendingPortalID = portalID;
        saveManager.SaveGame(p);
    }

    public void LoadGame()
    {
        if (saveManager == null) return;

        GameData data = saveManager.LoadGame();
        if (data == null) return;

        isLoadingFromSave = true; // ✅ đánh dấu

        SceneManager.LoadScene(data.currentScene);
    }

    // =============================
    // PLAYER
    // =============================

    public Player_Health EnsurePlayerExists()
    {
        if (currentPlayer != null && currentPlayer.gameObject != null && currentPlayer.gameObject.activeInHierarchy)
        {
            NotifyPlayerSpawned(currentPlayer);
            return currentPlayer;
        }

        GameObject found = GameObject.FindGameObjectWithTag(playerTag);
        if (found != null)
        {
            currentPlayer = found.GetComponent<Player_Health>();
            NotifyPlayerSpawned(currentPlayer);
            return currentPlayer;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("GameManager: playerPrefab is NULL!");
            return null;
        }

        GameObject obj = Instantiate(playerPrefab);
        obj.tag = playerTag;

        currentPlayer = obj.GetComponent<Player_Health>();

        if (currentPlayer == null)
        {
            Debug.LogError("GameManager: Spawned Player has no Player_Health!");
            return null;
        }

        NotifyPlayerSpawned(currentPlayer);
        return currentPlayer;
    }

    public Player_Health GetCurrentPlayer()
    {
        if (currentPlayer != null) return currentPlayer;

        GameObject found = GameObject.FindGameObjectWithTag(playerTag);
        if (found == null) return null;

        currentPlayer = found.GetComponent<Player_Health>();
        return currentPlayer;
    }

    private void NotifyPlayerSpawned(Player_Health hp)
    {
        if (hp == null) return;
        OnPlayerSpawned?.Invoke(hp);
    }

    private void BindCameraToPlayer()
    {
        var p = GetCurrentPlayer();
        if (p == null) return;

        var cam = FindObjectOfType<CinemachineVirtualCamera>();
        if (cam != null)
        {
            cam.Follow = p.transform;
            cam.LookAt = p.transform;
        }
    }

    private void TrySetHP(Player_Health p, int hpValue)
    {
        int diff = p.CurrentHP - hpValue;
        if (diff > 0) p.TakeDamage(diff);
        else if (diff < 0) p.Heal(-diff);
    }

    public void ResetPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer.gameObject);
            currentPlayer = null;
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}