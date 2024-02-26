#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial interface Interactable{
        void SetInteractionsList();
        void GetInteractions(out List<Interaction>interactions);
        string ContextName();
    }
}