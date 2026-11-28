# UVCS Data Model

## Table of Contents

- [Core Concepts](#core-concepts)
- [Branches](#branches)
- [Changesets](#changesets)
- [Labels](#labels)
- [Shelves](#shelves)
- [Naming & Identifier Formats](#naming--identifier-formats)
- [Server Types](#server-types)
- [Repository Identifier Formats](#repository-identifier-formats)
- [@unity Project Selection](#unity-project-selection)
- [Parameter Splitting](#parameter-splitting)
- [Revisions](#revisions)
- [Annotations (Blame)](#annotations-blame)
- [ClientDiff (Diff Results)](#clientdiff-diff-results)
- [ChangeInfo (Pending Changes)](#changeinfo-pending-changes)
- [ReviewInfo (Code Review)](#reviewinfo-code-review)
- [CodeReviewCommentInfo (Review Comment)](#codereviewcommentinfo-review-comment)

## Core Concepts

| Concept | Description |
|---------|-------------|
| **Server** | A UVCS organization endpoint (e.g. `myOrg@unity`, `myOrg@cloud`). |
| **Repository** | Belongs to one server/organization. Stores versioned project files. |
| **Workspace** | A local directory mapped to one repository. |
| **Branch** | A parallel line of development within a repository. |
| **Changeset** | A recorded set of file changes belonging to a branch (analogous to a Git commit). |
| **Label** | A tag applied to a changeset to mark a notable point (e.g. a release version). |
| **Shelve** | A saved set of changes that is **not** attached to any branch (analogous to a Git stash). |

## Branches

Default branch is `/main`. Branches can be nested: `/main/task001` is a child
of `/main`.

## Changesets

Equivalent to a Git commit. Each changeset belongs to exactly one branch.
Initial changeset is `cs:0`; numbers increment with each checkin.

When a changeset result includes both `ID`/`Id` and
`changesetId`/`ChangesetId`, treat them as different identifiers:

- **`changesetId` / `ChangesetId`** — the visible changeset number. Display
  this to the user, use it in `uvcs://show/changeset/<id>` links, and pass it
  to changeset tools.
- **`ID` / `Id`** — UVCS's internal object ID. Do not show this as the
  changeset number and do not use it for user-facing links.

## Labels

A tag attached to a changeset (e.g. `v2.5.4` for a release milestone).

## Shelves

Like `git stash` — saves pending changes outside any branch without committing.

Shelves are backed by a special shelve changeset. When a shelve result includes
both `ID`/`Id` and `changesetId`/`ChangesetId`, use the shelve
`changesetId`/`ChangesetId` as the canonical identifier. The `ID` field is an
internal UVCS object ID and is not what the user sees in the UI.

> **Note:** Shelve `changesetId` values are stored as negative numbers
> internally. Keep the signed negative number for UVCS tool parameters and
> `uvcs://` links, but display the absolute value to users. For example,
> `ChangesetId = -55465` should be shown as `shelve 55465` and linked as
> `[shelve 55465](uvcs://show/shelve/-55465)`.

## Naming & Identifier Formats

### Branches

| Format | Example | Notes |
|--------|---------|-------|
| `br:/<path>` | `br:/main`, `br:/main/task001` | Canonical form with prefix. |
| `/<path>` | `/main`, `/main/task001` | Short form (no prefix). |

Child branches use `/` to denote hierarchy: `/main/task001` is a child of `/main`.

### Changesets

| Format | Example | Notes |
|--------|---------|-------|
| `cs:<number>` | `cs:51225` | With prefix. |
| `<number>` | `51225` | Plain number. |

### Labels

| Format | Example | Notes |
|--------|---------|-------|
| `lb:<name>` | `lb:5.4` | With prefix. |
| `<name>` | `stable-code`, `v5.4` | Plain name. |

### Shelves

| Format | Example | Notes |
|--------|---------|-------|
| `sh:<number>` | `sh:55465` | User-facing form with positive display number. |
| `<number>` | `55465` | User-facing plain number. Convert to the signed internal `changesetId` when calling shelve tools or building links. |

## Server Types

| Suffix | Type | Cloud project required? |
|--------|------|------------------------|
| `@unity` | Unity Cloud organization | Yes — must select an existing Unity Cloud project before creating a repo. |
| `@cloud` | Standalone cloud organization | No — repositories are created directly. |

## Repository Identifier Formats

The format depends on the server type:

| Server type | Format | Example |
|-------------|--------|---------|
| `@unity` | `project/repo@org@unity` | `MyGame/MyGame-repo@studioX@unity` |
| `@cloud` | `repo@org@cloud` | `MyGame-repo@studioX@cloud` |

**Rules:**
- For `@unity`, the separator between project and repository is `/`, NEVER `@`.
- `project` must be an existing Unity Cloud project confirmed by
  `Unity.UVCS.Execute(GetAllProjects)`. NEVER assume the local Unity project name exists
  as a Cloud project without verification.

## `@unity` Project Selection

For `@unity` organizations, a Unity Cloud project must be selected before any
repository operation. The full project-selection rules (auto-select defaults,
fallbacks, and user prompts) are defined in
[onboarding-workflows.md — Path A](onboarding-workflows.md#path-a-unity-organization).
Always follow those steps.

## Parameter Splitting

When calling tools, split the full identifier into its parts:

| Full identifier | `repositoryName` param | `server` param |
|-----------------|----------------------|----------------|
| `projName/repoName@orgName@unity` | `projName/repoName` | `orgName@unity` |
| `repoName@orgName@cloud` | `repoName` | `orgName@cloud` |

## Revisions

A specific version of a file at a point in time. Each checkin creates a new
revision per modified file.

Key properties of a `RevisionInfo`:
- **`Id`** — Unique revision identifier (numeric).
- **`ItemId`** — The item (file or directory) this revision belongs to. Multiple
  revisions can share the same `ItemId` — they represent different versions of
  the same file.
- **`Changeset`** — The changeset number this revision was committed in.
- **`BranchId`** — The internal ID of the branch where this revision was created.
- **`ParentId`** — The ID of the previous revision of the same item (the
  revision this one was derived from).
- **`Type`** — File type: `Text`, `Binary`, `Directory`, etc.
- **`Size`** — File size in bytes.
- **`LocalTimeStamp`** — Timestamp of when the revision was created.
- **`CheckedOut`** — `true` if this revision represents an uncommitted local
  change (revision ID < 0 or changeset = -1).

### Revision Identifiers

| Format | Example | Notes |
|--------|---------|-------|
| `revid:<number>` | `revid:48230` | By revision ID. |
| `rev:itemid:<itemId>#cs:<csetId>@<repo>@<server>` | `rev:itemid:105#cs:1042@MyRepo@org@cloud` | By item + changeset. |

## Annotations (Blame)

Per-line blame data. Key properties of an `AnnotatedLine`:
- **`Author`** — The user who last modified this line.
- **`Branch`** — The branch name where the modification happened.
- **`Revision`** — The revision spec string.
- **`AnnotateInfo.Content`** — The actual text content of the line.
- **`AnnotateInfo.Rev`** — The full `RevisionInfo` for this line's last change.
- **`AnnotateInfo.IsMergeDstRev`** -- `true` if the line arrived on the current
  branch via a merge rather than a direct edit.

## ClientDiff (Diff Results)

A `ClientDiff` is returned by the diff tools (`GetBranchDifferences`,
`GetChangesetDifferences`, `GetChangesetsDifferences`, `GetLabelDifferences`,
`GetShelveDifferences`). It contains metadata only -- use
`Unity.UVCS.Execute(DownloadRevisionsToFiles)` to fetch actual file content.

Key properties accessed via `Diff` (a `Difference` object):

| Property | Type | Description |
|----------|------|-------------|
| **`Diff.Path`** | string | Repository-relative file path. |
| **`Diff.Status`** | DiffNodeStatus | `Added` (0), `Deleted` (1), `Moved` (2), `Changed` (3). |
| **`Diff.IsDirectory`** | bool | `true` for directories -- skip these for file comparison. |
| **`Diff.Base`** | RevisionInfo | Base/left revision. `null` for `Added`. Use `Diff.Base.Id` for download. |
| **`Diff.RevInfo`** | RevisionInfo | New/right revision. `null` for `Deleted`. Use `Diff.RevInfo.Id` for download. |

For `Moved` entries, the `Difference` is a `DiffMoved` subclass with `SrcPath`
(old path) and `SrcRevInfo` (old revision).

## ChangeInfo (Pending Changes)

A `ChangeInfo` is returned by `GetPendingChanges`. Two sides to compare:

- **BEFORE** (base/server version): download using `RevInfo.Id` via
  `DownloadRevisionsToFiles`. This is what was in the repository before the user
  made changes.
- **AFTER** (current version): the live file on disk at `Path`. This is what
  the user has modified. Read it directly — do NOT download it.

| Property | Type | Description |
|----------|------|-------------|
| **`Path`** | string | Absolute workspace file path. This is the **AFTER** (current version on disk). |
| **`ChangeTypes`** | flags | `Added`, `Deleted`, `Moved`, `LocallyMoved`, `CheckedOut`, `Changed`, `LocallyDeleted`, `Private`. |
| **`RevInfo`** | RevisionInfo | The **BEFORE** (base/server revision). `null` for new/private files. Use `RevInfo.Id` to download it. |
| **`OldPath`** | string | Previous path (for moved files). |
| **`IsDirectory`** | bool | `true` for directories -- skip these. |

## ReviewInfo (Code Review)

Returned by `CreateCodeReviewForBranch`, `CreateCodeReviewForChangeset`, and
`GetReviewsInStatus`.

| Property | Type | Description |
|----------|------|-------------|
| **`Id`** | long | The numeric ID of the code review. Pass to `CreateCodeComment`, `CreateConversationComment`, `GetCodeReviewLink`, `GetReviewComments`, `ReplyComment`, `SetReviewStatus`, `AddCurrentUserAsReviewer`. |
| **`Title`** | string | The review title (auto-generated from the branch or changeset). |
| **`StatusId`** | int | Review status: `0` = Under Review, `1` = Reviewed, `2` = Rework Required. |
| **`TargetId`** | long | The branch ID or changeset ID the review targets. |

## CodeReviewCommentInfo (Review Comment)

Returned by `GetReviewComments`. The full list of comments — top-level
comments, replies, and timeline events — comes back as a flat list; use
`ParentCommentId` to reconstruct the tree.

| Property | Type | Description |
|----------|------|-------------|
| **`Id`** | long | Comment ID. Pass as `parentCommentId` to `ReplyComment`. |
| **`Type`** | enum | `Comment` (0), `Change` (1), `Question` (2), `Conversation` (3), `Timeline` (4), `Discarded` (6). |
| **`ParentCommentId`** | long | `-1` for top-level comments; otherwise the parent comment's `Id`. |
| **`Comment`** | string | The comment text. |
| **`GUID`** | string | Globally unique ID. The first 8 hex chars (before the first `-`) form the `[apply-change:<8-hex>]` checkin tag that auto-marks a change as Done. |
| **`RevisionId`** | long | Revision the comment is anchored to (for inline code comments). |
| **`ChangesetId`** | long | Changeset that contains the revision. |
| **`LocationSpec`** | string | **0-based** line number as a string. Add 1 to get the editor (1-based) line. |
| **`AppliedInChangesetId`** | long | `-1` if not yet applied; otherwise the changeset that applied the change. |

### Comment lifecycle (for the Address Review Comments workflow)

A top-level `Change` comment is **pending** until either:
- A checkin appears with the matching `[apply-change:<8-hex>]` tag — UVCS sets
  `AppliedInChangesetId` to that changeset and the change is Done; or
- A child reply with `Type == Discarded` is posted (via `ReplyComment` with
  `replyType=Discard`) — the change is rejected.

A top-level `Question` comment is **pending** until any child reply exists.
