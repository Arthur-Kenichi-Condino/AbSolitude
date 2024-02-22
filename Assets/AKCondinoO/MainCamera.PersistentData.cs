#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AKCondinoO{
    internal partial class MainCamera{
     internal PersistentData persistentData;
        internal struct PersistentData{
         public Quaternion rotation;
         public Vector3    position;
            public override string ToString(){
             return string.Format(CultureInfoUtil.en_US,"persistentData={{ position={0} , rotation={1} , }}",position,rotation);
            }
            internal static PersistentData Parse(string s){
             PersistentData persistentData=new PersistentData();
             int positionStringStart=s.IndexOf("position=(");
             if(positionStringStart>=0){
                positionStringStart+=10;
              int positionStringEnd=s.IndexOf(") , ",positionStringStart);
              string positionString=s.Substring(positionStringStart,positionStringEnd-positionStringStart);
              string[]xyzString=positionString.Split(',');
              float x=float.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.position=new Vector3(x,y,z);
             }
             int rotationStringStart=s.IndexOf("rotation=(");
             if(rotationStringStart>=0){
                rotationStringStart+=10;
              int rotationStringEnd=s.IndexOf(") , ",rotationStringStart);
              string rotationString=s.Substring(rotationStringStart,rotationStringEnd-rotationStringStart);
              string[]xyzwString=rotationString.Split(',');
              float x=float.Parse(xyzwString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzwString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzwString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float w=float.Parse(xyzwString[3].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.rotation=new Quaternion(x,y,z,w);
             }
             return persistentData;
            }
        }
    }
}