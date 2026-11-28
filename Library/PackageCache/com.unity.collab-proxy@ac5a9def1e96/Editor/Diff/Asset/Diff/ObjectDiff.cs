using System.Collections.Generic;

using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal enum DiffType
    {
        Unchanged,
        Added,
        Removed,
        Modified
    }

    internal class ObjectDiff
    {
        internal DiffType DiffType;
        internal DataLossKind DataLoss;
        internal PropertyDiffNode PropertyDiffTree;
        internal List<ObjectDiff> ComponentDiffs;

        // Component reorder annotation. 0 = no position change.
        // Positive = moved down N positions, negative = moved up N positions.
        internal int PositionDelta;

        internal int ObjectId;
        internal string SrcDisplayName;
        internal string DstDisplayName;
        internal string TypeName;
        internal string ObjectName;
        internal bool IsGameObjectRow;
        internal UnityEngine.Texture Icon;
        internal UnityEngine.Texture GameObjectIcon;
        internal string SrcParentName;
        internal string DstParentName;
        internal bool ParentChanged;

        // Populated when this row represents a section of a .meta file —
        // the originating .meta filename (MetaFileName) and the nicified
        // top-level YAML key (MetaSectionName, e.g. "Audio Importer"),
        // analogous to a Component header in UnityObject diffs. MetaFileName
        // is the canonical "this is a meta row" flag (see IsMetaFileObject);
        // Src/DstObject are null on meta rows.
        internal string MetaFileName;
        internal string MetaSectionName;

        // Gated on MetaFileName rather than Src/DstObject being null, so we
        // don't misclassify a UnityObject row whose underlying native object
        // has been destroyed (Unity's overloaded == reports it null) as a
        // meta row.
        internal bool IsMetaFileObject
        {
            get { return !string.IsNullOrEmpty(MetaFileName); }
        }

        internal bool HasParentChange()
        {
            return ParentChanged;
        }

        internal int GetObjectId()
        {
            if (IsMetaFileObject)
                return (MetaFileName + ":" + MetaSectionName).GetHashCode();
            return ObjectId;
        }

        internal string GetSrcDisplayName()
        {
            if (IsMetaFileObject)
                return GetMetaDisplayName();
            return SrcDisplayName;
        }

        internal string GetDstDisplayName()
        {
            if (IsMetaFileObject)
                return GetMetaDisplayName();
            return DstDisplayName;
        }

        string GetMetaDisplayName()
        {
            return string.IsNullOrEmpty(MetaSectionName)
                ? MetaFileName
                : MetaSectionName;
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
    }
}
