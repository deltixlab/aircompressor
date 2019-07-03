/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using RTMath.ZStd;

namespace Zstandard.Tests
{
	[TestFixture]
	public class TestDecompress
	{
		public static byte[] AlphabetDataPrepare()
		{
			string alpahabet = "abcdefghijklmnopqrstuvwxyz";
			StringBuilder sb = new StringBuilder();

			int charNum = alpahabet.Length;
			alpahabet = alpahabet + alpahabet;
			XorShift128Plus rand = new XorShift128Plus(42, 24);
			for (int n = 0; n < 256; ++n)
			{
				int i = (int)(rand.Next() % (ulong)charNum);
				int l = (int)(rand.Next() % (ulong)charNum);
				sb = sb.Append(alpahabet, i, l);
			}

			var ret = Encoding.ASCII.GetBytes(sb.ToString());

			using (var writer = new BinaryWriter(File.Open("alphabetData.dat", FileMode.Create)))
			{
				writer.Write(ret);
			}

			return ret;
		}

		[Test]
		public static void RandomDecompress()
		{
			var refData = AlphabetDataPrepare();

			byte[] compressedData = {
				0x28, 0xB5, 0x2F, 0xFD, 0x64, 0x51, 0x0C, 0xB5, 0x0E, 0x00, 0x64, 0x03, 0x6F, 0x70, 0x71, 0x72,
				0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
				0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F, 0x6E, 0x6F, 0x6E, 0x6F, 0x7A, 0x61, 0x70, 0x6E, 0x6C,
				0x6A, 0x61, 0x70, 0x66, 0x75, 0x66, 0x6F, 0x70, 0x72, 0x63, 0x64, 0x65, 0x66, 0x6D, 0x6E, 0x64,
				0x65, 0x74, 0x80, 0xD6, 0xA8, 0xA1, 0xD3, 0xFD, 0x1F, 0x00, 0x40, 0x40, 0x18, 0x73, 0xC6, 0x02,
				0x32, 0x20, 0x14, 0x8A, 0xC3, 0x38, 0x10, 0xA2, 0x30, 0x0A, 0xE3, 0x30, 0x8C, 0xE3, 0x20, 0x94,
				0x6C, 0x01, 0x14, 0x85, 0x01, 0x5D, 0x96, 0x4C, 0xFC, 0x55, 0x5A, 0x90, 0x07, 0x9B, 0xE5, 0xB6,
				0xDC, 0x0B, 0x14, 0xE3, 0x18, 0x13, 0x10, 0x58, 0x65, 0xA3, 0xA1, 0xA2, 0x9E, 0x32, 0xE1, 0x95,
				0x9F, 0x52, 0xE5, 0xEB, 0x41, 0x54, 0x67, 0x9E, 0x36, 0x41, 0x1B, 0xEE, 0x85, 0xA0, 0xEB, 0x23,
				0xA6, 0x3C, 0x85, 0xB1, 0x4E, 0x19, 0x6D, 0x2F, 0x1E, 0x0B, 0x1A, 0xE7, 0xF5, 0x07, 0x27, 0x6D,
				0x7E, 0x92, 0x45, 0x33, 0x40, 0x00, 0xBA, 0x81, 0x6D, 0xC6, 0x0C, 0xE0, 0x1D, 0xB7, 0xEB, 0x3C,
				0x0D, 0xAB, 0x91, 0x47, 0x19, 0x36, 0x00, 0xCE, 0x58, 0xA3, 0xD4, 0xCE, 0x21, 0x53, 0xC0, 0xED,
				0x6D, 0x01, 0x17, 0xC6, 0xFA, 0x79, 0x88, 0xE6, 0x29, 0xED, 0xB8, 0x37, 0x1B, 0x34, 0x6D, 0x89,
				0x26, 0xC2, 0x88, 0x2E, 0x61, 0xBF, 0xDE, 0xBC, 0x75, 0x49, 0x7C, 0x4A, 0x6E, 0xA1, 0x20, 0x5F,
				0x00, 0xA1, 0x84, 0xCC, 0xC3, 0x7E, 0x43, 0x4B, 0x1B, 0xC0, 0x1B, 0x16, 0x5A, 0x37, 0x16, 0xCB,
				0xB2, 0x10, 0x45, 0xB9, 0x62, 0x4E, 0x93, 0x51, 0xD9, 0xA4, 0x69, 0xCF, 0x90, 0x93, 0xA4, 0x33,
				0x53, 0x32, 0xF7, 0x5F, 0xDD, 0x8D, 0xC3, 0xF1, 0x8E, 0xF2, 0x75, 0x00, 0x39, 0x1D, 0xE3, 0x9E,
				0xC6, 0xD3, 0xE7, 0x94, 0x3F, 0x3D, 0xFC, 0xBE, 0xAA, 0x19, 0x16, 0x2C, 0x66, 0xEF, 0x48, 0x78,
				0x0C, 0xF4, 0xC9, 0xF4, 0xE9, 0xBE, 0x6E, 0x63, 0x00, 0x14, 0x81, 0x1B, 0x4E, 0xD4, 0x8B, 0xCE,
				0xFA, 0xF4, 0x33, 0xCE, 0xC4, 0xE3, 0x29, 0xFC, 0x75, 0xD2, 0x1F, 0x1F, 0x4A, 0xA0, 0xA3, 0xFE,
				0x05, 0xB4, 0x3B, 0xC9, 0x8D, 0x40, 0xCE, 0xE4, 0x57, 0xEE, 0x7E, 0xE8, 0x6B, 0x42, 0x1E, 0x6E,
				0x92, 0x61, 0x58, 0x25, 0xDB, 0x12, 0x0A, 0x71, 0x29, 0x08, 0xCE, 0x40, 0x79, 0x73, 0xD5, 0x1D,
				0x8E, 0x73, 0x85, 0x1D, 0x9E, 0x87, 0x22, 0xDD, 0xA6, 0x91, 0x22, 0xC2, 0x1C, 0xE6, 0x51, 0xC6,
				0x6D, 0xE7, 0x1B, 0xE1, 0x02, 0xB3, 0x16, 0x0E, 0xDE, 0x72, 0x45, 0x1B, 0x1C, 0x73, 0x2A, 0x41,
				0x1C, 0xF1, 0x1B, 0xD7, 0x4E, 0xB7, 0xE9, 0xB7, 0xE6, 0x60, 0xEB, 0x59, 0xEA, 0x74, 0x20, 0xFA,
				0x1E, 0xEB, 0x1C, 0xDA, 0xFA, 0x9E, 0x4E, 0xCB, 0xCD, 0x83, 0x43, 0x98, 0x87, 0x25, 0xA8, 0x8D,
				0x3F, 0xB1, 0xBE, 0x36, 0x23, 0x93, 0x11, 0x14, 0x4C, 0x40, 0x2B, 0xFD, 0xD1, 0x5B, 0x66, 0xBB,
				0x7E, 0xF0, 0xE4, 0x36, 0x64, 0x54, 0x40, 0xCE, 0x4B, 0x04, 0x7D, 0x08, 0x8D, 0xBC, 0x97, 0xAA,
				0xED, 0xB4, 0x2D, 0xF6, 0xE9, 0x5F, 0xFA, 0x10, 0x78, 0x8A, 0x68, 0xDB, 0x82, 0x9A, 0x0C, 0x06,
				0xF9, 0x34, 0x9A, 0xF5, 0x30, 0x47, 0xF3, 0x54, 0x09, 0x8C, 0x53, 0xE0, 0xA9, 0x94, 0x33, 0xFF,
				0x33, 0xB2, 0xA0, 0xE9};

			int decompressSize = (int)ZStdDecompress.GetDecompressedSize(compressedData);
			Assert.AreEqual(refData.Length, decompressSize);

			var decompressedData = new byte[decompressSize];
			ZStdDecompress.Decompress(decompressedData, compressedData);

			for (int i = 0; i < decompressSize; ++i)
				Assert.AreEqual(refData[i], decompressedData[i]);
		}
	}
}