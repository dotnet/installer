# Source Build Smoke Tests

* Run these tests via `build.sh --run-smoke-test`
* Various configuration settings are stored in `Config.cs`

## Prereq Packages

Some prerelease scenarios, usually security updates, require non-source-built packages which are not publicly available.
Specify the directory where these packages can be found via the `SMOKE_TESTS_PREREQS_PATH` environment variable when running tests via `build.sh --run-smoke-test` e.g.
`SMOKE_TESTS_PREREQS_PATH=prereqs/packages/smoke-test-prereqs`.

### Downloading Prereq Packages

The packages required for running smoke tests can be downloaded by running the `prep.sh` script:

```bash
./prep.sh --no-artifacts --no-prebuilts \
    --smoke-test-prereqs-output prereqs/packages/smoke-test-prereqs
```

By default, these packages will be downloaded from nuget.org. To specify an additional package feed, such as would be necessary for prerelease scenarios, use the `--smoke-test-prereqs-feed` and `--smoke-test-prereqs-feed-key` options:

```bash
./prep.sh --no-artifacts --no-prebuilts \
    --smoke-test-prereqs-output prereqs/packages/smoke-test-prereqs \
    --smoke-test-prereqs-feed <nuget-feed> \
    --smoke-test-prereqs-feed-key <access-token>
```
