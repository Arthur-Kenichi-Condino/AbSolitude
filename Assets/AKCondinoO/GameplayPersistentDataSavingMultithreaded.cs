using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO{
    internal class GameplayPersistentDataSavingBackgroundContainer:BackgroundContainer{
     internal FileStream mainCameraFileStream;
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          if(mainCameraFileStream!=null){mainCameraFileStream.Dispose();mainCameraFileStream=null;}
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class GameplayPersistentDataSavingMultithreaded:BaseMultithreaded<GameplayPersistentDataSavingBackgroundContainer>{
        protected override void Execute(){
        }
    }
}