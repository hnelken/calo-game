using UnityEngine;
using System.Collections;

public class LevelGoal : MonoBehaviour {

	public int level;
	public int checkpoint;

	private Player player;

	private const int MAX_LEVEL = 1;
	private const int MAX_CHECKPOINT = 2;

	// Use this for initialization
	void Start () {
		this.player = GameObject.FindObjectOfType<Player>();
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.tag == "Player") {
			if (level == MAX_LEVEL && checkpoint == MAX_CHECKPOINT) {
				player.CompleteLevel("Level1-1");
			}
			else {
				player.CompleteLevel("Level" + level + "-" + (checkpoint + 1));
			}
		}
	}
}
