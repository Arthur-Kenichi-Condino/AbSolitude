using AKCondinoO.Bootstrap;
using UnityEngine;
namespace AKCondinoO.Ambience{
    internal class SimulationTime:MonoSingleton<SimulationTime>{
     [SerializeField]internal bool useRealTime=false;
     [SerializeField]internal float realTimeHoursInGameDay=4f;
     [SerializeField]internal float daysInMonth=28;
     [SerializeField]internal float monthsInYear=12;
     internal float gameSecond;
     internal float seconds;
        public override void Initialize(){
         base.Initialize();
         gameSecond=86400f/(realTimeHoursInGameDay*3600f);
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         seconds+=Time.deltaTime*gameSecond;
         //Logs.Debug(()=>"seconds:"+seconds);
        }
    }
}