# NemesisDecomp
My Nemesis Decompression programme, written in C# in just under 2 hours. Based on the Sonic the Hedgehog 1's disassembly's approach of decompressing this compression format.

## What is Nemesis Compression Format?
[See here!](https://segaretro.org/Nemesis_compression)
Long story short, it is a compression format used on MegaDrive/Genesis to compress the graphics.

## Then wtf is this?
This is a rewrited version of this in C#. Well theoretically it should work but nobody tested it.

## Why you didn't test it?
~~cuz im lazy.~~
Because I need some compressed data.




If you have any, pls test this 

it should write the data to:



".\\out\\Memory.bin"



## Can you show a comparison of the two?

yes



Sonic the Hedgehog (asm68k):
```
; ---------------------------------------------------------------------------
; Nemesis decompression	subroutine, decompresses art directly to VRAM
; Inputs:
; a0 = art address

; For format explanation see http://info.sonicretro.org/Nemesis_compression
; ---------------------------------------------------------------------------

; ||||||||||||||| S U B R O U T I N E |||||||||||||||||||||||||||||||||||||||

; Nemesis decompression to VRAM
NemDec:
		movem.l	d0-a1/a3-a5,-(sp)
		lea	(NemPCD_WriteRowToVDP).l,a3	; write all data to the same location
		lea	(vdp_data_port).l,a4	; specifically, to the VDP data port
		bra.s	NemDecMain

; ||||||||||||||| S U B R O U T I N E |||||||||||||||||||||||||||||||||||||||

; Nemesis decompression subroutine, decompresses art to RAM
; Inputs:
; a0 = art address
; a4 = destination RAM address
NemDecToRAM:
		movem.l	d0-a1/a3-a5,-(sp)
		lea	(NemPCD_WriteRowToRAM).l,a3 ; advance to the next location after each write

NemDecMain:
		lea	(v_ngfx_buffer).w,a1
		move.w	(a0)+,d2	; get number of patterns
		lsl.w	#1,d2
		bcc.s	loc_146A	; branch if the sign bit isn't set
		adda.w	#NemPCD_WriteRowToVDP_XOR-NemPCD_WriteRowToVDP,a3	; otherwise the file uses XOR mode

loc_146A:
		lsl.w	#2,d2	; get number of 8-pixel rows in the uncompressed data
		movea.w	d2,a5	; and store it in a5 because there aren't any spare data registers
		moveq	#8,d3	; 8 pixels in a pattern row
		moveq	#0,d2
		moveq	#0,d4
		bsr.w	NemDec_BuildCodeTable
		move.b	(a0)+,d5	; get first byte of compressed data
		asl.w	#8,d5	; shift up by a byte
		move.b	(a0)+,d5	; get second byte of compressed data
		move.w	#$10,d6	; set initial shift value
		bsr.s	NemDec_ProcessCompressedData
		movem.l	(sp)+,d0-a1/a3-a5
		rts	
; End of function NemDec

; ---------------------------------------------------------------------------
; Part of the Nemesis decompressor, processes the actual compressed data
; ---------------------------------------------------------------------------

; ||||||||||||||| S U B	R O U T	I N E |||||||||||||||||||||||||||||||||||||||


NemDec_ProcessCompressedData:
		move.w	d6,d7
		subq.w	#8,d7	; get shift value
		move.w	d5,d1
		lsr.w	d7,d1	; shift so that high bit of the code is in bit position 7
		cmpi.b	#%11111100,d1	; are the high 6 bits set?
		bhs.s	NemPCD_InlineData	; if they are, it signifies inline data
		andi.w	#$FF,d1
		add.w	d1,d1
		move.b	(a1,d1.w),d0	; get the length of the code in bits
		ext.w	d0
		sub.w	d0,d6	; subtract from shift value so that the next code is read next time around
		cmpi.w	#9,d6	; does a new byte need to be read?
		bhs.s	loc_14B2	; if not, branch
		addq.w	#8,d6
		asl.w	#8,d5
		move.b	(a0)+,d5	; read next byte

loc_14B2:
		move.b	1(a1,d1.w),d1
		move.w	d1,d0
		andi.w	#$F,d1	; get palette index for pixel
		andi.w	#$F0,d0

NemPCD_ProcessCompressedData:
		lsr.w	#4,d0	; get repeat count

NemPCD_WritePixel:
		lsl.l	#4,d4	; shift up by a nybble
		or.b	d1,d4	; write pixel
		subq.w	#1,d3	; has an entire 8-pixel row been written?
		bne.s	NemPCD_WritePixel_Loop	; if not, loop
		jmp	(a3)	; otherwise, write the row to its destination, by doing a dynamic jump to NemPCD_WriteRowToVDP, NemDec_WriteAndAdvance, NemPCD_WriteRowToVDP_XOR, or NemDec_WriteAndAdvance_XOR
; End of function NemDec_ProcessCompressedData


; ||||||||||||||| S U B	R O U T	I N E |||||||||||||||||||||||||||||||||||||||


NemPCD_NewRow:
		moveq	#0,d4	; reset row
		moveq	#8,d3	; reset nybble counter

NemPCD_WritePixel_Loop:
		dbf	d0,NemPCD_WritePixel
		bra.s	NemDec_ProcessCompressedData
; ===========================================================================

NemPCD_InlineData:
		subq.w	#6,d6	; 6 bits needed to signal inline data
		cmpi.w	#9,d6
		bhs.s	loc_14E4
		addq.w	#8,d6
		asl.w	#8,d5
		move.b	(a0)+,d5

loc_14E4:
		subq.w	#7,d6	; and 7 bits needed for the inline data itself
		move.w	d5,d1
		lsr.w	d6,d1	; shift so that low bit of the code is in bit position 0
		move.w	d1,d0
		andi.w	#$F,d1	; get palette index for pixel
		andi.w	#$70,d0	; high nybble is repeat count for pixel
		cmpi.w	#9,d6
		bhs.s	NemPCD_ProcessCompressedData
		addq.w	#8,d6
		asl.w	#8,d5
		move.b	(a0)+,d5
		bra.s	NemPCD_ProcessCompressedData
; End of function NemPCD_NewRow

; ===========================================================================

NemPCD_WriteRowToVDP:
		move.l	d4,(a4)	; write 8-pixel row
		subq.w	#1,a5
		move.w	a5,d4	; have all the 8-pixel rows been written?
		bne.s	NemPCD_NewRow	; if not, branch
		rts		; otherwise the decompression is finished
; ===========================================================================
NemPCD_WriteRowToVDP_XOR:
		eor.l	d4,d2	; XOR the previous row by the current row
		move.l	d2,(a4)	; and write the result
		subq.w	#1,a5
		move.w	a5,d4
		bne.s	NemPCD_NewRow
		rts	
; ===========================================================================

NemPCD_WriteRowToRAM:
		move.l	d4,(a4)+
		subq.w	#1,a5
		move.w	a5,d4
		bne.s	NemPCD_NewRow
		rts	
; ===========================================================================
NemPCD_WriteRowToRAM_XOR:
		eor.l	d4,d2
		move.l	d2,(a4)+
		subq.w	#1,a5
		move.w	a5,d4
		bne.s	NemPCD_NewRow
		rts	

; ||||||||||||||| S U B	R O U T	I N E |||||||||||||||||||||||||||||||||||||||
; ---------------------------------------------------------------------------
; Part of the Nemesis decompressor, builds the code table (in RAM)
; ---------------------------------------------------------------------------


NemDec_BuildCodeTable:
		move.b	(a0)+,d0	; read first byte

NemBCT_ChkEnd:
		cmpi.b	#$FF,d0	; has the end of the code table description been reached?
		bne.s	NemBCT_NewPALIndex	; if not, branch
		rts	; otherwise, this subroutine's work is done
; ===========================================================================

NemBCT_NewPALIndex:
		move.w	d0,d7

NemBCT_Loop:
		move.b	(a0)+,d0	; read next byte
		cmpi.b	#$80,d0	; sign bit being set signifies a new palette index
		bhs.s	NemBCT_ChkEnd	; a bmi could have been used instead of a compare and bcc
		
		move.b	d0,d1
		andi.w	#$F,d7	; get palette index
		andi.w	#$70,d1	; get repeat count for palette index
		or.w	d1,d7	; combine the two
		andi.w	#$F,d0	; get the length of the code in bits
		move.b	d0,d1
		lsl.w	#8,d1
		or.w	d1,d7	; combine with palette index and repeat count to form code table entry
		moveq	#8,d1
		sub.w	d0,d1	; is the code 8 bits long?
		bne.s	NemBCT_ShortCode	; if not, a bit of extra processing is needed
		move.b	(a0)+,d0	; get code
		add.w	d0,d0	; each code gets a word-sized entry in the table
		move.w	d7,(a1,d0.w)	; store the entry for the code
		bra.s	NemBCT_Loop	; repeat
; ===========================================================================

; the Nemesis decompressor uses prefix-free codes (no valid code is a prefix of a longer code)
; e.g. if 10 is a valid 2-bit code, 110 is a valid 3-bit code but 100 isn't
; also, when the actual compressed data is processed the high bit of each code is in bit position 7
; so the code needs to be bit-shifted appropriately over here before being used as a code table index
; additionally, the code needs multiple entries in the table because no masking is done during compressed data processing
; so if 11000 is a valid code then all indices of the form 11000XXX need to have the same entry
NemBCT_ShortCode:
		move.b	(a0)+,d0	; get code
		lsl.w	d1,d0	; get index into code table
		add.w	d0,d0	; shift so that high bit is in bit position 7
		moveq	#1,d5
		lsl.w	d1,d5
		subq.w	#1,d5	; d5 = 2^d1 - 1

NemBCT_ShortCode_Loop:
		move.w	d7,(a1,d0.w)	; store entry
		addq.w	#2,d0	; increment index
		dbf	d5,NemBCT_ShortCode_Loop	; repeat for required number of entries
		bra.s	NemBCT_Loop
; End of function NemDec_BuildCodeTable
```

Mine implementation (C#):
```
/*  ProximalElk6186's NemesisDecompressor, written in C#!
 *  u can use it in ur projects
 *  just credit me ok?
 *  As a base, it uses Sonic the Hedgehog 1's Disassembly
 *  by SonicRetro
 *  idk if it works but it should
 *  k lets go
*/
using System;
public class Program
{
    public static string memorystrg { get; private set; }

     static byte[]? memory;
     static int a0;
     static int a1;
     static int a2;
     static int a3;
     static int a4;
     static int a5;
     static int d0;
     static int d1;
     static int d2;
     static int d3;
     static int d4;
     static int d5;
     static int d6;
     static int d7;
     public static string MemoryOut = ".\\Memory.bin";

    static void Main(string[] args)
    {
        if (memory != null)
        {
            string memorystrg = BitConverter.ToString(memory).Replace("-", ""); //To parse
        }
        if (args.Length != 15)
        {
            Console.WriteLine("Usage: NemesisDecomp.exe <byte memory> <a0> <a1> <a2> <a3> <a4> <a5> <d0> <d1> <d2> <d3> <d4> <d5> <d6> <d7>");
            return;
        }
        memorystrg = args[0]; //To parse
        memory = HexStringToByteArray(memorystrg);

        a0 = int.Parse(args[1]);
        a1 = int.Parse(args[2]);
        a2 = int.Parse(args[3]);
        a3 = int.Parse(args[4]);
        a4 = int.Parse(args[5]);
        a5 = int.Parse(args[6]);
        d0 = int.Parse(args[7]);
        d1 = int.Parse(args[8]);
        d2 = int.Parse(args[9]);
        d3 = int.Parse(args[10]);
        d4 = int.Parse(args[11]);
        d5 = int.Parse(args[12]);
        d6 = int.Parse(args[13]);
        d7 = int.Parse(args[14]);
    }
    public static byte[] HexStringToByteArray(string hex)

    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }



    // The actual decompression starts here 😀, 60 strings just for parsing the values from args! F*cking World Record!







    //    public static void NemDec()
    //    {
    //        // Save registers (emulated by local variables)
    //        var savedRegisters = new { d0, a0, a1, a3, a4, a5 };
    //        
    //        a3 = (int)NemPCD_WriteRowToVDP; // Write all data to the same location
    //        a4 = (int)vdp_data_port;        // Specifically, to the VDP data port
    //
    //
    //
    //
    //
    //
    //        NemDecMain();
    //
    //        // Restore registers
    //        (d0, a0, a1, a3, a4, a5) = (savedRegisters.d0, savedRegisters.a0, savedRegisters.a1, savedRegisters.a3, savedRegisters.a4, savedRegisters.a5);
    //    }
    //
    //    public static void NemDecToRAM(int artAddress, int destRamAddress)
    //    {
    //        // Save registers (emulated by local variables)
    //        var savedRegisters = new { d0, a0, a1, a3, a4, a5 };
    //
    //        a3 = (int)NemPCD_WriteRowToRAM;
    //        a0 = artAddress;
    //        a4 = destRamAddress;
    //
    //        NemDecMain();
    //
    //        // Restore registers
    //        (d0, a0, a1, a3, a4, a5) = (savedRegisters.d0, savedRegisters.a0, savedRegisters.a1, savedRegisters.a3, savedRegisters.a4, savedRegisters.a5);
    //    }



    private delegate void WriteRowMethod();

    private static WriteRowMethod WriteRowToVDP;
    private static WriteRowMethod WriteRowToRAM;
    private static WriteRowMethod WriteRowToVDP_XOR;
    private static WriteRowMethod WriteRowToRAM_XOR;

    public static string GetMemoryOut() => MemoryOut;

    public static void NemDec(string memoryOut, byte[]? memory)
    {
        // Save registers (emulated by local variables)
        var savedRegisters = new { d0, a0, a1, a3, a4, a5 };

        // Initialize delegates
        WriteRowToVDP = NemPCD_WriteRowToVDP;
        WriteRowToVDP_XOR = NemPCD_WriteRowToVDP_XOR;

        // Choose which method to use
        WriteRowMethod currentWriteRowMethod;
        bool useXOR = false; // Replace with actual condition

        if (useXOR)
        {
            currentWriteRowMethod = WriteRowToVDP_XOR;
        }
        else
        {
            currentWriteRowMethod = WriteRowToVDP;
        }

        a4 = (int)vdp_data_port; // Specifically, to the VDP data port

        // Invoke the chosen method
        currentWriteRowMethod();

        NemDecMain();

        // Restore registers
        (d0, a0, a1, a3, a4, a5) = (savedRegisters.d0, savedRegisters.a0, savedRegisters.a1, savedRegisters.a3, savedRegisters.a4, savedRegisters.a5);

        // Save the output to a file specified in string MemoryOut (by default it is ".\\Memory.bin")
        File.WriteAllBytes(memoryOut, memory);
    }

    public static void NemDecToRAM(int artAddress, int destRamAddress)
    {
        // Save registers (emulated by local variables)
        var savedRegisters = new { d0, a0, a1, a3, a4, a5 };

        WriteRowToRAM = NemPCD_WriteRowToRAM;
        WriteRowToRAM_XOR = NemPCD_WriteRowToRAM_XOR;
        a0 = artAddress;
        a4 = destRamAddress;

        NemDecMain();

        // Restore registers
        (d0, a0, a1, a3, a4, a5) = (savedRegisters.d0, savedRegisters.a0, savedRegisters.a1, savedRegisters.a3, savedRegisters.a4, savedRegisters.a5);
    }



    private static void InitializeDelegates()
    {
        WriteRowToVDP = NemPCD_WriteRowToVDP;
        WriteRowToVDP_XOR = NemPCD_WriteRowToVDP_XOR;
    }





    private static void NemDecMain()
    {
        InitializeDelegates(); // Ensure delegates are initialized

        // Declare and initialize the delegate based on a condition
        WriteRowMethod currentWriteRowMethod;

        //bool useXOR = false; // Replace with actual condition

        a1 = (int)v_ngfx_buffer;
        d2 = (short)(memory[a0++] << 1); // Get number of patterns

        if ((d2 & 0x8000) != 0)
        {
            currentWriteRowMethod = WriteRowToVDP_XOR;
        }
        else
        {
            currentWriteRowMethod = WriteRowToVDP;
        }

        d2 <<= 2;    // Get number of 8-pixel rows in the uncompressed data
        a5 = d2;     // Store it in a5

        d3 = 8;      // 8 pixels in a pattern row
        d2 = 0;      // Clear d2
        d4 = 0;      // Clear d4

        NemDec_BuildCodeTable();

        d5 = memory[a0++];
        d5 = (d5 << 8) | memory[a0++];
        d6 = 0x10;

        // Invoke the selected method
        currentWriteRowMethod();

        NemDec_ProcessCompressedData();
    }



    private static void NemDec_ProcessCompressedData()
    {
        while (true)
        {
            d7 = d6 - 8;
            d1 = d5 >> d7;

            if ((d1 & 0xFC) == 0xFC)
            {
                NemPCD_InlineData();
                continue;
            }

            d1 &= 0xFF;
            d1 *= 2;
            d0 = memory[a1 + d1];
            d6 -= d0;

            if (d6 < 9)
            {
                d6 += 8;
                d5 = (d5 << 8) | memory[a0++];
            }

            d1 = memory[a1 + d1 + 1];
            d0 = d1;
            d1 &= 0x0F;
            d0 &= 0xF0;

            NemPCD_ProcessCompressedData();
        }
    }

    private static void NemPCD_ProcessCompressedData()
    {
        d0 = d0 >> 4;
    }

    private static void NemPCD_InlineData()
    {
        d6 -= 6;

        if (d6 < 9)
        {
            d6 += 8;
            d5 = (d5 << 8) | memory[a0++];
        }

        d6 -= 7;
        d1 = d5 >> d6;
        d0 = d1;
        d1 &= 0x0F;
        d0 &= 0x70;

        if (d6 < 9)
        {
            d6 += 8;
            d5 = (d5 << 8) | memory[a0++];
        }

        NemPCD_ProcessCompressedData();
    }

    private static void NemPCD_WriteRowToVDP()
    {
        memory[a4] = (byte)d4;
        a5--;
        d4 = a5;

        if (a5 != 0)
        {
            NemPCD_NewRow();
        }
    }

    private static void NemPCD_WriteRowToVDP_XOR()
    {
        d2 ^= d4;
        memory[a4] = (byte)d2;
        a5--;
        d4 = a5;

        if (a5 != 0)
        {
            NemPCD_NewRow();
        }
    }

    private static void NemPCD_WriteRowToRAM()
    {
        memory[a4++] = (byte)d4;
        a5--;
        d4 = a5;

        if (a5 != 0)
        {
            NemPCD_NewRow();
        }
    }

    private static void NemPCD_WriteRowToRAM_XOR()
    {
        d2 ^= d4;
        memory[a4++] = (byte)d2;
        a5--;
        d4 = a5;

        if (a5 != 0)
        {
            NemPCD_NewRow();
        }
    }

    private static void NemPCD_NewRow()
    {
        d4 = 0;
        d3 = 8;
    }

    private static void NemDec_BuildCodeTable()
    {
        while (true)
        {
            d0 = memory[a0++];
            if (d0 == 0xFF)
            {
                return;
            }

            d7 = d0;

            while (true)
            {
                d0 = memory[a0++];
                if ((d0 & 0x80) != 0)
                {
                    break;
                }

                d1 = d0 & 0x70;
                d7 &= 0x0F;
                d7 |= d1;
                d0 &= 0x0F;

                int index = d0 << 8 | memory[a0++];
                memory[a1 + index] = (byte)d7;

                if (d0 != 8)
                {
                    index += (1 << (8 - d0)) - 1;
                    for (int i = 0; i <= index; i++)
                    {
                        memory[a1 + index] = (byte)d7;
                    }
                }
            }
        }
    }

    private static byte vdp_data_port;
    private static byte v_ngfx_buffer;

    // Placeholders for dynamic jumps
    //private static Action NemPCD_WriteRowToVDP = () => { };
    //private static Action NemPCD_WriteRowToVDP_XOR = () => { };
    //private static Action NemPCD_WriteRowToRAM = () => { };
    //private static Action NemPCD_WriteRowToRAM_XOR = () => { };
}   //
```

Yeah, it's twice as long.



## Why did you make this software?

cuz i can



## I know that hand-translating assembly code to C# or any other high-level programming language is hard, then what did YOU do?
I used AI (ChatGPT), then corrected some of the errors (0 compiler errors now - my record!)
