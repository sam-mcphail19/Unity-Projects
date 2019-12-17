using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public Rigidbody player;
    public Collider delete;
    public float threshold = 1.0f;
    public float step = 0.1f;

    void Update()
    {
        if (player.position.y - transform.position.y > threshold) 
        { 
            transform.position = new Vector3(0, transform.position.y + step, transform.position.z);
            delete.transform.position = new Vector3(0, delete.transform.position.y + step, delete.transform.position.z);
        }
    }
}
