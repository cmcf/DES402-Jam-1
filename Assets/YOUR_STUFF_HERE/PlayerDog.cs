using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDog : MonoBehaviour
{
    [SerializeField] float dogMoveSpeed = 2f;
    public int screenID = -1;
    Vector2 inputDirection = Vector2.zero;

    void Update()
    {
        Vector2 moveDirection = inputDirection;
        transform.position += (Vector3)moveDirection * dogMoveSpeed * Time.deltaTime; // Time.deltaTime makes our movement consistent regardless of framerate
        transform.position = ScreenUtility.ClampToScreen(transform.position, screenID, 0.5f);
    }

    public void HandleDirectionalInput(Vector2 direction)
    {
        //Save the direciton to use later
        inputDirection = direction;
    }
}
