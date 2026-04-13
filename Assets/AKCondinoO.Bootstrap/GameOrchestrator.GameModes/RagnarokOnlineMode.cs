using AKCondinoO.SimActors;
using AKCondinoO.SimActors.SimInteractions;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Bootstrap.GameOrchestrator;
using static AKCondinoO.Bootstrap.InputInterpreter;
namespace AKCondinoO.Bootstrap.GameModes{
    internal class RagnarokOnlineMode:IGameModeLogic{
     private readonly GameOrchestrator orchestrator;
        internal RagnarokOnlineMode(GameOrchestrator orchestrator){
         this.orchestrator=orchestrator;
        }
        public void Run(SimActor activeSim,InputIntent intent){
         if(intent.cameraHitObject){
          pivot=intent.cameraHit.point;
         }else{
          pivot=intent.cameraForwardEndPoint;
         }
         switch(intent.action){
          case(InputAction.Forward):{
           cameraForward =intent.enabledState.curState[0];
           MoveCamera();
           break;
          }
          case(InputAction.Left):{
           cameraLeft    =intent.enabledState.curState[0];
           MoveCamera();
           break;
          }
          case(InputAction.Backward):{
           cameraBackward=intent.enabledState.curState[0];
           MoveCamera();
           break;
          }
          case(InputAction.Right):{
           cameraRight   =intent.enabledState.curState[0];
           MoveCamera();
           break;
          }
          case(InputAction.CameraOrbit):{
           Logs.Debug(()=>"pivot:"+pivot);
           lookX=intent.lookX;
           lookY=intent.lookY;
           MoveCamera();
           break;
          }
          case(InputAction.MouseX):{
           //Logs.Debug(()=>"InputAction.MouseX:"+intent.enabledState.curStateFloat[0]);
           break;
          }
          case(InputAction.MouseY):{
           //Logs.Debug(()=>"InputAction.MouseY:"+intent.enabledState.curStateFloat[0]);
           break;
          }
          case(InputAction.Action):{
           if(intent.enabledState.curState[0]){
            if((object)activeSim!=null){
             activeSim.OnReceiveInteractionIntent(intent);
            }
           }
           break;
          }
         }
        }
     Vector3 pivot;
     float lookX;
     float lookY;
     bool cameraForward;
     bool cameraBackward;
     bool cameraLeft;
     bool cameraRight;
        private void MoveCamera(){
         Vector3 movement=
          (cameraForward ?Vector3.forward:Vector3.zero)+
          (cameraBackward?Vector3.back   :Vector3.zero)+
          (cameraLeft    ?Vector3.left   :Vector3.zero)+
          (cameraRight   ?Vector3.right  :Vector3.zero);
         CameraIntent cameraIntent=new();
         cameraIntent.isFree=true;
         cameraIntent.followTarget=null;
         cameraIntent.pivot=pivot;
         cameraIntent.lookDelta=new(lookX,lookY);
         cameraIntent.move=movement;
         cameraIntent.zoom=0;
         cameraIntent.acceleration=40f;
         cameraIntent.damping=6f;
         cameraIntent.maxSpeed=12f;
         MainCamera.singleton.RunIntent(cameraIntent);
        }
    }
}