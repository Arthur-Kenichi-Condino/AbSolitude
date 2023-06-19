#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Combat{
    /// <summary>
    ///  [https://www.youtube.com/watch?v=znZXmmyBF-o]
    /// </summary>
    internal class AISensor:MonoBehaviour{
     internal SimActor actor;
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
         float currentHeight1=-height/2f;
         Vector2 deltaAngle=new Vector2(angleHorizontal*2f/segments.x,angleVertical*2f/segments.y);
         Vector2 currentAngle1=new Vector2(-angleHorizontal,-angleVertical);
         int v=0;
         Vector3 bottomBack ;//=Vector3.down*(-height/2f);
         Vector3 bottomLeft ;//=Vector3.down*(height/2f)+Quaternion.Euler(-angleVertical,-angleHorizontal,0)*Vector3.forward*distance;
         Vector3 bottomRight;//=Vector3.down*(height/2f)+Quaternion.Euler(-angleVertical, angleHorizontal,0)*Vector3.forward*distance;
         Vector3 topBack ;//=Vector3.up*(height/2f);
         Vector3 topLeft ;//=Vector3.up*(height/2f)+Quaternion.Euler(angleVertical,-angleHorizontal,0)*Vector3.forward*distance;
         Vector3 topRight;//=Vector3.up*(height/2f)+Quaternion.Euler(angleVertical, angleHorizontal,0)*Vector3.forward*distance;
         for(int y=0;y<segments.y;++y){
          bottomBack =Vector3.up*currentHeight1;
          float currentHeight2=currentHeight1;
          currentHeight1+=deltaHeight;
          topBack =Vector3.up*currentHeight1;
          bottomLeft =bottomBack+Quaternion.Euler(currentAngle1.y,-angleHorizontal,0)*Vector3.forward*distance;
          bottomRight=bottomBack+Quaternion.Euler(currentAngle1.y, angleHorizontal,0)*Vector3.forward*distance;
          Vector2 currentAngle2=currentAngle1;
          currentAngle1.y+=deltaAngle.y;
          topLeft =topBack+Quaternion.Euler(currentAngle1.y,-angleHorizontal,0)*Vector3.forward*distance;
          topRight=topBack+Quaternion.Euler(currentAngle1.y, angleHorizontal,0)*Vector3.forward*distance;
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
          for(int x=0;x<segments.x;++x){
           bottomLeft =bottomBack+Quaternion.Euler(currentAngle2.y,currentAngle2.x,0)*Vector3.forward*distance;
           topLeft =topBack+Quaternion.Euler(currentAngle1.y,currentAngle2.x,0)*Vector3.forward*distance;
           currentAngle2.x+=deltaAngle.x;
           bottomRight=bottomBack+Quaternion.Euler(currentAngle2.y,currentAngle2.x,0)*Vector3.forward*distance;
           topRight=topBack+Quaternion.Euler(currentAngle1.y,currentAngle2.x,0)*Vector3.forward*distance;
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
        void OnTriggerEnter(Collider other){
         Log.DebugMessage("AISensor:OnTriggerEnter:"+this.transform.root.gameObject.name+"-> senses <-"+other.transform.root.gameObject.name);
        }
        void OnTriggerExit(Collider other){
         Log.DebugMessage("AISensor:OnTriggerExit:"+other.transform.root.gameObject.name);
        }
        void OnDrawGizmos(){
         #if UNITY_EDITOR
             //Gizmos.color=Color.yellow;
             //Gizmos.DrawWireMesh(wedgeMesh,transform.position,transform.rotation);
         #endif
        }
    }
}