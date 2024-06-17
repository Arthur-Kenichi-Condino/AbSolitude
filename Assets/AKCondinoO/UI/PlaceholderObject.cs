#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI{
    internal class PlaceholderObject:MonoBehaviour{
     internal readonly List<Collider>collidersForTesting=new List<Collider>();
      readonly HashSet<GameObject>gameObjectsCloned=new HashSet<GameObject>();
     internal readonly List<Renderer>renderersForPreview=new List<Renderer>();
     internal readonly Dictionary<Collider,Ray[]>snappingRays=new();
        internal void BuildFrom(SimObject simObjectPrefab){
         SimObject simObjectPrefabComponent=simObjectPrefab.GetComponent<SimObject>();
         SimConstruction simConstruction=simObjectPrefabComponent as SimConstruction;
         gameObjectsCloned.Clear();
         foreach(Collider collider in simObjectPrefab.GetComponentsInChildren<Collider>()){
          if(collider.CompareTag("SimObjectVolume")&&!gameObjectsCloned.Contains(collider.gameObject)){
           Log.DebugMessage("adding collider for testing positioning...");
           Collider instantiatedCollider;
           collidersForTesting.Add(instantiatedCollider=Instantiate(collider,this.transform,false));
           foreach(Component component in instantiatedCollider.GetComponentsInChildren(typeof(Component))){
            if(!(component is Transform)&&!(component is Collider)){
             DestroyImmediate(component);
            }
           }
           instantiatedCollider.tag="Placeholder";
           instantiatedCollider.isTrigger=true;
           foreach(Transform transform in collider.gameObject.GetComponentsInChildren<Transform>()){
            gameObjectsCloned.Add(transform.gameObject);
           }
           if(simConstruction!=null){
            simConstruction.GetSnappingRays(instantiatedCollider,snappingRays);
           }
          }
         }
         gameObjectsCloned.Clear();
         foreach(Renderer renderer in simObjectPrefab.GetComponentsInChildren<Renderer>()){
          if(!gameObjectsCloned.Contains(renderer.gameObject)){
           Log.DebugMessage("adding renderer for previewing positioning...");
           Renderer instantiatedRenderer;
           renderersForPreview.Add(instantiatedRenderer=Instantiate(renderer,this.transform,false));
           foreach(Component component in instantiatedRenderer.GetComponentsInChildren(typeof(Component))){
            if(!(component is Transform)&&!(component is Renderer)&&!(component is MeshFilter)){
             DestroyImmediate(component);
            }
           }
           foreach(Transform transform in renderer.gameObject.GetComponentsInChildren<Transform>()){
            gameObjectsCloned.Add(transform.gameObject);
           }
          }
         }
        }
        [SerializeField]bool DEBUG_DRAW_COLLIDERS=false;
        #if UNITY_EDITOR
            protected void OnDrawGizmos(){
             if(DEBUG_DRAW_COLLIDERS){
              DrawColliders();
             }
            }
            void DrawColliders(){
             Color gizmosColor=Gizmos.color;
             foreach(Collider collider in collidersForTesting){
              if(collider is BoxCollider box){
               Gizmos.color=Color.green;
               Gizmos.matrix=Matrix4x4.TRS(transform.position+box.center,transform.rotation,transform.localScale);
               Gizmos.DrawCube(Vector3.zero,box.size);
              }
             }
             Gizmos.matrix=Matrix4x4.identity;
             foreach(var kvp in snappingRays){
              Collider collider=kvp.Key;
              Ray[]rays=kvp.Value;
              foreach(Ray ray in rays){
               Debug.DrawRay(collider.transform.position+ray.origin,ray.direction*11f,Color.green);
              }
             }
             Gizmos.color=gizmosColor;
            }
        #endif
    }
}