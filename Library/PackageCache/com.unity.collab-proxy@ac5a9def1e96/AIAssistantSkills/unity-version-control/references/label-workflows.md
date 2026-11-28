# Label Management Workflows

Workflows for creating, applying, renaming, and deleting labels in UVCS.

## Table of Contents

- [Workflow 1: Create a Label](#workflow-1-create-a-label)
- [Workflow 2: Apply a Label](#workflow-2-apply-a-label)
- [Workflow 3: Rename a Label](#workflow-3-rename-a-label)
- [Workflow 4: Delete a Label](#workflow-4-delete-a-label)

---

## Workflow 1: Create a Label

### Steps

1. If the user hasn't specified a label name, ask for one.
2. If the user hasn't specified a comment, generate a brief one based on context
   (e.g. the label name or the user's intent).
3. Call `Unity.UVCS.Execute(CreateLabel)` with the workspace path, label name, and comment.
   If the user specified a changeset number, pass it; otherwise omit it and the
   label will be applied to the current workspace changeset.
4. Confirm success and show the created label details.

---

## Workflow 2: Apply a Label

### Steps

1. If the user hasn't specified a label name, ask for one.
2. Warn that applying a label switches the workspace to the changeset the label
   points to, then call `Unity.UVCS.Execute(AskForConfirmation)`.
3. If confirmed, call `Unity.UVCS.Execute(ApplyLabel)` with the workspace path and label
   name.
4. Confirm success.

---

## Workflow 3: Rename a Label

### Steps

1. Call `Unity.UVCS.Execute(RenameLabel)` with the current label name and the new name.
2. Confirm success.

---

## Workflow 4: Delete a Label

### Steps

1. Call `Unity.UVCS.Execute(AskForConfirmation)` before deleting, warning the user that
   this operation is irreversible.
2. If confirmed, call `Unity.UVCS.Execute(DeleteLabel)` with the label name.
3. Confirm success.

### Critical Rules

- NEVER delete a label without explicit user confirmation via
  `Unity.UVCS.Execute(AskForConfirmation)`.
- NEVER apply a label without explicit user confirmation via
  `Unity.UVCS.Execute(AskForConfirmation)`, because it moves the workspace to a
  different changeset.
