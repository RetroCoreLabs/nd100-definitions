### Description

These instructions use decimal operands residing in main memory only.


**Instruction structure:**

    D1 (word address)
    ┌───────────────────────────────┐
    │15           ...            0  │
    │ word address                  │
    └───────────────────────────────┘

    D2 (operand features)
    ┌────┬────┬───────┬───────┬──────────────┬──────────────┐
    │15  │14  │13-11  │10     │  9  -    5   │  4  -    0   │
    ├────┼────┼───────┼───────┼──────────────┼──────────────┤
    │lr  │ •  │ASCII  │r      │decimal point | field length │    
    └────┴────┴───────┴───────┴──────────────┴──────────────┘

**Descriptors (D1 and D2):**
- Two 16-bit words (D1 and D2) specify the operands used in decimal instructions.
- D1: The first descriptor gives the word address of the decimal operand in memory.
- D2: Describes the following operand features:
  - **lr (bit 15):**
    - lr = 0: The operand starts in the left byte of a memory word (least significant 8 bits).
    - lr = 1: The operand starts in the right byte of a memory word (most significant 8 bits).
  - **ASCII (bits 11-13):**
    - These three bits give the sign representation used for ASCII format:

        ┌─────┬───────────────────────┐
        │bits │ sign representation   │
        ├─────┼───────────────────────┤
        │000  │ embedded trailing     │
        │001  │ separate trailing     │
        │010  │ embedded leading      │
        │011  │ separate leading      │
        │100  │ unsigned              │
        └─────┴───────────────────────┘

    - Bit 13 also represents an unsigned number in BCD representation.
  - **r (bit 10):**
    - Rounding bit. If rounding is selected, one is added to the shifted operand when the least significant digit is lost during shift and the last digit shifted out of the field is 25.
    - r = 1: rounding on
    - r = 0: rounding off
  - **decimal point (bits 5-9):**
    - These bits give the position of the decimal point (0 to 31, positive or negative). Zero is the decimal place to the right of the least significant digit. The number must be less than the operand field length.
  - **field length (bits 0-4):**
    - These bits give the operand field length in nibbles (4-bit values) or bytes (8-bit values). BCD numbers are represented by 4 bits (1 nibble), so the field length will be in nibbles; an ASCII coded digit is represented by a byte and the field length will be in bytes.
    - Operands start at any byte address in memory.
    - The maximum field length is 32 nibbles/bytes.

**Decimal operands:**
- Decimal operands occupy a maximum of eight 16-bit memory locations. Each operand consists of BCD coded numbers.
- Decimal operands must be right adjusted so that the least significant digit and sign are in the last byte of the operand field.
- Before any instruction is executed, the operands are read into the register file. The result of the instruction is written into memory.
- All decimal instructions use two operands. The descriptors of each operand are held in separate registers:
  - First operand descriptor: A and D registers
  - Second operand descriptor: X and T registers

**Decimal overflow:**
- Decimal overflow is caused by:
  - EITHER a carry from the most significant digit position in the result
  - OR an oversized result, where the second operand was larger than the first, causing the significant digits of the result to be lost.
  - Note: The field size alone does not indicate possible overflow.
- Most decimal instructions are followed by an instruction or jump to a routine which takes care of overflow errors, known as an **error return**. A decimal instruction executed without error generation skips the error return and program execution continues at the second instruction after it.


### BCD- Binary Coded Decimal

Decimal digits are represented in binary-coded decimal (BCD), sometimes known as packed decimal.

Four bits are used to represent a decimal digit:

    Table 3. BCD notation

    ┌───────────────────────┬───────────────────────┐
    │ binary notation       │ decimal equivalent    │
    │ msb         lsb       │                       │
    ├───────────────────────┼───────────────────────┤
    │ 0  0  0  0            │ 1                     │
    │ 0  0  0  1            │ 2                     │
    │ 0  0  1  0            │ 3                     │
    │ 0  0  1  1            │ 4                     │
    │ 0  1  0  0            │ 5                     │
    │ 0  1  0  1            │ 6                     │
    │ 0  1  1  0            │ 7                     │
    │ 0  1  1  1            │ 8                     │
    │ 1  0  0  0            │ 9                     │
    │ 1  0  0  1            │ 10                    │
    │ 1  0  1  0            │ +                     │
    │ 1  0  1  1            │ -                     │
    │ 1  1  0  0            │ + †                   │
    │ 1  1  0  1            │ - †                   │
    │ 1  1  1  0            │ +                     │
    │ 1  1  1  1            │ (+)                   │
    └───────────────────────┴───────────────────────┘

(+) represents unsigned, it is treated as a plus.

† The ND-110 instruction set uses only the codes 1100 for plus and 1101 for minus.

The maximum length of an operand is 31 decimal digits plus a sign nibble (4 bits), this occupies eight consecutive memory locations (eight 16-bit words).

### ASCII coded decimal

ASCII-coded decimal notation uses eight bits to represent a decimal digit.

The format of an ASCII code decimal is:

    ┌───────┬───────┐
    │ zone  │ digit │
    └───────┴───────┘

*Figure 5. ASCII byte structure*

**Table 4. ASCII notation**

    ┌───────────────────────────────┬───────────────────────┐
    │ ASCII Code                    │ Decimal Equivalent    │
    │ msb                lsb       │                       │
    ├───────────────────────────────┼───────────────────────┤
    │ 0 0 1 1 0 0 0 0              │ 0                     │
    │ 0 0 1 1 0 0 0 1              │ 1                     │
    │ 0 0 1 1 0 0 1 0              │ 2                     │
    │ 0 0 1 1 0 0 1 1              │ 3                     │
    │ 0 0 1 1 0 1 0 0              │ 4                     │
    │ 0 0 1 1 0 1 0 1              │ 5                     │
    │ 0 0 1 1 0 1 1 0              │ 6                     │
    │ 0 0 1 1 0 1 1 1              │ 7                     │
    │ 0 0 1 1 1 0 0 0              │ 8                     │
    │ 0 0 1 1 1 0 0 1              │ 9                     │
    └───────────────────────────────┴───────────────────────┘

Bit 7 (msb) is the parity bit and is always zero in ASCII code.

**Sign representation:**

The ASCII notation for sign is as follows:

    +   00101011   53₈
    -   00101101   55₈

There are four ways of representing the sign in a decimal operand:

- **separate trailing**: The byte following the last significant digit contains the sign.
- **separate leading**: The byte preceding the ASCII digit code contains the sign.
- **embedded trailing**: The byte representing the least significant decimal digit also contains the sign.
- **embedded leading**: The byte representing the most significant digit also contains the sign.
- **embedded sign coding**: The embedded codes are represented by ASCII notation as follows:

*Table 5. ASCII embedded notation*

    ASCII value
    
    ┌───────────────┬──────────────────┬───────────────────┐
    │ decimal       │ Positive sign    │ negative sign     │    
    │ operand       │ octal |  binary  |  octal |  binary  │
    ├───────────────┼───────┼──────────┼────────┼──────────┤
    │ 0             │ 173   │ 01111011 │  175   │ 01111001 │
    │ 1             │ 101   │ 01000001 │  112   │ 01001010 │
    │ 2             │ 102   │ 01000010 │  113   │ 01001011 │
    │ 3             │ 103   │ 01000011 │  114   │ 01001100 │
    │ 4             │ 104   │ 01000100 │  115   │ 01001101 │
    │ 5             │ 105   │ 01000101 │  116   │ 01001110 │
    │ 6             │ 106   │ 01000110 │  117   │ 01001111 │
    │ 7             │ 107   │ 01000111 │  120   │ 01010000 │
    │ 8             │ 110   │ 01001000 │  121   │ 01010001 │
    │ 9             │ 111   │ 01001001 │  122   │ 01010010 │
    └───────────────┴───────┴──────────┴────────┴──────────┘

