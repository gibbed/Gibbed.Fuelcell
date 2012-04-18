/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;

namespace Gibbed.FuelCell.Bogocrypt
{
    internal static class BadCryptBlues
    {
        public struct State
        {
            public uint Seed;
            public int Index;
        }

        private static readonly byte[] _Key = new byte[]
        {
            0x21, 0x79, 0x30, 0x30, 0x42, 0x34, 0x73, 0x74, // !y00B4st412D!
            0x34, 0x31, 0x32, 0x44, 0x21,
            0x21
        };

        public static State Encrypt(byte[] buffer, int offset, int length)
        {
            return Encrypt(buffer, offset, length, new State());
        }

        public static State Encrypt(byte[] buffer, int offset, int length, State state)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            int end = offset + length;
            int keyLength = _Key.Length - 1;

            for (int i = offset; i < end; i++)
            {
                var a = _Key[state.Index + 0];
                var b = _Key[state.Index + 1];

                state.Seed += a;
                state.Seed *= b;

                buffer[i] ^= (byte)state.Seed;
                buffer[i] -= b;
                buffer[i] += a;

                state.Index = (state.Index + 1) % keyLength;
            }

            return state;
        }

        public static State Decrypt(byte[] buffer, int offset, int length)
        {
            return Decrypt(buffer, offset, length, new State());
        }

        public static State Decrypt(byte[] buffer, int offset, int length, State state)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            int end = offset + length;
            int keyLength = _Key.Length - 1;

            for (int i = offset; i < end; i++)
            {
                var a = _Key[state.Index + 0];
                var b = _Key[state.Index + 1];

                state.Seed += a;
                state.Seed *= b;

                buffer[i] -= a;
                buffer[i] += b;
                buffer[i] ^= (byte)state.Seed;

                state.Index = (state.Index + 1) % keyLength;
            }

            return state;
        }
    }
}
