using AKCondinoO.Bootstrap.CameraModes;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class MainCamera:MonoSingleton<MainCamera>{
     internal Camera mainCamera;
     private readonly Dictionary<CameraMode,ICameraModeLogic>cameraModes=new();
     private CameraMode currentCameraMode=CameraMode.FreeCameraMode;
     private ICameraModeLogic currentCameraModeLogic;
        public override void Initialize(){
         base.Initialize();
         mainCamera=GetComponent<Camera>();
         cameraModes[CameraMode.OrbitalCameraMode]=new OrbitalCameraMode();
        }
        internal void SetCameraMode(CameraMode mode){
         if(currentCameraMode!=mode){
          currentCameraModeLogic?.Exit(this);
          cameraModes.TryGetValue(mode,out currentCameraModeLogic);
          currentCameraModeLogic?.Enter(this);
         }
         currentCameraMode=mode;
        }
        internal void RunIntent(CameraIntent intent){
         currentCameraModeLogic?.DoIntent(this,intent);
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         currentCameraModeLogic?.OnManualUpdate(this);
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