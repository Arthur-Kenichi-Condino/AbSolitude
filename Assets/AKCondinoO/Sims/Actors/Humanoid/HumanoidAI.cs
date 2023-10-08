using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid{
    internal partial class HumanoidAI:BaseAI{
        protected override void OnUMACharacterUpdated(UMAData simUMAData){
         Log.DebugMessage("OnUMACharacterUpdated");
         SetBodyPart("rightHand","hand.R",out _);
         SetBodyPart("abdomen","spine.001",out _);
         SetBodyPart("pelvis","spine",out _);
         SetBodyPart("chest","spine.003",out _);
         SetBodyPart("neck","spine.004",out _);
         SetBodyPart( "leftThigh","thigh.L",out _);
         SetBodyPart("rightThigh","thigh.R",out _);
         SetBodyPart( "leftKnee","shin.L",out _);
         SetBodyPart("rightKnee","shin.R",out _);
         SetBodyPart( "leftShin","shin.L",out _);
         SetBodyPart("rightShin","shin.R",out _);
         base.OnUMACharacterUpdated(simUMAData);
        }
    }
}