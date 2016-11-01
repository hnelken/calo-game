using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {

	// MARK: - Public References

	public CollisionInfo collisions;
	[HideInInspector]
	public Vector2 playerInput;
	public float maxSlopeAngle = 80;
	

	// MARK: - Public API

	public override void Start() {
		base.Start ();
		collisions.faceDir = 1;
	}

	public void Move(Vector2 moveAmount, bool onPlatform) {
		Move (moveAmount, Vector2.zero, onPlatform);
	}

	// Move the player with a given moveAmount
	public void Move(Vector2 moveAmount, Vector2 input, bool onPlatform = false) {

		// Get origins for collision detection rays
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.moveAmountOld = moveAmount;
		playerInput = input;

		// Constrain horizontal moveAmount
		if (moveAmount.y < 0) {
			DescendSlope(ref moveAmount);
		}

		if (moveAmount.x != 0) {
			collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
		}

		HorizontalCollisions (ref moveAmount);
		
		// Constrain vertical moveAmount
		if (moveAmount.y != 0) {
			VerticalCollisions (ref moveAmount);
		}

		// Perform the movements
		transform.Translate (moveAmount);

		if (onPlatform) {
			collisions.below = true;
		}
	}


	// MARK: - Private API

	// Constrain a moveAmount vector reference to horizontal obstacles
	void HorizontalCollisions (ref Vector2 moveAmount) {
		float dirX = collisions.faceDir;
		float rayLength = Mathf.Abs (moveAmount.x) + skinWidth;

		if (Mathf.Abs(moveAmount.x) < skinWidth) {
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
				if (i == 0 && slopeAngle < maxSlopeAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						moveAmount = collisions.moveAmountOld;
					}
					float distToSlope = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distToSlope = hit.distance - skinWidth;
						moveAmount.x -= distToSlope * dirX;
					}
					ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
					moveAmount.x += distToSlope * dirX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle) {
					moveAmount.x = (hit.distance - skinWidth) * dirX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
					}

					collisions.left = dirX == -1;
					collisions.right = dirX == 1;
				}
			}

			Debug.DrawRay(rayOrigin, Vector2.right * dirX * rayLength, Color.white);
		}
	}

	// Constrain a moveAmount vector reference to vertical obstacles
	void VerticalCollisions (ref Vector2 moveAmount) {
		float dirY = Mathf.Sign (moveAmount.y);
		float rayLength = Mathf.Abs (moveAmount.y) + skinWidth;

		for (int i = 0; i < vRayCount; i++) {
			Vector2 rayOrigin = (dirY == -1) ? origins.bottomLeft : origins.topLeft;
			rayOrigin += Vector2.right * (vRaySpacing * i + moveAmount.x);
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

				moveAmount.y = (hit.distance - skinWidth) * dirY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (moveAmount.x);
				}
				collisions.below = dirY == -1;
				collisions.above = dirY == 1;
			}

			Debug.DrawRay(rayOrigin, Vector2.up * dirY * rayLength, Color.white);
		}

		if (collisions.climbingSlope) {
			float dirX = Mathf.Sign(moveAmount.x);
			rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
			Vector2 rayOrigin = ((dirX == -1) ? origins.bottomLeft : origins.bottomRight) + Vector2.up * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (slopeAngle != collisions.slopeAngle) {
					moveAmount.x = (hit.distance - skinWidth) * dirX;
					collisions.slopeAngle = slopeAngle;
					collisions.slopeNormal = hit.normal;
				}
			}
		}
	}

	void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal) {
		float moveDistance = Mathf.Abs (moveAmount.x);
		float climbVelY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (moveAmount.y <= climbVelY) {
			moveAmount.y = climbVelY;
			moveAmount.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (moveAmount.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
			collisions.slopeNormal = slopeNormal;
		}

	}

	void DescendSlope(ref Vector2 moveAmount) {

		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(origins.bottomLeft, Vector2.down, Mathf.Abs (moveAmount.y) + skinWidth, collisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(origins.bottomRight, Vector2.down, Mathf.Abs (moveAmount.y) + skinWidth, collisionMask);

		if (maxSlopeHitLeft ^ maxSlopeHitRight) {
			SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
			SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
		}

		if (!collisions.slidingDownSlope) {
			float dirX = Mathf.Sign (moveAmount.x);
			Vector2 rayOrigin = (dirX == -1) ? origins.bottomRight : origins.bottomLeft;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
					if (Mathf.Sign (hit.normal.x) == dirX) {
						if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x)) {
							float moveDistance = Mathf.Abs(moveAmount.x);
							float descVelY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
							moveAmount.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (moveAmount.x);
							moveAmount.y -= descVelY;

							collisions.slopeAngle = slopeAngle;
							collisions.descendingSlope = true;
							collisions.below = true;
							collisions.slopeNormal = hit.normal;
						}
					}
				}
			}
		}
	}

	void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount) {
		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle > maxSlopeAngle) {
				moveAmount.x = hit.normal.x * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan (slopeAngle * Mathf.Deg2Rad);
				collisions.slopeAngle = slopeAngle;
				collisions.slidingDownSlope = true;
				collisions.slopeNormal = hit.normal;
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
		public bool slidingDownSlope;

		public float slopeAngle, slopeAngleOld;
		public int faceDir;
		public bool fallingThroughPlatform;

		public Vector2 slopeNormal;
		public Vector2 moveAmountOld;

		public void Reset() {
			above = below = false;
			left = right = false;
			climbingSlope = descendingSlope = false;
			slidingDownSlope = false;

			slopeNormal = Vector2.zero;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
