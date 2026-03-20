using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum ItemType { Food, Water }
    public ItemType type;
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Cập nhật vào QuestManager
            QuestManager.Instance.CollectItem(type.ToString());

            // Phát âm thanh nếu có
            if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            // Biến mất
            Destroy(gameObject);
        }
    }
}