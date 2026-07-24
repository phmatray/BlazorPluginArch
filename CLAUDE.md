# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Workflow

> **🚨 CRITICAL — THIS IS A BLOCKING REQUIREMENT 🚨**
>
> **BEFORE writing, editing, or modifying ANY code file, you MUST complete steps 1-3 below.**
> There are ZERO exceptions to this rule. It applies to:
> - One-line fixes
> - "Trivial" changes
> - Bug fixes the user says are urgent
> - Any change to any `.cs`, `.json`, `.csproj`, or other source file
>
> **If you have already edited a file on `dev` without a worktree, STOP. Revert the change immediately with `git checkout -- <file>`, then start this workflow from step 1.**
>
> Never edit code directly on `dev`. Always use a worktree + feature branch, even for small fixes.

Every feature or bugfix follows this procedure **in order**:

1. **Sync dev** — Before starting any work, ensure `dev` is up to date:
   ```bash
   git checkout dev && git pull origin dev
   ```

2. **Create a worktree FIRST** — Use `git worktree add` (or the `EnterWorktree` tool) to isolate work from the main checkout. This keeps `dev` clean and allows parallel work. **You MUST create the worktree BEFORE writing any code. No exceptions.**

3. **Create a branch** — Branch from `dev` using conventional naming:
   - Features: `feat/<short-description>`
   - Bugfixes: `fix/<short-description>`

4. **Implement & commit** — Only NOW may you edit code. Make one or more focused commits using conventional commit messages (`feat:`, `fix:`, `chore:`, `docs:`). Each commit should be atomic and build successfully.

5. **Simplify & verify (MANDATORY)** — You MUST run `/simplify` to review changed code for reuse, quality, and efficiency. Then run the UI and use the browser MCP to test your implementation end-to-end. **Fix any problems found during testing before proceeding. Do NOT skip this step.**

6. **Create a PR (MANDATORY)** — You MUST push the branch and open a pull request targeting `dev` before considering the task complete. **The task is NOT done until the PR exists. Do NOT skip this step.**
   ```bash
   gh pr create --base dev --title "feat: short description" --body "..."
   ```
