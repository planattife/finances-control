````md
---
name: create-pull-request
description: Create a pull request from the current branch. Generate the PR title from the branch name and create a concise PR description based on the work completed during the current Codex session.
---

# Create Pull Request

When asked to create, prepare, draft, or open a pull request, follow these instructions.

## Determine the Branch Name

Retrieve the current branch name:

```bash
git branch --show-current
````

## Generate the Pull Request Title

Convert the branch name into a human-readable title.

Rules:

* Remove common branch prefixes:

  * feature/
  * fix/
  * bugfix/
  * hotfix/
  * chore/
  * refactor/
  * docs/
  * test/

* Preserve the ticket/task number at the beginning.

* Replace hyphens with spaces.

* Insert " - " after the ticket/task number.

* Convert the remaining text to Title Case.

* Preserve common technical acronyms in uppercase:

  * API
  * DTO
  * HTTP
  * HTTPS
  * SQL
  * JWT
  * UI
  * UX
  * CRUD
  * URL
  * ID

Examples:

* feature/0001-creating-endpoints
  -> 0001 - Creating Endpoints

* feature/0015-mapping-dtos
  -> 0015 - Mapping DTOs

* fix/0023-correct-api-timeout
  -> 0023 - Correct API Timeout

## Analyze Changes

Before generating the pull request description:

1. Review the current git diff.
2. Review files modified during the current session.
3. Review commits created during the current session if available.
4. Identify:

   * Features implemented
   * Bugs fixed
   * Refactorings performed
   * Tests added or modified
   * Documentation updates

Never invent changes that are not present in the repository state.

## Generate the Pull Request Description

Use the following template:

```md
## Summary

- <change 1>
- <change 2>
- <change 3>

## Validation

- <test command executed>
- <additional validation performed>

```

If no validation was performed:

```md
## Validation

- Not run (not requested).
```

The description must:

* Be written in English.
* Be concise and professional.
* Focus on what was accomplished during the current Codex session.
* Avoid mentioning Codex, AI tools, or automated assistance.
* Avoid speculation.

## Create the Pull Request

Prefer GitHub CLI when available:

```bash
gh pr create \
  --title "<generated-title>" \
  --body "<generated-description>"
```

If GitHub CLI is unavailable or authentication is missing:

* Explain what is missing.
* Provide the exact command required to resolve the issue.
* Do not guess credentials or repository settings.

## Expected Output

Produce:

1. Generated PR title.
2. Generated PR description.
3. Pull request creation command (if needed).
4. Pull request URL after creation (if available).

```
```
