---
name: gitlab-basics
description: "Use when doing quick GitLab auth and everyday git operations: login, clone/pull, remote setup, push, branch, merge request, and common permission fixes."
version: 1.0.4
author: mag1art
license: Apache License 2.0
triggers:
  - gitlab
  - gitlab.com
  - self-hosted GitLab
  - glab
  - PAT
  - clone gitlab
  - gitlab remote
  - git remote
  - git pull
  - git push
  - branch
  - merge request
  - MR
  - protected branch
metadata:
  hermes:
    tags: [gitlab, git, glab, auth, pat, ssh, clone, remote, branch, merge-request, ci-cd, repository-management]
    related_skills: [gitlab-glab-workflow]
---

# GitLab Basics

## Overview

Compact GitLab workflow for day-to-day repository work. GitLab may be hosted at `gitlab.com` or at a company/self-hosted address such as `gitlab.company.com`, `git.example.org`, or `https://gitlab.internal`. Treat the host as a variable: `<gitlab-host>`.

Prefer `glab` for GitLab API/MR actions and normal `git` for local history, pull, push, branches, and remotes.

## When to Use

- User asks how to authenticate with GitLab.
- Need to clone, pull, push, set remotes, or open a merge request.
- Need a quick checklist before changing a GitLab repository.
- For protected branches, initial push issues, or version-specific `glab` flags, also load `gitlab-glab-workflow`.

## Auth

Check tools and current auth:

```bash
command -v git
git --version
command -v glab || true
glab --version 2>/dev/null || true
glab auth status
```

Login interactively:

```bash
# Public GitLab:
glab auth login --hostname gitlab.com

# Company/self-hosted GitLab:
glab auth login --hostname <gitlab-host>
```

Typical choices:

- Host: `gitlab.com` or the company/self-hosted GitLab domain (`<gitlab-host>`), without the repository path.
- Protocol for Git operations: choose `HTTPS` unless SSH keys are already configured.
- If using a Personal Access Token (PAT), paste it only into the secure `glab auth login` prompt, never into chat.

### Personal Access Token (PAT)

Use a PAT when password login is disabled, SSO/2FA is enabled, or automation needs non-interactive auth.

Create PAT in GitLab UI:

- Public GitLab: `https://gitlab.com/-/user_settings/personal_access_tokens`
- Self-hosted/company GitLab: `https://<gitlab-host>/-/user_settings/personal_access_tokens`

Common scopes:

- `read_repository`: clone/fetch/pull private repos.
- `write_repository`: push over HTTPS.
- `api`: `glab` API actions such as repo view, merge requests, pipelines, protected branches.

Recommended default for normal `glab` + git work: `api`, `read_repository`, `write_repository` with the shortest practical expiration.

Non-interactive login, if the PAT is already in an environment variable:

```bash
printf '%s' "$GITLAB_TOKEN" | glab auth login --hostname <gitlab-host> --stdin
```

Security rules:

- Do not print, echo, commit, or paste PAT values into chat/logs.
- Prefer environment variables or password manager copy/paste into prompts.
- Rotate/revoke the PAT immediately if exposed.
- Use `glab auth status` for masked verification.

## Clone / Pull

Clone HTTPS:

```bash
# Public GitLab:
git clone https://gitlab.com/<namespace>/<repo>.git

# Company/self-hosted GitLab:
git clone https://<gitlab-host>/<namespace>/<repo>.git

cd <repo>
```

Clone SSH, only if SSH key is configured in GitLab:

```bash
# Public GitLab:
git clone git@gitlab.com:<namespace>/<repo>.git

# Company/self-hosted GitLab:
git clone git@<gitlab-host>:<namespace>/<repo>.git

cd <repo>
```

Pull latest changes:

```bash
git status --short
git branch --show-current
git pull --ff-only
```

If `--ff-only` fails, inspect before merging/rebasing:

```bash
git status
git log --oneline --graph --decorate --all -20
```

## Remotes

Inspect remotes:

```bash
git remote -v
```

Add origin:

```bash
# Public GitLab:
git remote add origin https://gitlab.com/<namespace>/<repo>.git

# Company/self-hosted GitLab:
git remote add origin https://<gitlab-host>/<namespace>/<repo>.git
```

Change origin:

```bash
# Public GitLab:
git remote set-url origin https://gitlab.com/<namespace>/<repo>.git

# Company/self-hosted GitLab:
git remote set-url origin https://<gitlab-host>/<namespace>/<repo>.git
```

Verify project visibility:

```bash
glab repo view <namespace>/<repo> --repo <gitlab-host>/<namespace>/<repo> --web=false
```

If `glab repo view` behaves differently on the installed version, run it from inside the cloned repository after setting the correct remote.

## Commit / Push

Create or switch branch:

```bash
git checkout -b feat/<short-name>
# or: git switch -c feat/<short-name>
```

Commit:

```bash
git status --short
git add <files>
git commit -m "feat: <short summary>"
```

Push current branch:

```bash
git push -u origin HEAD
```

For initial repo setup:

```bash
git branch -M main
git push -u origin main
```

## Merge Request

Create MR with `glab`:

```bash
glab mr create \
  --source-branch <branch> \
  --target-branch main \
  --title "feat: <short summary>" \
  --description "## Summary
- ...

## Test Plan
- ..." \
  --yes
```

List MRs:

```bash
glab mr list
```

View current MR:

```bash
glab mr view
```

## Common Fixes

### SSH permission denied

If push fails with `Permission denied (publickey)`, either configure an SSH key in GitLab or switch remote to HTTPS:

```bash
# Public GitLab:
git remote set-url origin https://gitlab.com/<namespace>/<repo>.git

# Company/self-hosted GitLab:
git remote set-url origin https://<gitlab-host>/<namespace>/<repo>.git

git push -u origin HEAD
```

### Authentication failed over HTTPS

Refresh login:

```bash
glab auth status

# Public GitLab:
glab auth login --hostname gitlab.com

# Company/self-hosted GitLab:
glab auth login --hostname <gitlab-host>
```

If prompted for a password over HTTPS, use a PAT instead of the GitLab account password when 2FA/SSO is enabled.

```bash
git pull --ff-only
# or
git push -u origin HEAD
```

### Protected branch rejects push

Do not force-push. Create a feature branch and MR:

```bash
git checkout -b feat/<short-name>
git push -u origin HEAD
glab mr create --target-branch main --fill --yes
```

If this is the initial push to an empty project, ask an Owner/Maintainer to adjust branch protection or perform the first push.

## Verification Checklist

- [ ] `glab auth status` succeeds for the intended GitLab host (`gitlab.com` or `<gitlab-host>`).
- [ ] PAT, if used, has only needed scopes and was not printed/logged.
- [ ] `git remote -v` points to the intended project, host, and protocol.
- [ ] `git status --short` has only expected changes.
- [ ] `git pull --ff-only` succeeds before pushing to a shared branch.
- [ ] `git push -u origin HEAD` succeeds and sets upstream.
- [ ] `glab repo view ...` can see the project on the intended host; for self-hosted GitLab, verify from inside the repo or pass the host-aware repo selector.
- [ ] MR exists if work should be reviewed before merge.

## Pitfalls

- `glab` login does not make SSH work unless SSH keys are configured.
- GitLab is not always `gitlab.com`; check the actual company/self-hosted host and use it consistently in `glab auth login --hostname`, clone URLs, remotes, and PAT UI URLs.
- Prefer HTTPS if unsure; it usually matches `glab auth login` credentials.
- Never paste tokens into commands that will be saved in shell history.
- A PAT is equivalent to a password for its scopes; use short expiration, least scopes, and rotate it if exposed.
- Avoid `git push --force`; use `--force-with-lease` only when explicitly required and understood.
- Do not push directly to protected `main`; use a branch and MR.
