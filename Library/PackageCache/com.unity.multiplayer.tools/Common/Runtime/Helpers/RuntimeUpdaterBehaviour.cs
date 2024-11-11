using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Multiplayer.Tools.Common
{
    /// <summary>
    /// Interface specifying callbacks for all relevant Unity events
    /// </summary>
    public interface IRuntimeUpdater
    {
        /// <summary>
        /// Callback for OnStart
        /// </summary>
        event Action OnStart;
        /// <summary>
        /// Callback for OnAwake
        /// </summary>
        event Action OnAwake;
        /// <summary>
        /// Callback for OnUpdate
        /// </summary>
        event Action OnUpdate;
        /// <summary>
        /// Callback for OnFixedUpdate
        /// </summary>
        event Action OnFixedUpdate;
        /// <summary>
        /// Callback for OnLateUpdate
        /// </summary>
        event Action OnLateUpdate;
        /// <summary>
        /// Callback for OnDestroyed
        /// </summary>
        event Action OnDestroyed;
    }

    class RuntimeUpdater : IRuntimeUpdater
    {
        RuntimeUpdaterBehaviour m_Component;

        event Action m_OnAwake;
        event Action m_OnStart;
        event Action m_OnUpdate;
        event Action m_OnFixedUpdate;
        event Action m_OnLateUpdate;
        event Action m_OnDestroyed;

        // TODO: Encapsulate those similar access pattern into a separate class.

        public event Action OnAwake
        {
            add
            {
                if (m_Component != null)
                {
                    m_Component.OnAwake += value;
                }

                // Keep tracks of events even when the gameObject wasn't instantiated yet:
                m_OnAwake += value;
            }
            remove
            {
                if (m_Component != null)
                {
                    m_Component.OnAwake -= value;
                }
                m_OnAwake -= value;
            }
        }

        public event Action OnStart
        {
            add
            {
                if (m_Component != null)
                {
                    m_Component.OnStart += value;
                }

                // Keep tracks of events even when the gameObject wasn't instantiated yet:
                m_OnStart += value;
            }
            remove
            {
                if (m_Component != null)
                {
                    m_Component.OnStart -= value;
                }
                m_OnStart -= value;
            }
        }

        public event Action OnUpdate
        {
            add
            {
                if (m_Component != null)
                {
                    m_Component.OnUpdate += value;
                }

                // Keep tracks of events even when the gameObject wasn't instantiated yet:
                m_OnUpdate += value;
            }
            remove
            {
                if (m_Component != null)
                {
                    m_Component.OnUpdate -= value;
                }
                m_OnUpdate -= value;
            }
        }

        public event Action OnFixedUpdate
        {
            add
            {
                if (m_Component != null)
                {
                    m_Component.OnFixedUpdate += value;
                }

                // Keep tracks of events even when the gameObject wasn't instantiated yet:
                m_OnFixedUpdate += value;
            }
            remove
            {
                if (m_Component != null)
                {
                    m_Component.OnFixedUpdate -= value;
                }
                m_OnFixedUpdate -= value;
            }
        }

        public event Action OnLateUpdate
        {
            add
            {
                if (m_Component != null)
                {
                    m_Component.OnLateUpdate += value;
                }

                // Keep tracks of events even when the gameObject wasn't instantiated yet:
                m_OnLateUpdate += value;
            }
            remove
            {
                if (m_Component != null)
                {
                    m_Component.OnLateUpdate -= value;
                }
                m_OnLateUpdate -= value;
            }
        }

        public event Action OnDestroyed
        {
            add
            {
                if (m_Component != null)
                {
                    m_Component.OnDestroyed += value;
                }

                // Keep tracks of events even when the gameObject wasn't instantiated yet:
                m_OnDestroyed += value;
            }
            remove
            {
                if (m_Component != null)
                {
                    m_Component.OnDestroyed -= value;
                }
                m_OnDestroyed -= value;
            }
        }

        public RuntimeUpdater()
        {
#if UNITY_EDITOR
            // Handling Edit Time behavior:
            void OnEditorApplicationOnplayModeStateChanged(PlayModeStateChange change)
            {
                switch (change)
                {
                    case PlayModeStateChange.EnteredPlayMode:
                    {
                        CreateInstance();
                        break;
                    }
                    case PlayModeStateChange.ExitingPlayMode:
                    {
                        Object.Destroy(m_Component);
                        break;
                    }
                }
            }

            EditorApplication.playModeStateChanged += OnEditorApplicationOnplayModeStateChanged;
#else
            CreateInstance();
#endif
        }

        void CreateInstance()
        {
            Debug.Assert(m_Component == null, m_Component);

            m_Component = new GameObject($"[{nameof(RuntimeUpdaterBehaviour)}]").AddComponent<RuntimeUpdaterBehaviour>();
            m_Component.gameObject.hideFlags = HideFlags.HideAndDontSave;
            m_Component.ReplaceCallbacks(m_OnAwake, m_OnStart, m_OnUpdate, m_OnFixedUpdate, m_OnLateUpdate, m_OnDestroyed);
            Object.DontDestroyOnLoad(m_Component.gameObject);
        }
    }

    /// <summary>
    /// Generic Updater for hooking into Update, FixedUpdate and LateUpdate callbacks.
    /// </summary>
    class RuntimeUpdaterBehaviour : MonoBehaviour, IRuntimeUpdater
    {
        public event Action OnAwake;
        public event Action OnStart;
        public event Action OnUpdate;
        public event Action OnFixedUpdate;
        public event Action OnLateUpdate;
        public event Action OnDestroyed;

        static string RuntimeErrorMessage => $"{nameof(RuntimeUpdaterBehaviour)} can only be called at runtime.";

        internal void ReplaceCallbacks(
            Action onAwake,
            Action onStart,
            Action onUpdate,
            Action onFixedUpdate,
            Action onLateUpdate,
            Action onDestroyed)
        {
            OnAwake = onAwake;
            OnStart = onStart;
            OnUpdate = onUpdate;
            OnFixedUpdate = onFixedUpdate;
            OnLateUpdate = onLateUpdate;
            OnDestroyed = onDestroyed;
        }

        void Awake()
        {
            DebugUtil.TraceMethodNameUsingStackFrame();
            OnAwake?.Invoke();
        }

        void Start()
        {
            DebugUtil.TraceMethodNameUsingStackFrame();
            OnStart?.Invoke();
        }

        void Update()
        {
            Debug.Assert(Application.isPlaying, RuntimeErrorMessage);
            OnUpdate?.Invoke();
        }

        void FixedUpdate()
        {
            Debug.Assert(Application.isPlaying, RuntimeErrorMessage);
            OnFixedUpdate?.Invoke();
        }

        void LateUpdate()
        {
            Debug.Assert(Application.isPlaying, RuntimeErrorMessage);
            OnLateUpdate?.Invoke();
        }

        void OnDestroy()
        {
            DebugUtil.Trace($"{nameof(RuntimeUpdaterBehaviour)} Destroyed");
            OnDestroyed?.Invoke();
            OnAwake = null;
            OnStart = null;
            OnUpdate = null;
            OnFixedUpdate = null;
            OnLateUpdate = null;
        }
    }
}
