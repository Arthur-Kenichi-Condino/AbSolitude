---
name: unity-version-control
description: "Performs Unity Version Control (UVCS / Plastic SCM) operations: workspace setup and project onboarding; querying branches, changesets, labels, shelves, and revisions by author, date, or name; branch ops (create, rename, switch, merge, cherry-pick); diff and compare across branches, changesets, labels, shelves, and pending changes; file history and per-line blame/annotate; code review (create findings, address feedback, rework); checkin, shelve/stash, undo/discard, revert; label and lock management; root cause analysis and bug tracing; ignore rules; UVCS view navigation. Use when the user asks about UVCS, Plastic SCM, source control, changesets, shelves, checkin, commit, push, undo, discard, revert, merge, cherry-pick, code review, file history, blame, who changed something, why something changed, what changed, or tracing a regression to its source."
tools:
  - Unity.UVCS.Execute
  - Unity.GetUnityDependenciesTool
  - Unity.GetDependency
  - Unity.Grep
required_packages:
  com.unity.collab-proxy: ">=2.13.0"
---

# Unity Version Control

Assist the user with Unity Version Control (Unity VCS, UVCS, Plastic SCM) operations.

## Invoking UVCS operations

Every UVCS capability is exposed through a **single** agent tool: `Unity.UVCS.Execute`. It takes two parameters:
- `operation` — the operation to run (e.g. `FindQuery`, `GetBranchInfo`, `UndoChanges`).
- `parametersJson` — a JSON object string with that operation's arguments. Omit it for operations that take no parameters.

Throughout this skill and its references, an operation written as **`Unity.UVCS.Execute(FindQuery)`** is shorthand for: call `Unity.UVCS.Execute` with `operation = "FindQuery"` and `parametersJson` set to that operation's arguments. For example:
- `Unity.UVCS.Execute(FindQuery)` → `operation="FindQuery"`, `parametersJson={"query":"find branch where name like 'scm%'"}`
- `Unity.UVCS.Execute(GetBranchInfo)` → `operation="GetBranchInfo"`, `parametersJson={"branchName":"/main/task001"}`
- `Unity.UVCS.Execute(ListLocks)` → `operation="ListLocks"` (no `parametersJson`)

The exact `parametersJson` shape and return value of **every** operation is documented in [references/tool-reference.md](references/tool-reference.md). Read it before calling an operation you are unsure about.

> **Only UVCS operations go through `Unity.UVCS.Execute`.** The other tools available to this skill — `Unity.GetUnityDependenciesTool`, `Unity.GetDependency`, and `Unity.Grep` — are separate standalone tools. Call them by their exact names (e.g. `Unity.GetDependency`); never wrap them as `Unity.UVCS.Execute(...)`.

## Critical Rules

### Identity & Sourcing
- **NEVER use Git commands/jargon for version control when the project is under UVCS.** If the project has a UVCS workspace (verified via `Unity.UVCS.Execute(GetWorkspaceFromPath)`), ALL version control operations — querying branches, changesets, history, authors, dates, merges, labels, revisions, etc. — MUST go through UVCS tools (e.g. `Unity.UVCS.Execute(FindQuery)`). Do NOT fall back to `git log`, `git branch`, `git show`, `Unity.RunCommand` with git commands, or any other Git-based approach. UVCS and Git are separate systems; Git commands return incorrect or empty results for UVCS-managed projects.
- **When the project is NOT under a UVCS workspace, do NOT promote or assume UVCS.** If `Unity.UVCS.Execute(GetWorkspaceFromPath)` returns no workspace, the project is not under UVCS — it may use Git or another system, or none. In that case, UNLESS the user is explicitly asking to set up / onboard the project to Unity Version Control (the "Add Project to Unity Version Control" workflow), do NOT run UVCS tools and do NOT steer the user toward UVCS. Handle the request exactly as the base assistant would, including normal Git operations; only mention UVCS if the user asks about it. The presence of a `.git` directory is corroborating evidence that another VCS is in use, but the `GetWorkspaceFromPath` result is authoritative: if a UVCS workspace **is** present, it wins even when a `.git` directory also exists.
- **ALWAYS call the relevant UVCS tool before answering user-specific state questions.** NEVER guess UVCS state.
- **ALWAYS read the tool documentation before using a tool.** Follow any specific instructions or rules mentioned there. The full documentation is in [references/tool-reference.md](references/tool-reference.md).
- **Before calling `Unity.UVCS.Execute(FindQuery)`, read the query syntax reference.** Do not compose UVCS queries from SQL, Git, or memory. Load [references/query-syntax.md](references/query-syntax.md), check the target object's valid fields and sortable fields, then call `FindQuery` once with a validated query. If the first query fails, do not retry with speculative syntax; reread the reference and report unsupported operations clearly.
- **If a tool fails, report the error and suggest the next step.** Do not retry automatically.

### User Interaction
- **ALWAYS use the user interaction tools, not text lists or text questions.**
  - Multiple options to pick from → `Unity.UVCS.Execute(SelectFromList)` (dropdown).
  - Options that need explanations to compare → `Unity.UVCS.Execute(SelectOption)` (radio buttons with descriptions).
  - Confirm an action before performing it → `Unity.UVCS.Execute(AskForConfirmation)` (confirmation dialog).
- **If a user interaction tool returns "User cancelled" / "User declined", STOP the current workflow immediately.** Tell the user the operation was cancelled. Do NOT retry, do NOT re-present the options as a numbered text list, and do NOT continue with the workflow.

### Safety
- **NEVER call `Unity.UVCS.Execute(UndoChanges)` without explicit user confirmation.** This operation is **DESTRUCTIVE and IRREVERSIBLE** — all unsaved work in the affected files will be permanently lost. You MUST:
  1. Call `Unity.UVCS.Execute(GetPendingChanges)` first to show the user exactly which files will be affected.
  2. Explicitly warn the user that **all local modifications will be permanently deleted and cannot be recovered**.
  3. Suggest shelving first if the user might want to preserve the changes.
  4. Call `Unity.UVCS.Execute(AskForConfirmation)` with a clear warning message before proceeding.
  Do NOT call `Unity.UVCS.Execute(UndoChanges)` automatically, even if you believe it would be helpful. The user MUST always confirm.
- **NEVER switch branches without explicit user confirmation.** If the workspace is not pointing to the expected branch, inform the user and ask whether they want to switch. Use `Unity.UVCS.Execute(AskForConfirmation)` before calling `Unity.UVCS.Execute(LaunchSwitchToBranchUI)`. Do NOT switch branches automatically, even if you believe it would be helpful.

### Presentation
- **When presenting structured data** (history, file lists, query results, etc.) prefer a **table** over a bullet list — tables are more compact and easier to scan. Keep tables to **4 columns or fewer**; if more fields are needed, pick the most important ones for the columns and fold the rest into an existing column (e.g. append a rename note to the Comment column).
- **For changesets and shelves, present the user-visible changeset number, not the internal object ID.** Query/tool results can include both `ID`/`Id` and `changesetId`/`ChangesetId`. The `ID` field is UVCS's internal object ID and is not the number the user can recognize or use in the UI. For changesets, display and link the `changesetId`. For shelves, use the shelve's `changesetId` as the canonical identifier; it is usually negative internally, so display its absolute value to the user (for example, `shelve 54`) while keeping the signed negative value for UVCS tool parameters and `uvcs://` links (for example, `uvcs://show/shelve/-54`). Never show users the internal `ID` as the changeset/shelve number.

### Clickable Links (`uvcs://`)

**Before creating or validating any `uvcs://` link, read [references/clickable-links.md](references/clickable-links.md).** Do not invent link actions, entity types, or URL shapes from memory. The reference is the source of truth for valid patterns; if the link you want is not listed there, do not fabricate a new `uvcs://` action.

**ALWAYS wrap changeset IDs, branch names, shelve IDs, and label names in clickable `uvcs://` links.** Use action links (`merge`, `open`) when suggesting actions. Never output a bare entity name without a link.

**CRITICAL link formatting rules:**
- **ALWAYS use markdown link syntax:** `[display text](uvcs://action/type/id)`. The `uvcs://` URL must ONLY appear inside the parentheses of a markdown link, NEVER as visible text.
- **NEVER write bare `uvcs://` URLs as text.** Writing `uvcs:///main` or `uvcs:///main/seb` is WRONG — these are malformed URLs with no action or type segment, and they should never appear as visible text.
- **Correct format:** `[/main/seb](uvcs://show/branch/main/seb)` — the user sees `/main/seb` as a clickable link.
- **Applying a shelve uses the merge action:** `[apply it](uvcs://merge/shelve/-54)`. `uvcs://apply/shelve/-54` is invalid because `apply` is not a defined `uvcs://` action.
- **`show/` and `merge/` use lowercase singular entity types (`branch`, `changeset`, `shelve`, `history`).** `open/` and `refresh/` use the **PascalCase `ViewType` enum** as a single segment, with NO extra `view/` segment and NO kebab-case. The valid `open/` URLs are exactly: `uvcs://open/PendingChangesView`, `uvcs://open/IncomingChangesView`, `uvcs://open/ChangesetsView`, `uvcs://open/ShelvesView`, `uvcs://open/BranchesView`, `uvcs://open/LabelsView`, `uvcs://open/LocksView`, `uvcs://open/BranchExplorerView`. Anything like `uvcs://open/view/branch-explorer`, `uvcs://open/pending-changes`, or `uvcs://open/shelves` is invalid.
- **If no link action exists for the verb you want to express, write plain text — do NOT invent a link.** The "wrap every entity in a link" rule covers existing entities (a known changeset, branch, shelve, label). It does NOT cover *creation* verbs. There is no `uvcs://` action for shelving pending changes, checking in, creating a branch, or creating a label. Phrases like "I can shelve these changes" or "checkin your work" must be plain text. NEVER fabricate a placeholder ID such as `-1`, `merge/shelve/-1`, or `show/changeset/0` to satisfy the linking rule. If you want to give the user something clickable, link to the relevant view instead, e.g. `[pending changes](uvcs://open/PendingChangesView)`.

For the full link pattern reference (show, open, refresh, merge) and examples, see [references/clickable-links.md](references/clickable-links.md).

### Diff Behavior

All diff workflows share one rule: the diff tools return metadata only, and **you** decide whether to also download and explain file content based on the user's phrasing:
- **"Which files changed?"**, **"List the changed files"**, **"Enumerate changes"** → Return metadata only (file list with statuses).
- **"What changed?"**, **"Tell me what changed"**, **"Explain"**, **"Describe"** → Download ALL text file revisions, read them, and explain the actual content differences. Do NOT just list file names.

## Tools Overview

For detailed parameters, see [references/tool-reference.md](references/tool-reference.md).
### Workspace & onboarding
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(GetWorkspaceFromPath)` | Check if a path is under UVCS; returns workspace info if so |
| `Unity.UVCS.Execute(GetWorkspaceConfiguration)` | Get what the workspace is currently pointing to (branch, changeset, label, or shelve) |
| `Unity.UVCS.Execute(GetKnownServers)` | List accessible UVCS servers/organizations |
| `Unity.UVCS.Execute(GetAllRepositories)` | List repositories on a server |
| `Unity.UVCS.Execute(GetAllProjects)` | List Unity Cloud projects (`@unity` only) |
| `Unity.UVCS.Execute(CreateRepository)` | Create a new repository |
| `Unity.UVCS.Execute(CreateWorkspace)` | Create a local workspace mapped to a repository |
| `Unity.UVCS.Execute(PerformInitialCheckin)` | Add all project files to version control |
| `Unity.UVCS.Execute(RefreshUIAfterCreateWorkspace)` | Refresh editor UI after workspace creation |

### Query
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(FindQuery)` | Search any UVCS object (branches, changesets, labels, revisions, etc.) using the query language |

### Branches & changesets
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(GetBranchInfo)` | Get information about a specific branch by name |
| `Unity.UVCS.Execute(CreateBranch)` | Create a new child branch in the workspace, optionally with a short comment |
| `Unity.UVCS.Execute(RenameBranch)` | Rename an existing branch |
| `Unity.UVCS.Execute(LaunchSwitchToBranchUI)` | Switch the workspace to a given branch via the editor UI |
| `Unity.UVCS.Execute(GetChangesetInfo)` | Get detailed information about a specific changeset by its numeric ID |

### File history & annotate
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(GetFileHistory)` | Get the full revision history of a file (all revisions, moves, removals over time) |
| `Unity.UVCS.Execute(GetFileAnnotations)` | Get per-line blame data for a file (slow — see Performance Note) |
| `Unity.UVCS.Execute(GetRevisionInfo)` | Get detailed metadata about a specific file revision by its revision ID |

### Diff & content
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(GetBranchDifferences)` | Changed file metadata for a branch vs its parent (no file download) |
| `Unity.UVCS.Execute(GetChangesetDifferences)` | Changed file metadata for a changeset vs its parent |
| `Unity.UVCS.Execute(GetChangesetsDifferences)` | Changed file metadata between two changesets |
| `Unity.UVCS.Execute(GetLabelDifferences)` | Changed file metadata for the changeset a label points to |
| `Unity.UVCS.Execute(GetShelveDifferences)` | Changed file metadata for a shelve |
| `Unity.UVCS.Execute(GetPendingChanges)` | Locally pending (uncommitted) changes metadata |
| `Unity.UVCS.Execute(DownloadRevisionsToFiles)` | Download file revisions to temp files in a single batch call |
| `Unity.UVCS.Execute(CleanupDiffTempFiles)` | Delete all temp diff files. MUST be called after every diff comparison |

### Checkin & shelve
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(CheckinPendingChanges)` | Checkin all pending changes with a comment; returns the new changeset number |
| `Unity.UVCS.Execute(ShelvePendingChanges)` | Shelve all pending changes with a comment; returns the new shelve changeset number |

### Undo & revert
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(UndoChanges)` | **DESTRUCTIVE** — Undo (discard) pending changes for specific files or the entire workspace. Data loss is permanent. |
| `Unity.UVCS.Execute(UndoUnchanged)` | Remove unchanged files from the pending changes list (safe, no data loss) |
| `Unity.UVCS.Execute(RevertToRevision)` | Revert a file to the content of a specific revision |

### Locks
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(ListLocks)` | List all locks in the repository (who locked what, status, branch) |
| `Unity.UVCS.Execute(GetLockRule)` | Get the lock rules configured for the repository |
| `Unity.UVCS.Execute(ReleaseLocks)` | Release (unlock) specific locks by their item IDs |
| `Unity.UVCS.Execute(RemoveLocks)` | Force-remove specific locks by their item IDs (even if held by others) |

### Labels
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(CreateLabel)` | Create a new label on a changeset (defaults to the current workspace changeset) |
| `Unity.UVCS.Execute(ApplyLabel)` | Apply an existing label to the workspace (switch to the labeled changeset) |
| `Unity.UVCS.Execute(RenameLabel)` | Rename an existing label |
| `Unity.UVCS.Execute(DeleteLabel)` | Delete a label from the repository (irreversible) |

### Code review
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(CreateCodeReviewForBranch)` | Create a new UVCS code review for a branch; returns `ReviewInfo` |
| `Unity.UVCS.Execute(CreateCodeReviewForChangeset)` | Create a new UVCS code review for a changeset; returns `ReviewInfo` |
| `Unity.UVCS.Execute(AddCurrentUserAsReviewer)` | Add the current user as a reviewer on a review |
| `Unity.UVCS.Execute(CreateCodeComment)` | Add an inline code comment to a review on a specific file and line |
| `Unity.UVCS.Execute(CreateConversationComment)` | Add a general conversation comment to a review |
| `Unity.UVCS.Execute(SetReviewStatus)` | Set the current user's reviewer status (Reviewed / ReworkRequired) |
| `Unity.UVCS.Execute(GetCodeReviewLink)` | Get the `plastic://` deep link to open a review in the UVCS client |
| `Unity.UVCS.Execute(GetReviewsInStatus)` | Get code reviews matching a given status for a specific target (branch ID or changeset ID) |
| `Unity.UVCS.Execute(GetReviewComments)` | Get all comments for a code review (changes, questions, replies) |
| `Unity.UVCS.Execute(ReplyComment)` | Reply to a code review comment (answer a question or discard a change) |
| `Unity.UVCS.Execute(ReRequestReview)` | Re-request review from all reviewers after addressing their feedback |

### User interaction
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(SelectFromList)` | Show a dropdown selection UI |
| `Unity.UVCS.Execute(SelectOption)` | Show radio button options with descriptions |
| `Unity.UVCS.Execute(AskForConfirmation)` | Show a confirmation dialog before an action |

### UI navigation
| Tool | Purpose |
|------|---------|
| `Unity.UVCS.Execute(RefreshView)` | Refresh a UVCS view to show the latest data |
| `Unity.UVCS.Execute(ShowView)` | Open the UVCS window and switch to a specific view tab |
| `Unity.UVCS.Execute(ShowBranchExplorer)` | Open the Branch Explorer window |
| `Unity.UVCS.Execute(ShowFileHistory)` | Open the History view for a specific file in the UVCS window |
| `Unity.UVCS.Execute(ShowBranchesView)` | Open the Branches view, optionally selecting a specific branch |
| `Unity.UVCS.Execute(ShowChangesetsView)` | Open the Changesets view, optionally selecting a specific changeset |
| `Unity.UVCS.Execute(ShowShelvesView)` | Open the Shelves view, optionally selecting a specific shelve |
| `Unity.UVCS.Execute(LaunchMergeFromBranchUI)` | Open the merge view to merge from a branch |
| `Unity.UVCS.Execute(LaunchMergeFromChangesetUI)` | Open the merge view to merge a specific changeset |
| `Unity.UVCS.Execute(LaunchApplyShelveUI)` | Open the merge view to apply a shelve in the workspace |

## Workflows

Workflows are predefined step-by-step guides for common UVCS operations. When a user's request matches a workflow's triggers, follow that workflow's prescribed sequence of tool calls and rules. If no workflow matches, compose your own sequence using the available tools directly. Workflows ensure well-known operations are handled correctly and consistently — they are guardrails, not limits.
### Check If Project Is Under UVCS
- **Triggers:** User wants to check if the project has an active UVCS workspace.
- **Examples:** "Is my project under UVCS?", "Do I have version control set up?", "Check my source control status"
- **Not this workflow:** if the user wants to *set up* version control → use "Add Project to Unity Version Control" instead.

If no workspace is found, offers to add the project to source control. Detailed guide: [references/onboarding-workflows.md](references/onboarding-workflows.md#workflow-1-check-if-project-is-under-uvcs).
### Add Project to Unity Version Control
- **Triggers:** User wants to onboard their project to UVCS for the first time.
- **Examples:** "Add my project to UVCS", "Set up version control", "Initialize version control"
- **Not this workflow:** if the user just wants to *check* whether version control is configured → use "Check If Project Is Under UVCS" instead.

Creates repository, workspace, and performs initial checkin. Supports `@unity` and `@cloud` organizations. Detailed guide: [references/onboarding-workflows.md](references/onboarding-workflows.md#workflow-2-add-project-to-source-control).
### Query UVCS Objects
- **Triggers:** User wants to search or list UVCS objects (branches, changesets, labels, shelves, revisions) by criteria such as author, date, or name.
- **Examples:** "Find branches created this week", "List changesets by user X", "Show me shelves from last week"
- **Not this workflow:** if the user wants to see *what files changed* in a specific object → use "Diff / Compare Changes" instead.

Uses `Unity.UVCS.Execute(FindQuery)` with the UVCS query language. **Never Git commands.** Detailed guide: [references/query-workflows.md](references/query-workflows.md).
### Switch to a Branch
- **Triggers:** User wants to switch the workspace to an existing branch.
- **Examples:** "Switch to branch task001", "Work on branch X", "Go to release-1.0"
- **Not this workflow:** if the user wants to *create* a new branch → use "Create and Switch to a Branch" instead.

Resolves partial branch names automatically (e.g. `task001` → `/main/task001`) and opens the switch dialog in the Unity editor. If no matching branch is found, offers to create it. Detailed guide: [references/branch-workflows.md](references/branch-workflows.md#workflow-1-switch-to-a-branch).
### Create and Switch to a Branch
- **Triggers:** User wants to create a new branch and switch to it.
- **Examples:** "Create a branch", "Start a new branch for this task", "Make a branch called fix-enemy-ai"
- **Not this workflow:** if the branch already exists and user just wants to switch → use "Switch to a Branch" instead.

Creates a new child branch as a child of an existing branch (defaults to the current workspace branch). Handles parent branch selection, optional changeset pinning, and branch naming validation. Detailed guide: [references/branch-workflows.md](references/branch-workflows.md#workflow-2-create-and-switch-to-a-branch).
### Rename a Branch
- **Triggers:** User wants to rename an existing branch.
- **Examples:** "Rename branch task001 to task002", "Change the name of branch X"

Resolves partial branch names and validates the new name (no slashes allowed in branch leaf names). Detailed guide: [references/branch-workflows.md](references/branch-workflows.md#workflow-3-rename-a-branch).
### Diff / Compare Changes
- **Triggers:** User wants to see what changed in a branch, changeset, label, shelve, or pending workspace changes — or compare two changesets.
- **Examples:** "What changed in branch task001?", "Compare changeset 100 to 200", "Show me my pending changes"
- **Not this workflow:** if the user wants to search/list UVCS objects by criteria → use "Query UVCS Objects". If the user wants a code quality review → use "Code Review".

Shows what changed in one of the following objects, or between two of them:

| Object | Tool | What it compares |
|--------|------|------------------|
| **Branch** | `Unity.UVCS.Execute(GetBranchDifferences)` | Branch vs its parent branch |
| **Changeset** | `Unity.UVCS.Execute(GetChangesetDifferences)` | Changeset vs its parent changeset |
| **Two changesets** | `Unity.UVCS.Execute(GetChangesetsDifferences)` | Older (source) vs newer (destination) changeset |
| **Label** | `Unity.UVCS.Execute(GetLabelDifferences)` | Labeled changeset vs its parent changeset |
| **Shelve** | `Unity.UVCS.Execute(GetShelveDifferences)` | Shelved changes content |
| **Pending changes** | `Unity.UVCS.Execute(GetPendingChanges)` | Current uncommitted workspace changes |

Any question about what changed "in my project" or "in my workspace" without specifying a branch, changeset, label, or shelve refers to **pending changes**.

Returns either a file list (metadata only) or a full content-level explanation depending on the user's phrasing — see the Diff Behavior rule in Critical Rules. Detailed guide: [references/diff-workflows.md](references/diff-workflows.md).
### Checkin Pending Changes
- **Triggers:** User wants to commit pending changes to the UVCS repository.
- **Examples:** "Checkin my changes", "Commit my changes", "Push my changes"
- **Not this workflow:** if the user wants to temporarily save changes without committing → use "Shelve Pending Changes" instead.

Shows pending changes, asks for a comment (if not provided), confirms, and checks in. Returns the new changeset number on success. Detailed guide: [references/checkin-shelve-workflows.md](references/checkin-shelve-workflows.md#workflow-1-checkin-pending-changes).
### Shelve Pending Changes
- **Triggers:** User wants to temporarily save pending changes as a shelve on the server without committing them.
- **Examples:** "Shelve my changes", "Stash my changes", "Save my changes as a shelve"
- **Not this workflow:** if the user wants to permanently commit changes → use "Checkin Pending Changes" instead.

The user can choose to keep or undo local changes after shelving. A shelve comment is auto-generated from the changes if not provided. Detailed guide: [references/checkin-shelve-workflows.md](references/checkin-shelve-workflows.md#workflow-2-shelve-pending-changes).
### Show a UVCS View
- **Triggers:** User wants to open or navigate to a specific view tab in the UVCS window (branches, changesets, pending changes, incoming changes, shelves, labels, locks).
- **Examples:** "Show me the branches view", "Open pending changes", "Open the shelves view"
- **Not this workflow:** if the user wants the visual branch graph → use "Show Branch Explorer". If the user wants file history data → use "File History". Detailed guide: [references/ui-navigation-workflows.md](references/ui-navigation-workflows.md#workflow-1-show-a-uvcs-view).
### Lock Management
- **Triggers:** User wants to view, release, or force-remove file locks, or see configured lock rules.
- **Examples:** "Show me all locks", "Release my locks", "What are the lock rules?"

Lists locks, releases current user's locks, or force-removes locks held by others. Also shows configured lock rules. Detailed guide: [references/lock-workflows.md](references/lock-workflows.md).
### Label Management
- **Triggers:** User wants to create, apply, rename, or delete a label on a changeset.
- **Examples:** "Create label v1.0", "Apply label to my workspace", "Delete label"

Manages labels (named snapshots of changesets). Deletion requires explicit confirmation (irreversible). Detailed guide: [references/label-workflows.md](references/label-workflows.md).
### Undo Changes (Discard)
- **Triggers:** User wants to permanently discard pending changes for specific files or the entire workspace.
- **Examples:** "Discard my changes", "Undo all pending changes", "Reset my workspace"
- **Not this workflow:** if the user wants to clean up unchanged checkouts (safe, no data loss) → use "Undo Unchanged". If the user wants to restore a file to a specific previous revision → use "Revert File to Revision".

**DESTRUCTIVE.** All local modifications are lost and cannot be recovered. Always shows the affected files, suggests shelving first, and requires explicit confirmation before proceeding. Detailed guide: [references/undo-revert-workflows.md](references/undo-revert-workflows.md#workflow-1-undo-changes-discard).
### Undo Unchanged
- **Triggers:** User wants to remove unmodified files from the pending changes list (safe, no data loss).
- **Examples:** "Clean up unchanged checkouts", "Remove files I didn't actually change"
- **Not this workflow:** if the user wants to discard actual modifications → use "Undo Changes (Discard)" instead. Detailed guide: [references/undo-revert-workflows.md](references/undo-revert-workflows.md#workflow-2-undo-unchanged).
### Revert File to Revision
- **Triggers:** User wants to restore a specific file to the content of a previous revision.
- **Examples:** "Revert Player.cs to revision 42", "Restore file to a previous version"
- **Not this workflow:** if the user wants to discard all pending changes → use "Undo Changes (Discard)" instead.

The file will appear as a pending change after reverting. If no revision is specified, shows available revisions and lets the user choose. Detailed guide: [references/undo-revert-workflows.md](references/undo-revert-workflows.md#workflow-3-revert-file-to-revision).
### Show Branch Explorer
- **Triggers:** User wants to see the visual branch graph/tree.
- **Examples:** "Open branch explorer", "Show me the branch graph"
- **Not this workflow:** if the user wants a list of branches → use "Show a UVCS View" with the branches tab, or "Query UVCS Objects".

Opens the Branch Explorer visual graph in the Unity editor. Detailed guide: [references/ui-navigation-workflows.md](references/ui-navigation-workflows.md#workflow-2-show-branch-explorer).
### Show File History in UI
- **Triggers:** User wants to open the visual History view for a file in the UVCS window.
- **Examples:** "Open the history view for Player.cs", "Show revisions in the UVCS window"
- **Not this workflow:** if the user wants file history *data* (not the visual UI) → use "File History" instead. Detailed guide: [references/ui-navigation-workflows.md](references/ui-navigation-workflows.md#workflow-3-show-file-history-in-ui).
### Merge from a Branch
- **Triggers:** User wants to merge changes from another branch into the current workspace.
- **Examples:** "Merge branch task001", "Pull changes from branch X"
- **Not this workflow:** if the user wants to merge a single changeset → use "Merge from a Changeset". If the user wants to switch branches → use "Switch to a Branch".

Opens the merge view in the Unity editor. Detailed guide: [references/merge-workflows.md](references/merge-workflows.md#workflow-1-merge-from-a-branch).
### Merge from a Changeset
- **Triggers:** User wants to cherry-pick a specific changeset into the current workspace.
- **Examples:** "Merge changeset 42", "Cherry-pick changeset 55"
- **Not this workflow:** if the user wants to merge an entire branch → use "Merge from a Branch" instead.

Opens the merge view in the Unity editor. Detailed guide: [references/merge-workflows.md](references/merge-workflows.md#workflow-2-merge-from-a-changeset).
### Apply a Shelve
- **Triggers:** User wants to apply shelved changes to the current workspace.
- **Examples:** "Apply shelve 100", "Unshelve changes", "Restore my shelved changes"

Opens the merge view to apply a shelve in the workspace. Detailed guide: [references/merge-workflows.md](references/merge-workflows.md#workflow-3-apply-a-shelve).
### Ignore Private Files
- **Triggers:** User wants to exclude private/local files from UVCS tracking via `ignore.conf` rules (like `.gitignore`).
- **Examples:** "Ignore this file", "Add ignore rule", "I have private files showing up in pending changes"

Configures `ignore.conf` rules so unwanted files stop appearing in pending changes. Detailed guide: [references/ignore-workflows.md](references/ignore-workflows.md).
### Code Review
- **Triggers:** User wants to review code changes for defects and quality issues (acting as the reviewer).
- **Examples:** "Review my changes", "Review branch X", "Can you check my code?"
- **Not this workflow:** if the user wants to *fix/address* existing review feedback (acting as the author) → use "Address Code Review Comments" instead.

Reviews **only changed/new code**, classifies findings by severity (BLOCKER, IMPORTANT, SUGGESTION, NIT). For branches/changesets, offers to publish as UVCS code review comments. Detailed guide: [references/code-review-create.md](references/code-review-create.md).
### Address Code Review Comments
- **Triggers:** User asks you to fix, address, or apply feedback from an existing code review as the code author, or asks for help responding to review comments.
- **Examples:** "Fix the review comments", "Address the review feedback", "Apply the review changes"
- **Not this workflow:** if the user wants to *create* a new review (be the reviewer) → use "Code Review" instead.

Helps the author address UVCS code review feedback: apply changes and answer questions. Two modes: Assisted (user drives) or Automated (LLM autonomous).

**CRITICAL:** You MUST follow the detailed guide EXACTLY, step by step. Do NOT improvise your own approach. Detailed guide: [references/code-review-address.md](references/code-review-address.md).
### File History
- **Triggers:** User wants to see the complete revision history of a file (data, not the visual UI).
- **Examples:** "Show me the history of this file", "What changes were made to Player.cs?", "How has this file evolved?"
- **Not this workflow:** if the user wants the visual history tab in the UVCS window → use "Show File History in UI". If the user wants per-line blame → use "Annotate / Blame a File".

Retrieves complete file revision history (revisions, moves, deletions) in reverse chronological order. Detailed guide: [references/file-history-workflows.md](references/file-history-workflows.md).
### Annotate / Blame a File
- **Triggers:** User wants per-line authorship/blame data for an entire file.
- **Examples:** "Annotate this file", "Blame this file", "Who modified line 42?"
- **Not this workflow:** if the user wants file-level history (not per-line) → use "File History" or "Query UVCS Objects".

**Performance note:** `GetFileAnnotations` is slow — only use when you need per-line metadata. Detailed guide: [references/annotate-workflows.md](references/annotate-workflows.md#workflow-1-basic-file-annotate).
### Who Made a Specific Change / Why Was Something Changed
- **Triggers:** User wants to know who changed a specific piece of code or why a value is different from expected.
- **Examples:** "Who changed this method?", "Why is this value 5 instead of 10?", "Why does this cube have a different material?"
- **Not this workflow:** if the user wants full-file blame → use "Annotate / Blame a File". If the user wants to trace a bug to its source → use "Root Cause Analysis".

Traces who made a specific change and why, using annotations and changeset comments. Detailed guide: [references/annotate-workflows.md](references/annotate-workflows.md#workflow-2-who-made-a-specific-change--why-was-something-changed).
### In Which Branch Was Something Added
- **Triggers:** User wants to find which branch introduced a specific piece of code or asset.
- **Examples:** "Which branch added the enemy spawner?", "Where did this code come from?", "What branch introduced this component?"

Uses file annotations to identify the origin branch. Detailed guide: [references/annotate-workflows.md](references/annotate-workflows.md#workflow-3-find-the-branch-where-something-was-added).
### When Was a File Last Modified
- **Triggers:** User wants to know when a file was last changed, by whom, and on which branch.
- **Examples:** "When was this file last modified?", "Is this file recent?", "How old is this code?"
- **Not this workflow:** if the user wants the full revision history → use "File History".

Reports the date, author, and branch of the most recent change. Detailed guide: [references/annotate-workflows.md](references/annotate-workflows.md#workflow-4-when-was-a-file-last-modified).
### Find Most Active Contributors for a File
- **Triggers:** User wants to see who contributes the most to a file and their line ownership percentages.
- **Examples:** "Who works on this file the most?", "Who owns this file?", "Who should I ask about this script?"

Shows top contributors with line counts and percentages. Helps identify who to consult. Detailed guide: [references/annotate-workflows.md](references/annotate-workflows.md#workflow-5-find-the-most-active-contributors-for-a-file).
### Annotate Previous Version of a File
- **Triggers:** User wants to see the annotated state of a file one revision back from the latest change.
- **Examples:** "Show me the previous version of this file", "What did this file look like before the latest change?"
- **Not this workflow:** if the user wants the state before a *specific* changeset → use "Annotate Before a Specific Change" instead.

Uses `FindQuery` to get the parent revision ID efficiently (does not annotate the current version first). Detailed guide: [references/annotate-workflows.md](references/annotate-workflows.md#workflow-8-annotate-previous-version-of-a-file).
### Annotate Before a Specific Change
- **Triggers:** User wants to see a file's state just before a specific changeset modified it.
- **Examples:** "What was the value before changeset 14?", "What color was this cube before it was changed to red?"
- **Not this workflow:** if the user just wants one revision back (no specific changeset) → use "Annotate Previous Version of a File" instead.

Finds the last revision before the target changeset and annotates it. Detailed guide: [references/annotate-workflows.md](references/annotate-workflows.md#workflow-9-annotate-before-a-specific-change).
### Root Cause Analysis
- **Triggers:** User wants to trace a bug, regression, or runtime error back to the changeset that caused it.
- **Examples:** "What changeset broke this?", "Find the root cause", "Check console errors and find what caused them"
- **Not this workflow:** if the user just wants to know who changed a specific line → use "Who Made a Specific Change". If the user just wants to see what changed → use "Diff / Compare Changes".

Combines VCS history, dependency graph, and console logs. Two entry points: user-described symptoms or console errors.

**CRITICAL:** You MUST follow the detailed guide EXACTLY, step by step. Do NOT improvise your own approach. Detailed guide: [references/root-cause-analysis.md](references/root-cause-analysis.md).

## Reminders

Before completing any response, verify:

- Did you use UVCS tools (NOT Git commands) for all version control queries?
- If the project has no UVCS workspace and the user is not onboarding to UVCS, did you stay out of the way — no UVCS tools, no UVCS promotion — and handle the request as the base assistant would?
- Did you call the relevant UVCS tool before answering — not guessing state?
- Did you read the tool documentation before using it?
- If you downloaded temp files, did you call `CleanupDiffTempFiles`?
- If you presented options, did you use `SelectFromList` or `SelectOption` instead of a numbered text list?
- After mutating operations (checkin, shelve, branch creation), the relevant UVCS views are auto-refreshed. However, if you modify files outside UVCS tools (e.g. via Run Command or file editing tools), call `Unity.UVCS.Execute(RefreshView)` with `PendingChanges` so the user sees their changes reflected in the Pending Changes view.
- Did you wrap every changeset ID, branch name, shelve ID, and label name in a clickable `uvcs://` link using markdown syntax `[text](url)`? Every entity mention should be a link. Check that NO bare `uvcs://` URL appears as visible text — URLs must ONLY be inside markdown link parentheses.
- For changesets and shelves, did you use `changesetId`/`ChangesetId` rather than internal `ID`/`Id` when displaying and linking? For shelves, did the visible label use the positive absolute value while the link/tool parameter kept the signed negative value?
- Did you use action links (merge, open) when suggesting actions the user can take?
- Before creating any `uvcs://` link, did you read [references/clickable-links.md](references/clickable-links.md) and use only the action/type patterns defined there?
