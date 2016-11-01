using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Player))]
public class DeathManager : MonoBehaviour {

	public Transform spawnPoint;
	private bool living;

	// Use this for initialization
	void Awake () {
		living = true;
	}

	public void RespawnPlayer() {
		transform.position = spawnPoint.position;
		TogglePlayerDeath();
	}

	// Sets the player to be dead
	public void TogglePlayerDeath() {
		living = !living;
		Time.timeScale = (living) ? 1 : 0;
		transform.localScale = new Vector3(transform.localScale.x,
		                                   -transform.localScale.y,
		                                   transform.localScale.z);
	}

	public bool PlayerIsAlive() {
		return living;
	}
}
