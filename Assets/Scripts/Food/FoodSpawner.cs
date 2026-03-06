using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;

    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    void Start()
    {
        StartCoroutine(SpawnFoodFlow());
    }

    IEnumerator SpawnFoodFlow()
    {
        yield return new WaitForSeconds(10f);
        SpawnFood();

        yield return new WaitForSeconds(10f);
        SpawnFood();

        yield return new WaitForSeconds(10f);
        SpawnFood();
    }

    void SpawnFood()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        Vector2 spawnPosition = new Vector2(randomX, randomY);

        Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
    }
}