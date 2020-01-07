using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public float threshold = 1.0f;
    public float step = 0.04f;

    private float greatestY;

    void Update()
    {
        if (player.transform.position.y > greatestY) 
        {
            greatestY = player.transform.position.y;
        }

        if (greatestY > transform.position.y)
        {
            transform.position = new Vector3(0, player.transform.position.y, transform.position.z);
        }
    }
}
