using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class MainCamera:MonoSingleton<MainCamera>{
     public override int initOrder{get{return 4;}}
    }
}