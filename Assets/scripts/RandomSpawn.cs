using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
    public GameObject candle;
    public int spawnCount = 5;
    private bool hasSpawned = false;

    void Start()
    {
        if (!hasSpawned)
        {
            StartCoroutine(SpawnCandlesGradually());
        }
    }

    IEnumerator SpawnCandlesGradually()
    {
        hasSpawned = true;
        int originalCount = spawnCount;

        for (int i = 0; i < originalCount; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(0, 99), 0.35f, Random.Range(0, 99));
            Instantiate(candle, spawnPosition, Quaternion.identity);
            spawnCount--;
            yield return new WaitForSeconds(0.1f); // Small delay between spawns
        }
    }
}
