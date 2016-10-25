using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {
	
	public LayerMask collisionMask;

	public const float skinWidth = .015f;
	public int hRayCount = 4;
	public int vRayCount = 4;

	[HideInInspector]
	public float hRaySpacing;
	[HideInInspector]
	public float vRaySpacing;

	[HideInInspector]
	public BoxCollider2D boxCollider;
	public RaycastOrigins origins;

	// Use this for initialization
	public virtual void Start () {
		boxCollider = GetComponent<BoxCollider2D>();
		
		// Get spacing for collision detection rays
		CalculateRaySpacing ();
	}

	public void UpdateRaycastOrigins() {
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (skinWidth * -2);
		
		origins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		origins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		origins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		origins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}
	
	public void CalculateRaySpacing() {
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (skinWidth * -2);
		
		hRayCount = Mathf.Clamp (hRayCount, 2, int.MaxValue);
		vRayCount = Mathf.Clamp (vRayCount, 2, int.MaxValue);
		
		hRaySpacing = bounds.size.y / (hRayCount - 1);
		vRaySpacing = bounds.size.x / (vRayCount - 1);
	}

	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
