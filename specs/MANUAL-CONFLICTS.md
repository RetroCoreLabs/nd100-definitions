# Where the manuals disagree, or disagree with these files

Found while writing the `detailed_description` texts from the two manuals (2026-11-18):
ND-06.029.1 (ND-110 Instruction Set) and ND-06.014.2A (ND-100 Reference Manual), as
transcribed in the NDInsight repository under `Reference-Manuals/`. Line numbers are lines of
those transcriptions. Nothing here has been settled; each item stays until someone checks
the printed page or a real machine.

## A field in an instruction file that the manuals do not back

| File | What the file says | What the manuals say |
|---|---|---|
| leav.yaml | `privilege: user` (kept, decided 2026-11-18) | the ND-110 index marks LEAVE privileged (line 1576); its own section, the ND-100 manual and its partner ENTR do not |
| movew.yaml | its tables mark 02, 05, 06, 07, 10 privileged | ND-110 lines 3846-3851 leave 06 unmarked; nd100x (do_movew) checks privilege for 06 too. Not tested on a machine |
| cnrek.yaml | "CLNREENT is the same operation for other SINTRAN versions" | ND-110 line 5772 says only "(SINTRAN K only)" |
| geco.yaml | the whole instruction | GECO is in neither manual; it has no `detailed_description` |

Fixed: jpl.yaml called JPL "Jump if Plus"; both manuals call it "Jump to subroutine (jump
link)" with no condition (ND-110 line 3218, ND-100 line 8329).

Fixed: saa, sab, sat and sax listed carry and overflow as changed. The manual names no flags
for them, and nd100x only loads the register (checked 2026-11-18); now `flags_affected: []`.

## The two manuals disagree

| Instruction | ND-110 (ND-06.029.1) | ND-100 (ND-06.014.2A) | Text written |
|---|---|---|---|
| ELEAV | A is loaded with an error code (lines 4428, 4433) | A is stored into ERRCODE (lines 6854, 6930) | ND-100; nd100x does the same |
| COMD | an error return, skipped unless error (lines 4153-4163) | always skips (line 6752) | both |
| ENTR, LEAVE, ELEAV | the stack instructions survive page faults (line 4410) | a page fault can destroy B; do not use them where faults happen (line 6843) | both, each labelled |
| IRW | does nothing when the current level's A register is named (line ~4660) | ... when the current level's P register is named (line 5891) | both |
| FAD, FSB, FMU | two memory words, A and D (lines ~2820-2875) | three memory words, T, A and D (lines 4466-4500) | neutral, ND-100 named for T A D |
| ADD | O dynamic, Q static (lines 2716-2720), against its own STS table (lines 3666-3667) | "dynamic" and "static" without letters (lines 4414-4416) | no letters |

## Text damaged in the transcription, so left out

- SHDE: the sign-code table (line ~4251) and the rounding limit (line 4256).
- UPACK: the BCD side of the sign table (line ~4356).
- FAD: the formulas in the ND-110 manual.
- IDENT: the ND-110 level-code table; the ND-100 codes (lines 6174-6178) were used.
- REMPL, CLPT, CNREK, ENPT: the PLANC listings in Appendix B; only what reads clearly was used.
- TRR: the example at line 4920 carries an MST code (1503xx).

## Codes printed differently in the manuals, not a real conflict

The section headings print some codes with the 3-bit field filled in: SZSB 140746 (line 6131),
LXCB 140745, LXSB 140744, LDXTX 143341 (line ~5572). With that field zero they are the files'
codes. Two are plain misprints that the manual's own tables contradict: STATX 143343 (line
5596; the table, Appendix C and the file say 143304), and SHDE 141426 in Appendix C (line
6609; the section and the file say 140126).
