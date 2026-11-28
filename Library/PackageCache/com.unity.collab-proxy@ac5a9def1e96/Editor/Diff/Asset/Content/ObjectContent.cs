using System.Collections.Generic;

using PlasticGui;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content
{
    internal class ObjectContent
    {
        internal PropertyTreeNode PropertyTree;
        internal List<ObjectContent> ComponentContents;

        internal int ObjectId;
        internal string DisplayName;
        internal string TypeName;
        internal string ObjectName;
        internal bool IsGameObjectRow;
        internal UnityEngine.Texture Icon;
        internal UnityEngine.Texture GameObjectIcon;

        // Populated when this row represents a section of a .meta file —
        // the originating .meta filename (MetaFileName) and the nicified
        // top-level YAML key (MetaSectionName, e.g. "Audio Importer"),
        // analogous to a Component header in UnityObject diffs. MetaFileName
        // is the canonical "this is a meta row" flag (see IsMetaFileObject);
        // Object is null on meta rows.
        internal string MetaFileName;
        internal string MetaSectionName;

        // Gated on MetaFileName rather than Object being null, so we don't
        // misclassify a UnityObject row whose underlying native object has
        // been destroyed (Unity's overloaded == reports it null) as a meta
        // row.
        internal bool IsMetaFileObject
        {
            get { return !string.IsNullOrEmpty(MetaFileName); }
        }

        internal int GetObjectId()
        {
            if (IsMetaFileObject)
                return (MetaFileName + ":" + MetaSectionName).GetHashCode();
            return ObjectId;
        }

        internal string GetDisplayName()
        {
            if (IsMetaFileObject)
                return GetMetaDisplayName();
            return DisplayName;
        }

        internal string GetTypeName()
        {
            return TypeName;
        }

        internal string GetObjectName()
        {
            return ObjectName;
        }

        internal bool IsGameObject()
        {
            return IsGameObjectRow;
        }

        string GetMetaDisplayName()
        {
            return string.IsNullOrEmpty(MetaSectionName)
                ? MetaFileName
                : MetaSectionName;
        }
    }
}
