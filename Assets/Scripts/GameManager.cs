using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // UI s? nghe event này ?? l?y Player_Health m?i
    public event Action<Player_Health> OnPlayerSpawned;

    [Header("References")]
    [SerializeField] private SaveManager saveManager;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string playerTag = "Player";

    // Player hi?n t?i (???c gi? ?? UI/camera d? truy c?p)
    private Player_Health currentPlayer;

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

    // M?i khi load scene xong -> ??m b?o có Player và b?n event cho UI
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsurePlayerExists();
        BindCameraToPlayer();
    }

    public void SaveGame()
    {
        var p = GetCurrentPlayer();
        if (p == null || saveManager == null) return;

        saveManager.SaveGame(p);
    }

    public void LoadGame()
    {
        if (saveManager == null) return;

        GameData data = saveManager.LoadGame();
        if (data == null) return;

        // Load scene theo save
        SceneManager.LoadScene(data.currentScene);

        // Sau khi scene load xong 1 frame thì apply data
        StartCoroutine(ApplyLoadedDataNextFrame(data));
    }

    private IEnumerator ApplyLoadedDataNextFrame(GameData data)
    {
        yield return null;

        var p = EnsurePlayerExists();

        // Apply v? trí
        p.transform.position = new Vector2(data.playerPosX, data.playerPosY);

        // Apply HP: cách t?t nh?t là SetHP tr?c ti?p (mình thêm hàm TrySetHP)
        TrySetHP(p, data.playerHP);

        if (CompanionManager.Instance != null)
        {
            CompanionManager.Instance.isUnlocked = data.isDogUnlocked;
        }

        BindCameraToPlayer();
    }

    // ??m b?o luôn có player trong scene
    public Player_Health EnsurePlayerExists()
    {
        // N?u currentPlayer còn s?ng và ?ang active -> dùng luôn
        if (currentPlayer != null && currentPlayer.gameObject != null && currentPlayer.gameObject.activeInHierarchy)
        {
            NotifyPlayerSpawned(currentPlayer);
            return currentPlayer;
        }

        // Tìm Player trong scene theo tag (phòng khi player ???c spawn b?i script khác)
        GameObject found = GameObject.FindGameObjectWithTag(playerTag);
        if (found != null)
        {
            currentPlayer = found.GetComponent<Player_Health>();
            NotifyPlayerSpawned(currentPlayer);
            return currentPlayer;
        }

        // Không th?y -> spawn m?i
        if (playerPrefab == null)
        {
            Debug.LogError("GameManager: playerPrefab is NULL!");
            return null;
        }

        GameObject obj = Instantiate(playerPrefab);
        obj.tag = playerTag; // ??m b?o tag ?úng
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

    // An toàn h?n TakeDamage âm/d??ng: ?u tiên Set tr?c ti?p n?u b?n có th? s?a Player_Health
    private void TrySetHP(Player_Health p, int hpValue)
    {
        // Cách 1: n?u b?n cho phép thêm hàm SetHP(int) trong Player_Health thì g?i tr?c ti?p:
        // p.SetHP(hpValue);

        // Cách 2 (không s?a Player_Health): dùng TakeDamage/Heal theo chênh l?ch
        int diff = p.CurrentHP - hpValue;
        if (diff > 0) p.TakeDamage(diff);
        else if (diff < 0) p.Heal(-diff);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}