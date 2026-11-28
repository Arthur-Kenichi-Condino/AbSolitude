# Annotate Workflows

Workflows using UVCS file annotations to answer questions about authorship and
change attribution.

For the underlying data model (`AnnotateInfo`, `AnnotatedLine`), see
[data-model.md — Annotations](data-model.md#annotations-blame).

## Table of Contents

- [Critical Rules](#critical-rules)
- [Workflow 1: Basic File Annotate](#workflow-1-basic-file-annotate)
- [Workflow 2: Who Made a Specific Change / Why Was Something Changed](#workflow-2-who-made-a-specific-change--why-was-something-changed)
- [Workflow 3: Find the Branch Where Something Was Added](#workflow-3-find-the-branch-where-something-was-added)
- [Workflow 4: When Was a File Last Modified](#workflow-4-when-was-a-file-last-modified)
- [Workflow 5: Find the Most Active Contributors for a File](#workflow-5-find-the-most-active-contributors-for-a-file)
- [Workflow 6: Trace the Full History of a Line or Section](#workflow-6-trace-the-full-history-of-a-line-or-section)
- [Workflow 7: Identify Lines Changed via Merge](#workflow-7-identify-lines-changed-via-merge)
- [Workflow 8: Annotate Previous Version of a File](#workflow-8-annotate-previous-version-of-a-file)
- [Workflow 9: Annotate Before a Specific Change](#workflow-9-annotate-before-a-specific-change)

## Critical Rules

1. **`Unity.UVCS.Execute(GetFileAnnotations)` is slow and resource-intensive.** Only
   call it when you genuinely need per-line metadata (who changed a specific
   line, which branch introduced a particular piece of content). For
   file-level questions, prefer:
   - `Unity.UVCS.Execute(FindQuery)` — e.g.
     `find revision where item = '...'` or `find changeset where ...`
   - `Unity.UVCS.Execute(GetFileHistory)` — full revision history, no per-line cost
   - `Unity.UVCS.Execute(GetRevisionInfo)` — single revision metadata

2. **For the "previous version" workflow (Workflow 8) do NOT annotate the
   current version first to find the previous revision.** Use
   `Unity.UVCS.Execute(FindQuery)` with `find revision where item = '...' order by
   changeset desc limit 2` to get the parent revision ID directly — much
   faster.

3. **Group consecutive lines with the same author/changeset** when presenting
   results. A 200-line file with 3 distinct contributors should produce 3 (or
   a few) blocks, not 200 entries.

## Workflow 1: Basic File Annotate

Use this workflow when the user asks to annotate or blame a file, or wants to
see per-line authorship information.

### Steps

1. **Get the workspace** by calling `Unity.UVCS.Execute(GetWorkspaceFromPath)` with
   `Application.dataPath`.
2. **Identify the file path.** The user may provide:
   - A project-relative path (e.g. `Assets/Scripts/Player.cs`) — use it
     directly.
   - A partial name (e.g. `Player.cs`) — search for the file first using
     file-system tools to obtain the project-relative path.
   - A reference to a selected GameObject or component — determine the script
     path from the asset reference.
3. **Call `Unity.UVCS.Execute(GetFileAnnotations)`** with the project-relative file
   path.
4. **Present the results** clearly. For each line (or range of lines), show:
   - Line number
   - Author
   - Branch
   - Changeset number (from `AnnotateInfo.Rev.Changeset`)
   - The line content

   Group consecutive lines that share the same author/changeset to keep the
   output readable. For large files, summarize by showing only the distinct
   author/changeset blocks rather than every single line.

### Example Conversation

**User:** "Annotate Assets/Scripts/EnemyController.cs"

**Assistant:**
1. Calls `Unity.UVCS.Execute(GetWorkspaceFromPath)` with `Application.dataPath`.
2. Calls `Unity.UVCS.Execute(GetFileAnnotations)` with
   `filePath = "Assets/Scripts/EnemyController.cs"`.
3. Presents grouped results:
   - Lines 1-15: authored by `john@example.com` on branch [/main](uvcs://show/branch/main) ([changeset 1042](uvcs://show/changeset/1042))
   - Lines 16-30: authored by `jane@example.com` on branch [/main/enemy-ai](uvcs://show/branch/main/enemy-ai) ([changeset 1105](uvcs://show/changeset/1105))
   - Lines 31-45: authored by `john@example.com` on branch [/main](uvcs://show/branch/main) ([changeset 1042](uvcs://show/changeset/1042))

---

## Workflow 2: Who Made a Specific Change / Why Was Something Changed

Use when the user asks who changed a specific method/property/lines, or **WHY
something looks different from expected** (e.g. "why is this color red?", "this
was supposed to be green", "why was the speed changed?").

### Steps

1. **Get the workspace** by calling `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. **Determine the file and line range.** The user may specify:
   - A method or property name — read the file to find the line range.
   - Specific line numbers — use them directly.
   - A code snippet — search the file for matching lines.
   - A GameObject, component, or property — determine which file contains the
     value (e.g. a `.prefab`, `.unity` scene file, `.mat` material, or `.cs`
     script) and search for the relevant property in that file.
3. **Call `Unity.UVCS.Execute(GetFileAnnotations)`** for the file.
4. **Filter the results** to only the relevant lines where the property or
   value appears.
5. **Report the author(s)** who last modified those lines, along with the
   branch and changeset where the change was made.
6. **Look up the changeset comment** by using `Unity.UVCS.Execute(FindQuery)` to
   understand WHY the change was made:
   ```
   find changeset where changesetid = <number>
   ```
   The changeset comment often explains the reason for the change. Always
   include this in your answer when the user is asking "why".

### Example Conversations

**User:** "Who changed the `TakeDamage` method in EnemyController.cs?"

**Assistant:**
1. Reads `EnemyController.cs` to find the `TakeDamage` method spans lines 52-68.
2. Calls `Unity.UVCS.Execute(GetFileAnnotations)` for the file.
3. Filters annotations for lines 52-68.
4. Reports: "The `TakeDamage` method was last modified by `jane@example.com` in
   [changeset 1105](uvcs://show/changeset/1105) on branch
   [/main/enemy-ai](uvcs://show/branch/main/enemy-ai). Lines 52-60 were
   authored by `jane@example.com`, and lines 61-68 by `john@example.com` in
   [changeset 1042](uvcs://show/changeset/1042) on
   [/main](uvcs://show/branch/main)."

**User:** "Why is the color of this cube red? It was supposed to be green."

**Assistant:**
1. Identifies the cube's material file (e.g. `Assets/Materials/CubeMaterial.mat`).
2. Reads the file to find the color property line (e.g. `m_Color: {r: 1, g: 0, b: 0}`).
3. Calls `Unity.UVCS.Execute(GetFileAnnotations)` for the material file.
4. Filters to the line with the color property — finds it was last changed in
   cs:1340 by `mike@example.com` on branch `/main/visual-update`.
5. Queries: `find changeset where changesetid = 1340`
6. Reads the changeset comment: "Updated cube materials to match new art direction".
7. Reports: "The color was changed to red by `mike@example.com` on April 8 in
   [changeset 1340](uvcs://show/changeset/1340) (branch
   [/main/visual-update](uvcs://show/branch/main/visual-update)). The
   changeset comment says: 'Updated cube materials to match new art direction'.
   You may want to check with Mike about whether this cube was supposed to be
   included in that update."

---

## Workflow 3: Find the Branch Where Something Was Added

Use this workflow when the user asks which branch introduced a specific
GameObject, component, feature, or piece of code.

### Steps

1. **Get the workspace** by calling `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. **Determine the file to annotate.** If the user mentions:
   - A GameObject — find the `.prefab` or scene file that contains it. Note that
     `.prefab` and `.unity` scene files are text-based YAML in Unity, so they can
     be annotated.
   - A script or component — find the `.cs` file.
   - A specific asset — find the corresponding asset file.
3. **Call `Unity.UVCS.Execute(GetFileAnnotations)`** for the file.
4. **Analyze the annotations:**
   - For prefab/scene files: search for the relevant GameObject by name in the
     line contents, then report the branch from the annotation of those lines.
   - For scripts: find the relevant code section and report the branch.
5. **Report the branch** where the content was first introduced.
6. **Optionally provide changeset details** by querying with `FindQuery`:
   ```
   find changeset where changesetid = <number>
   ```

### Example Conversation

**User:** "In which branch was the EnemySpawner GameObject added?"

**Assistant:**
1. Determines the EnemySpawner is in `Assets/Scenes/MainLevel.unity`.
2. Calls `Unity.UVCS.Execute(GetFileAnnotations)` for the scene file.
3. Searches the annotated lines for "EnemySpawner" in the content.
4. Finds those lines were authored on branch `/main/enemy-system` in cs:892.
5. Reports: "The EnemySpawner GameObject was added in branch
   [/main/enemy-system](uvcs://show/branch/main/enemy-system)
   ([changeset 892](uvcs://show/changeset/892)) by `mike@example.com`."

---

## Workflow 4: When Was a File Last Modified

Use this workflow when the user asks how recently a file was changed, when the
last modification happened, or how old certain code is.

### Steps

1. **Get the workspace** by calling `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. **Call `Unity.UVCS.Execute(GetFileAnnotations)`** for the target file.
3. **Find the most recent changeset** across all annotated lines by comparing
   the `AnnotateInfo.Rev.Changeset` values. The highest changeset number is the
   most recent change.
4. **Get the changeset timestamp** by querying:
   ```
   find changeset where changesetid = <highest_changeset_number>
   ```
   The `date` field in the result gives you the exact timestamp.
5. **Calculate and report the time difference** between now and the changeset
   date. Present it in a human-friendly format (e.g. "3 days ago", "2 weeks ago",
   "about 6 months ago").
6. **Optionally report the author** who made the most recent change and what
   branch it was on.

### Example Conversation

**User:** "When was Player.cs last modified?"

**Assistant:**
1. Calls `Unity.UVCS.Execute(GetFileAnnotations)` for `Assets/Scripts/Player.cs`.
2. Finds the highest changeset is 1210 (from lines 40-45, authored by
   `sarah@example.com` on `/main/player-movement`).
3. Queries: `find changeset where changesetid = 1210`
4. Gets the date: 2026-04-10.
5. Reports: "Player.cs was last modified 4 days ago (April 10, 2026) by
   `sarah@example.com` in [changeset 1210](uvcs://show/changeset/1210) on
   branch [/main/player-movement](uvcs://show/branch/main/player-movement)."

---

## Workflow 5: Find the Most Active Contributors for a File

Use this workflow when the user asks who works on a file the most, who the main
contributors are, or who to ask about a particular script.

### Steps

1. **Get the workspace** by calling `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. **Call `Unity.UVCS.Execute(GetFileAnnotations)`** for the target file.
3. **Count lines per author.** Iterate through all annotated lines and tally how
   many lines each distinct `Author` currently owns (i.e., last modified).
4. **Sort authors by line count** in descending order.
5. **Present a summary** showing:
   - Each author and the number/percentage of lines they last modified.
   - The branches they worked on.
   - Optionally, the changeset range for each author.

   This helps the user understand who has the most knowledge about the file and
   who to consult for questions.

### Example Conversation

**User:** "Who are the main contributors to EnemyController.cs?"

**Assistant:**
1. Calls `Unity.UVCS.Execute(GetFileAnnotations)` for `Assets/Scripts/EnemyController.cs`.
2. Tallies line ownership:
   - `jane@example.com`: 45 lines (52%)
   - `john@example.com`: 30 lines (35%)
   - `mike@example.com`: 11 lines (13%)
3. Reports:
   - "**jane@example.com** is the primary contributor with 52% of the lines,
     mainly from work on branch [/main/enemy-ai](uvcs://show/branch/main/enemy-ai)
     ([cs:1105](uvcs://show/changeset/1105))."
   - "**john@example.com** contributed 35%, mostly on
     [/main](uvcs://show/branch/main) ([cs:1042](uvcs://show/changeset/1042))."
   - "**mike@example.com** contributed 13%, from
     [/main/enemy-system](uvcs://show/branch/main/enemy-system)
     ([cs:892](uvcs://show/changeset/892))."
   - "For questions about this file, jane@example.com would be the best person
     to ask."

---

## Workflow 6: Trace the Full History of a Line or Section

Use when the user wants the chain of changes for a specific line over time. The
`revisionId` parameter of `GetFileAnnotations` lets you annotate at any
historical revision.

### Steps

1. **Get the workspace** by calling `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. **Call `Unity.UVCS.Execute(GetFileAnnotations)`** for the file (current version).
3. **Identify the target lines** and their changeset numbers from the annotate
   results.
4. **Query the revision history** for the file to find all prior revisions:
   ```
   find revision where item = 'path/to/file' order by changeset desc
   ```
5. **Annotate previous revisions** by calling `Unity.UVCS.Execute(GetFileAnnotations)`
   again with the `revisionId` parameter set to older revision IDs from step 4.
   This lets you see who had modified the same line *before* the current author.
6. **Query changeset details** using `Unity.UVCS.Execute(FindQuery)`:
   ```
   find changeset where changesetid = <number>
   ```
   This gives the full changeset metadata including comment, date, author, and
   branch.
7. **Present a timeline** of changes showing who changed what, when, and on
   which branch.

### Example Conversation

**User:** "Show me the history of changes to the `maxHealth` field in Player.cs"

**Assistant:**
1. Reads `Player.cs` to find `maxHealth` is on line 12.
2. Calls `Unity.UVCS.Execute(GetFileAnnotations)` — line 12 was last changed in cs:1180
   by `sarah@example.com` on `/main/player-movement` (revId: 4820).
3. Queries the revision history:
   `find revision where item = 'Assets/Scripts/Player.cs' order by changeset desc limit 20`
4. Finds an earlier revision (revId: 3950) — calls `Unity.UVCS.Execute(GetFileAnnotations)`
   with `revisionId = 3950` to see who had that line before sarah's change.
5. Reports a timeline of the field's changes across revisions.

---

## Workflow 7: Identify Lines Changed via Merge

Use when the user wants to know which lines arrived through merges rather than
direct edits on the current branch.

### Steps

1. **Get the workspace** by calling `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. **Call `Unity.UVCS.Execute(GetFileAnnotations)`** for the file.
3. **Filter lines** where `AnnotateInfo.IsMergeDstRev` is `true`. These lines
   were brought into the current branch via a merge from another branch.
4. **Report the merged lines**, including which branch they originated from and
   the changeset that merged them.

### Example Conversation

**User:** "Which lines in GameManager.cs came from merges?"

**Assistant:**
1. Calls `Unity.UVCS.Execute(GetFileAnnotations)` for `Assets/Scripts/GameManager.cs`.
2. Filters for lines where `IsMergeDstRev = true`.
3. Reports: "Lines 22-35 were merged from branch
   [/main/multiplayer](uvcs://show/branch/main/multiplayer)
   ([cs:1050](uvcs://show/changeset/1050)), and lines 80-95 were merged from
   [/main/ui-overhaul](uvcs://show/branch/main/ui-overhaul)
   ([cs:1120](uvcs://show/changeset/1120))."

---

## Workflow 8: Annotate Previous Version of a File

Use this workflow when the user wants to see the annotated state of a file one
revision back — the version just before the most recent change. This is the
equivalent of the GUI's "Annotate parent revision" context menu action.

### Steps

1. **Get the workspace** by calling `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. **Query the revision history** of the file to find the previous revision:
   ```
   find revision where item = 'path/to/file' order by changeset desc limit 2
   ```
   The first result is the current revision; the second is the parent (previous)
   revision.
3. **Call `Unity.UVCS.Execute(GetFileAnnotations)`** with the `revisionId` set to the
   second revision's ID.
4. **Present the annotated results** for the previous version.

**NOTE:** Do NOT annotate the current version first just to find the previous
one. Use `FindQuery` to get the revision history directly — it is much faster
than running an annotate.

### Example Conversation

**User:** "Show me the annotations for the previous version of Player.cs"

**Assistant:**
1. Queries: `find revision where item = 'Assets/Scripts/Player.cs' order by changeset desc limit 2`
2. Gets two results: revId:4820 (cs:1180, current) and revId:3950 (cs:1042, previous).
3. Calls `Unity.UVCS.Execute(GetFileAnnotations)` with `revisionId = 3950`.
4. Presents the per-line annotations for the file as it was at cs:1042.

---

## Workflow 9: Annotate Before a Specific Change

Use this workflow when the user wants to see what a file (or a specific value in
a file) looked like *before* a particular changeset modified it. This is the
equivalent of the GUI's "Annotate before these changes" context menu action.

Typical triggers: "What was the color before it was changed to red?", "What
value did maxHealth have before changeset 1180?", "Show me the file before that
change".

### Steps

1. **Get the workspace** by calling `Unity.UVCS.Execute(GetWorkspaceFromPath)`.
2. **Identify the changeset** that introduced the change. This may come from:
   - A previous annotate result (the `AnnotateInfo.Rev.Changeset` of the line
     in question).
   - The user specifying a changeset number directly.
   - A `FindQuery` result.
3. **Find the file's revision just before that changeset** by querying:
   ```
   find revision where item = 'path/to/file' and changeset < <changeset_number> order by changeset desc limit 1
   ```
   This returns the last revision of the file before the target changeset.
4. **Call `Unity.UVCS.Execute(GetFileAnnotations)`** with the `revisionId` from step 3.
5. **Find the relevant lines** in the older annotated version and report the
   previous value.
6. **Optionally compare** the old and new values for the user.

### Example Conversation

**User:** "The color of this cube is red now, what color was it before?"

**Assistant:**
1. Identifies the material file `Assets/Materials/CubeMaterial.mat`.
2. Calls `Unity.UVCS.Execute(GetFileAnnotations)` for the current version — finds the
   color line (`m_Color: {r: 1, g: 0, b: 0}`) was last changed in cs:1340
   (revId: 5200).
3. Queries:
   `find revision where item = 'Assets/Materials/CubeMaterial.mat' and changeset < 1340 order by changeset desc limit 1`
4. Gets revId:4800 (cs:1290) — the version before the color change.
5. Calls `Unity.UVCS.Execute(GetFileAnnotations)` with `revisionId = 4800`.
6. Finds the color line in the older version: `m_Color: {r: 0, g: 1, b: 0}`.
7. Reports: "Before [changeset 1340](uvcs://show/changeset/1340), the cube's
   color was **green** (`r:0, g:1, b:0`). It was changed to **red** by
   `mike@example.com` on April 8 in branch
   [/main/visual-update](uvcs://show/branch/main/visual-update)."

### Chaining: Multiple Steps Back

The user may ask to go further back (e.g. "and what was it before *that*?").
Repeat steps 3-5 using the changeset from the older annotate result to go
another step back in history. Each iteration reveals the previous value.
