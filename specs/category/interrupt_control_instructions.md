
### INTERRUPT CONTROL INSTRUCTIONS

These instructions are **PRIVILEGED** and only available to:

- Programs running in system mode (rings 2-3)
- Programs running without memory protection

These instructions control the CPU interrupt system.


---

## General Description

The ND-110 has a priority interrupt system with 16 program levels. 

Each program level has its own set of working registers (A, B, D, L, P, STS, T, X). 

The program levels have increasing priority, that is, program level 15 has the highest priority and program level 0 the lowest.

The 16 levels are subdivided as follows:

| level | used for                           | controlled by        |
|-------|------------------------------------|----------------------|
| 15    | very fast user interrupts          | program/ext. device  |
| 14    | internal hardware status interrupts| program/ext. device  |
| 13-10 | vectored interrupts*               | program/ext. device  |
| 9-0   | system and user programs           | program              |

*2048 possible sources

---

### Program level selection and control

Program level selection and control is via two 16-bit registers:

- **PID**: Priority Interrupt Detect
- **PIE**: Priority Interrupt Enable

PID is affected by program and external interrupts; PIE is controlled by program only. They can only be changed or monitored by the privileged instructions: `TRA`, `TRR`, `MST`, and `MCL` (see pages 118-120).

**Note:**
When the power is turned on, the power-up sequence resets PIE and PID so that program level 0 is selected.

---

### Interrupt programming is via three registers:

- **IIC**: Internal Interrupt Code
- **IIE**: Internal Interrupt Enable
- **PVL**: Previous Level (of hardware interrupt source)

