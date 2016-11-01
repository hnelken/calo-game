using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {

	private Player player;					// The player script to serve input to

	// Use this for initialization
	void Start () {
		player = GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
		// Get raw input vector and pass it to player script
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		player.SetDirectionalInput(input);

		// Respond to jump input events
		if (Input.GetKeyDown(KeyCode.Space)) {
			player.OnJumpInputDown();
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			player.OnJumpInputUp();
		}
		if (!player.PlayerIsAlive() && Input.GetKeyDown(KeyCode.K)) {
			player.TogglePlayerDeath();
		}
	}
}
