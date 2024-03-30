using System;
using System.Collections.Generic;
using System.Linq;

namespace Display.Helper.Crypto
{
    public static class Crc32Helper
    {
        private static readonly uint[] CrcTable;

        static Crc32Helper()
        {
            const uint poly = 0xedb88320u;
            CrcTable = new uint[256];
            for (uint i = 0; i < CrcTable.Length; ++i)
            {
                var crc = i;
                for (var j = 8; j > 0; --j)
                    crc = (crc & 1) == 1 ? poly ^ crc >> 1 : crc >> 1;
                CrcTable[i] = crc;
            }
        }

        public static uint Compute(IEnumerable<byte> bytes)
        {
            var crc = bytes.Aggregate(uint.MaxValue, (current, t) => CrcTable[(current ^ t) & 0xff] ^ current >> 8);
            return ~crc;
        }
        public static byte[] CalcCrc32(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            
            var newCrc32 = Compute(bytes);
            var newResult = BitConverter.GetBytes(newCrc32);

            return newResult;
        }
    }
}
