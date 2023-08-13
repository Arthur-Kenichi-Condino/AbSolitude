#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Combat{
    internal class Hitboxes:MonoBehaviour{
     internal Rigidbody kinematicRigidbody;
     internal SimActor actor;
        internal void OnTriggerEnter(Collider other){
        }
    }
}