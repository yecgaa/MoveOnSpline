using UnityEngine;
using System.Collections;

public class FollowingObject : MonoBehaviour {
	
	public GameObject player = null;
	private Vector3 pastPosition;
	
	public float distance = 8f;
	public float offset =5f; 
	private Vector3 tmp;
	public float pastEffect = 0f;
	public float THRESHOLD=0.1f;
	
	// Use this for initialization
	void Start () {
		if (this.player == null) {
			this.player = GameObject.FindGameObjectWithTag ("Player");
		}
		pastPosition = this.player.transform.position;
		tmp = new Vector3 (0,0,0);
	}
	
	// Update is called once per frame
	void Update () {
		if (this.player.transform.position == pastPosition) {
			return;
		}
		Vector3 delta = (pastPosition - this.player.transform.position);
		if (delta.magnitude < THRESHOLD) {
			pastPosition = this.player.transform.position;
			return;
		}
		delta.Normalize ();
		tmp= new Vector3 (
			delta.x*distance+tmp.x*pastEffect,
			delta.y*distance+tmp.y*pastEffect+offset,
			delta.z*distance+tmp.z*pastEffect
		);
		this.transform.position = this.player.transform.position + tmp;

		this.transform.rotation = Quaternion.Slerp(
			this.transform.rotation, 
			Quaternion.LookRotation(this.player.transform.position - this.transform.position),
			0.07f
		);
		pastPosition = this.player.transform.position;
	}
}