using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _8080emulator
{
    public class OpCodeDisassembler
    {
        public OpCodeData Disassemble(int opCode)
        {
            return new OpCodeData()
            {
                OpCode = (ushort)opCode,
                NNN = (ushort)(opCode & 0x0FFF),
                NN = (byte)(opCode & 0x00FF),
                N = (byte)(opCode & 0x000F),
                X = (byte)((opCode & 0x0F00) >> 8),
                Y = (byte)((opCode & 0x00F0) >> 4)
            };
        }
    }
}
