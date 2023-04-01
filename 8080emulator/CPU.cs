using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace _8080emulator
{
    internal class CPU
    {
        public byte[] Registers = new byte[16];
        public byte[] RAM = new byte[4096];
        public ushort IndexRegister = 0x200;
        public ushort PC, I;
        public Stack<ushort> Stack = new Stack<ushort>();
        public Random Random = new Random();
        public Display Display = new Display(64, 32, 1);
        public byte DelayTimer;
        public byte SoundTimer;
        public byte[] Keyboard = new byte[16];
        Dictionary<byte, Action<OpCodeData>> opCodes;
        Dictionary<byte, Action<OpCodeData>> opCodesMisc;

        public CPU()
        {
            PC = (ushort)IndexRegister;
            opCodes = new Dictionary<byte, Action<OpCodeData>>
            {
                { 0x0, ClearOrReturn },
                { 0x1, Jump },
                { 0x2, CallSubroutine },
                { 0x3, SkipIfXEqual },
                { 0x4, SkipIfXNotEqual },
                { 0x5, SkipIfXEqualY },
                { 0x6, SetX },
                { 0x7, AddX },
                { 0x8, Arithmetic },
                { 0x9, SkipIfXNotEqualY },
                { 0xA, SetI },
                { 0xB, JumpWithOffset },
                { 0xC, Rnd },
                { 0xD, DrawSprite },
                { 0xE, SkipOnKey },
                { 0xF, Misc },
            };
            opCodesMisc = new Dictionary<byte, Action<OpCodeData>>
            {
                { 0x07, SetXToDelay },
                { 0x0A, WaitForKey },
                { 0x15, SetDelay },
                { 0x18, SetSound },
                { 0x1E, AddXToI },
                { 0x29, SetIForChar },
                { 0x33, BinaryCodedDecimal },
                { 0x55, SaveX },
                { 0x65, LoadX }
            };
        }
        public byte[] LoadROM(string fileName)
        {
            byte[] rom;
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                FileInfo fileInfo = new FileInfo(fileName);
                rom = new byte[fileInfo.Length];

                for (int i = 0; i < fileInfo.Length; i++)
                {
                    rom[i] = reader.ReadByte();
                }
            }
            return rom;
        }
        public void Cycle()
        {
            //fetch
            var opCode = (ushort)(RAM[PC++] << 8 | RAM[PC++]);
            //decode
            var op = new OpCodeData()
            {
                OpCode = opCode,
                NNN = (ushort)(opCode & 0x0FFF),
                NN = (byte)(opCode & 0x00FF),
                N = (byte)(opCode & 0x000F),
                X = (byte)((opCode & 0x0F00) >> 8),
                Y = (byte)((opCode & 0x00F0) >> 4),
            };
            //execute
            opCodes[(byte)(opCode >> 12)](op);
            // Update timers
            if (DelayTimer > 0)
            {
                --DelayTimer;
            }
            if (SoundTimer > 0)
            {
                if (SoundTimer == 1)
                    Console.WriteLine("BEEP!\n");
                --SoundTimer;
            }
        }
        void ClearOrReturn(OpCodeData data)
        {
            switch (data.NN)
            {
                case 0xE0:
                    Console.WriteLine("clear screen");
                    Display.Clear();
                    break;
                case 0xEE:
                    Console.WriteLine("return from subroutine");
                    PC = Stack.Pop();
                    break;
            }
        }
        void Jump(OpCodeData data)
        {
            Console.WriteLine("jump");
            PC = data.NNN;
        }
        void CallSubroutine(OpCodeData data)
        {
            Console.WriteLine("call rubroutinne");
            Stack.Push(PC);
            PC = data.NNN;
        }

        void SkipIfXEqual(OpCodeData data)
        {
            Console.WriteLine("skip if x equal");
            if (Registers[data.X] == data.NN)
            {
                PC += 2;
            }
        }
        void SkipIfXEqualY(OpCodeData data)
        {
            Console.WriteLine("skip if x equals y");
            if (Registers[data.X] == Registers[data.Y])
            {
                PC += 2;
            }
        }
        void SkipIfXNotEqual(OpCodeData data)
        {
            Console.WriteLine("skip if x not equal");
            if (Registers[data.X] != data.NN)
            {
                PC += 2;
            }
        }
        void SetX(OpCodeData data)
        {
            Console.WriteLine("set x");
            Registers[data.X] = data.NN;
        }
        void AddX(OpCodeData data)
        {
            Console.WriteLine("add x");
            Registers[data.X] += data.NN;
        }
        void Arithmetic(OpCodeData data)
        {
            Console.WriteLine("set v[x] to v[y]");
            switch (data.N)
            {
                case 0x0:
                    Registers[data.X] = Registers[data.Y];
                    break;
                case 0x1:
                    Registers[data.X] |= Registers[data.Y];
                    break;
                case 0x2:
                    Registers[data.X] &= Registers[data.Y];
                    break;
                case 0x3:
                    Registers[data.X] ^= Registers[data.Y];
                    break;
                case 0x4:
                    Registers[0xF] = (byte)(Registers[data.X] + Registers[data.Y] > 0xFF ? 1 : 0); // Set flag if we overflowed.
                    Registers[data.X] += Registers[data.Y];
                    break;
                case 0x5:
                    Registers[0xF] = (byte)(Registers[data.X] > Registers[data.Y] ? 1 : 0); // Set flag if we underflowed.
                    Registers[data.X] -= Registers[data.Y];
                    break;
                case 0x6:
                    Registers[0xF] = (byte)((Registers[data.X] & 0x1) != 0 ? 1 : 0); // Set flag if we shifted a 1 off the end.
                    Registers[data.X] /= 2; // Shift right.
                    break;
                case 0x7: // Note: This is Y-X, 5 was X-Y.
                    Registers[0xF] = (byte)(Registers[data.Y] > Registers[data.X] ? 1 : 0); // Set flag if we underflowed.
                    Registers[data.Y] -= Registers[data.X];
                    break;
                case 0xE:
                    Registers[0xF] = (byte)((Registers[data.X] & 0xF) != 0 ? 1 : 0); // Set flag if we shifted a 1 off the end.
                    Registers[data.X] *= 2; // Shift left.
                    break;
            }
        }
        void SkipIfXNotEqualY(OpCodeData data)
        {
            Console.WriteLine("skip if x not equal");
            if (Registers[data.X] != Registers[data.Y])
            {
                PC += 2;
            }
        }
        void SetI(OpCodeData data)
        {
            Console.WriteLine("set i");
            I = data.NNN;
        }

        void JumpWithOffset(OpCodeData data)
        {
            Console.WriteLine("jump w/ offset");
            PC = (ushort)(Registers[0] + data.NNN);
        }

        void Rnd(OpCodeData data)
        {
            Console.WriteLine("random");
            Registers[data.X] = (byte)(Random.Next(0, 256) & data.NN);
        }
        void DrawSprite(OpCodeData data)
        {
            Display.Draw(data);
            Console.WriteLine("draw sprite");
        }
        void SkipOnKey(OpCodeData data)
        {

        }
        void Misc(OpCodeData data)
        {

        }
        void SetXToDelay(OpCodeData data)
        {

        }
        void WaitForKey(OpCodeData data)
        {

        }
        void SetDelay(OpCodeData data)
        {

        }
        void SetSound(OpCodeData data)
        {

        }
        void AddXToI(OpCodeData data)
        {

        }
        void SetIForChar(OpCodeData data)
        {

        }
        void BinaryCodedDecimal(OpCodeData data)
        {

        }
        void SaveX(OpCodeData data)
        {

        }
        void LoadX(OpCodeData data)
        {

        }
    }
}

