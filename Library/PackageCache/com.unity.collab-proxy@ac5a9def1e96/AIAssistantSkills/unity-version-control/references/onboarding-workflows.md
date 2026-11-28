# Onboarding Workflows

Workflows for checking UVCS status and adding a Unity project to source control.

For data model details (identifier formats, `@unity` project selection rules),
see [data-model.md](data-model.md).

## Table of Contents

- [Workflow 1: Check If Project Is Under UVCS](#workflow-1-check-if-project-is-under-uvcs)
- [Workflow 2: Add Project to Source Control](#workflow-2-add-project-to-source-control)
  - [Path A: @unity Organization](#path-a-unity-organization)
  - [Path B: @cloud Organization](#path-b-cloud-organization)
  - [4-Step Onboarding Sequence](#4-step-onboarding-sequence)

---

## Workflow 1: Check If Project Is Under UVCS

The user may ask variations such as:
- "Is my project under Unity Version Control?"
- "Is my project under UVCS?"
- "Am I working with Unity Version Control?"
- "Am I working with UVCS?"
- "Is this project under source control?"

**Steps:**

1. Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` with `Application.dataPath` as the
   path.
2. If a workspace is returned, the project **is** under UVCS. Report the
   workspace name and path.
3. If no workspace is found, the project is **not** under UVCS. Inform the user
   and offer to add it (see Workflow 2).

---

## Workflow 2: Add Project to Source Control

When the user asks to onboard a project to UVCS.

For data model details (identifier formats, parameter splitting),
see [data-model.md](data-model.md).

### Decision Flow

| Step | Condition | → Action |
|------|-----------|----------|
| 1 | Always | → `Unity.UVCS.Execute(GetWorkspaceFromPath)` |
| 2 | Workspace exists | → **STOP** (already under UVCS) |
| 3 | No workspace | → `Unity.UVCS.Execute(GetKnownServers)` |
| 4 | Multiple servers | → `Unity.UVCS.Execute(SelectFromList)` (server list) |
| 4 | Single server | → auto-select (no tool call) |
| 5a | Server ends with `@unity` | → `Unity.UVCS.Execute(GetAllProjects)` |
| 5b | Project matches editor name | → propose it, **WAIT** for user confirmation |
| 5c | "Cloud Repositories" exists | → propose it, **WAIT** for user confirmation |
| 5d | No match at all | → `Unity.UVCS.Execute(SelectFromList)` (project list) |
| 6 | Server ends with `@cloud` | → `Unity.UVCS.Execute(GetAllRepositories)` (skip projects) |
| 7 | Project/server chosen | → `Unity.UVCS.Execute(GetAllRepositories)` |
| 8 | Ready to create | → `Unity.UVCS.Execute(AskForConfirmation)` |
| 9 | User confirms | → 4-step onboarding sequence |

### Common Steps

1. **Check if already under source control**: Call
   `Unity.UVCS.Execute(GetWorkspaceFromPath)` with `Application.dataPath` as the path.
   If a workspace is returned, the project is **already** under UVCS. Inform
   the user (include the workspace name and path) and **stop** — do not
   continue with the remaining steps.
2. **Discover servers**: Call `Unity.UVCS.Execute(GetKnownServers)`.
3. **Select server**:
   - If only one server is returned, auto-select it.
   - If the user already specified a target, use that server.
   - If multiple servers → **call `Unity.UVCS.Execute(SelectFromList)`** with the server
     list to let the user pick from a dropdown.
4. Follow **Path A** or **Path B** based on the server suffix.

### Path A: `@unity` Organization

Follow this path ONLY if the selected server ends with `@unity`.

1. → **Call `Unity.UVCS.Execute(GetAllProjects)(server)`** immediately. **Display ALL
   returned projects** — NEVER truncate or show only the first 10.
2. **Auto-select a default project (MANDATORY).** Scan the FULL project list and
   apply these rules IN ORDER:
   - **First priority — exact match:** If a project name **exactly matches** the
     current Unity editor project name → propose it as the default **and ask
     the user to confirm** (e.g. *"I found 'MyGame' which matches your Unity
     project. Shall I use it?"*). **Stop here and wait for the user's response
     before calling any further tools.**
   - **Second priority — Cloud Repositories:** If no exact match but a project
     named **"Cloud Repositories"** (case-insensitive) exists → propose it as
     the default **and ask the user to confirm** (e.g. *"I found
     'Cloud Repositories' which is the recommended default. Shall I use it?"*).
     **Stop here and wait for the user's response before calling any further
     tools.**
   - **Fallback — no match:** If neither exists → the immediate next action is
     **`Unity.UVCS.Execute(SelectFromList)`** with the complete project list. The user
     will choose from a dropdown in the Unity editor. **Stop here and wait for
     the user's choice.** Do NOT offer "create a new project" as an alternative
     at this point — that is handled in the next step only if the user rejects
     all listed projects.
3. If no suitable project exists and the user does not want any of the listed
   projects, instruct the user to create a new project manually in the Unity
   Dashboard, then STOP and WAIT. The URL format is:
   `https://cloud.unity.com/home/organizations/<orgId>/projects` — replace
   `<orgId>` with the actual organization ID from the server address.
4. Do not continue until an existing Unity Cloud project is explicitly chosen.
5. After the project is chosen → **call `Unity.UVCS.Execute(GetAllRepositories)(server)`**.
6. Build arguments:
   - `repositoryName` = `chosenProjectName/baseRepositoryName`
   - `server` = `orgName@unity`
   - Full repository identifier:
     `chosenProjectName/baseRepositoryName@orgName@unity`
7. Verify that this repository does not already exist. If it exists, generate
   another candidate (for example add a numeric suffix) or ask the user.
8. Prepare arguments:
   - `workspaceName`: user-provided or project-based workspace name
   - `workspacePath`: local project root path
   - `repositoryName`: value built above
   - `server`: value from the selected server
9. → **Call `Unity.UVCS.Execute(AskForConfirmation)`** with a summary of ALL details. The
   `action` should be `"Add project to source control"` and the `detail` MUST
   include every field:
   - Server/Organization
   - Unity Cloud Project
   - New Repository name
   - Local Workspace name
   - Workspace Path (the full local path where the workspace will be created)

   Only proceed with the **4-step onboarding** (see below) if the user confirms.

**IMPORTANT:**
- NEVER use the local Unity project name as `chosenProjectName` unless it was
  actually returned by `Unity.UVCS.Execute(GetAllProjects)`.
- NEVER propose to create a repository for an `@unity` server before a valid
  existing Unity Cloud project is confirmed.

### Path B: `@cloud` Organization

Follow this path ONLY if the selected server ends with `@cloud`.

1. → **Call `Unity.UVCS.Execute(GetAllRepositories)(server)`**.
2. Build arguments:
   - `repositoryName` = `baseRepositoryName`
   - `server` = `orgName@cloud`
   - Full repository identifier: `baseRepositoryName@orgName@cloud`
3. Verify that this repository does not already exist. If it exists, generate
   another candidate (for example add a numeric suffix) or ask the user.
4. Prepare arguments:
   - `workspaceName`: user-provided or project-based workspace name
   - `workspacePath`: local project root path
   - `repositoryName`: value from above
   - `server`: value from above
5. → **Call `Unity.UVCS.Execute(AskForConfirmation)`** with a summary of ALL details. The
   `action` should be `"Add project to source control"` and the `detail` MUST
   include every field:
   - Server/Organization
   - New Repository
   - Local Workspace name
   - Workspace Path (the full local path where the workspace will be created)

   Only proceed with the **4-step onboarding** (see below) if the user confirms.

### 4-Step Onboarding Sequence

These four steps MUST be executed in this exact order. Each step depends on the
previous one succeeding. If any step fails, report the error and do NOT continue.

1. **Create the repository**: Call `Unity.UVCS.Execute(CreateRepository)` with
   `repositoryName` and `server`.
2. **Create the workspace**: Call `Unity.UVCS.Execute(CreateWorkspace)` with
   `workspaceName`, `workspacePath`, and `repositoryName`.
3. **Perform the initial checkin**: Call `Unity.UVCS.Execute(PerformInitialCheckin)`
   (no parameters — operates on the current workspace).
4. **Refresh the UI**: Call `Unity.UVCS.Execute(RefreshUIAfterCreateWorkspace)`
   (no parameters — operates on the current workspace).
