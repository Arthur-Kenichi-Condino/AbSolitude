#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.BaseAI;
namespace AKCondinoO.Sims.Inventory{
    internal class InContainerTransformData:MonoBehaviour{
     [SerializeField]internal string simTypeName;
     [SerializeField]internal ActorMotion motion;
     [SerializeField]internal ActorWeaponLayerMotion weaponMotion;
     [SerializeField]internal ActorToolLayerMotion toolMotion;
     [SerializeField]internal string layer;
     [SerializeField]internal string parentBodyPartName;
     [SerializeField]internal int layerPriority;
     [SerializeField]internal string otherParameters;
    }
}