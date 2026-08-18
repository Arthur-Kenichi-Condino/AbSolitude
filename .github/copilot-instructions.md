<!--
		Repository: AbSolitude
		Purpose: provide concise, repository-specific guidance for AI coding agents (Copilot/assistants)
		Keep this file short and actionable. Avoid high-level generic coaching — focus on patterns, files, and commands
-->

# Copilot instructions for AbSolitude

Regra permanente: não editar, criar, mover ou apagar arquivos sem minha aprovação explícita: e se eu pedir alguma edição, antes de qualquer mudança, me mostrar o plano de diff proposto.
IMPORTANTÍSSIMO: tentar medir o próprio uso de tokens e economizar o máximo possível de tokens/uso da AI para que possa usar o plano gratuito o máximo possível após cada renovação da semana.

## Context

- Unity project using .NET Standard 2.1 assemblies. Solution: `AbSolitude.slnx`.
- Runtime code lives under `Assets/`.
- Editor-only code belongs under `Assets/**/Editor/` or must be guarded by `#if UNITY_EDITOR`.
- UI code: `Assets/AKCondinoO.UIObjects/`.
- World/biome code: `Assets/AKCondinoO.World/`.
- The Visual Studio solution is for editing; Unity manages generated `.csproj` files.

## Editing rules

- NEVER create, delete, move, rename, or modify files without explicit user approval.
- If the user requests an edit, FIRST show the proposed diff/plan and wait for approval before making changes.
- Prefer the smallest possible diff.
- Preserve the existing architecture and conventions unless the user explicitly requests refactoring.
- Do not modify Unity project settings or generated files unless explicitly requested and justified.

## Investigation / debugging mode

When investigating a bug, optimize for finding the actual cause quickly, not for performing a repository-wide audit.

1. Start with the reported symptom and the code provided by the user.
2. Trace the smallest execution path that can produce the symptom.
3. Form ONE concrete hypothesis before expanding the search.
4. Test that hypothesis against the available code.
5. Prefer simple local causes:
   - execution order
   - incorrect condition/value
   - state transition
   - lock scope
   - race condition
   - deadlock
   - offset/stream position
   - serialization version
   - null/disposal/lifetime
   - unexpected call
6. Do NOT broadly scan the repository looking for possible problems.
7. Do NOT inspect unrelated files "just in case".
8. Inspect another file only when a concrete dependency is required to confirm or reject the current hypothesis.
9. If the current hypothesis explains the symptom, STOP investigating and report it.
10. Do not turn a local bug into an architecture review.
11. Do not invent hypothetical bugs or list unrelated improvements.
12. Do not refactor, abstract, optimize, or redesign unless required to fix the reported problem.
13. Prefer the smallest practical correction that preserves the existing design.
14. Do not keep investigating after finding a sufficient causal explanation.

### Evidence discipline

Always distinguish:

- **CONFIRMED:** directly demonstrated by the code/control flow.
- **LIKELY:** strongly supported but requiring runtime confirmation.
- **UNKNOWN:** insufficient evidence.

Never present a hypothesis as a confirmed bug.

If evidence is insufficient, state exactly what information is missing and why it is needed. Do not compensate by scanning the entire repository.

## Concurrency

For race conditions:

- Identify the exact operations that can execute concurrently.
- Identify the exact interval between them where the race occurs.
- Determine which lock protects each operation.
- Check whether the lock protects the ENTIRE operation that must be atomic.
- Do not assume that two individually locked operations are atomic as a combined sequence.
- Pay special attention to `ReaderWriterLockSlim` upgradeable/read/write transitions and nested lock acquisition.
- Check lock ordering when multiple locks are involved.
- Prefer fixing the smallest incorrect critical section rather than redesigning the synchronization system.

For deadlocks:

1. Identify the thread/operation that holds each lock.
2. Identify what it attempts to acquire next.
3. Look for lock-order inversion or reentrant acquisition.
4. Trace only the relevant call chain.
5. Do not blame unrelated locks without a concrete acquisition path.

## Persistence / serialization

When debugging persistent data:

- Explicitly track:
  - `Stream.Position`
  - `Stream.Length`
  - record `offset`
  - `serializationSize`
  - format `version`
- For WRITE:
  - use the current application format version.
- For READ:
  - obtain the record's stored version first;
  - then pass that version to the appropriate serializer.
- Never confuse the current write version with the version stored in an old record.
- When checking offsets, account for every serialized field in the exact order written.
- If a record contains its own size, use that size to determine its boundary rather than assuming the current format size.
- When debugging "end of stream", "corrupt data", or incorrect deserialization, first verify the exact byte layout and stream position before investigating unrelated code.

## Pools / dictionaries / shared state

Before proposing structural changes:

- Check ownership of pooled objects.
- Check every `Rent` / `Return` path relevant to the symptom.
- Check whether pooled state is cleared before reuse.
- Check synchronization around shared dictionaries and mutable state.
- Check whether a reference escapes its intended lifetime.
- Do not replace existing collections or pooling mechanisms unless they are demonstrated to cause the problem.

## Search discipline

Minimize tool usage and repository exploration.

Preferred order:

1. User-provided code.
2. Directly referenced methods/types.
3. Immediate caller/callee when required.
4. Only then, additional files required to verify a concrete dependency.

Do NOT search the whole repository when the supplied code already contains enough evidence.

When opening another file, have a specific reason:
"Need X from file Y because method Z calls it and this determines whether hypothesis H is possible."

Do not search for unrelated examples, similar implementations, or architectural patterns unless the current problem genuinely requires them.

## Solution discipline

When the cause is found, stop searching.

Return:

### Cause
1–3 concise sentences describing the actual causal mechanism.

### Evidence
Only the relevant code path or state transition.

### Fix
The smallest concrete change required.

### Why it works
Brief explanation connecting the fix to the symptom.

### Confidence
`CONFIRMED`, `LIKELY`, or `UNKNOWN`, with a short reason.

If the user asks for a code change:

1. Show the proposed diff/plan.
2. Wait for explicit approval.
3. Only then modify files.

## Token / resource discipline

Optimize for useful work, not verbose reasoning.

- Minimize files opened.
- Minimize searches.
- Minimize repeated inspection of the same code.
- Avoid duplicate analyses.
- Avoid long explanations when a short causal explanation is sufficient.
- Do not output internal chain-of-thought.
- Provide the relevant conclusion and evidence, not a transcript of the investigation.
- Prefer one well-tested hypothesis over many speculative hypotheses.
- Stop as soon as the current bug is sufficiently explained.

The objective is:

SYMPTOM
→ LOCAL EXECUTION PATH
→ ONE HYPOTHESIS
→ VERIFY
→ ROOT CAUSE
→ MINIMAL FIX
→ STOP

Not:

SYMPTOM
→ REPOSITORY-WIDE SEARCH
→ MANY POSSIBLE PROBLEMS
→ ARCHITECTURAL REVIEW
→ SPECULATION
→ NO CONCRETE FIX

## Coding style and repetitive changes

Preserve the user's existing code formatting and coding style exactly unless explicitly asked to change it.

The preferred style is compact:
- Do not add unnecessary blank lines.
- Do not reformat surrounding code.
- Do not expand compact expressions merely for stylistic preference.
- Preserve indentation, brace style, spacing, naming conventions, and member ordering already used in the surrounding code.
- When adding code, match the formatting of the immediately surrounding code.
- Do not apply automatic formatting to unrelated lines.
- A smaller diff is preferred over stylistic normalization.

When the user needs the same change in many places:

1. Prefer the fastest safe way to apply the repetition consistently.
2. Identify whether the changes are truly repetitive or whether each occurrence has meaningful differences.
3. If the same manual change would need to be repeated across many files or classes, consider whether the code can be structured so that the behavior is defined in one place instead.
4. Before proposing a structural solution, determine whether it would actually reduce future duplication without unnecessarily increasing complexity.
5. Do not refactor merely to eliminate a small amount of repetition.

If many repetitive edits are required, proactively tell the user when using the coding agent, a scripted transformation, search/replace, or another automated approach would be faster and safer than manually editing every occurrence.

When suggesting automation:
- Explain what pattern can be automated.
- Explain the scope of the change.
- Warn about any cases that may require manual review.
- Prefer deterministic transformations over broad speculative rewrites.

## Duplication / single-source-of-truth heuristic

When the user repeatedly has to make the same conceptual change in many locations, consider whether the problem is not the repetition itself but the location of the abstraction.

Ask:

"Can this behavior be defined once so future changes require modifying only one place?"

If yes, briefly suggest the option, but do not force a refactor.

Prefer simple solutions such as:
- shared helper methods;
- common serializers;
- base classes;
- interfaces;
- centralized constants/configuration;
- generic methods;
- data-driven tables;
- version dispatch;
- reusable validation;
- code generation or scripted transformations when appropriate.

Avoid introducing abstractions when they are more complex than the repetition they eliminate.

## Repetition efficiency

For large repetitive changes, optimize for:
correctness + consistency + minimal manual work.

If the user is about to perform the same mechanical edit many times, warn them before they start and suggest a safer automated approach.

Example:

"These changes are mechanically repetitive across 20 serializers. Rather than editing them individually, we can apply one deterministic transformation with the agent/script and then review the diff."

Do not perform the automated change without the user's approval.

## Repeated-change detection

When reviewing the user's planned changes, distinguish between:

A. Intentional repetition:
   The code is naturally repetitive and centralizing it would make the design worse.

B. Accidental repetition:
   The same rule, format, or behavior is duplicated in multiple places and future changes will require changing all of them.

If B is detected, briefly flag it and suggest a single-source-of-truth design.

Do not automatically refactor it. Let the user decide whether the reduction in future maintenance justifies the change.

If a centralized solution is proposed, prefer the simplest abstraction that removes the repetition. Do not introduce generic frameworks, factories, registries, reflection, dependency injection, or additional layers unless they provide a concrete benefit for the current codebase.

## Formatting preference

The user intentionally prefers compact C# formatting.

Example preferred style:

if(condition){
 DoSomething();
}else{
 DoSomethingElse();
}

Do NOT automatically transform it into:

if (condition)
{
    DoSomething();
}
else
{
    DoSomethingElse();
}

Preserve the user's compact formatting when modifying existing code.

Do not insert blank lines between logically adjacent members unless the surrounding code already uses them.

Do not reformat code merely because another style is considered more conventional.

---
If this file misses any repository-specific conventions you rely on, please edit and commit the addition or tell the assistant which details to add.
