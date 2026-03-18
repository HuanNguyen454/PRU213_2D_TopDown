using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        savePath = Application.persistentDataPath + "/save.json";
    }

    public void SaveGame(Player_Health player)
    {
        GameData data = new GameData();

        data.playerHP = player.CurrentHP;

        data.playerPosX = player.transform.position.x;
        data.playerPosY = player.transform.position.y;

        data.currentScene = SceneManager.GetActiveScene().name;

        if (CompanionManager.Instance != null)
        {
            data.isDogUnlocked = CompanionManager.Instance.isUnlocked;
        }

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(savePath, json);

        Debug.Log("Game saved to: " + savePath);
    }

    public GameData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file found");
            return null;
        }

        string json = File.ReadAllText(savePath);

        GameData data = JsonUtility.FromJson<GameData>(json);

        return data;
    }
}