---
name: branch-and-pr
description: Ensures all code changes are made on a new branch with a pull request. Use this skill whenever making any code modification, bug fix, feature addition, or refactor.
---

# Branch and Pull Request Workflow

Before making **any** code changes, you **must** follow this workflow. Never commit directly to `master` or `main`.

## Steps

### 1. Check the current branch

```bash
git branch --show-current
```

If on `master` or `main`, create a new branch before making changes.

### 2. Create a new branch

Use a descriptive name with a conventional prefix:

- `fix/<short-description>` — bug fixes
- `feature/<short-description>` — new features
- `refactor/<short-description>` — refactoring
- `docs/<short-description>` — documentation changes
- `chore/<short-description>` — maintenance tasks

```bash
git checkout -b <branch-name>
```

### 3. Make the code changes

Implement the requested changes on the new branch.

### 4. Commit the changes

```bash
git add -A
git commit -m "<type>: <description>"
```

### 5. Push the branch

```bash
git push -u origin <branch-name>
```

### 6. Create a pull request

```bash
gh pr create --title "<title>" --body "<description of changes>" --base master
```

Include in the PR body:
- A summary of what changed and why
- Any testing performed
- Related issue numbers if applicable

## Rules

- **NEVER** commit directly to `master` or `main`.
- **ALWAYS** create a new branch for every change, no matter how small.
- **ALWAYS** open a pull request after pushing.
- If already on a non-default feature branch related to the current task, you may continue on it.
- Keep commits atomic and focused.
