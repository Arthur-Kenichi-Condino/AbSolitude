#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     protected readonly List<Interaction>interactions=new List<Interaction>();
        public virtual void SetInteractionsList(){
        }
        public virtual void GetInteractions(out List<Interaction>interactions){
         interactions=this.interactions;
        }
        public virtual string ContextName(){
         return"Sim Object";
        }
    }
}