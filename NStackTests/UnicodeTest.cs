using NUnit.Framework;
using System;
using NStack;

namespace NStackTests {
	[TestFixture ()]
	public class UnicodeTest {
		uint [] testDigit = new uint [] {
			0x0030,
			0x0039,
			0x0661,
			0x06F1,
			0x07C9,
			0x0966,
			0x09EF,
			0x0A66,
			0x0AEF,
			0x0B66,
			0x0B6F,
			0x0BE6,
			0x0BEF,
			0x0C66,
			0x0CEF,
			0x0D66,
			0x0D6F,
			0x0E50,
			0x0E59,
			0x0ED0,
			0x0ED9,
			0x0F20,
			0x0F29,
			0x1040,
			0x1049,
			0x1090,
			0x1091,
			0x1099,
			0x17E0,
			0x17E9,
			0x1810,
			0x1819,
			0x1946,
			0x194F,
			0x19D0,
			0x19D9,
			0x1B50,
			0x1B59,
			0x1BB0,
			0x1BB9,
			0x1C40,
			0x1C49,
			0x1C50,
			0x1C59,
			0xA620,
			0xA629,
			0xA8D0,
			0xA8D9,
			0xA900,
			0xA909,
			0xAA50,
			0xAA59,
			0xFF10,
			0xFF19,
			0x104A1,
			0x1D7CE,
		};

		uint [] testLetter = new uint [] {
			0x0041,
			0x0061,
			0x00AA,
			0x00BA,
			0x00C8,
			0x00DB,
			0x00F9,
			0x02EC,
			0x0535,
			0x06E6,
			0x093D,
			0x0A15,
			0x0B99,
			0x0DC0,
			0x0EDD,
			0x1000,
			0x1200,
			0x1312,
			0x1401,
			0x1885,
			0x2C00,
			0xA800,
			0xF900,
			0xFA30,
			0xFFDA,
			0xFFDC,
			0x10000,
			0x10300,
			0x10400,
			0x20000,
			0x2F800,
			0x2FA1D,
		};

		[Test]
		public void TestDigit ()
		{
			foreach (var letter in testLetter)
				Assert.IsFalse (Unicode.IsDigit (letter), "Expected IsDigit to return false for the letter 0x{0:x}", letter);
			foreach (var digit in testDigit)
				Assert.IsTrue (Unicode.IsDigit (digit),"Expected IsDigit to return true for the digit 0x{0:x}", digit);
		}

		[Test]
		public void TestLatinOptimizations ()
		{
			var isp = Unicode.IsLower (0x41);
			var sss = Unicode.P.InRange (0x24);

			for (uint rune = 0; rune < Unicode.MaxLatin1; rune++) {
				Assert.AreEqual (Unicode.Digit.InRange (rune), Unicode.IsDigit (rune), "Digit, the fast path and the slow path disagree for rune 0x{0:x}", rune);

				// Control
				var iscontrol = (rune <= 0x1f || (0x7f <= rune && rune <= 0x9f));
				Assert.AreEqual (iscontrol, Unicode.IsControl (rune), "Control character, the fast and slow path disagree for rune 0x{0:x}", rune);
				Assert.AreEqual (Unicode.L.InRange (rune), Unicode.IsLetter (rune), "Letter, The fast path and the slow path disagree for rune 0x{0:x}", rune);
				Assert.AreEqual (Unicode.Upper.InRange (rune), Unicode.IsUpper (rune), "IsUpper, The fast path and the slow path disagree for rune 0x{0:x}", rune);
				Assert.AreEqual (Unicode.Lower.InRange (rune), Unicode.IsLower (rune), "IsLower, The fast path and the slow path disagree for rune 0x{0:x}", rune);
				Assert.AreEqual (Unicode.N.InRange (rune), Unicode.IsNumber (rune), "IsNumber, The fast path and the slow path disagree for rune 0x{0:x}", rune);
				Assert.AreEqual (Unicode.IsRuneInRanges (rune, Unicode.PrintRanges) || rune == ' ', Unicode.IsPrint (rune), "IsPrint, The fast path and the slow path disagree for rune 0x{0:x}", rune);
				Assert.AreEqual (Unicode.IsRuneInRanges (rune, Unicode.GraphicRanges), Unicode.IsGraphic (rune), "IsGraphic, The fast path and the slow path disagree for rune 0x{0:x}", rune);
				Assert.AreEqual (Unicode.P.InRange (rune), Unicode.IsPunct (rune), "IsPunct, The fast path and the slow path disagree for rune 0x{0:x}", rune);
				Assert.AreEqual (Unicode.White_Space.InRange (rune), Unicode.IsSpace (rune), "IsSpace, The fast path and the slow path disagree for rune 0x{0:x}", rune);
				Assert.AreEqual (Unicode.S.InRange (rune), Unicode.IsSymbol (rune), "IsSymbol, The fast path and the slow path disagree for rune 0x{0:x}", rune);

			}
		}

		uint [] upperTest = new uint [] {
			0x41,
			0xc0,
			0xd8,
			0x100,
			0x139,
			0x14a,
			0x178,
			0x181,
			0x376,
			0x3cf,
			0x13bd,
			0x1f2a,
			0x2102,
			0x2c00,
			0x2c10,
			0x2c20,
			0xa650,
			0xa722,
			0xff3a,
			0x10400,
			0x1d400,
			0x1d7ca,
		};

		uint [] notupperTest = new uint []{
			0x40,
			0x5b,
			0x61,
			0x185,
			0x1b0,
			0x377,
			0x387,
			0x2150,
			0xab7d,
			0xffff,
			0x10000,
		};

		uint [] letterTest = new uint [] {
			0x41,
			0x61,
			0xaa,
			0xba,
			0xc8,
			0xdb,
			0xf9,
			0x2ec,
			0x535,
			0x620,
			0x6e6,
			0x93d,
			0xa15,
			0xb99,
			0xdc0,
			0xedd,
			0x1000,
			0x1200,
			0x1312,
			0x1401,
			0x2c00,
			0xa800,
			0xf900,
			0xfa30,
			0xffda,
			0xffdc,
			0x10000,
			0x10300,
			0x10400,
			0x20000,
			0x2f800,
			0x2fa1d,
		};

		uint [] notletterTest = new uint [] {
			0x20,
			0x35,
			0x375,
			0x619,
			0x700,
			0x1885,
			0xfffe,
			0x1ffff,
			0x10ffff,
		};

		// Contains all the special cased Latin-1 chars.
		uint [] spaceTest = new uint [] {
			0x09,
			0x0a,
			0x0b,
			0x0c,
			0x0d,
			0x20,
			0x85,
			0xA0,
			0x2000,
			0x3000,
		};

		[Test]
		public void TestIsLetter ()
		{
			foreach (var rune in upperTest)
				Assert.AreEqual (true, Unicode.IsLetter (rune), "IsLetter Expected rune 0x{0:x} to be a letter", rune);
			foreach (var rune in letterTest)
				Assert.AreEqual (true, Unicode.IsLetter (rune), "IsLetter Expected rune 0x{0:x} to be a letter", rune);
			foreach (var rune in notletterTest)
				Assert.AreEqual (false, Unicode.IsLetter (rune), "IsLetter Expected rune 0x{0:x} to not be a letter", rune);
		}

		[Test]
		public void TestIsUpper ()
		{
			foreach (var rune in upperTest)
				Assert.AreEqual (true, Unicode.IsUpper (rune), "IsUpper Expected rune 0x{0:x} to be a upper", rune);
			foreach (var rune in notupperTest)
				Assert.AreEqual (false, Unicode.IsUpper (rune), "IsLetter Expected rune 0x{0:x} not to be a upper", rune);
			foreach (var rune in notletterTest)
				Assert.AreEqual (false, Unicode.IsUpper (rune), "IsLetter Expected rune 0x{0:x} to not be an upper case", rune);
		}

		const int UpperCase = 0;
		const int LowerCase = 1;
		const int TitleCase = 2;

		int [,] caseTest = new int [,] {
			        // errors
		        {-1, '\n', 0xFFFD},
			{UpperCase, -1, -1},
			{UpperCase, 1 << 30, 1 << 30},
		        
		        // ASCII (special-cased so test carefully)
		        {UpperCase, '\n', '\n'},
			{UpperCase, 'a', 'A'},
			{UpperCase, 'A', 'A'},
			{UpperCase, '7', '7'},
			{LowerCase, '\n', '\n'},
			{LowerCase, 'a', 'a'},
			{LowerCase, 'A', 'a'},
			{LowerCase, '7', '7'},
			{TitleCase, '\n', '\n'},
			{TitleCase, 'a', 'A'},
			{TitleCase, 'A', 'A'},
			{TitleCase, '7', '7'},
		        
		        // Latin-1: easy to read the tests!
		        {UpperCase, 0x80, 0x80},
			{UpperCase, 'Å', 'Å'},
			{UpperCase, 'å', 'Å'},
			{LowerCase, 0x80, 0x80},
			{LowerCase, 'Å', 'å'},
			{LowerCase, 'å', 'å'},
			{TitleCase, 0x80, 0x80},
			{TitleCase, 'Å', 'Å'},
			{TitleCase, 'å', 'Å'},

		        // 0131;LATIN SMALL LETTER DOTLESS I;Ll;0;L;;;;;N;;;0049;;0049
		        {UpperCase, 0x0131, 'I'},
			{LowerCase, 0x0131, 0x0131},
			{TitleCase, 0x0131, 'I'},

		        // 0133;LATIN SMALL LIGATURE IJ;Ll;0;L;<compat> 0069 006A;;;;N;LATIN SMALL LETTER I J;;0132;;0132
		        {UpperCase, 0x0133, 0x0132},
			{LowerCase, 0x0133, 0x0133},
			{TitleCase, 0x0133, 0x0132},

		        // 212A;KELVIN SIGN;Lu;0;L;004B;;;;N;DEGREES KELVIN;;;006B;
		        {UpperCase, 0x212A, 0x212A},
			{LowerCase, 0x212A, 'k'},
			{TitleCase, 0x212A, 0x212A},

		        // From an UpperLower sequence
		        // A640;CYRILLIC CAPITAL LETTER ZEMLYA;Lu;0;L;;;;;N;;;;A641;
		        {UpperCase, 0xA640, 0xA640},
			{LowerCase, 0xA640, 0xA641},
			{TitleCase, 0xA640, 0xA640},
		       // A641;CYRILLIC SMALL LETTER ZEMLYA;Ll;0;L;;;;;N;;;A640;;A640
		        {UpperCase, 0xA641, 0xA640},
			{LowerCase, 0xA641, 0xA641},
			{TitleCase, 0xA641, 0xA640},
		        // A64E;CYRILLIC CAPITAL LETTER NEUTRAL YER;Lu;0;L;;;;;N;;;;A64F;
		        {UpperCase, 0xA64E, 0xA64E},
			{LowerCase, 0xA64E, 0xA64F},
			{TitleCase, 0xA64E, 0xA64E},
		        // A65F;CYRILLIC SMALL LETTER YN;Ll;0;L;;;;;N;;;A65E;;A65E
		        {UpperCase, 0xA65F, 0xA65E},
			{LowerCase, 0xA65F, 0xA65F},
			{TitleCase, 0xA65F, 0xA65E},
		        
		        // From another UpperLower sequence
		        // 0139;LATIN CAPITAL LETTER L WITH ACUTE;Lu;0;L;004C 0301;;;;N;LATIN CAPITAL LETTER L ACUTE;;;013A;
		        {UpperCase, 0x0139, 0x0139},
			{LowerCase, 0x0139, 0x013A},
			{TitleCase, 0x0139, 0x0139},
		        // 013F;LATIN CAPITAL LETTER L WITH MIDDLE DOT;Lu;0;L;<compat> 004C 00B7;;;;N;;;;0140;
		        {UpperCase, 0x013f, 0x013f},
			{LowerCase, 0x013f, 0x0140},
			{TitleCase, 0x013f, 0x013f},
		        // 0148;LATIN SMALL LETTER N WITH CARON;Ll;0;L;006E 030C;;;;N;LATIN SMALL LETTER N HACEK;;0147;;0147
		        {UpperCase, 0x0148, 0x0147},
			{LowerCase, 0x0148, 0x0148},
			{TitleCase, 0x0148, 0x0147},
		        
		        // Lowercase lower than uppercase.
		        // AB78;CHEROKEE SMALL LETTER GE;Ll;0;L;;;;;N;;;13A8;;13A8
		        {UpperCase, 0xab78, 0x13a8},
			{LowerCase, 0xab78, 0xab78},
			{TitleCase, 0xab78, 0x13a8},
			{UpperCase, 0x13a8, 0x13a8},
			{LowerCase, 0x13a8, 0xab78},
			{TitleCase, 0x13a8, 0x13a8},

		        // Last block in the 5.1.0 table
		        // 10400;DESERET CAPITAL LETTER LONG I;Lu;0;L;;;;;N;;;;10428;
		        {UpperCase, 0x10400, 0x10400},
			{LowerCase, 0x10400, 0x10428},
			{TitleCase, 0x10400, 0x10400},
		        // 10427;DESERET CAPITAL LETTER EW;Lu;0;L;;;;;N;;;;1044F;
		        {UpperCase, 0x10427, 0x10427},
			{LowerCase, 0x10427, 0x1044F},
			{TitleCase, 0x10427, 0x10427},
		        // 10428;DESERET SMALL LETTER LONG I;Ll;0;L;;;;;N;;;10400;;10400
		        {UpperCase, 0x10428, 0x10400},
			{LowerCase, 0x10428, 0x10428},
			{TitleCase, 0x10428, 0x10400},
		        // 1044F;DESERET SMALL LETTER EW;Ll;0;L;;;;;N;;;10427;;10427
		        {UpperCase, 0x1044F, 0x10427},
			{LowerCase, 0x1044F, 0x1044F},
			{TitleCase, 0x1044F, 0x10427},

		        // First one not in the 5.1.0 table
		        // 10450;SHAVIAN LETTER PEEP;Lo;0;L;;;;;N;;;;;
		        {UpperCase, 0x10450, 0x10450},
			{LowerCase, 0x10450, 0x10450},
			{TitleCase, 0x10450, 0x10450},

		        // Non-letters with case.
		        {LowerCase, 0x2161, 0x2171},
			{UpperCase, 0x0345, 0x0399},

		};

		[Test]
		public void TestTo ()
		{
			Unicode.To (Unicode.Case.Upper, 0x133);
			for (int i = 0; i < caseTest.GetLength (0); i++){
				var input = (uint) caseTest [i, 1];
				var output = (uint) caseTest [i, 2];
				var tcase = (Unicode.Case)caseTest [i, 0];

				Assert.AreEqual (output, Unicode.To (tcase, input), "Invalid return for 0x{0:x} for {1}", input, tcase);
			}
		}

		[Test]
		public void TestSpace ()
		{
			foreach (var x in spaceTest) 
				Assert.IsTrue (Unicode.IsSpace (x), "For rune: 0x{0:x}", x);
			foreach (var x in letterTest)
				Assert.IsFalse (Unicode.IsSpace (x), "For rune: 0x{0:x}", x);
		}

		[Test]
		public void TestLetterOptimizations ()
		{
			for (uint rune = 0; rune < Unicode.MaxLatin1; rune++) {
				Assert.AreEqual (Unicode.To (Unicode.Case.Lower, rune), Unicode.ToLower (rune), "For rune 0x{0:x} ToLower", rune);
				Assert.AreEqual (Unicode.To (Unicode.Case.Upper, rune), Unicode.ToUpper (rune), "For rune 0x{0:x} ToUpper", rune);
				Assert.AreEqual (Unicode.To (Unicode.Case.Title, rune), Unicode.ToTitle (rune), "For rune 0x{0:x} ToTitle", rune);
			}
		}

		[Test]
		public void TestTurkishCase ()
		{
			var lower = "abcçdefgğhıijklmnoöprsştuüvyz";
			var upper = "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ";

			Assert.AreEqual (lower.Length, upper.Length);
			for (int i = 0; i < lower.Length; i++) {
				var l = lower [i];
				var u = upper [i];

				Assert.AreEqual (l, Unicode.TurkishCase.ToLower (l), "l == Tolower(l) Must be the same for rune 0x{0:x}", (int) l);
				Assert.AreEqual (u, Unicode.TurkishCase.ToUpper (u), "u == ToLower (u) Must be the same for rune 0x{0:x}", (int)l);
				Assert.AreEqual (u, Unicode.TurkishCase.ToUpper (l), "u == ToLower (l) Must be the same for rune 0x{0:x}", (int)l);
				Assert.AreEqual (l, Unicode.TurkishCase.ToLower (u), "l == ToUpper (u) Must be the same for rune 0x{0:x}", (int)l);
				Assert.AreEqual (u, Unicode.TurkishCase.ToTitle (u), "u == ToTitle (u) Must be the same for rune 0x{0:x}", (int)l);
				Assert.AreEqual (u, Unicode.TurkishCase.ToTitle (l), "u == ToTitle (l) Must be the same for rune 0x{0:x}", (int)l);
			}
		}

		byte [][] simpleFoldTests = new byte [][] {
			// SimpleFold(x) returns the next equivalent rune > x or wraps
		        // around to smaller values.
		        
		        // Easy cases.
			new byte []{ 0x41, 0x61 },
			new byte []{ 0xce, 0xb4, 0xce, 0x94 },

		        
		        // ASCII special cases.
			new byte []{ 0x4b, 0x6b, 0xe2, 0x84, 0xaa },
			new byte []{ 0x53, 0x73, 0xc5, 0xbf },
		        
		        // Non-ASCII special cases.
			new byte []{ 0xcf, 0x81, 0xcf, 0xb1, 0xce, 0xa1 },
			new byte []{ 0xcd, 0x85, 0xce, 0x99, 0xce, 0xb9, 0xe1, 0xbe, 0xbe }
		                        
		        // Upper comes before lower (Cherokee).
		        //"\u13b0\uab80",
		};

		// SimpleFold(x) returns the next equivalent rune > x or wraps
		// around to smaller values.
		[Test]
		public void TestSimpleFold ()
		{
			foreach (byte [] test in simpleFoldTests) {
				var tstr = ustring.Make (test);
				var cycle = tstr.ToRunes ();
				uint r = cycle [cycle.Length - 1];
				for (int i = 0; i < cycle.Length; i++) {
					var expected = cycle [i];
					r = Unicode.SimpleFold (r);
					Assert.AreEqual (r, expected, "SimpleFold error, for 0x{0:x} wanted 0x{1:x}", r, expected);
					r = expected;
				}
			}
			unchecked {
				Assert.AreEqual ((uint)-42, Unicode.SimpleFold ((uint)-42));
			}
		}
	}
}
