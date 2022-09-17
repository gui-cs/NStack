# Contributing to NStack

We welcome contributions from the community. See [Issues](https://github.com/gui-cs/NStack/issues) for a list of open [bugs](https://github.com/gui-cs/NStack/issues?q=is%3Aopen+is%3Aissue+label%3Abug) and [enhancements](https://github.com/gui-cs/NStack/issues?q=is%3Aopen+is%3Aissue+label%3Aenhancement). Contributors looking for something fun to work on should look at issues tagged as:

- [good first issue](https://github.com/gui-cs/NStack/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22)
- [up for grabs](https://github.com/gui-cs/NStack/issues?q=is%3Aopen+is%3Aissue+label%3Aup-for-grabs)
- [help wanted](https://github.com/gui-cs/NStack/issues?q=is%3Aopen+is%3Aissue+label%3Aup-for-grabs)

## Forking and Submitting Changes

NStack uses the [GitFlow](https://nvie.com/posts/a-successful-git-branching-model/) branching model. 

* The `main` branch is always stable, and always matches the most recently released Nuget package.
* The `develop` branch is where new development and bug-fixes happen. It is the default branch.

### Forking NStack

1. Use GitHub to fork the `NStack` repo to your account (https://github.com/gui-cs/NStack/fork).

2. Clone your fork to your local machine

```
git clone https://github.com/<yourID>/NStack
```

Now, your local repo will have an `origin` remote pointing to `https://github.com/<yourID>/NStack`.

3. Add a remote for `upstream`: 
```
git remote add upstream https://github.com/gui-cs/NStack
```
You now have your own fork and a local repo that references it as `origin`. Your local repo also now references the orignal NStack repo as `upstream`. 

### Starting to Make a Change

Ensure your local `develop` branch is up-to-date with `upstream` (`github.com/gui-cs/NStack`):
```powershell
cd ./NStack
git checkout develop
git pull upstream develop
```

Create a new local branch:
```powershell
git checkout -b my_new_branch
```

#### Making Changes
Follow all the guidelines below.

* Coding Style
* Unit Tests
* Sample Code
* API Documentation
* etc...

When you're ready, commit your changes:

```powershell
git add .
git commit -m "Fixes #1234. Some bug"
```

### Submitting a Pull Request

1. Push your local branch to your fork (`origin`):

```powershell
git push --set-upstream origin my_new_branch
```

2. Create the Pull Request:

In the output of the `git push` command you'll see instructions with a link to the Pull Request:

```powershell
 $ git push --set-upstream origin my_new_branch
Enumerating objects: 8, done.
...
remote:
remote: Create a pull request for 'my_new_branch' on GitHub by visiting:
remote:      https://github.com/<yourID>/NStack/pull/new/more_doc_fixes
remote:
...
```

3. Go to that URL and create the Pull Request:

(in Windows Terminal, just CTRL-Click on the URL)

Follow the template instructions found on Github.

## NStack Coding Style

**NStack** follows the [Mono Coding Guidelines](https://www.mono-project.com/community/contributing/coding-guidelines/). [`/.editorconfig`](https://github.com/gui-cs/NStack/blob/b0a43ba338adf5ec069066e5a7dff8fea39b41db/.editorconfig) enforces this style in Visual Studio. Use `Ctrl-K-D` in Visual Studio to have it reformat code.

## User Experience Tenets

**NStack**, as a UI framework, heavily influences how console graphical user interfaces (GUIs) work. We use the following [tenets](https://ceklog.kindel.com/2020/02/10/tenets/) to guide us:

*NOTE: Like all tenets, these are up for debate. If you disagree, have questions, or suggestions about these tenets and guidelines submit an Issue using the [design](https://github.com/gui-cs/NStack/issues?q=is%3Aopen+is%3Aissue+label%3Adesign) tag.*

1. **Honor What's Come Before**. The Mac and Windows OS's have well-established GUI idioms that are mostly consistent. We adhere to these versus inventing new ways for users to do things. For example, **NStack** adopts the `ctrl/command-c`, `ctrl/command-v`, and `ctrl/command-x` keyboard shortcuts for cut, copy, and paste versus defining new shortcuts.
2. **Consistency Matters**. Common UI idioms should be consistent across the GUI framework. For example, `ctrl/command-q` quits/exits all modal views. See [Issue #456](https://github.com/gui-cs/NStack/issues/456) as a counter-example that should be fixed.
3. **Honor the OS, but Work Everywhere**. **NStack** is cross-platform, but we support taking advantage of a platform's unique advantages. For example, the Windows Console API is richer than the Unix API in terms of keyboard handling. Thus, in Windows pressing the `alt` key in a **NStack** app will activate the `MenuBar`, but in Unix, the user has to press the full hotkey (e.g. `alt-f`) or `F9`. 
4. **Keyboard first, Mouse also**. Users use consoles primarily with the keyboard; **NStack** is optimized for getting stuff done without using the Mouse. However, as a GUI framework, the Mouse is essential thus we strive to ensure that everything also works via the Mouse.

## Public API Tenets & Guidelines

**NStack** provides an API that is used by many. As the project evolves, contributors should follow these [tenets](https://ceklog.kindel.com/2020/02/10/tenets/) to ensure Consistency and backward compatibility.

*NOTE: Like all tenets, these are up for debate. If you disagree, have questions, or suggestions about these tenets and guidelines submit an Issue using the [design](https://github.com/gui-cs/NStack/issues?q=is%3Aopen+is%3Aissue+label%3Adesign) tag.*

1. **Stand on the shoulders of giants.** Follow the [Microsoft .NET Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/) where appropriate. 
2. **Don't Break Existing Stuff.** Avoid breaking changes to user behavior or the public API; instead, figure out how to implement new functionality in a similar way. If a breaking change can't be avoided, follow the guidelines below.
3. **Fail-fast.** Fail-fast makes bugs and failures appear sooner, leading to a higher-quality framework and API.
4. **Standards Reduce Complexity**. We strive to adopt standard API idoms because doing so reduces complexity for users of the API. For example, see Tenet #1 above. A counterexample is [Issue #447](https://github.com/gui-cs/NStack/issues/447).

### Include API Documentation

Great care has been provided thus far in ensuring **NStack** has great [API Documentation](https://gui-cs.github.io/NStack/api/NStack/NStack.html). Contributors have the responsibility of continuously improving the API Documentation.

- All public APIs must have clear, concise, and complete documentation in the form of [XML Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/).
- Keep the `<summary></summary>` terse.
- Use `<see cref=""/>` liberally to cross-link topics.
- Use `<remarks></remarks>` to add more context and explanation.
- For complex topics, provide conceptual documentation in the `docfx/articles` folder as a `.md` file. It will automatically get picked up and be added to [Conceptual Documentation](https://gui-cs.github.io/NStack/articles/index.html).
- Use proper English and good grammar.

## Breaking Changes to User Behavior or the Public API

- Tag all pull requests that cause breaking changes to user behavior or the public API with the [breaking-change](https://github.com/gui-cs/NStack/issues?q=is%3Aopen+is%3Aissue+label%3Abreaking-change) tag. This will help project maintainers track and document these.
- Add a `<remark></remark>` to the XML Documentation to the code describing the breaking change. These will get picked up in the [API Documentation](https://gui-cs.github.io/NStack/api/NStack/NStack.html).

## Unit Tests

PRs should never cause code coverage to go down. Ideally, every PR will get the project closer to 100%. PRs that include new functionality (e.g. a new control) should have at least 70% code coverage for the new functionality. 

**NStack** has an automated unit or regression test suite. See the [Testing wiki](https://github.com/gui-cs/NStack/wiki/Testing).

We analyze unit tests and code coverage on each PR push. 

The code coverage of the latest released build (on NuGet) is shown as a badge at the top of `README.md`. Here as well:

![Code Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/gui-cs/90ef67a684cb71db1817921a970f8d27/raw/code-coverage.json)

The project uses Fine Code Coverage to allow easy access to code coverage info on a per-component basis.

Use the following command to generate the same CC info that the Publish Github Action uses to publish the results to the badge:

```
dotnet test --no-restore --verbosity normal --collect:"XPlat Code Coverage"  --settings UnitTests/coverlet.runsettings
```

Then open up the resulting `coverage.opencover.xml` file and you'll see the `sequenceCoverage` value:

```xml
<?xml version="1.0" encoding="utf-8"?>
<CoverageSession>
  <Summary numSequencePoints="15817" visitedSequencePoints="7249" numBranchPoints="9379" visitedBranchPoints="3640" sequenceCoverage="45.83" branchCoverage="38.81" maxCyclomaticComplexity="10276" minCyclomaticComplexity="10276" visitedClasses="105" numClasses="141" visitedMethods="965" numMethods="1751" />
 
```
