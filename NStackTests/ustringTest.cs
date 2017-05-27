// 
// ustringtest: ustring tests
//
//﻿ Miguel de Icaza
//
using NUnit.Framework;
using System;
using NStack;
using System.Runtime.InteropServices;

namespace NStackTests
{
	[TestFixture]
	public class UstringTest
	{
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
			Assert.AreNotEqual (a, b);
			Assert.AreNotEqual (b, a);
			Assert.AreNotEqual (a, aa);
			Assert.AreNotEqual (aa, a);
			Assert.AreEqual (empty, empty);
			Assert.AreEqual (empty, secondempty);

		}

		[Test]
		public void Contains ()
		{
			Assert.IsTrue (aa.Contains (a));
			Assert.IsFalse (aa.Contains (b));
			Assert.IsTrue (bb.Contains (b));
		}

		[Test]
		public void IndexOf ()
		{

		}

		[Test]
		public void Length ()
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
			SliceTests (ustring.Make ("x" + str + "x") [1,11]);

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
			Action<ustring, IntPtr> releaseFunc = (str, block) => {
				released = true;
			};
			var ptr = Marshal.AllocHGlobal (10);
			var s = ustring.Make (ptr, 10, releaseFunc);
			Assert.True (s is IDisposable);
			var id = s as IDisposable;
			id.Dispose ();
			Assert.True (released);
		}
	}
}
