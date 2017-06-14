using UnityEngine;
using System.Collections;

public class DeathZone : MonoBehaviour {

	private Player player;

	void Start() {
		player = GameObject.FindObjectOfType<Player>();
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.tag == "Player") {
			player.TogglePlayerDeath();
		}
	}
}
