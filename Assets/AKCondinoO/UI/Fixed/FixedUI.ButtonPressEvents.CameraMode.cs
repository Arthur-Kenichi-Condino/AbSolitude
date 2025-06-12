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
  [SerializeField]internal RectTransform theSims3CameraModeButtonRect;
     internal Button theSims3CameraModeButton;
        public void OnTheSims3CameraModeButtonPress(){
         Log.DebugMessage("FixedUI:OnTheSims3CameraModeButtonPress");
        }
        public void OnFreeCameraModeButtonPress(){
         Log.DebugMessage("FixedUI:OnFreeCameraModeButtonPress");
         MainCamera.singleton.OnStopFollowing();
        }
     [SerializeField]internal RectTransform followerCameraModeButtonRect;
     internal Button followerCameraModeButton;
        public void OnFollowerCameraModeButtonPress(){
         Log.DebugMessage("FixedUI:OnFollowerCameraModeButtonPress");
         if(ScreenInput.singleton.currentActiveSim!=null){
          ScreenInput.singleton.currentActiveSim.OnThirdPersonCamFollow();
         }
        }
    }
}