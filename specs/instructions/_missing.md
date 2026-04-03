

ND-110 ?

	// INSTRUCTIONS TO SPEED UP SINTRAN III SEGMENT HANDLING *  THE INSTRUCTION GROUP 'S3SEG‘ OPCODE 1403XX

	// PDF Page 101 (page number 99) in "MICROPROGRAMLISTNING FOR ND-110_32 BIT VERSION K-Gandalf-OCR.pdf"
	// PDF Page 133+ in "ND-06029-1-EN ND-110 Instruction Set.PDF"
	// CX instructions ref https://www.ndwiki.org/wiki/ND-1xx_Instructions

	/*			  
	    MOVEW			1431xx Move block of words (xx is in the range 00 through 08)
		TSET			140123 Test and set
		RDUS			140127 Read don’t use cache
		SETPT			140300 Set page tables
		CLEPT			140301 Clear page tables
		CLNREENT		140302 Clear non reentrant
		CHREENTPAGES	140303 Change not reentrant pages
		CLEPU			140304 Clear page tables, collect PGU information
	*/



Mnemonic | Octal
=================
* SETPT | 0140300*
* CLEPT | 0140301
* CLNREENT | 0140302
* CHREENTPAGES | 0140303
* CLEPU | 0140304


ND-110 advanced ?
=================
Mnemonic | Octal | Comment
=====================
WGLOB | 0140500 | ✔️ X
RGLOB | 0140501 | ✔️ X
INSPL | 0140502 | ✔️X
REMPL | 0140503 | ✔️ X
CNREK | 0140504 | ✔️X
CLPT | 0140505 | ✔️X
ENPT | 0140506 | ✔️X
REPT | 0140507 | ✔️ X
LBIT | 0140510 | ✔️ X
LBITP | 0140511 | ✔️ X
SBIT | 0140512 | ✔️ X
SBITP | 0140513 | ✔️ X
LBYTP | 0140514 | ✔️ X
SBYTP | 0140515 | ✔️ X
TSETP | 0140516 | ✔️ X
RDUSP | 0140517 | ✔️X
LASB | 0140700 | ✔️ X
SASB | 0140701 | ✔️X
LACB | 0140702 | ✔️ X
SACB | 0140703 | ✔️ X
LXSB | 0140704 | ✔️ X
LXCB | 0140705 | ✔️ X
SZSB | 0140706 | ✔️ X
SZCB | 0140707 | ✔️ X