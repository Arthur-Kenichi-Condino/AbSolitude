# Diff Workflows

Workflows for comparing file content using UVCS diff tools. For data model
details, see [data-model.md](data-model.md).

## Table of Contents

- [Quick Reference](#quick-reference)
- [Critical Rules](#critical-rules)
- [Return Types](#return-types)
- [Workflow: Metadata Only](#workflow-metadata-only)
- [Workflow: Full Comparison](#workflow-full-comparison)
- [Pending Changes Exception](#pending-changes-exception)
- [Resolving Object Names](#resolving-object-names)
- [Notes](#notes)

---

## Quick Reference

| Scenario | Diff tool | What it compares | Returns |
|----------|-----------|------------------|---------|
| Branch | `Unity.UVCS.Execute(GetBranchDifferences)` | Branch vs its parent branch | `ClientDiff[]` |
| Changeset | `Unity.UVCS.Execute(GetChangesetDifferences)` | Changeset vs its parent changeset | `ClientDiff[]` |
| Two changesets | `Unity.UVCS.Execute(GetChangesetsDifferences)` | Older (source) vs newer (destination) changeset | `ClientDiff[]` |
| Label | `Unity.UVCS.Execute(GetLabelDifferences)` | Labeled changeset vs its parent changeset | `ClientDiff[]` |
| Shelve | `Unity.UVCS.Execute(GetShelveDifferences)` | Shelved changes content | `ClientDiff[]` |
| Pending changes | `Unity.UVCS.Execute(GetPendingChanges)` | Current uncommitted workspace changes | `ChangeInfo[]` |

**Content tools** (used by all workflows when full comparison is needed):
`Unity.UVCS.Execute(DownloadRevisionsToFiles)`, `Unity.UVCS.Execute(CleanupDiffTempFiles)`.

---

## Critical Rules

1. **Two behaviors based on user phrasing:**
   - **"Which files changed?"**, **"List the changed files"**, **"Enumerate"**
     → Metadata only. Present file list. Stop.
   - **"What changed?"**, **"Tell me what changed"**, **"Explain"**, **"Describe"**
     → Full content comparison. Download files, read, explain content. Stop.

2. **`DownloadRevisionsToFiles` automatically writes to `Temp/UVCSDiff/`** inside
   the project (ignored by Unity and UVCS) and returns a map of revision ID to
   temp file path. You do NOT choose where to write. ALWAYS delete the returned
   temp files after comparison. Always download all needed revisions in a single
   batch call instead of one-by-one.

3. **An empty result (`[]` or `null`) means NO changes.** This is normal. Do NOT
   interpret it as an error. Do NOT retry or call other tools to verify. Simply
   tell the user there are no changes.

4. **Each diff tool already returns the COMPLETE diff.** `GetBranchDifferences`
   returns ALL files changed across the entire branch. `GetChangesetDifferences`
   returns ALL files changed in that changeset. There is NEVER a need to call
   additional diff tools, `GetChangesetDifferences`, `GetBranchInfo`, `FindQuery`
   for changesets, or any other tool to "dig deeper" after the initial diff call.

5. **The workflow is exactly 4 steps: get metadata, download, compare, cleanup.**
   Do NOT add extra investigation steps. Do NOT query individual changesets
   within a branch. Do NOT look up changeset comments. Do NOT call
   `GetBranchInfo`. The diff result plus the downloaded file content is ALL you
   need to answer the user.

---

## Return Types

### ClientDiff (returned by branch/changeset/label/shelve diffs)

Each `ClientDiff` contains a `Diff` property (a `Difference` object):

- **`Diff.Path`** -- File path in the repository.
- **`Diff.Status`** -- Enum: `Added` (0), `Deleted` (1), `Moved` (2), `Changed` (3).
- **`Diff.IsDirectory`** -- `true` for directories (skip these).
- **`Diff.Base`** -- `RevisionInfo` of the base version. `null` for `Added`.
  Use `Diff.Base.Id` for downloading.
- **`Diff.RevInfo`** -- `RevisionInfo` of the new version. `null` for `Deleted`.
  Use `Diff.RevInfo.Id` for downloading.
- For `Moved`: `DiffMoved.SrcPath` gives the old path.

### ChangeInfo (returned by GetPendingChanges)

- **`Path`** -- Absolute workspace file path on disk (this IS the current version).
- **`ChangeTypes`** -- Flags: `Added`, `Deleted`, `Moved`, `LocallyMoved`,
  `CheckedOut`, `Changed`, `LocallyDeleted`, `Private`, etc.
- **`RevInfo`** -- Base revision. `null` for new/private files. Use `RevInfo.Id`
  for downloading.
- **`OldPath`** -- Previous path (for moved files).
- **`IsDirectory`** -- `true` for directories (skip these).

---

## Workflow: Metadata Only

Use when the user asks **which** files changed, or asks to **list** / **enumerate**.

1. Get workspace via `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. Call the appropriate diff tool from the [Quick Reference](#quick-reference).
3. Filter out directory entries.
4. Present file list grouped by status. **Stop.**

### Example

**User:** "Which files changed in branch /main/task001?"

1. `GetWorkspaceFromPath` → workspace.
2. `GetBranchDifferences(wkPath, "/main/task001")` → `ClientDiff[]`.
3. Filter directories.
4. Present:
   > **Changed:** `Player.cs`, `GameConfig.asset`
   > **Added:** `GameManager.cs`
   > **Deleted:** `OldHelper.cs`

---

## Workflow: Full Comparison

Use when the user asks **what** changed, or asks to **explain** / **describe** /
**tell me** / **show** the changes.

1. **Get the workspace** via `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. **Call the appropriate diff tool** from the [Quick Reference](#quick-reference).
3. **Filter** out directory entries and binary files (`.png`, `.fbx`, `.wav`,
   `.dll`, `.asset`, `.mat`, `.prefab`, `.unity`, `.meta`, etc.).
4. **Download text file revisions** using `Unity.UVCS.Execute(DownloadRevisionsToFiles)`.
   Collect ALL revision IDs you need, then download them in a single batch call.
   The tool returns a map of revision ID to temp file path.
   - `Changed`: download both `Diff.Base.Id` (BEFORE) and `Diff.RevInfo.Id` (AFTER).
   - `Added`: download `Diff.RevInfo.Id` only (AFTER — the new file).
   - `Deleted`: download `Diff.Base.Id` only (BEFORE — the removed file).
5. **Read the files and compare** BEFORE vs AFTER.
6. **Present a content-level summary** for each file: what lines/sections were
   added, removed, or modified. Always label values as "Before" (from the base
   revision) and "Now" (from the new/workspace revision). Be specific.
7. **Mention binary files** at the end with status only.
8. **Call `Unity.UVCS.Execute(CleanupDiffTempFiles)`** to delete all downloaded temp
   files. This is MANDATORY — never skip this step.
9. **STOP.** Do NOT make any additional tool calls. The answer is complete.

### Example

**User:** "Explain what changed in branch /main/task001"

1. `GetWorkspaceFromPath` → workspace.
2. `GetBranchDifferences(wkPath, "/main/task001")` → `ClientDiff[]`
   covering ALL changes in the entire branch (on
   [/main/task001](uvcs://show/branch/main/task001)).
3. Filter: `Player.cs` (text, changed), `GameManager.cs` (text, added),
   `Cube.mat` (binary, skip content).
4. Download all needed revisions in one batch call:
   - `DownloadRevisionsToFiles(wkPath, [<Player Base.Id>, <Player RevInfo.Id>, <GameManager RevInfo.Id>])`
     → `{ <Player Base.Id>: ".../rev_438", <Player RevInfo.Id>: ".../rev_510", <GameManager RevInfo.Id>: ".../rev_511" }`
5. Read files, compare.
6. Present:
   > **Player.cs**: The `Health` property was increased from 100 to 150.
   > A new `TakeDamage(int amount)` method was added that subtracts from health
   > and calls `Die()` when health reaches zero.
   >
   > **GameManager.cs** (new file): Implements a singleton game manager with
   > `StartGame()` and `EndGame()` methods and a `Score` counter.
   >
   > **Cube.mat**: Modified (binary file, content not shown).
7. `CleanupDiffTempFiles(wkPath)` → temp files deleted. **Done.**

---

## Pending Changes Exception

`GetPendingChanges` returns `ChangeInfo[]` (NOT `ClientDiff[]` — see
[Return Types](#return-types)). The full comparison differs because the current
version is the workspace file on disk, not a downloaded revision:

1. Download `RevInfo.Id` for each changed file → this is the **BEFORE**
   (base/server revision).
2. Read the workspace file at `Path` → this is the **AFTER** (current version
   on disk, the one the user has modified).
3. **Do NOT swap them.** The downloaded temp file is always BEFORE. The
   workspace file is always AFTER.
4. Compare BEFORE vs AFTER. Present content summary.
5. Call `Unity.UVCS.Execute(CleanupDiffTempFiles)`. **Done.**

### Example

**User:** "Tell me what I've changed"

1. `GetPendingChanges(wkPath)` → `ChangeInfo[]`.
2. For each changed text file:
   - Download `RevInfo.Id` to temp file → this is **BEFORE** (server/base version).
   - Read the workspace file at `Path` → this is **AFTER** (current on disk).
3. Compare BEFORE vs AFTER. Present:
   > **Cube.mat**: The `_Color` property was changed:
   > - Before: `{r: 0.624, g: 0.217, b: 0.173, a: 1}` (reddish)
   > - Now: `{r: 0.556, g: 0.624, b: 0.173, a: 1}` (yellowish-green)
4. `CleanupDiffTempFiles(wkPath)` → temp files deleted. **Done.**

---

## Resolving Object Names

### Branches

If partial name, use `Unity.UVCS.Execute(FindQuery)`:
```
find branch where name like '%task001'
```

### Changesets

Strip `cs:` prefix. If described by comment/author, use `FindQuery`:
```
find changeset where comment like '%fix enemy%' order by date desc limit 5
```

### Labels

Strip `lb:` prefix. If partial:
```
find label where name like '%v1.0%'
```

### Shelves

Strip `sh:` prefix. If unknown:
```
find shelve where owner = 'me' order by date desc limit 5
```

### Two Changesets

Older = source (left), newer = destination (right). Ask if ambiguous.

---

## Notes

- **Label diff** shows what the labeled changeset introduced vs its parent. To
  compare a label to the current state, resolve both changeset IDs and use
  `GetChangesetsDifferences`.
- **Pending changes** use `ChangeInfo` (not `ClientDiff`). The current version
  is the workspace file — do NOT download a dest revision (it is `-1`).
