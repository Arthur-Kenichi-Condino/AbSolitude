from pathlib import Path

PROJECT_ROOT_RELATIVE = "../"

for path in Path(PROJECT_ROOT_RELATIVE).rglob('*.cs'):
    with open(path, 'r') as file:
        new = [line.rstrip() for line in file]
    with open(path, 'w') as file:
        [file.write('%s\n' % line) for line in new]
