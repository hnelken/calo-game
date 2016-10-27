using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float jumpHeight = 4;
	public float jumpTime = .4f;
	public float moveSpeed = 6;
	float accTimeAir = .2f;
	float accTimeGround = .1f;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallJumpAway;

	float gravity;
	float jumpVelocity;
	float xSmoothing;
	Vector3 velocity;

	Controller2D controller;

	// Use this for initialization
	void Start () {
		controller = GetComponent<Controller2D> ();
		gravity = -(2 * jumpHeight) / Mathf.Pow (jumpTime, 2);
		jumpVelocity = Mathf.Abs (gravity) * jumpTime;
	}
	
	// Update is called once per frame
	void Update () {
		
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref xSmoothing, (controller.collisions.below) ? accTimeGround : accTimeAir);

		bool wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) 
		    && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) {
				xSmoothing = 0;
				velocity.x = 0;

				if (input.x != wallDirX && input.x != 0) {
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


		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		if (Input.GetKeyDown (KeyCode.Space)) {

			if (wallSliding) {
				if (wallDirX == input.x) {
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				}
				else if (input.x == 0) {
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				}
				else {
					velocity.x = -wallDirX * wallJumpAway.x;
					velocity.y = wallJumpAway.y;
				}
			}
			if (controller.collisions.below) {
				velocity.y = jumpVelocity;
			}
		}

		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime);
	}
}
