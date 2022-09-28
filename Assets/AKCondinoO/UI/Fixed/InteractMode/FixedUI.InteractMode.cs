#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.GameMode;
namespace AKCondinoO.UI.Fixed{
    internal partial class FixedUI{
     [SerializeField]internal RectTransform interactModeUI;
        public void OnInteractModeButtonPress(){
         Log.DebugMessage("FixedUI:OnInteractModeButtonPress");
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.Interact);
        }
    }
}