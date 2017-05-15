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
	}
}
