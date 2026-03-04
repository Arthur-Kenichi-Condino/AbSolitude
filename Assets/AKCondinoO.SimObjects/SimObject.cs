using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObject:MonoBehaviour{
     [SerializeField]internal bool useInstancedRendering=false;
     [SerializeField]internal GameObject meshObject;
     internal int instancedRenderingIndex=-1;
    }
}