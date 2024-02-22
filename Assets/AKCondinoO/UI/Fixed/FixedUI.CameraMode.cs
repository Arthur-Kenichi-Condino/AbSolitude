#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UI.Fixed{
    internal partial class FixedUI{
        public void UpdateUIForCameraMode(){
         if(followerCameraModeButton==null){
          followerCameraModeButton=followerCameraModeButtonRect.GetComponent<Button>();
         }
         if(followerCameraModeButton!=null){
          if(ScreenInput.singleton.currentActiveSim!=null&&ScreenInput.singleton.currentActiveSim.canBeThirdPersonCamFollowed){
           followerCameraModeButton.interactable=true;
          }else{
           followerCameraModeButton.interactable=false;
          }
         }
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