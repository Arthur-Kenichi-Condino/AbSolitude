using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class Vector3Util{
     static readonly Vector3[]upsToProject=new Vector3[]{
      Vector3.up,Vector3.right,Vector3.forward,
     };
        internal static void GetBestUpAndProjectionsOnPlane(Vector3 dir1,Vector3 dir2,ref Vector3 up,out Vector3 projDir1OnUp,out Vector3 projDir2OnUp,bool tryDir2Dir1CrossUp=true){
         Vector3 initialUp=up;
         Project(ref up,out projDir1OnUp,out projDir2OnUp);
         void Project(ref Vector3 up,out Vector3 projDir1OnUp,out Vector3 projDir2OnUp){
          projDir1OnUp=Vector3.ProjectOnPlane(dir1,up).normalized;
          projDir2OnUp=Vector3.ProjectOnPlane(dir2,up).normalized;
         }
         ValidadeUpForProjection(ref up,ref projDir1OnUp,ref projDir2OnUp);
         void ValidadeUpForProjection(ref Vector3 up,ref Vector3 projDir1OnUp,ref Vector3 projDir2OnUp){
          for(int i=0;((projDir1OnUp==Vector3.zero||projDir2OnUp==Vector3.zero)&&i<upsToProject.Length);++i){
           up=upsToProject[i];
           if(up==initialUp){continue;}
           Project(ref up,out projDir1OnUp,out projDir2OnUp);
          }
          if(tryDir2Dir1CrossUp){
           if(projDir1OnUp==Vector3.zero||projDir2OnUp==Vector3.zero){
            Vector3 a=dir2;//  left
            Vector3 b=dir1;//  forward
            up=Vector3.Cross(
             a,
             b
            );
            projDir1OnUp=b.normalized;
            projDir2OnUp=a.normalized;
           }
          }
         }
         //if(
         // projDir1OnUp==Vector3.zero||
         // projDir2OnUp==Vector3.zero||
         // projDir1OnUp==projDir2OnUp
         //){
         // up=Vector3.up;
         // projDir1OnUp=Vector3.ProjectOnPlane(dir1,up).normalized;
         // projDir2OnUp=Vector3.ProjectOnPlane(dir2,up).normalized;
         // if(
         //  projDir1OnUp==Vector3.zero||
         //  projDir2OnUp==Vector3.zero||
         //  projDir1OnUp==projDir2OnUp
         // ){
         //  up=Vector3.right;
         //  projDir1OnUp=Vector3.ProjectOnPlane(dir1,up).normalized;
         //  projDir2OnUp=Vector3.ProjectOnPlane(dir2,up).normalized;
         //  if(
         //   projDir1OnUp==Vector3.zero||
         //   projDir2OnUp==Vector3.zero||
         //   projDir1OnUp==projDir2OnUp
         //  ){
         //   up=Vector3.forward;
         //   projDir1OnUp=Vector3.ProjectOnPlane(dir1,up).normalized;
         //   projDir2OnUp=Vector3.ProjectOnPlane(dir2,up).normalized;
         //   if(
         //    projDir1OnUp==Vector3.zero||
         //    projDir2OnUp==Vector3.zero||
         //    projDir1OnUp==projDir2OnUp
         //   ){
         //    if(tryDir2Dir1CrossUp){
         //     Vector3 a=dir2;//  left
         //     Vector3 b=dir1;//  forward
         //     up=Vector3.Cross(
         //      a,
         //      b
         //     );
         //     projDir1OnUp=b.normalized;
         //     projDir2OnUp=a.normalized;
         //    }
         //   }
         //  }
         // }
         //}
        }
    }
}