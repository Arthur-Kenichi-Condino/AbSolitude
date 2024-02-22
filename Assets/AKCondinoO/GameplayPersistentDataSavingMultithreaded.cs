using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO{
    internal class GameplayPersistentDataSavingBackgroundContainer:BackgroundContainer{
     internal MainCamera.PersistentData?mainCameraPersistentData=null;
     internal FileStream mainCameraFileStream;
      internal StreamWriter mainCameraFileStreamWriter;
      internal StreamReader mainCameraFileStreamReader;
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          if(mainCameraFileStreamWriter!=null){mainCameraFileStreamWriter.Dispose();mainCameraFileStreamWriter=null;}
          if(mainCameraFileStreamReader!=null){mainCameraFileStreamReader.Dispose();mainCameraFileStreamReader=null;}
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class GameplayPersistentDataSavingMultithreaded:BaseMultithreaded<GameplayPersistentDataSavingBackgroundContainer>{
        protected override void Execute(){
         if(container.mainCameraFileStream!=null){
          if(container.mainCameraPersistentData!=null){
           container.mainCameraFileStream.SetLength(0L);
           container.mainCameraFileStreamWriter.Write(container.mainCameraPersistentData.Value.ToString());
           container.mainCameraFileStreamWriter.Flush();
          }
         }
        }
    }
}