using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {

	public LayerMask passengerMask;
	public Vector3 move;
	 
	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller2D> passengerDict = new Dictionary<Transform, Controller2D>();

	// Use this for initialization
	public override void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateRaycastOrigins();
		Vector3 velocity = move * Time.deltaTime;
		CalculatePassengerMovement(velocity);

		MovePassengers(true);
		transform.Translate (velocity);
		MovePassengers(false);
	}

	void MovePassengers(bool beforePlatform) {
		foreach(PassengerMovement passenger in passengerMovement) {
			if (!passengerDict.ContainsKey(passenger.transform)) {
				passengerDict.Add (passenger.transform, passenger.transform.GetComponent<Controller2D>());
			}
			if (passenger.beforePlatform == beforePlatform) {
				passengerDict[passenger.transform].Move(passenger.velocity, passenger.onPlatform);
			}
		}
	}

	void CalculatePassengerMovement(Vector3 velocity) {
		HashSet<Transform> passengers = new HashSet<Transform>();
		passengerMovement = new List<PassengerMovement>();
		float dirX = Mathf.Sign (velocity.x);
		float dirY = Mathf.Sign (velocity.y);

		// Vertical Platform
		if (velocity.y != 0) {
			float rayLength = Mathf.Abs (velocity.y) + skinWidth;
			
			for (int i = 0; i < vRayCount; i++) {
				Vector2 rayOrigin = (dirY == -1) ? origins.bottomLeft : origins.topLeft;
				rayOrigin += Vector2.right * (vRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, passengerMask);

				if (hit) {
					if (!passengers.Contains(hit.transform)) {
						passengers.Add(hit.transform);
						float pushX = (dirY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinWidth) * dirY;

						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), dirY == 1, true));
					}
				}
			}
		}

		// Horizontal Platform
		if (velocity.x != 0) {
			float rayLength = Mathf.Abs (velocity.x) + skinWidth;
			
			for (int i = 0; i < hRayCount; i++) {
				Vector2 rayOrigin = (dirX == -1) ? origins.bottomLeft : origins.bottomRight;
				rayOrigin += Vector2.up * hRaySpacing * i;
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, passengerMask);

				if (hit) {
					if (!passengers.Contains(hit.transform)) {
						passengers.Add(hit.transform);
						float pushX = velocity.x - (hit.distance - skinWidth) * dirX;
						float pushY = -skinWidth;
						
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
					}
				}
			}
		}

		// Passenger on top of horizontal or downward moving platform
		if (dirY == -1 || velocity.y == 0 && velocity.x != 0) {
			float rayLength = 2 * skinWidth;
			
			for (int i = 0; i < vRayCount; i++) {
				Vector2 rayOrigin = origins.topLeft + Vector2.right * (vRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
				
				if (hit) {
					if (!passengers.Contains(hit.transform)) {
						passengers.Add(hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;
						
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
				}
			}
		}
	}

	struct PassengerMovement {
		public Transform transform;
		public Vector3 velocity;
		public bool onPlatform;
		public bool beforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _onPlatform, bool _beforePlatform) {
			transform = _transform;
			velocity = _velocity;
			onPlatform = _onPlatform;
			beforePlatform = _beforePlatform;
		}
	}
}
