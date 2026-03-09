---
name: branch-and-pr
description: Ensures that code changes for features and bug fixes are always made on a new branch with a pull request. Use this when making any non-documentation changes such as features, bug fixes, refactors, or configuration changes.
---

# Branch and PR Workflow

When making changes to code, configuration, tests, or any non-documentation files (i.e. features, bug fixes, refactors), always follow this workflow:

## Steps

1. **Create a new branch** from the current branch before making any changes:
   - Use a descriptive branch name following the pattern: `feature/<short-description>` for features, `fix/<short-description>` for bug fixes, or `refactor/<short-description>` for refactors.
   - Run `git checkout -b <branch-name>` to create and switch to the new branch.

2. **Make the changes** on the new branch.

3. **Commit the changes** with a clear, descriptive commit message.

4. **Push the branch** to the remote repository using `git push -u origin <branch-name>`.

5. **Create a pull request** using the GitHub MCP server's tools targeting the base branch you originally branched from.

## When to Apply

- **DO** apply this workflow for: features, bug fixes, refactors, test changes, configuration changes, build script changes, dependency updates, and any other code modifications.
- **DO NOT** apply this workflow for: documentation-only changes (e.g. README updates, release notes, markdown files) or if an appropriate branch is prexists. Documentation changes can be committed directly to the current branch.

## Guidelines

- Never commit feature or bug fix changes directly to `main` or the default branch.
- Each branch should focus on a single logical change.
- The PR title and description should clearly explain what changed and why.
