using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {

	// MARK: - Public References

	public CollisionInfo collisions;


	// MARK: - Private Variables

	float maxClimbAngle = 80;
	float maxDescendAngle = 75;
	Vector2 playerInput;


	// MARK: - Public API

	public override void Start() {
		base.Start ();
		collisions.faceDir = 1;
	}

	public void Move(Vector3 velocity, bool onPlatform) {
		Move (velocity, Vector2.zero, onPlatform);
	}

	// Move the player with a given velocity
	public void Move(Vector3 velocity, Vector2 input, bool onPlatform = false) {

		// Get origins for collision detection rays
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.velocityOld = velocity;
		playerInput = input;

		if (velocity.x != 0) {
			collisions.faceDir = (int) Mathf.Sign(velocity.x);
		}

		// Constrain horizontal velocity
		if (velocity.y < 0) {
			DescendSlope(ref velocity);
		}

		HorizontalCollisions (ref velocity);
		
		// Constrain vertical velocity
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		// Perform the movements
		transform.Translate (velocity);

		if (onPlatform) {
			collisions.below = true;
		}
	}


	// MARK: - Private API

	// Constrain a velocity vector reference to vertical obstacles
	void HorizontalCollisions (ref Vector3 velocity) {
		float dirX = collisions.faceDir;
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		if (Mathf.Abs(velocity.x) < skinWidth) {
			rayLength = 2 * skinWidth; 
		}
		
		for (int i = 0; i < hRayCount; i++) {
			Vector2 rayOrigin = (dirX == -1) ? origins.bottomLeft : origins.bottomRight;
			rayOrigin += Vector2.up * hRaySpacing * i;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, collisionMask);
			
			if (hit) {

				if (hit.distance == 0) {
					continue;
				}

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (i == 0 && slopeAngle < maxClimbAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
					float distToSlope = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distToSlope = hit.distance - skinWidth;
						velocity.x -= distToSlope * dirX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distToSlope * dirX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					velocity.x = (hit.distance - skinWidth) * dirX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}

					collisions.left = dirX == -1;
					collisions.right = dirX == 1;
				}
			}

			Debug.DrawRay(rayOrigin, Vector2.right * dirX * rayLength, Color.white);
		}
	}

	// Constrain a velocity vector reference to vertical obstacles
	void VerticalCollisions (ref Vector3 velocity) {
		float dirY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		for (int i = 0; i < vRayCount; i++) {
			Vector2 rayOrigin = (dirY == -1) ? origins.bottomLeft : origins.topLeft;
			rayOrigin += Vector2.right * (vRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, collisionMask);

			if (hit) {
				if (hit.collider.tag == "Through") {
					if (dirY == 1 || hit.distance == 0) {
						continue;
					}
					if (collisions.fallingThroughPlatform) {
						continue;
					}
					if (playerInput.y == -1) {
						collisions.fallingThroughPlatform = true;
						Invoke ("ResetFallingThroughPlatform", .5f);
						continue;
					}
				}

				velocity.y = (hit.distance - skinWidth) * dirY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}
				collisions.below = dirY == -1;
				collisions.above = dirY == 1;
			}

			Debug.DrawRay(rayOrigin, Vector2.up * dirY * rayLength, Color.white);
		}

		if (collisions.climbingSlope) {
			float dirX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((dirX == -1) ? origins.bottomLeft : origins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (slopeAngle != collisions.slopeAngle) {
					velocity.x = (hit.distance - skinWidth) * dirX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelY) {
			velocity.y = climbVelY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}

	}

	void DescendSlope(ref Vector3 velocity) {
		float dirX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (dirX == -1) ? origins.bottomRight : origins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				if (Mathf.Sign (hit.normal.x) == dirX) {
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
						float moveDistance = Mathf.Abs(velocity.x);
						float descVelY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						velocity.y -= descVelY;

						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}

	void ResetFallingThroughPlatform() {
		collisions.fallingThroughPlatform = false;
	}

	// MARK: - Structs

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public bool descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public int faceDir;
		public bool fallingThroughPlatform;

		public Vector3 velocityOld;

		public void Reset() {
			above = below = false;
			left = right = false;
			climbingSlope = descendingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
