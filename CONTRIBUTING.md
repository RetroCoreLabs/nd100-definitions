# Contributing to nd100-definitions

Thank you for helping improve the ND-100 machine definitions.  
Because this repo is consumed as a git submodule by multiple tools, changes here have wide impact — please read these guidelines before opening a pull request.

---

## Where files go

| What | Where |
|---|---|
| Instruction definitions | `specs/instructions/<mnemonic>.yaml` |
| Instruction categories (name, source, text) | `specs/categories.yaml` |
| Internal register definitions | `specs/internalregisters/<name>.yaml` |
| Operand / addressing-mode types | `specs/operand_types/<name>.yaml` |
| Behavioural pseudocode (EMIL) | `specs/behaviour/` |
| YAML schemas | `specs/schema/` |
| CPU entry point | `specs/cpu.yaml` |

Do **not** add definition files outside `specs/`.  
Do **not** rename or move `specs/cpu.yaml` — it is the loader entry point for all consumers.

---

## Schema and validation

If a schema validator is present (check the repo root or CI configuration), run it before committing:

```sh
# example — adapt to whatever tooling is wired up
python tools/validate.py specs/cpu.yaml
```

If no validator exists yet, manually confirm that your YAML is well-formed (`yamllint` or any online checker).

---

## Breaking changes

A **breaking change** is anything that:

- Renames, moves, or removes a file that existing consumers reference.
- Removes or renames a field that schemas or tools depend on.
- Changes the semantics of an existing field in a non-backward-compatible way.

Breaking changes require a **major version bump** (see the versioning section in [README.md](README.md)) and should be clearly flagged in the PR description so downstream maintainers can plan their updates.

---

## Pull request expectations

1. **One logical change per PR.** Mixing unrelated instruction fixes and schema changes makes review harder.
2. **Describe what changed and why.** Reference the ND-100 hardware manual section if applicable.
3. **Validate before pushing.** Broken YAML stalls everyone who pulls.
4. **Note downstream impact.** If your change affects the schema or file layout, say so explicitly.

---

## Commit style

Use short, imperative subject lines:

```
Add SWAP instruction definition
Fix operand type for SHA immediate form
Correct interrupt level numbering in levels.yaml
```

---

## Questions?

Open a GitHub issue describing what you want to change and why. For large structural changes, discuss first before writing code.
