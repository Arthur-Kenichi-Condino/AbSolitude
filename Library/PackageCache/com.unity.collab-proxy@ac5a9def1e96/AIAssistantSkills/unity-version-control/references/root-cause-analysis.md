# Root Cause Analysis

Workflow for tracing a bug, regression, or runtime error back to the changeset
that most likely introduced it. Combines version control history with Unity's
dependency graph and console logs to systematically narrow down the culprit.

## Table of Contents

- [Critical Rules](#critical-rules)
- [Confidence Definitions](#confidence-definitions)
- [Output Format](#output-format--mandatory)
- [Phase 0: Workspace Validation & Symptom Capture](#phase-0--workspace-validation--symptom-capture)
- [Phase 1: Identify Suspect Files](#phase-1--identify-suspect-files)
- [Phase 2: Gather Changeset History](#phase-2--gather-changeset-history)
- [Phase 3: Analyze & Rank Changesets](#phase-3--analyze--rank-changesets)
- [Phase 4: Present Findings](#phase-4--present-findings)
- [Phase 5: Deep Dive (Optional)](#phase-5--deep-dive-optional)
- [Example: User-Described Bug](#example-user-described-bug)
- [Example: Console Error Entry Point](#example-console-error-entry-point)
- [Notes](#notes)

---

## Critical Rules

1. **Always start with Phase 0.** Never skip workspace validation. Never guess
   UVCS state.

2. **Use UVCS tools for all version control queries.** Never use Git commands,
   even if the question sounds Git-like.

3. **Expand the suspect file set via dependencies.** After identifying the
   symptom file(s), ALWAYS call `Unity.GetDependency` with
   `direction: "outgoing"` to find what the broken file depends on. A change
   in a dependency can break its dependents. This is what makes cross-file
   root cause analysis possible.

4. **Bound the investigation.** Default time window is **4 weeks**. If the user
   provides a known-good date or changeset, use that as the lower bound. Never
   query unbounded history -- it produces too much noise.

5. **Rank by evidence, not speculation.** Every suspect changeset MUST have a
   concrete connection: it touched a file in the suspect set, or it modified a
   line that the stack trace references. Do NOT flag changesets without evidence.

6. **Cap at 10 suspect changesets.** If more candidates are found, present the
   top 10 by confidence and note how many were omitted. Offer to expand if
   the user asks.

7. **Each finding gets a unique ID.** Use `R1`, `R2`, `R3`, ... (R for Root
   cause suspect). The user can reference specific findings by ID in Phase 5.

8. **Clean up temp files.** If any revisions are downloaded during Phase 5 deep
   dives, ALWAYS call `Unity.UVCS.Execute(CleanupDiffTempFiles)` afterward. Never leave
   temp files behind.

9. **Never switch branches without explicit user confirmation.** Same safety
   rule as all other UVCS workflows.

10. **Use the exact output templates.** Follow the templates in this document
    exactly. Do NOT improvise formatting. Consistent output helps the user scan
    results quickly.

---

## Confidence Definitions

Use exactly these labels. Consistent classification lets the user triage
suspects quickly.

### 🔴 HIGH

Strong evidence this changeset introduced the bug. The changeset modified the
exact line(s) referenced in the stack trace or symptom description, AND the diff
shows a logic change that directly explains the failure (e.g., removed a null
check, changed a visibility modifier, altered a calculation).

### 🟡 MEDIUM

Likely contributor. The changeset modified a file in the suspect set within the
relevant time window, and the changes are semantically related to the symptom
(e.g., modified the same method, changed an API signature used by the affected
code, altered a shared data structure).

### ⚪ LOW

Possible but uncertain. The changeset touched a related file or a dependency of
the suspect files, but the connection to the symptom is indirect or the change
appears minor (e.g., renaming, formatting, adding unrelated methods to a shared
file).

---

## Output Format -- MANDATORY

**YOU MUST USE THIS EXACT FORMAT. The goal is a compact, scannable layout
that works well in narrow chat panels. Do NOT use dense multi-line blocks
for findings -- use the table format below.**

### Header

A compact 2-line summary. First line: scope. Second line: confidence tally
with emoji indicators. Only include levels that have at least one finding:

```
🔍 Examined <N> changesets across <N> files (<date_from> – <date_to>).

🔴 HIGH (<N>)   🟡 MEDIUM (<N>)   ⚪ LOW (<N>)
```

### Findings Table

Present all suspect changesets in a **single table**. This is the most
important part of the output -- keep it compact and scannable:

```
| | Changeset | Author | What changed |
|---|-----------|--------|-------------|
| 🔴 **R1** | [cs:1205](uvcs://show/changeset/1205) · Apr 16 | @alice | Made `PlayerPosition` private — `EnemyAI.cs` reads this to chase the player |
| 🟡 **R2** | [cs:1210](uvcs://show/changeset/1210) · Apr 18 | @bob | Refactored state machine in `EnemyAI.cs` — could break chase transitions |
| ⚪ **R3** | [cs:1208](uvcs://show/changeset/1208) · Apr 17 | @carol | Added `DashAbility()` to `PlayerController.cs` — unrelated to chase logic |
```

**Table rules:**
- Column 1: confidence emoji + finding ID. The emoji IS the confidence
  indicator — no text label needed. 🔴 = HIGH, 🟡 = MEDIUM, ⚪ = LOW.
- Column 2: changeset number (as a `uvcs://show/changeset/<id>` clickable
  link) and date.
- Column 3: author (short form, `@firstname` or `@username`).
- Column 4: **one sentence** combining what changed and why it matters.
  Keep it under ~100 characters. Be specific: name the exact symbol, method,
  or property. "Modified a related file" is NOT acceptable. "Made
  `PlayerPosition` private — `EnemyAI.cs` reads this to chase the player"
  IS acceptable.
- Sort: 🔴 first, then 🟡, then ⚪.
- Max 4 columns. Never add more.

### Next Steps Menu

After the table, present a **short inline list** (not a table):

```
➡️ "Diff R1" · "Annotate EnemyAI.cs:34" · "Before R1" · "Done"
```

Keep it on one or two lines. The commands are self-explanatory — no
descriptions needed.

---

## Phase 0 -- Workspace Validation & Symptom Capture

### Step 0.1 -- Validate workspace

1. Call `Unity.UVCS.Execute(GetWorkspaceFromPath)` with `Application.dataPath`.
   If no workspace found, inform the user and STOP.
2. Call `Unity.UVCS.Execute(GetWorkspaceConfiguration)()` to get the
   current selector (branch, changeset).
3. Silent unless there is a problem.

### Step 0.2 -- Determine entry point

The workflow supports two entry points. If the user's request makes it clear
which one applies, proceed directly. If ambiguous, use
`Unity.UVCS.Execute(SelectOption)`:

- **Title:** `"How would you like to start the investigation?"`
- **Message:** `"Choose how to identify the symptom."`
- **Options:**
  - **Label:** `"Describe the bug"`,
    **Description:** `"I'll ask you to describe the symptom, affected files, and when it last worked."`
  - **Label:** `"Check console errors"`,
    **Description:** `"I'll capture current Unity console errors and use the stack traces to identify affected files."`

#### Entry Point A -- User describes a bug

Extract from the user's description:
- **Affected files** -- any file paths, script names, class names, or method
  names mentioned.
- **Symptom** -- what is broken (e.g., "enemies don't chase", "player falls
  through floor", "null reference in Update").
- **Known-good reference** -- any mention of when it last worked (e.g., "it
  worked last Tuesday", "it was fine before changeset 50", "before we merged
  the AI branch"). Convert relative dates to absolute dates.

If the user did not mention specific files, ask:
`"Can you point me to a specific script, file, or class name related to this
bug? Or should I check the console for errors?"`

If the user redirects to console, switch to Entry Point B.

#### Entry Point B -- Console errors

1. Call `Unity.GetConsoleLogs(logTypes: "error", maxEntries: 50, includeStackTrace: true)`.
2. If no errors found, inform the user:
   `"No errors in the Unity console. Can you describe the bug instead?"`
   Switch to Entry Point A.
3. If errors found, group them by root file (the first user-code file in each
   stack trace, ignoring Unity internals like `UnityEngine.*` or
   `UnityEditor.*` frames).
4. If **one distinct error**: use it directly.
5. If **multiple distinct errors**: present them and let the user choose which
   to investigate using `Unity.UVCS.Execute(SelectFromList)`:
   - **Title:** `"Multiple errors found"`
   - **Message:** `"Select the error you want to investigate."`
   - **Label:** `"Error"`
   - **Choices:** one entry per distinct error, formatted as
     `"<ErrorType> in <FileName>:<Line> — <first line of message>"`

### Step 0.3 -- Establish time bounds

- If user provided a known-good changeset: lower bound = that changeset number.
- If user provided a known-good date (e.g., "last Tuesday"): use
  `Unity.UVCS.Execute(FindQuery)` to find the latest changeset before that date on the
  current branch:
  ```
  find changeset where branch = '<current_branch>' and date < '<known_good_date>' order by date desc limit 1
  ```
  Use that changeset number as the lower bound.
- If no known-good reference: default to `'4 weeks ago'`.
- Upper bound: always the current workspace changeset (from
  `GetWorkspaceConfiguration`).

### Step 0.4 -- Present investigation header

Output exactly:

```
🔍 **Investigating:** <one-line symptom description>
📁 <short file names> · 🔀 <branch_name> · 📅 <date_from> – <date_to>

Analyzing dependencies and changeset history...
```

Keep it to 2 lines max. Use short file names (e.g., `EnemyAI.cs` not
`Assets/Scripts/EnemyAI.cs`) for readability. The full paths are used
internally but not shown to the user here.

---

## Phase 1 -- Identify Suspect Files

This phase expands from the symptom files to their dependencies, so we catch
cases where "file A changed and broke file B."

### Step 1.1 -- Start with symptom files

Use the files identified in Phase 0 (from user description or stack trace).

### Step 1.2 -- Expand via dependency graph

For each symptom file, call:

```
Unity.GetDependency(queryType: "get_edges", filePath: "<file>", direction: "outgoing")
```

This returns what the symptom file **depends on**. If any of those dependencies
were recently changed, they could be the root cause.

Focus on these relationship types in the results:
- `dependsOn` -- asset/scene dependencies
- `inheritsFrom` -- base class changes can break subclasses
- `implements` -- interface changes can break implementations
- `uses` -- type usage (return types, parameters)

Add the dependency files to the suspect set.

### Step 1.3 -- Expand via text search (optional)

If the symptom involves a specific class name, method, or variable that might
not be captured by the pre-indexed dependency graph (e.g., reflection-based
access, string references, or dynamic lookups):

```
Unity.Grep(pattern: "<class_or_method_name>", type: "cs")
```

Add any additional files found to the suspect set.

### Step 1.4 -- Cap and confirm the suspect set

- Cap the suspect set at **20 files**. Prioritize: symptom files first, then
  direct dependencies (outgoing edges), then text-search matches.
- If the suspect set has **more than 10 files**, present a table and ask
  the user to confirm before proceeding:

```
The investigation will cover <N> files:

| # | File | Relationship |
|---|------|-------------|
| 1 | Assets/Scripts/EnemyAI.cs | Symptom (from stack trace) |
| 2 | Assets/Scripts/PlayerController.cs | Dependency (EnemyAI uses PlayerController) |
| 3 | Assets/Scripts/AIStateMachine.cs | Dependency (EnemyAI inherits from AIStateMachine) |
| ... | ... | ... |

Continue with all <N> files, or narrow the set?
```

Use `Unity.UVCS.Execute(SelectOption)` with:
- **Label:** `"Continue with all"`, **Description:** `"Investigate all <N> files."`
- **Label:** `"Narrow the set"`, **Description:** `"I'll tell you which files to focus on."`

If the user narrows, wait for their input and adjust the suspect set.

- If the suspect set has **10 or fewer files**, proceed silently.

---

## Phase 2 -- Gather Changeset History

### Step 2.1 -- Query changesets for each suspect file

For each file in the suspect set, query its revision history within the time
bounds:

```
find revision where item = '<file_path>' and date > '<lower_bound_date>' order by changeset desc
```

If a changeset-based lower bound is available instead of a date:

```
find revision where item = '<file_path>' and changeset > <lower_bound_cs> order by changeset desc
```

### Step 2.2 -- Deduplicate changesets

Multiple suspect files may have been modified in the same changeset. Merge into
a unique changeset list, noting which suspect files each changeset touched.

### Step 2.3 -- Get changeset metadata

For each unique changeset, get its details:

```
find changeset where changesetid = <N>
```

This gives: author, date, comment, branch. The **comment** is crucial -- it
often explains the intent of the change.

### Step 2.4 -- Cap at 10

If more than 10 unique changesets are found, keep the 10 most likely candidates
using this priority:
1. Changesets that touch **symptom files** directly (highest priority).
2. Changesets that touch **multiple** suspect files.
3. Most recent changesets (recency bias -- recent changes are more likely to be
   the cause of recently discovered bugs).

Note how many were omitted:
`"Found <N> candidate changesets. Showing the top 10. Say 'show all' to see the rest."`

---

## Phase 3 -- Analyze & Rank Changesets

### Step 3.1 -- Score each changeset

For each candidate changeset, assign a confidence level based on these criteria:

**HIGH -- requires ALL of:**
- Changeset touches a symptom file directly (the file from the stack trace or
  user description).
- The changeset comment or diff metadata suggests a logic change (not just
  formatting, renaming, or comments).
- The change is semantically related to the symptom (e.g., changeset modifies
  the same method or property mentioned in the error).

**MEDIUM -- requires ANY of:**
- Changeset touches a dependency of the symptom file (found via
  `Unity.GetDependency` in Phase 1), and the change is semantically related.
- Changeset touches the symptom file but the change appears minor or
  tangentially related (e.g., modified a different method in the same class).

**LOW -- everything else in the candidate list:**
- Changeset touches a suspect file but the connection is indirect.
- Changeset is a large refactor that touched many files including a suspect.

### Step 3.2 -- Line-level confirmation (HIGH suspects only)

If a stack trace with specific line numbers is available from Phase 0, perform
line-level annotation on the symptom file to check if a suspect changeset
modified the exact error line:

1. Call `Unity.UVCS.Execute(GetFileAnnotations)(filePath)` on the
   symptom file.
2. Find the annotation for the error line number.
3. Check if `AnnotateInfo.Rev.Changeset` matches any of the HIGH suspects.

If yes, this is the **strongest signal**: the changeset that last modified the
exact line where the error occurs. Promote to HIGH if not already, and add this
to the evidence.

If the annotation points to a changeset that is NOT in the candidate list (e.g.,
it predates the time window), note this -- the bug may be older than expected.
Inform the user: `"The error line was last modified in cs:<N> (<date>), which is
outside the investigation window. The bug may be older than expected."`

### Step 3.3 -- Build the findings list

Sort candidates: HIGH first, then MEDIUM, then LOW. Assign IDs: `R1`, `R2`,
`R3`, ...

---

## Phase 4 -- Present Findings

Output the findings using the mandatory format (header + table + next steps
menu). The entire output should be compact enough to scan in a narrow chat
panel without scrolling.

After the table, ALWAYS present the next steps menu on a single line:

```
➡️ "Diff R1" · "Annotate <file>:<line>" · "Before R1" · "Done"
```

Replace `R1` and `<file>:<line>` with actual values from the findings. If a
stack trace line is known, include it as a concrete example (e.g.,
`"Annotate EnemyAI.cs:34"`). Wait for the user's response before proceeding.

**Special cases:**

- **No candidates found** (no changesets touched the suspect files in the time
  window): Report this clearly and suggest expanding the window:
  `"No changesets modified the suspect files in the last 4 weeks. Would you like
  to expand the search window to 3 months?"`
  If the user agrees, repeat Phase 2 with the wider window.

- **Single obvious candidate** (one HIGH-confidence changeset, no others):
  Present the table with the single entry, then offer to diff immediately:
  `"One strong candidate. Want me to show what changed?"`

---

## Phase 5 -- Deep Dive (Optional)

Triggered when the user selects a command from the Phase 4 menu. Each action
follows the pattern: execute, present results, re-present the next steps line.

After every deep-dive action, end with:
```
➡️ "Diff R2" · "Annotate <file>:<line>" · "Before R2" · "Done"
```
(Update the IDs/files to reflect what's still relevant.)

### "Diff RN"

Show the content-level diff for the selected changeset, focused on the suspect
files:

1. Call `Unity.UVCS.Execute(GetChangesetDifferences)(changesetId)` to get
   the file list.
2. Filter to the suspect files (skip files not in the suspect set unless the
   user asks to see all).
3. Filter out directories and binary files.
4. For each text file:
   - `Changed`: download both `Diff.Base.Id` (BEFORE) and `Diff.RevInfo.Id`
     (AFTER) using `Unity.UVCS.Execute(DownloadRevisionsToFiles)`.
   - `Added`: download `Diff.RevInfo.Id` only.
   - `Deleted`: download `Diff.Base.Id` only.
5. Read the files and present a content-level summary: what lines/sections were
   added, removed, or modified. Label values as **Before** and **After**.
6. Call `Unity.UVCS.Execute(CleanupDiffTempFiles)()`. **MANDATORY.**
7. Re-present the next steps line.

### "Annotate <file>:<line>"

Show per-line blame for a specific file and line range:

1. Call `Unity.UVCS.Execute(GetFileAnnotations)(filePath)`.
2. Extract the annotation for the specified line (or line range).
3. Report: author, changeset, branch, and date for those lines.
4. Cross-reference with the suspect list: if the annotated changeset matches
   a finding, call it out explicitly (e.g., `"This line was last modified in
   cs:1205, which is finding R1."`).
5. Get the changeset comment:
   ```
   find changeset where changesetid = <N>
   ```
6. Report the comment to explain **why** the change was made.
7. Re-present the next steps line.

### "Before RN"

Show the state of the symptom file just before the selected changeset modified
it. This helps the user see what the code looked like when it was still working:

1. Get the changeset number from the selected finding.
2. Find the file's revision just before that changeset:
   ```
   find revision where item = '<file_path>' and changeset < <changeset_number> order by changeset desc limit 1
   ```
3. Call `Unity.UVCS.Execute(GetFileAnnotations)(filePath, revisionId)`
   with the previous revision ID.
4. Present the relevant lines (around the error line if known, or the full file
   if short).
5. Compare with the current state and highlight what changed.
6. Re-present the next steps line.

### "Done"

End the investigation. Output a compact wrap-up:

```
✅ **Most likely cause:** [R1] [cs:<number>](uvcs://show/changeset/<number>) by @<author> — <one-line evidence>
```

If no HIGH-confidence suspect was found:

```
🟡 **Best lead:** [R1] [cs:<number>](uvcs://show/changeset/<number>) (MEDIUM) — <evidence>
No definitive root cause found. Try expanding the time window or providing
more details about when the bug first appeared.
```

---

## Example: User-Described Bug

**User:** "The enemy AI stopped chasing the player. It was working last
Tuesday."

### Phase 0

1. `GetWorkspaceFromPath` -> workspace at `/Users/dev/MyGame`.
2. `GetWorkspaceConfiguration` -> branch `/main`, changeset 1215.
3. Symptom: enemy AI stopped chasing the player.
4. Known-good: last Tuesday (2026-04-15).
5. Time bounds: `date > '2026-04-15'`, upper = cs:1215.
6. User mentioned "enemy AI" but no specific file.
7. Ask: "Can you point me to a specific script related to this bug?"
8. User: "Check EnemyAI.cs"

Output:
```
🔍 **Investigating:** Enemy AI stopped chasing the player
📁 EnemyAI.cs · 🔀 /main · 📅 Apr 15 – today

Analyzing dependencies and changeset history...
```

### Phase 1

1. Symptom file: `Assets/Scripts/EnemyAI.cs`.
2. `Unity.GetDependency("get_edges", "Assets/Scripts/EnemyAI.cs", "outgoing")`:
   - `Assets/Scripts/PlayerController.cs` (uses `PlayerController.PlayerPosition`)
   - `Assets/Scripts/AIStateMachine.cs` (inheritsFrom)
3. Suspect set (3 files): `EnemyAI.cs`, `PlayerController.cs`,
   `AIStateMachine.cs`. Under 10 -- proceed silently.

### Phase 2

1. Query revisions for each file since 2026-04-15:
   - `EnemyAI.cs` -> cs:1210 (Bob, "Refactor AI state machine")
   - `PlayerController.cs` -> cs:1205 (Alice, "Make PlayerPosition private"),
     cs:1208 (Carol, "Add dash ability")
   - `AIStateMachine.cs` -> no changes in window
2. Deduplicate: 3 unique changesets (cs:1205, cs:1208, cs:1210).
3. Get metadata for each.

### Phase 3

1. **cs:1205** (Alice): Changed `PlayerPosition` from `public` to `private`.
   `EnemyAI.cs` uses `PlayerController.PlayerPosition` to locate the player.
   If the API broke, enemies can't chase. -> **HIGH**
2. **cs:1210** (Bob): Refactored the state machine in `EnemyAI.cs` directly.
   -> **MEDIUM** (touched symptom file, but "refactor" could be benign)
3. **cs:1208** (Carol): Added dash ability to `PlayerController.cs`, but
   the diff metadata shows the changes are in `DashAbility()`, unrelated to
   `PlayerPosition`. -> **LOW**

### Phase 4

```
🔍 Examined 3 changesets across 3 files (Apr 15 – today, branch /main).

🔴 HIGH (1)   🟡 MEDIUM (1)   ⚪ LOW (1)

| | Changeset | Author | What changed |
|---|-----------|--------|-------------|
| 🔴 **R1** | [cs:1205](uvcs://show/changeset/1205) · Apr 16 | @alice | Made `PlayerPosition` private — `EnemyAI.cs` reads this to chase the player |
| 🟡 **R2** | [cs:1210](uvcs://show/changeset/1210) · Apr 18 | @bob | Refactored state machine in `EnemyAI.cs` — could break chase transitions |
| ⚪ **R3** | [cs:1208](uvcs://show/changeset/1208) · Apr 17 | @carol | Added `DashAbility()` to `PlayerController.cs` — unrelated to chase logic |

➡️ "Diff R1" · "Annotate EnemyAI.cs:42" · "Before R1" · "Done"
```

**User:** "Diff R1"

### Phase 5

1. `GetChangesetDifferences(wkPath, 1205)` -> `PlayerController.cs` (Changed).
2. Download BEFORE (Base.Id) and AFTER (RevInfo.Id).
3. Read and compare:

```
**PlayerController.cs** (cs:1205 by @alice):

**Before:**
`public static Vector3 PlayerPosition;`

**After:**
`private static Vector3 PlayerPosition;`

This change made `PlayerPosition` invisible to other scripts. Since
`EnemyAI.cs` accesses `PlayerController.PlayerPosition` to locate the player
for chasing behavior, this change directly broke the enemy AI.
```

4. `CleanupDiffTempFiles(wkPath)`.
5. Output: `➡️ "Diff R2" · "Before R1" · "Done"`

---

## Example: Console Error Entry Point

**User:** "Check console errors and find what caused them."

### Phase 0

1. `GetWorkspaceFromPath` -> workspace info.
2. `GetWorkspaceConfiguration` -> branch `/main/enemy-ai`, changeset 1220.
3. `Unity.GetConsoleLogs(logTypes: "error", maxEntries: 50, includeStackTrace: true)`:
   ```
   3 errors:
   - NullReferenceException at EnemyAI.Update() (EnemyAI.cs:34)
   - NullReferenceException at EnemyAI.Update() (EnemyAI.cs:34)
   - NullReferenceException at EnemyAI.Update() (EnemyAI.cs:34)
   ```
4. Group by root file: 1 distinct error (`NullReferenceException` in
   `EnemyAI.cs:34`). Single error, use directly.
5. No known-good reference -> default to 4 weeks.

Output:
```
🔍 **Investigating:** NullReferenceException in EnemyAI.Update() at line 34
📁 EnemyAI.cs · 🔀 /main/enemy-ai · 📅 last 4 weeks

Analyzing dependencies and changeset history...
```

### Phase 1

1. Symptom file: `Assets/Scripts/EnemyAI.cs`.
2. `Unity.GetDependency("get_edges", "Assets/Scripts/EnemyAI.cs", "outgoing")`:
   - `Assets/Scripts/PlayerController.cs` (uses `PlayerController`)
3. Suspect set: `EnemyAI.cs`, `PlayerController.cs`.

### Phase 2

1. Query revisions in last 4 weeks:
   - `EnemyAI.cs` -> cs:1218 (Dave, "Add patrol waypoints")
   - `PlayerController.cs` -> cs:1215 (Eve, "Remove unused fields")
2. Two candidate changesets.

### Phase 3

1. **Line-level confirmation**: call `GetFileAnnotations` for `EnemyAI.cs`.
   Line 34 was last modified in **cs:1218** (Dave). This matches candidate
   cs:1218 -> **HIGH**.
2. **cs:1215** (Eve): Modified a dependency, but "Remove unused fields" could
   have removed something `EnemyAI.cs:34` uses -> **MEDIUM**.

### Phase 4

```
🔍 Examined 2 changesets across 2 files (last 4 weeks, branch /main/enemy-ai).
Line-level annotation confirms cs:1218 last modified the exact error line.

🔴 HIGH (1)   🟡 MEDIUM (1)

| | Changeset | Author | What changed |
|---|-----------|--------|-------------|
| 🔴 **R1** | [cs:1218](uvcs://show/changeset/1218) · Apr 19 | @dave | Last to modify `EnemyAI.cs:34` — the exact NullRef line (confirmed via annotate) |
| 🟡 **R2** | [cs:1215](uvcs://show/changeset/1215) · Apr 17 | @eve | Removed fields from `PlayerController.cs` — could break `EnemyAI.cs` references |

➡️ "Diff R1" · "Annotate EnemyAI.cs:34" · "Before R1" · "Done"
```

---

## Notes

- **Binary files** (textures, materials, prefabs, scenes): these appear in
  changeset diffs as metadata but cannot be meaningfully analyzed for code-level
  root cause. Mention them in findings if relevant (e.g., "cs:1210 also modified
  EnemyPrefab.prefab") but do not attempt deep analysis.

- **Performance of GetFileAnnotations**: this tool is slow. Only use it in
  Phase 3 Step 3.2 when a stack trace with line numbers is available, and in
  Phase 5 for explicit user-requested annotate/before commands. Do NOT annotate
  every file in the suspect set preemptively.

- **Multiple branches**: if the current branch has recent merges, some
  changesets may originate from other branches. The changeset metadata includes
  the branch field -- use it to help the user understand whether the bug was
  introduced on the current branch or merged in.

- **Expanding the time window**: if no candidates are found in the default
  4-week window, offer to expand to 3 months. If still nothing, the bug may
  predate the user's awareness or be caused by external factors (Unity version
  upgrade, package update, etc.).

- **False positives**: if the user says "that's not it" about a finding, remove
  it from consideration and re-rank the remaining candidates. Do NOT argue with
  the user about confidence levels.

- **GetFileAnnotations returns checkout lines**: if the file has locally
  pending (uncommitted) changes, annotated lines will show
  `AnnotateInfo.Rev.CheckedOut = true` and `Changeset = -1` for those lines.
  This means the user themselves modified that line in their current workspace.
  Note this when relevant: "Line 34 has a local (uncommitted) modification --
  the bug may be in your current changes."
