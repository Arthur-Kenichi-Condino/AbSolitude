# UI Navigation Workflows

Workflows for opening and navigating to specific views and windows in the
Unity Version Control UI.

## Table of Contents

- [Workflow 1: Show a UVCS View](#workflow-1-show-a-uvcs-view)
- [Workflow 2: Show Branch Explorer](#workflow-2-show-branch-explorer)
- [Workflow 3: Show File History in UI](#workflow-3-show-file-history-in-ui)

---

## Workflow 1: Show a UVCS View

### Steps

1. Call `Unity.UVCS.Execute(ShowView)` with the appropriate view type.
2. Confirm to the user which view is now displayed.

---

## Workflow 2: Show Branch Explorer

### Steps

1. Call `Unity.UVCS.Execute(ShowBranchExplorer)`.
2. Confirm to the user that the Branch Explorer window is now open.

---

## Workflow 3: Show File History in UI

### Steps

1. Call `Unity.UVCS.Execute(ShowFileHistory)` with the project-relative file path
   (e.g. `Assets/Scripts/Player.cs`).
2. Confirm to the user that the History view is now showing the file's
   revisions.

**Note:** If the user just wants history data (not the visual UI), use
`Unity.UVCS.Execute(GetFileHistory)` instead. See the
[File History workflow](file-history-workflows.md) for data retrieval.
