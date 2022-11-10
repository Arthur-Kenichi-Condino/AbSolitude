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
    }
}