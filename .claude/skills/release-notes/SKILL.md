# Release Notes Generator

Generates a `## What's New` section for the next OneMore release by fetching all open GitHub
issues labeled `next-release`, cross-referencing them with git commits since the last release
tag, and formatting them in the same style as the previous release.

## Steps

1. Find the most recent release tag, skipping unpublished release tags:
   `git tag --sort=-version:refname | Where-Object { $_ -match '^v?\d+\.\d+\.\d+$' } | Select-Object -First 1`

2. Fetch all open issues with the `next-release` label:
   `gh issue list --label next-release --state open --limit 100 --repo stevencohn/OneMore --json number,title,labels`

3. Fetch commits since that tag:
   `git log <tag>..HEAD --oneline`

4. For each issue, determine what was implemented by matching issue numbers in commit messages.
   Use the issue title and labels as additional signal. Fetch individual issue details 
   with `gh issue view <number> --repo stevencohn/OneMore` for any that need more context.

5. Categorize each issue:
   - **Added** — new commands, features, or capabilities (usually `feature-request` issues)
   - **Updated** — enhancements to existing features (usually `enhancement` issues, or feature
     requests that extend existing behavior)
   - **Fixed** — bug fixes (usually `bug` or `telemetry` issues)

6. Write one-liner summaries following this exact format (no trailing period; issue link preceded by a comma):
   - Added:   `* Added [noun phrase] [description], [#NNNN](https://github.com/stevencohn/OneMore/issues/NNNN)`
   - Updated: `* Updated [noun phrase] [description], [#NNNN](https://github.com/stevencohn/OneMore/issues/NNNN)`
   - Fixed:   `* Fixed an issue where [description], [#NNNN](https://github.com/stevencohn/OneMore/issues/NNNN)`

   IMPORTANT: The issue reference MUST be a Markdown hyperlink in the form `[#NNNN](URL)` —
   the `#NNNN` text in square brackets followed immediately by the URL in parentheses, with no
   space between them. Never write a bare `#NNNN` or a raw URL on a separate line.

7. Output all Added items first, then Updated, then Fixed — as a flat bullet list
   under `## What's New` with no sub-headers between categories. Wrap the entire
   output in a fenced code block (``` ``` ```) so the raw markdown is visible and
   can be copied without the chat interface rendering the links.

## Style Reference (from v7.0.0 release notes)

- `* Added Verilog and Scala language support to the colorizer, [#1018](...)`
- `* Updated Hashtag service performance and stability during background scans, [#2054](...)`
- `* Fixed an issue where Text to Table would fail when text contains non-breaking spaces, [#1999](...)`

Tone: concise and semi-technical. Each item should be 10–25 words. Do not explain implementation 
details — describe the user-visible behavior or capability.
