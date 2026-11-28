using AKCondinoO.Bootstrap;
using UnityEngine;
using static AKCondinoO.Ambience.SimulationTime.Season;
namespace AKCondinoO.Ambience{
    internal class DirectionalLightController:PoolableMonoBehaviour{
     [SerializeField]internal string id;
     internal Light lightComponent;
        internal override void OnRent(){
         base.OnRent();
         EnsureInitialized();
         if(lightComponent!=null){
          lightComponent.intensity=1f;
         }
        }
        internal override void OnPool(){
         EnsureInitialized();
         if(lightComponent!=null){
          lightComponent.color=Color.black;
          lightComponent.intensity=0f;
         }
         base.OnPool();
        }
        void EnsureInitialized(){
         if(this==null){
          return;
         }
         if(lightComponent==null){
          lightComponent=GetComponent<Light>();
         }
        }
     SeasonDirectionalLightAttributes attributes;
        internal void Set(SeasonDirectionalLightAttributes attributes){
         this.attributes=attributes;
        }
        internal void OnManualUpdate(int periodIndex,float localTime){
         var simTime=SimulationTime.singleton;
         lightComponent.color=EvaluateGradient(periodIndex,localTime,attributes.colorGradients);
        }
    }
}