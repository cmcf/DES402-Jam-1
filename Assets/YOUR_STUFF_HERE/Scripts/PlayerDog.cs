using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDog : MonoBehaviour
{
    public GameObject foodSegmentPrefab;
    public GameObject firstSegmentPrefab;
    public GameObject lastSegmentPrefab;

    public List<Transform> segments = new List<Transform>(); // List of all player food segments
    List<Vector3> positions = new List<Vector3>(); // Positions for food segments to follow
    [SerializeField] float segmentSpacing = 0.5f; // Distance between each food segment

    [SerializeField] float dogMoveSpeed = 2f;

    [SerializeField] float timerIncreaseAmount = 0.5f;
    [SerializeField] float timerDecreaseAmount = 0.5f;
    public int screenID = -1;
    Vector2 inputDirection = Vector2.zero;

    public int playerScore = 0;

    [SerializeField] float foodFollowSpeed = 6f;

    SpriteRenderer spriteRenderer;
    GameTimer gameTimer;

    MinigameManager minigameManager;

    Vector2 lastMoveDirection;

    float yPadding = 0.1f;
    float xPadding = 0.5f;

    void Start()
    { 
        segments.Add(transform);

        spriteRenderer = GetComponent<SpriteRenderer>();
        minigameManager = FindObjectOfType<MinigameManager>();


        spriteRenderer = GetComponent<SpriteRenderer>();
        minigameManager = FindObjectOfType<MinigameManager>();
    }

    void Update()
    {
        Move();

        MoveFoodSegments();

        ClampScreen();
    }

    void Move()
    {
        Vector2 moveDirection;

        // Set movement direction
        if (inputDirection != Vector2.zero)
        {
            moveDirection = inputDirection;
        }
        else
        {
            moveDirection = lastMoveDirection;
        }

        // Update position based on the move direction
        transform.position += (Vector3)moveDirection * dogMoveSpeed * Time.deltaTime;

        // Rotate or flip the player if move direction changes
        if (moveDirection != Vector2.zero && moveDirection != lastMoveDirection)
        {
            ApplyRotationAndFlip(moveDirection);

            // Update last move direction
            lastMoveDirection = moveDirection;
        }
    }

    void MoveFoodSegments()
    {
        // Move each segment to the position behind the previous segment with smooth following
        for (int i = 1; i < segments.Count; i++)
        {
            Transform previousSegment = segments[i - 1];

            // Calculate the target position for the current segment based on the previous segment's position
            Vector3 targetPosition = previousSegment.position - previousSegment.up * segmentSpacing;

            // Use Lerp to smoothly move towards the target position
            segments[i].position = Vector3.Lerp(segments[i].position, targetPosition, Time.deltaTime * foodFollowSpeed);
        }
    }

    void ApplyRotationAndFlip(Vector2 dir)
    {
        // Determine the rotation based on the direction the player is facing 
        if (dir.x > 0) // Moving right
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            spriteRenderer.flipX = true;
        }
        else if (dir.x < 0) // Moving left
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            spriteRenderer.flipX = false;
        }
        else if (dir.y < 0) // Moving down
        {
            transform.eulerAngles = new Vector3(0, 0, 90);
            spriteRenderer.flipX = false;
        }
        else if (dir.y > 0) // Moving up
        {
            transform.eulerAngles = new Vector3(0, 0, -90);
            spriteRenderer.flipX = false;
        }

        // Apply the same rotation to the last segment
        if (segments.Count > 0)
        {
            // Rotate the last segment
            segments[segments.Count - 1].eulerAngles = transform.eulerAngles;
        }

    }

    public void ClampScreen()
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
        position.y = Mathf.Clamp(position.y, -4.5f, 4.5f);

        // Return the y clamped position
        return position;
    }

    void AddSegment()
    {
        // If it's the first food collected, spawn the first segment with legs
        if (segments.Count == 1)
        {
            // Position the first segment behind the player
            Vector3 firstSegmentPosition = transform.position - transform.up * segmentSpacing;

            // Instantiate the first segment
            GameObject firstSegment = Instantiate(firstSegmentPrefab, firstSegmentPosition, Quaternion.identity);

            // Add the first segment directly after the dogs head
            segments.Add(firstSegment.transform);
        }
        else if (segments.Count == 2)
        {
            // Instantiate the last segment behind the first segment
            Vector3 lastSegmentPosition = segments[0].position - segments[0].up * segmentSpacing;

            // Instantiate the last segment
            GameObject lastSegmentObj = Instantiate(lastSegmentPrefab, lastSegmentPosition, Quaternion.identity);

            // Add the last segment to the list
            segments.Add(lastSegmentObj.transform);
        }
        else 
        {
            // Place the food segments between the first segment and the last segment
            Transform secondLastSegment = segments[segments.Count - 2];

            // Calculate the position for the new food segment
            Vector3 newSegmentPosition = secondLastSegment.position - secondLastSegment.up * segmentSpacing;

            // Instantiate a new food segment
            GameObject newSegment = Instantiate(foodSegmentPrefab, newSegmentPosition, Quaternion.identity);

            // Add the new segment before the last one
            segments.Insert(segments.Count - 1, newSegment.transform);
        }
    }

    void UpdateLastSegment()
    {
        if (segments.Count > 1)
        {
            // Set the last segment to the tail segment
            Transform lastSegment = segments[segments.Count - 1];
            lastSegment.GetComponent<SpriteRenderer>().sprite = lastSegmentPrefab.GetComponent<SpriteRenderer>().sprite;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Players scale is increased if they collide with food
        if (collision.CompareTag("Food"))
        {
            // Spawn a new segment and place it behind the last one
            AddSegment();

            // Increase the player's score when food is collected
            playerScore += 1;
            Debug.Log("Player Score: " + playerScore);

            if (minigameManager != null)
            {
                // Timer is increased by the increase amount when food is collected
                minigameManager.GetTimer().AddTime(timerIncreaseAmount);
            }

            Destroy(collision.gameObject);
        }

        // Food segmenet is removed from player if they collide with an obstacle
        if (collision.CompareTag("Obstacle"))
        {
            if (segments.Count > 1) // Ensure there's at least one segment to remove
            {
                // Remove the last segment
                Transform lastSegment = segments[segments.Count - 1];
                segments.RemoveAt(segments.Count - 1);
                Destroy(lastSegment.gameObject);

                // Update the new last segment to have the "tail" sprite
                UpdateLastSegment();

            }

            if (minigameManager != null)
            {
                // Decrease game timer
                minigameManager.GetTimer().AddTime(-timerDecreaseAmount);
            }

            Destroy(collision.gameObject);
        }
    }
}
