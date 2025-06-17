#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UI.Fixed{
    internal partial class FixedUI{
        void AwakeUIForCameraMode(){
         followerCameraModeButton=followerCameraModeButtonRect.GetComponent<Button>();
        }
        void UpdateUIForCameraMode(){
        }
        void OnGUIForCameraMode(){
         if(ScreenInput.singleton.currentActiveSim!=null&&ScreenInput.singleton.currentActiveSim.canBeThirdPersonCamFollowed){
          followerCameraModeButton.interactable=true;
         }else{
          followerCameraModeButton.interactable=false;
         }
        }
    }
}