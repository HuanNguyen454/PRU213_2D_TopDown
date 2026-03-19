using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Scene2";
    [SerializeField] private string playerTag = "Player";

    [Header("Portal")]
    [SerializeField] private string targetPortalID; // NEW

    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading) return;
        if (!other.CompareTag(playerTag)) return;

        isLoading = true;

        Time.timeScale = 1f;

        // QUAN TRỌNG: dùng portal system
        GameManager.Instance.SaveGameWithPortal(targetPortalID);

        SceneManager.LoadScene(nextSceneName);
    }
}