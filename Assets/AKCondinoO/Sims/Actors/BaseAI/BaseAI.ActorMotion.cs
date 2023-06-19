#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal enum ActorMotion:int{
         MOTION_STAND=0,
         MOTION_MOVE =1,
         MOTION_RIFLE_STAND=50,
         MOTION_RIFLE_MOVE =51,
        }
        internal virtual void UpdateMotion(bool fromAI){
         if(fromAI){
          if(MyPathfinding==PathfindingResult.TRAVELLING){
              if(MyWeaponType==WeaponTypes.SniperRifle){
               MyMotion=ActorMotion.MOTION_RIFLE_MOVE;
              }else{
               MyMotion=ActorMotion.MOTION_MOVE;
              }
          }else{
              if(MyWeaponType==WeaponTypes.SniperRifle){
               MyMotion=ActorMotion.MOTION_RIFLE_STAND;
              }else{
               MyMotion=ActorMotion.MOTION_STAND;
              }
          }
         }else{
          if(moveVelocityFlattened>0f){
              if(MyWeaponType==WeaponTypes.SniperRifle){
               MyMotion=ActorMotion.MOTION_RIFLE_MOVE;
              }else{
               MyMotion=ActorMotion.MOTION_MOVE;
              }
          }else{
              if(MyWeaponType==WeaponTypes.SniperRifle){
               MyMotion=ActorMotion.MOTION_RIFLE_STAND;
              }else{
               MyMotion=ActorMotion.MOTION_STAND;
              }
          }
         }
        }
        internal virtual void OnShouldSetNextMotion(int layerIndex,string lastClipName,string currentClipName){
         //Log.DebugMessage("OnShouldSetNextMotion");
        }
    }
}