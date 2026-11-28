# Undo & Revert Workflows

Workflows for discarding pending changes, cleaning up unchanged checkouts,
and reverting files to previous revisions.

## Table of Contents

- [Workflow 1: Undo Changes (Discard)](#workflow-1-undo-changes-discard)
- [Workflow 2: Undo Unchanged](#workflow-2-undo-unchanged)
- [Workflow 3: Revert File to Revision](#workflow-3-revert-file-to-revision)

---

## Workflow 1: Undo Changes (Discard)

**CRITICAL: This operation causes PERMANENT, IRREVERSIBLE DATA LOSS.** All
local modifications in the affected files will be destroyed and cannot be
recovered. Follow every step below without exception.

### Steps

1. Call `Unity.UVCS.Execute(GetPendingChanges)` to verify there are changes to undo.
   If no pending changes, inform the user and stop.
2. In your chat response (NOT in the confirmation dialog), present:
   - The number of pending changes (e.g. "You have 3 pending changes").
   - A short summary table or list of the affected files.
   - A disclaimer: *"This operation is irreversible. If you want to preserve
     your changes, you can shelve them first."*
3. Determine whether to undo all or specific files based on the user's request.
4. Call `Unity.UVCS.Execute(AskForConfirmation)` with:
   - `action`: `"Discard changes"` (or `"Discard N file(s)"` when specific
     files).
   - `detail`: A short, focused warning — file count and irreversibility only.
     Example: `"Permanently discard all local modifications in 3 file(s).\nThis cannot be undone."`.
     Do NOT list individual files here (they are already visible in the chat).
     Do NOT ask about shelving here (the user already saw the suggestion).
5. If the user confirms, call `Unity.UVCS.Execute(UndoChanges)` with the workspace path
   and the list of paths to undo. To undo all changes, pass the workspace root
   path as a single-item list.
6. Confirm to the user that the changes were discarded.

### Critical Rules

- NEVER skip the confirmation step. NEVER call `Unity.UVCS.Execute(UndoChanges)`
  without going through `Unity.UVCS.Execute(AskForConfirmation)` first.
- If the user declines the confirmation, STOP immediately.

---

## Workflow 2: Undo Unchanged

### Steps

1. Call `Unity.UVCS.Execute(UndoUnchanged)` with the workspace path.
2. Confirm to the user that unchanged files were removed from the pending
   changes list.

---

## Workflow 3: Revert File to Revision

### Steps

1. If the user hasn't specified a revision, call `Unity.UVCS.Execute(GetFileHistory)`
   to show available revisions and let the user choose.
2. Call `Unity.UVCS.Execute(AskForConfirmation)` to confirm the revert, showing the
   file path and target revision.
3. Call `Unity.UVCS.Execute(RevertToRevision)` with the project-relative file path
   (e.g. `Assets/Scripts/Player.cs`) and revision ID.
4. Confirm to the user that the file was reverted. The file will appear as a
   pending change.
