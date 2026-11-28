# Ignore Private Files Workflow

Workflow for configuring UVCS ignore rules to exclude private and local files
from version control tracking.

## Table of Contents

- [Workflow: Ignore Private Files](#workflow-ignore-private-files)

---

## Workflow: Ignore Private Files

UVCS uses an `ignore.conf` file at the workspace root to define ignore rules,
similar to `.gitignore` in Git. Files matching these rules are treated as
private (not tracked by version control).

### Rule Syntax

One rule per line:

- `Library` — exact name match (file or directory).
- `*.csproj` — wildcard pattern to ignore by extension.
- `**/Assets/AssetStoreTools` — `**/` prefix for recursive match from any depth.
- `/Assets/Plugins/PlasticSCM*` — `/` prefix anchors to the workspace root.
- `~UnityDirMonSyncFile~*` — `~` prefix/suffix for temp file patterns.
- `^*.private.[0-9]+$` — regex pattern (anchored with `^` and `$`).
- `.DS_Store*` — ignore OS-generated files.
- Rules are **case-sensitive**. To ignore both `Library` and `library`, add
  both as separate lines.
- Empty lines and whitespace-only lines are ignored.

### Example ignore.conf

```
Library
library
Temp
temp
UserSettings
usersettings
Logs
logs
*.csproj
*.sln
*.suo
.vs
.vscode
.idea
.DS_Store*
Thumbs.db
```

### Steps

1. If the user asks to ignore files, check if `ignore.conf` exists at the
   workspace root. If not, create it.
2. Read the current `ignore.conf` to avoid duplicating rules.
3. Add the new rule(s) at the end of the file. Remember to add both
   the original casing and the lowercase variant if needed.
4. Inform the user that newly ignored files will no longer appear in pending
   changes.

### Tips

- If the user has no controlled pending changes but sees private files
  cluttering their workspace, suggest adding ignore rules for those files or
  patterns.
