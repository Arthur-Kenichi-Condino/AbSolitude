#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Sims.Actors;
using AKCondinoO.UI.Context;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UI.Fixed{
    internal partial class FixedUI{
        public void OnFreeCameraModeButtonPress(){
         Log.DebugMessage("FixedUI:OnFreeCameraModeButtonPress");
         MainCamera.singleton.OnStopFollowing();
        }
    }
}