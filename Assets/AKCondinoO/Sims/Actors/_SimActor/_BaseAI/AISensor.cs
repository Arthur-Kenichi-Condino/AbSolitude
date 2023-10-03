#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Combat{
    /// <summary>
    ///  [https://www.youtube.com/watch?v=t9e2XBQY4Og]
    ///  [https://www.youtube.com/watch?v=FbM4CkqtOuA]
    ///  [https://www.youtube.com/watch?v=znZXmmyBF-o]
    /// </summary>
    internal class AISensor:MonoBehaviour{
     internal BaseAI actor;
     [SerializeField]internal bool zIsUp=false;
     [SerializeField]internal float distance=96f;
     [SerializeField]internal float angleHorizontal=60f;
     [SerializeField]internal float height=.5f;
     [SerializeField]internal float angleVertical=25f;
     [SerializeField]internal Vector2Int segments=new Vector2Int(10,10);
      internal Mesh wedgeMesh;
       internal MeshCollider meshCollider;
        internal void CreateWedgeMesh(){
         meshCollider.sharedMesh=null;
         if(wedgeMesh){
          DestroyImmediate(wedgeMesh);
         }
         wedgeMesh=new Mesh();
         int trianglesCount=(segments.x*2)+(segments.y*4)+(segments.y*segments.x*2);
         int verticesCount=trianglesCount*3;
         Vector3[]vertices=new Vector3[verticesCount];
          int[]triangles=new int[verticesCount];
         float deltaHeight=height/segments.y;
         float height1=-height/2f;
         Vector2 deltaAngle=new Vector2(angleHorizontal*2f/segments.x,-angleVertical*2f/segments.y);
         Vector2 angles1=new Vector2(-angleHorizontal,angleVertical);
         int v=0;
         Vector3 bottomBack ;
         Vector3 bottomLeft ;
         Vector3 bottomRight;
         Vector3 topBack ;
         Vector3 topLeft ;
         Vector3 topRight;
         for(int y=0;y<segments.y;++y){
          bottomBack =Vector3.up*height1;
          float height2=height1;
          height2+=deltaHeight;
          topBack =Vector3.up*height2;
          bottomLeft =bottomBack+Quaternion.Euler(angles1.y,-angleHorizontal,0)*Vector3.forward*distance;
          bottomRight=bottomBack+Quaternion.Euler(angles1.y, angleHorizontal,0)*Vector3.forward*distance;
          Vector2 angles2=angles1;
          angles2.y+=deltaAngle.y;
          topLeft =topBack+Quaternion.Euler(angles2.y,-angleHorizontal,0)*Vector3.forward*distance;
          topRight=topBack+Quaternion.Euler(angles2.y, angleHorizontal,0)*Vector3.forward*distance;
          //  left side
          vertices[v++]=bottomBack;
          vertices[v++]=bottomLeft;
          vertices[v++]=topLeft;
          vertices[v++]=topLeft;
          vertices[v++]=topBack;
          vertices[v++]=bottomBack;
          //  right side
          vertices[v++]=bottomBack;
          vertices[v++]=topBack;
          vertices[v++]=topRight;
          vertices[v++]=topRight;
          vertices[v++]=bottomRight;
          vertices[v++]=bottomBack;
          Vector2 angles3=angles1;
          Vector2 angles4=angles2;
          for(int x=0;x<segments.x;++x){
           bottomLeft =bottomBack+Quaternion.Euler(angles4.y,angles4.x,0)*Vector3.forward*distance;
              topLeft =   topBack+Quaternion.Euler(angles3.y,angles4.x,0)*Vector3.forward*distance;
           angles4.x+=deltaAngle.x;
           bottomRight=bottomBack+Quaternion.Euler(angles4.y,angles4.x,0)*Vector3.forward*distance;
              topRight=   topBack+Quaternion.Euler(angles3.y,angles4.x,0)*Vector3.forward*distance;
           //  far side
           vertices[v++]=bottomLeft;
           vertices[v++]=bottomRight;
           vertices[v++]=topRight;
           vertices[v++]=topRight;
           vertices[v++]=topLeft;
           vertices[v++]=bottomLeft;
           if(y<=0){
            //  bottom
            vertices[v++]=bottomBack;
            vertices[v++]=bottomRight;
            vertices[v++]=bottomLeft;
           }
           if(y>=segments.y-1){
            //  top
            vertices[v++]=topBack;
            vertices[v++]=topLeft;
            vertices[v++]=topRight;
           }
          }
          height1=height2;
          angles1=angles2;
         }
         for(int i=0;i<verticesCount;++i){
          triangles[i]=i;
         }
         wedgeMesh.vertices=vertices;
         wedgeMesh.triangles=triangles;
         wedgeMesh.RecalculateNormals();
         meshCollider.sharedMesh=wedgeMesh;
        }
        void Awake(){
         meshCollider=GetComponent<MeshCollider>();
         CreateWedgeMesh();
        }
        void OnDestroy(){
         meshCollider.sharedMesh=null;
         if(wedgeMesh){
          DestroyImmediate(wedgeMesh);
         }
        }
        internal void Activate(){
         this.gameObject.SetActive(true);
        }
        internal void Deactivate(){
         this.gameObject.SetActive(false);
         //  OnTriggerExit will not be called
         simObjectCollidersInSight.Clear();
         foreach(var kvp in gotInSightOf){
          SimObject simObjectGotInSightOf=kvp.Key;
          if(simObjectGotInSightOf is BaseAI simActor&&simActor.aiSensor!=null){
           simActor.aiSensor.simObjectCollidersInSight.RemoveWhere(collider=>{return collider.transform.root==this.transform.root;});
          }
         }
         gotInSightOf.Clear();
        }
     internal readonly Dictionary<SimObject,int>gotInSightOf=new Dictionary<SimObject,int>();
     internal readonly HashSet<Collider>simObjectCollidersInSight=new HashSet<Collider>();
        void OnTriggerEnter(Collider other){
         if(!Core.singleton.isServer){
          return;
         }
         if(other.transform.root==this.transform.root){
          return;
         }
         //Log.DebugMessage("AISensor:OnTriggerEnter:"+this.transform.root.gameObject.name+"-> senses <-"+other.transform.root.gameObject.name);
         if(IsValidForSensing(other,out SimObject otherSimObject,out BaseAI otherSimActor)){
          //Log.DebugMessage("AISensor:OnTriggerEnter:SimObjectVolume:"+this.transform.root.gameObject.name+"-> senses <-"+other.transform.root.gameObject.name);
          simObjectCollidersInSight.Add(other);
          if(!otherSimActor.aiSensor.gotInSightOf.ContainsKey(actor)){
           otherSimActor.aiSensor.gotInSightOf.Add(actor,0);
          }else{
           otherSimActor.aiSensor.gotInSightOf[actor]++;
          }
          actor.OnSimObjectIsInSight(otherSimObject);
         }
        }
        void OnTriggerExit(Collider other){
         if(!Core.singleton.isServer){
          return;
         }
         //Log.DebugMessage("AISensor:OnTriggerExit:"+other.transform.root.gameObject.name);
         simObjectCollidersInSight.Remove(other);
         if(IsValidForSensing(other,out SimObject otherSimObject,out BaseAI otherSimActor)){
          actor.OnSimObjectIsOutOfSight(otherSimObject);
          if(otherSimActor.aiSensor.gotInSightOf.ContainsKey(actor)){
           otherSimActor.aiSensor.gotInSightOf[actor]--;
           if(otherSimActor.aiSensor.gotInSightOf[actor]<0){
            otherSimActor.aiSensor.gotInSightOf.Remove(actor);
           }
          }
         }
        }
        internal bool IsValidForSensing(Collider other,out SimObject otherSimObject,out BaseAI otherSimActor){
         otherSimActor=null;
         if(other.CompareTag("SimObjectVolume")&&!other.isTrigger&&(otherSimObject=other.GetComponentInParent<SimObject>())!=null&&otherSimObject is BaseAI otherIsSimActor&&otherIsSimActor.aiSensor!=null){
          otherSimActor=otherIsSimActor;
          return true;
         }
         otherSimObject=null;
         return false;
        }
        void OnDrawGizmos(){
         #if UNITY_EDITOR
             //Gizmos.color=Color.yellow;
             //Gizmos.DrawWireMesh(wedgeMesh,transform.position,transform.rotation);
         #endif
        }
    }
}