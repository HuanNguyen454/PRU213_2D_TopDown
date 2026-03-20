using UnityEngine;

public class WinZone : MonoBehaviour
{
    [SerializeField] private GameObject winUI; // Bảng thông báo Win
    private bool gameEnded = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameEnded) return;

        if (other.CompareTag("Player"))
        {
            if (QuestManager.Instance.CanEscape())
            {
                WinGame();
            }
            else
            {
                Debug.Log("Bạn chưa đủ vật phẩm (Cần: Chó, Thức ăn, Nước)!");
                // Có thể hiện một dòng text nhắc nhở người chơi ở đây
            }
        }
    }

    void WinGame()
    {
        gameEnded = true;
        Time.timeScale = 0f; // Dừng game
        if (winUI) winUI.SetActive(true);
        Debug.Log("CHÚC MỪNG! BẠN ĐÃ THOÁT KHỎI ĐẢO ZOMBIE!");
    }
}