using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDog : MonoBehaviour
{
    public GameObject foodSegmentPrefab; // Prefab of the food segment to spawn
    public List<Transform> segments = new List<Transform>(); // List of all player food segments

    private List<Vector3> positions = new List<Vector3>(); // Positions for food segments to follow

    public float segmentSpacing = 0.5f; // Distance between each food segment

    [SerializeField] float dogMoveSpeed = 2f;
    [SerializeField] float dogSizeAmount = 0.2f;

    [SerializeField] float timerIncreaseAmount = 0.5f;
    [SerializeField] float timerDecreaseAmount = 0.5f;
    public int screenID = -1;
    Vector2 inputDirection = Vector2.zero;

    public int playerScore = 0;

    SpriteRenderer spriteRenderer;
    GameTimer gameTimer;

    MinigameManager minigameManager;

    Vector2 lastMoveDirection;

    float yPadding = 0.1f;
    float xPadding = 0.5f;

    void Start()
    {
        // Add the initial player object to the list of segments
        segments.Add(transform);

        spriteRenderer = GetComponent<SpriteRenderer>();
        minigameManager = FindObjectOfType<MinigameManager>();
    }

    void Update()
    {
        // Player moves at a consistent speed
        Vector2 moveDirection = inputDirection;
        transform.position += (Vector3)moveDirection * dogMoveSpeed * Time.deltaTime;

        // Rotates the player only if the move direction has changed
        if (moveDirection != Vector2.zero && moveDirection != lastMoveDirection)
        {
            transform.eulerAngles = new Vector3(0, 0, GetAngleFromVector(moveDirection));
            // Updates last move direction
            lastMoveDirection = moveDirection; 
        }

        // Add the player's current position to the list of positions at the start of each frame
        positions.Insert(0, transform.position);

        // Ensure the number of stored positions matches the number of segments
        if (positions.Count > segments.Count)
        {
            positions.RemoveAt(positions.Count - 1);
        }

        // Move each segment to the position behind the previous segment with smooth following
        for (int i = 1; i < segments.Count; i++)
        {
            Transform previousSegment = segments[i - 1];

            // Calculate the target position for the current segment based on the previous segment's position
            Vector3 targetPosition = previousSegment.position - previousSegment.up * segmentSpacing;

            // Gradually move the current segment towards the target position using Lerp for smooth movement
            segments[i].position = Vector3.Lerp(segments[i].position, targetPosition, Time.deltaTime * dogMoveSpeed);
        }

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
        position.y = Mathf.Clamp(position.y, -4.5f, 4.5f);

        // Return the y clamped position
        return position;
    }

    void AddSegment()
    {
        // Get the last segment in the list
        Transform lastSegment = segments[segments.Count - 1];

        // Calculates the position for the new segment behind the last segment
        Vector3 newSegmentPosition = lastSegment.position - lastSegment.up * segmentSpacing;

        // Instantiate a new segment at the calculated position
        GameObject newSegment = Instantiate(foodSegmentPrefab, newSegmentPosition, Quaternion.identity);

        // Add the new segment to the list
        segments.Add(newSegment.transform);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Players scale is increased if they collide with food
        if (collision.CompareTag("Food"))
        {
            //OLD CODE
            //Vector3 newScale = transform.localScale;
            //newScale.y += dogSizeAmount;
            //transform.localScale = newScale;

            // Spawn a new food segment and place it behind the last one
            AddSegment();

            // Increase the player's score when food is collected
            playerScore += 1;
            Debug.Log("Player Score: " + playerScore);

            if (minigameManager!= null)
            {
                // Timer is increased by the increase amount when food is collected
                minigameManager.GetTimer().AddTime(timerIncreaseAmount);
            }

            Destroy(collision.gameObject);
        }

        // Player's scale is decreased if they collide with obstacles
        if (collision.CompareTag("Obstacle"))
        {
            Vector3 newScale = transform.localScale;

            // Decrease the y scale, but ensure it doesn't go below 1
            newScale.y = Mathf.Max(newScale.y - dogSizeAmount, 1f);

            if (minigameManager != null)
            {
                // Decrease game timer
                minigameManager.GetTimer().AddTime(-timerDecreaseAmount);
            }

            transform.localScale = newScale;

            Destroy(collision.gameObject);
        }
    }
}
