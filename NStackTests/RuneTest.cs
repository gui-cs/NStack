using NUnit.Framework;
using System;
namespace NStackTests {
	public class RuneTest {
		Rune a = 'a';
		Rune b = 'b';
		Rune c = 123;
		Rune d = '\u1150';  // 0x1150	ᅐ	Unicode Technical Report #11
		Rune e = '\u1161';  // 0x1161	ᅡ	null character with column equal to 0
		Rune f = 31;    // non printable character
		Rune g = 127;   // non printable character

		[Test]
		public void TestColumnWidth()
		{
			var rt = new RuneTest();

			Assert.AreEqual(1, Rune.ColumnWidth(rt.a));
			Assert.AreEqual(1, Rune.ColumnWidth(rt.b));
			var l = a < b;
			Assert.IsTrue(l);
			Assert.AreEqual(1, Rune.ColumnWidth(rt.c));
			Assert.AreEqual(2, Rune.ColumnWidth(rt.d));
			Assert.AreEqual(0, Rune.ColumnWidth(rt.e));
			Assert.AreEqual(-1, Rune.ColumnWidth(rt.f));
			Assert.AreEqual(-1, Rune.ColumnWidth(rt.g));
		}
	}
}
