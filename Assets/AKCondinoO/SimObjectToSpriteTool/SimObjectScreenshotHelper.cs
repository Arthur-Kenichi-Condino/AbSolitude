using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjectToSpriteTool{
    //  [https://answers.unity.com/questions/733240/how-to-take-a-screenshot-and-apply-it-as-a-texture.html]
    //  [https://answers.unity.com/questions/1473282/show-screen-capture-as-ui-image.html]
    internal class SimObjectScreenshotHelper:MonoBehaviour{
     WaitForEndOfFrame waitForEndOfFrame=new WaitForEndOfFrame();
        internal IEnumerator TakeScreenshot(Camera camera){
         yield return waitForEndOfFrame;
         if(camera==null){
          camera=Camera.main;
         }
         if(camera!=null){
          int sWidth=Screen.width;int sHeight=Screen.height;
          RenderTexture rt=new RenderTexture(sWidth,sHeight,24);
          camera.targetTexture=rt;
          Texture2D screenshot=new Texture2D(sWidth,sHeight,TextureFormat.RGB24,false);
          camera.Render();
          RenderTexture.active=rt;
          screenshot.ReadPixels(new Rect(0,0,sWidth,sHeight),0,0);
          screenshot.Apply();
          camera.targetTexture=null;
          RenderTexture.active=null;
          Destroy(rt);
         }
        }
    }
}