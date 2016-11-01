using UnityEngine;
using System.Collections;

public class DeathZone : MonoBehaviour {

	public Player player;

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.tag == "Player") {
			player.TogglePlayerDeath();
		}
	}
}
