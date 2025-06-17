#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UI.Fixed{
    internal partial class FixedUI{
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