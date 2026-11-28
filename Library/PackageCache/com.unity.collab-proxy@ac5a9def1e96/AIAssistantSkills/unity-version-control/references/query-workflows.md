# Query Workflows

Workflow for searching and querying UVCS objects (branches, changesets, labels,
shelves, revisions) using the UVCS query language.

For query syntax details (field names, operators, date constants, and examples),
see [query-syntax.md](query-syntax.md).

## Table of Contents

- [Workflow: Query UVCS Objects](#workflow-query-uvcs-objects)

---

## Workflow: Query UVCS Objects

### Steps

1. **Read the query syntax reference** at
   [query-syntax.md](query-syntax.md) to understand the full query language
   before constructing a query. This is required even for simple queries,
   because field names and sortable fields are object-specific.
2. **Validate the target object before writing the query:**
   - Confirm the object type exists.
   - Confirm every `where` field is listed for that object.
   - Confirm any `order by` field is listed as sortable for that object.
   - If sorting is not supported, omit `order by` and sort/filter the returned
     results yourself.
3. **Construct the query** using the correct object type, fields, operators,
   and date constants as documented in the reference.
4. **Call `Unity.UVCS.Execute(FindQuery)`** with the constructed query string.
5. **Present the results** clearly to the user.
   - For changeset results, use `changesetId`/`ChangesetId` as the displayed
     and linked changeset number. Do not use `ID`/`Id`; that is an internal
     UVCS object ID.
   - For shelve results, use the shelve `changesetId`/`ChangesetId` as the
     canonical identifier. Keep its signed value internally, but display the
     positive absolute value to the user.

### Critical Rules

- **Use `Unity.UVCS.Execute(FindQuery)` for all queries — NEVER use Git commands.** Even
  if the question sounds like a typical `git log` or `git branch` request, the
  answer MUST come from UVCS tools when the project is under UVCS.
- ALWAYS consult [query-syntax.md](query-syntax.md) before building a query to
  ensure correct field names and syntax.
- All queries are **case-sensitive**.
- Use `'me'` for the current user in `owner` / `assignee` fields.
- Use date constants like `'today'`, `'one week ago'`, `'3 months ago'` for
  time-based filters.
- The query parameter must start with the keyword `find` followed by the object
  type and optional where/order/limit clauses (e.g.
  `find branch where date > 'one week ago'`).
- Do not retry failed queries by guessing alternate field names or sort fields.
  Re-check [query-syntax.md](query-syntax.md), then either issue one corrected
  query or explain that the requested query shape is unsupported.

### Common Pitfall: Latest Shelve

Shelves have `date` and `shelveid` fields for filtering/results, but the
`shelve` object does **not** support `order by`. These queries are invalid:

```
find shelve order by date desc limit 1
find shelve order by id desc limit 1
```

Use an unsorted query, then pick the newest returned result locally:

```
find shelve
find shelve where owner = 'me'
```

For a request like "show a link to the latest created shelve and display it in
the UI", query shelves, select the newest result from the returned `date` or
`changesetId`/`shelveid`, format it as
`[shelve N](uvcs://show/shelve/-N)` when the shelve changeset ID is negative,
then call `Unity.UVCS.Execute(ShowShelvesView)` with that signed shelve changeset ID.
