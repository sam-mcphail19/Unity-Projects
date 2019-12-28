using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    public GameObject player;
    public GameObject cloudPrefab;

    private float highestCloudHeight;
    private float distBetweenClouds = 1.5f;

    void Start() 
    {
        createCloud(player.transform.position - new Vector3(0, 2, 0));
    }

    void FixedUpdate()
    {
        if (player.transform.position.y > highestCloudHeight - 5) 
        {
            createCloud(new Vector3(Random.Range(-5f, 5f), highestCloudHeight + distBetweenClouds, 0));
        }
    }

    void createCloud(Vector3 pos) 
    {
        Instantiate(cloudPrefab, pos, Quaternion.identity);
        highestCloudHeight = pos.y;
    }

}
