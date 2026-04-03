#### Description

These instructions manipulate single bits within the working and STS registers.

Instruction structure:

    15         7 6     3   0
    ┌──────────┬───────┬───┐
    │ bit op   │  bn   │dr │
    └──────────┴───────┴───┘

Bit operation:
  Bits 11-15 are always set to one for a bit operation.
  Bits 7-10 determine the type of operation as follows:

    ┌──────────┬────────────┬───────────────────────────────┬─────────────┐
    │type bits │ mnemonic   │ description                   │ code        │
    │10 9 8 7  │            │                               │             │
    ├──────────┼────────────┼───────────────────────────────┼─────────────┤
    │ 0 0 0 0  │ BSET ZRO   │ bit ⟵ 0                      │ 174000₈     │
    │ 0 0 0 1  │ BSET ONE   │ bit ⟵ 1                      │ 174200₈     │
    │ 0 0 1 0  │ BSET BCM   │ bit ⟵ bit                    │ 174400₈     │
    │ 0 0 1 1  │ BSET BAC   │ bit ⟵ K                      │ 174600₈     │
    │ 0 1 0 0  │ BSKP ZRO   │ skip if bit = 0               │ 175000₈     │
    │ 0 1 0 1  │ BSKP ONE   │ skip if bit = 1               │ 175200₈     │
    │ 0 1 1 0  │ BSKP BCM   │ skip if bit = bit             │ 175400₈     │
    │ 0 1 1 1  │ BSKP BAC   │ skip if bit = K               │ 175600₈     │
    │ 1 0 0 0  │ BSTC       │ bit ⟵ K, K ⟵ 1              │ 176000₈     │
    │ 1 0 0 1  │ BSTA       │ bit ⟵ K, K ⟵ 0              │ 176200₈     │
    │ 1 0 1 0  │ BLDC       │ K ⟵ bit                      │ 176400₈     │
    │ 1 0 1 1  │ BLDA       │ K ⟵ bit                      │ 176600₈     │
    │ 1 1 0 0  │ BANC       │ K ⟵ (bit AND K)              │ 177000₈     │
    │ 1 1 0 1  │ BAND       │ K ⟵ (bit AND K)              │ 177200₈     │
    │ 1 1 1 0  │ BORC       │ K ⟵ (bit OR K)               │ 177400₈     │
    │ 1 1 1 1  │ BORA       │ K ⟵ (bit OR K)               │ 177600₈     │
    └──────────┴────────────┴───────────────────────────────┴─────────────┘

**Sub-instructions:**
Only the BSET and BSKP instructions have the following qualifying sub-instructions:
    ZRO
    ONE
    BCM
    BAC

**bn (bits 3-6):**
  The address of the bit to be manipulated is given by these four bits.
  Remember that each bit is given its OCTAL address.

**dr (bits 0-2):**
  The following registers allow bit operations and are specified as follows:

    ┌──────────┬──────────┬────────┐
    │ register │ mnemonic │ code   │
    ├──────────┼──────────┼────────┤
    │ STS      │ †        │ 0₈     │
    │ D        │ DD       │ 1₈     │
    │ P        │ DP       │ 2₈     │
    │ B        │ DB       │ 3₈     │
    │ L        │ DL       │ 4₈     │
    │ A        │ DA       │ 5₈     │
    │ T        │ DT       │ 6₈     │
    │ X        │ DX       │ 7₈     │
    └──────────┴──────────┴────────┘

† For STS no mnemonic is required as it is implied by the following table of compound mnemonics:

**STS register:**
There are only eight bits which can be operated on in the STS register. They have special mnemonics and unique octal code values which combine the bn and dr fields.

    ┌────────────────┬──────────┬─────────────────────────────┬─────────────┐
    │ compound       │ STS bit  │ description                 │ octal code  │
    │ mnemonic       │          │                             │             │
    ├────────────────┼──────────┼─────────────────────────────┼─────────────┤
    │ SSPTM          │ 0        │ page table flag             │ 00₈         │
    │ SSTG           │ 1        │ floating point rounding flag│ 10₈         │
    │ SSK            │ 2        │ 1 bit accumulator (K)       │ 20₈         │
    │ SSZ            │ 3        │ error flag (Z)              │ 30₈         │
    │ SSQ            │ 4        │ dynamic overflow flag (Q)   │ 40₈         │
    │ SSO            │ 5        │ static overflow flag (O)    │ 50₈         │
    │ SSC            │ 6        │ carry flag (C)              │ 60₈         │
    │ SSM            │ 7        │ multi-shift link flag (M)   │ 70₈         │
    └────────────────┴──────────┴─────────────────────────────┴─────────────┘
