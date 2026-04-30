using System;
using System.Collections.Generic;

namespace Unity.Multiplayer.Tools.DependencyInjection
{
    class Container
    {
        readonly List<DependencyDefinition> m_DependencyDefinitions = new List<DependencyDefinition>();

        internal IReadOnlyCollection<DependencyDefinition> DependencyDefinitions => m_DependencyDefinitions;

        public Container AddTransient<TImplementation>()
            where TImplementation : class
        {
            return AddTransient<TImplementation, TImplementation>();
        }

        public Container AddTransient<TDependency, TImplementation>()
            where TImplementation : class, TDependency
        {
            m_DependencyDefinitions.Add(DependencyDefinition.Transient<TDependency, TImplementation>());

            return this;
        }

        public Container AddTransient<TImplementation>(Func<TImplementation> instanceFactory)
            where TImplementation : class
        {
            return AddTransient<TImplementation, TImplementation>(instanceFactory);
        }

        public Container AddTransient<TDependency, TImplementation>(Func<TImplementation> instanceFactory)
            where TImplementation : class, TDependency
        {
            m_DependencyDefinitions.Add(DependencyDefinition.Transient<TDependency, TImplementation>(instanceFactory));

            return this;
        }

        public Container AddSingleton<TImplementation>()
            where TImplementation : class
        {
            return AddSingleton<TImplementation, TImplementation>();
        }

        public Container AddSingleton<TDependency, TImplementation>()
            where TImplementation : class, TDependency
        {
            m_DependencyDefinitions.Add(DependencyDefinition.Singleton<TDependency, TImplementation>());

            return this;
        }

        public Container AddSingleton<TImplementation>(TImplementation instance)
            where TImplementation : class
        {
            return AddSingleton<TImplementation, TImplementation>(instance);
        }

        public Container AddSingleton<TDependency, TImplementation>(TImplementation instance)
            where TImplementation : class, TDependency
        {
            m_DependencyDefinitions.Add(DependencyDefinition.Singleton<TDependency, TImplementation>(instance));

            return this;
        }
    }
}
