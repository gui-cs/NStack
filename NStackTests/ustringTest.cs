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
			Assert.IsTrue (ap == bp);
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
			Assert.IsTrue (ast [8, 0] != cp [8, 0]);
			Assert.IsTrue (cp [1, 5] == cpalias [1, 5]);
			Assert.IsTrue (cp [1, 5] == cst [1, 5]);
			Assert.IsTrue (cp [8, 0] != bst [8, 0]);

		}

		// string, substring, expected
		(string, string, bool) [] ContainTests = {
			("abc", "bc", true),
			("abc", "bcd", false),
			("abc", "", true),
			("", "a", false),

			// cases to cover code in runtime/asm_amd64.s:indexShortStr
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
			Assert.AreEqual ("90", a [8, 0].ToString ());
			Assert.AreEqual ("90", a [-2, 0].ToString ());
			Assert.AreEqual ("9", a [8, 9].ToString ());
			Assert.AreEqual ("789", a [-4, -1].ToString ());
			Assert.AreEqual ("7890", a [-4, 0].ToString ());
			Assert.AreEqual ("7890", a [-4, 0].ToString ());
			Assert.AreEqual ("234567", a [-9, -3].ToString ());
			Assert.AreEqual ("", a [100, 200].ToString ());
			Assert.AreEqual ("", a [-100, 0].ToString ());
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

			unsafe
			{
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
			id.Dispose ();
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
			Marshal.WriteByte (p, (byte) 'A');
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
	}
}
