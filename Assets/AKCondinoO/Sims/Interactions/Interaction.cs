#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class Interaction{
        internal virtual void Do(BaseAI sim){
        }
        public override string ToString(){
         return string.Intern(this.GetType()+":string vazio");
        }
 }
}