# Lock Management Workflows

Workflow for viewing, releasing, and managing file locks in a UVCS repository.

## Table of Contents

- [Workflow: Lock Management](#workflow-lock-management)

---

## Workflow: Lock Management

### Steps

1. Call `Unity.UVCS.Execute(ListLocks)` to get all current locks.
2. Present the locks in a table with columns: File (CmPath), Status, Workspace,
   and Branch.
3. If the user wants to release or remove specific locks:
   a. Show the locks and let the user identify which ones to act on.
   b. Use `Unity.UVCS.Execute(AskForConfirmation)` before releasing or removing locks,
      explaining which locks will be affected.
   c. Call `Unity.UVCS.Execute(ReleaseLocks)` (for normal unlock) or
      `Unity.UVCS.Execute(RemoveLocks)` (for force-remove, e.g. locks held by others).
4. If the user asks about lock rules, call `Unity.UVCS.Execute(GetLockRule)` and present
   the rules and excluded branches.

### Critical Rules

- `Unity.UVCS.Execute(ReleaseLocks)` releases locks held by the current user. Use this
  for normal unlock operations.
- `Unity.UVCS.Execute(RemoveLocks)` force-removes locks regardless of who holds them.
  Only use when the user explicitly requests force-removal. Always confirm with
  `Unity.UVCS.Execute(AskForConfirmation)` before force-removing.
