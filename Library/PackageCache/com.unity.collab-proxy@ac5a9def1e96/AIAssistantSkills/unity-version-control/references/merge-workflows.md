# Merge & Apply Workflows

Workflows for merging branches, changesets, and applying shelves to the
current workspace.

## Table of Contents

- [Workflow 1: Merge from a Branch](#workflow-1-merge-from-a-branch)
- [Workflow 2: Merge from a Changeset](#workflow-2-merge-from-a-changeset)
- [Workflow 3: Apply a Shelve](#workflow-3-apply-a-shelve)

---

## Workflow 1: Merge from a Branch

### Steps

1. Call `Unity.UVCS.Execute(LaunchMergeFromBranchUI)` with the workspace path and
   branch name.
2. Confirm to the user that the merge view is now open.

---

## Workflow 2: Merge from a Changeset

### Steps

1. Call `Unity.UVCS.Execute(LaunchMergeFromChangesetUI)` with the workspace path and
   visible changeset ID (`changesetId`/`ChangesetId`, not internal `ID`/`Id`).
2. Confirm to the user that the merge view is now open.

---

## Workflow 3: Apply a Shelve

### Steps

1. Call `Unity.UVCS.Execute(LaunchApplyShelveUI)` with the workspace path and the signed
   shelve `changesetId`/`ChangesetId`, not internal `ID`/`Id`. If the user typed
   the positive display value, convert it to the signed internal shelve value.
2. Confirm to the user that the merge view is now open to apply the shelve.
