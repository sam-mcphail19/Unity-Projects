using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{

    private Rigidbody2D body;
    private SpriteRenderer sprite;

    public float xSpeed = 1000.0f;
    public float jumpSpeed = 100.0f;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float translation = Input.GetAxis("Horizontal") * xSpeed * Time.deltaTime;
        body.velocity = new Vector2(translation, body.velocity.y);

        if (body.position.x + sprite.bounds.extents.x < -5)
        {
            body.position = new Vector2(5, body.position.y);
        }
        else if (body.position.x > 5)
        {
            body.position = new Vector2(-5 - sprite.bounds.extents.x, body.position.y);
        }

    }

    void OnCollisionEnter2D(Collision2D col)
    {
        float bottom = body.position.y - sprite.bounds.extents.y;

        // Only jump when we're moving downwards, and not from the side
        if (body.velocity.y <= 0 && bottom >= col.rigidbody.position.y)
        {
            body.velocity = new Vector2(body.velocity.x, jumpSpeed * Time.deltaTime);
        }
    }

}
