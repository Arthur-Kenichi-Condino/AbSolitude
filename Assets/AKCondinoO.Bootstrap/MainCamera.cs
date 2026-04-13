using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class MainCamera:MonoSingleton<MainCamera>{
     internal Camera mainCamera;
        public override void Initialize(){
         base.Initialize();
         mainCamera=GetComponent<Camera>();
         pivot=transform.position+transform.forward;
         orbitDistance=1f;
         Quaternion lookRot=Quaternion.LookRotation(pivot-transform.position);
         Vector3 euler=lookRot.eulerAngles;
         orbitAngles.x=euler.y;//  yaw
         orbitAngles.y=euler.x;//  pitch
        }
     private float acceleration=80f;
     private float damping=10f;
     private float maxSpeed=20f;
     private Vector3 movement=Vector3.zero;
     private float orbitDistance=1f;
     private Vector2 orbitAngles=Vector2.zero;
     private Vector2 sensitivity=new(10f,2f);
     private Vector3 pivot;
        internal void RunIntent(CameraIntent intent){
         movement=intent.move;
         acceleration=intent.acceleration;
         damping=intent.damping;
         maxSpeed=intent.maxSpeed;
         pivot=intent.pivot;
         orbitDistance=Vector3.Distance(transform.position,pivot);
         rawLookDelta=intent.lookDelta;
         //Logs.Debug(()=>"rawLookDelta:"+rawLookDelta);
        }
     private Vector3 velocity;
     private Vector2 rawLookDelta;
     private Vector2 smoothLookDelta;
     private Vector2 lookVelocity;
        public override void ManualUpdate(){
         base.ManualUpdate();
         Vector3 desiredDir=movement.sqrMagnitude>0f?
          movement.normalized:
          Vector3.zero;
         velocity+=desiredDir*acceleration*Time.deltaTime;
         if(velocity.magnitude>maxSpeed){
          velocity=velocity.normalized*maxSpeed;
         }
         velocity=Vector3.Lerp(velocity,Vector3.zero,damping*Time.deltaTime);
         pivot+=velocity*Time.deltaTime;
         smoothLookDelta=Vector2.SmoothDamp(
          smoothLookDelta,
          rawLookDelta,
          ref lookVelocity,
          0.05f
         );
         rawLookDelta=Vector2.zero;
         orbitAngles.x+=smoothLookDelta.x*sensitivity.x;
         orbitAngles.y-=smoothLookDelta.y*sensitivity.y;
         orbitAngles.y=Mathf.Clamp(orbitAngles.y,-60f,60f);
         Quaternion orbitRotation=Quaternion.Euler(
          orbitAngles.y,//  pitch
          orbitAngles.x,//  yaw
          0f
         );
         Vector3 offset=orbitRotation*new Vector3(0f,0f,-orbitDistance);
         transform.position=pivot+offset;
         transform.LookAt(pivot);
        }
    }
    internal struct CameraIntent{
     internal bool isFree;
     internal Transform followTarget;
     internal Vector3 pivot;
     internal Vector2 lookDelta;
     internal Vector3 move;
     internal float zoom;
     internal float acceleration;
     internal float damping;
     internal float maxSpeed;
    }
}