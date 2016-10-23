using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	const float skinWidth = .015f;
	public int hRayCount = 4;
	public int vRayCount = 4;

	float hRaySpacing;
	float vRaySpacing;

	BoxCollider2D collider;
	RaycastOrigins origins;

	struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	// Use this for initialization
	void Start () {
		collider = GetComponent<BoxCollider2D>();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateRaycastOrigins ();
		CalculateRaySpacing ();

		for (int i = 0; i < vRayCount; i++) {
			Debug.DrawRay(origins.bottomLeft + Vector2.right * vRaySpacing * i, Vector2.up * -2, Color.white);
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
}
