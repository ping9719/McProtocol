using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace McProtocol
{
    [StructLayout(LayoutKind.Explicit)]
    public class Union
    {
        [FieldOffset(0)]
        public float REAL;
        [FieldOffset(0)]
        public short INT;
        [FieldOffset(0)]
        public uint UINT;
        [FieldOffset(0)]
        public int DINT;
        [FieldOffset(0)]
        public uint UDINT;
        [FieldOffset(0)]
        public char letter;
        [FieldOffset(0)]
        public byte bite;
        [FieldOffset(0)]
        public byte a;
        [FieldOffset(1)]
        public byte b;
        [FieldOffset(2)]
        public byte c;
        [FieldOffset(3)]
        public byte d;
    }
}
