# Github Workflows
These workflows help automate bits of my development day-to-day.

## build.yml
Builds the tip of `main` and creates a new beta release.
These beta release builds are for users who are willing to grab a preview
of the next release. _Thank you, by the way!_

## cleanup.yml
This is weekly cron job (Sunday night) to keep the run log low.

#### Manual Cleanup
Workflow runs can be cleaned up manually by running a command similar to the following.
It will only grab the top 500, so run it until there are none left.

```powershell
gh run list --repo stevencohn/onemore 
	--workflow "pages-build-deployment"  # replace with workflow name
	--limit 500 --json databaseId -q '.[].databaseId' | foreach { 
		gh run delete --repo stevencohn/onemore $_
	}
```

## pages.yml
This updates the onemoreaddin.com web site.
It is auto-triggered by the Pages web site whenever there is changes in the docs/** folder.

## pr-issue-comment-on-merge.yml
When a PR is merged, this will add a coment to the associated ticket
to ensure they are linked. (Github has not yet released an API or `gh` command
to update the "Development" links on ticket.)

## pr-issue-link.yml
When a PR is opened, grabs the ticket number from the PR title and ensures
the ticket is referenced in the PR description. PR titles should be prefixed
with the ticket number, such as "1234-do-something-cool".
