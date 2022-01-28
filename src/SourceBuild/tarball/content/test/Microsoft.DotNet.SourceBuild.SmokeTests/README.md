# Source Build Smoke Tests

* Run these tests via `build.sh --run-smoke-test`
* Various configuration settings are stored in `Config.cs`

## Prereq Packages
Some prerelease scenarios, usually security updates, require non-source-built packages are not publicly available.
Place any smoke-test NuGet packages required to run the tests in the tarball's `packages/smoke-test-prereqs`.
When prereq packages are required, the `EXCLUDE_ONLINE_TESTS=true` environment variable should be set when running smoke-tests via `build.sh --run-smoke-test`
