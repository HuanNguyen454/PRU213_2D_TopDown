using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Status")]
    public bool hasFood = false;
    public bool hasWater = false;

    // Con chó chúng ta kiểm tra qua CompanionManager đã viết trước đó
    public bool IsDogFound => CompanionManager.Instance != null && CompanionManager.Instance.isUnlocked;

    [Header("UI (Optional)")]
    // Bạn có thể kéo các icon vật phẩm vào đây để làm mờ/sáng khi nhặt được
    public GameObject foodIcon;
    public GameObject waterIcon;
    public GameObject dogIcon;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void CollectItem(string itemName)
    {
        if (itemName == "Food") { hasFood = true; if (foodIcon) foodIcon.SetActive(true); }
        if (itemName == "Water") { hasWater = true; if (waterIcon) waterIcon.SetActive(true); }

        Debug.Log($"Đã nhặt: {itemName}. Food: {hasFood}, Water: {hasWater}, Dog: {IsDogFound}");
    }

    public bool CanEscape()
    {
        return hasFood && hasWater && IsDogFound;
    }
}