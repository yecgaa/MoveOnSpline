#define DEBUG

using UnityEngine;

public class SplineMove : MonoBehaviour{
	public Spline spline=null;
	public Spline[] splineList=null;
	
	public WrapMode wrapMode = WrapMode.Loop;
	
	public float speed = 0.0001f;
	public float offSet = 0f;
	public float SPEED_MAX=0.001f;
	public float LIMITED_SPEED = 0.001f;

	public float acc = 0;

	public float DISTANCE_THRESHOLD=100f;
	private float param=0f;
	private float clampedParam=0f;

	public Vector3 nextPosition;
	public Quaternion nextRotation;
	
	private GameObject sphere;
	
	void Start(){
		#if DEBUG
		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.localScale=new Vector3(2*DISTANCE_THRESHOLD,2*DISTANCE_THRESHOLD,2*DISTANCE_THRESHOLD);
		sphere.renderer.material.color = new Color(0.5f,0.5f,0.5f,0.3f);
		sphere.renderer.material.shader = Shader.Find("Transparent/Diffuse");
		#endif
	}
	
	
	//numbering left to right 0-MAX
	void Update ()
	{
		float horAxis = 0f;
		if ((horAxis = Input.GetAxis ("Horizontal")) != 0f) {
			foreach (Spline s in splineList) {
				float closeParam = s.GetClosestPointParam (this.transform.position, 5);
				float closeClampedParam = WrapValue (closeParam + offSet, 0f, 1f, wrapMode);
				Vector3 closestPoint = s.GetPositionOnSpline (closeClampedParam);
				float distance = (this.transform.position - closestPoint).magnitude;
				//Debug.Log (distance);
				if (distance < DISTANCE_THRESHOLD) {
					int sNum = int.Parse (s.tag);
					int splineNum = int.Parse (this.spline.tag);
					if (horAxis < 0 && sNum < splineNum
						|| horAxis > 0 && sNum > splineNum) {
						//splineの乗り換え
						Debug.Log ("spline change " + this.spline.tag + " to " + s.tag);
						this.spline=s;
						this.transform.position = closestPoint;
						param = closeParam;
						
						break;
					}
				}
			}
		}

		//  加速処理
		if (Input.GetAxis ("Vertical") > 0) {
			acc += Time.deltaTime * speed;
			if (acc > SPEED_MAX) {
				acc = SPEED_MAX;
			}
		} else if (Input.GetAxis ("Vertical") < 0) {
			//pastForce=-Time.deltaTime * speed;
		} else if (acc > 0.00005f) {
			acc *= 0.5f;
		} else {
			acc = 0;
		}


		//カーブ判定
		SplineSegment ssegment = this.spline.GetSplineSegment (clampedParam);
		if (ssegment.StartNode.tag == "sharp curve") {
			Debug.Log ("through :sharp curve");
			if (acc > LIMITED_SPEED) {
				acc = LIMITED_SPEED;
			}
		}

		param += acc;

		clampedParam = WrapValue (param + offSet, 0f, 1f, wrapMode);

		transform.position = spline.GetPositionOnSpline (clampedParam);
		transform.rotation = spline.GetOrientationOnSpline (clampedParam);
		
		#if DEBUG
		sphere.transform.position = transform.position;
		#endif

		float predictParam = WrapValue (param + 0.05f + offSet, 0f, 1f, wrapMode);

		nextPosition = spline.GetPositionOnSpline (predictParam);
		nextRotation = spline.GetOrientationOnSpline (predictParam);
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
