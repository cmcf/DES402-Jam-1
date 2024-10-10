using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class PlayerDog : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    GameTimer gameTimer;
    MinigameManager minigameManager;
    AudioSource audioSource;

    [Header("Segment Prefabs")]
    public GameObject foodSegmentPrefab;
    public GameObject frontLegSegment;
    public GameObject backLegSegmentPrefab;
    public GameObject tailSegment;

    [Header("Segment List")]
    public List<Transform> segments = new List<Transform>(); // List of all player food segments
    List<Vector3> positions = new List<Vector3>(); // Positions for food segments to follow

    [Header("Player Settings")]
    [SerializeField] float segmentSpacing = 0.5f; // Distance between each food segment
    [SerializeField] float dogMoveSpeed = 2f;
    [SerializeField] float foodFollowSpeed = 6f;
    public int playerScore = 0;

    [Header("Timer Settings")]
    [SerializeField] float timerIncreaseAmount = 0.5f;
    [SerializeField] float timerDecreaseAmount = 0.5f;

    [Header("Player SFX")]
    public AudioClip moveSFX;
    public AudioClip hitSFX;

    public int screenID = -1;

    Vector2 inputDirection = Vector2.zero;
    Vector2 lastMoveDirection;

    float yPadding = 0.1f;
    float xPadding = 0.5f;

    public bool canFlip = false;

    void Start()
    {
        segments.Add(transform);

        audioSource = FindObjectOfType<AudioSource>();

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

            audioSource.PlayOneShot(moveSFX);
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
        if (dir.x > 0)
        {
            // Flip the player sprite when facing right
            transform.eulerAngles = new Vector3(0, 0, 0);
            spriteRenderer.flipX = true;
        }
        else if (dir.x < 0)
        {
            // No rotation for horizontal movement
            transform.eulerAngles = new Vector3(0, 0, 0);
            spriteRenderer.flipX = false;
        }
        else if (dir.y < 0)
        {
            // Rotate the player for downward movement
            transform.eulerAngles = new Vector3(0, 0, 90);
            spriteRenderer.flipX = false;
        }
        else if (dir.y > 0)
        {
            // Rotate the player for upward movement
            transform.eulerAngles = new Vector3(0, 0, -90);
            spriteRenderer.flipX = false;
        }
        // Flip segments based on players direction

        FlipSegments(dir);
    }

    void FlipSegments(Vector2 dir)
    {
        // Flip the first segment
        if (segments.Count > 1)
        {
            Transform firstSegment = segments[1];
            SpriteRenderer firstSegmentRenderer = firstSegment.GetComponent<SpriteRenderer>();

            if (dir.x > 0)
            {
                firstSegmentRenderer.flipX = true;
            }
            else if (dir.x < 0)
            {
                firstSegmentRenderer.flipX = false;
            }
        }

        // Flip the back legs segment
        if (segments.Count > 2)
        {
            Transform backLegSegment = segments[2];
            SpriteRenderer backLegSegmentRenderer = backLegSegment.GetComponent<SpriteRenderer>();

            if (dir.x > 0)
            {
                backLegSegmentRenderer.flipX = true;
            }
            else if (dir.x < 0)
            {
                backLegSegmentRenderer.flipX = false;
            }
        }

        // Apply the same rotation to the last segment
        if (segments.Count > 3)
        {
            Transform lastSegment = segments[segments.Count - 1]; // Last segment
            SpriteRenderer lastSegmentRenderer = lastSegment.GetComponent<SpriteRenderer>();

            if (dir.x > 0)
            {
                lastSegmentRenderer.flipX = true;
                lastSegment.eulerAngles = new Vector3(0, 0, 0);
            }
            else if (dir.x < 0)
            {
                lastSegmentRenderer.flipX = false;
                lastSegment.eulerAngles = new Vector3(0, 0, 0);
            }
            else if (dir.y < 0)
            {
                lastSegment.eulerAngles = new Vector3(0, 0, 90);
                lastSegmentRenderer.flipX = false;
            }
            else if (dir.y > 0)
            {
                lastSegment.eulerAngles = new Vector3(0, 0, -90);
                lastSegmentRenderer.flipX = false;
            }
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
        // If it's the first food collected, spawn the first segment with front legs
        if (segments.Count == 1)
        {
            // Position the first segment behind the player
            Vector3 firstSegmentPosition = transform.position - transform.up * segmentSpacing;

            // Instantiate the first segment
            GameObject firstSegment = Instantiate(frontLegSegment, firstSegmentPosition, Quaternion.identity);

            // Add the first segment directly after the dog's head
            segments.Add(firstSegment.transform);
        }
        else if (segments.Count == 2)
        {
            // Instantiate the back leg segment behind the front legs
            Vector3 backLegPosition = segments[1].position - segments[1].up * segmentSpacing;

            // Instantiate the back leg segment
            GameObject backLegSegment = Instantiate(backLegSegmentPrefab, backLegPosition, Quaternion.identity);

            // Add the back leg segment to the list
            segments.Add(backLegSegment.transform);
        }
        else if (segments.Count == 3)
        {
            // Instantiate the tail segment behind the back legs
            Vector3 tailSegmentPosition = segments[2].position - segments[2].up * segmentSpacing;

            // Instantiate the tail segment
            GameObject tailSegmentObj = Instantiate(tailSegment, tailSegmentPosition, Quaternion.identity);

            // Add the tail segment to the list
            segments.Add(tailSegmentObj.transform);
        }
        else
        {
            // Place the food segment between the front legs and the back legs
            Transform frontLegSegment = segments[1];
            Transform backLegSegment = segments[2];

            // Calculates the position for the food segment between the front and back legs
            Vector3 foodSegmentPosition = (frontLegSegment.position + backLegSegment.position) / 2;

            // Instantiate a new food segment
            GameObject foodSegment = Instantiate(foodSegmentPrefab, foodSegmentPosition, Quaternion.identity);

            // Add the new food segment in between the front legs and the back legs
            segments.Insert(2, foodSegment.transform);
        }
    }


    void UpdateLastSegment()
    {
        if (segments.Count > 1)
        {
            // Set the last segment to the tail segment
            Transform lastSegment = segments[segments.Count - 1];
            lastSegment.GetComponent<SpriteRenderer>().sprite = tailSegment.GetComponent<SpriteRenderer>().sprite;
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

        // Player loses food segments if collided with obstacles
        if (collision.CompareTag("Obstacle"))
        {
            audioSource.PlayOneShot(hitSFX);

            // Checks if the last segment is food and remove it
            if (segments.Count > 1 && segments[segments.Count - 1].CompareTag("Food"))
            {
                Transform lastFoodSegment = segments[segments.Count - 1];
                segments.RemoveAt(segments.Count - 1);
                Destroy(lastFoodSegment.gameObject);
            }
            else if (segments.Count > 3)
            {
                // Remove the third-to-last segment
                Transform segmentToRemove = segments[segments.Count - 3];
                segments.RemoveAt(segments.Count - 3);
                Destroy(segmentToRemove.gameObject);

                // Update the new last segment
                UpdateLastSegment();
            }

            if (minigameManager != null)
            {
                // Decrease game timer
                minigameManager.GetTimer().AddTime(-timerDecreaseAmount);
            }
            // Destroy the obstacle
            Destroy(collision.gameObject); 
        }
    }
}
