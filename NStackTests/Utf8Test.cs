﻿﻿// 
// Utf8Tescs: UTF8 tests
//
// Based on the Go UTF8 tests
// 
// C# ification by Miguel de Icaza
//
using NUnit.Framework;
using System;
using NStack;

namespace NStackTests
{
	[TestFixture]
	public class Utf8Test
	{

		struct RuneMap
		{
			public uint Rune;
			public byte [] Bytes;
			public RuneMap (uint rune, params byte [] bytes)
			{
				Rune = rune;
				Bytes = bytes;
			}
		};

		ustring [] testStrings = new ustring [] {
			ustring.Empty,
			new ustring ("abcd"),
			new ustring (0xE2, 0x98, 0xBA, 0xE2, 0x98, 0xBB, 0xE2, 0x98, 0xB9),
			new ustring (
				0xE6, 0x97, 0xA5, 0x61, 0xE6, 0x9C, 0xAC, 0x62, 0xE8, 0xAA, 0x9E, 0xC3,
				0xA7, 0xE6, 0x97, 0xA5, 0xC3, 0xB0, 0xE6, 0x9C, 0xAC, 0xC3, 0x8A, 0xE8,
				0xAA, 0x9E, 0xC3, 0xBE, 0xE6, 0x97, 0xA5, 0xC2, 0xA5, 0xE6, 0x9C, 0xAC,
				0xC2, 0xBC, 0xE8, 0xAA, 0x9E, 0x69, 0xE6, 0x97, 0xA5, 0xC2, 0xA9),
			new ustring (
				0xE6, 0x97, 0xA5, 0x61, 0xE6, 0x9C, 0xAC, 0x62, 0xE8, 0xAA, 0x9E, 0xC3,
				0xA7, 0xE6, 0x97, 0xA5, 0xC3, 0xB0, 0xE6, 0x9C, 0xAC, 0xC3, 0x8A, 0xE8,
				0xAA, 0x9E, 0xC3, 0xBE, 0xE6, 0x97, 0xA5, 0xC2, 0xA5, 0xE6, 0x9C, 0xAC,
				0xC2, 0xBC, 0xE8, 0xAA, 0x9E, 0x69, 0xE6, 0x97, 0xA5, 0xC2, 0xA9, 0xE6,
				0x97, 0xA5, 0x61, 0xE6, 0x9C, 0xAC, 0x62, 0xE8, 0xAA, 0x9E, 0xC3, 0xA7,
				0xE6, 0x97, 0xA5, 0xC3, 0xB0, 0xE6, 0x9C, 0xAC, 0xC3, 0x8A, 0xE8, 0xAA,
				0x9E, 0xC3, 0xBE, 0xE6, 0x97, 0xA5, 0xC2, 0xA5, 0xE6, 0x9C, 0xAC, 0xC2,
				0xBC, 0xE8, 0xAA, 0x9E, 0x69, 0xE6, 0x97, 0xA5, 0xC2, 0xA9, 0xE6, 0x97,
				0xA5, 0x61, 0xE6, 0x9C, 0xAC, 0x62, 0xE8, 0xAA, 0x9E, 0xC3, 0xA7, 0xE6,
				0x97, 0xA5, 0xC3, 0xB0, 0xE6, 0x9C, 0xAC, 0xC3, 0x8A, 0xE8, 0xAA, 0x9E,
				0xC3, 0xBE, 0xE6, 0x97, 0xA5, 0xC2, 0xA5, 0xE6, 0x9C, 0xAC, 0xC2, 0xBC,
				0xE8, 0xAA, 0x9E, 0x69, 0xE6, 0x97, 0xA5, 0xC2, 0xA9),
			new ustring (0x80, 0x80, 0x80, 0x80)
		};

		RuneMap [] runeMap = new RuneMap [] {
			new RuneMap (0x0000,   0x00),
			new RuneMap (0x0001,   0x01),
			new RuneMap (0x007e,   0x7e),
			new RuneMap (0x007f,   0x7f),
			new RuneMap (0x0080,   0xc2, 0x80),
			new RuneMap (0x0081,   0xc2, 0x81),
			new RuneMap (0x00bf,   0xc2, 0xbf),
			new RuneMap (0x00c0,   0xc3, 0x80),
			new RuneMap (0x00c1,   0xc3, 0x81),
			new RuneMap (0x00c8,   0xc3, 0x88),
			new RuneMap (0x00d0,   0xc3, 0x90),
			new RuneMap (0x00e0,   0xc3, 0xa0),
			new RuneMap (0x00f0,   0xc3, 0xb0),
			new RuneMap (0x00f8,   0xc3, 0xb8),
			new RuneMap (0x00ff,   0xc3, 0xbf),
			new RuneMap (0x0100,   0xc4, 0x80),
			new RuneMap (0x07ff,   0xdf, 0xbf),
			new RuneMap (0x0400,   0xd0, 0x80),
			new RuneMap (0x0800,   0xe0, 0xa0, 0x80),
			new RuneMap (0x0801,   0xe0, 0xa0, 0x81),
			new RuneMap (0x1000,   0xe1, 0x80, 0x80),
			new RuneMap (0xd000,   0xed, 0x80, 0x80),
			new RuneMap (0xd7ff,   0xed, 0x9f, 0xbf), // last code point before surrogate hal),
			new RuneMap (0xe000,   0xee, 0x80, 0x80), // first code point after surrogate hal),
			new RuneMap (0xfffe,   0xef, 0xbf, 0xbe),
			new RuneMap (0xffff,   0xef, 0xbf, 0xbf),
			new RuneMap (0x10000,  0xf0, 0x90, 0x80, 0x80),
			new RuneMap (0x10001,  0xf0, 0x90, 0x80, 0x81),
			new RuneMap (0x40000,  0xf1, 0x80, 0x80, 0x80),
			new RuneMap (0x10fffe, 0xf4, 0x8f, 0xbf, 0xbe),
			new RuneMap (0x10ffff, 0xf4, 0x8f, 0xbf, 0xbf),
			new RuneMap (0xFFFD,   0xef, 0xbf, 0xbd)
		};

		RuneMap [] surrogateMap = new RuneMap [] {
			new RuneMap (0xd800, 0xed, 0xa0, 0x80),
			new RuneMap (0xdfff, 0xed, 0xbf, 0xbf),
		};

		[Test]
		public void TestFullRune ()
		{
			foreach (var rm in runeMap) {
				Assert.IsTrue (Utf8.FullRune (rm.Bytes), "Error with FullRune on ({0})", rm.Bytes);
				Assert.IsTrue (Utf8.FullRune (new ustring (rm.Bytes)), "Error with FullRune(ustring) on {0}", rm.Bytes);

				var brokenSequence = new byte [rm.Bytes.Length - 1];
				Array.Copy (rm.Bytes, brokenSequence, rm.Bytes.Length - 1);
				Assert.IsFalse (Utf8.FullRune (brokenSequence), "Expected false for a partial sequence");
				Assert.IsFalse (Utf8.FullRune (new ustring (brokenSequence)), "Expected false for a partial sequence");
			}
			Assert.IsTrue (Utf8.FullRune (new ustring (0xc0)));
			Assert.IsTrue (Utf8.FullRune (new ustring (0xc0)));
		}

		[Test]
		public void TestEncodeRune ()
		{
			var result = new byte [10];
			Utf8.EncodeRune (0x10000, result);
			foreach (var rm in runeMap) {
				var n = Utf8.EncodeRune (rm.Rune, result);
				for (int i = 0; i < rm.Bytes.Length; i++)
					Assert.AreEqual (rm.Bytes [i], result [i], "Failure with rune {0} (0x{0:x}) at index {1}", rm.Rune, i);
			}
		}

		[Test]
		public void TestDecodeRune ()
		{
			foreach (var rm in runeMap) {
				var buffer = rm.Bytes;

				(var rune, var size) = Utf8.DecodeRune (buffer);
				Assert.AreEqual (rune, rm.Rune, "Decoding rune 0x{0:x} got 0x{1:x}", rm.Rune, rune);
				Assert.AreEqual (rm.Bytes.Length, size, "Decoding rune size for 0x{0:x} got 0x{1:x}", rm.Bytes.Length, size);
				(rune, size) = Utf8.DecodeRune (new ustring (rm.Bytes));
				Assert.AreEqual (rune, rm.Rune, "Decoding rune from string 0x{0:x} got 0x{1:x}", rm.Rune, rune);
				Assert.AreEqual (rm.Bytes.Length, size, "Decoding rune size for 0x{0:x} got 0x{1:x}", rm.Bytes.Length, size);

				// Add trailing zero;
				var buffer2 = new byte [rm.Bytes.Length + 1];
				Array.Copy (buffer, buffer2, rm.Bytes.Length);
				(rune, size) = Utf8.DecodeRune (buffer2);
				Assert.AreEqual (rune, rm.Rune, "Decoding rune 0x{0:x} got 0x{1:x}", rm.Rune, rune);
				Assert.AreEqual (rm.Bytes.Length, size, "Decoding rune size for 0x{0:x} got 0x{1:x}", rm.Bytes.Length, size);
				(rune, size) = Utf8.DecodeRune (new ustring (buffer2));
				Assert.AreEqual (rune, rm.Rune, "Decoding rune from string 0x{0:x} got 0x{1:x}", rm.Rune, rune);
				Assert.AreEqual (rm.Bytes.Length, size, "Decoding rune size for 0x{0:x} got 0x{1:x}", rm.Bytes.Length, size);

				// Tru removing one byte
				var wantsize = 1;
				if (wantsize >= rm.Bytes.Length)
					wantsize = 0;
				var buffer3 = new byte [rm.Bytes.Length - 1];
				Array.Copy (buffer, buffer3, buffer3.Length);
				(rune, size) = Utf8.DecodeRune (buffer3);
				Assert.AreEqual (rune, Utf8.RuneError, "Expected an error for short buffer in 0x{0:0}", rm.Rune);
				Assert.AreEqual (wantsize, size, "Expected {0}={1} for 0x{2}", wantsize, size, rm.Rune);

				// Make sure bad sequences fail
				var buffer4 = (byte []) rm.Bytes.Clone ();
				if (buffer4.Length == 1)
					buffer4 [0] = 0x80;
				else
					buffer4 [buffer4.Length - 1] = 0x7f;
				(rune, size) = Utf8.DecodeRune (buffer4);
				Assert.AreEqual (rune, Utf8.RuneError, "Expected malformed buffer to return an error for rune 0x{0:x}", rm.Rune);
				Assert.AreEqual (1, size, "Expected malformed buffer to return size 1");

				(rune, size) = Utf8.DecodeRune (new ustring (buffer4));
				Assert.AreEqual (rune, Utf8.RuneError, "Expected malformed buffer to return an error");
				Assert.AreEqual (1, size, "Expected malformed buffer to return size 1");
			}
		}

		[Test]
		public void TestDecodeSurrogateRune ()
		{
			foreach (var rm in surrogateMap) {
				(var rune, var size) = Utf8.DecodeRune (rm.Bytes);
				Assert.AreEqual (rune, Utf8.RuneError);
				Assert.AreEqual (1, size);

				(rune, size) = Utf8.DecodeRune (new ustring (rm.Bytes));
				Assert.AreEqual (rune, Utf8.RuneError);
				Assert.AreEqual (1, size);
			}
		}

		public byte [] Subset (byte [] source, int start, int end)
		{
			if (end == -1)
				end = source.Length;
			var n = end - start;
			var result = new byte [n];
			Array.Copy (source, start, result, 0, n);
			return result;
		}

		public void TestSequence (ustring s)
		{
			var index = new(int idx, uint rune) [s.Length];
			var si = 0;
			var j = 0;
			foreach ((var i, var rune) in s.Range ()) {
				Assert.AreEqual (i, si, "Sequence mismatch at index {0} = {1}", i, si);
				index [j] = (i, rune);
				j++;
				(var r1, var size1) = Utf8.DecodeRune (Subset (s.Bytes, i, -1));
				Assert.AreEqual (rune, r1, "DecodeRune 0x{0:x} = want 0x{1:x} with {2}", r1, rune, s);
				(var r2, var size2) = Utf8.DecodeRune (new ustring (Subset (s.Bytes, i, -1)));
				Assert.AreEqual (size1, size2);
				si += size1;
			}
			j--;

			for (si = s.Length; si > 0;) {
				(var r1, var size1) = Utf8.DecodeLastRune (Subset (s.Bytes, 0, si));
				(var r2, var size2) = Utf8.DecodeLastRune (new ustring (Subset (s.Bytes, 0, si)));
				Assert.AreEqual (size1, size2);
				Assert.AreEqual (r1, index [j].rune);
				Assert.AreEqual (r2, index [j].rune);
				si -= size1;
				Assert.AreEqual (si, index [j].idx);
				j--;
			}
			Assert.AreEqual (si, 0, "DecodeLastRune finished at {0} not 0", si);
		}

		[Test]
		public void TestSequencing ()
		{
			TestSequence (new ustring ("abcd"));
			foreach (var ts in testStrings) {
				foreach (var m in runeMap) {
					var variations = new ustring [] {
						ts + new ustring (m.Bytes),
						new ustring (m.Bytes) + ts,
						ts + new ustring (m.Bytes) + ts
					};
					foreach (var x in variations)
						TestSequence (x);
				}
			}
		}

		ustring [] invalidSequenceTests = new ustring [] {
		        new ustring (0xed, 0xa0, 0x80, 0x80),// surrogate min
			new ustring (0xed, 0xbf, 0xbf, 0x80),// surrogate max

			// xx
			new ustring (0x91, 0x80, 0x80, 0x80),
			        
			// s1
			new ustring (0xC2, 0x7F, 0x80, 0x80),
			new ustring (0xC2, 0xC0, 0x80, 0x80),
			new ustring (0xDF, 0x7F, 0x80, 0x80),
			new ustring (0xDF, 0xC0, 0x80, 0x80),
			
			// s2
			new ustring (0xE0, 0x9F, 0xBF, 0x80),
			new ustring (0xE0, 0xA0, 0x7F, 0x80),
			new ustring (0xE0, 0xBF, 0xC0, 0x80),
			new ustring (0xE0, 0xC0, 0x80, 0x80),
			        
			// s3
			new ustring (0xE1, 0x7F, 0xBF, 0x80),
			new ustring (0xE1, 0x80, 0x7F, 0x80),
			new ustring (0xE1, 0xBF, 0xC0, 0x80),
			new ustring (0xE1, 0xC0, 0x80, 0x80),
			                
			//s4
			new ustring (0xED, 0x7F, 0xBF, 0x80),
			new ustring (0xED, 0x80, 0x7F, 0x80),
			new ustring (0xED, 0x9F, 0xC0, 0x80),
			new ustring (0xED, 0xA0, 0x80, 0x80),
			                
			// s5
			new ustring (0xF0, 0x8F, 0xBF, 0xBF),
			new ustring (0xF0, 0x90, 0x7F, 0xBF),
			new ustring (0xF0, 0x90, 0x80, 0x7F),
			new ustring (0xF0, 0xBF, 0xBF, 0xC0),
			new ustring (0xF0, 0xBF, 0xC0, 0x80),
			new ustring (0xF0, 0xC0, 0x80, 0x80),
			        
			// s6
			new ustring (0xF1, 0x7F, 0xBF, 0xBF),
			new ustring (0xF1, 0x80, 0x7F, 0xBF),
			new ustring (0xF1, 0x80, 0x80, 0x7F),
			new ustring (0xF1, 0xBF, 0xBF, 0xC0),
			new ustring (0xF1, 0xBF, 0xC0, 0x80),
			new ustring (0xF1, 0xC0, 0x80, 0x80),
						
			// s7
			new ustring (0xF4, 0x7F, 0xBF, 0xBF),
			new ustring (0xF4, 0x80, 0x7F, 0xBF),
			new ustring (0xF4, 0x80, 0x80, 0x7F),
			new ustring (0xF4, 0x8F, 0xBF, 0xC0),
			new ustring (0xF4, 0x8F, 0xC0, 0x80),
			new ustring (0xF4, 0x90, 0x80, 0x80),
		};

		[Test]
		public void TestDecodeInvalidSequence ()
		{
			foreach (var str in invalidSequenceTests) {
				(var r1, _) = Utf8.DecodeRune (str.Bytes);
				Assert.AreEqual (r1, Utf8.RuneError);
				(var r2, _) = Utf8.DecodeRune (new ustring (str.Bytes));
				Assert.AreEqual (r1, Utf8.RuneError);

				Assert.AreEqual (r1, r2);
			}
		}

		(ustring testString, int count) [] runeCountTests = new (ustring,int) [] {
			(new ustring ("abcd"), 4),
			(new ustring (0xE2, 0x98, 0xBA, 0xE2, 0x98, 0xBB, 0xE2, 0x98, 0xB9), 3),
			(new ustring ("1,2,3,4"), 7),
			(new ustring (0xe2, 0x00), 2),
			(new ustring (0xe2, 0x80), 2),
			(new ustring (0x61, 0xe2, 0x80), 3),
		};

		[Test]
		public void TestRuneCount ()
		{
			foreach (var t in runeCountTests) {
				Assert.AreEqual (t.count, Utf8.RuneCount (t.testString.Bytes));
			}
		}

		[Test]
		public void TestRuneLen()
		{
			(uint rune, int size)[] runeLenTests = new(uint, int)[] {
				(0, 1),
				('e', 1),
				('é', 2),
				('☺', 3),
				(Utf8.RuneError, 3),
				(Utf8.MaxRune, 4),
				(0xd800, -1),
				(0xdfff, -1),
				(Utf8.MaxRune+1, -1),
				(unchecked ((uint) -1), -1)
			};
			foreach (var test in runeLenTests)
			{
				Assert.AreEqual(test.size, Utf8.RuneLen(test.rune), "Testing size for Rune 0x{0:x}", test.rune);
			}
		}

		[Test]
		public void TestValid()
		{
			(ustring input, bool output)[] validTests = new(ustring, bool)[] {
				(new ustring (""), true),
				(new ustring ("a"), true),
				(new ustring ("abc"), true),
				(new ustring ("Ж"), true),
				(new ustring ("ЖЖ"), true),
				(new ustring ("брэд-ЛГТМ"), true),
				(new ustring (0xE2, 0x98, 0xBA, 0xE2, 0x98, 0xBB, 0xE2, 0x98, 0xB9), true),
				(new ustring (0xaa, 0xe2), false),
				(new ustring (66, 250), false),
				(new ustring (66, 250, 67), false),
				(new ustring ("a\uffDb"), true),
				(new ustring (0xf4, 0x8f, 0xbf, 0xbf),  true), // U+10FFFF
				(new ustring (0xf4, 0x90, 0x80, 0x80), false), // U+10FFFF+1 out of range
				(new ustring (0xF7, 0xBF, 0xBF, 0xBF), false),     // 0x1FFFFF; out of range
				(new ustring (0xFB, 0xBF, 0xBF, 0xBF, 0xBF), false), // 0x3FFFFFF; out of range
				(new ustring (0xc0, 0x80), false),             // U+0000 encoded in two bytes: incorrect
				(new ustring (0xed, 0xa0, 0x80), false),         // U+D800 high surrogate (sic)
				(new ustring (0xed, 0xbf, 0xbf), false),         // U+DFFF low surrogate (sic)
			};
			foreach (var test in validTests)
			{
				Assert.AreEqual(test.output, Utf8.Valid(test.input.Bytes));
				Assert.AreEqual(test.output, Utf8.Valid(test.input));
			}
		}

		[Test]
		public void ValidRuneTests()
		{
			(uint rune, bool ok)[] validRuneTest = new(uint, bool)[] {
				(0, true),
				('e', true),
				('é', true),
				('☺', true),
				(Utf8.RuneError, true),
				(Utf8.MaxRune, true),
				(0xd7ff, true),
				(0xd800, false),
				(0xdfff, false),
				(0xe000, true),
				(Utf8.MaxRune+1, false),
				(unchecked ((uint) -1),false)
			};
			foreach (var test in validRuneTest)
			{
				Assert.AreEqual(test.ok, Utf8.ValidRune(test.rune), "Testing for valid rune 0x{0:x}", test.rune);
			}

		}
	}
}