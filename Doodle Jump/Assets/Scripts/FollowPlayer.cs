using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public GameObject delete;
    public float threshold = 1.0f;
    public float step = 0.04f;

    private MeshRenderer playerRenderer;
    private bool hasBeenVisible = false;
    private float greatestY;

    private void Start()
    {
        playerRenderer = player.GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (player.transform.position.y > greatestY) 
        {
            greatestY = player.transform.position.y;
        }

        updateCameraHeight2();
        updateDeleteHeight();
        checkGameOver();
    }

    private void updateCameraHeight2() 
    {
        if (greatestY > transform.position.y)
        {
            transform.position = new Vector3(0, player.transform.position.y, transform.position.z);
        }
    }
    private void updateCameraHeight() 
    {
        if (greatestY - transform.position.y > threshold)
        {
            transform.position = new Vector3(0, transform.position.y + step, transform.position.z);
        }
    }
    private void updateDeleteHeight()
    {
        delete.transform.position = transform.position + new Vector3(0, -10, 0);
    }

    private void checkGameOver()
    {
        // isVisible returns false for the first tick before updates are made
        if (playerRenderer.isVisible) 
        {
            hasBeenVisible = true;
        } 
        else if (hasBeenVisible)
        {
            player.GetComponent<PlayerBehaviour>().endGame();
        }
    }

}
