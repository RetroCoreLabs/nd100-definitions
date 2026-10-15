# nd100-definitions

Machine-readable CPU metadata and instruction definitions for the **Norsk Data ND-100 / ND-110** minicomputer, expressed in YAML (and a few JSON/Markdown files for supplementary data).

This repository is the single source of truth for ND-100 instruction encoding, operand types, internal registers, I/O device tables, interrupt levels, and behavioural pseudocode. Downstream tools — assemblers, disassemblers, code generators, IDE extensions, emulators — consume it as a **git submodule** so they always pin an exact, reproducible snapshot.

---

## Directory layout

```
specs/                        ← everything lives here; use this as the root
  cpu.yaml                    ← entry point: references all sub-trees below
  instructions/               ← one YAML file per mnemonic (e.g. add.yaml)
  category/                   ← Markdown descriptions of instruction groups
  internalregisters/          ← CPU internal register definitions
  operand_types/              ← addressing-mode and operand-type definitions
  behaviour/                  ← behavioural pseudocode (EMIL) and libraries
  schema/                     ← YAML schemas for validation
  levels.yaml                 ← interrupt priority levels
  io-devices.json             ← I/O device table
  mon-calls.json              ← monitor (syscall) table
```

**Start with `specs/cpu.yaml`.**  
All paths inside that file (and inside instruction files) are relative to `specs/`, so loaders should set their working root to `specs/` and resolve everything from there.

---

## Using this repo as a git submodule

### Adding the submodule

```sh
git submodule add https://github.com/RetroCoreLabs/nd100-definitions.git path/to/nd100-definitions
git submodule update --init --recursive
```

Replace `path/to/nd100-definitions` with wherever your tool expects the definitions (e.g. `vendor/nd100-definitions` or `data/nd100`).

### Checking out an existing repo that already has the submodule

```sh
git clone --recurse-submodules <parent-repo-url>
# or, if you already cloned without --recurse-submodules:
git submodule update --init --recursive
```

### Pinning a release for reproducible builds

Each stable release is tagged (e.g. `v1.0.0`). Parent repos should **pin the submodule to a tag**, not track `main`, so that tool builds are reproducible even when the definitions evolve.

```sh
cd path/to/nd100-definitions
git fetch --tags
git checkout v1.0.0
cd ..
git add path/to/nd100-definitions
git commit -m "Pin nd100-definitions to v1.0.0"
```

Record the tag (or full commit SHA) in your parent repo's documentation so other contributors know which definition version is in use.

### Updating to a newer release

```sh
cd path/to/nd100-definitions
git fetch --tags
git checkout v2.0.0   # or a specific commit
cd ..
git add path/to/nd100-definitions
git commit -m "Bump nd100-definitions to v2.0.0"
```

---

## Versioning

Releases are tagged as `vMAJOR.MINOR.PATCH` (semantic versioning):

- **MAJOR** — breaking changes to schema or file layout (affects all consumers).
- **MINOR** — new instructions, registers, or fields added in a backward-compatible way.
- **PATCH** — corrections, typo fixes, metadata-only changes.

Downstream tools should document which tag or commit SHA they depend on.

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines. Changes here affect every tool that uses this repo as a submodule, so please read it before opening a pull request.

---

## License

MIT — see [LICENSE](LICENSE).
