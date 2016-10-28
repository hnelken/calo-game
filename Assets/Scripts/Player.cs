using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	#region Editor Properties

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1; 
	public float jumpTime = .4f;
	public float moveSpeed = 6;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallJumpAway;

	#endregion

	#region Private Variables
	float accTimeAir = .2f;
	float accTimeGround = .1f;
	float timeToWallUnstick;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	float xSmoothing;
	Vector3 velocity;

	Controller2D controller;
	
	Vector2 directionalInput;
	bool wallSliding;
	int wallDirX;
	#endregion

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
			velocity.y = 0;
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
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
			}
			// Jump away
			else {
				velocity.x = -wallDirX * wallJumpAway.x;
				velocity.y = wallJumpAway.y;
			}
		}

		// Normal jump
		if (controller.collisions.below) {
			velocity.y = maxJumpVelocity;
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
