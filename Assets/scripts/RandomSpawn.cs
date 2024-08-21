using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
    public GameObject candle;
    public int spawnCount = 5;

    // Update is called once per frame
    void Update()
    {
        while (spawnCount != 0) {
            Vector3 ran = new Vector3(Random.Range(0, 99), 0.35f, Random.Range(0, 99));
            Instantiate(candle, ran, Quaternion.identity);
            spawnCount--;
        }
    }
}
