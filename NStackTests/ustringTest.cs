// 
// ustringtest: ustring tests
//
//﻿ Miguel de Icaza
//
using NUnit.Framework;
using System;
using NStack;

namespace NStackTests
{
	[TestFixture]
	public class UstringTest
	{
		bstring a = new bstring ("a");
		bstring seconda = new bstring ("a");
		bstring aa = new bstring ("aa");
		bstring b = new bstring ("b");
		bstring bb = new bstring ("bb");
		bstring empty = new bstring ("");
		bstring secondempty = new bstring ("");

		bstring hello = new bstring ("hello, world");
		bstring longhello = new bstring ("");
		bstring kosme = new bstring (0xce, 0xba, 0xcf, 0x8c, 0xcf, 0x83, 0xce, 0xbc, 0xce, 0xb5);

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

	}
}
