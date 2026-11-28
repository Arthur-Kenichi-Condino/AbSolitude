# UVCS Query Syntax Reference

Comprehensive reference for the `Unity.UVCS.Execute(FindQuery)` tool query language.

## Table of Contents

- [General Syntax](#general-syntax)
- [Available Objects](#available-objects)
- [Where Clause](#where-clause)
- [Date Constants](#date-constants)
- [Object Spec Syntax](#object-spec-syntax)
- [Sorting](#sorting)
- [Pagination](#pagination)
- [Object Fields Reference](#object-fields-reference)
  - [attribute](#attribute)
  - [attributetype](#attributetype)
  - [branch](#branch)
  - [changeset](#changeset)
  - [label](#label)
  - [merge](#merge)
  - [replicationlog](#replicationlog)
  - [review](#review)
  - [revision](#revision)
  - [shelve](#shelve)
- [Replication Fields](#replication-fields)
- [Complete Query Examples](#complete-query-examples)

## General Syntax

```
find object [where conditions] [order by field [asc|desc]] [limit N] [offset N]
```

- **find**: Required keyword that starts every query.
- **object**: The type of UVCS object to search for (e.g. `branch`, `changeset`,
  `label`, `revision`, etc.).
- **where conditions**: Optional filtering expressions.
- **order by**: Optional sorting clause (only supported by some objects).
- **limit / offset**: Optional pagination.

You can use singular or plural forms interchangeably (e.g. `branch` or
`branches`).

**All queries are case-sensitive.**

**IMPORTANT — Clause order is mandatory.** The clauses must appear in exactly
this order: `find` → object → `where` → `order by` → `limit` → `offset`.
You cannot rearrange them. For example:

```
WRONG: find branch limit 5 order by date desc
RIGHT: find branch order by date desc limit 5
```

---

## Available Objects

| Object           | Description                                      |
|------------------|--------------------------------------------------|
| `attribute`      | Attributes applied to objects                    |
| `attributetype`  | Attribute type definitions                       |
| `branch`         | Branches in the repository                       |
| `changeset`      | Changesets (commits)                             |
| `label`          | Labels (tags) applied to changesets              |
| `merge`          | Merge operations                                 |
| `replicationlog` | Replication log entries                          |
| `review`         | Code reviews                                     |
| `revision`       | File revisions                                   |
| `shelve`         | Shelved changes                                  |

---

## Where Clause

Use `where` followed by one or more conditions joined by `and` / `or`.

### Comparison Operators

| Operator   | Meaning                  | Example                                  |
|------------|--------------------------|------------------------------------------|
| `=`        | Equal                    | `owner = 'me'`                           |
| `!=`       | Not equal                | `owner != 'me'`                          |
| `>`        | Greater than             | `date > 'one week ago'`                  |
| `<`        | Less than                | `date < 'yesterday'`                     |
| `>=`       | Greater than or equal    | `changesets >= 'today'`                  |
| `<=`       | Less than or equal       | `date <= 'this month'`                   |
| `like`     | Pattern match (% = wildcard) | `name like 'scm%'`                  |
| `between`  | Range (inclusive)        | `date between '3 days ago' and 'today'`  |

### Logical Operators

- `and` — both conditions must be true.
- `or` — either condition can be true.

### String Values

Enclose string values in single quotes: `'value'`.

### Special User Value

Use `'me'` as a shortcut for the current user in `owner` and `assignee`
fields:
```
find changeset where owner = 'me'
find review where assignee = 'me'
```

### Boolean Values

Boolean fields accept `'true'` and `'false'` (case-insensitive). Some fields
also accept the shorthand `'T'` and `'F'`:
```
find branch where hidden = 'true'
find replicationlog where package = 'T'
```

---

## Date Constants

For any field of type `date`, you can use these constants instead of literal
dates:

| Constant          | Meaning                                    |
|-------------------|--------------------------------------------|
| `'today'`         | Today's date                               |
| `'yesterday'`     | Yesterday's date                           |
| `'this week'`     | Monday of the current week                 |
| `'this month'`    | First day of the current month             |
| `'this year'`     | January 1st of the current year            |
| `'one day ago'`   | One day before the current date            |
| `'one week ago'`  | Seven days before the current date         |
| `'one month ago'` | One month before the current date          |
| `'n days ago'`    | *n* days before the current date           |
| `'n months ago'`  | *n* months before the current date         |
| `'n years ago'`   | *n* years before the current date          |

You can also use literal date strings that match the machine locale, e.g.
`'2024/01/15'`.

### Date Examples

```
find branch where date > 'one week ago'
find changeset where date >= 'today'
find label where date between '3 days ago' and 'one day ago'
find changeset where date < 'one month ago' and date > '3 months ago'
find revision where date > '1 year ago'
```

---

## Object Spec Syntax

Some fields accept **object specs** — a formatted reference to a UVCS object:

| Prefix   | Object type | Example                              |
|----------|-------------|--------------------------------------|
| `br:`    | Branch      | `br:/main/scm23343`                 |
| `cs:`    | Changeset   | `cs:30`                              |
| `lb:`    | Label       | `lb:v0.14.1`                         |
| `item:`  | Item (file) | `item:readme.txt`                    |
| `rev:`   | Revision    | `rev:readme.txt#br:/main/task002`    |
| `revid:` | Revision ID | `rev:revid:1126`                    |

---

## Sorting

Use `order by` to sort results by a specific field. Add `asc` (ascending,
default) or `desc` (descending).

**IMPORTANT — Sort fields have dedicated names.** The field name used in
`order by` is NOT always the same as the field name used in `where` clauses.
For example, you filter branches with `name`, but sort them with `branchname`.
The table below lists the exact sort field names to use.

### Sortable Fields by Object

| Object      | Sort field name  | Description                       |
|-------------|------------------|-----------------------------------|
| `branch`    | `date`           | Branch creation date              |
| `branch`    | `branchname`     | Branch name (alphabetical)        |
| `changeset` | `date`           | Changeset creation date           |
| `changeset` | `changesetid`    | Changeset numeric ID              |
| `label`     | `date`           | Label creation date               |
| `label`     | `labelname`      | Label name (alphabetical)         |
| `review`    | `date`           | Review creation date              |
| `review`    | `modifieddate`   | Review last-modified date         |
| `review`    | `status`         | Review status (alphabetical)      |

**IMPORTANT:** Only `branch`, `changeset`, `label`, and `review` support
`order by`. All other objects (`attribute`, `attributetype`, `merge`,
`replicationlog`, `revision`, `shelve`) do **NOT** support sorting — omit the
`order by` clause for these types.

### Sort Examples

```
find changeset where owner = 'me' order by date desc
find branch where date > '2024/01/01' order by branchname asc
find label where owner = 'me' order by labelname desc
find review where assignee = 'me' order by modifieddate desc
```

---

## Pagination

| Clause     | Description                                                |
|------------|------------------------------------------------------------|
| `limit N`  | Return at most *N* results.                                |
| `offset N` | Skip the first *N* results (start from result *N + 1*).   |

### Pagination Examples

```
find label where owner = 'me' limit 10 offset 20
find branch where owner = 'me' limit 10
```

---

## Object Fields Reference

Each object section below lists the fields available for filtering with `where`
clauses, whether sorting is supported (and with which field names), and whether
replication fields are available.

### `attribute`

| Field     | Type    | Notes                                                   |
|-----------|---------|---------------------------------------------------------|
| `type`    | string  | Attribute type name                                     |
| `value`   | string  | Attribute value                                         |
| `date`    | date    | When the attribute was applied                          |
| `owner`   | user    | Who applied it; supports `'me'`                         |
| `GUID`    | GUID    | Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`          |
| `comment` | string  | Comment text                                            |
| `srcobj`  | objspec | Source object (item, branch, changeset, revision, label)|
| `ID`      | integer | Object ID                                               |

**Sorting:** Not supported. Do not use `order by` with attribute queries.
**Replication fields:** Not supported.

#### Examples

```
find attribute where type = 'status'
find attribute where date > 'this week'
find attribute where date > 'today'
find attribute where value = 'resolved' and owner = 'me'
find attribute where srcobj = 'item:readme.txt'
find attribute where srcobj = 'br:/main/scm23343'
find attribute where srcobj = 'rev:readme.txt#br:/main/task002'
find attribute where srcobj = 'rev:revid:1126'
```

---

### `attributetype`

| Field     | Type    | Notes                                                   |
|-----------|---------|---------------------------------------------------------|
| `name`    | string  | Attribute type name                                     |
| `value`   | string  | Value                                                   |
| `date`    | date    | Creation date                                           |
| `owner`   | user    | Creator; supports `'me'`                                |
| `GUID`    | GUID    | Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`          |
| `comment` | string  | Comment text                                            |
| `source`  | objspec | Source object (item, branch, changeset, or label)       |
| `ID`      | integer | Object ID                                               |

**Sorting:** Not supported. Do not use `order by` with attributetype queries.
**Replication fields:** Supported. See [Replication Fields](#replication-fields).

#### Examples

```
find attributetype where name like 'st%'
find attributetype where date > 'today'
find attributetype where comment != ''
find attributetype where source = 'item:readme.txt'
find attributetype where source = 'cs:30'
find attributetype where source = 'lb:v0.14.1'
```

---

### `branch`

| Field       | Type    | Notes                                                 |
|-------------|---------|-------------------------------------------------------|
| `name`      | string  | Branch name                                           |
| `date`      | date    | Branch creation date                                  |
| `changesets`| date    | Filters branches by the creation date of their changesets. E.g., `changesets >= 'today'` finds branches that have changesets created today. |
| `attribute` | string  | Attribute name applied to the branch                  |
| `attrvalue` | string  | Value of the attribute on the branch                  |
| `owner`     | user    | Branch creator; supports `'me'`                       |
| `parent`    | brspec  | Parent branch (use object spec `br:/main`)            |
| `comment`   | string  | Branch comment                                        |
| `hidden`    | boolean | Filter to hidden branches. `'true'` returns only hidden branches; omit to return only visible ones. |
| `GUID`      | GUID    | Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`        |
| `ID`        | integer | Branch ID                                             |

**Sorting:** Supported. Sort field names: `date`, `branchname`.
**Replication fields:** Supported. See [Replication Fields](#replication-fields).

#### Examples

```
find branch where name like 'scm23%'
find branch where date > 'one week ago'
find branch where changesets >= 'today'
find branch where attribute = 'status' and attrvalue = 'failed'
find branch where owner != 'me' and parent != 'br:/main'
find branch where id = 2029607
find branch where hidden = 'true'
```

---

### `changeset`

| Field              | Type    | Notes                                            |
|--------------------|---------|--------------------------------------------------|
| `branch`           | brspec  | Branch the changeset belongs to                  |
| `changesetid`      | integer | Changeset number                                 |
| `attribute`        | string  | Attribute name applied to the changeset          |
| `attrvalue`        | string  | Value of the attribute                           |
| `date`             | date    | Changeset creation date                          |
| `owner`            | user    | Who created it; supports `'me'`                  |
| `GUID`             | GUID    | Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`   |
| `comment`          | string  | Changeset comment                                |
| `ignorehidden`     | boolean | By default, changesets from hidden branches are excluded. Set to `'false'` to include them. |
| `onlywithrevisions`| boolean | Filter by whether a changeset has file revisions. `'true'` = only changesets with revisions; `'false'` = only those without. |
| `returnparent`     | boolean | Set `'true'` to return the parent changeset      |
| `parent`           | integer | Parent changeset ID                              |
| `ID`               | integer | Object ID                                        |

**Sorting:** Supported. Sort field names: `date`, `changesetid`.
**Replication fields:** Supported. See [Replication Fields](#replication-fields).

**Display rule:** Query result objects may include both `ID` and
`changesetId`/`ChangesetId`. `ID` is UVCS's internal object ID. When presenting
changesets, selecting changesets in the UI, or building `uvcs://` links, use the
visible `changesetId`/`ChangesetId` value instead.

#### Examples

```
find changeset where branch = '/main/scm23119'
find changeset where attribute = 'status'
find changeset where date >= '2024/06/08' and owner != 'me'
find changeset where guid = '1b30674f-14cc-4fd7-962b-676c8a6f5cb6'
find changeset where comment = ''
find changeset where onlywithrevisions = 'false'
find changeset where changesetid = 29 and returnparent = 'true'
find changeset where parent = 548
```

---

### `label`

| Field       | Type    | Notes                                              |
|-------------|---------|----------------------------------------------------|
| `name`      | string  | Label name                                         |
| `attribute` | string  | Attribute name applied to the label                |
| `attrvalue` | string  | Value of the attribute                             |
| `date`      | date    | Label creation date                                |
| `owner`     | user    | Label creator; supports `'me'`                     |
| `GUID`      | GUID    | Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`     |
| `branch`    | brspec  | Branch the label is applied to                     |
| `branchid`  | integer | Branch ID                                          |
| `changeset` | integer | Changeset ID the label is applied to               |
| `comment`   | string  | Label comment                                      |
| `ID`        | integer | Object ID                                          |

**Sorting:** Supported. Sort field names: `date`, `labelname`.
**Replication fields:** Supported. See [Replication Fields](#replication-fields).

#### Examples

```
find label where name like '7.0.16.%'
find label where date >= 'this month' and attribute = 'publish-status' and attrvalue != 'PUBLISHED'
find label where branch = '/main'
find label where changeset = 111733
```

---

### `merge`

| Field          | Type    | Notes                                           |
|----------------|---------|-------------------------------------------------|
| `srcbranch`    | brspec  | Source branch of the merge                      |
| `srcchangeset` | integer | Source changeset ID                             |
| `dstbranch`    | brspec  | Destination branch of the merge                 |
| `dstchangeset` | integer | Destination changeset ID                        |
| `date`         | date    | Merge date                                      |
| `owner`        | user    | Who performed the merge; supports `'me'`        |
| `GUID`         | GUID    | Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`  |
| `type`         | string  | See merge type values below                     |
| `ID`           | integer | Object ID                                       |

**Merge type values:** `merge`, `cherrypick`, `cherrypicksubstractive`,
`interval`, `intervalcherrypick`, `intervalcherrypicksubstractive`

**Sorting:** Not supported. Do not use `order by` with merge queries.
**Replication fields:** Not supported.

#### Examples

```
find merge where srcbranch = 'br:/main'
find merge where dstchangeset = 108261
find merge where type = 'cherrypick' and owner = 'me'
```

---

### `replicationlog`

| Field            | Type    | Notes                                        |
|------------------|---------|----------------------------------------------|
| `branch`         | brspec  | Replicated branch                            |
| `repositoryname` | string  | Repository name                              |
| `owner`          | user    | Who performed the replication; supports `'me'`|
| `date`           | date    | Replication date                             |
| `server`         | string  | Server address                               |
| `package`        | boolean | Whether it was a package replication         |
| `ID`             | integer | Object ID                                    |

**Sorting:** Not supported. Do not use `order by` with replicationlog queries.
**Replication fields:** Not applicable (this object *is* the replication log).

#### Examples

```
find replicationlog where branch = '/main/gm22358'
find replicationlog where package = 'T' and server like '%cloud%'
find replicationlog where date > 'one week ago'
```

---

### `review`

| Field        | Type    | Notes                                          |
|--------------|---------|-------------------------------------------------|
| `status`     | string  | Review status (e.g. `'pending'`)                |
| `assignee`   | string  | Assigned reviewer                               |
| `title`      | string  | Review title                                    |
| `target`     | objspec | Target object (branch or changeset spec)        |
| `targetid`   | integer | Target object ID                                |
| `targettype` | string  | `'branch'` or `'changeset'`                     |
| `date`       | date    | Review creation date                            |
| `owner`      | user    | Review creator; supports `'me'`                 |
| `GUID`       | GUID    | Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`  |
| `ID`         | integer | Object ID                                       |

**Sorting:** Supported. Sort field names: `date`, `modifieddate`, `status`.
**Replication fields:** Not supported.

#### Examples

```
find review where status = 'pending' and assignee = 'me'
find review where target = 'br:/main/scm17932'
find review where targettype = 'changeset'
```

---

### `revision`

| Field                | Type    | Notes                                        |
|----------------------|---------|----------------------------------------------|
| `branch`             | brspec  | Branch the revision belongs to               |
| `changeset`          | integer | Changeset ID                                 |
| `item`               | string/int | Item path or item ID                      |
| `itemid`             | integer | Item ID                                      |
| `attribute`          | string  | Attribute name                               |
| `attrvalue`          | string  | Attribute value                              |
| `archived`           | boolean | Whether the revision is archived in external storage |
| `comment`            | string  | Comment text                                 |
| `date`               | date    | Revision creation date                       |
| `GUID`               | GUID    | Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`|
| `owner`              | user    | Creator; supports `'me'`                     |
| `parent`             | integer | Parent revision ID                           |
| `returnparent`       | boolean | Set `'true'` to return parent revision       |
| `shelve`             | integer | Shelve ID                                    |
| `size`               | integer | File size in bytes                           |
| `type`               | string  | `'dir'`, `'bin'`, or `'txt'`                 |
| `workspacecheckoutid`| integer | Workspace checkout ID                        |
| `ID`                 | integer | Object ID                                    |

**Sorting:** Not supported. Do not use `order by` with revision queries.
**Replication fields:** Supported. See [Replication Fields](#replication-fields).

#### Examples

```
find revision where changeset >= 111756
find revision where item = 'readme.txt' or itemid = 2250
find revision where item = 'readme.txt' or item = 2250
find revision where attribute = 'status' and attrvalue != 'open'
find revision where archived = 'true'
find revision where type = 'txt' and size > 300000 and owner = 'me' and date >= '2 months ago'
```

---

### `shelve`

| Field      | Type    | Notes                                           |
|------------|---------|-------------------------------------------------|
| `owner`    | user    | Creator; supports `'me'`                        |
| `date`     | date    | Shelve creation date                            |
| `attribute`| string  | Attribute name                                  |
| `attrvalue`| string  | Attribute value                                 |
| `comment`  | string  | Comment text                                    |
| `GUID`     | GUID    | Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`  |
| `parent`   | integer | Parent ID                                       |
| `shelveid` | integer | Shelve numeric ID                               |
| `ID`       | integer | Object ID                                       |

**Sorting:** Not supported. Do not use `order by` with shelve queries.
**Replication fields:** Supported. See [Replication Fields](#replication-fields).

**Display rule:** Shelve result objects may include both `ID` and
`changesetId`/`ChangesetId`. `ID` is UVCS's internal object ID. Use the
shelve `changesetId`/`ChangesetId` as the canonical shelve identifier. Because
shelve changeset IDs are negative internally, keep the negative value for tool
parameters and `uvcs://` links, but display the positive absolute value to the
user.

To find the latest shelve, run an unsorted `find shelve` query and choose the
newest result from the returned `date` or shelve `changesetId`/`shelveid`
fields locally. Do not use `order by date`, `order by id`, or `order by
shelveid` with shelves.

#### Examples

```
find shelve where owner != 'me' and date >= '1 years ago'
find shelve where shelveid = 2
```

---

## Replication Fields

Many objects track replication data, meaning UVCS records where they were
originally created. Replicated objects retain their original creation date, so
these fields are useful when you need to find objects based on when or where
they were replicated.

| Field               | Type    | Notes                                            |
|---------------------|---------|--------------------------------------------------|
| `ReplSrcServer`     | repspec | Server the object was replicated from            |
| `ReplSrcRepository` | string  | Repository the object was replicated from        |
| `ReplLogId`         | integer | ID of the replication operation                  |
| `ReplSrcDate`       | date    | Date when the replication took place             |
| `ReplSrcId`         | integer | ID of the replication source server              |

**Supported on:** `attributetype`, `branch`, `changeset`, `label`, `revision`,
`shelve`

**Not supported on:** `attribute`, `merge`, `replicationlog`, `review`

### Discovering Replication Source IDs

You can query `replicationsource` objects to discover the `ReplSrcId` values
for use in other queries:
```
find replicationsource
```

This returns entries like:
```
7860739 codice@AFRODITA:8087 d9c4372a-dc55-4fdc-ad3d-baeb2e975f27
8175854 codice@BACKYARD:8087 66700d3a-036b-4b9a-a26f-adfc336b14f9
```

You can then use the ID to find objects replicated from that source:
```
find changeset where replsrcid = 7860739
```

### Replication Examples

```
find branch where replsrcserver = 'skull.codicefactory.com:9095'
find branch where replsrcserver = 'skull.codicefactory.com:9095' and replsrcrepository = 'codice'
find revision where repllogid = 2019974
find label where replsrcdate >= 'one month ago' and date >= '15 days ago'
find changeset where replsrcid = 7860739
```

---

## Complete Query Examples

Find branches created in the last week:
```
find branch where date > 'one week ago'
```

Find my pending code reviews:
```
find review where status = 'pending' and assignee = 'me'
```

Find changesets on a branch, sorted newest first:
```
find changeset where branch = '/main' order by date desc
```

Find the 10 most recent labels I created:
```
find label where owner = 'me' order by date desc limit 10
```

Find large text files I modified in the last 2 months:
```
find revision where type = 'txt' and size > 300000 and owner = 'me' and date >= '2 months ago'
```

Find all cherry-pick merges performed by me:
```
find merge where type = 'cherrypick' and owner = 'me'
```

Find branches with a specific attribute value:
```
find branch where attribute = 'status' and attrvalue = 'failed'
```

Find changesets with no comments:
```
find changeset where comment = ''
```

Find shelves created by others in the last year:
```
find shelve where owner != 'me' and date >= '1 years ago'
```

Find labels within a date range using `between`:
```
find label where date between '3 days ago' and 'today'
```

Find branches that are not children of `/main`:
```
find branch where owner != 'me' and parent != 'br:/main'
```

Find hidden branches:
```
find branch where hidden = 'true'
```

Find changesets without revisions (empty changesets):
```
find changeset where onlywithrevisions = 'false'
```

Find the parent of a specific changeset:
```
find changeset where changesetid = 29 and returnparent = 'true'
```

Find all children changesets of a specific changeset:
```
find changeset where parent = 548
```
