using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;

    [Header("Spawn Area")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("Spawn Setting")]
    public float spawnDelay = 5f;   // th?i gian spawn
    public int maxFood = 10;        // s? item t?i ?a trên map

    private List<GameObject> foods = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);

            // Xóa item ?ã b? ?n (null)
            foods.RemoveAll(f => f == null);

            // N?u ch?a ?? s? l??ng ? spawn thêm
            if (foods.Count < maxFood)
            {
                SpawnFood();
            }
        }
    }

    void SpawnFood()
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        Vector2 pos = new Vector2(x, y);

        GameObject food = Instantiate(foodPrefab, pos, Quaternion.identity);

        foods.Add(food);

        Debug.Log("Spawn thêm item");
    }
}