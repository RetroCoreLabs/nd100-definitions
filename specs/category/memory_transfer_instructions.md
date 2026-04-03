### SINTRAN III Memory Transfer Instructions

These instructions are **PRIVILEGED** and only available to:

- Programs running in system mode (rings 2-3)
- Programs running without memory protection

These instructions read/write from/to physical memory locations independent of whether paging is on or off. If the address is within the page-table range then the page tables are affected.

---

### Instruction format

```
<physical instruction mnemonic> <displacement>
```

### Instruction structure

```
 | 15                      6 | 5          3 | 2-0 |
 |---------------------------|--------------|-----|
 | physical memory operation | displacement |type |
```

- **physical memory operation type** (bits 15-6 and 2-0):
  There are seven physical memory read/write instructions, specified by a base octal code of 143300₈ (bits 15-6) and type field (bits 2-0).
- **disp.**: The contents of the T and X register give the effective address of the physical memory location (see page 28). 
- A 3-bit displacement can be added to the X register within the instruction code. This is denoted by Δ in the following codes:

| instruction mnemonic | octal code |
|----------------------|------------|
| LDATX                | 1433Δ0     |
| LDXTX                | 1433Δ1     |
| LDDTX                | 1433Δ2     |
| LDBTX                | 1433Δ3     |
| STATX                | 1433Δ4     |
| STZTX                | 1433Δ5     |
| STDTX                | 1433Δ6     |

† If you use programs written for ND-100 computers with the microprogram version numbers 015xx A-J (48 bit) or 026xx A-F (32 bit), LDBTX would have been followed by a word containing 177777₈. 

This is not necessary for later ND-100 versions nor the ND-110. Running these earlier programs may change the status of the K bit in the ND-110 and later ND-100s.
