using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDog : MonoBehaviour
{
    [SerializeField] float dogMoveSpeed = 2f;
    public int screenID = -1;
    Vector2 inputDirection = Vector2.zero;

    float yPadding = 0.1f;
    float xPadding = 0.5f;

    void Update()
    {
        // Player moves at a consistent speed
        Vector2 moveDirection = inputDirection;
        transform.position += (Vector3)moveDirection * dogMoveSpeed * Time.deltaTime;

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
}
