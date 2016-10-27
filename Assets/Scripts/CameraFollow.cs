using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	public Controller2D target;
	public float verticalOffset;
	public float lookAheadDistanceX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;
	public Vector2 focusAreaSize;

	FocusArea focusArea;

	float currentLookaheadX;
	float targetLookaheadX;
	float lookAheadDirX;

	float smoothLookVelocityX;
	float verticalSmoothVelocity;

	bool lookAheadStopped;

	void Start() {
		focusArea = new FocusArea(target.boxCollider.bounds, focusAreaSize);
	}

	void LateUpdate() {
		focusArea.Update(target.boxCollider.bounds);

		Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;

		if (focusArea.velocity.x != 0) {
			lookAheadDirX = Mathf.Sign (focusArea.velocity.x); 
			if (target.playerInput.x != 0 && Mathf.Sign (target.playerInput.x) == Mathf.Sign (focusArea.velocity.x)) {
				lookAheadStopped = false;
				targetLookaheadX = lookAheadDirX * lookAheadDistanceX;
			}
			else {
				if (!lookAheadStopped) {
					lookAheadStopped = true;
					targetLookaheadX = currentLookaheadX + (lookAheadDirX * lookAheadDistanceX - currentLookaheadX) / 4f;
				}
			}
		}

		currentLookaheadX = Mathf.SmoothDamp (currentLookaheadX, targetLookaheadX, ref smoothLookVelocityX, lookSmoothTimeX);
		focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref verticalSmoothVelocity, verticalSmoothTime);
		focusPosition += Vector2.right * currentLookaheadX;

		transform.position = (Vector3)focusPosition + Vector3.forward * -10;
	}

	void OnDrawGizmos() {
		Gizmos.color = new Color(1, 0, 0, .5f);
		Gizmos.DrawCube(focusArea.center, focusAreaSize);
	}

	struct FocusArea {
		public Vector2 center;
		public Vector2 velocity;
		public float left, right;
		public float top, bottom;

		public FocusArea(Bounds targetBounds, Vector2 size) {
			left = targetBounds.center.x - size.x / 2;
			right = targetBounds.center.x + size.x / 2;
			bottom = targetBounds.min.y;
			top = bottom + size.y;

			velocity = Vector2.zero;
			center = new Vector2((left + right) / 2, (top + bottom) / 2); 
		}

		public void Update(Bounds targetBounds) {
			float shiftX = 0;
			if (targetBounds.min.x < left) {
				shiftX = targetBounds.min.x - left;
			}
			else if (targetBounds.max.x > right) {
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom) {
				shiftY = targetBounds.min.y - bottom;
			}
			else if (targetBounds.max.y > top) {
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;
			center = new Vector2((left + right) / 2, (top + bottom) / 2); 
			velocity = new Vector2(shiftX, shiftY);
		}
	}
}
