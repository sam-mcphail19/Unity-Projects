using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour
{
    public float xSpeed = 250f;
    public float jumpSpeed = 400f;

    private Rigidbody body;
    private Renderer player_renderer;

    private Vector3 viewportPos;
    private bool hasBeenVisible = false;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        player_renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // isVisible returns false for the first tick before updates are made
        if (player_renderer.isVisible)
        {
            hasBeenVisible = true;
        }

        body.velocity = new Vector3(Input.GetAxis("Horizontal") * xSpeed * Time.deltaTime, body.velocity.y);
        
        viewportPos = Camera.main.WorldToViewportPoint(transform.position);

        CheckWrapX();
        CheckGameOver();

    }

    void OnTriggerEnter(Collider col)
    {
        Ray downRay = new Ray(body.transform.position, Vector3.down);

        // Only jump when we're moving downwards, and the cloud is below us
        if (body.velocity.y <= 0 && Physics.Raycast(downRay, out _, 0.5f))
        {
            body.velocity = new Vector3(body.velocity.x, jumpSpeed * Time.deltaTime);
        }
    }

    private void CheckWrapX() 
    {
        if (hasBeenVisible)
        {
            if (viewportPos.x < 0 || viewportPos.x > 1) // wrap from left to right
            {
                transform.position = new Vector3(-transform.position.x, transform.position.y, transform.position.z);
            }
        }

    }

    private void CheckGameOver()
    {
        if (hasBeenVisible)
        {
            if (viewportPos.y < 0)
            {
                EndGame();
            }
        }
    }
    public void EndGame() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
