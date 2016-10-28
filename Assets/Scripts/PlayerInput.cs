using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {

	private Player player;

	// Use this for initialization
	void Start () {
		player = GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		player.SetDirectionalInput(input);

		if (Input.GetKeyDown(KeyCode.Space)) {
			player.OnJumpInputDown();
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			player.OnJumpInputUp();
		}
	}
}
