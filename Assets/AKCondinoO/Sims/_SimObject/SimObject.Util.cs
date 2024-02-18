#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal virtual Quaternion GetRotation(){
         return transform.rotation;
        }
        internal virtual float GetHeight(){
         return localBounds.extents.y;
        }
        internal virtual float GetRadius(){
         return Mathf.Max(localBounds.extents.x,localBounds.extents.z);
        }
    }
}