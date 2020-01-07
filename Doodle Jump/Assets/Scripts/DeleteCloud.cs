using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteCloud : MonoBehaviour
{
    
    void Update()
    {
        // Destroy cloud when it is no longer on screen
        if (Camera.main.WorldToViewportPoint(transform.position).y < 0)
        {
            Destroy(gameObject);
        }   
    }
}
