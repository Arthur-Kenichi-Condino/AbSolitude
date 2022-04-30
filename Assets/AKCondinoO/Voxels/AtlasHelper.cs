using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
namespace AKCondinoO.Voxels{
    internal static class AtlasHelper{
     internal static readonly Vector2[]uv=new Vector2[Enum.GetNames(typeof(MaterialId)).Length];
        internal static void SetAtlasData(){
         uv[(int)MaterialId.Dirt]=new Vector2(1,0);
         uv[(int)MaterialId.Rock]=new Vector2(0,0);
        }
    }
}