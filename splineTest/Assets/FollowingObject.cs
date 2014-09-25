using UnityEngine;
using System.Collections;

public class FollowingObject : MonoBehaviour {
	
	public GameObject player = null;
	private Vector3 nextPosition;
	
	public float distance = 8f;
	public float offset =5f; 
	private Vector3 tmp;
	public float pastEffect = 0f;
	public float THRESHOLD=0f;
	
	// Use this for initialization
	void Start () {
		if (this.player == null) {
			this.player = GameObject.FindGameObjectWithTag ("Player");
		}
		nextPosition = this.player.GetComponent<SplineMove>().nextPosition;
		tmp = new Vector3 (0,0,0);
	}
	
	// Update is called once per frame
	void Update () {
		nextPosition = this.player.GetComponent<SplineMove>().nextPosition;
		if (this.player.transform.position == nextPosition) {
			return;
		}
		Vector3 delta = (this.player.transform.position-nextPosition);
	
		if (delta.magnitude < THRESHOLD) {
			nextPosition = this.player.transform.position;
			return;
		}

		delta.Normalize ();
		tmp= new Vector3 (
			delta.x*distance+tmp.x*pastEffect,
			delta.y*distance+tmp.y*pastEffect+offset,
			delta.z*distance+tmp.z*pastEffect
		);
		this.transform.position = nextPosition + tmp;

		this.transform.rotation = Quaternion.Slerp(
			this.transform.rotation, 
			Quaternion.LookRotation(nextPosition - this.transform.position),
			0.07f
		);
	}
}