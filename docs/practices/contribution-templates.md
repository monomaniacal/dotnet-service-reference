# Contribution templates

## What

An issue form with required "Why now" and "Verified by" fields and tier and theme dropdowns; a
pull request template that asks what breaks without the change, which test drove it, and which
practice page changed; a `CONTRIBUTING.md` describing the test-first loop.

## Why

The questions people skip are the ones worth asking every time. Making them fields, not
guidelines, means every item and every change states its reason and its proof.

## Source

- Issue forms: https://docs.github.com/communities/using-templates-to-encourage-useful-issues-and-pull-requests/syntax-for-issue-forms
- Pull request templates: https://docs.github.com/communities/using-templates-to-encourage-useful-issues-and-pull-requests/creating-a-pull-request-template-for-your-repository

## Enforced by

Required fields in the issue form. The PR template is a prompt; a reviewer refuses a PR that
leaves "which test drove it" blank.

## Opt out

Delete the files under `.github/`.

## Go deeper

A workflow that fails a PR whose body still contains the template placeholders.
