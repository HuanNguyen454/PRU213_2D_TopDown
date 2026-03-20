using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "Game/Food Data")]
public class FoodDataSO : ScriptableObject
{
    public Sprite sprite;

    [Header("Heal")]
    public int healAmount;

    [Header("Speed")]
    public float speedBoost;
    public float duration;
}