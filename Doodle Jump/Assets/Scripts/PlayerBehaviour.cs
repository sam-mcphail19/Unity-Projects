using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour
{

    private Rigidbody body;
    private Collider player_collider;

    public float xSpeed = 250f;
    public float jumpSpeed = 400f;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        player_collider = GetComponent<Collider>();
    }

    void Update()
    {
        float translation = Input.GetAxis("Horizontal") * xSpeed * Time.fixedDeltaTime;
        body.velocity = new Vector3(translation, body.velocity.y);

        if (body.position.x + player_collider.bounds.extents.x < -5)
        {
            body.position = new Vector3(5, body.position.y);
        }
        else if (body.position.x > 5)
        {
            body.position = new Vector3(-5 - player_collider.bounds.extents.x, body.position.y);
        }

    }

    void OnTriggerEnter(Collider col)
    {
        RaycastHit hit;
        Ray downRay = new Ray(body.transform.position, Vector3.down);

        // Only jump when we're moving downwards, and the cloud is below us
        if (body.velocity.y <= 0 && Physics.Raycast(downRay, out hit, 0.5f))
        {
            body.velocity = new Vector3(body.velocity.x, jumpSpeed * Time.deltaTime);
        }
    }

    public void endGame() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
