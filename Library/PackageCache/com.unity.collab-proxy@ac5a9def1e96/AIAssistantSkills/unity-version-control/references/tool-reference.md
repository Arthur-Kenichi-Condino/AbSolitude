# UVCS Tool Reference

Detailed parameter documentation for each UVCS operation.

For identifier formats and parameter splitting examples, see
[data-model.md](data-model.md).

## How operations are called

All UVCS operations run through one agent tool, `Unity.UVCS.Execute`, with two parameters:

- `operation` (string) — one of the operation names listed below (e.g. `FindQuery`).
- `parametersJson` (string) — a JSON object holding that operation's arguments. Omit it for operations that take none.

In this document a heading like `## Unity.UVCS.Execute(GetBranchInfo)` documents the `GetBranchInfo`
operation; the fields under "Parameter(s)" are the keys you put inside `parametersJson`. Enum-valued
fields are passed as their string name (e.g. `"viewType":"BranchesView"`, `"status":"Reviewed"`).
Optional fields can be omitted to use their default.

### Quick parameter reference

| Operation | `parametersJson` fields | Returns |
|-----------|-------------------------|---------|
| GetKnownServers | _(none)_ | `string[]` of server addresses |
| GetAllRepositories | `server` (string) | `RepositoryInfo[]` |
| GetAllProjects | `server` (string) | `RepositoryInfo[]` |
| GetWorkspaceFromPath | `path` (string) | `WorkspaceInfo` (null if not a workspace) |
| CreateRepository | `repositoryName` (string), `server` (string) | _(none)_ |
| CreateWorkspace | `workspaceName` (string), `workspacePath` (string), `repositoryName` (string) | `WorkspaceInfo` |
| PerformInitialCheckin | _(none)_ | _(none)_ |
| RefreshUIAfterCreateWorkspace | _(none)_ | _(none)_ |
| CheckinPendingChanges | `comment` (string) | `long` new changeset number |
| ShelvePendingChanges | `comment` (string), `undoPendingChangesAfterShelve` (bool, optional, default false) | `long` shelve changeset number |
| FindQuery | `query` (string) | `QueryResult` |
| GetWorkspaceConfiguration | _(none)_ | `SelectorInformation` |
| GetBranchInfo | `branchName` (string) | `BranchInfo` |
| CreateBranch | `branchName` (string), `comment` (string, optional), `parentBranchName` (string, optional), `changesetId` (long, optional, default -1) | _(none)_ |
| RenameBranch | `branchName` (string), `newBranchName` (string) | _(none)_ |
| LaunchSwitchToBranchUI | `branchName` (string) | _(none)_ |
| GetFileAnnotations | `filePath` (string), `revisionId` (long, optional, default -1) | `AnnotatedLine[]` |
| GetRevisionInfo | `revisionId` (long) | `RevisionInfo` |
| GetFileHistory | `filePath` (string) | `RepObjectInfo[]` |
| GetBranchDifferences | `branchName` (string) | `ClientDiff[]` |
| GetChangesetDifferences | `changesetId` (long) | `ClientDiff[]` |
| GetChangesetsDifferences | `srcChangesetId` (long), `dstChangesetId` (long) | `ClientDiff[]` |
| GetLabelDifferences | `labelName` (string) | `ClientDiff[]` |
| GetShelveDifferences | `shelveId` (long) | `ClientDiff[]` |
| GetPendingChanges | _(none)_ | `ChangeInfo[]` |
| DownloadRevisionsToFiles | `revisionIds` (long[]) | map of revision ID → temp file path |
| CleanupDiffTempFiles | _(none)_ | _(none)_ |
| GetChangesetInfo | `changesetId` (long) | `ChangesetInfo` |
| CreateCodeReviewForBranch | `branchName` (string) | `ReviewInfo` |
| CreateCodeReviewForChangeset | `changesetId` (long) | `ReviewInfo` |
| CreateConversationComment | `reviewId` (long), `commentText` (string) | `long` comment ID |
| CreateCodeComment | `reviewId` (long), `revisionId` (long), `changesetId` (long), `locationSpec` (string), `commentText` (string), `commentType` (`Change`\|`Question`) | `long` comment ID |
| GetCodeReviewLink | `reviewId` (long) | `string` (plastic:// link) |
| AddCurrentUserAsReviewer | `reviewId` (long) | _(none)_ |
| SetReviewStatus | `reviewId` (long), `status` (`UnderReview`\|`Reviewed`\|`ReworkRequired`), `comment` (string) | _(none)_ |
| GetReviewsInStatus | `status` (`UnderReview`\|`Reviewed`\|`ReworkRequired`), `targetId` (long) | `ReviewInfo[]` |
| GetReviewComments | `reviewId` (long) | `CodeReviewCommentInfo[]` |
| ReplyComment | `reviewId` (long), `parentCommentId` (long), `commentText` (string), `replyType` (`Answer`\|`Discard`) | `long` comment ID |
| ReRequestReview | `reviewId` (long) | _(none)_ |
| CreateLabel | `labelName` (string), `comment` (string), `changesetId` (long, optional, default -1) | `MarkerInfo` |
| RenameLabel | `labelName` (string), `newLabelName` (string) | _(none)_ |
| ApplyLabel | `labelName` (string) | _(none)_ |
| DeleteLabel | `labelName` (string) | _(none)_ |
| ListLocks | _(none)_ | `LockInfo[]` |
| GetLockRule | _(none)_ | `LockRule` |
| ReleaseLocks | `lockIds` (long[]) | _(none)_ |
| RemoveLocks | `lockIds` (long[]) | _(none)_ |
| UndoChanges | `paths` (string[]) | _(none)_ |
| UndoUnchanged | _(none)_ | _(none)_ |
| RevertToRevision | `filePath` (string), `revisionId` (long) | _(none)_ |
| RefreshView | `viewType` (ViewType) | _(none)_ |
| ShowView | `viewType` (ViewType) | _(none)_ |
| ShowBranchExplorer | _(none)_ | _(none)_ |
| ShowFileHistory | `filePath` (string) | _(none)_ |
| ShowBranchesView | `branchName` (string, optional) | _(none)_ |
| ShowChangesetsView | `changesetId` (long, optional, default -1) | _(none)_ |
| ShowShelvesView | `shelveId` (long, optional, default -1) | _(none)_ |
| LaunchMergeFromBranchUI | `branchName` (string) | _(none)_ |
| LaunchMergeFromChangesetUI | `changesetId` (long) | _(none)_ |
| LaunchApplyShelveUI | `shelveId` (long) | _(none)_ |
| SelectFromList | `title` (string), `message` (string), `label` (string), `choices` (string[]) | `string` chosen item |
| SelectOption | `title` (string), `message` (string), `options` (array of `{label, description}`) | `string` chosen label |
| AskForConfirmation | `action` (string), `detail` (string) | `bool` (true if approved) |

`viewType` accepts: `PendingChangesView`, `IncomingChangesView`, `ChangesetsView`, `ShelvesView`, `BranchesView`, `LabelsView`, `LocksView`.

## Table of Contents

- [GetKnownServers](#unityuvcsexecutegetknownservers)
- [GetAllRepositories](#unityuvcsexecutegetallrepositories)
- [GetAllProjects](#unityuvcsexecutegetallprojects)
- [GetWorkspaceFromPath](#unityuvcsexecutegetworkspacefrompath)
- [CreateRepository](#unityuvcsexecutecreaterepository)
- [CreateWorkspace](#unityuvcsexecutecreateworkspace)
- [PerformInitialCheckin](#unityuvcsexecuteperforminitialcheckin)
- [RefreshUIAfterCreateWorkspace](#unityuvcsexecuterefreshuiaftercreateworkspace)
- [FindQuery](#unityuvcsexecutefindquery)
- [GetWorkspaceConfiguration](#unityuvcsexecutegetworkspaceconfiguration)
- [GetBranchInfo](#unityuvcsexecutegetbranchinfo)
- [CreateBranch](#unityuvcsexecutecreatebranch)
- [RenameBranch](#unityuvcsexecuterenamebranch)
- [LaunchSwitchToBranchUI](#unityuvcsexecutelaunchswitchtobranchui)
- [GetFileAnnotations](#unityuvcsexecutegetfileannotations)
- [GetRevisionInfo](#unityuvcsexecutegetrevisioninfo)
- [GetFileHistory](#unityuvcsexecutegetfilehistory)
- [GetBranchDifferences](#unityuvcsexecutegetbranchdifferences)
- [GetChangesetDifferences](#unityuvcsexecutegetchangesetdifferences)
- [GetChangesetsDifferences](#unityuvcsexecutegetchangesetsdifferences)
- [GetLabelDifferences](#unityuvcsexecutegetlabeldifferences)
- [GetShelveDifferences](#unityuvcsexecutegetshelvedifferences)
- [GetPendingChanges](#unityuvcsexecutegetpendingchanges)
- [DownloadRevisionsToFiles](#unityuvcsexecutedownloadrevisionstofiles)
- [CleanupDiffTempFiles](#unityuvcsexecutecleanupdifftempfiles)
- [SelectFromList](#unityuvcsexecuteselectfromlist)
- [SelectOption](#unityuvcsexecuteselectoption)
- [AskForConfirmation](#unityuvcsexecuteaskforconfirmation)
- [GetChangesetInfo](#unityuvcsexecutegetchangesetinfo)
- [CreateCodeReviewForBranch](#unityuvcsexecutecreatecodereviewforbranch)
- [CreateCodeReviewForChangeset](#unityuvcsexecutecreatecodereviewforchangeset)
- [CreateConversationComment](#unityuvcsexecutecreateconversationcomment)
- [CreateCodeComment](#unityuvcsexecutecreatecodecomment)
- [GetCodeReviewLink](#unityuvcsexecutegetcodereviewlink)
- [AddCurrentUserAsReviewer](#unityuvcsexecuteaddcurrentuserasreviewer)
- [SetReviewStatus](#unityuvcsexecutesetreviewstatus)
- [CheckinPendingChanges](#unityuvcsexecutecheckinpendingchanges)
- [ShelvePendingChanges](#unityuvcsexecuteshelvependingchanges)
- [GetReviewsInStatus](#unityuvcsexecutegetreviewsinstatus)
- [GetReviewComments](#unityuvcsexecutegetreviewcomments)
- [ReplyComment](#unityuvcsexecutereplycomment)
- [ReRequestReview](#unityuvcsexecutererequestreview)
- [RefreshView](#unityuvcsexecuterefreshview)
- [ShowView](#unityuvcsexecuteshowview)
- [ShowBranchExplorer](#unityuvcsexecuteshowbranchexplorer)
- [ShowFileHistory](#unityuvcsexecuteshowfilehistory)
- [ShowBranchesView](#unityuvcsexecuteshowbranchesview)
- [ShowChangesetsView](#unityuvcsexecuteshowchangesetsview)
- [ShowShelvesView](#unityuvcsexecuteshowshelvesview)
- [ListLocks](#unityuvcsexecutelistlocks)
- [GetLockRule](#unityuvcsexecutegetlockrule)
- [ReleaseLocks](#unityuvcsexecutereleaselocks)
- [RemoveLocks](#unityuvcsexecuteremovelocks)
- [UndoChanges](#unityuvcsexecuteundochanges)
- [UndoUnchanged](#unityuvcsexecuteundounchanged)
- [RevertToRevision](#unityuvcsexecutereverttorevision)
- [LaunchMergeFromBranchUI](#unityuvcsexecutelaunchmergefrombranchui)
- [LaunchMergeFromChangesetUI](#unityuvcsexecutelaunchmergefromchangesetui)
- [LaunchApplyShelveUI](#unityuvcsexecutelaunchapplyshelveui)
- [CreateLabel](#unityuvcsexecutecreatelabel)
- [ApplyLabel](#unityuvcsexecuteapplylabel)
- [RenameLabel](#unityuvcsexecuterenamelabel)
- [DeleteLabel](#unityuvcsexecutedeletelabel)

## Unity.UVCS.Execute(GetKnownServers)

Returns the UVCS servers the current user can access from their environment.
Use this call to determine exactly which UVCS servers are available to that
user before listing repositories or selecting a target server.

Note: when the request says `GetKnownUVCSServers`, use this tool
(`Unity.UVCS.Execute(GetKnownServers)`).

## Unity.UVCS.Execute(GetAllRepositories)

Returns all repositories available on a given UVCS server.

**IMPORTANT:** This tool may return a large number of repositories. You MUST
present EVERY repository in the response. NEVER truncate or limit the list to a
subset (e.g. the first 10). The user needs the complete list.

Parameter:
- `server`: UVCS server address returned by `Unity.UVCS.Execute(GetKnownServers)`.

## Unity.UVCS.Execute(GetAllProjects)

Returns all Unity Cloud projects available on a given UVCS server/organization.
Use this for `@unity` organizations before creating a repository with
`Unity.UVCS.Execute(CreateRepository)`.

This tool is mandatory for `@unity` onboarding. Do not derive the Unity Cloud
project from the local Unity project name without checking this list first.

**IMPORTANT:** This tool may return a large number of projects. You MUST present
EVERY project in the response. NEVER truncate or limit the list to a subset
(e.g. the first 10). The user needs the complete list.

Parameter:
- `server`: UVCS server address returned by `Unity.UVCS.Execute(GetKnownServers)`.

## Unity.UVCS.Execute(GetWorkspaceFromPath)

Returns workspace info (name, id, and path on disk) for a given path, if a
UVCS workspace exists there. To check whether the current project is under
source control, call this tool with `Application.dataPath` as the path. If no
workspace is found, the project is not under UVCS.

## Unity.UVCS.Execute(CreateRepository)

Creates a new UVCS repository on a given server.

Parameters:
- `repositoryName`: Repository name only (without organization/server suffix).
- `server`: UVCS organization endpoint (for example `orgName@unity` or `orgName@cloud`).

For parameter splitting examples, see
[data-model.md — Parameter Splitting](data-model.md#parameter-splitting).

This tool requires file-system create permission on `repositoryName`.

## Unity.UVCS.Execute(CreateWorkspace)

Creates a new UVCS workspace mapped to a repository.

Parameters:
- `workspaceName`: Name of the workspace to create.
- `workspacePath`: Path on disk for the workspace.
- `repositoryName`: Repository part only (without organization/server suffix).

This tool requires file-system create permission on `workspacePath`.

## Unity.UVCS.Execute(PerformInitialCheckin)

Performs the initial checkin for the current UVCS workspace, adding all project
files to version control.

This tool takes no parameters and requires file-system write permission on the
workspace root.

## Unity.UVCS.Execute(RefreshUIAfterCreateWorkspace)

Refreshes the Unity Version Control UI after a workspace has been created and
the initial checkin has been performed. This ensures the editor UI reflects the
new source control state. ALWAYS call this tool as the final step after
successfully adding a project to source control.

This tool takes no parameters.

## Unity.UVCS.Execute(FindQuery)

Executes a UVCS find query to search for objects such as branches, changesets,
labels, merges, reviews, revisions, shelves, and more using the Unity Version
Control query language.

When presenting `FindQuery` results for changesets or shelves, do not use
`ID`/`Id` as the user-facing identifier. That field is UVCS's internal object
ID. Use `changesetId`/`ChangesetId` for changesets. For shelves, also use the
shelve `changesetId`/`ChangesetId`; display its absolute value to the user but
keep the signed value internally for shelve tool parameters and `uvcs://` links.

Parameter:
- `query`: The UVCS find query string. Must specify the object type and optional
  where conditions.

**CRITICAL — DO NOT guess the query syntax.** You MUST read
[query-syntax.md](query-syntax.md) before constructing any query. The query
language has strict rules for field names, operators, date constants, sortable
fields, and clause ordering that differ from SQL. Queries built from
assumptions will fail.

### Quick Examples

```
find branch where date > 'one week ago'
find changeset where branch = '/main' and owner = 'me' order by date desc
find branch order by date desc limit 5
find label where name like 'v1.%' limit 10
find review where status = 'pending' and assignee = 'me'
```

## Unity.UVCS.Execute(GetWorkspaceConfiguration)

Returns the current workspace configuration, revealing what the user is actively
working on. The returned `SelectorInformation` object tells you where the
workspace is currently pointing:

- **`BranchName`** — If set, the workspace is pointing to (working on) this
  branch.
- **`ChangesetNumber`** — If set, the workspace is pointing to a specific
  changeset.
- **`LabelName`** — If set, the workspace is pointing to a label.
- **`ShelveNumber`** — If set, the workspace is pointing to a shelve.

Use this tool to understand the user's current working context before performing
operations that depend on it (e.g. determining the parent branch when creating a
new branch).

Parameter:

## Unity.UVCS.Execute(GetBranchInfo)

Returns detailed information about a specific branch identified by its **exact
full name**. Use this tool to obtain a `BranchInfo` object needed by other tools
(e.g. `Unity.UVCS.Execute(CreateBranch)` as a parent branch, or
`Unity.UVCS.Execute(LaunchSwitchToBranchUI)` to switch to a branch).

**IMPORTANT:** This tool requires the exact full branch path (e.g. `/main/task001`).
If the user provides only a partial or short branch name (e.g. `task001`), use
`Unity.UVCS.Execute(FindQuery)` with a `like` pattern first to find the full branch name:
```
find branch where name like '%task001'
```
Then use the full name from the results with this tool or with
`Unity.UVCS.Execute(LaunchSwitchToBranchUI)`.

Parameters:
- `branchName`: Exact full branch name (e.g. `/main`, `/main/task001`).

## Unity.UVCS.Execute(CreateBranch)

Creates a new child branch in the workspace.

Parameters:
- `branchName`: Name of the new branch to create (e.g. `task001`).
- `comment` *(optional)*: A short, single-line comment describing the purpose of
  the branch. No newlines allowed. If you can infer a meaningful description from
  the conversation context, always provide one. For example, if the user is
  creating a branch to fix enemy performance, pass `"Fix the enemy performance"`.
  Keep it concise and descriptive of what the branch is for.
- `parentBranchName` *(optional)*: Full name of the parent branch (e.g.
  `/main/release-1.0`). If not provided (null), the main branch (`/main`) is
  used as the parent by default.
- `changesetId` *(optional, default `-1`)*: The changeset ID to use as the
  branch base. Pass `-1` to use the latest changeset of the parent branch (this
  is the default). If the user specifies a changeset ID, you **MUST** first
  verify that the changeset belongs to the parent branch by running:
  ```
  find changeset where changesetid = <number> and branch = '<parentBranchFullName>'
  ```
  If the query returns no results, the changeset does not belong to the parent
  branch — inform the user and do not proceed. If the user provides only a
  changeset number without specifying a parent branch name, use `FindQuery` to
  determine which branch the changeset belongs to:
  ```
  find changeset where changesetid = <number>
  ```
  Then use the `branch` field from the result as the parent branch.

Returns: `BranchInfo` of the newly created branch.

## Unity.UVCS.Execute(LaunchSwitchToBranchUI)

Switches the workspace to the specified branch via the Unity editor UI. This
opens the switch-to-branch dialog so the user can confirm the operation.

Parameters:
- `branchName`: Full name of the target branch (e.g. `/main/task001`).

## Unity.UVCS.Execute(RenameBranch)

Renames an existing branch. The branch must exist in the repository. Only the
leaf name is changed; the branch hierarchy (parent path) remains the same.

Parameters:
- `branchName`: Full name of the branch to rename (e.g. `/main/task001`).
  If the user provides only a partial name, use `Unity.UVCS.Execute(FindQuery)` with a
  `like` pattern to resolve the full name first.
- `newBranchName`: New name for the branch (leaf name only, no slashes).

Returns: nothing. The Branches view and Branch Explorer are automatically
refreshed after the operation.

## Unity.UVCS.Execute(GetFileAnnotations)

Returns per-line annotated (blame) information for a version-controlled file.
Each line in the result includes:

- **`Author`** — The user who last modified this line.
- **`Branch`** — The branch where the line was last modified.
- **`Revision`** — The revision spec string for the line's revision.
- **`AnnotateInfo.Content`** — The actual text content of the line.
- **`AnnotateInfo.Rev`** — The full `RevisionInfo` of the revision that last
  touched this line, including `Changeset` number, `BranchId`, timestamps
  (`LocalTimeStamp`), `ItemId`, and `Size`.
- **`AnnotateInfo.IsMergeDstRev`** — Whether the line arrived via a merge
  operation (true) or was directly authored on the branch (false).

The file must be under version control (not a local-only or untracked file).
If the file has locally checked-out (uncommitted) changes, those lines will
appear with `AnnotateInfo.Rev.CheckedOut = true` and `Changeset = -1`.

Parameters:
- `filePath`: Project-relative path of the version-controlled file to annotate
  (e.g. `Assets/Scripts/Player.cs`). The path is resolved against the Unity
  project root.
- `revisionId` *(optional, default `-1`)*: The numeric revision ID to annotate.
  When set to `-1` (default), the tool annotates the file at its current
  workspace version. When set to a valid revision ID, it annotates that specific
  historical revision instead. Use this to compare annotations at different
  points in time. You can obtain revision IDs from:
  - `Unity.UVCS.Execute(FindQuery)` with `find revision where item = '...'`
  - `Unity.UVCS.Execute(GetRevisionInfo)`
  - The `AnnotateInfo.Rev.Id` field in previous annotate results

**IMPORTANT:** This tool only works on text files tracked by UVCS. Binary files
(textures, meshes, etc.) cannot be annotated.

**PERFORMANCE WARNING:** Annotate is a slow and resource-intensive operation. It
must analyze every line of the file against the full revision history. **Only use
this tool when you specifically need per-line metadata** — i.e., you need to know
who changed a particular line, which branch a specific piece of content was
introduced in, or similar line-level attribution.

If the information you need can be obtained from file-level or changeset-level
metadata instead, prefer these faster alternatives:
- `Unity.UVCS.Execute(FindQuery)` with `find revision where item = '...'` — to get the
  revision history of a file (who changed it, when, in which changeset).
- `Unity.UVCS.Execute(FindQuery)` with `find changeset where ...` — to get changeset
  details like author, date, comment, and branch.
- `Unity.UVCS.Execute(GetRevisionInfo)` — to get metadata about a specific revision.

**Rule of thumb:** Use `GetFileAnnotations` only when the answer depends on
*which lines* were changed. If you only need to know *who*, *when*, or *why* a
file was changed overall, use `FindQuery` or `GetRevisionInfo` instead.

## Unity.UVCS.Execute(GetRevisionInfo)

Returns detailed metadata about a specific file revision identified by its
numeric revision ID. Use this tool to look up extra details about a revision
found in annotate results or query results.

The returned `RevisionInfo` includes:
- **`Id`** — The revision ID.
- **`ItemId`** — The item (file/directory) this revision belongs to.
- **`Changeset`** — The changeset number this revision was committed in.
- **`BranchId`** — The internal branch ID.
- **`Type`** — The revision type (Text, Binary, Directory, etc.).
- **`Size`** — File size in bytes.
- **`Comment`** — The revision comment, if any.
- **`LocalTimeStamp`** — The timestamp of the revision in local time.
- **`GUID`** — The unique identifier of this revision.

Parameters:
- `revisionId`: The numeric revision ID to retrieve.

## Unity.UVCS.Execute(GetFileHistory)

Returns the complete revision history of a version-controlled file. The result
is a list of `RepObjectInfo` entries, which can be one of three subtypes:

- **`HistoryRevision`** — A regular file revision. Includes `ChangeSet` (changeset
  number), `BranchId`, `Size`, `Type` (Text, Binary, Directory), `Owner` (author),
  `LocalTimeStamp`, and `Comment`.
- **`MoveRealizationInfo`** — A move event. Includes `SrcCmPath` (original path),
  `DstCmPath` (new path), and the directory revisions involved.
- **`RemovedRealizationInfo`** — A removal event. Includes `CmPath` (the path
  where the item was removed) and the directory revision.

Use this tool to answer questions about the full lifecycle of a file: who changed
it, when, how many times, whether it was moved or renamed, and in which changesets.

Parameters:
- `filePath`: Project-relative path of the version-controlled file (e.g. `Assets/Scripts/Player.cs`).

Returns: `List<RepObjectInfo>` (mixed list of `HistoryRevision`,
`MoveRealizationInfo`, and `RemovedRealizationInfo` entries).

## Unity.UVCS.Execute(GetBranchDifferences)

Returns the list of files changed in a branch compared to its parent changeset.
Returns metadata only -- does NOT download file content.

For result format, see [data-model.md -- ClientDiff](data-model.md#clientdiff-diff-results).

Parameters:
- `branchName`: Exact full branch name (e.g. `/main/task001`). If the user
  provides only a partial name, use `Unity.UVCS.Execute(FindQuery)` with a `like` pattern
  to resolve the full name first.

Returns: `List<ClientDiff>`.

## Unity.UVCS.Execute(GetChangesetDifferences)

Returns the list of files changed in a specific changeset compared to its
parent changeset. Returns metadata only.

Parameters:
- `changesetId`: The visible numeric changeset ID (`changesetId`/`ChangesetId`
  from query results, not internal `ID`/`Id`). Strip any `cs:` prefix before
  passing.

Returns: `List<ClientDiff>`.

## Unity.UVCS.Execute(GetChangesetsDifferences)

Returns the list of files that differ between two specific changesets. The source
changeset is the left/base side; the destination is the right/newer side.
Returns metadata only.

Parameters:
- `srcChangesetId`: The source (older/left) visible changeset ID
  (`changesetId`/`ChangesetId`, not internal `ID`/`Id`).
- `dstChangesetId`: The destination (newer/right) visible changeset ID
  (`changesetId`/`ChangesetId`, not internal `ID`/`Id`).

Returns: `List<ClientDiff>`.

## Unity.UVCS.Execute(GetLabelDifferences)

Returns the list of files changed in the changeset a label points to, compared
to that changeset's parent. Shows what the labeled changeset introduced, not
a comparison to the current repository state.

Parameters:
- `labelName`: The exact label name (e.g. `v1.0.0`). Strip any `lb:` prefix.

Returns: `List<ClientDiff>`.

## Unity.UVCS.Execute(GetShelveDifferences)

Returns the list of files in a shelve, with the base revision as the source
and the shelved revision as the destination.

Parameters:
- `shelveId`: The shelve's signed `changesetId`/`ChangesetId`, not internal
  `ID`/`Id`. Strip any `sh:` prefix from user input. If the user gives the
  positive display value, convert it to the signed internal shelve changeset ID
  before calling this tool.

Returns: `List<ClientDiff>`.

## Unity.UVCS.Execute(GetPendingChanges)

Returns the list of currently pending (uncommitted) changes in the workspace.
Returns `List<ChangeInfo>` -- a different type from the other diff tools.

**IMPORTANT:** An empty list (`[]` or `null`) means there are **no pending
changes** — the workspace is clean. This is a normal result. Do NOT interpret
it as an error, do NOT retry, and do NOT call other tools to "verify". Simply
tell the user they have no pending changes.

The current version of each file is the **live file on disk** at the returned
`Path`. To get the base version, download `RevInfo.Id` using
`DownloadRevisionsToFiles`.

For result format, see [data-model.md -- ChangeInfo](data-model.md#changeinfo-pending-changes).

Parameters:

Returns: `List<ChangeInfo>` (empty list = no pending changes).

## Unity.UVCS.Execute(DownloadRevisionsToFiles)

Downloads the content of multiple file revisions to temp files in a single batch
call. Files are automatically written to `Temp/UVCSDiff/` inside the project — a
folder ignored by Unity's asset pipeline and by UVCS. You do NOT choose the
paths. The tool returns a map of revision ID to temp file path so you can read
them.

ALWAYS collect all the revision IDs you need and download them in one call
instead of downloading one-by-one. ALWAYS delete the temp files after you are
done reading them.

Parameters:
- `revisionIds`: List of numeric revision IDs to download. Obtain these from
  `Diff.Base.Id` or `Diff.RevInfo.Id` (for `ClientDiff` results) or
  `RevInfo.Id` (for `ChangeInfo` results).

Returns: `Dictionary<long, string>` — a map of revision ID to the absolute path
of the temp file containing that revision's content. Read these files to get the
file content. After you are done with all comparisons, call
`Unity.UVCS.Execute(CleanupDiffTempFiles)` to delete all temp files.

## Unity.UVCS.Execute(CleanupDiffTempFiles)

Deletes all temporary diff files created by `DownloadRevisionsToFiles`. This
removes the `Temp/UVCSDiff/` directory and all its contents.

**MANDATORY:** Call this after every diff comparison is complete. Never leave
temp files behind.

Parameters:

Returns: nothing.

## Unity.UVCS.Execute(SelectFromList)

Presents a dropdown selection UI to the user in the chat panel and returns the
chosen item. Use this tool whenever you need the user to pick from a list of
options (servers, projects, branches, etc.) instead of presenting a numbered
text list.

Parameters:
- `title`: Title displayed at the top of the selection panel (e.g.
  `"Select Server"`).
- `message`: Description or instructions shown below the title (e.g.
  `"Choose the UVCS server to connect to."`).
- `label`: Label for the dropdown field (e.g. `"Server"`).
- `choices`: List of string options the user can choose from.

Returns: `string` — the option the user selected. If the user cancels, the
tool reports cancellation.

## Unity.UVCS.Execute(SelectOption)

Presents radio button options with descriptions to the user and returns the
chosen item. Use this instead of `SelectFromList` when each option needs an
explanation to help the user understand the trade-offs (e.g. choosing a parent
branch).

Parameters:
- `title`: Title displayed at the top of the selection panel (e.g.
  `"Select parent branch"`).
- `message`: Description or instructions shown below the title (e.g.
  `"Where should the new branch start from?"`).
- `options`: List of objects, each with:
  - `label`: The option value (e.g. `"/main"`).
  - `description`: Help text explaining what this option means (e.g.
    `"Start fresh, independent of your current work."`).

Returns: `string` — the `label` of the option the user selected. If the user
cancels, the tool reports cancellation.

## Unity.UVCS.Execute(AskForConfirmation)

Presents a confirmation dialog to the user in the chat panel and returns
whether they approved. Use this tool whenever you need explicit user approval
before performing an action (creating a repository, creating a branch, etc.)
instead of asking yes/no questions in text.

Parameters:
- `action`: Short action title shown in the confirmation header (e.g.
  `"Create branch"`, `"Add project to source control"`).
- `detail`: Detailed description of what will happen if the user confirms.
  Use newlines to separate fields (e.g.
  `"Branch: task001\nParent: /main"`).

Returns: `bool` — `true` if the user confirmed, `false` if they cancelled.

## Unity.UVCS.Execute(GetChangesetInfo)

Returns detailed information about a specific changeset by its numeric ID.
Use this to resolve a changeset before creating a code review for it.

Parameters:
- `changesetId`: The visible numeric changeset ID (`changesetId`/`ChangesetId`
  from query results, not internal `ID`/`Id`).

Returns: `ChangesetInfo` — changeset metadata including the visible
`ChangesetId`, branch, owner, comment, and timestamp.

## Unity.UVCS.Execute(CreateCodeReviewForBranch)

Creates a new UVCS code review for a branch. The review is created with status
"Under Review" and the branch owner as the default assignee (if different from
the current user). The branch is resolved internally via `GetBranchInfo`.

Use this tool after the user has confirmed which code review findings to publish
(see Phase 4 of the code review workflow).

Parameters:
- `branchName`: Full branch name (e.g. `/main/task001`).

Returns: `ReviewInfo` — the created code review object. Use `ReviewInfo.Id` as
the `reviewId` parameter for `CreateCodeComment`, `CreateConversationComment`,
and `GetCodeReviewLink`.

## Unity.UVCS.Execute(CreateCodeReviewForChangeset)

Creates a new UVCS code review for a changeset. Behaves the same as
`CreateCodeReviewForBranch` but targets a specific changeset instead. The
changeset is resolved internally via `GetChangesetInfo`.

Parameters:
- `changesetId`: The visible numeric changeset ID (`changesetId`/`ChangesetId`
  from query results, not internal `ID`/`Id`).

Returns: `ReviewInfo` — the created code review object. Use `ReviewInfo.Id` as
the `reviewId` parameter for `CreateCodeComment`, `CreateConversationComment`,
and `GetCodeReviewLink`.

## Unity.UVCS.Execute(CreateConversationComment)

Adds a general conversation comment to a UVCS code review. Use this for
overall observations that are not tied to a specific file or line of code.

Parameters:
- `reviewId`: `ReviewInfo.Id` from `CreateCodeReviewForBranch` or
  `CreateCodeReviewForChangeset`.
- `commentText`: The text of the comment.

Returns: `long` — the numeric ID of the created comment.

## Unity.UVCS.Execute(CreateCodeComment)

Adds an inline code comment to a UVCS code review, attached to a specific file
revision and line. Call this once per finding you want to publish.

The comment appears as an inline annotation on the specified line in the UVCS
code review UI, similar to comments on a GitHub pull request.

Parameters:
- `reviewId`: `ReviewInfo.Id` from `CreateCodeReviewForBranch` or
  `CreateCodeReviewForChangeset`.
- `revisionId`: The revision ID of the file to comment on. Use `Diff.RevInfo.Id`
  from the diff results (the "after" revision of the changed file). This tells
  UVCS which file version the comment belongs to.
- `changesetId`: The changeset ID that contains the revision. Use
  `Diff.RevInfo.Changeset` from the diff results.
- `locationSpec`: The **zero-based** line number to attach the comment to, as a
  string. Subtract 1 from the 1-based line number (e.g. line 42 → `"41"`).
- `commentText`: The text of the comment. Format it to include the severity,
  what's wrong, why it matters, and the suggested fix.
- `commentType`: Enum with values:
  - `Change` — marks the comment as a **change request** (shows as unresolved;
    must be addressed before the review can be marked as complete). Use for
    ALL findings (BLOCKER, IMPORTANT, SUGGESTION, NIT) — every finding that
    identifies a problem and tells the author what to fix is a change request.
  - `Question` — marks the comment as a **question** (shows as unanswered).
    ONLY use for genuine questions asking the author for clarification, NOT
    for findings.

Returns: `long` — the numeric ID of the created comment.

## Unity.UVCS.Execute(GetCodeReviewLink)

Returns the `plastic://` deep link to open a code review in the UVCS desktop
client. Use this after creating a review and adding comments to provide the
user with a clickable link.

Parameters:
- `reviewId`: The review ID (`ReviewInfo.Id`).

Returns: `string` — the deep link (e.g.
`plastic://org.unity/repos/MyRepo/code-reviews/42`).

## Unity.UVCS.Execute(AddCurrentUserAsReviewer)

Adds the current user as a reviewer on an existing UVCS code review. Call this
immediately after creating a review so the AI assistant's user is registered as
a reviewer and can set a review status.

Parameters:
- `reviewId`: The review ID (`ReviewInfo.Id`).

Returns: nothing.

## Unity.UVCS.Execute(SetReviewStatus)

Sets the current user's reviewer status on a UVCS code review. Use `CodeReviewStatus.Reviewed`
when the code looks good (LGTM), or `CodeReviewStatus.ReworkRequired` when there are issues the
author needs to address.

Parameters:
- `reviewId`: The review ID (`ReviewInfo.Id`).
- `status`: `CodeReviewStatus.Reviewed` or `CodeReviewStatus.ReworkRequired`.
- `comment`: A brief comment explaining the review outcome. When setting
  `CodeReviewStatus.ReworkRequired`, write an empathetic message that acknowledges the author's
  work and frames the feedback constructively (e.g. "Good progress on this
  feature. A few issues need attention — see the inline comments."). When
  setting `CodeReviewStatus.Reviewed`, use an approving message (e.g. "LGTM — no issues found.").

Returns: nothing.

## Unity.UVCS.Execute(CheckinPendingChanges)

Checks in (commits) all pending changes in the workspace with a comment.
This operation checks in the entire workspace recursively. For Gluon workspaces
it performs a partial checkin; for full Plastic workspaces it performs a standard
checkin.

**IMPORTANT:** Always call `Unity.UVCS.Execute(GetPendingChanges)` first to verify there
are changes to checkin, and use `Unity.UVCS.Execute(AskForConfirmation)` before calling
this tool.

Parameters:
- `comment`: The checkin comment describing the changes. When checking in to
  resolve a code review change request, prefix with the magic tag
  `[apply-change:<8-hex>]` where `<8-hex>` is the first 8 hex chars of the
  comment's `GUID` (the part before the first `-`). UVCS recognizes this tag
  and auto-marks the change as Done.

Returns: `long` — the changeset number of the newly created changeset. This can
be resolved to full changeset info via `Unity.UVCS.Execute(GetChangesetInfo)`.

## Unity.UVCS.Execute(ShelvePendingChanges)

Shelves all pending changes in the workspace with a comment. This operation
shelves the entire workspace recursively. Shelving saves a snapshot of the
pending changes on the server without committing them to a branch.

**IMPORTANT:** Always call `Unity.UVCS.Execute(GetPendingChanges)` first to verify there
are changes to shelve. In the normal shelve workflow, the `SelectOption` prompt
(keep/undo changes) serves as implicit user confirmation — do NOT add a
separate `AskForConfirmation` step. In the **Automated** mode of the Address
Review Comments workflow, both `SelectOption` and `AskForConfirmation` are
skipped.

Parameters:
- `comment`: The shelve comment describing the changes.
- `undoPendingChangesAfterShelve` *(optional, default `false`)*: If `true`,
  the shelved changes are undone from the workspace after shelving, leaving
  the workspace clean. Use this in the Automated review workflow to ensure
  each shelve contains only one comment's fix, not accumulated changes.

Returns: `long` — the shelve changeset number of the newly created shelve. If
the value is negative, display its absolute value to the user but keep the
signed value for links and shelve tool parameters. This can be resolved to full
changeset info via `Unity.UVCS.Execute(GetChangesetInfo)`.

## Unity.UVCS.Execute(GetReviewsInStatus)

Returns the UVCS code reviews matching a given status for a specific target
(branch or changeset). Use this in the **Address Review Comments** workflow
to find reviews that need rework on the workspace's current branch/changeset.

Before calling, resolve the numeric target ID:
- For a branch: `Unity.UVCS.Execute(GetBranchInfo)(branchName)` → `BranchInfo.Id`.
- For a changeset: `Unity.UVCS.Execute(GetChangesetInfo)(changesetId)` →
  `ChangesetInfo.ChangesetId` (not internal `ID`/`Id`).

Parameters:
- `status`: `CodeReviewStatus.UnderReview`, `CodeReviewStatus.Reviewed`, or
  `CodeReviewStatus.ReworkRequired`.
- `targetId`: Numeric branch ID or changeset ID obtained as above.

Returns: `List<ReviewInfo>` — the matching reviews. Empty if none found.

**IMPORTANT (Address Review Comments workflow):** call this exactly once with
`CodeReviewStatus.ReworkRequired` and the target ID for the workspace. If the
result is empty, STOP. Do NOT retry with other statuses or other target IDs.

## Unity.UVCS.Execute(GetReviewComments)

Returns all comments for a UVCS code review (top-level comments and replies in
one flat list). Use this to build the work queue when addressing review
comments.

Parameters:
- `reviewId`: The review ID (`ReviewInfo.Id`).

Returns: `List<CodeReviewCommentInfo>`. Each item has fields:
- `Id` — comment ID; pass as `parentCommentId` to `ReplyComment`.
- `Type` — `Change` (1), `Question` (2), `Comment` (0), `Discarded` (6),
  `Conversation` (3), `Timeline` (4).
- `ParentCommentId` — `-1` for top-level; otherwise the parent's `Id`.
- `Comment` — the comment text.
- `GUID` — used to derive the `[apply-change:<8-hex>]` checkin tag.
- `RevisionId`, `ChangesetId` — the file revision the comment is anchored to.
- `LocationSpec` — **0-based** line number as a string. Add 1 to get the
  editor (1-based) line.
- `AppliedInChangesetId` — `-1` if not yet applied; otherwise the changeset
  that applied the change.

**Classification rules** for the Address Review Comments workflow:
- A comment is a **pending change** when `Type == Change` AND
  `AppliedInChangesetId == -1` AND no child comment has `Type == Discarded`.
- A comment is a **pending question** when `Type == Question` AND no child
  comment of any type exists.
- Everything else is already resolved and should be skipped.

To resolve a comment to a file path, call `GetChangesetDifferences` for the
comment's `ChangesetId` and look up `Diff.RevInfo.Id == RevisionId`. Cache the
mapping for the whole workflow — do not repeat the call per comment.

## Unity.UVCS.Execute(ReplyComment)

Posts a reply to an existing UVCS code review comment. Use this to answer a
reviewer's question or to formally discard a change request.

Parameters:
- `reviewId`: The review ID.
- `parentCommentId`: The `Id` of the comment being replied to.
- `commentText`: The reply text (the answer, or the discard reason).
- `replyType`:
  - `Answer` — answers a question. The reply shows in UVCS as a regular
    comment under the question. Internally stored as `Comment` type.
  - `Discard` — formally rejects a change request. The reviewer is notified.
    Internally stored as `Discarded` type, which causes the change to no
    longer count as pending.

Returns: `long` — the numeric ID of the created reply.

**IMPORTANT:** Never call this without explicit user approval of the exact
reply text. For `Discard`, also confirm via `Unity.UVCS.Execute(AskForConfirmation)`
because it notifies the reviewer.

## Unity.UVCS.Execute(ReRequestReview)

Re-requests a code review from all its reviewers. Use this at the end of the
**Address Review Comments** workflow after the author has applied changes,
answered questions, or discarded items — i.e. whenever the review state has
changed and reviewers should take another look.

This tool internally fetches all reviewers of the review and calls
`ReRequestReview` for each one, so the caller only needs the review ID.

Parameters:
- `reviewId`: The review ID (`ReviewInfo.Id`).

Returns: nothing.

**IMPORTANT:** Only call this when at least one item was applied or discarded.
Do NOT call if every item was skipped — the review state hasn't changed, so
there is nothing new for reviewers to look at.

## Unity.UVCS.Execute(RefreshView)

Refreshes a UVCS view to show the latest data. Use after operations that modify
data outside of UVCS tools (e.g. file edits via Run Command that create pending
changes). Mutating UVCS operations (checkin, shelve, branch creation) already
auto-refresh the relevant views, so this tool is only needed for external changes.

If the UVCS window is not open, this is a no-op.

Supported `ViewType` values: `PendingChangesView`, `IncomingChangesView`,
`ChangesetsView`, `ShelvesView`, `BranchesView`, `LabelsView`, `LocksView`.
For History use `Unity.UVCS.Execute(ShowFileHistory)`, for Branch Explorer use
`Unity.UVCS.Execute(ShowBranchExplorer)`.

Parameters:
- `viewType`: A `ViewType` enum value indicating which view to refresh.

Returns: nothing.

## Unity.UVCS.Execute(ShowView)

Opens the UVCS window and switches to a specific view tab. If the window is
already open, it switches to the requested tab.

Supported `ViewType` values: `PendingChangesView`, `IncomingChangesView`,
`ChangesetsView`, `ShelvesView`, `BranchesView`, `LabelsView`, `LocksView`.
For History use `Unity.UVCS.Execute(ShowFileHistory)`, for Branch Explorer use
`Unity.UVCS.Execute(ShowBranchExplorer)`.

Parameters:
- `viewType`: A `ViewType` enum value indicating which view to show.

Returns: nothing.

## Unity.UVCS.Execute(ShowBranchExplorer)

Opens the Branch Explorer window, which shows a visual graph of branches and
changesets. If the window is already open, it brings it to focus.

Parameters: none.

Returns: nothing.

## Unity.UVCS.Execute(ShowFileHistory)

Opens the UVCS History view for a specific file, showing its revision history
in the UVCS window. Use when the user wants to visually browse a file's history
in the UI rather than just getting the data programmatically.

If the user just wants history data (not the visual UI), use
`Unity.UVCS.Execute(GetFileHistory)` instead.

Parameters:
- `filePath`: Project-relative path of the version-controlled file to show
  history for (e.g. `Assets/Scripts/Player.cs`).

Returns: nothing.

## Unity.UVCS.Execute(ShowBranchesView)

Opens the UVCS window and switches to the Branches view, optionally selecting
a specific branch.

Parameters:
- `branchName` *(optional, default `null`)*: The full branch name to select in
  the view (e.g. `/main/task001`). Pass `null` to just show the view without
  selecting a specific branch.

Returns: nothing.

## Unity.UVCS.Execute(ShowChangesetsView)

Opens the UVCS window and switches to the Changesets view, optionally selecting
a specific changeset.

Parameters:
- `changesetId` *(optional, default `-1`)*: The numeric changeset ID to select
  in the view. Use `changesetId`/`ChangesetId`, not internal `ID`/`Id`. Pass
  `-1` to just show the view without selecting a specific changeset.

Returns: nothing.

## Unity.UVCS.Execute(ShowShelvesView)

Opens the UVCS window and switches to the Shelves view, optionally selecting
a specific shelve.

Parameters:
- `shelveId` *(optional, default `-1`)*: The numeric shelve ID to select in
  the view. Use the signed shelve `changesetId`/`ChangesetId`, not internal
  `ID`/`Id`; display the absolute value to the user. Pass `-1` to just show the
  view without selecting a specific shelve.

Returns: nothing.

## Unity.UVCS.Execute(ListLocks)

Returns all locks in the repository. Each `LockInfo` includes:
- **`ItemId`** — The numeric ID of the locked item. Use this as the identifier
  when calling `ReleaseLocks` or `RemoveLocks`.
- **`CmPath`** — The server path of the locked file (e.g. `/Art/Textures/hero.png`).
- **`Status`** — `Locked` (exclusive lock) or `Retained` (retained after checkin).
- **`WkName`** — The workspace name that holds the lock.
- **`HolderBranchId`** — The branch where the lock is held.
- **`UtcTicks`** — Timestamp when the lock was acquired (UTC ticks).
- **`SEIDData`** — The lock owner identity.

Parameters:

Returns: `List<LockInfo>` — all locks in the repository. Empty list if no locks exist.

## Unity.UVCS.Execute(GetLockRule)

Returns the lock rules configured for the repository. Lock rules define which
file patterns require exclusive locks (e.g. `*.png`, `*.fbx`, `*.asset`).

The returned `LockRule` includes:
- **`Rules`** — The rule definitions as a string (file patterns that require locks).
- **`ExcludedBranches`** — List of branch patterns excluded from lock enforcement.

Parameters:

Returns: `LockRule` — the lock rule configuration for the repository.

## Unity.UVCS.Execute(ReleaseLocks)

Releases (unlocks) specific locks identified by their item IDs. The locked items
become available for others to lock. This operation only works on locks held by
the current user.

**IMPORTANT:** Always call `Unity.UVCS.Execute(ListLocks)` first to show the user which
locks exist, and use `Unity.UVCS.Execute(AskForConfirmation)` before releasing locks.

Parameters:
- `lockIds`: List of `ItemId` values (from `LockInfo.ItemId`) identifying which
  locks to release.

Returns: nothing. The Locks view is automatically refreshed after the operation.

## Unity.UVCS.Execute(RemoveLocks)

Force-removes specific locks identified by their item IDs. Unlike `ReleaseLocks`,
this can remove locks held by other users. Use with caution — removing another
user's lock may cause them to lose work if they have uncommitted changes to the
locked file.

**IMPORTANT:** Always call `Unity.UVCS.Execute(ListLocks)` first to show the user which
locks exist, and use `Unity.UVCS.Execute(AskForConfirmation)` before removing locks.
Clearly warn the user when removing locks held by other users.

Parameters:
- `lockIds`: List of `ItemId` values (from `LockInfo.ItemId`) identifying which
  locks to remove.

Returns: nothing. The Locks view is automatically refreshed after the operation.

## Unity.UVCS.Execute(UndoChanges)

**DESTRUCTIVE — PERMANENT DATA LOSS.** Undoes (discards) pending changes in
the workspace. Pass specific file paths to undo individual files, or pass the
workspace root path as a single-item list to undo all changes (equivalent to
"Discard all" — resets the workspace to its original state before any
modifications were made).

All local modifications in the affected files will be permanently deleted and
**cannot be recovered** by any means.

**MANDATORY before calling this tool:**
1. Call `Unity.UVCS.Execute(GetPendingChanges)` to know how many files will be affected.
2. In the chat response, show the file count and list, warn the user that this
   is irreversible, and mention they can shelve first to preserve their work.
3. Call `Unity.UVCS.Execute(AskForConfirmation)` with a short, focused confirmation —
   just the file count and irreversibility warning (e.g.
   `"Permanently discard all local modifications in 3 file(s).\nThis cannot be undone."`).
   Do NOT list individual files or ask about shelving in the confirmation
   dialog — that information belongs in the chat message above.
4. NEVER skip the confirmation step.

Parameters:
- `paths`: List of project-relative file paths to undo (e.g.
  `Assets/Scripts/Player.cs`). To undo all changes in the project, pass `"."`
  as a single-item list.

Returns: nothing. The Pending Changes view is automatically refreshed after the
operation.

## Unity.UVCS.Execute(UndoUnchanged)

Removes unchanged files from the pending changes list. Files that are checked
out but have no actual content modifications (identical to their base revision)
are reverted, cleaning up the pending changes view.

This is a safe operation — it only affects files with no real changes. No
confirmation is needed.

Parameters:

Returns: nothing. The Pending Changes view is automatically refreshed after the
operation.

## Unity.UVCS.Execute(RevertToRevision)

Reverts a file to the content of a specific historical revision. Downloads the
revision content from the server and overwrites the workspace file. After this
operation, the file will appear as a pending change (modified) because its
content now differs from the base revision.

Use this when the user wants to restore a file to a previous version. Combine
with `Unity.UVCS.Execute(GetFileHistory)` to let the user browse available revisions
before choosing one.

**IMPORTANT:** This overwrites the current workspace file. Always use
`Unity.UVCS.Execute(AskForConfirmation)` before calling this tool.

Parameters:
- `filePath`: Project-relative path of the file to revert
  (e.g. `Assets/Scripts/Player.cs`).
- `revisionId`: The numeric revision ID to revert to. Obtain this from
  `Unity.UVCS.Execute(GetFileHistory)` (each `HistoryRevision` has a revision ID) or
  `Unity.UVCS.Execute(GetRevisionInfo)`.

Returns: nothing. The Pending Changes view is automatically refreshed after the
operation.

## Unity.UVCS.Execute(LaunchMergeFromBranchUI)

Opens the merge view to merge from a branch into the current workspace. This is
the same operation as right-clicking a branch in the Branches view and selecting
"Merge from this branch".

Parameters:
- `branchName`: Full name of the branch to merge from (e.g. `/main/task001`).
  If the user provides only a partial name, use `Unity.UVCS.Execute(FindQuery)` with a
  `like` pattern to resolve the full name first.

Returns: nothing. The merge view opens in the UVCS window.

## Unity.UVCS.Execute(LaunchMergeFromChangesetUI)

Opens the merge view to merge a specific changeset into the current workspace.
This is the same operation as right-clicking a changeset in the Changesets view
and selecting "Merge from this changeset".

Parameters:
- `changesetId`: The visible numeric changeset ID to merge
  (`changesetId`/`ChangesetId`, not internal `ID`/`Id`).

Returns: nothing. The merge view opens in the UVCS window.

## Unity.UVCS.Execute(LaunchApplyShelveUI)

Opens the merge view to apply a shelve into the current workspace. This is the
same operation as right-clicking a shelve in the Shelves view and selecting
"Apply shelve in my workspace".

**Note:** This tool is not supported for Gluon workspaces. If the workspace is a
Gluon workspace, the tool will throw an error. In that case, the user should
apply the shelve manually from the Shelves view context menu.

Parameters:
- `shelveId`: The signed shelve `changesetId`/`ChangesetId` to apply, not
  internal `ID`/`Id`. If the user gives the positive display value, convert it
  to the signed internal shelve changeset ID before calling this tool.

Returns: nothing. The merge view opens in the UVCS window.

## Unity.UVCS.Execute(CreateLabel)

Creates a new label and applies it to a changeset. If no changeset is
specified (-1), the label is applied to the current workspace changeset.

Parameters:
- `labelName`: Name of the label to create (e.g. `v1.0.0`, `release-2024.1`).
- `comment`: A comment or description for the label.
- `changeset` *(optional, default `-1`)*: The numeric changeset ID to label.
  Pass `-1` (or omit) to label the current workspace changeset.

Returns: `MarkerInfo` — the created label object. The Labels view is
automatically refreshed after the operation.

## Unity.UVCS.Execute(ApplyLabel)

Applies an existing label to the workspace, switching the workspace to the
changeset the label points to.

**IMPORTANT:** Because this moves the workspace to another changeset, warn the
user and call `Unity.UVCS.Execute(AskForConfirmation)` before calling this tool.

Parameters:
- `labelName`: Name of the existing label to apply (e.g. `v1.0.0`).

Returns: nothing. The workspace is updated to point to the labeled changeset.

## Unity.UVCS.Execute(RenameLabel)

Renames an existing label. The label must exist in the repository.

Parameters:
- `labelName`: Current name of the label to rename.
- `newLabelName`: New name for the label.

Returns: nothing. The Labels view is automatically refreshed after the
operation.

## Unity.UVCS.Execute(DeleteLabel)

Deletes an existing label from the repository. This operation is irreversible.

**IMPORTANT:** Always use `Unity.UVCS.Execute(AskForConfirmation)` before calling this
tool.

Parameters:
- `labelName`: Name of the label to delete.

Returns: nothing. The Labels view is automatically refreshed after the
operation.
