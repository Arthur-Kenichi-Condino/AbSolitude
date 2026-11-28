# File History Workflow

Workflow for retrieving and presenting the complete revision history of a file,
including revisions, moves, and deletions.

## Table of Contents

- [Workflow: File History](#workflow-file-history)

---

## Workflow: File History

### Steps

1. Call `Unity.UVCS.Execute(GetFileHistory)` with the project-relative file path
   (e.g. `Assets/Scripts/Player.cs`).
2. Present the results in **reverse chronological order** (newest revisions
   first). **Distinguish entry types visually:**
   - **Regular revisions (`HistoryRevision`)**: show changeset, date, author,
     and comment.
   - **Moves (`MoveRealizationInfo`)**: prefix with **(Moved)** and include the
     rename concisely, e.g. `OldName.cs` → `NewName.cs` (file names only, not
     full paths, unless the directory changed too).
   - **Removals (`RemovedRealizationInfo`)**: prefix with **(Deleted)**.
3. If the user wants more detail about a specific revision, use
   `Unity.UVCS.Execute(GetRevisionInfo)` or `Unity.UVCS.Execute(GetChangesetDifferences)`.

**Note:** If the user wants to see the history visually in the UVCS window (not
as data), use the
[Show File History in UI](ui-navigation-workflows.md#workflow-3-show-file-history-in-ui)
workflow instead.
