# OneMore Smoke Tests Notebook

This folder contains a notebook that specifies indvidual smoke test used to prove that
OneMore features are behaving as expected. These are executed as manual tests.

The notebook structure matches the structure folder beneath the OneMore/Commands folder,
where each section corresponds to a command category and each page corresponds to a
command in that category. The commands implemented in the Commands/Clean folder are
described by the Clean section. Each command in a Commands/category folder has a 
corresponding smoke test page under its similarly-named section.

You can view a read-only copy 
of [this notebook online here](https://1drv.ms/u/s!ArTUF0U30CVpg8lnWV_-ee6DvD9QyA?e=VHZirT)

## How to Test

Generally

1. Get a fresh copy of the notebook
1. Open the notebook in OneNote
1. Run the tests
1. Close the notebook

### Testing
Most releases are generally small so don't warrant executing the entire suite.
Instead, specific tests can be run that cover the expected blast-radius of the changes in the
release.

Walk through each selected page and execute the test protocol described. 

If an observed behavior does not match the expected outcome, open a new defect ticket.

### Teardown

When testing is completed, it's best to close the notebook and delete it or undo any
changes to the repo files (git revert, git reset) so that it will be back in its
ready-state for the test test execution.
