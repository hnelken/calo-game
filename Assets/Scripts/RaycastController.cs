using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	[HideInInspector]
	public BoxCollider2D boxCollider;
	public LayerMask collisionMask;
	
	protected const float distBetweenRays = .25f;
	protected const float skinWidth = .015f;
	protected float hRaySpacing;
	protected float vRaySpacing;
	protected int hRayCount = 4;
	protected int vRayCount = 4;
	protected RaycastOrigins origins;

	public virtual void Awake() {
		boxCollider = GetComponent<BoxCollider2D>();
	}

	public virtual void Start() {
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

		hRayCount = Mathf.RoundToInt(bounds.size.y / distBetweenRays);
		vRayCount = Mathf.RoundToInt(bounds.size.x / distBetweenRays);
		
		hRaySpacing = bounds.size.y / (hRayCount - 1);
		vRaySpacing = bounds.size.x / (vRayCount - 1);
	}

	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
