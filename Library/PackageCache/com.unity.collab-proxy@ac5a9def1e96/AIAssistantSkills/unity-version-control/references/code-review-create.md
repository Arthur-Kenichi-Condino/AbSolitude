# Code Review: Create (Reviewer)

Workflow for performing a code review on UVCS changes — analyzing a diff for
defects and (optionally) publishing findings as inline comments on a UVCS code
review.

The interaction workflow (phases, tool calls, confirmation prompts, publish
flow) is the core of this document and MUST be followed exactly. The code
review rules (Critical Rules, Severity Definitions, Output Format) are
defaults — if an existing skill or directive defines how to review code,
prefer that over the rules here.

For the author-side workflow (addressing comments left on a review), see
[code-review-address.md](code-review-address.md).

Code reviews build on top of the diff workflow documented in
[diff-workflows.md](diff-workflows.md). The diff workflow retrieves the changed
files; this workflow analyzes those changes for defects.

## Table of Contents

- [Critical Rules](#critical-rules)
- [Severity Definitions](#severity-definitions)
- [Output Format](#output-format--mandatory)
- [Workflow: Code Review](#workflow-code-review)
- [Phase 2: Publish Confirmation](#phase-2--publish-confirmation-branch-and-changeset-reviews-only)
- [Phase 3: Optional Quick Edit](#phase-3--optional-quick-edit)
- [Phase 4: Publish to UVCS](#phase-4--publish-to-uvcs)
- [Example: Review Pending Changes](#example-review-pending-changes)
- [Example: Review a Branch with Publish](#example-review-a-branch-with-publish)
- [Notes](#notes)

---

## Critical Rules

1. **Only review the diff.** Do NOT comment on pre-existing code outside the
   changed or new lines. If you spot a bug in unchanged code that is clearly related
   to the new changes, tag it with `[PRE-EXISTING]` so the author knows the
   current change did not introduce it.

2. **Confidence gate.** For every finding, ask yourself: "Can I point to the
   exact line and explain precisely why this is wrong?" If no, do NOT report it.
   Two real bugs are worth more than ten probable ones.

3. **Never report style preferences as bugs.** Do NOT flag: formatting, import
   order, naming conventions consistent with the surrounding codebase, or comment
   style. These are noise.

4. **Concrete fixes only.** Every `BLOCKER` and `IMPORTANT` finding MUST include
   a code suggestion showing the corrected code. A finding without a fix is not
   actionable.

5. **Be specific, not vague.** Every finding MUST name the exact variable,
   method, parameter, or expression that is wrong. "This could cause issues" is
   NOT acceptable. "Calling `Destroy(gameObject)` on line 34 inside
   `OnTriggerEnter` will destroy the manager singleton, breaking all future
   calls to `EnemyAI.Instance`" IS acceptable. The reader should be able to
   locate the exact problem from the finding text alone, without re-reading
   the code. Vague findings waste the author's time.

6. **Stop at 15 findings.** If you find more than 15 issues, report the
   highest-severity ones and note how many additional lower-severity findings
   were omitted. Offer a second pass if needed.

7. **Semi-formal reasoning for BLOCKERs.** Before classifying anything as
   `BLOCKER`, trace the execution path: state the precondition, trace what
   happens at the specific line, and state the failure mode. Do NOT classify as
   `BLOCKER` based on surface pattern matching alone.

8. **When uncertain about intent, say so.** Use the pattern:
   "If the intent is X, this is a BLOCKER. If the intent is Y, this is fine."
   Do NOT escalate uncertainty into a higher severity.

9. **NITs are included by default.** Emit `NIT` findings as part of every
   review unless the user explicitly opts out (e.g., "skip nits", "no nits",
   "review without nits"). When opted out, suppress them entirely — do not
   mention that nits were omitted.

10. **Do NOT offer to fix issues when reviewing branches, changesets, shelves, or labels.** 
   The workspace may not be pointing to the branch being reviewed,
   so modifying files would corrupt the wrong branch.
   **For branches and changesets:** after presenting findings, offer to
   **publish** the selected findings to a UVCS code review (see Phase 2-4 below).
   Do NOT offer to fix the code.
   **For shelves and labels:** STOP after presenting findings. No fix offers and
   no code review publishing (UVCS does not support code reviews for these types).
   **Exception -- pending changes:** When reviewing pending changes, the files
   are on disk in the current workspace, so fixes can be safely applied. After
   presenting findings, ask the user whether they would like you to fix the
   reported issues. Do NOT offer code review publishing for pending changes.

11. **You MUST use the exact output format defined below.** Do NOT use bullet
    points, free-form paragraphs, or any other layout. Every single finding MUST
    follow the structured block format with `**[ID] [SEVERITY]**`,
    `**What's wrong:**`, `**Why it matters:**`, and `**Fix:**` on separate lines.
    No exceptions. Every finding MUST have a unique short ID (e.g. `B1`, `I2`,
    `S3`, `N1`) so the user can reference specific findings by ID.

12. **Chat output IS the code review comment.** What you show the user in chat
    for each finding is EXACTLY what will be published as a comment in the UVCS
    code review. The user must be able to read the finding in chat and know
    precisely what the code review comment will say — no rewording, no
    summarizing, no reformatting when publishing. This means the finding text
    must be written as if it were already the code review comment: clear,
    self-contained, and directly addressed to the code author.

13. **Comment type is `Change`, never `Question`.** When publishing findings to
    a UVCS code review, ALL findings (BLOCKER, IMPORTANT, SUGGESTION, NIT)
    MUST use `commentType: Change`. These are change requests — they identify
    something wrong and tell the author what to fix. Do NOT use
    `commentType: Question` for findings. `Question` is ONLY appropriate when
    the reviewer has a genuine question and is asking the author for
    clarification (e.g., "Is this timeout value intentional?"). If you can
    point to what's wrong and provide a fix, it is a `Change`, not a `Question`.

---

## Severity Definitions

Use exactly these labels and definitions. Consistent classification is critical
so the author can triage findings quickly.

### BLOCKER

`🔴 BLOCKER` -- A defect you are **highly confident** would cause incorrect
behavior, a crash, data loss, a security vulnerability, or a resource leak in
production. You must be able to trace the execution path to demonstrate the
failure.

Examples: wrong operator (`=` vs `==`, `&` vs `&&`), null dereference,
unclosed resource, SQL injection, hardcoded secret, logic that can never be
reached, variable assigned to itself, off-by-one that causes a panic, infinite
loop, race condition with observable data corruption.

### IMPORTANT

`🟡 IMPORTANT` -- A real issue that is likely to surface under realistic
conditions (edge inputs, concurrent load, partial failures) but does not
immediately crash or corrupt data.

Examples: missing null check on a field that callers sometimes omit, O(n^2) loop
where n grows unbounded, missing error propagation on a fallible call, partial
write without rollback, missing `Dispose` on a short-lived object that holds
unmanaged resources.

### SUGGESTION

`💡 SUGGESTION` -- An objectively better alternative with a clear, concrete
benefit (readability, testability, or robustness) that most senior engineers
would agree with. Must NOT be a style preference.

Examples: rename that removes ambiguity, extract a method that duplicates
non-trivial logic, use a language-provided safe API over a manual unsafe
equivalent, replace error-prone manual indexing with LINQ.

### NIT

`⚪ NIT` -- Style or cosmetic. Included by default. Suppress only when the user
explicitly opts out ("skip nits", "no nits").

Examples: formatting, comment phrasing with no semantic difference, import
ordering, naming opinion.

---

## Output Format -- MANDATORY

**YOU MUST USE THIS EXACT FORMAT. Do NOT use bullet points, numbered lists,
free-form paragraphs, or any other layout. Every finding MUST follow the block
structure below exactly as shown. Non-compliance with this format is a failure.**

**IMPORTANT: Do NOT offer to fix issues when reviewing branches, changesets,
shelves, or labels. The review output is the final deliverable. STOP after
presenting findings. Exception: when reviewing pending changes, ask the user
whether they would like you to fix the reported issues.**

### Summary Section

One paragraph: what the change does and your overall confidence in its
correctness. Be direct.

### Tally Header

After the summary paragraph, output a one-line severity tally. Only include
severities that have at least one finding:

```
🔴 BLOCKER (3)   🟡 IMPORTANT (2)   💡 SUGGESTION (1)   ⚪ NIT (2)
```

### Findings Section

Every finding gets a **unique short ID** used for referencing in Phase 2.
IDs are sequential within each severity category:
- BLOCKERs: `B1`, `B2`, `B3`, ...
- IMPORTANTs: `I1`, `I2`, ...
- SUGGESTIONs: `S1`, `S2`, ...
- NITs: `N1`, `N2`, ...

For each issue found, output EXACTLY this block (copy this structure literally):

```
**[B1] 🔴 BLOCKER** `file.ext:line`
**What's wrong:** One sentence describing the exact defect.
**Why it matters:** One sentence on the concrete impact.
**Fix:**
\`\`\`suggestion
[concrete corrected code -- not pseudocode]
\`\`\`
```

Replace `[B1] 🔴 BLOCKER` with the appropriate ID and severity. The pattern is:
`**[ID] SEVERITY_EMOJI SEVERITY_LABEL**`.

**Format rules:**
- Each finding is its own block. Do NOT merge findings into bullet lists.
- `**What's wrong:**` and `**Why it matters:**` are each ONE sentence on their
  own line. Do NOT combine them into a single paragraph or bullet point.
- The `Fix:` section with the code suggestion is REQUIRED for `BLOCKER` and
  `IMPORTANT`. For `SUGGESTION` it is recommended. For `NIT` it is optional.
- Group findings by severity: BLOCKERs first, then IMPORTANT, then SUGGESTION,
  then NIT (NITs are included by default; only absent if the user opted out).
- For **branch and changeset** reviews: after the last finding, proceed to
  Phase 2 (publish confirmation). See below.
- For **shelve and label** reviews: do NOT add any text after the last finding.
  The review ends there.
- For **pending changes** reviews: after the last finding, ask the user whether
  they would like you to fix the reported issues.

If no issues are found, say so explicitly: "No issues found. The changes look
correct."
- **For branches and changesets:** offer to create a UVCS code review with a
  `CodeReviewStatus.Reviewed` (LGTM) status: "Would you like me to create a UVCS code review
  and mark it as approved?" If the user agrees, run Phase 4 with no comments
  and set status to `CodeReviewStatus.Reviewed`.
- **For shelves, labels, and pending changes:** STOP.

---

## Workflow: Code Review

### Trigger

"Review my changes", "Code review branch X", "Review my pending changes",
"Review changeset 42", "Review shelve 100", "Review label v1.0",
"Can you check my code?", "Look at what I changed", "Are my changes correct?",
"Review the code in branch X", or similar requests where the user wants a
quality/correctness review of code changes.

### Steps

1. **Get the workspace** via `Unity.UVCS.Execute(GetWorkspaceFromPath)`.

2. **Determine the review target.** The user may specify a branch, changeset,
   two changesets, label, shelve, or pending changes. If not specified, default
   to **pending changes** (review what the user is about to check in). If
   ambiguous, ask the user using `Unity.UVCS.Execute(SelectOption)` with the possible
   targets.

3. **Get changed files** by calling ONE diff tool matching the target:
   - Branch: `Unity.UVCS.Execute(GetBranchDifferences)`
   - Single changeset: `Unity.UVCS.Execute(GetChangesetDifferences)`
   - Two changesets: `Unity.UVCS.Execute(GetChangesetsDifferences)`
   - Label: `Unity.UVCS.Execute(GetLabelDifferences)`
   - Shelve: `Unity.UVCS.Execute(GetShelveDifferences)`
   - Pending changes: `Unity.UVCS.Execute(GetPendingChanges)`

4. **Filter** out directory entries and binary files (`.png`, `.fbx`, `.wav`,
   `.dll`, `.asset`, `.mat`, `.prefab`, `.unity`, `.meta`, `.anim`,
   `.controller`, `.physicMaterial`, `.renderTexture`, `.flare`, `.giparams`,
   `.lighting`, etc.). Code review only applies to text-based source files.

5. **Download text file revisions** using `Unity.UVCS.Execute(DownloadRevisionsToFiles)`
   following the same rules as the diff workflow:
   - `Changed`: download both `Diff.Base.Id` (BEFORE) and `Diff.RevInfo.Id`
     (AFTER).
   - `Added`: download `Diff.RevInfo.Id` only (AFTER -- the new file).
   - `Deleted`: download `Diff.Base.Id` only (BEFORE -- the removed file).
     For deleted files, there is nothing to review for defects. Note the
     deletion in the summary but do not produce findings.
   - **Pending changes:** download `RevInfo.Id` as BEFORE; read the workspace
     file at `Path` as AFTER.

6. **Read the downloaded files** and identify the changed lines/sections by
   comparing BEFORE and AFTER.

7. **Review ONLY the changed code.** For each changed file:
   - Read the surrounding context (the unchanged lines around the diff) to
     understand what the change does, but do NOT flag issues in unchanged code
     unless they are directly caused by the new changes.
   - Apply the confidence gate: can you point to the exact line and explain
     precisely why it is wrong?
   - Classify each finding using the severity definitions above.
   - For BLOCKERs, perform semi-formal reasoning: state the precondition, trace
     the execution at the line, state the failure.
   - For BLOCKERs and IMPORTANTs, include a concrete fix with corrected code.

8. **Present the review** using the output format defined above: summary
   paragraph, then findings grouped by severity. If no issues are found, state
   that clearly.

9. **Call `Unity.UVCS.Execute(CleanupDiffTempFiles)`** to delete all downloaded temp
   files. This is MANDATORY -- never skip this step.

10. **Wrap up.**
    - **If reviewing a branch or changeset:** you MUST output the **exact
      Phase 2 prompt** shown below. Do NOT paraphrase it, do NOT shorten it,
      do NOT replace it with a generic yes/no question. The user needs to see
      the selection examples so they know they can pick specific findings.
      Do NOT offer to fix the code.
    - **If reviewing a shelve or label:** STOP. Do NOT make any additional tool
      calls. Do NOT offer to fix the issues or publish a code review.
    - **If reviewing pending changes:** ask the user whether they would like you
      to fix the reported issues, since the files are on disk and can be safely
      modified. Do NOT offer code review publishing.

---

## Phase 2 -- Publish Confirmation (branch and changeset reviews only)

**MANDATORY:** After presenting findings for a **branch** or **changeset**
review (and after calling `CleanupDiffTempFiles`), you MUST end your response
with the following prompt **exactly as written** (only replace `N` with the
actual finding count and adjust the example IDs to match your actual findings).
Do NOT paraphrase, do NOT shorten, do NOT replace with a generic question like
"Would you like me to publish these?". The user needs to see the selection
examples:

```
I found N issues. Which ones should I add to the UVCS code review?

You can say things like:
- "Add all" -- adds every finding
- "Add all blockers" -- adds only blocker-severity findings
- "Add B1, S1" -- adds specific findings by ID
- "Add all except B1" -- adds everything except the listed ones
- "Edit B1" -- adjust a finding before adding
- "No thanks" -- skip
```

Wait for the user's response.

**Parse the user's natural-language selection** to determine which findings to
include. Examples:
- "Add all" -> all findings
- "Add all blockers" -> all findings with BLOCKER severity
- "Add B1, I2, S3" -> exactly those IDs
- "Add all except B3 and I1" -> all findings minus the excluded IDs
- "No" / "No thanks" / "Skip" -> do not publish, STOP

If the user says "No" or declines, STOP the workflow. Do NOT publish anything.

**IMPORTANT:** Do NOT skip Phase 2. The user MUST explicitly choose which
findings to publish. Never auto-publish all findings without user confirmation.

---

## Phase 3 -- Optional Quick Edit

If the user says "Edit B2" (or similar), show just that finding and ask:

```
Here's B2:

[show the finding in full]

Tell me how to adjust it, or say "looks good".
```

Apply the user's edit to the finding's text, then return to the Phase 2 prompt
so the user can continue selecting findings.

This is a targeted micro-loop: one finding, one exchange, done. Do NOT
re-present the entire review.

---

## Phase 4 -- Publish to UVCS

Once the user has specified which findings to add:

1. **Confirm** using `Unity.UVCS.Execute(AskForConfirmation)`:
   - Action: `"Create code review"`
   - Detail: `"Branch: /main/this-task\nComments: 7 (3 change requests, 4 comments)"`
   
   Adjust the detail to match the actual branch/changeset and comment counts.
   If the user cancels, STOP.

2. **Create the code review** using the appropriate tool:
   - For branches: `Unity.UVCS.Execute(CreateCodeReviewForBranch)(branchName)`
   - For changesets: `Unity.UVCS.Execute(CreateCodeReviewForChangeset)(changesetId)`
   
   The tool returns a `ReviewInfo` object. Use `ReviewInfo.Id` as the review ID
   for subsequent calls.

3. **Add the current user as a reviewer** by calling
   `Unity.UVCS.Execute(AddCurrentUserAsReviewer)(reviewId)`.
   This assigns the AI assistant's user as a reviewer on the code review.

4. **Add each selected finding as a comment** using the appropriate tool.

   **CRITICAL: The comment text you publish MUST be the SAME text you already
   showed the user in chat.** Do NOT rewrite, summarize, or reformat findings
   when publishing. The user already approved the exact text they saw. Copy
   the finding text as-is.

   - For **ALL severity levels** (BLOCKER, IMPORTANT, SUGGESTION, NIT), use
     `Unity.UVCS.Execute(CreateCodeComment)` — these are inline code comments attached
     to a specific file and line:
     - `reviewId`: `ReviewInfo.Id` from step 2
     - `revisionId`: use `Diff.RevInfo.Id` from the diff data for the file
       this finding references (the "after" revision). **You must retain the
       diff metadata from the analysis phase to map each finding's file path
       back to its revision ID.**
     - `changesetId`: use `Diff.RevInfo.Changeset` from the diff data for the
       same file (the changeset that contains the revision)
     - `locationSpec`: the **zero-based** line number (as a string). If the
       finding references line 34 in the file, pass `"33"`. Subtract 1 from
       the 1-based line number shown in the finding.
     - `commentText`: use the EXACT finding text shown in chat, formatted as:
       ```
       [SEVERITY] What's wrong
       
       Why it matters: ...
       
       Fix:
       [code suggestion if any]
       ```
     - `commentType`: **ALWAYS `Change`**. Every finding is a change request —
       it identifies something wrong and tells the author what to fix. Do NOT
       use `Question`. The `Question` type is reserved for genuine questions
       where the reviewer is asking for clarification, NOT for findings.

   - Use `Unity.UVCS.Execute(CreateConversationComment)` ONLY for general observations
     not tied to any specific line. This requires only `reviewId` and
     `commentText`. This is rare — most findings are tied to a line.

   **Comment type mapping:** every finding — regardless of severity — is
   published with `CreateCodeComment` and `commentType: Change` (inline change
   request). There are no exceptions: BLOCKER, IMPORTANT, SUGGESTION, and NIT
   all use the same call shape.

5. **Set the review status** by calling
   `Unity.UVCS.Execute(SetReviewStatus)(reviewId, status, comment)`.

   The status depends on whether findings were published:

   - **If findings were published** (any severity): set status to
     `CodeReviewStatus.ReworkRequired` with a brief, empathetic comment summarizing the review
     outcome. The comment should acknowledge the author's work and frame the
     feedback constructively. Examples:
     - `"Good progress on this feature. A few issues need attention before merging — please see the inline comments for details."`
     - `"Solid foundation overall. Found some issues that should be addressed — see inline comments."`
     - `"Nice work on the implementation. There are a couple of things to revisit — check the inline comments."`
   
   - **If no findings were published** (user declined all, or review found no
     issues): set status to `CodeReviewStatus.Reviewed` with an approving comment. Examples:
     - `"LGTM — no issues found. Good to merge."`
     - `"Looks good! Clean implementation, no concerns."`

6. **Get the review link** by calling
   `Unity.UVCS.Execute(GetCodeReviewLink)(reviewId)`.

7. **Output confirmation** with the review link:
   ```
   Added N comments to code review on [/main/this-task](uvcs://show/branch/main/this-task).
   Open Review: plastic://server/repos/repo/code-reviews/42
   ```

---

## Example: Review Pending Changes

**User:** "Review my changes"

1. `GetWorkspaceFromPath` -> workspace path.
2. No target specified -> default to pending changes.
3. `GetPendingChanges(wkPath)` -> returns `ChangeInfo` list:
   - `PlayerController.cs` (Changed)
   - `EnemyAI.cs` (Added)
   - `Cube.mat` (Changed, binary)
4. Filter: skip `Cube.mat` (binary).
5. Download:
   - `PlayerController.cs`: download `RevInfo.Id` -> BEFORE temp file. Read
     workspace file at `Path` -> AFTER.
   - `EnemyAI.cs`: new file, read workspace file at `Path` -> AFTER only.
6. Read files, identify changed lines.
7. Review the changed code only.
8. Present:

> ### Summary
>
> This change adds an `EnemyAI` component with patrol and chase behavior and
> modifies `PlayerController` to expose the player's position via a public
> static property. The changes are mostly correct, with one issue in the
> enemy detection logic.
>
> 🟡 IMPORTANT (1)   💡 SUGGESTION (1)
>
> ### Findings
>
> **[I1] 🟡 IMPORTANT** `EnemyAI.cs:34`
> **What's wrong:** `Physics.OverlapSphere` result is used without a null check
> on the collider's `gameObject`, which can throw `NullReferenceException` if a
> collider is destroyed between the physics query and the access.
> **Why it matters:** Under load with many spawning/despawning enemies, this
> will intermittently crash.
> **Fix:**
> ```suggestion
> var colliders = Physics.OverlapSphere(transform.position, detectionRadius);
> foreach (var col in colliders)
> {
>     if (col == null || col.gameObject == null) continue;
>     if (col.CompareTag("Player")) { ... }
> }
> ```
>
> **[S1] 💡 SUGGESTION** `PlayerController.cs:12`
> **What's wrong:** `PlayerPosition` is a public static mutable field. A
> read-only property returning `transform.position` would prevent other scripts
> from accidentally overwriting it.
> **Why it matters:** A stale or overwritten position would silently break all
> consumers (like the new `EnemyAI`).
> **Fix:**
> ```suggestion
> public static Vector3 PlayerPosition => Instance.transform.position;
> ```

9. `CleanupDiffTempFiles(wkPath)` -> temp files deleted.
10. Since this is a pending changes review, ask:
    "Would you like me to fix these issues?"
    (Do NOT offer code review publishing for pending changes.)

---

## Example: Review a Branch with Publish

**User:** "Review the code in branch /main/feature-inventory"

1. `GetWorkspaceFromPath` -> workspace path.
2. Target: branch `/main/feature-inventory`.
3. `GetBranchDifferences(wkPath, "/main/feature-inventory")` -> `ClientDiff` list.
   Retain the diff metadata (especially `Diff.RevInfo.Id` per file) for Phase 4.
4. Filter out directories and binaries.
5. Download BEFORE/AFTER for each changed text file.
6. Read and identify changed lines.
7. Review only the changed code, produce findings.
8. Present summary, tally, and findings with IDs:

> ### Summary
>
> This change adds an inventory system with item stacking and weight limits.
> Two issues found in the stacking logic.
>
> 🔴 BLOCKER (1)   🟡 IMPORTANT (1)   💡 SUGGESTION (1)
>
> ### Findings
>
> **[B1] 🔴 BLOCKER** `InventoryManager.cs:87`
> **What's wrong:** `AddItem` does not check `maxStackSize` before incrementing
> the stack count, allowing stacks to exceed their limit.
> **Why it matters:** Players can stack unlimited items, breaking game balance
> and causing serialization overflow.
> **Fix:**
> ```suggestion
> if (slot.Count >= item.MaxStackSize) return false;
> slot.Count++;
> ```
>
> **[I1] 🟡 IMPORTANT** `InventoryManager.cs:112`
> **What's wrong:** `RemoveItem` subtracts from the stack count without checking
> if the count goes below zero.
> **Why it matters:** Negative stack counts will cause display glitches and
> deserialization errors.
> **Fix:**
> ```suggestion
> slot.Count = Mathf.Max(0, slot.Count - amount);
> if (slot.Count == 0) ClearSlot(slotIndex);
> ```
>
> **[S1] 💡 SUGGESTION** `ItemData.cs:24`
> **What's wrong:** `Weight` is a public field that could be set to a negative
> value. A property with a clamp would be safer.
> **Why it matters:** Negative weight would cause the total weight calculation
> to incorrectly allow more items than intended.
> **Fix:**
> ```suggestion
> public float Weight { get => _weight; set => _weight = Mathf.Max(0f, value); }
> ```

9. `CleanupDiffTempFiles(wkPath)` -> temp files deleted.
10. Proceed to Phase 2:

> I found 3 issues. Which ones should I add to the UVCS code review?
>
> You can say things like:
> - "Add all" -- adds every finding
> - "Add all blockers" -- adds only blocker-severity findings
> - "Add B1, I1" -- adds specific findings by ID
> - "Add all except S1" -- adds everything except the listed ones
> - "Edit B1" -- adjust a finding before adding
> - "No thanks" -- skip

**User:** "Add B1 and I1"

11. Phase 4: Confirm via `AskForConfirmation`:
    - Action: "Create code review"
    - Detail: "Branch: /main/feature-inventory\nComments: 2 (2 change requests)"
12. User confirms -> `CreateCodeReviewForBranch(wkPath, "/main/feature-inventory")`
    -> returns `ReviewInfo` with `Id = 123`.
13. `AddCurrentUserAsReviewer(wkPath, 123)` -> assigns current user as reviewer.
14. `CreateCodeComment(wkPath, 123, revId_InventoryManager, csId, "87", "...", Change)`.
15. `CreateCodeComment(wkPath, 123, revId_InventoryManager, csId, "112", "...", Change)`.
16. `SetReviewStatus(wkPath, 123, CodeReviewStatus.ReworkRequired, "Good progress on the inventory system. A couple of issues need attention — see inline comments.")`.
17. `GetCodeReviewLink(wkPath, 123)` -> `plastic://org.unity/repos/MyGame/code-reviews/123`.
18. Output:
    > Added 2 comments to code review on [/main/feature-inventory](uvcs://show/branch/main/feature-inventory).
    > Review status: Rework required
    > Open Review: plastic://org.unity/repos/MyGame/code-reviews/123

---

## Notes

- **Deleted files**: mention them in the summary (e.g., "Removed `OldManager.cs`")
  but do not produce findings for deleted code. There is nothing to fix in code
  that no longer exists.
- **Moved files**: if only moved (no content change), mention in the summary.
  If moved AND changed, review the content changes only.
- **Large diffs**: if a branch or changeset touches many files, focus your review
  on the files most likely to contain defects: logic files (`.cs`, `.js`, `.ts`,
  `.py`, etc.) over configuration, shaders, or data files. Mention skipped files
  at the end.
- **Context matters**: use the surrounding unchanged code to understand the
  purpose of the change, but never flag issues in it unless the new change
  directly introduces a regression in that context.
- **NITs are included by default**: emit `⚪ NIT` findings as part of every
  review. Suppress them only when the user explicitly opts out ("skip nits",
  "no nits"); when opted out, do not mention that nits were omitted.

---

For the author-side workflow (addressing the comments published by this
workflow), see [code-review-address.md](code-review-address.md).
