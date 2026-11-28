using AKCondinoO.Bootstrap;
using UnityEngine;
namespace AKCondinoO.Ambience{
    internal class SkySphere:MonoBehaviour{
     internal MeshRenderer skyRenderer;
     internal Material skyMaterial;
     internal Color color{
      set{
       if(skyMaterial!=null){
        skyMaterial.color=value;
       }
      }
     }
        internal void OnInitialize(){
         skyRenderer=GetComponentInChildren<MeshRenderer>();
         if(skyRenderer!=null){
          skyMaterial=skyRenderer.sharedMaterial;
          Logs.Debug(()=>$"renderer:{skyRenderer.name};material:{skyMaterial.name};shader:{skyMaterial.shader.name}");
         }
        }
    }
}