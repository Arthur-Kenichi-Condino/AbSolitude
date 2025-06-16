using UnityEngine;
namespace AKCondinoO{
    internal static class AnimatorUtil{
        internal static bool HasParameter(string paramName,Animator animator){
         foreach(AnimatorControllerParameter param in animator.parameters){
          if(param.name==paramName)
           return true;
         }
         return false;
        }
    }
}