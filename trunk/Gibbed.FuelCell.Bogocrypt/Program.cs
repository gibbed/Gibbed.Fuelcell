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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using NDesk.Options;

namespace Gibbed.FuelCell.Bogocrypt
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private static bool IsKnownFormat(byte[] buffer, int length)
        {
            // 7z archive
            if (length >= 6 &&
                buffer[0] == '7' && buffer[1] == 'z' &&
                buffer[2] == 0xBC && buffer[3] == 0xAF &&
                buffer[4] == 0x27 && buffer[5] == 0x1C)
            {
                return true;
            }

            return false;
        }

        public static void Main(string[] args)
        {
            var showHelp = false;
            var force = false;

            var options = new OptionSet()
            {
                {
                    "f|force",
                    "force crypt even if data is unidentified",
                    v => force = v != null
                    },
                {
                    "h|help",
                    "show this message and exit",
                    v => showHelp = v != null
                    },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 ||
                showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_fil [output_fil]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPath = extras[0];
            var outputPath = extras.Count > 1 ? extras[1] : inputPath;

            byte[] temp;
            bool encrypted = false;
            using (var input = File.OpenRead(inputPath))
            {
                var header = new byte[16];
                var length = input.Read(header, 0, header.Length);

                var known = IsKnownFormat(header, length);
                if (known == false)
                {
                    BadCryptBlues.Decrypt(header, 0, length);
                    known = IsKnownFormat(header, length);
                    if (known == true)
                    {
                        encrypted = true;
                    }
                }

                if (known == false &&
                    force == false)
                {
                    Console.WriteLine("Unidentified data format, ignoring...");
                    return;
                }

                input.Seek(0, SeekOrigin.Begin);
                temp = input.ReadBytes((uint)input.Length);
            }

            if (encrypted == false)
            {
                BadCryptBlues.Encrypt(temp, 0, temp.Length);
            }
            else
            {
                BadCryptBlues.Decrypt(temp, 0, temp.Length);
            }

            using (var output = File.Create(outputPath))
            {
                output.WriteBytes(temp);
            }
        }
    }
}
