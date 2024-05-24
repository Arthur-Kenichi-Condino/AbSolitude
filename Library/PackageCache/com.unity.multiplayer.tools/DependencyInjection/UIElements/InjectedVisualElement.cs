using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.DependencyInjection.UIElements
{
    /// <remarks>
    /// We have discussed making the dependency injection more generic and not bound to visual elements only.
    /// We feel like the logic is easy enough to extract and implement in a mechanism that will suit our needs
    /// when we get to that point. As of now, the capabilities that this allows in VisualElement classes is enough.
    /// </remarks>
    abstract class InjectedVisualElement<TView> : VisualElement
    {
        protected InjectedVisualElement()
        {
            InitializeAsync().Forget();
        }

        async Task InitializeAsync()
        {
            InjectDependencies();
            await LoadUxmlAsync();
            InjectQueryFields();
            Initialized();
        }

        void InjectDependencies()
        {
            foreach (var field in InjectedVisualElementCache.GetInjectFields<TView>())
            {
                var value = Context.Resolver.Resolve(field.FieldType);
                field.SetValue(this, value);
            }
        }

        async Task LoadUxmlAsync()
        {
            var uxmlPath = (LoadUxmlViewAttribute)Attribute.GetCustomAttribute(typeof(TView), typeof(LoadUxmlViewAttribute));
            if (uxmlPath == null || string.IsNullOrWhiteSpace(uxmlPath.RootPath))
            {
                return;
            }

            var viewName = string.IsNullOrWhiteSpace(uxmlPath.ViewName) ? typeof(TView).Name : uxmlPath.ViewName;
            var filePath = $"{uxmlPath.RootPath}{viewName}.uxml";
            var loadAssetAtPath = await AssetDatabaseHelper.LoadAssetAtPathAsync<VisualTreeAsset>(filePath);
            loadAssetAtPath.CloneTree(this);
        }

        void InjectQueryFields()
        {
            foreach (var field in InjectedVisualElementCache.GetQueryFields<TView>())
            {
                var attribute = (UxmlQueryAttribute)field.GetCustomAttribute(typeof(UxmlQueryAttribute));
                var queryName = string.IsNullOrWhiteSpace(attribute.Name) ? field.Name : attribute.Name;
                var element = this.Q(queryName);
                field.SetValue(this, element);
            }
        }

        protected virtual void Initialized()
        {
        }
    }

    /// <remarks>
    /// Leveraging InitializeOnLoad allows us to compute this when the domain is reloaded. The alternative is getting
    /// the fields we need whenever the view is first created, which results in a noticeable hang.
    /// </remarks>
    [InitializeOnLoad]
    static class InjectedVisualElementCache
    {
        static readonly Dictionary<Type, List<FieldInfo>> s_InjectAttributeFields;
        static readonly Dictionary<Type, List<FieldInfo>> s_QueryAttributeFields;

        static InjectedVisualElementCache()
        {
            s_InjectAttributeFields = TypeCache.GetFieldsWithAttribute<InjectAttribute>()
                .GroupBy(x => x.ReflectedType)
                .ToDictionary(x => x.Key, x => x.ToList());
            s_QueryAttributeFields = TypeCache.GetFieldsWithAttribute<UxmlQueryAttribute>()
                .GroupBy(x => x.ReflectedType)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public static IReadOnlyCollection<FieldInfo> GetInjectFields<TView>()
        {
            if (!s_InjectAttributeFields.TryGetValue(typeof(TView), out var fields))
            {
                fields = new List<FieldInfo>();
            }

            return fields;
        }

        public static IReadOnlyCollection<FieldInfo> GetQueryFields<TView>()
        {
            if (!s_QueryAttributeFields.TryGetValue(typeof(TView), out var fields))
            {
                fields = new List<FieldInfo>();
            }

            return fields;
        }
    }
}
