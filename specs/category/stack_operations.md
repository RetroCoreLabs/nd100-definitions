
#### Description

These instructions handle stack operations improving the execution time of high-level language-based programs.

The **B** register will always point to a "stack-frame" containing:

| Stack frame content mnemonic | Pointed to by B= (octal) | Description                            |
|-----------------------------|---------------------------|----------------------------------------|
| LINK                        | -200₈                    | next instruction address †             |
| PREVB                       | -177₈                    | previous stack frame address           |
| STP                         | -176₈                    | next stack frame address               |
| SMAX                        | -175₈                    | top of stack address                   |
| —                           | -174₈                    | reserved for system use                |
| ERRCODE                     | -173₈                    | (A) after an ELEAV instruction         |

† In the case of a `LEAVE` instruction.

The stack-handling instructions are page-fault tolerant in the ND-110.
