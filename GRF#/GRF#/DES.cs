using System;
using System.Security.Cryptography;
using System.IO;

namespace GRFSharp
{
    public static class DES
    {
        #region local variables
        static byte[] ip_table = new byte[64] {
			58, 50, 42, 34, 26, 18, 10,  2, 60, 52, 44, 36, 28, 20, 12,  4,
			62, 54, 46, 38, 30, 22, 14,  6, 64, 56, 48, 40, 32, 24, 16,  8,
			57, 49, 41, 33, 25, 17,  9,  1, 59, 51, 43, 35, 27, 19, 11,  3,
			61, 53, 45, 37, 29, 21, 13,  5, 63, 55, 47, 39, 31, 23, 15,  7
		};

        static byte[] fp_table = new byte[64] {
			40,  8, 48, 16, 56, 24, 64, 32, 39,  7, 47, 15, 55, 23, 63, 31,
			38,  6, 46, 14, 54, 22, 62, 30, 37,  5, 45, 13, 53, 21, 61, 29,
			36,  4, 44, 12, 52, 20, 60, 28, 35,  3, 43, 11, 51, 19, 59, 27,
			34,  2, 42, 10, 50, 18, 58, 26, 33,  1, 41,  9, 49, 17, 57, 25
		};

        static byte[] tp_table = new byte[32]{
			16, 7, 20, 21, 29, 12, 28, 17,  1, 15, 23, 26,  5, 18, 31, 10,
		    2,  8, 24, 14, 32, 27,  3,  9, 19, 13, 30,  6, 22, 11,  4, 25
		};

        static byte[] expand_table = new byte[48] {
            32,  1,  2,  3,  4,  5,
             4,  5,  6,  7,  8,  9,
             8,  9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32,  1,
        };

        static byte[][] s_table = new byte[4][] {
			new byte[64] {
				0xef, 0x03, 0x41, 0xfd, 0xd8, 0x74, 0x1e, 0x47,  0x26, 0xef, 0xfb, 0x22, 0xb3, 0xd8, 0x84, 0x1e,
				0x39, 0xac, 0xa7, 0x60, 0x62, 0xc1, 0xcd, 0xba,  0x5c, 0x96, 0x90, 0x59, 0x05, 0x3b, 0x7a, 0x85,
				0x40, 0xfd, 0x1e, 0xc8, 0xe7, 0x8a, 0x8b, 0x21,  0xda, 0x43, 0x64, 0x9f, 0x2d, 0x14, 0xb1, 0x72,
				0xf5, 0x5b, 0xc8, 0xb6, 0x9c, 0x37, 0x76, 0xec,  0x39, 0xa0, 0xa3, 0x05, 0x52, 0x6e, 0x0f, 0xd9,
			}, new byte[64] {
				0xa7, 0xdd, 0x0d, 0x78, 0x9e, 0x0b, 0xe3, 0x95,  0x60, 0x36, 0x36, 0x4f, 0xf9, 0x60, 0x5a, 0xa3,
				0x11, 0x24, 0xd2, 0x87, 0xc8, 0x52, 0x75, 0xec,  0xbb, 0xc1, 0x4c, 0xba, 0x24, 0xfe, 0x8f, 0x19,
				0xda, 0x13, 0x66, 0xaf, 0x49, 0xd0, 0x90, 0x06,  0x8c, 0x6a, 0xfb, 0x91, 0x37, 0x8d, 0x0d, 0x78,
				0xbf, 0x49, 0x11, 0xf4, 0x23, 0xe5, 0xce, 0x3b,  0x55, 0xbc, 0xa2, 0x57, 0xe8, 0x22, 0x74, 0xce,
			}, new byte[64] {
				0x2c, 0xea, 0xc1, 0xbf, 0x4a, 0x24, 0x1f, 0xc2,  0x79, 0x47, 0xa2, 0x7c, 0xb6, 0xd9, 0x68, 0x15,
				0x80, 0x56, 0x5d, 0x01, 0x33, 0xfd, 0xf4, 0xae,  0xde, 0x30, 0x07, 0x9b, 0xe5, 0x83, 0x9b, 0x68,
				0x49, 0xb4, 0x2e, 0x83, 0x1f, 0xc2, 0xb5, 0x7c,  0xa2, 0x19, 0xd8, 0xe5, 0x7c, 0x2f, 0x83, 0xda,
				0xf7, 0x6b, 0x90, 0xfe, 0xc4, 0x01, 0x5a, 0x97,  0x61, 0xa6, 0x3d, 0x40, 0x0b, 0x58, 0xe6, 0x3d,
			}, new byte[64] {
				0x4d, 0xd1, 0xb2, 0x0f, 0x28, 0xbd, 0xe4, 0x78,  0xf6, 0x4a, 0x0f, 0x93, 0x8b, 0x17, 0xd1, 0xa4,
				0x3a, 0xec, 0xc9, 0x35, 0x93, 0x56, 0x7e, 0xcb,  0x55, 0x20, 0xa0, 0xfe, 0x6c, 0x89, 0x17, 0x62,
				0x17, 0x62, 0x4b, 0xb1, 0xb4, 0xde, 0xd1, 0x87,  0xc9, 0x14, 0x3c, 0x4a, 0x7e, 0xa8, 0xe2, 0x7d,
				0xa0, 0x9f, 0xf6, 0x5c, 0x6a, 0x09, 0x8d, 0xf0,  0x0f, 0xe3, 0x53, 0x25, 0x95, 0x36, 0x28, 0xcb,
			}
		};

        static byte[] mask = new byte[8] 
        {
            0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01
        };
        #endregion

        #region private functions
        private static byte DesSubs(byte inb)
        {
            byte outb;

            switch (inb)
            {
                case 0x00: outb = 0x2B; break;
                case 0x2B: outb = 0x00; break;
                case 0x6C: outb = 0x80; break;
                case 0x01: outb = 0x68; break;
                case 0x68: outb = 0x01; break;
                case 0x48: outb = 0x77; break;
                case 0x60: outb = 0xFF; break;
                case 0x77: outb = 0x48; break;
                case 0xB9: outb = 0xC0; break;
                case 0xC0: outb = 0xB9; break;
                case 0xFE: outb = 0xEB; break;
                case 0xEB: outb = 0xFE; break;
                case 0x80: outb = 0x6C; break;
                case 0xFF: outb = 0x60; break;
                default: outb = inb; break;
            }

            return outb;
        }

        private static void DesShuffle(byte[] src, int start)
        {
            byte[] tmp = new byte[8];

            tmp[0] = src[start + 3];
            tmp[1] = src[start + 4];
            tmp[2] = src[start + 6];
            tmp[3] = src[start + 0];
            tmp[4] = src[start + 1];
            tmp[5] = src[start + 2];
            tmp[6] = src[start + 5];
            tmp[7] = DesSubs(src[start + 7]);

            Array.Copy(tmp, 0, src, start, 8);
        }

        private static void DecodeHeader(byte[] src)
        {
            int nblocks = src.Length / 8;

            for (int i = 0; i < 20 && i < nblocks; i++)
            {
                DesDecodeBlock(src, i * 8);
            }
        }

        private static void DecodeFull(byte[] src, int cycle)
        {
            int nblocks = src.Length / 8;
            int dcycle, scycle;
            int i, j;

            for (i = 0; i < 20 && i < nblocks; i++)
                DesDecodeBlock(src, i * 8);

            dcycle = cycle;
            scycle = 7;

            j = -1;
            for (i = 20; i < nblocks; i++)
            {
                if ((i % dcycle) == 0)
                {
                    DesDecodeBlock(src, i * 8);
                    continue;
                }

                j++;

                if ((j % scycle) == 0 && j != 0)
                {
                    DesShuffle(src, i * 8);
                    continue;
                }
            }
        }

        private static void RoundFunction(byte[] src)
        {
            byte[] block = new byte[8];
            Array.Copy(src, 0, block, 0, 8);

            E(block);
            SBOX(block);
            TP(block);

            src[0] ^= block[4];
            src[1] ^= block[5];
            src[2] ^= block[6];
            src[3] ^= block[7];
        }

        private static void DesDecodeBlock(byte[] src, int i)
        {
            byte[] block = new byte[8];
            Array.Copy(src, i, block, 0, 8);

            IP(block);
            RoundFunction(block);
            FP(block);

            Array.Copy(block, 0, src, i, 8);
        }

        private static void FP(byte[] src)
        {
            byte[] block = new byte[8];

            for (int i = 0; i < fp_table.Length; i++)
            {
                byte j = (byte)(fp_table[i] - 1);

                if ((src[(j >> 3) & 7] & mask[j & 7]) != 0)
                    block[(i >> 3) & 7] |= mask[i & 7];
            }

            Array.Copy(block, 0, src, 0, 8);
        }

        private static void IP(byte[] src)
        {
            byte[] block = new byte[8];

            for (int i = 0; i < ip_table.Length; i++)
            {
                byte j = (byte)(ip_table[i] - 1);

                if ((src[(j >> 3) & 7] & mask[j & 7]) != 0)
                    block[(i >> 3) & 7] |= mask[i & 7];
            }

            Array.Copy(block, 0, src, 0, 8);
        }

        private static void E(byte[] src)
        {
            byte[] block = new byte[8];

            block[0] = (byte)(((src[7] << 5) | (src[4] >> 3)) & 0x3f);    // ..0 vutsr
            block[1] = (byte)(((src[4] << 1) | (src[5] >> 7)) & 0x3f);    // ..srqpo n
            block[2] = (byte)(((src[4] << 5) | (src[5] >> 3)) & 0x3f);    // ..o nmlkj
            block[3] = (byte)(((src[5] << 1) | (src[6] >> 7)) & 0x3f);    // ..kjihg f
            block[4] = (byte)(((src[5] << 5) | (src[6] >> 3)) & 0x3f);    // ..g fedcb
            block[5] = (byte)(((src[6] << 1) | (src[7] >> 7)) & 0x3f);    // ..cba98 7
            block[6] = (byte)(((src[6] << 5) | (src[7] >> 3)) & 0x3f);    // ..8 76543
            block[7] = (byte)(((src[7] << 1) | (src[4] >> 7)) & 0x3f);    // ..43210 v

            Array.Copy(block, 0, src, 0, 8);
        }

        private static void SBOX(byte[] src)
        {
            byte[] block = new byte[8];

            for (int i = 0; i < s_table.Length; i++)
            {
                block[i] = (byte)((s_table[i][src[i * 2 + 0]] & 0xf0)
                            | (s_table[i][src[i * 2 + 1]] & 0x0f));
            }

            Array.Copy(block, 0, src, 0, 8);
        }

        private static void TP(byte[] src)
        {
            byte[] block = new byte[8];

            for (int i = 0; i < tp_table.Length; i++)
            {
                byte j = (byte)(tp_table[i] - 1);

                if ((src[(j >> 3) + 0] & mask[j & 7]) != 0)
                    block[(i >> 3) + 4] |= mask[i & 7];
            }

            Array.Copy(block, 0, src, 0, 8);
        }
        #endregion

        public static void GrfDecode(byte[] src, int type, int len)
        {
            if ((type & 2) != 0)
            {
                int digits;
                int cycle;

                digits = 1;
                for (int i = 10; i <= len; i *= 10)
                    ++digits;

                cycle = (digits < 3) ? 1
                      : (digits < 5) ? digits + 1
                      : (digits < 7) ? digits + 9
                      : digits + 15;

                DecodeFull(src, cycle);
            }
            else if ((type & 4) != 0)
            {
                DecodeHeader(src);
            }
        }
    }
}

