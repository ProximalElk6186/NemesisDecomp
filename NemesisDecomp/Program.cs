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