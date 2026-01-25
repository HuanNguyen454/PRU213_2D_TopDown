using System.Collections;
using System.Collections.Generic;


using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject enemyPrefab;   // kéo Zombie_Big.prefab vào ?ây

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;  // kéo các ?i?m spawn (Empty objects) vào ?ây

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f; // m?i 2s spawn 1 con
    [SerializeField] private int maxAlive = 10;        // t?i ?a bao nhiêu con ?ang t?n t?i
    [SerializeField] private bool spawnOnStart = true;

    private int aliveCount = 0;

    private void Start()
    {
        if (spawnOnStart)
            StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (enemyPrefab == null) continue;
            if (spawnPoints == null || spawnPoints.Length == 0) continue;
            if (aliveCount >= maxAlive) continue;

            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, p.position, Quaternion.identity);

        aliveCount++;

        // Khi enemy b? destroy (ch?t) thì gi?m count
        EnemyLifeHook hook = enemy.AddComponent<EnemyLifeHook>();
        hook.onDestroyed = () => aliveCount--;
    }

    // helper component: khi object b? Destroy thì callback
    private class EnemyLifeHook : MonoBehaviour
    {
        public System.Action onDestroyed;
        private void OnDestroy() => onDestroyed?.Invoke();
    }
}
