#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.UI.Fixed.BuildBuyEditMode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.GameMode;
namespace AKCondinoO.UI.Fixed{
    internal partial class FixedUI{
     [SerializeField]internal RectTransform              buildBuyEditModeUI;
      [SerializeField]internal BuildBuyEditModeUIContent buildBuyEditModeUIContent=new BuildBuyEditModeUIContent();
        public void OnBuildBuyEditModeButtonPress(){
         Log.DebugMessage("FixedUI:OnBuildBuyEditModeButtonPress");
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.BuildBuyEdit);
        }
    }
}