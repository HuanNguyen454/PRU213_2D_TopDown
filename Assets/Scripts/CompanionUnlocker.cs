using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionUnlocker : MonoBehaviour
{
    [SerializeField] private GameObject dogVisual; // Cái ảnh con chó đứng yên

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (CompanionManager.Instance != null) // Thêm dòng kiểm tra này
            {
                CompanionManager.Instance.UnlockCompanion(transform.position);
                if (dogVisual != null) dogVisual.SetActive(false);
                Destroy(gameObject, 0.1f);
            }
            else
            {
                Debug.LogError("LỖI: Chưa tìm thấy CompanionManager trong Scene!");
            }
        }
    }
}