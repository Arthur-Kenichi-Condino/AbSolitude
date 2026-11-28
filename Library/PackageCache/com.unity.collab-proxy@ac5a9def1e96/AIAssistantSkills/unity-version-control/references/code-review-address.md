# Code Review: Address Review Comments (Author)

Workflow for the **code author** addressing feedback left on a UVCS code
review: applying requested changes and answering questions.

For the reviewer-side workflow (creating findings and publishing them as a
code review), see [code-review-create.md](code-review-create.md).

## Table of Contents

- [Mental Model](#mental-model)
- [Trigger](#trigger)
- [Critical Rules](#critical-rules)
- [Phase 0: Workspace Validation](#phase-0--workspace-validation-silent-unless-invalid)
- [Phase 1: Find & Select the Review](#phase-1--find--select-the-review)
- [Phase 2: The Work Queue Briefing](#phase-2--the-work-queue-briefing)
- [Phase 3 — Assisted Mode](#phase-3--assisted-mode)
- [Phase 3 — Automated Mode](#phase-3--automated-mode)
- [Phase 4: Wrap up](#phase-4--wrap-up)
- [Example: Assisted Walkthrough](#example-assisted-walkthrough)
- [Example: Automated Walkthrough](#example-automated-walkthrough)
- [Example: Switching Branches When Wrong Workspace](#example-switching-branches-when-wrong-workspace)

---

This is the author-side workflow — the counterpart to the reviewer-side
workflow in [code-review-create.md](code-review-create.md). The reviewer's
comments are the spec. Your job is to help the user address those comments,
under the user's direction (Assisted mode) or autonomously (Automated mode).
The user picks the mode after seeing the briefing.

## Mental Model

This workflow offers two modes after presenting the briefing:

- **Assisted** (default) — the LLM waits for the user to drive. The user asks
  for specific actions ("fix C1", "answer Q1") and approves each step before
  it happens. The user is always the decision-maker.
- **Automated** — the LLM works through every comment autonomously, applying
  fixes and creating a separate shelve for each one. No user interaction is
  needed until the end. The user reviews all the shelves afterward.

## Trigger

"Fix the review", "Address the review comments", "Apply the review changes",
"Let's fix the code review", "Apply pending review comments",
"Fix comment C1 from the review", "Let's fix the review on the current branch",
or similar requests where the user wants to address feedback as the code author.

## Critical Rules

### Rules for both modes

1. **One item, one shelve/checkin.** Each change request gets its own shelve
   (Automated) or checkin (Assisted) so the `[apply-change:<8-hex>]` tag
   correctly links to the specific comment. Do NOT batch.

2. **Never switch branches automatically.** If the workspace points to the
   wrong target, ask the user with `Unity.UVCS.Execute(AskForConfirmation)` before
   calling `Unity.UVCS.Execute(LaunchSwitchToBranchUI)`. Then STOP and tell the user
   to re-run the request after the switch completes.

3. **Never search for reviews on other branches.** No `ReworkRequired`
   reviews on this branch = nothing to do. STOP. Do NOT call `FindQuery` to
   look elsewhere, do NOT try other statuses, do NOT suggest other branches.

4. **Only query `CodeReviewStatus.ReworkRequired`.** Call
   `Unity.UVCS.Execute(GetReviewsInStatus)` exactly once with that status and the
   target ID. If empty, stop.

5. **Comment line numbers are 0-based.** `LocationSpec="33"` means line 34 in
   the editor (1-based). Add 1 when reading the workspace file.

6. **Always use branch/changeset diffs to resolve files and understand
   context.** The reviewer created their comments while looking at the
   branch or changeset diffs. You MUST retrieve the same diffs to:
   (a) resolve file paths from `RevisionId`/`ChangesetId`, and
   (b) understand the code context the reviewer was looking at.
   - If the review targets a **branch**: call
     `Unity.UVCS.Execute(GetBranchDifferences)(branchName)`.
   - If the review targets a **changeset**: call
     `Unity.UVCS.Execute(GetChangesetDifferences)(changesetId)`.
   NEVER search the workspace directory, use `FindQuery` to locate files,
   or guess file paths. The diff data is the single source of truth for
   mapping review comments to files. See Step 2.5 for details.

7. **Preserve original finding IDs; assign new ones only when missing.**
   Review comments created by the reviewer workflow have IDs embedded in
   the text like `[C1]`, `[S1]`, `[I1]`, `[N1]`. You MUST parse and reuse
   these original IDs throughout the workflow — NEVER override them with
   new IDs.
   For comments that do NOT have an embedded ID (e.g., manually written
   comments), assign a new ID based on the comment type:
   - Change → `CH1`, `CH2`, `CH3`, ... (sequential, skipping any number
     already taken by an embedded ID)
   - Question → `Q1`, `Q2`, `Q3`, ...
   This way every item in the queue has a short, referable ID.

9. **Use the exact output templates.** Every phase below includes an explicit
   output template. Follow it exactly. Do NOT improvise formatting, do NOT
   add extra decorations, do NOT change the structure. The templates are
   designed for readability in the chat UI.

10. **Render comment text with line breaks.** Review comments contain
    structured fields (`What's wrong:`, `Why it matters:`, `Fix:`). When
    displaying a comment, you MUST render each field on its own line with a
    blank line between them. NEVER concatenate fields into a single paragraph.

### Additional rules for Assisted mode only

11. **Never shelve/checkin without explicit approval.** After editing code, show
    the change and wait for the user to say "ok" / "yes" / "go". Use
    `Unity.UVCS.Execute(AskForConfirmation)` for the final checkin.

12. **Never post a reply (Answer or Discard) without explicit approval.** Show
    the proposed text first. The user can edit it. Confirm before calling
    `Unity.UVCS.Execute(ReplyComment)`.

13. **Never proactively suggest discarding.** Discard is a social act — the
    reviewer gets notified their request was rejected. Only call the discard
    path when the user explicitly says "discard" or equivalent.

### Rules that do NOT apply to Automated mode

In Automated mode, the LLM operates autonomously — it does NOT ask for
confirmation before shelving, does NOT wait for user approval between items,
and does NOT ask for confirmation before answering questions. The user chose
this mode precisely to let the LLM handle everything without interruption.

---

## Phase 0 — Workspace Validation (silent unless invalid)

Before anything else, anchor the workflow to a real, applyable target and
ensure the workspace is clean.

1. `Unity.UVCS.Execute(GetWorkspaceFromPath)(Application.dataPath)` → `WorkspaceInfo`.
2. `Unity.UVCS.Execute(GetPendingChanges)()` — check for pending changes.
   - If the list is **not empty**, STOP immediately:
     > Your workspace has pending changes. Please checkin or shelve them before
     > addressing review comments, so we don't mix unrelated changes with
     > review fixes.
   - If the list is empty, continue silently.
3. `Unity.UVCS.Execute(GetWorkspaceConfiguration)()` → `SelectorInformation`.

Three cases:

- **Workspace is on a branch** (`SelectorInformation.BranchName` is set):
  - `Unity.UVCS.Execute(GetBranchInfo)(BranchName)` → `BranchInfo`.
  - `targetId = BranchInfo.Id`.
  - Continue silently. Do NOT announce "workspace looks good".

- **Workspace is on a specific changeset** (`SelectorInformation.ChangesetNumber`
  is set, no branch):
  - This is unusual — the workspace is read-only-ish, and checking in is not
    the natural flow. Use `Unity.UVCS.Execute(AskForConfirmation)`:
    - Action: `"Continue on a changeset selector?"`
    - Detail: `"Your workspace is pinned to changeset cs:<N>, not a branch tip.
      Applying changes here is unusual; you may want to switch to a branch
      first. Continue anyway?"`
  - If the user declines, STOP.
  - If the user confirms, call `Unity.UVCS.Execute(GetChangesetInfo)(N)`
    and use `targetId = ChangesetInfo.ChangesetId`.

- **Workspace is on a label or shelve**: STOP immediately.
  - Tell the user: "Addressing review comments needs the workspace to be on
    the reviewed branch. Right now it's on label/shelve `<name>`. Switch to
    the branch and try again."

**If the user named a target that doesn't match the workspace** (e.g. "fix the
review on /main/feature-x" but the workspace is on /main/feature-y):

- `Unity.UVCS.Execute(AskForConfirmation)`:
  - Action: `"Switch workspace?"`
  - Detail: `"The review you asked to fix is on /main/feature-x, but your
    workspace is on /main/feature-y. To apply changes you need to switch.
    Switch now?"`
- If yes → `Unity.UVCS.Execute(LaunchSwitchToBranchUI)("/main/feature-x")`.
  Then STOP and say: "Switching… re-run the request once the switch
  completes."
- If no → STOP.

---

## Phase 1 — Find & Select the Review

`Unity.UVCS.Execute(GetReviewsInStatus)(CodeReviewStatus.ReworkRequired, targetId)`

Three outcomes:

- **0 reviews**:
  > There are no reviews with "Rework Required" status on `<branch>`. The
  > branch may have been approved already, or no review was created.

  STOP. Do NOT search elsewhere.

- **1 review**: announce it briefly, then move to Phase 2.

  Output template:
  ```
  Found one code review needing rework:

  **"<Title>"** by @<owner> — loading comments...
  ```

- **2+ reviews**: use `Unity.UVCS.Execute(SelectFromList)`:
  - `title`: `"Select review to address"`
  - `message`: `"Pick the review you want to work through."`
  - `label`: `"Review"`
  - `choices`: one entry per review, formatted as `"#<Id> — <Title> (by <owner>)"`.

  After the user picks, confirm in one line:
  > Working on **"<Title>"**.

---

## Phase 2 — The Work Queue Briefing

This is the most important UX moment. Before touching code, give the user a
scannable map of all pending items so they can re-order, skip, or discard
upfront.

### Step 2.1 — Load comments

`Unity.UVCS.Execute(GetReviewComments)(reviewId)` →
`List<CodeReviewCommentInfo>`. Cache this list — every later step uses it.

### Step 2.2 — Understand comment fields

The returned `CodeReviewCommentInfo` fields you'll use:
- `Id` — comment ID, used as `parentCommentId` for replies.
- `Type` — `Change` (1), `Question` (2), `Comment` (0), `Discarded` (6),
  `Conversation` (3), `Timeline` (4).
- `ParentCommentId` — `-1` for top-level; otherwise the parent comment Id.
- `Comment` — the comment text.
- `GUID` — unique ID; the first 8 hex chars (before the first dash) form
  the `[apply-change:<8-hex>]` tag.
- `RevisionId`, `ChangesetId`, `LocationSpec` — anchor for inline comments;
  `LocationSpec` is the **0-based** line number as a string.
- `AppliedInChangesetId` — `-1` if not yet applied; otherwise the cs that
  applied this change.

### Step 2.3 — Determine each item's display ID

Each comment's `Comment` field may start with a structured finding ID from the
review creation workflow. Parse it using this logic:

1. **Has an embedded ID** — If the comment text starts with `[<ID>]`
   (e.g., `[C1] 🔴 BLOCKER`, `[S1] 💡 SUGGESTION`, `[I1] 🟡 IMPORTANT`,
   `[N1] ⚪ NIT`), extract that ID (e.g., `C1`, `S1`, `I1`, `N1`).
   Use it as-is. NEVER override it with a different ID.

2. **No embedded ID** — If the comment text does NOT start with a `[<ID>]`
   pattern (e.g., a manually written comment like "Please add a null check
   here"), assign a new ID based on the comment type:
   - `Type == Change` → `CH1`, `CH2`, `CH3`, ... (sequential, skipping any
     number that would collide with an existing embedded ID)
   - `Type == Question` → `Q1`, `Q2`, `Q3`, ... (same collision-avoidance)

Every item in the queue must have a short, unique ID so the user can
reference it by name (e.g., "skip C1", "discard CH2", "start with Q1").

### Step 2.4 — Build the work queue

For each top-level comment (`ParentCommentId == -1`):

| Bucket | Conditions |
|---|---|
| **Pending change** | `Type == Change` AND `AppliedInChangesetId == -1` AND no child has `Type == Discarded` |
| **Pending question** | `Type == Question` AND no child comment exists (no comment has `ParentCommentId == this.Id`) |
| **Skip silently** | Anything else (already applied, already discarded, already answered, conversation, timeline) |

Sort each bucket by `(file, lineNumber)` so the queue reads top-down through
the codebase. Keep the original finding IDs (parsed in step 2.3).

### Step 2.5 — Fetch diffs and resolve file paths

**This step is critical.** The reviewer created their comments while looking at
the branch or changeset diffs. You MUST retrieve the same diffs to resolve
file paths and to understand the context the reviewer was seeing.

`CodeReviewCommentInfo` carries `RevisionId` / `ChangesetId` but not the file
path directly. Resolve paths using the appropriate diff tool:

- **If the review targets a branch:** call
  `Unity.UVCS.Execute(GetBranchDifferences)(branchName)` to get ALL
  files changed in the branch. This gives you the complete picture the
  reviewer saw.
- **If the review targets a changeset:** call
  `Unity.UVCS.Execute(GetChangesetDifferences)(changesetId)` to get
  the files changed in that specific changeset.

From the diff results, look up `Diff.RevInfo.Id == RevisionId` to get the
file `Path` for each comment. Cache the mapping
`commentId → (filePath, lineNumber, GUID)` for the whole workflow.

**NEVER** resolve file paths by searching the workspace directory, using
`FindQuery`, guessing paths from file names in comment text, or any other
method. The diff data is the single source of truth.

### Step 2.6 — Handle empty queue

If both buckets are empty:
> All comments on this review are already addressed (applied, discarded, or
> answered). Nothing to do.

STOP.

### Step 2.7 — Present the briefing

**You MUST use this exact template.** Only the content inside changes. Do NOT
add extra decorations, do NOT merge sections, do NOT use horizontal rule
characters or box-drawing characters inside the list. Keep it clean and plain.

```
**Code Review #<id>: "<title>"**
Reviewer: @<owner> · <N> items pending

**Changes to make** (code edits + checkin)

| # | File | Comment |
|---|------|---------|
| C1 | /Player.cs:42 | A new byte array is allocated in every Update frame |
| S1 | /Player.cs:88 | SendMessage is used for communication between methods... |
| I1 | /GameManager.cs:15 | The null check for the rotator reference was removed... |

**Questions to answer** (reply in the review)

| # | File | Question |
|---|------|----------|
| Q1 | /Player.cs:104 | Is this timeout value intentional? |

```

**Rules for the briefing table:**
- Use a markdown table, NOT a free-form list.
- The `#` column shows the original finding ID (e.g., `C1`, `S1`, `I1`).
- The `Comment` column shows only the first ~60 characters of the first
  meaningful line (the "What's wrong:" content), followed by `...` if truncated.
- Do NOT include severity emojis or severity labels in the table — the ID
  prefix already conveys the severity (C = blocker, I = important, S = suggestion,
  N = nit).
- Omit the "Questions to answer" section entirely if there are no questions.
- Omit the "Changes to make" section entirely if there are no changes.

### Step 2.8 — Ask the user to choose a workflow mode

After presenting the briefing table, use `Unity.UVCS.Execute(SelectOption)` to let the
user choose how to proceed:

- `title`: `"How would you like to work through these items?"`
- `message`: `"Choose a workflow mode for addressing the review comments."`
- `options`:
  - `label`: `"Assisted"`,
    `description`: `"I wait for your instructions. You tell me what to do: 'fix C1', 'answer Q1', 'discard S1'. I show you each change and ask for approval before checkin. You control the pace and every decision."`
  - `label`: `"Automated"`,
    `description`: `"I work through ALL items on my own, with no interruptions. For each change I apply the fix and create a shelve. For each question I post an answer. You review the results when I'm done. Fast, hands-off."`

**WAIT** for the user's choice. Do NOT start any work until the user selects a
mode.

- If the user selects **"Assisted"** → continue with **Phase 3 — Assisted Mode** below.
- If the user selects **"Automated"** → continue with **Phase 3 — Automated Mode** below.

---

## Phase 3 — Assisted Mode

This phase runs when the user selected the **Assisted** workflow mode (the
default).

The user drives the workflow. The LLM only acts when explicitly asked, and
always asks for approval before checkin or posting replies.

After the user selects this mode, output this exact template:

```
You're in control. Here's what you can ask me to do:

- **"Fix C1"** — I'll read the comment, apply the fix, and ask for your approval before checkin.
- **"Answer Q1"** — I'll investigate and propose a reply for you to approve.
- **"Discard S1"** — I'll ask for a reason and post the discard after confirmation.
- **"Show C1"** — I'll display the full comment details without making changes.
- **"Status"** — I'll show the current progress (what's done, pending, skipped).

Just tell me what to do next.
```

Then **WAIT**. Do NOT start any work. Only act when the user explicitly asks.

### Assisted rules

1. **Only act on explicit requests.** Never proactively pick the next item,
   suggest which item to work on, or start fixing something the user hasn't
   asked for. If the user's message is ambiguous, ask for clarification.

2. **Track progress in four buckets** (`done`, `discarded`, `skipped`,
   `pending`). When the user asks for "status", output:

   ```
   **Progress on review #<id>**

   | Status | Items |
   |--------|-------|
   | ✅ Applied | C1 |
   | 🚫 Discarded | — |
   | ⏭️ Skipped | — |
   | ⏳ Pending | S1, I1, Q1 |
   ```

   Use `—` for empty rows. Always include all four rows.

3. **The user can switch to Automated mode at any time.** If the user says
   "go", "continue automatically", "do the rest", or similar, switch to the
   Phase 3 — Automated Mode for the remaining pending items. Announce:
   > Switching to automated mode. I'll shelve each remaining fix
   > automatically.

   Then follow the Automated mode rules from that point.

4. **When all items are resolved** (no more pending items), proceed to
   **Phase 4 — Wrap up** automatically.

5. **The user can also trigger wrap-up early.** If the user says "done",
   "that's all", "wrap up", or similar while items are still pending, those
   remaining items count as skipped. Proceed to Phase 4.

### 3A — Change item (Assisted)

Triggered when the user says "fix C1", "apply C1", or similar.

**Step 1 — Show the request.**

Use this exact template:

````
---

**<ID>** · `<file>:<line>`

@<reviewer> requested:

> **What's wrong:** <what's wrong text>
>
> **Why it matters:** <why it matters text>
>
> **Fix:**
> ```
> <code suggestion>
> ```
````

**CRITICAL formatting rules for this step:**
- Parse the comment text into its structured fields: the text between
  `What's wrong:` and `Why it matters:` is the "what's wrong" content,
  the text between `Why it matters:` and `Fix:` is the "why it matters"
  content, and the text after `Fix:` is the code suggestion.
- Render each field on its own line with a blank line between them.
- If the comment text does NOT follow the structured format, display it
  as-is in a blockquote, preserving any line breaks in the original text.
- The `---` horizontal rule separates this item from the previous output.
- The `<ID>` is the original finding ID (e.g., `C1`, `S1`), NOT a new ID.
- The `<line>` is the 1-based line number (add 1 to `LocationSpec`).

**Step 2 — Read & edit.**

Use the file path resolved from the diff data in Step 2.5 (NEVER search for
the file or guess the path). Read the workspace file at that path and the
relevant line (remember: `LocationSpec` is 0-based, add 1 to get the editor
line). Apply the requested change.

**Step 3 — Show the proposed change.**

Use this exact template:

````
I edited `<file>`:

```diff
<show ~10 lines around the change as a unified diff snippet>
```

Does this look right? Reply **"ok"** to checkin, or tell me to adjust it.
You can also say **"skip"** or **"discard"**.
````

**Step 4 — Micro-loop (no commit yet).**

- User asks for tweaks → revise → re-show step 3. Repeat as long as needed.
- User says "ok" / "yes" / "go" → step 5.
- User says "skip" → mark the item skipped, no checkin, no reply. Print
  the progress nudge and move on.
- User says "discard" → branch to **3C**.

**Step 5 — Confirm checkin.**

Use `Unity.UVCS.Execute(AskForConfirmation)`:
- Action: `"Checkin fix for <ID>"`
- Detail: `"Comment: [apply-change:<8-hex>] <one-line description>\nFiles: <count> changed"`

If user cancels → leave item in pending state, print a note, and move on.

**Step 6 — Checkin.**

`Unity.UVCS.Execute(CheckinPendingChanges)("[apply-change:<8-hex>] <description>")`

The `<8-hex>` is the first 8 chars of `CodeReviewCommentInfo.GUID` (the part
before the first `-`). Example: GUID `8123d65a-620d-49ee-bf51-f526741a3341` →
tag `[apply-change:8123d65a]`. UVCS recognizes this tag and auto-marks the
change as Done in the review.

**Step 7 — Progress nudge.**

```
✅ <ID> checked in. <remaining> pending.
```

### 3B — Question item (Assisted)

Triggered when the user says "answer Q1", "reply to Q1", or similar.

**Step 1 — Show the question.**

Use this exact template:

```
---

**<ID>** · `<file>:<line>`

@<reviewer> asked:

> <full question text, preserving line breaks>
```

**Step 2 — Investigate.**

Read the file at the commented line plus relevant surrounding context. If the
answer requires code understanding outside the file, do that work too.

**Step 3 — Propose a reply.**

Use this exact template:

```
Suggested reply:

> <2-4 sentence answer, written as if the user is replying to the reviewer>

Post this reply? Say **"yes"**, edit it ("change it to say X"), or **"skip"**.
```

**Step 4 — Micro-loop.**

- User edits → revise → re-show step 3. Ask once more.
- "yes" → step 5.
- "skip" → mark item skipped. Print progress nudge. Move on.
- "discard" → not valid for questions; reply that questions can only be
  answered or skipped.

**Step 5 — Post the answer.**

`Unity.UVCS.Execute(ReplyComment)(reviewId, parentCommentId=<comment.Id>, commentText=<answer>, replyType=Answer)`

**Step 6 — Progress nudge.**

```
💬 <ID> answered. <remaining> pending.
```

### 3C — User-initiated discard (Assisted, change items only)

Triggered when the user says "discard", "skip and discard", or similar.

**Step 1 — Ask for the discard reason.**

```
Discarding <ID> will notify @<reviewer> that their request was rejected.
What reason should I give?
```

**Step 2 — Confirm via `Unity.UVCS.Execute(AskForConfirmation)`:**
- Action: `"Discard change request <ID>"`
- Detail: `"<ID>: <comment first line>\nReason: <user's reason>"`

If cancelled → leave item in pending, return to the conversation.

**Step 3 — Post the discard.**

`Unity.UVCS.Execute(ReplyComment)(reviewId, parentCommentId=<comment.Id>, commentText=<reason>, replyType=Discard)`

**Step 4 — Progress nudge.**

```
🚫 <ID> discarded. <remaining> pending.
```

---

## Phase 3 — Automated Mode

This phase runs when the user selected the **Automated** workflow mode.

The LLM works through every item autonomously with **no user interaction**.
It applies fixes, creates shelves, and answers questions on its own. The user
reviews the results when the loop is done.

After the user selects this mode, output:

```
Working through all items automatically. I'll create a separate shelve
for each change and post answers to questions. Sit back — I'll report
when I'm done.
```

Then **immediately start the loop**. Do NOT wait for further input.

### Automated loop — for each item in the queue

Process every pending item in order (changes first, then questions).

#### Change items

For each pending change:

1. **Read & edit.** Use the file path resolved from the diff data in Step 2.5
   (NEVER search for the file or guess the path). Read the workspace file at
   that path and the relevant line (remember: `LocationSpec` is 0-based,
   add 1 to get the editor line). Apply the requested change.

2. **Shelve and undo.** Call `Unity.UVCS.Execute(ShelvePendingChanges)(
   "[apply-change:<8-hex>] <one-line description>",
   undoPendingChangesAfterShelve: true)`.

   The `<8-hex>` is the first 8 chars of `CodeReviewCommentInfo.GUID` (the
   part before the first `-`). Example: GUID
   `8123d65a-620d-49ee-bf51-f526741a3341` → tag `[apply-change:8123d65a]`.

   **`undoPendingChangesAfterShelve: true` is critical** — it reverts the
   workspace to a clean state after shelving so the next fix starts from a
   clean baseline. Without it, changes accumulate and each shelve would
   contain all previous fixes too.

   Do NOT call `Unity.UVCS.Execute(AskForConfirmation)`. Do NOT show the change to
   the user. Do NOT wait for approval. Just shelve it and move on.

3. **Progress nudge.** Print exactly one line:
   ```
   📦 <ID> shelved ([sh:<positiveShelveId>](uvcs://show/shelve/<signedShelveId>)). <remaining> left.
   ```
   Use the signed shelve changeset ID from `ShelvePendingChanges` in the link,
   but display the positive absolute value in `sh:<positiveShelveId>`.

#### Question items

For each pending question:

1. **Investigate.** Read the file at the commented line plus relevant
   surrounding context. Understand what the reviewer is asking.

2. **Post the answer.** Call `Unity.UVCS.Execute(ReplyComment)(reviewId,
   parentCommentId=<comment.Id>, commentText=<answer>, replyType=Answer)`.

   Write a 2–4 sentence answer as if the user is replying to the reviewer.
   Do NOT ask the user to approve the answer. Just post it.

3. **Progress nudge.** Print exactly one line:
   ```
   💬 <ID> answered. <remaining> left.
   ```

#### Errors

If a fix fails to apply or a shelve fails, print a warning and skip the item:
```
⚠️ <ID> failed: <short reason>. Skipping.
```
Mark it as skipped and continue with the next item. Do NOT stop the loop.

When all items are processed, proceed to **Phase 4 — Wrap up**.

---

## Phase 4 — Wrap up

When the queue is empty (or the user triggers early wrap-up), output the
summary. The format depends on the workflow mode.

**Assisted mode** (checkins were made):

```
---

**Done with review #<id>.**

| Status | Items |
|--------|-------|
| ✅ Applied | C1, S1, I1 |
| 🚫 Discarded | C2 |
| ⏭️ Skipped | Q1 |

Each applied change is auto-marked as Done in the review via the
`[apply-change:]` tag.
```

- Only include rows that have at least one item.

### Re-request review (Assisted mode — automatic)

In **Assisted mode**, if at least one item was applied or discarded (i.e. the
review state changed), call `Unity.UVCS.Execute(ReRequestReview)(reviewId)` to notify
all reviewers that the rework is done and they should
review the changes again.

After re-requesting, append this line to the wrap-up output:

```
Reviewers have been notified to re-review.
```

**Automated mode** (shelves were created):

```
---

**Done with review #<id>.**

| Status | Items | Shelve |
|--------|-------|--------|
| 📦 Shelved | C1 | [sh:101](uvcs://show/shelve/-101) |
| 📦 Shelved | S1 | [sh:102](uvcs://show/shelve/-102) |
| 💬 Answered | Q1 | — |
| ⏭️ Skipped | I1 | — |

Each shelve contains the fix for one comment. To finalize:

1. Apply the shelves you want to keep, or undo the ones you don't.
2. Checkin the applied changes.
3. When you're done, tell me to **re-request review** so reviewers
   are notified.
```

- Every shelved item gets its own row with the shelve number.
- If `ShelvePendingChanges` returns a negative shelve changeset ID, display the
  positive absolute value in the row text while keeping the signed value in the
  `uvcs://` link.
- Answered and skipped items use `—` in the Shelve column.
- Do NOT call `Unity.UVCS.Execute(ReRequestReview)` automatically in Automated mode.
  The user needs to apply and checkin the shelves first. Wait for the user
  to explicitly ask to re-request review.

### Deferred re-request (Automated mode)

After the wrap-up summary, the user will apply shelves and checkin on their
own. When the user says "re-request review", "notify reviewers",
"re-request", or similar:

1. Call `Unity.UVCS.Execute(ReRequestReview)(reviewId)`.
2. Output:
   ```
   Reviewers have been notified to re-review.
   ```

**Both modes:** optionally call
`Unity.UVCS.Execute(GetCodeReviewLink)(reviewId)` and include the
`plastic://` link at the end.

Do NOT re-request in either mode if everything was skipped — the review state
hasn't changed, so there is nothing new for reviewers to look at.

If everything was skipped (nothing applied, nothing discarded, nothing
answered):

> No changes were applied. The review is unchanged. Let me know when you'd
> like to come back to it.

---

## Example: Assisted Walkthrough

**User:** "Fix the review on the current branch"

1. **Phase 0:**
   - `GetWorkspaceFromPath` → `wkInfo` for `/main/fix-inventory`.
   - `GetPendingChanges` → empty list — workspace is clean.
   - `GetWorkspaceConfiguration` → `BranchName = "/main/fix-inventory"`.
   - `GetBranchInfo(wk, "/main/fix-inventory")` → `BranchInfo.Id = 305`.
   - Silent — workspace is good.

2. **Phase 1:**
   - `GetReviewsInStatus(wk, ReworkRequired, 305)` → one review, `Id=42`,
     title "Inventory cleanup", owner `alice`.

   Output:
   ```
   Found one code review needing rework:

   **"Inventory cleanup"** by @alice — loading comments...
   ```

3. **Phase 2:**
   - `GetReviewComments(wk, 42)` → 3 comments.
   - `GetBranchDifferences(wk, "/main/fix-inventory")` → diff list showing
     which files changed in the branch. Use `Diff.RevInfo.Id` to match each
     comment's `RevisionId` and resolve the file `Path`. Cache the mapping.
   - Pending: C1, S1 (2 changes), Q1 (1 question).

   Output:
   ```
   **Code Review #42: "Inventory cleanup"**
   Reviewer: @alice · 3 items pending

   **Changes to make** (code edits + checkin)

   | # | File | Comment |
   |---|------|---------|
   | C1 | /InventoryManager.cs:89 | Add null check before accessing slot |
   | S1 | /ItemData.cs:26 | Clamp Weight to >= 0 |

   **Questions to answer** (reply in the review)

   | # | File | Question |
   |---|------|----------|
   | Q1 | /Player.cs:104 | Is this timeout value intentional? |

   ```

   → `SelectOption`: "How would you like to work through these items?"
     - "Assisted" / "Automated"

   User selects **"Assisted"**.

   Output:
   ```
   You're in control. Here's what you can ask me to do:

   - **"Fix C1"** — I'll read the comment, apply the fix, and ask for your approval before checkin.
   - **"Answer Q1"** — I'll investigate and propose a reply for you to approve.
   - **"Discard S1"** — I'll ask for a reason and post the discard after confirmation.
   - **"Show C1"** — I'll display the full comment details without making changes.
   - **"Status"** — I'll show the current progress (what's done, pending, skipped).

   Just tell me what to do next.
   ```

4. **Phase 3 — Assisted:**

   **User:** "Fix C1"

   → Show comment, read file, apply fix, show diff, wait for "ok".
   → User says "ok" → `AskForConfirmation` → `CheckinPendingChanges`.
   → Output: `✅ C1 checked in. 2 pending.`

   **User:** "discard S1, the field is set internally"

   → Ask for discard reason → user gives reason → `AskForConfirmation`
   → `ReplyComment(..., Discard)`
   → Output: `🚫 S1 discarded. 1 pending.`

   **User:** "do the rest"

   → Output: `Switching to automated mode. I'll shelve each remaining fix automatically.`
   → Investigate Q1, post answer automatically.
   → Output: `💬 Q1 answered. 0 left.`

5. **Phase 4:**

   → `ReRequestReview(wk, 42)` — notifies all reviewers.

   Output:
   ```
   ---

   **Done with review #42.**

   | Status | Items |
   |--------|-------|
   | ✅ Applied | C1 |
   | 🚫 Discarded | S1 |
   | 💬 Answered | Q1 |

   Reviewers have been notified to re-review.
   ```

---

## Example: Automated Walkthrough

**User:** "Fix the review on the current branch"

1. **Phases 0–2:** Same as above — briefing table is shown.

   → `SelectOption`: "How would you like to work through these items?"

   User selects **"Automated"**.

   Output:
   ```
   Working through all items automatically. I'll create a separate shelve
   for each change and post answers to questions. Sit back — I'll report
   when I'm done.
   ```

2. **Phase 3 — Automated — runs without stopping:**

   - Read `/InventoryManager.cs`, apply null check fix.
   - `ShelvePendingChanges(wk, "[apply-change:8123d65a] Add null check on slot access", undoPendingChangesAfterShelve: true)` → `sh:-101`
   - Output: `📦 C1 shelved ([sh:101](uvcs://show/shelve/-101)). 2 left.`

   - Read `/ItemData.cs`, apply weight clamping.
   - `ShelvePendingChanges(wk, "[apply-change:a4e7f012] Clamp Weight to >= 0", undoPendingChangesAfterShelve: true)` → `sh:-102`
   - Output: `📦 S1 shelved ([sh:102](uvcs://show/shelve/-102)). 1 left.`

   - Read `/Player.cs:104`, investigate timeout context.
   - `ReplyComment(wk, 42, Q1.Id, "Yes, the 30s timeout is intentional — it matches the server-side session expiry.", Answer)`
   - Output: `💬 Q1 answered. 0 left.`

3. **Phase 4:**

   Output:
   ```
   ---

   **Done with review #42.**

   | Status | Items | Shelve |
   |--------|-------|--------|
| 📦 Shelved | C1 | [sh:101](uvcs://show/shelve/-101) |
| 📦 Shelved | S1 | [sh:102](uvcs://show/shelve/-102) |
   | 💬 Answered | Q1 | — |

   Each shelve contains the fix for one comment. To finalize:

   1. Apply the shelves you want to keep, or undo the ones you don't.
   2. Checkin the applied changes.
   3. When you're done, tell me to **re-request review** so reviewers
      are notified.
   ```

4. **User:** "re-request review"

   → `ReRequestReview(wk, 42)` — notifies all reviewers.
   → Output: `Reviewers have been notified to re-review.`

---

## Example: Switching Branches When Wrong Workspace

**User:** "Fix the review on /main/feature-x"

1. Phase 0:
   - `GetWorkspaceConfiguration` → workspace is on `/main/feature-y`.
   - User explicitly named `/main/feature-x` → mismatch.
   - `AskForConfirmation`:
     - Action: `"Switch workspace?"`
     - Detail: `"Review is on /main/feature-x, but your workspace is on /main/feature-y. Switch?"`
2. User confirms → `LaunchSwitchToBranchUI(wk, "/main/feature-x")` → say
   "Switching… re-run when ready." STOP.
