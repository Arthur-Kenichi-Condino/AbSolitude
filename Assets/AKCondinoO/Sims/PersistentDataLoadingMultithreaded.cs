#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class PersistentDataLoadingBackgroundContainer:BackgroundContainer{
    }
    internal class PersistentDataLoadingMultithreaded:BaseMultithreaded<PersistentDataLoadingBackgroundContainer>{ 
        protected override void Execute(){
        }
    }
}