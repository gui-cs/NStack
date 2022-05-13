// 
// ustringtest: ustring tests
//
//﻿ Miguel de Icaza
//
using NUnit.Framework;
using System;
using NStack;
using System.Runtime.InteropServices;
using System.Text;
using Rune = System.Rune;

namespace NStackTests {
	[TestFixture]
	public class UstringTest {
		ustring a = ustring.Make ("a");
		ustring seconda = ustring.Make ("a");
		ustring aa = ustring.Make ("aa");
		ustring b = ustring.Make ("b");
		ustring bb = ustring.Make ("bb");
		ustring empty = ustring.Make ("");
		ustring secondempty = ustring.Make ("");

		ustring hello = ustring.Make ("hello, world");
		ustring longhello = ustring.Make ("");
		ustring kosme = ustring.Make (0xce, 0xba, 0xcf, 0x8c, 0xcf, 0x83, 0xce, 0xbc, 0xce, 0xb5);
		ustring kosmex = ustring.Make (0xce, 0xba, 0xcf, 0x8c, 0xcf, 0x83, 0xce, 0xbc, 0xce, 0xb5, 0x41);

		[Test]
		public void ToLowerTest ()
		{
			var x = ustring.Make ("C-x");
			var res = x.ToLower ();
		}

		[Test]
		public void IComparableTests ()
		{


			// Compares same-sized strings
			Assert.AreEqual (-1, a.CompareTo (b));
			Assert.AreEqual (1, b.CompareTo (a));
			Assert.AreEqual (-1, aa.CompareTo (bb));
			Assert.AreEqual (1, bb.CompareTo (aa));

			// Empty
			Assert.AreEqual (0, empty.CompareTo (empty));
			Assert.AreEqual (-1, empty.CompareTo (a));
			Assert.AreEqual (1, a.CompareTo (empty));

			// Same instances
			Assert.AreEqual (0, a.CompareTo (a));
			Assert.AreEqual (0, seconda.CompareTo (a));
			Assert.AreEqual (0, a.CompareTo (seconda));

			// Different sizes
			Assert.AreEqual (-1, a.CompareTo (aa));
			Assert.AreEqual (1, aa.CompareTo (a));
		}

		[Test]
		public void Compare ()
		{
			Assert.AreEqual (a, seconda);
			Assert.IsTrue (a != b);
			Assert.IsFalse (a.Equals (b));
			Assert.IsFalse (b.Equals (a));
			Assert.AreNotEqual (a, b);
			Assert.AreNotEqual (b, a);
			Assert.AreNotEqual (a, aa);
			Assert.AreNotEqual (aa, a);
			Assert.AreEqual (empty, empty);
			Assert.AreEqual (empty, secondempty);
		}

		[Test]
		public void Equals ()
		{
			Assert.IsTrue (a == seconda);
			Assert.IsFalse (a == b);

			string aref = "asdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdf$";
			var ast = ustring.Make (aref);
			var bst = ustring.Make (aref);
			var cst = ustring.Make (aref.Replace ("$", "D"));
			Assert.IsTrue (ast == bst);
			Assert.IsTrue (ast != cst);

			var abytes = Encoding.UTF8.GetBytes (aref);
			var bbytes = Encoding.UTF8.GetBytes (aref);
			var cbytes = Encoding.UTF8.GetBytes (aref.Replace ("$", "D"));

			var len = abytes.Length;
			var a1 = Marshal.AllocHGlobal (abytes.Length + 1);
			Marshal.Copy (abytes, 0, a1, abytes.Length);
			var b1 = Marshal.AllocHGlobal (bbytes.Length + 1);
			Marshal.Copy (abytes, 0, b1, abytes.Length);
			var c1 = Marshal.AllocHGlobal (cbytes.Length + 1);
			Marshal.Copy (cbytes, 0, c1, abytes.Length);
			var ap = ustring.Make (a1, len);
			var bp = ustring.Make (b1, len);
			var cp = ustring.Make (c1, len);

			var apalias = ap;
			Assert.IsTrue(ap.Equals(bp));
			Assert.IsTrue (ap == bp);

			string arefMod = "asdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdy$";
			Assert.IsFalse(ap.Equals(arefMod));
			Assert.IsFalse(ap == arefMod);

			Assert.IsTrue (ap == apalias);
			Assert.IsTrue (ap != cp);

			// Now compare ones with others
			Assert.IsTrue (ast == ap);
			Assert.IsTrue (ast == bp);
			Assert.IsTrue (ap == bst);
			Assert.IsTrue (ast == bp);
			Assert.IsTrue (ast != cp);
			var cpalias = cp;
			Assert.IsTrue (cp == cpalias);
			Assert.IsTrue (cp == cst);
			Assert.IsTrue (cp != bst);

			// Slices
			Assert.IsTrue (ast [1, 5] == ap [1, 5]);
			Assert.IsTrue (ast [1, 5] == bp [1, 5]);
			Assert.IsTrue (ap [1, 5] == bst [1, 5]);
			Assert.IsTrue (ast [1, 5] == bp [1, 5]);
			Assert.IsTrue (ast [8, null] != cp [8, null]);
			Assert.IsTrue (cp [1, 5] == cpalias [1, 5]);
			Assert.IsTrue (cp [1, 5] == cst [1, 5]);
			Assert.IsTrue (cp [8, null] != bst [8, null]);

		}

		// string, substring, expected
		(string, string, bool) [] ContainTests = {
			("abc", "bc", true),
			("abc", "bcd", false),
			("abc", "", true),
			("", "a", false),

			// 2-byte needle
			("xxxxxx", "01", false),
			("01xxxx", "01", true),
			("xx01xx", "01", true),
			("xxxx01", "01", true),
			("1xxxxx", "01", false),
			("xxxxx0", "01", false),
			// 3-byte needle
			("xxxxxxx", "012", false),
			("012xxxx", "012", true),
			("xx012xx", "012", true),
			("xxxx012", "012", true),
			("12xxxxx", "012", false),
			("xxxxx01", "012", false),
			// 4-byte needle
			("xxxxxxxx", "0123", false),
			("0123xxxx", "0123", true),
			("xx0123xx", "0123", true),
			("xxxx0123", "0123", true),
			("123xxxxx", "0123", false),
			("xxxxx012", "0123", false),
			// 5-7-byte needle
			("xxxxxxxxx", "01234", false),
			("01234xxxx", "01234", true),
			("xx01234xx", "01234", true),
			("xxxx01234", "01234", true),
			("1234xxxxx", "01234", false),
			("xxxxx0123", "01234", false),
			// 8-byte needle
			("xxxxxxxxxxxx", "01234567", false),
			("01234567xxxx", "01234567", true),
			("xx01234567xx", "01234567", true),
			("xxxx01234567", "01234567", true),
			("1234567xxxxx", "01234567", false),
			("xxxxx0123456", "01234567", false),
			// 9-15-byte needle
			("xxxxxxxxxxxxx", "012345678", false),
			("012345678xxxx", "012345678", true),
			("xx012345678xx", "012345678", true),
			("xxxx012345678", "012345678", true),
			("12345678xxxxx", "012345678", false),
			("xxxxx01234567", "012345678", false),
			// 16-byte needle
			("xxxxxxxxxxxxxxxxxxxx", "0123456789ABCDEF", false),
			("0123456789ABCDEFxxxx", "0123456789ABCDEF", true),
			("xx0123456789ABCDEFxx", "0123456789ABCDEF", true),
			("xxxx0123456789ABCDEF", "0123456789ABCDEF", true),
			("123456789ABCDEFxxxxx", "0123456789ABCDEF", false),
			("xxxxx0123456789ABCDE", "0123456789ABCDEF", false),
			// 17-31-byte needle
			("xxxxxxxxxxxxxxxxxxxxx", "0123456789ABCDEFG", false),
			("0123456789ABCDEFGxxxx", "0123456789ABCDEFG", true),
			("xx0123456789ABCDEFGxx", "0123456789ABCDEFG", true),
			("xxxx0123456789ABCDEFG", "0123456789ABCDEFG", true),
			("123456789ABCDEFGxxxxx", "0123456789ABCDEFG", false),
			("xxxxx0123456789ABCDEF", "0123456789ABCDEFG", false),

			// partial match cases
			("xx01x", "012", false),                             // 3
			("xx0123x", "01234", false),                         // 5-7
			("xx01234567x", "012345678", false),                 // 9-15
			("xx0123456789ABCDEFx", "0123456789ABCDEFG", false), // 17-31, issue 15679
		};

		(string, string, bool) [] containsAnyTests = {
			// string, substring, expected
			("", "", false),
			("", "a", false),
			("", "abc", false),
			("a", "", false),
			("a", "a", true),
			("aaa", "a", true),
			("abc", "xyz", false),
			("abc", "xcz", true),
			("a☺b☻c☹d", "uvw☻xyz", true),
			("aRegExp*", ".(|)*+?^$[]", true),
			("1....2....3....41....2....3....41....2....3....4", " ", false),

		};

		[Test]
		public void TestContainsAny ()
		{
			foreach ((var str, var substr, bool expected) in containsAnyTests) {
				var ustr = ustring.Make (str);
				Assert.AreEqual (expected, ustr.ContainsAny (substr), $"{str}.ContainsAny ({substr})");
			}
		}

		[Test]
		public void TestContains ()
		{
			Assert.IsTrue (aa.Contains (a));
			Assert.IsFalse (aa.Contains (b));
			Assert.IsTrue (bb.Contains (b));

			foreach ((string str, string sub, bool expected) in ContainTests) {
				var ustr = ustring.Make (str);
				var usub = ustring.Make (sub);
				Assert.AreEqual (expected, ustr.Contains (usub), $"{ustr}.Contains({usub}) error");
			}
		}

		(string, uint, bool) [] containsRuneTests = {
			("", 'a', false),
			("a", 'a', true),
			("aaa", 'a', true),
			("abc", 'y', false),
			("abc", 'c', true),
			("a☺b☻c☹d", 'x', false),
			("a☺b☻c☹d", '☻', true),
			("aRegExp*", '*', true),
		};

		[Test]
		public void TestContainsRune ()
		{
			foreach ((var str, uint rune, bool expected) in containsRuneTests) {
				var ustr = ustring.Make (str);
				Assert.AreEqual (expected, ustr.Contains (rune), $"{ustr}.Contains({rune})");
			}
		}

		(string, string, bool) [] equalFoldsTest = {
			("abc", "abc", true),
			("ABcd", "ABcd", true),
			("123abc", "123ABC", true),
			("αβδ", "ΑΒΔ", true),
			("abc", "xyz", false),
			("abc", "XYZ", false),
			("abcdefghijk", "abcdefghijX", false),

			// need byte array for these, as they are not 
			("abcdefghijk", "abcdefghij\u212A", true),
			("abcdefghijK", "abcdefghij\u212A", true),
			("abcdefghijkz", "abcdefghij\u212Ay", false),
			("abcdefghijKz", "abcdefghij\u212Ay", false),

		};

		[Test]
		public void TestEqualFolds ()
		{
			var k = ustring.Make (0x212a);
			Assert.AreEqual (true, k.EqualsFold ("k"));

			foreach ((string s, string t, bool expected) in equalFoldsTest) {
				Assert.AreEqual (expected, ustring.Make (s).EqualsFold (t), $"For {s} and {t}");
				Assert.AreEqual (expected, ustring.Make (t).EqualsFold (s), $"For {s} and {t}");
			}
		}

		(string, string, int) [] countTests = {
			("", "", 1),
			("", "notempty", 0),
			("notempty", "", 9),
			("smaller", "not smaller", 0),
			("12345678987654321", "6", 2),
			("611161116", "6", 3),
			("notequal", "NotEqual", 0),
			("equal", "equal", 1),
			("abc1231231123q", "123", 3),
			("11111", "11", 2)
		};

		[Test]
		public void TestCount ()
		{
			foreach ((string src, string sub, int count) in countTests) {
				Assert.AreEqual (count, ustring.Make (src).Count (sub), $"Count for ({src}, {sub})");
			}
		}

		[Test]
		public void TestIndexOf ()
		{
			Assert.AreEqual (0, hello.IndexOf ('h'));
			Assert.AreEqual (1, hello.IndexOf ('e'));
			Assert.AreEqual (2, hello.IndexOf ('l'));
			Assert.AreEqual (10, kosmex.IndexOf (0x41));
		}

		[Test]
		public void TestLength ()
		{
			Assert.AreEqual (12, hello.Length, "Byte length");
			Assert.AreEqual (12, hello.RuneCount, "Rune Count");
			Assert.AreEqual (10, kosme.Length, "Byte kosme");
			Assert.AreEqual (5, kosme.RuneCount, "Rune kosme");
		}

		void SliceTests (ustring a)
		{
			Assert.AreEqual ("1234", a [0, 4].ToString ());
			Assert.AreEqual ("90", a [8, 10].ToString ());
			Assert.AreEqual ("90", a [8, null].ToString ());
			Assert.AreEqual ("90", a [-2, null].ToString ());
			Assert.AreEqual ("9", a [8, 9].ToString ());
			Assert.AreEqual ("789", a [-4, -1].ToString ());
			Assert.AreEqual ("7890", a [-4, null].ToString ());
			Assert.AreEqual ("7890", a [-4, null].ToString ());
			Assert.AreEqual ("234567", a [-9, -3].ToString ());
			Assert.AreEqual ("", a [100, 200].ToString ());
			Assert.AreEqual ("", a [-100, null].ToString ());

			Assert.AreEqual ("", a [-100, 0].ToString ());
			Assert.AreEqual ("", a [0, 0].ToString ());
		}

		[Test]
		public void TestSliceRanges ()
		{
			var str = "1234567890";
			ustring a = ustring.Make (str);
			Assert.AreEqual (str, a.ToString ());

			var asbyte = new byte [] { (byte)'y', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'0', (byte)'z' };

			SliceTests (a);
			SliceTests (ustring.Make ("x" + str + "x") [1, 11]);

			var f = ustring.Make (asbyte, 1, 10);
			SliceTests (f);

			unsafe {
				fixed (byte* p = &asbyte [1]) {
					var ptrstr = ustring.Make ((IntPtr)p, 10);
					SliceTests (ptrstr);
				}
			}
		}

		//
		// Tests that we call the release function if provided
		// explicitly does this by calling Dispose
		//
		[Test]
		public void TestBlockRelease ()
		{
			bool released = false;
			Action<IntPtr> releaseFunc = (block) => {
				released = true;
			};
			var ptr = Marshal.AllocHGlobal (10);
			var s = ustring.Make (ptr, 10, releaseFunc);
			Assert.True (s is IDisposable);
			var id = s as IDisposable;
			id?.Dispose ();
			Assert.True (released);
		}

		[Test]
		public void TestTrim ()
		{
			Assert.IsTrue (ustring.Make ("hello") == ustring.Make (" hello ").TrimSpace ());
			Assert.IsTrue (ustring.Make ("hello") == ustring.Make ("\nhello\t").TrimSpace ());
			Assert.IsTrue (ustring.Make ("hel \t\tlo") == ustring.Make ("    hel \t\tlo ").TrimSpace ());
			Assert.IsTrue (ustring.Make ("  hello") == ustring.Make ("  hello").TrimEnd (ustring.Make (" ")));
			Assert.IsTrue (ustring.Make ("hello  ") == ustring.Make ("hello  ").TrimStart (ustring.Make (" ")));
			Assert.IsTrue (ustring.Make ("  hello") == ustring.Make ("  hello  ").TrimEnd (ustring.Make (" ")));
			Assert.IsTrue (ustring.Make ("hello  ") == ustring.Make ("  hello  ").TrimStart (ustring.Make (" ")));

			Assert.IsTrue (ustring.Make ("oot") == ustring.Make ("ffffffoot").TrimStart (x => x == 'f'));
		}

		[Test]
		public void Split ()
		{
			var gecos = ustring.Make ("miguel:*:100:200:Miguel de Icaza:/home/miguel:/bin/bash");
			var fields = gecos.Split (":");

			Assert.AreEqual (7, fields.Length);
			Assert.IsTrue (ustring.Make ("miguel") == fields [0]);
			Assert.IsTrue (ustring.Make ("*") == fields [1]);
			Assert.IsTrue (ustring.Make ("100") == fields [2]);
			Assert.IsTrue (ustring.Make ("200") == fields [3]);
			Assert.IsTrue (ustring.Make ("Miguel de Icaza") == fields [4]);
			Assert.IsTrue (ustring.Make ("/home/miguel") == fields [5]);
			Assert.IsTrue (ustring.Make ("/bin/bash") == fields [6]);

			gecos = ustring.Make ("miguel<>*<>100<>200<>Miguel de Icaza<>/home/miguel<>/bin/bash");
			fields = gecos.Split ("<>");
			Assert.AreEqual (7, fields.Length);
			Assert.IsTrue (ustring.Make ("miguel") == fields [0]);
			Assert.IsTrue (ustring.Make ("*") == fields [1]);
			Assert.IsTrue (ustring.Make ("100") == fields [2]);
			Assert.IsTrue (ustring.Make ("200") == fields [3]);
			Assert.IsTrue (ustring.Make ("Miguel de Icaza") == fields [4]);
			Assert.IsTrue (ustring.Make ("/home/miguel") == fields [5]);
			Assert.IsTrue (ustring.Make ("/bin/bash") == fields [6]);
		}

		[Test]
		public void TestCopy ()
		{
			// Test the zero-terminator method
			var j = Encoding.UTF8.GetBytes ("Hello");
			var p = Marshal.AllocHGlobal (j.Length + 1);
			Marshal.Copy (j, 0, p, j.Length);
			Marshal.WriteByte (p, j.Length, 0);
			var str = ustring.Make (p);
			Assert.AreEqual (5, str.Length);


			// Now test the copy
			var str2 = ustring.MakeCopy (p);
			Marshal.WriteByte (p, (byte)'A');
			Assert.AreEqual (str.ToString (), "Aello");
			Assert.AreEqual (str2.ToString (), "Hello");
			Assert.IsFalse (str == str2);

			((IDisposable)str).Dispose ();
			((IDisposable)str2).Dispose ();
		}

		static (string, string, string, int, string) [] replaceTexts = {
			// input, oldValue, newValue, n parameter, expected
			("hello", "l", "L", 0, "hello"),
			("hello", "l", "L", -1, "heLLo"),
			("hello", "x", "X", -1, "hello"),
			("", "x", "X", -1, ""),
			("radar", "r", "<r>", -1, "<r>ada<r>"),
			("", "", "<>", -1, "<>"),
			("banana", "a", "<>", -1, "b<>n<>n<>"),
			("banana", "a", "<>", 1, "b<>nana"),
			("banana", "a", "<>", 1000, "b<>n<>n<>"),
			("banana", "an", "<>", -1, "b<><>a"),
			("banana", "ana", "<>", -1, "b<>na"),
			("banana", "", "<>", -1, "<>b<>a<>n<>a<>n<>a<>"),
			("banana", "", "<>", 10, "<>b<>a<>n<>a<>n<>a<>"),
			("banana", "", "<>", 6, "<>b<>a<>n<>a<>n<>a"),
			("banana", "", "<>", 5, "<>b<>a<>n<>a<>na"),
			("banana", "", "<>", 1, "<>banana"),
			("banana", "a", "a", -1, "banana"),
			("banana", "a", "a", 1, "banana"),
			("☺☻☹", "", "<>", -1, "<>☺<>☻<>☹<>")
		};

		[Test]
		public void TestReplace ()
		{
			ustring.Make ("banana").Replace ("", "<>", -1);
			foreach ((var input, var oldv, var newv, var n, var expected) in replaceTexts) {
				var sin = ustring.Make (input);
				var result = sin.Replace (oldv, newv, n);
				Assert.IsTrue (result == expected, $"For test on Replace (\"{input}\",\"{oldv}\",\"{newv}\",{n}) got {result}");
			}
		}

		(string, string, int) [] indexTests = {
			// string, substring, expected index return
			("", "", 0),
			("", "a", -1),
			("", "foo", -1),
			("fo", "foo", -1),
			("foo", "foo", 0),
			("oofofoofooo", "f", 2),
			("oofofoofooo", "foo", 4),
			("barfoobarfoo", "foo", 3),
			("foo", "", 0),
			("foo", "o", 1),
			("abcABCabc", "A", 3),
			// cases with one byte strings - test special case in Index()
			("", "a", -1),
			("x", "a", -1),
			("x", "x", 0),
			("abc", "a", 0),
			("abc", "b", 1),
			("abc", "c", 2),
			("abc", "x", -1),
			// test special cases in Index() for short strings
			("", "ab", -1),
			("bc", "ab", -1),
			("ab", "ab", 0),
			("xab", "ab", 1),
			("", "abc", -1),
			("xbc", "abc", -1),
			("abc", "abc", 0),
			("xabc", "abc", 1),
			("xabxc", "abc", -1),
			("", "abcd", -1),
			("xbcd", "abcd", -1),
			("abcd", "abcd", 0),
			("xabcd", "abcd", 1),
			("xbcqq", "abcqq", -1),
			("abcqq", "abcqq", 0),
			("xabcqq", "abcqq", 1),
			("xabxcqq", "abcqq", -1),
			("xabcqxq", "abcqq", -1),
			("", "01234567", -1),
			("32145678", "01234567", -1),
			("01234567", "01234567", 0),
			("x01234567", "01234567", 1),
			("x0123456x01234567", "01234567", 9),
			("", "0123456789", -1),
			("3214567844", "0123456789", -1),
			("0123456789", "0123456789", 0),
			("x0123456789", "0123456789", 1),
			("x012345678x0123456789", "0123456789", 11),
			("x01234567x89", "0123456789", -1),
			("", "0123456789012345", -1),
			("3214567889012345", "0123456789012345", -1),
			("0123456789012345", "0123456789012345", 0),
			("x0123456789012345", "0123456789012345", 1),
			("x012345678901234x0123456789012345", "0123456789012345", 17),
			("", "01234567890123456789", -1),
			("32145678890123456789", "01234567890123456789", -1),
			("01234567890123456789", "01234567890123456789", 0),
			("x01234567890123456789", "01234567890123456789", 1),
			("x0123456789012345678x01234567890123456789", "01234567890123456789", 21),
			("", "0123456789012345678901234567890", -1),
			("321456788901234567890123456789012345678911", "0123456789012345678901234567890", -1),
			("0123456789012345678901234567890", "0123456789012345678901234567890", 0),
			("x0123456789012345678901234567890", "0123456789012345678901234567890", 1),
			("x012345678901234567890123456789x0123456789012345678901234567890", "0123456789012345678901234567890", 32),
			("", "01234567890123456789012345678901", -1),
			("32145678890123456789012345678901234567890211", "01234567890123456789012345678901", -1),
			("01234567890123456789012345678901", "01234567890123456789012345678901", 0),
			("x01234567890123456789012345678901", "01234567890123456789012345678901", 1),
			("x0123456789012345678901234567890x01234567890123456789012345678901", "01234567890123456789012345678901", 33),
			("xxxxxx012345678901234567890123456789012345678901234567890123456789012", "012345678901234567890123456789012345678901234567890123456789012", 6),
			("", "0123456789012345678901234567890123456789", -1),
			("xx012345678901234567890123456789012345678901234567890123456789012", "0123456789012345678901234567890123456789", 2),
			("xx012345678901234567890123456789012345678901234567890123456789012", "0123456789012345678901234567890123456xxx", -1),
			("xx0123456789012345678901234567890123456789012345678901234567890120123456789012345678901234567890123456xxx", "0123456789012345678901234567890123456xxx", 65)
		};

		[Test]
		public void TestIndex ()
		{
			foreach ((string s, string sep, int pos) in indexTests) {
				Assert.AreEqual (pos, ustring.Make (s).IndexOf (sep), $"{s}.IndexOf ({sep})");
			}
		}

		(string, string, int) [] lastIndexTests = {
			("", "", 0),
			("", "a", -1),
			("", "foo", -1),
			("fo", "foo", -1),
			("foo", "foo", 0),
			("foo", "f", 0),
			("oofofoofooo", "f", 7),
			("oofofoofooo", "foo", 7),
			("barfoobarfoo", "foo", 9),
			("foo", "", 3),
			("foo", "o", 2),
			("abcABCabc", "A", 3),
			("abcABCabc", "a", 6),

		};

		[Test]
		public void TestLastIndex ()
		{
			foreach ((string s, string sep, int pos) in lastIndexTests) {
				Assert.AreEqual (pos, ustring.Make (s).LastIndexOf (sep), $"{s}.LastIndexOf ({sep}) = {ustring.Make (s).LastIndexOf (sep)}");
			}
		}

		(string, string, int) [] indexAnyTests = {
			("", "", -1),
			("", "a", -1),
			("", "abc", -1),
			("a", "", -1),
			("a", "a", 0),
			("aaa", "a", 0),
			("abc", "xyz", -1),
			("abc", "xcz", 2),
			("ab☺c", "x☺yz", 2),
			("a☺b☻c☹d", "cx", ustring.Make ("a☺b☻").Length),
			("a☺b☻c☹d", "uvw☻xyz", ustring.Make("a☺b").Length),
			("aRegExp*", ".(|)*+?^$[]", 7),
			("1....2....3....41....2....3....41....2....3....4", " ", -1),

			// Need a byte initializer instead for Go [\xff][b] below
			// ("012abcba210", "\xffb", 4),
			//("012\x80bcb\x80210", "\xffb", 3)			
		}, lastIndexAnyTests = {
			("abc", "xyz", -1),
			("a", "a", 0),
			("", "", -1),
			("", "a", -1),
			("", "abc", -1),
			("a", "", -1),
			("aaa", "a", 2),
			("abc", "ab", 1),
			("ab☺c", "x☺yz", 2),
			("a☺b☻c☹d", "cx", ustring.Make("a☺b☻").Length),
			("a☺b☻c☹d", "uvw☻xyz", ustring.Make("a☺b").Length),
			("a.RegExp*", ".(|)*+?^$[]", 8),
			("1....2....3....41....2....3....41....2....3....4", " ", -1),
			// Need a byte initializer instead for Go [\xff][b] below
			//("012abcba210", "\xffb", 6),
			//("012\x80bcb\x80210", "\xffb", 7)
		}, lastIndexByteTests = {
			("abcdefabcdef", "a", ustring.Make("abcdef").Length),      // something in the middle
			("", "q", -1),
			("abcdef", "q", -1),
			("abcdefabcdef", "f", ustring.Make("abcdefabcde").Length), // last byte
			("zabcdefabcdef", "z", 0),                 // first byte
			("a☺b☻c☹d", "b", ustring.Make("a☺").Length),               // non-ascii
		};

		[Test]
		public void TestIndexAny ()
		{
			foreach ((string s, string sep, int pos) in indexAnyTests) {
				Assert.AreEqual (pos, ustring.Make (s).IndexOfAny (sep), $"{s}.IndexOfAny ({sep})");
			}
		}

		[Test]
		public void TestLastIndexAny ()
		{
			foreach ((string s, string sep, int pos) in lastIndexAnyTests) {
				Assert.AreEqual (pos, ustring.Make (s).LastIndexOfAny (sep), $"{s}.LastIndexOfAny ({sep})");
			}
		}

		[Test]
		public void TestLastIndexByte ()
		{
			foreach ((string s, string sep, int pos) in lastIndexByteTests) {
				Assert.AreEqual (pos, ustring.Make (s).LastIndexByte ((byte)sep [0]), $"{s}.LastIndexByte ({sep})");
			}
		}

		[Test]
		public void TestIndexRune ()
		{
			(string, uint, int) [] testFirst = {
				("", 'a', -1),
				("", '☺', -1),
				("foo", '☹', -1),
				("foo", 'o', 1),
				("foo☺bar", '☺', 3),
				("foo☺☻☹bar", '☹', 9),
				("a A x", 'A', 2),
				("some_text=some_value", '=', 9),
				("☺a", 'a', 3),
				("a☻☺b", '☺', 4),
			};
			foreach ((string str, uint rune, int expected) in testFirst) {
				var ustr = ustring.Make (str);
				Assert.AreEqual (expected, ustr.IndexOf (rune));
			}
#if false
			(ustring, uint, int) [] testSecond = {
				//(ustring.Make (0xef, 0xbf, 0xbd), 0xfffd, 0),
				(ustring.Make (0xff), 0xfffd, 0),
				(ustring.Make (0xe2, 0x98, 0xbb, 0x78, 0xef, 0xbf, 0xbd), 0xfffd, 0),
				(ustring.Make (0xe2, 0x98, 0xbb, 0x78, 0xef, 0xbf, 0xbd), 0xfffd, 4),
				(ustring.Make (0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98), 0xfffd, 4),
				(ustring.Make (0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98, 0xef, 0xbf, 0xbd), 0xfffd, 4),
				(ustring.Make (0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98, 0x78), 0xfffd, 4)
			};

			// The tests below are removed because they were a port of the Go test, which
			// has slightly different semantics in the range enumerator for the string than
			// the actual encoding.   So I need to decide what way to go here.
			return;

			var ret = Utf8.Valid (new byte [] { 0xef, 0xbf, 0xbd });
			int idx = 0;
			foreach ((ustring ustr, uint rune, int expected) in testSecond) {
				Assert.AreEqual (expected, ustr.IndexOf (rune), ("For value at " + idx));
				idx++;
			}
#endif
		}

		[Test]
		public void TestConsoleWidth()
		{
			var sc = new Rune(0xd83d);
			var r = new Rune(0xdd2e);
			Assert.AreEqual(1, Rune.ColumnWidth(sc));
			Assert.False(Rune.IsNonSpacingChar(r));
			Assert.AreEqual(1, Rune.ColumnWidth(r));
			var fr = new Rune(sc, r);
			Assert.False(Rune.IsNonSpacingChar(fr));
			Assert.AreEqual(1, Rune.ColumnWidth(fr));
			var us = ustring.Make(fr);
			Assert.AreEqual(1, us.ConsoleWidth);
		}

		[Test]
		public void Test_Substring()
		{
			ustring us = "This a test to return a substring";
			Assert.AreEqual("test to return a substring", us.Substring(7));
			Assert.AreEqual("test to return", us.Substring(7, 14));
		}

		[Test]
		public void Test_RuneSubstring()
		{
			ustring us = "This a test to return a substring";
			Assert.AreEqual("test to return a substring", us.RuneSubstring(7));
			Assert.AreEqual("test to return", us.RuneSubstring(7, 14));
		}

		[Test]
		public void Test_ToRunes()
		{
			ustring us = "Some long text that 🤖🧠 is super cool";
			uint[] runesArray = us.ToRunes();
			Assert.AreEqual(us, runesArray);
		}

		[Test]
		public void Make_Environment_NewLine()
		{
			var us = ustring.Make(Environment.NewLine);
			if (Environment.NewLine.Length == 1)
			{
				Assert.AreEqual('\n', us[0]);
				Assert.AreEqual(10, us[0]);
			}
			else
			{
				Assert.AreEqual('\r', us[0]);
				Assert.AreEqual(13, us[0]);

				Assert.AreEqual('\n', us[1]);
				Assert.AreEqual(10, us[1]);
			}
		}
	}
}
