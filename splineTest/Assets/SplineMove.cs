using UnityEngine;

public class SplineMove : MonoBehaviour{
	public Spline spline=null;
	public Spline[] splineList=null;
	private int splineIndex=0;
	
	public WrapMode wrapMode = WrapMode.Loop;
	
	public float speed = 0.1f;
	public float offSet = 0f;
	
	public float passedTime = 0f;
	public float pastForce = 0;

	public float DISTANCE_THRESHOLD=100f;
	private float param=0f;

	//numbering left to right 0-MAX
	void Update(){
		int tmpIndex = this.splineIndex;
		if (Input.GetAxis ("Horizontal") > 0) {
			Debug.Log ("Horizontal");
			if (this.splineIndex < this.splineList.Length - 1) {
					tmpIndex = this.splineIndex + 1;
			}
		} else if (Input.GetAxis ("Horizontal") < 0) {
			Debug.Log ("Horizontal");
			if (this.splineIndex > 0) {
				tmpIndex = this.splineIndex - 1;
			}
		}
		if (tmpIndex != this.splineIndex) {
			Spline s = this.splineList [tmpIndex];
			float closeParam = s.GetClosestPointParam (this.transform.position, 5);
			float closeClampedParam = WrapValue (closeParam + offSet, 0f, 1f, wrapMode);
			Vector3 closestPoint = s.GetPositionOnSpline (closeClampedParam);
			float distance = (this.transform.position - closestPoint).magnitude;
			//Debug.Log (distance);
			if (distance < DISTANCE_THRESHOLD) {
				this.spline = s;
				Debug.Log ("spline change " + this.splineIndex + " to " + tmpIndex);
				this.splineIndex=tmpIndex;
				this.transform.position = closestPoint;
				param = closeParam;
			}
		}
		if (Input.GetAxis("Vertical")>0) {
			pastForce=Time.deltaTime * speed;
		} else if (Input.GetAxis("Vertical")<0) {
			pastForce=-Time.deltaTime * speed;
		}else if(pastForce>0.00005f){
			pastForce*=0.95f;
		}else{
			pastForce=0;
		}
		param += pastForce;

		float clampedParam = WrapValue (param + offSet, 0f, 1f, wrapMode);

		transform.position = spline.GetPositionOnSpline (clampedParam);
		transform.rotation = spline.GetOrientationOnSpline (clampedParam);
	}
	
	private float WrapValue( float v, float start, float end, WrapMode wMode ){
		switch( wMode ){
		case WrapMode.Clamp:
		case WrapMode.ClampForever:
			return Mathf.Clamp( v, start, end );
		case WrapMode.Default:
		case WrapMode.Loop:
			return Mathf.Repeat( v, end - start ) + start;
		case WrapMode.PingPong:
			return Mathf.PingPong( v, end - start ) + start;
		default:
			return v;
		}
	}
}
