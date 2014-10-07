#define DEBUG

using UnityEngine;

public class SplineMove : MonoBehaviour{
	public Spline spline=null;
	public Spline[] splineList=null;
	
	public WrapMode wrapMode = WrapMode.Loop;
	
	public float speed = 0.0001f;
	public float offSet = 0f;
	
	//constant
	public float SPEED_MAX=0.001f;
	public float LIMITED_SPEED = 0.001f;
	public float DISTANCE_THRESHOLD=100f;
	private float CHANGE_REJECT_TIME=1f;
	private float SHARP_CURVE=100f;

	public float acc = 0;

	private float param=0f;
	private float clampedParam=0f;

	public Vector3 nextPosition;
	public Quaternion nextRotation;
	
	private float changedTime;
	
	private SplineSegment pastSegment=null;
	private float theta=0f;
	
	private GameObject right;
	private GameObject left;
	
	
	//for debug
	private GameObject sphere;
	private GameObject rightCube;
	private GameObject leftCube;
	
	void Start(){
		#if DEBUG
		//changeSpline 判定
		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.localScale=new Vector3(2*DISTANCE_THRESHOLD,2*DISTANCE_THRESHOLD,2*DISTANCE_THRESHOLD);
		sphere.renderer.material.color = new Color(0.5f,0.5f,0.5f,0.3f);
		sphere.renderer.material.shader = Shader.Find("Transparent/Diffuse");
		
		//左右ベクトル判定
		right = transform.Find("right").gameObject;
		left = transform.Find("left").gameObject;
		
		rightCube=GameObject.CreatePrimitive(PrimitiveType.Cube);
		rightCube.renderer.material.color = Color.red;
		
		leftCube=GameObject.CreatePrimitive(PrimitiveType.Cube);
		leftCube.renderer.material.color = Color.blue;
		
		#endif
		
		changedTime=Time.time;
		
	}
	
	//左から昇順に並べる
	void Update ()
	{
		float horAxis = Input.GetAxis ("Horizontal");
		if (horAxis != 0f) {
			if ((Time.time - changedTime) < CHANGE_REJECT_TIME) {
				#if DEBUG
				//Debug.Log ("Change of the spline was rejected. Time has not passed since the last change.");
				#endif
			} else {
				try {
					foreach (Spline s in splineList) {
						float closeParam = s.GetClosestPointParam (this.transform.position, 5);
						float closeClampedParam = WrapValue (closeParam + offSet, 0f, 1f, wrapMode);
						Vector3 closestPoint = s.GetPositionOnSpline (closeClampedParam);
						Vector3 targetVector = (closestPoint - this.transform.position);
						float distance = targetVector.magnitude;
						
						if (distance < DISTANCE_THRESHOLD) {
							Vector3 leftVec=(left.transform.position - transform.position);
							Vector3 rightVec=(right.transform.position - transform.position);
							
							if((horAxis > 0 && Vector3.Angle(targetVector,rightVec)<=90
							|| horAxis < 0 && Vector3.Angle(targetVector,leftVec)<=90)==false){
								continue;
							}
							
							//splineの乗り換え
							#if DEBUG
							Debug.Log ("spline change " + this.spline.tag + " to " + s.tag);
							#endif
							this.spline = s;
							this.transform.position = closestPoint;
							param = closeParam;
							changedTime = Time.time;
							break;
						}
					}
				} catch (UnityException e) {
					#if DEBUG
					Debug.Log (e);
					#endif
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
		
		if (pastSegment != null) {
			
			if (pastSegment.EndNode.Position != ssegment.EndNode.Position) {
				Vector3 pastVec = (-pastSegment.EndNode.Position + pastSegment.StartNode.Position);
				Vector3 nextVec = ssegment.EndNode.Position - ssegment.StartNode.Position;

				theta = Vector3.Angle (pastVec, nextVec);
				Debug.Log (theta);
				#if DEBUG
				if(theta<=SHARP_CURVE){
					Debug.Log ("through :sharp curve");
				}
				#endif

				pastSegment = ssegment;
			}
		} else {
			pastSegment = ssegment;
		}
		
				
		if (theta <= SHARP_CURVE) {
			if (acc > LIMITED_SPEED) {
				acc = LIMITED_SPEED;
			}
		}
		
		param += acc;

		clampedParam = WrapValue (param + offSet, 0f, 1f, wrapMode);
		Vector3 target;
		target = spline.GetPositionOnSpline (clampedParam);
		//transform.position = Vector3.Lerp(transform.position,target,0.1f);
		transform.position = target;
		transform.rotation = spline.GetOrientationOnSpline (clampedParam);
		
		#if DEBUG
		sphere.transform.position = transform.position;
		#endif

		//少し先の予測
		float predictParam = WrapValue (param + 0.05f + offSet, 0f, 1f, wrapMode);
		nextPosition = spline.GetPositionOnSpline (predictParam);
		nextRotation = spline.GetOrientationOnSpline (predictParam);
		
		#if DEBUG
		rightCube.transform.position=right.transform.position;
		rightCube.transform.rotation = transform.rotation;
		
		leftCube.transform.position=left.transform.position;
		leftCube.transform.rotation = transform.rotation;
		#endif
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
