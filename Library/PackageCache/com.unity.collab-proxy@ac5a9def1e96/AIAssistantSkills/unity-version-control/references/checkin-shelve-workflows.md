# Checkin & Shelve Workflows

Workflows for checking in and shelving pending changes in UVCS.

## Table of Contents

- [Workflow 1: Checkin Pending Changes](#workflow-1-checkin-pending-changes)
- [Workflow 2: Shelve Pending Changes](#workflow-2-shelve-pending-changes)

---

## Workflow 1: Checkin Pending Changes

### Steps

1. Call `Unity.UVCS.Execute(GetPendingChanges)` to verify there are changes to checkin.
   If no pending changes, inform the user and stop.
2. Show the user what will be checked in (list the pending changes).
3. Ask the user for a checkin comment if not already provided.
4. Call `Unity.UVCS.Execute(AskForConfirmation)` to confirm the checkin action,
   showing the comment and the number of files being checked in.
5. Call `Unity.UVCS.Execute(CheckinPendingChanges)` with the workspace path and comment.
6. Report the returned changeset number to the user and confirm success. Link it
   as the visible changeset ID; do not substitute any internal `ID` field from
   later query results.

---

## Workflow 2: Shelve Pending Changes

### Steps

1. Call `Unity.UVCS.Execute(GetPendingChanges)` to verify there are changes to shelve.
   If no pending changes, inform the user and stop.
2. Show the user what will be shelved (list the pending changes).
   **Do NOT ask any questions in this message** (no text questions about
   comments or workspace behavior). Just list the files, then immediately
   proceed to step 3.
3. Use `Unity.UVCS.Execute(SelectOption)` to ask what to do with the workspace after
   shelving:
   - `title`: `"After shelving, what should happen to the workspace changes?"`
   - `message`: `"Shelving saves your changes on the server. Choose whether to keep or remove them locally."`
   - `options`:
     - `label`: `"Keep changes"`,
       `description`: `"Shelve the changes but leave them in the workspace. You can continue working on them."`
     - `label`: `"Undo changes"`,
       `description`: `"Shelve the changes and revert the workspace to a clean state."`
   - If the user cancels, STOP.
   - **Do NOT call `AskForConfirmation` after this step.** The user selecting
     an option here is implicit confirmation to proceed with the shelve.
4. **Generate a shelve comment** (if the user did not already provide one):
   - Analyze the pending changes to understand what was modified. If needed,
     download and read the diffs to understand the nature of the changes.
   - Write a concise, meaningful comment that summarizes the changes
     (e.g. `"Refactor enemy AI movement logic"`, `"Fix null reference in
     PlayerHealth.OnDamage"`).
   - Only if you genuinely cannot determine a meaningful comment from the
     changes (e.g. many unrelated files changed with no clear theme), use
     `Unity.UVCS.Execute(SelectFromList)` to propose 2-3 candidate comments and let
     the user pick one.
5. Call `Unity.UVCS.Execute(ShelvePendingChanges)` with the workspace path, comment,
   and `undoPendingChangesAfterShelve` set to `true` if the user chose
   "Undo changes", or `false` if "Keep changes".
6. Report the shelve changeset number to the user and confirm success. If the
   returned value is negative, display its absolute value (for example,
   `shelve 54`) but keep the signed value for links and future shelve tool
   calls.
