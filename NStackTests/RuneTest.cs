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

		[Test]
		public void TestRune()
		{
			Rune a = new Rune('a');
			Assert.AreEqual("a", a.ToString());
			Rune b = new Rune(0x0061);
			Assert.AreEqual("a", b.ToString());
			Rune c = new Rune('\u0061');
			Assert.AreEqual("a", c.ToString());
			Rune d = new Rune(0x10421);
			Assert.AreEqual("𐐡", d.ToString());
			Assert.Throws<ArgumentOutOfRangeException>(() => new Rune('\ud799', '\udc21'));
			Rune e = new Rune('\ud801', '\udc21');
			Assert.AreEqual("𐐡", e.ToString());
			Assert.Throws<ArgumentException>(() => new Rune('\ud801'));
			Rune f = new Rune('\ud83c', '\udf39');
			Assert.AreEqual("🌹", f.ToString());
		}
	}
}
