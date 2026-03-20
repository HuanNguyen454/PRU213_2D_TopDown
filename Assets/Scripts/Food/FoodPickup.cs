using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPickup : MonoBehaviour
{
    [Header("Data")]
    public List<FoodDataSO> foodDatabase;

    [Header("Component")]
    public SpriteRenderer spriteRenderer;

    [Header("Setting")]
    public float lifeTime = 20f;

    private FoodDataSO currentData;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        RandomFood();
        Destroy(gameObject, lifeTime);
    }

    void RandomFood()
    {
        if (foodDatabase == null || foodDatabase.Count == 0)
        {
            Debug.LogWarning("Food Database is empty!");
            return;
        }

        int index = Random.Range(0, foodDatabase.Count);
        currentData = foodDatabase[index];

        spriteRenderer.sprite = currentData.sprite;

        Debug.Log("Spawn item: " + currentData.name);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player_Health player = other.GetComponent<Player_Health>();
        Player_Movement movement = other.GetComponent<Player_Movement>();

        if (currentData == null) return;

        Debug.Log("Ăn item: " + currentData.name);

        // ❤️ HEAL
        if (currentData.healAmount > 0 && player != null)
        {
            player.Heal(currentData.healAmount);
        }

        // ⚡ SPEED
        if (currentData.speedBoost > 0 && movement != null)
        {
            StartCoroutine(BoostSpeed(movement, currentData.speedBoost, currentData.duration));
        }

        Destroy(gameObject);
    }

    IEnumerator BoostSpeed(Player_Movement movement, float amount, float duration)
    {
        float originalSpeed = movement.MoveSpeed;

        movement.AddSpeed(amount);

        Debug.Log("Speed +" + amount);

        yield return new WaitForSeconds(duration);

        movement.ResetSpeed(originalSpeed);

        Debug.Log("Speed reset");
    }
}