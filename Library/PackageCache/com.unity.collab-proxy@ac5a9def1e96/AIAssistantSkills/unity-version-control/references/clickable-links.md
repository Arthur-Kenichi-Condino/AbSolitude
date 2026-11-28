# Clickable Links (`uvcs://`)

## Table of Contents

- [Rules](#rules)
- [Available Link Patterns](#available-link-patterns)
- [Examples](#examples)

The chat supports clickable `uvcs://` links that trigger UI actions when the
user clicks them. **You MUST use these links whenever you mention a UVCS
entity or suggest a UI action.** This makes your responses interactive and
lets the user navigate with a single click.

## Rules

- **ALWAYS use markdown link syntax `[display text](url)`.** NEVER write a
  bare `uvcs://` URL as text. The URL must ONLY appear inside the parentheses
  of a markdown link, NEVER as visible text in your response.
  - **WRONG:** `uvcs:///main` or `uvcs://show/branch/main` as visible text.
  - **RIGHT:** `[/main](uvcs://show/branch/main)` — the user sees `/main`
    as a clickable link.
- **NEVER construct URLs by appending entity names directly to `uvcs://`.**
  The URL structure is ALWAYS `uvcs://<action>/<type>/<identifier>`.
  - **WRONG:** `uvcs:///main/seb` (three slashes, no action or type).
  - **RIGHT:** `uvcs://show/branch/main/seb` (action=show, type=branch).
- **ALWAYS wrap changeset IDs, branch names, shelve IDs, and label names in
  clickable links** when mentioning them in your responses. **Every single
  mention** must be a link — including in bullet lists, tables, and prose.
  - Write `[Changeset 42](uvcs://show/changeset/42)`, NOT just "Changeset 42".
  - Write `[/main/task001](uvcs://show/branch/main/task001)`, NOT just "/main/task001".
  - Write `[shelve 54](uvcs://show/shelve/-54)` when the shelve
    `ChangesetId` is `-54`, NOT just "shelve 54".
  - **This applies in ALL contexts:** headings, bullet lists, table cells,
    inline prose, summaries, history lists — no exceptions.
- **Use `changesetId`/`ChangesetId`, not internal `ID`/`Id`, for changeset and
  shelve links.** UVCS query results can include both. `ID` is an internal
  object ID and should not be displayed as the changeset/shelve number.
- **Shelve `changesetId` values are negative internally.** Hide that detail
  from the user by displaying the positive absolute value, but keep the signed
  negative value inside `uvcs://` links and tool parameters. For example,
  `ChangesetId = -54` should be displayed as
  `[shelve 54](uvcs://show/shelve/-54)`.
- **Use action links when suggesting an action the user can take.** For
  example, instead of "you can now merge branch /main/task001", write
  "you can now [merge /main/task001](uvcs://merge/branch/main/task001)".
- **Branch names in links omit the leading `/`** — the handler adds it.
  So branch `/main/task001` becomes `uvcs://show/branch/main/task001`.
- **File paths in `show/history` links are project-relative** (the same form
  used by file tools and shown to the user). Do NOT use absolute paths.
  - **WRONG:** `uvcs://show/history//Users/me/Project/Assets/Foo.cs`
  - **RIGHT:** `uvcs://show/history/Assets/Foo.cs`
- **Schema note: `show/`, `merge/` and `open/` use DIFFERENT shapes — they
  are NOT interchangeable.**
  - `show/<entityType>/<id>` and `merge/<entityType>/<id>` use **lowercase
    singular** entity types: `branch`, `changeset`, `shelve`, `history`.
  - `open/<ViewType>` and `refresh/<ViewType>` use the **PascalCase
    `ViewType` enum name** as a single segment, with no extra `view/`
    segment and no kebab-case. Valid values: `PendingChangesView`,
    `IncomingChangesView`, `ChangesetsView`, `ShelvesView`, `BranchesView`,
    `LabelsView`, `LocksView`, `BranchExplorerView`. Anything else is
    invalid.
- **If no defined link action exists for the verb you want to express, do
  NOT wrap it in a link — write it as plain text instead.** The "wrap every
  entity in a link" rule applies to entities that already exist (a known
  changeset, branch, shelve, or label). It does NOT authorize you to invent
  a link for an action that has no entry in the link table.
  - There is **no** `uvcs://` action for *creating* a new shelve, checkin,
    branch, or label. Phrases like "shelve these changes", "checkin",
    "create branch X" must be plain text. Never invent placeholder IDs
    like `-1` to satisfy the linking rule.
  - If you want to point the user somewhere actionable, link to the
    relevant view instead, e.g.
    `[pending changes](uvcs://open/PendingChangesView)`.

### Common Mistakes to Avoid

| Wrong | Why it's wrong | Correct |
|-------|---------------|---------|
| `Changeset 107: Fixed bug` | Changeset ID is plain text, not a link | `[Changeset 107](uvcs://show/changeset/107): Fixed bug` |
| `uvcs:///main` | No action/type, three slashes, bare URL | `[/main](uvcs://show/branch/main)` |
| `uvcs:///main/seb` | No action/type, three slashes, bare URL | `[/main/seb](uvcs://show/branch/main/seb)` |
| `Merged from uvcs:///main.` | Bare URL used as display text | `Merged from [/main](uvcs://show/branch/main).` |
| `I reviewed branch uvcs:///main/seb` | Bare URL used as branch name | `I reviewed branch [/main/seb](uvcs://show/branch/main/seb)` |
| `[Changeset 987654](uvcs://show/changeset/987654)` when the result also has `changesetId = 107` | Uses internal `ID` instead of visible changeset ID | `[Changeset 107](uvcs://show/changeset/107)` |
| `[shelve -54](uvcs://show/shelve/-54)` | Shows the internal negative shelve value to the user | `[shelve 54](uvcs://show/shelve/-54)` |
| `[apply it](uvcs://apply/shelve/-54)` | `apply` is not a valid `uvcs://` action | `[apply it](uvcs://merge/shelve/-54)` |
| `[shelve these changes](uvcs://merge/shelve/-1)` | No shelve exists yet — `merge/shelve/<id>` *applies* an existing shelve. There is no link action for *creating* a shelve. Inventing `-1` as a placeholder is wrong. | Plain text: `I can shelve these changes` (no link). Optionally point to `[pending changes](uvcs://open/PendingChangesView)`. |
| `uvcs://open/view/branch-explorer` | `open/` takes a single PascalCase `ViewType` segment. There is no `view/` segment, and view names are never kebab-case. | `uvcs://open/BranchExplorerView` |
| `uvcs://open/view/pending-changes` | Same as above — invented `view/` segment and kebab-case view name. | `uvcs://open/PendingChangesView` |
| `uvcs://open/shelves` | Lowercase entity name borrowed from the `show/` schema. `open/` requires the full `ViewType` enum name. | `uvcs://open/ShelvesView` |

## Available Link Patterns

**`show` — Navigate to a view and select a specific item:**

| Link | Action |
|------|--------|
| `uvcs://show/changeset/<id>` | Open Changesets view with changeset selected |
| `uvcs://show/branch/<name>` | Open Branches view with branch selected |
| `uvcs://show/shelve/<id>` | Open Shelves view with shelve selected. For shelves from query results, use the signed `changesetId` internally; display its absolute value. |
| `uvcs://show/history/<filePath>` | Open file history for a specific file. `<filePath>` is **project-relative** (e.g. `Assets/Scripts/Player.cs`). |

**`open` — Open a UVCS view tab (no item selection):**

| Link | Action |
|------|--------|
| `uvcs://open/PendingChangesView` | Open Pending Changes tab |
| `uvcs://open/IncomingChangesView` | Open Incoming Changes tab |
| `uvcs://open/ChangesetsView` | Open Changesets tab |
| `uvcs://open/ShelvesView` | Open Shelves tab |
| `uvcs://open/BranchesView` | Open Branches tab |
| `uvcs://open/LabelsView` | Open Labels tab |
| `uvcs://open/LocksView` | Open Locks tab |
| `uvcs://open/BranchExplorerView` | Open Branch Explorer window |

**`refresh` — Refresh a view's data:**

| Link | Action |
|------|--------|
| `uvcs://refresh/<ViewType>` | Refresh a view (e.g. `refresh/PendingChangesView`) |

**`merge` — Launch merge/apply UI:**

| Link | Action |
|------|--------|
| `uvcs://merge/branch/<name>` | Launch merge from branch |
| `uvcs://merge/changeset/<id>` | Launch merge from changeset |
| `uvcs://merge/shelve/<id>` | Launch apply shelve UI. For shelves from query results, use the signed `changesetId` internally; display its absolute value. |

## Examples

- Checkin result: "Created [changeset 128](uvcs://show/changeset/128)
  on branch [/main/task001](uvcs://show/branch/main/task001)."
- Shelve result: "Changes shelved as [shelve 54](uvcs://show/shelve/-54).
  When you're ready, you can [apply it](uvcs://merge/shelve/-54)."
- Branch creation: "Created branch
  [/main/feature-x](uvcs://show/branch/main/feature-x)."
- Merge suggestion: "You can
  [merge /main/task001](uvcs://merge/branch/main/task001)
  when you're ready."
- View navigation: "Check your
  [pending changes](uvcs://open/PendingChangesView) to review what will
  be checked in."
- File history: "See the
  [history of Cube-renamed.mat](uvcs://show/history/Assets/Scenes/ManualTesting/Assets/Cube-renamed.mat)
  for past changes."
- Branch history list:
  - "[Changeset 107](uvcs://show/changeset/107): Initial updates to
    manifest and Project Settings."
  - "[Changeset 112](uvcs://show/changeset/112): Updated organization ID."
  - "[Changeset 113](uvcs://show/changeset/113): Merged latest changes
    from [/main](uvcs://show/branch/main)."
