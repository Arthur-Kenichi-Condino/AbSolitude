#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UI.Fixed{
    internal partial class FixedUI{
  [SerializeField]internal RectTransform theSims3CameraModeButtonRect;
     internal Button theSims3CameraModeButton;
        public void OnTheSims3CameraModeButtonPress(){
         Log.DebugMessage("FixedUI:OnTheSims3CameraModeButtonPress");
        }
    }
}