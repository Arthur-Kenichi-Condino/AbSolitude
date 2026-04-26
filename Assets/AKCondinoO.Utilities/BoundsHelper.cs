using UnityEngine;
namespace AKCondinoO.Utilities{
    internal static class BoundsHelper{
        internal static void TransformBoundsVertices(
         Bounds localBounds,
         Transform t,
         Vector3[]world
        ){
         TransformBoundsVertices(localBounds,t.localToWorldMatrix,world);
        }
        internal static void TransformBoundsVertices(
         Bounds localBounds,
         Vector3 position,
         Quaternion rotation,
         Vector3 scale,
         Vector3[]world
        ){
         var matrix=Matrix4x4.TRS(position,rotation,scale);
         TransformBoundsVertices(localBounds,matrix,world);
        }
        internal static void TransformBoundsVertices(
         Bounds localBounds,
         Matrix4x4 matrix,
         Vector3[]world
        ){
         Vector3 min=localBounds.min;
         Vector3 max=localBounds.max;
         world[0]=matrix.MultiplyPoint3x4(new(min.x,min.y,min.z));
         world[1]=matrix.MultiplyPoint3x4(new(max.x,min.y,min.z));
         world[2]=matrix.MultiplyPoint3x4(new(max.x,min.y,max.z));
         world[3]=matrix.MultiplyPoint3x4(new(min.x,min.y,max.z));
         world[4]=matrix.MultiplyPoint3x4(new(min.x,max.y,min.z));
         world[5]=matrix.MultiplyPoint3x4(new(max.x,max.y,min.z));
         world[6]=matrix.MultiplyPoint3x4(new(max.x,max.y,max.z));
         world[7]=matrix.MultiplyPoint3x4(new(min.x,max.y,max.z));
        }
    }
}