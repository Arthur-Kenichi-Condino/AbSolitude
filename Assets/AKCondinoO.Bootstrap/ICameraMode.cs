using UnityEngine;
namespace AKCondinoO.Bootstrap.CameraModes{
    internal enum CameraMode{
     FreeCameraMode=0,
     OrbitalCameraMode=1,
    }
    internal interface ICameraModeLogic{
        void Enter(MainCamera cam);
        void Exit(MainCamera cam);
        void DoIntent(MainCamera cam,CameraIntent intent);
        void OnManualUpdate(MainCamera cam);
    }
}