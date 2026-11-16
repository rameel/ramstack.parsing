# Contributing

## Commit messages

Use scoped commit messages:

```text
<scope>: <concise description>
```

The scope identifies the part of Ramstack.Parsing that changed. Write the description in the imperative mood,
begin it with a lowercase letter, and describe the outcome rather than classifying the change.
It should complete the sentence: "This change modifies Ramstack.Parsing to _____."

```text
characters: correct ARM64 SIMD range comparisons
literals: add quoted character parsing
parsers: support left and right associative folds
diagnostics: unify line and column calculation
internals: reduce unsafe code in StringBuffer
```

Do not prefix messages with conventional-commit types such as `feat`, `fix`, `refactor`, or `chore`.
The description should make that context clear when it matters.

### Scopes

Use the smallest stable area that describes the outcome. Start with these root scopes;
add a subscope only when the history of a root scope becomes difficult to navigate:

| Scope         | Area                                                                                              |
|---------------|---------------------------------------------------------------------------------------------------|
| `parsers`     | Base parser API, parsing context and results, primitive parsers, and combinators                  |
| `characters`  | Character parsers, character classes, Unicode categories, and character search optimizations      |
| `literals`    | Number, quoted string, quoted character, and escape sequence parsing in `Literal.*`               |
| `diagnostics` | Shared error collection, suppression, formatting, and error position calculation                  |
| `internals`   | Shared implementation infrastructure, including `ArrayBuilder`, `StringBuffer`, and compatibility |
| `samples`     | JSON, calculator, and TinyC examples                                                              |
| `bench`       | Measurement scenarios, library comparisons, and benchmark results                                 |
| `docs`        | General project documentation                                                                     |
| `tests`       | Shared test infrastructure                                                                        |
| `build`       | Build, CI, dependencies, development tooling, and NuGet packaging                                 |
| `repo`        | Repository-wide metadata and contribution rules                                                   |

Use the product area as the scope for its tests, documentation, and sample code, even when a commit changes only
those supporting artifacts. For example, use `characters: test Unicode category matching`,
`literals: clarify number parsing options`, and `internals: test StringBuffer`.
Reserve `tests`, `docs`, and `samples` for their own infrastructure or for coherent changes
that span multiple product areas. Use `samples` for changes to an example's grammar or behavior.

Use `parsers` for combinators such as `Seq`, `Choice`, `Repeat`, and `FoldL`, and for primitive parsers
such as string matching and end-of-input checks. Use `characters` for `Character`, `Set`, `CharClass*`,
Unicode categories, and their search implementations, including SIMD optimizations.
Use `diagnostics` for shared error reporting behavior, even when its implementation spans parser and context files.
Keep an error reporting correction specific to one parser under that parser's product scope.

Use `internals` only for shared implementation work with no single product owner, such as common collection,
buffer, debugging, or compatibility machinery. Prefer the owning product scope when an internal change
serves one product area. Use `bench` for measurement scenarios, comparisons, and reported results;
use the owning product scope for performance improvements to the library itself.

Split unrelated changes into separate commits. If one atomic change spans multiple areas,
choose its primary product area; do not use a type prefix or a ticket number as the scope.
Put ticket references and implementation details in the message body when needed.

Use the same scoped style for pull request titles. For a pull request spanning multiple scopes,
use the scope of its primary outcome. The title does not need to summarize every supporting commit.

### Recent examples

These recent non-merge commits would read as follows under this convention:

| Commit    | Scoped message                                                             |
|-----------|----------------------------------------------------------------------------|
| `d397c48` | `build: migrate the solution to slnx`                                      |
| `ae9ee6e` | `build: restrict publishing to the main repository`                        |
| `5fa31a3` | `characters: correct ARM64 SIMD range comparisons`                         |
| `58ba846` | `characters: remove ambiguous Unicode rendering from test output`          |
| `1fc12e4` | `characters: include hexadecimal character codes in test failure messages` |
| `0914e55` | `build: exercise SIMD optimizations in CI`                                 |
| `ee626d6` | `diagnostics: unify line and column calculation`                           |
| `74338a3` | `parsers: correct a comment in Repeat`                                     |
| `c707d8e` | `internals: conditionally compile StringBuffer tests`                      |
| `013e94c` | `internals: test StringBuffer`                                             |

## Issue titles

Use a clear, descriptive issue title. Scope prefixes are optional; when used, choose from the same scopes
as commits and pull requests. Describe the problem or proposed capability; the imperative mood is not required.

For example, both titles are valid:

```text
parsers: Repeat hangs when the inner parser consumes no input
Repeat hangs when the inner parser consumes no input
```

## Test names

Prefer `Method_Condition_ExpectedResult` when adding or renaming tests. This is a recommendation,
not a strict requirement: choose a clearer name when the pattern does not fit naturally.

The first part identifies the method, parser, or combinator under test. The second describes the input
or condition, and the third describes the expected observable behavior. Prefer PascalCase within each
part, with underscores separating the parts.

```text
FoldL_NoInputConsumed_StopsWithoutReducing
FoldL_TrailingEmptyPair_SkipsReduction
Separated_MinimumNotReached_RestoresPosition
Separated_InvalidBounds_ThrowsArgumentOutOfRangeException
```

Prefer natural English, such as `NoInputConsumed` rather than `NoConsumed`. Keep names concise,
but descriptive enough to understand the scenario from test output. A specific result such as
`RestoresPosition` is more useful than a generic `Works` or `Succeeds` when that is what the test checks.

Focus on the main behavior; the name does not need to enumerate every assertion or implementation detail.
For parameterized tests, describe the common scenario and let the arguments distinguish individual cases.
There is no need to rename existing tests solely to follow this convention.
