#if UNITY_6000_5_OR_NEWER
using System;
using System.Reflection;

using Unity.Hierarchy;

namespace Unity.PlasticSCM.Editor.UnityInternals.UnityEditor
{
    internal static class HierarchyViewExtensions
    {
        static HierarchyViewExtensions()
        {
            CacheInternalReferences();
        }

        internal static void RefreshItems(this HierarchyView view)
        {
            if (view == null ||
                ListViewProperty == null ||
                RefreshItemsMethod == null)
                return;

            object listView = ListViewProperty.GetValue(view);

            if (listView == null)
                return;

            RefreshItemsMethod?.Invoke(listView, RefreshItemsMethodArgs);
        }

        static void CacheInternalReferences()
        {
            ListViewProperty = typeof(HierarchyView).GetProperty(
                "ListView",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (ListViewProperty == null)
                return;

#if UNITY_6000_6_OR_NEWER
            // From 6.6, the method takes a boolean
            RefreshItemsMethod = ListViewProperty.PropertyType.GetMethod(
                "RefreshItems",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(bool) },
                null);

            RefreshItemsMethodArgs = new object[] { true };
#else
            // In 6.5, the method takes no parameters
            RefreshItemsMethod = ListViewProperty.PropertyType.GetMethod(
                "RefreshItems",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                Type.EmptyTypes,
                null);
#endif
        }

        static PropertyInfo ListViewProperty;
        static MethodInfo RefreshItemsMethod;
        static object[] RefreshItemsMethodArgs;
    }
}
#endif
