# Branch Workflows

Workflows for switching to and creating branches in UVCS.

For data model details, see [data-model.md](data-model.md).

## Table of Contents

- [Critical Rules](#critical-rules)
- [Workflow 1: Switch to a Branch](#workflow-1-switch-to-a-branch)
- [Workflow 2: Create and Switch to a Branch](#workflow-2-create-and-switch-to-a-branch)
- [Workflow 3: Rename a Branch](#workflow-3-rename-a-branch)

---

## Critical Rules

1. **No `/` in new branch names.** The `branchName` parameter of
   `Unity.UVCS.Execute(CreateBranch)` is the **leaf segment only**; UVCS automatically
   prepends the parent branch path. If the leaf contains `/`, UVCS treats
   each segment as a hierarchy level and the create fails when intermediate
   branches don't exist (e.g. `feature/new-ui` requires `/main/feature` to
   already exist). **Always use hyphens instead**: `feature-new-ui`,
   `fix-enemy-performance`, `task001`.

2. **NEVER switch branches without explicit user confirmation.** Use
   `Unity.UVCS.Execute(AskForConfirmation)` before `Unity.UVCS.Execute(LaunchSwitchToBranchUI)`.

3. **If the user names only a partial branch (e.g. `task001` instead of
   `/main/task001`), resolve it with `Unity.UVCS.Execute(FindQuery)`** using a `like`
   pattern: `find branch where name like '%<partial>'`. Do NOT guess the
   full path.

---

## Workflow 1: Switch to a Branch

The user may ask variations such as:
- "Switch to branch scm55432"
- "Go to branch release-1.0"
- "Switch to task001"

### Steps

1. **Get the workspace**: Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` with
   `Application.dataPath` to obtain the current workspace. If no workspace is
   found, inform the user that the project is not under UVCS and offer to
   onboard it (see
   [onboarding-workflows.md](onboarding-workflows.md#workflow-2-add-project-to-source-control)).

2. **Search for the branch**: The user may provide an incomplete branch name
   (e.g. `task001` instead of `/main/task001`). To handle this reliably, use
   `Unity.UVCS.Execute(FindQuery)` with a `like` pattern:
   ```
   find branch where name like '%<branchName>'
   ```
   For example, if the user says "switch to task001", run:
   ```
   find branch where name like '%task001'
   ```

3. **Evaluate the results**:
   - **Exactly one match** → Use that branch. Call
     `Unity.UVCS.Execute(LaunchSwitchToBranchUI)` with the workspace path and the full
     branch name from the result. Inform the user that the switch dialog is now
     open.
   - **Multiple matches** → Call `Unity.UVCS.Execute(SelectFromList)` with the matching
     branch names to let the user pick from a dropdown. Once chosen, call
     `Unity.UVCS.Execute(LaunchSwitchToBranchUI)`.
   - **No matches** → Inform the user that no branch matching the name was found
     and ask for confirmation before creating it (e.g. *"No branch matching
     'scm55432' was found. Would you like me to create it and switch to it?"*).
     If the user confirms, follow
     [Workflow 2: Create and Switch to a Branch](#workflow-2-create-and-switch-to-a-branch)
     starting from step 3 (Determine the parent branch).

### Example Conversation Flow

**User:** "Switch to branch task001"

1. Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` → workspace found.
2. Call `Unity.UVCS.Execute(FindQuery)("find branch where name like '%task001'")`.
   - **One match** (`/main/task001`) → Call
     `Unity.UVCS.Execute(LaunchSwitchToBranchUI)("/main/task001")` →
     switch dialog opens. Inform the user:
     *"Switching to [/main/task001](uvcs://show/branch/main/task001)."*
   - **Multiple matches** (e.g. `/main/task001`, `/main/dev/task001`) → Call
     `Unity.UVCS.Execute(SelectFromList)` to let the user pick one.
   - **No matches** → Ask: *"No branch matching 'task001' was found. Would you
     like me to create it and switch to it?"*
     - User confirms → proceed with creation (Workflow 2, from step 3).
     - User declines → done.

---

## Workflow 2: Create and Switch to a Branch

The user may ask variations such as:
- "Create a branch called task001"
- "I want to work on a new branch"
- "Create and switch to a branch"

### Steps

1. **Get the workspace**: Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` with
   `Application.dataPath` to obtain the current workspace. If no workspace is
   found, inform the user that the project is not under UVCS and offer to
   onboard it (see
   [onboarding-workflows.md](onboarding-workflows.md#workflow-2-add-project-to-source-control)).

2. **Determine the branch name**:
   - If the user already provided a branch name (e.g. "create branch task001"),
     use it directly. Do NOT ask again.
   - If no branch name was provided, ask the user for one before continuing.
   - **IMPORTANT — No slashes in branch names**: The `branchName` parameter is
     the **leaf segment only**. UVCS automatically prepends the parent branch
     path (e.g. `/main/`). If the branch name itself contains `/` (e.g.
     `feature/new-ui`), UVCS interprets each segment as a hierarchy level and
     will fail if intermediate branches don't exist (`/main/feature` must exist
     for `/main/feature/new-ui` to be created). **Always use hyphens instead of
     slashes**: `feature-new-ui`, `fix-enemy-performance`, `task001`.
     - Good: `feature-new-ui`, `bugfix-enemy-perf`, `enhance-manual-testing`
     - Bad: `feature/new-ui`, `bugfix/enemy-perf` (these require intermediate
       branches to exist)

3. **Determine the branch comment**:
   - If you can infer a meaningful purpose for the branch from the conversation
     context, generate a short, single-line comment describing what the branch is
     for. For example, if the user says "create a branch to fix the enemy
     performance bug", use a comment like `"Fix the enemy performance"`.
   - The comment must have **no newlines**, be **meaningful**, and be **short**.
     It should describe what the branch is or does.
   - If there is no context to derive a meaningful comment, pass `null`.

4. **Determine the parent branch**:
   - If the user specified **only a changeset number** (no parent branch name),
     use `Unity.UVCS.Execute(FindQuery)` to find which branch the changeset belongs to:
     ```
     find changeset where changesetid = <number>
     ```
     Use the `branch` field from the result as the parent branch.
   - If the user **did** specify a parent branch, search for it using
     `Unity.UVCS.Execute(FindQuery)` with a `like` pattern (the user may provide an
     incomplete name):
     ```
     find branch where name like '%<parentBranchName>'
     ```
     Evaluate the results:
     - **Exactly one match** → Use that branch as the parent.
     - **Multiple matches** → Call `Unity.UVCS.Execute(SelectFromList)` with the matching
       branch names to let the user pick the parent from a dropdown.
     - **No matches** → Inform the user that no branch matching the name was
       found and ask them to provide a different parent branch name.
   - If the user did **not** specify a parent branch (and no changeset), call
     `Unity.UVCS.Execute(GetWorkspaceConfiguration)` to check what the workspace is
     currently pointing to, then:
     - **Workspace is on `/main`** → Use `/main` as the parent automatically.
       Do NOT ask for confirmation about the parent; proceed directly to the
       next step.
     - **Workspace is on a child branch** (any branch that is not `/main`) →
       Call `Unity.UVCS.Execute(SelectOption)` with:
       - `title`: `"Select parent branch"`
       - `message`: `"Where should the new branch start from?"`
       - `options`:
         ```json
         [
           {
             "label": "/main",
             "description": "Start fresh, independent of your current work."
           },
           {
             "label": "<current branch name>",
             "description": "Continue building on your existing changes."
           }
         ]
         ```

       Wait for the user to pick an option before continuing.
     - **Workspace is on a changeset, label, or shelve** (not on a branch) →
       Use `/main` as the parent automatically, same as if the workspace were on
       `/main`.

5. **Determine the changeset**:
   - If the user did **not** specify a changeset number, pass `-1` (the default).
     This uses the latest changeset of the parent branch.
   - If the user **did** specify a changeset number, you **MUST** verify that it
     belongs to the parent branch before proceeding:
     ```
     find changeset where changesetid = <number> and branch = '<parentBranchFullName>'
     ```
     - If the query returns a result → the changeset is valid, use it.
     - If the query returns no results → inform the user that changeset
       `<number>` does not belong to branch `<parentBranchFullName>` and do
       **not** proceed. Ask the user to provide a valid changeset or remove the
       constraint.

6. **Ask for confirmation** before creating the branch. Call
   `Unity.UVCS.Execute(AskForConfirmation)` with:
   - `action`: `"Create branch"`
   - `detail`: A summary including the branch name, parent branch (or `/main`
     if defaulting), and changeset (if specified). For example:
     `"Branch: task001\nParent: /main"` or
     `"Branch: task001\nParent: /main\nChangeset: 42"`

   Only proceed if the user confirms.

7. **Create the branch**: Call `Unity.UVCS.Execute(CreateBranch)` with the workspace,
   branch name, comment (or `null`), parent branch (or `null`), and changeset
   (or `-1`). This returns the `BranchInfo` of the newly created branch.

8. **Switch to the branch**: Call `Unity.UVCS.Execute(LaunchSwitchToBranchUI)` with the
   workspace and the `BranchInfo` returned by the previous step. This opens the
   switch-to-branch dialog in the Unity editor so the user can confirm.

### Error Handling

- If `Unity.UVCS.Execute(CreateBranch)` fails (e.g. the branch already exists), report
  the error to the user and suggest alternatives (e.g. choose a different name,
  or switch to the existing branch using `Unity.UVCS.Execute(GetBranchInfo)` +
  `Unity.UVCS.Execute(LaunchSwitchToBranchUI)`).
- If `Unity.UVCS.Execute(LaunchSwitchToBranchUI)` fails, report the error and suggest
  the user tries switching manually from the UVCS window.

### Example Conversation Flow

**User:** "Create a branch called task001"

1. Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` → workspace found.
2. Branch name is `task001` (provided by the user).
3. No meaningful context to derive a comment → `comment = null`.
4. No parent branch specified → call `Unity.UVCS.Execute(GetWorkspaceConfiguration)`.
   - Workspace is on `/main` → use `/main` as parent (no need to ask).
5. No changeset specified → `changesetId = -1`.
6. Call `Unity.UVCS.Execute(AskForConfirmation)("Create branch", "Branch: task001\nParent: /main")`
   → User confirms.
7. Call `Unity.UVCS.Execute(CreateBranch)(workspace, "task001", null, null, -1)` → returns
   new `BranchInfo`.
8. Call `Unity.UVCS.Execute(LaunchSwitchToBranchUI)(workspace, branchInfo)` → switch
   dialog opens.
9. Inform the user: "Branch [/main/task001](uvcs://show/branch/main/task001)
   has been created from [/main](uvcs://show/branch/main) and the switch
   dialog is now open."

**User:** "Create a branch to fix the enemy performance bug"
*(workspace is currently on `/main/feature-sprint3`)*

1. Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` → workspace found.
2. Branch name is `fix-enemy-performance` (inferred from context).
3. Comment is `"Fix the enemy performance"` (inferred from context).
4. No parent branch specified → call `Unity.UVCS.Execute(GetWorkspaceConfiguration)` →
   workspace is on `/main/feature-sprint3` (a child branch, not `/main`).
   Call `Unity.UVCS.Execute(SelectOption)` with options:
   - `/main` — "Start fresh, independent of your current work."
   - `/main/feature-sprint3` — "Continue building on your existing changes."

   → User picks `/main/feature-sprint3`.
5. No changeset specified → `changesetId = -1`.
6. Call `Unity.UVCS.Execute(AskForConfirmation)("Create branch", "Branch: fix-enemy-performance\nParent: /main/feature-sprint3")`
   → User confirms.
7. Call `Unity.UVCS.Execute(CreateBranch)(workspace, "fix-enemy-performance", "Fix the enemy performance", parentBranchInfo, -1)`
   → returns new `BranchInfo`.
8. Call `Unity.UVCS.Execute(LaunchSwitchToBranchUI)(workspace, branchInfo)` → switch
   dialog opens.
9. Inform the user: "Branch
   [/main/feature-sprint3/fix-enemy-performance](uvcs://show/branch/main/feature-sprint3/fix-enemy-performance)
   has been created from
   [/main/feature-sprint3](uvcs://show/branch/main/feature-sprint3) and the
   switch dialog is now open."

**User:** "Create a branch called hotfix from /main/release-1.0"

1. Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` → workspace found.
2. Branch name is `hotfix`.
3. No meaningful context to derive a comment → `comment = null`.
4. Parent branch is `/main/release-1.0` → call
   `Unity.UVCS.Execute(FindQuery)("find branch where name like '%release-1.0'")` →
   one match (`/main/release-1.0`).
5. No changeset specified → `changesetId = -1`.
6. Call `Unity.UVCS.Execute(AskForConfirmation)("Create branch", "Branch: hotfix\nParent: /main/release-1.0")`
   → User confirms.
7. Call `Unity.UVCS.Execute(CreateBranch)(workspace, "hotfix", null, parentBranchInfo, -1)`
   → returns new `BranchInfo`.
8. Call `Unity.UVCS.Execute(LaunchSwitchToBranchUI)(workspace, branchInfo)` → switch
   dialog opens.
9. Inform the user: "Branch
   [/main/release-1.0/hotfix](uvcs://show/branch/main/release-1.0/hotfix) has
   been created from
   [/main/release-1.0](uvcs://show/branch/main/release-1.0) and the switch
   dialog is now open."

**User:** "Create a branch called hotfix from changeset 42"

1. Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` → workspace found.
2. Branch name is `hotfix`.
3. No meaningful context to derive a comment → `comment = null`.
4. No parent branch name provided, but changeset `42` specified → call
   `Unity.UVCS.Execute(FindQuery)("find changeset where changesetid = 42")`
   → result shows `branch = '/main/release-1.0'`. Use `/main/release-1.0` as
   the parent branch.
5. Changeset is `42`. Verify it belongs to the resolved parent branch → already
   confirmed from step 4.
6. Call `Unity.UVCS.Execute(AskForConfirmation)("Create branch", "Branch: hotfix\nParent: /main/release-1.0\nChangeset: 42")`
   → User confirms.
7. Call `Unity.UVCS.Execute(CreateBranch)(workspace, "hotfix", null, parentBranchInfo, 42)`
   → returns new `BranchInfo`.
8. Call `Unity.UVCS.Execute(LaunchSwitchToBranchUI)(workspace, branchInfo)` → switch
   dialog opens.
9. Inform the user: "Branch
   [/main/release-1.0/hotfix](uvcs://show/branch/main/release-1.0/hotfix) has
   been created from
   [/main/release-1.0](uvcs://show/branch/main/release-1.0) at
   [changeset 42](uvcs://show/changeset/42) and the switch dialog is now
   open."

---

## Workflow 3: Rename a Branch

The user may ask variations such as:
- "Rename branch task001 to task002"
- "Rename branch /main/task001 to fix-performance"
- "Change the name of branch X"

### Steps

1. **Get the workspace**: Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` with
   `Application.dataPath` to obtain the current workspace. If no workspace is
   found, inform the user that the project is not under UVCS.

2. **Resolve the branch**: The user may provide an incomplete branch name
   (e.g. `task001` instead of `/main/task001`). Use `Unity.UVCS.Execute(FindQuery)`
   with a `like` pattern:
   ```
   find branch where name like '%<branchName>'
   ```
   - **Exactly one match** → Use that branch.
   - **Multiple matches** → Call `Unity.UVCS.Execute(SelectFromList)` with the matching
     branch names to let the user pick from a dropdown.
   - **No matches** → Inform the user that no branch matching the name was
     found and stop.

3. **Determine the new name**: If the user already provided the new name, use
   it. Otherwise, ask for one.
   - **IMPORTANT — No slashes in the new branch name**: The new name is the
     **leaf segment only**. Always use hyphens instead of slashes.

4. **Ask for confirmation**: Call `Unity.UVCS.Execute(AskForConfirmation)` with:
   - `action`: `"Rename branch"`
   - `detail`: A summary including the current full branch name and the new
     name. For example:
     `"Branch: /main/task001\nNew name: task002"`

5. **Rename the branch**: Call `Unity.UVCS.Execute(RenameBranch)` with the workspace
   path, the full branch name, and the new name.

6. **Confirm success**: Inform the user that the branch was renamed.

### Example Conversation Flow

**User:** "Rename branch task001 to fix-performance"

1. Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` → workspace found.
2. Call `Unity.UVCS.Execute(FindQuery)("find branch where name like '%task001'")`.
   - One match (`/main/task001`) → use it.
3. New name is `fix-performance` (provided by the user).
4. Call `Unity.UVCS.Execute(AskForConfirmation)("Rename branch", "Branch: /main/task001\nNew name: fix-performance")`
   → User confirms.
5. Call `Unity.UVCS.Execute(RenameBranch)("/main/task001", "fix-performance")`.
6. Inform the user: "Branch `/main/task001` has been renamed to
   [/main/fix-performance](uvcs://show/branch/main/fix-performance)."

