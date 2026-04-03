# Internal Registers

This directory contains YAML definitions for the ND-100 internal registers.

## Structure

Each YAML file defines one internal register with its properties:

- `name`: Register name
- `description`: Detailed description of the register
- `size`: Size in bits
- `type`: Register type (user, system, special, etc.)
- `bits`: Bit fields within the register
  - `name`: Bit field name
  - `position`: Starting bit position
  - `size`: Size of the bit field in bits
  - `description`: Description of what this bit field represents

## Example

```yaml
register:
  name: STS
  description: Status register
  size: 16
  type: system
  bits:
    - name: C
      position: 0
      size: 1
      description: Carry flag
    - name: O
      position: 1
      size: 1
      description: Overflow flag
    - name: Q
      position: 2
      size: 1
      description: Dynamic overflow flag
```

## Known Registers

From TRR/TRA instructions, we know about these registers:
- STS: Status register
- DD: D register
- DP: P register
- DB: B register
- DL: L register
- DA: A register
- DT: T register
- DX: X register 