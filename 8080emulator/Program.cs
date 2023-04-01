using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace _8080emulator
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = "spaceinvaders.ch8";
            var cpu = new CPU();
            var rom = cpu.LoadROM(fileName);
            for (int i = 0; i < rom.Length; i++)
            {
                cpu.RAM[0x200 + i] = rom[i];
            }
            while (true)
            {
                cpu.Cycle();

            }
        }
    }
}