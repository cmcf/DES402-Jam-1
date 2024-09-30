using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDog : MonoBehaviour
{
    [SerializeField] float dogMoveSpeed = 2f;
    public int screenID = -1;
    Vector2 inputDirection = Vector2.zero;

    SpriteRenderer spriteRenderer;

    float yPadding = 0.1f;
    float xPadding = 0.5f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Player moves at a consistent speed
        Vector2 moveDirection = inputDirection;
        transform.position += (Vector3)moveDirection * dogMoveSpeed * Time.deltaTime;

        // Rotate the player based on the direction
        transform.eulerAngles = new Vector3(0, 0, GetAngleFromVector(moveDirection));

        ClampScreen();
    }

    float GetAngleFromVector(Vector2 dir)
    {
        // Default rotation angle is 0 when player is facing up
        float angle = 0;

        // Determine the angle based on direction
        if (dir.x > 0) // Moving right
        {
            angle = -90;  // Rotate -90 degrees for right
        }
        else if (dir.x < 0) // Moving left
        {
            angle = 90;  // Rotate 90 degrees for left
        }
        else if (dir.y < 0) // Moving down
        {
            angle = 180;  // Rotate 180 degrees for down
        }
        else if (dir.y > 0) // Moving up
        {
            angle = 0;  // Keep 0 degrees for up
        }

        return angle;
    }

    private void ClampScreen()
    {
        // Clamps the x-position
        Vector3 clampedXPosition = ScreenUtility.ClampToScreen(transform.position, screenID, xPadding);

        // Clamps the y-position
        Vector3 clampedYPosition = ClampToScreen(transform.position, screenID, yPadding);

        // Apply the clamped x and y positions to the player transform
        transform.position = new Vector3(clampedXPosition.x, clampedYPosition.y, transform.position.z);
    }

    public void HandleDirectionalInput(Vector2 direction)
    {
        // Save the direction to use later
        inputDirection = direction;
    }

    public static Vector3 ClampToScreen(Vector3 position, int screenID, float yPadding)
    {
        // Clamp the y-position to the fixed screen range
        position.y = Mathf.Clamp(position.y, -7.5f, 7f);

        // Return the y clamped position
        return position;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Players scale is increased if they collide with food
        if (collision.CompareTag("Food"))
        {
            Debug.Log("Eaten");
            Vector3 newScale = transform.localScale;
            newScale.y += 1;
            transform.localScale = newScale;

            Destroy(collision.gameObject);
        }

        // Player's scale is decreased if they collide with obstacles
        if (collision.CompareTag("Obstacle"))
        {
            Debug.Log("Eaten");
            Vector3 newScale = transform.localScale;

            // Decrease the y scale, but ensure it doesn't go below 1
            newScale.y = Mathf.Max(newScale.y - 1, 1f);

            transform.localScale = newScale;

            Destroy(collision.gameObject);
        }
    }
}
