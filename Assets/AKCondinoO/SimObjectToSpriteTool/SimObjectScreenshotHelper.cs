using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjectToSpriteTool{
    //  [https://answers.unity.com/questions/733240/how-to-take-a-screenshot-and-apply-it-as-a-texture.html]
    //  [https://answers.unity.com/questions/1473282/show-screen-capture-as-ui-image.html]
    internal class SimObjectScreenshotHelper:MonoBehaviour{
     private new Camera camera;
     WaitForEndOfFrame waitForEndOfFrame=new WaitForEndOfFrame();
        private IEnumerator TakeScreenshot(){
         yield return waitForEndOfFrame;
         camera=Camera.main;
         if(camera!=null){
          int sWidth=Screen.width;int sHeight=Screen.height;
          RenderTexture rt=new RenderTexture(sWidth,sHeight,24);
         }
        }
    }
}