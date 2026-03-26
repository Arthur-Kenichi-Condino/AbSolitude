using AKCondinoO.Utilities;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    ///<summary>
    ///  AI: software não é uma escultura em pedra. É mais parecido com:
    /// um jardim que você mantém ao longo do tempo. Você poda, ajusta, melhora.
    ///  Main:
    ///  - Entry point real do runtime
    ///  - Responsável por inicializar sistemas globais
    ///  - Controla ciclo de vida de threads
    ///  - Coordena Singletons
    ///  - Atualiza manualmente sistemas no Update
    ///</summary>
    [DefaultExecutionOrder(-100000)]
    internal class Main:MonoSingleton<Main>{
     [SerializeField]private int workerCount=8;
     [SerializeField]ActiveZone activeZonePrefab;
     [SerializeField]private bool debugToggleLogsDebugMessages=false;
     [SerializeField]private bool debugCreateActiveZone=false;
     [SerializeField]private ulong debugCreateActiveZoneClientId=1u;
     [SerializeField]private Vector3 debugCreateActiveZonePos=new(16,128,16);
        protected override void Awake(){
         base.Awake();
         if(singleton==this){
         }
        }
        void Start(){
         if(singleton==this){
          Logs.Debug(()=>"...hello, World! I'm alive! :D");
          Pool.MultithreadedReturnDispatcher.Initialize();
          ThreadDispatcher.Initialize(workerCount<=0?null:workerCount);
          ActiveZone.EnsureExists(activeZonePrefab);
          SingletonManager.InitializeAll();
          ActiveZone.InitializeAny();
         }
        }
        protected override void OnDestroy(){
         if(singleton==this){
          SingletonManager.PreShutdownAll();
          ThreadDispatcher.Shutdown();
          ThreadDispatcher.FlushCompleted(true);
          SingletonManager.ShutdownAll();
          ActiveZone.ShutdownAny();
          Pool.MultithreadedReturnDispatcher.Shutdown();
         }
         base.OnDestroy();
        }
        public override void Initialize(){
         base.Initialize();
         if(this!=null){
         }
        }
        public override void Shutdown(){
         if(this!=null){
         }
         Logs.Debug(()=>"...good night, World. :)");
         base.Shutdown();
        }
        void Update(){
         if(debugToggleLogsDebugMessages){
          Logs.enableAll=!Logs.enableAll;
          debugToggleLogsDebugMessages=false;
         }
         if(debugCreateActiveZone){
          debugCreateActiveZone=false;
          ActiveZone.OnAddZoneFor(debugCreateActiveZoneClientId,activeZonePrefab);
          ActiveZone.InitializeAny();
          ActiveZone.zones[debugCreateActiveZoneClientId].transform.position=debugCreateActiveZonePos;
         }
         ThreadDispatcher.FlushCompleted();
         ActiveZone.ManualUpdateTransformAll();
         SingletonManager.ManualUpdateAll();
        }
    }
}