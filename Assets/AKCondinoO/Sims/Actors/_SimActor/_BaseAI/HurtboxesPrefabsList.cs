#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Combat{
    internal class HurtboxesPrefabsList:MonoBehaviour{
     [SerializeField]internal Hurtboxes[]prefabs;
    }
}