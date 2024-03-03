# Future Changelog

This file (CHANGELOG.future.md) is used to record changelog entries for future releases.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

### Purpose of this file

This file allows us to include changelog entries for changes targeting future versions in the PRs containing these changes, without causing PR validation failures.

Such PR validation failures occur because package validation does not allow inclusion of entries for future releases in CHANGELOG.md.


### How to use this file

When you are making changes in develop that are targeting a future release that is greater than the current package version number, record the changelog entries for these changes in this file.
1. You should follow the same format for changelog entries as CHANGELOG.md
2. If you know the specific version number of the release these changes will be included in, use that version number.
3. If you don't know the specific version number of the release these changes will be included in, just put them under "Next Release"

Then, when we have a new release branch and version number, we can move these changelog entries from this file into CHANGELOG.md under the next version number, and delete this file from the release branch as needed.


## Next Release

### *AREA*