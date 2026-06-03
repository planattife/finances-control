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
* Do not guess credentials or repository settings.

Execute the command only once.

If the command succeeds:

* Return the pull request URL if available.

If the command fails:

* Follow the Failure Handling section.
* Do not retry.

## Failure Handling

Attempt the pull request creation process only once.

Do not retry any command automatically.

Do not attempt alternative commands, authentication flows, branch pushes, repository configuration changes, or recovery actions.

If any step fails, including but not limited to:

- GitHub CLI errors
- Authentication failures
- Permission errors
- Missing remote branches
- Network issues
- Repository configuration issues
- Pull request creation failures

Immediately stop the process and return:

1. The generated pull request title.
2. The generated pull request description.
3. A brief explanation of the failure.

Do not make additional attempts to create the pull request.
Do not ask for confirmation before stopping.
Do not retry under any circumstances.

## Expected Output

Produce:

1. Generated PR title.
2. Generated PR description.
3. Pull request creation command (if needed).
4. Pull request URL after creation (if available).
