#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI.Fixed{
    internal partial class FixedUI{
        public void OnFreeCameraModeButtonPress(){
         Log.DebugMessage("FixedUI:OnFreeCameraModeButtonPress");
         MainCamera.singleton.OnStopFollowing();
        }
    }
}