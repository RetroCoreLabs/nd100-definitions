#### Description

These instructions use byte operands, which occupy fields within memory. Byte operands are specified by two 16-bit words, known as descriptors, giving the start address and the field length.




   as
sr dr  15                                                        0
A  X   ┌──────────────────────────────────────────────────────────┐
       │                      word address                        │  D1
       └──────────────────────────────────────────────────────────┘

   15 14 13 12 11                                              0
D  T  ┌──┬──┬─┬─┬───────────────────────────────────────────────┐
      │lr│pt│0│X│                field length                   │  D2
      └──┴──┴─┴─┴───────────────────────────────────────────────┘

Note:
  bit 13 should always be zero
  bit 12 can be any value


**Descriptor D1:**  
- The first word gives the start address of the operand.

**Descriptor D2:**  
- The second word contains:
  - **lr** (bit 15): Determines whether the operand starts in the left or right byte of the memory location addressed by D1.
    - `lr = 0`: left byte start
    - `lr = 1`: right byte start
  - **pt** (bit 14): Page table mode.
    - `pt = 0`: normal
    - `pt = 1`: alternative
  - **Bit 13:** Should always be zero.
  - **Bit 12:** Can be any value.
  - **Field length (bits 11-0):** Number of operand bytes (maximum 4095 bytes).

**Register usage:**  
- The A and D registers hold the source operand descriptor (D1 and D2).
- The X and T registers hold the destination operand descriptor.

**Summary Table:**

| Descriptor | Bits         | Purpose                                 |
|------------|--------------|-----------------------------------------|
| D1         | 15-0         | Start address of operand                |
| D2         | 15           | lr: left/right byte start               |
|            | 14           | pt: page table mode                     |
|            | 13           | Must always be zero                     |
|            | 12           | Can be any value                        |
|            | 11-0         | Field length (number of operand bytes)  |

> **Note:**  
> - Bit 13 of D2 should always be zero.  
> - Bit 12 of D2 can be any value.

