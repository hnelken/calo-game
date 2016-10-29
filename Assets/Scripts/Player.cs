using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 4;				// Maximum height of a jump
	public float minJumpHeight = 1; 			// Minimum height of a jump
	public float jumpTime = .4f;				// Duration of a jump
	public float moveSpeed = 6;					// Movement speed

	public float wallSlideSpeedMax = 3;			// Maximum descent speed while wall sliding
	public float wallStickTime = .25f;			// Total that wall stick resists movement away from wall
	
	public Vector2 wallJumpClimb;				// Wall jump climb velocity vector
	public Vector2 wallJumpDown;				// Wall jump down velocity vector
	public Vector2 wallJumpAway;				// Wall jump away velocity vector

	float accTimeAir = .2f;						// Acceleration time while airborne
	float accTimeGround = .1f;					// Acceleration time while grounded
	float timeToWallUnstick;					// Remaining time that wall stick resists movement away from wall

	float gravity;								// Strength of the gravity
	float maxJumpVelocity;						// Maximum velocity of a jump
	float minJumpVelocity;						// Minimum velocity of a jump
	float xSmoothing;							// Smoothing factor in x movement
	Vector3 velocity;							// General velocity vector

	Controller2D controller;					// Controller reference
	
	Vector2 directionalInput;					// Input vector
	bool wallSliding;							// True if wall sliding, false otherwise
	int wallDirX;								// Direction of the wall collision

	#region Unity Lifecycle
	// Initialization
	void Start () {
		// Set controller, gravity, min and max jump velocity
		controller = GetComponent<Controller2D> ();
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (jumpTime, 2);
		maxJumpVelocity = Mathf.Abs (gravity) * jumpTime;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	// Update is called once per frame
	void Update () {
		// Get velocity
		CalculateVelocity();

		// Constrain velocity to wall sliding
		CheckForWallSliding();

		// Move player with modified velocity
		controller.Move (velocity * Time.deltaTime, directionalInput);

		// Allow platform drop through
		if (controller.collisions.above || controller.collisions.below) {
			if (controller.collisions.slidingDownSlope) {
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			}
			else {
				velocity.y = 0;
			}
		}
	}
	#endregion

	#region Public API
	// Sets player directional input
	public void SetDirectionalInput(Vector2 input) {
		directionalInput = input;
	}

	// Handle jumping
	public void OnJumpInputDown() {
		// Check for wall jump
		if (wallSliding) {
			// Jump climb
			if (wallDirX == directionalInput.x) {
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			}
			// Jump down
			else if (directionalInput.x == 0) {
				velocity.x = -wallDirX * wallJumpDown.x;
				velocity.y = wallJumpDown.y;
			}
			// Jump away
			else {
				velocity.x = -wallDirX * wallJumpAway.x;
				velocity.y = wallJumpAway.y;
			}
		}

		// Normal jump
		if (controller.collisions.below) {
			if (controller.collisions.slidingDownSlope) {
				if (directionalInput.x != -Mathf.Sign (controller.collisions.slopeNormal.x)) {
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			}
			else {
				velocity.y = maxJumpVelocity;
			}
		}
	}

	// Handle variable jump height on jump input up
	public void OnJumpInputUp() {
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}
	#endregion

	#region Private API
	// Calculate the player velocity based on input
	private void CalculateVelocity() {
		float targetVelocityX = directionalInput.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref xSmoothing, (controller.collisions.below) ? accTimeGround : accTimeAir);
		velocity.y += gravity * Time.deltaTime;
	}

	// Check if the player is wall sliding
	private void CheckForWallSliding() {
		wallSliding = false;
		wallDirX = (controller.collisions.left) ? -1 : 1;
		if (PlayerIsWallSliding()) {
			HandleWallSliding();
		}
	}
	
	// Modify player velocity to handle wall sliding
	private void HandleWallSliding() {
		wallSliding = true;

		// Constrain vertical speed to wall slide speed
		if (velocity.y < -wallSlideSpeedMax) {
			velocity.y = -wallSlideSpeedMax;
		}

		// Check or reset wall stick status
		if (timeToWallUnstick > 0) {
			xSmoothing = 0;
			velocity.x = 0;

			// Adjust stick time if player is unsticking
			if (PlayerIsUnstickingFromWall()) {
				timeToWallUnstick -= Time.deltaTime;
			}
			else {
				timeToWallUnstick = wallStickTime;
			}
		}
		else {
			timeToWallUnstick = wallStickTime;
		}
	}

	// Returns true if the player is wall sliding
	private bool PlayerIsWallSliding() {
		return (controller.collisions.left || controller.collisions.right) 
			&& !controller.collisions.below && velocity.y < 0;
	}

	// Returns true if the player is moving away from a wall
	private bool PlayerIsUnstickingFromWall() {
		return directionalInput.x != wallDirX && directionalInput.x != 0;
	}
	#endregion
}
