using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid{
    internal partial class HumanoidAI:BaseAI{
        protected override void OnUMACharacterUpdated(UMAData simUMAData){
         Log.DebugMessage("OnUMACharacterUpdated");
         SetBodyPart("rightHand","hand.R",out _);
         base.OnUMACharacterUpdated(simUMAData);
        }
    }
}