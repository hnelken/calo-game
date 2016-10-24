using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	// MARK: - Public References

	const float skinWidth = .015f;
	public int hRayCount = 4;
	public int vRayCount = 4;
	public LayerMask collisionMask;
	public CollisionInfo collisions;


	// MARK: - Private Variables

	float maxClimbAngle = 80;

	float hRaySpacing;
	float vRaySpacing;

	BoxCollider2D collider;
	RaycastOrigins origins;


	// MARK: - Public API

	// Move the player with a given velocity
	public void Move(Vector3 velocity) {
		// Get origins for collision detection rays
		UpdateRaycastOrigins ();
		collisions.Reset ();

		// Constrain horizontal velocity
		if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
		}
		
		// Constrain vertical velocity
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		// Perform the movements
		transform.Translate (velocity);
	}


	// MARK: - Private API

	// Use this for initialization
	void Start () {
		collider = GetComponent<BoxCollider2D>();

		// Get spacing for collision detection rays
		CalculateRaySpacing ();
	}

	// Constrain a velocity vector reference to vertical obstacles
	void HorizontalCollisions (ref Vector3 velocity) {
		float dirX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;
		
		for (int i = 0; i < hRayCount; i++) {
			Vector2 rayOrigin = (dirX == -1) ? origins.bottomLeft : origins.bottomRight;
			rayOrigin += Vector2.up * hRaySpacing * i;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, collisionMask);
			
			if (hit) {

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (i == 0 && slopeAngle < maxClimbAngle) {
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

	void UpdateRaycastOrigins() {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		origins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		origins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		origins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		origins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing() {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		hRayCount = Mathf.Clamp (hRayCount, 2, int.MaxValue);
		vRayCount = Mathf.Clamp (vRayCount, 2, int.MaxValue);

		hRaySpacing = bounds.size.y / (hRayCount - 1);
		vRaySpacing = bounds.size.x / (vRayCount - 1);
	}


	// MARK: - Structs

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public float slopeAngle, slopeAngleOld;

		public void Reset() {
			above = below = false;
			left = right = false;
			climbingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
	struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
