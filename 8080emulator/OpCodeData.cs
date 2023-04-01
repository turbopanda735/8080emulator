using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _8080emulator
{
    public struct OpCodeData
    {
        public ushort OpCode;
        public ushort NNN;
        public byte NN, X, Y, N;
    }
}
