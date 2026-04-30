using UnityEngine;
namespace AKCondinoO.Bootstrap.CameraModes{
    internal class OrbitalCameraMode:ICameraModeLogic{
     private float acceleration=80f;
     private float damping=10f;
     private float maxSpeed=20f;
     private Vector3 movement=Vector3.zero;
     private float zoomSpeed=5f;
     private float zoomInput=0f;
     private Vector2 sensitivity=new(10f,2f);
     private Vector2 orbitAngles=Vector2.zero;
     private Vector3 pivot;
     private float orbitDistance;
        public void Enter(MainCamera cam){
         pivot=cam.transform.position+cam.transform.forward;
         orbitDistance=1f;
         Quaternion lookRot=Quaternion.LookRotation(pivot-cam.transform.position);
         Vector3 euler=lookRot.eulerAngles;
         orbitAngles.x=euler.y;//  yaw
         orbitAngles.y=euler.x;//  pitch
        }
        public void Exit(MainCamera cam){
        }
        public void DoIntent(MainCamera cam,CameraIntent intent){
         movement=intent.move;
         acceleration=intent.acceleration;
         damping=intent.damping;
         maxSpeed=intent.maxSpeed;
         pivot=intent.pivot;
         orbitDistance=Vector3.Distance(cam.transform.position,pivot);
         zoomInput+=intent.zoom;
         rawLookDelta=intent.lookDelta;
         //Logs.Debug(()=>"rawLookDelta:"+rawLookDelta);
         Logs.Debug(()=>"intent.zoom:"+intent.zoom);
        }
     private Vector3 velocity;
     private float zoomCurrent;
     private float zoomVelocity;
     private Vector2 rawLookDelta;
     private Vector2 smoothLookDelta;
     private Vector2 lookVelocity;
        public void OnManualUpdate(MainCamera cam){
         float dt=Mathf.Min(Time.deltaTime,0.02f);
         zoomCurrent=Mathf.SmoothDamp(
          zoomCurrent,
          zoomInput,
          ref zoomVelocity,
          0.05f
         );
         zoomInput=0f;
         orbitDistance-=zoomCurrent*zoomSpeed;
         Vector3 forward=cam.transform.forward;
         Vector3 right  =cam.transform.right;
         forward.y=0f;
         right  .y=0f;
         forward.Normalize();
         right  .Normalize();
         Vector3 desiredDir=movement.sqrMagnitude>0f?
          movement.normalized:
          Vector3.zero;
         desiredDir=
          forward*movement.z+
          right  *movement.x;
         velocity+=desiredDir*acceleration*dt;
         if(velocity.magnitude>maxSpeed){
          velocity=velocity.normalized*maxSpeed;
         }
         velocity=Vector3.Lerp(velocity,Vector3.zero,damping*dt);
         pivot+=velocity*dt;
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
         cam.transform.position=pivot+offset;
         cam.transform.LookAt(pivot);
        }
    }
}