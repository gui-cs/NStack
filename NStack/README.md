# Build & Deploy

## Regenerating Tables when the Unicode standard is updated

To regenerate the Tables.cs file from the reference unicode files, run `Makefile`
with the command `make tables`, which will create the updated tables.

There is some work to be done to dump the tables as a binary blob,
without going through the various data structures that we have now, it would 
avoid all these constructors triggered on the static class.

## Version Numbers

Version info for NStack is managed by [gitversion](https://gitversion.net).

Install `gitversion`:

```powershell
dotnet tool install --global GitVersion.Tool
dotnet-gitversion
```

The project version (the nuget package and in `NStack.dll`) is determined from the latest `git tag`. 

The format of version numbers is `vmajor.minor.patch.build.height` and follows the [Semantic Versioning](https://semver.org/) rules.

To define a new version (e.g. with a higher `major`, `minor`, `patch`, or `build` value) tag a commit using `git tag`:

```powershell
git tag v1.3.4-beta.5 -a -m "Release v1.3.4 Beta 5"
dotnet-gitversion /updateprojectfiles
dotnet build -c Release
```

**DO NOT COMMIT AFTER USING `/updateprojectfiles`!**

Doing so will update the `.csproj` files in your branch with version info, which we do not want.

## Deploying a new version of the NStack Nuget Library

To release a new version (e.g. with a higher `major`, `minor`, or `patch` value) tag a commit using `git tag` and then 
push that tag directly to the `main` branch on `github.com/gui-cs/NStack` (`upstream`).

The `tag` must be of the form `v<major>.<minor>.<patch>`, e.g. `v2.3.4`.

`patch` can indicate pre-release or not (e.g. `pre`, `beta`, `rc`, etc...). 

### 1) Verify the `develop` branch is ready for release

* Ensure everything is committed and pushed to the `develop` branch
* Ensure your local `develop` branch is up-to-date with `upstream/develop`

### 2) Create a pull request for the release in the `develop` branch

The PR title should be of the form "Release v2.3.4"

```powershell
git checkout develop
git pull upstream develop
git checkout -b v_2_3_4
git merge develop
git add .
git commit -m "Release v2.3.4"
git push
```

Go to the link printed by `git push` and fill out the Pull Request.

### 3) On github.com, verify the build action worked on your fork, then merge the PR

### 4) Pull the merged `develop` from `upstream`

```powershell
git checkout develop
git pull upstream develop
```

### 5) Merge `develop` into `main`

```powershell
git checkout main
git pull upstream main
git merge develop
```

Fix any merge errors.

### 6) Create a new annotated tag for the release on `main`

```powershell
git tag v2.3.4 -a -m "Release v2.3.4"
```       

### 7) Push the new tag to `main` on `upstream`

```powershell
git push --atomic upstream main v2.3.4
```       

*See https://stackoverflow.com/a/3745250/297526*

### 8) Monitor Github Actions to ensure the Nuget publishing worked.

https://github.com/gui-cs/NStack/actions

### 9) Check Nuget to see the new package version (wait a few minutes) 
https://www.nuget.org/packages/NStack.Core

### 10) Add a new Release in Github: https://github.com/gui-cs/NStack/releases

Generate release notes with the list of PRs since the last release 

Use `gh` to get a list with just titles to make it easy to paste into release notes: 

```powershell
gh pr list --limit 500 --search "is:pr is:closed is:merged closed:>=2022-11-1"
```
### 11) Update the `develop` branch with the new version

```powershell
git checkout develop
git pull upstream develop
git merge main
git push upstream develop
```
