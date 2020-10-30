using NStack;
using NUnit.Framework;
using System;
using System.Globalization;

namespace NStackTests
{
	public class RuneTest
	{
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
			Assert.AreEqual(1, Rune.ColumnWidth(a));
			Assert.AreEqual("a", a.ToString());
			Assert.AreEqual(1, Rune.ColumnWidth(b));
			Assert.AreEqual("b", b.ToString());
			var l = a < b;
			Assert.IsTrue(l);
			Assert.AreEqual(1, Rune.ColumnWidth(c));
			Assert.AreEqual("{", c.ToString());
			Assert.AreEqual(2, Rune.ColumnWidth(d));
			Assert.AreEqual("ᅐ", d.ToString());
			Assert.AreEqual(0, Rune.ColumnWidth(e));
			Assert.AreEqual("ᅡ", e.ToString());
			Assert.AreEqual(-1, Rune.ColumnWidth(f));
			Assert.AreEqual(1, f.ToString().Length);
			Assert.AreEqual(-1, Rune.ColumnWidth(g));
			Assert.AreEqual(1, g.ToString().Length);
		}

		[Test]
		public void TestRune()
		{
			Rune a = new Rune('a');
			Assert.AreEqual(1, Rune.ColumnWidth(a));
			Assert.AreEqual(1, a.ToString().Length);
			Assert.AreEqual("a", a.ToString());
			Rune b = new Rune(0x0061);
			Assert.AreEqual(1, Rune.ColumnWidth(b));
			Assert.AreEqual(1, b.ToString().Length);
			Assert.AreEqual("a", b.ToString());
			Rune c = new Rune('\u0061');
			Assert.AreEqual(1, Rune.ColumnWidth(c));
			Assert.AreEqual(1, c.ToString().Length);
			Assert.AreEqual("a", c.ToString());
			Rune d = new Rune(0x10421);
			Assert.AreEqual(1, Rune.ColumnWidth(d));
			Assert.AreEqual(2, d.ToString().Length);
			Assert.AreEqual("𐐡", d.ToString());
			Assert.Throws<ArgumentOutOfRangeException>(() => new Rune('\ud799', '\udc21'));
			Rune e = new Rune('\ud801', '\udc21');
			Assert.AreEqual(1, Rune.ColumnWidth(e));
			Assert.AreEqual(2, e.ToString().Length);
			Assert.AreEqual("𐐡", e.ToString());
			Assert.Throws<ArgumentException>(() => new Rune('\ud801'));
			Rune f = new Rune('\ud83c', '\udf39');
			Assert.AreEqual(1, Rune.ColumnWidth(f));
			Assert.AreEqual(2, f.ToString().Length);
			Assert.AreEqual("🌹", f.ToString());
			Assert.DoesNotThrow(() => new Rune(0x10ffff));
			var g = new Rune(0x10ffff);
			Assert.AreEqual(1, Rune.ColumnWidth(g));
			Assert.AreEqual(2, g.ToString().Length);
			Assert.AreEqual("􏿿", g.ToString());
			Assert.Throws<ArgumentOutOfRangeException>(() => new Rune(0x12345678));
			var h = new Rune('\u1150');
			Assert.AreEqual(2, Rune.ColumnWidth(h));
			Assert.AreEqual(1, h.ToString().Length);
			Assert.AreEqual("ᅐ", h.ToString());
			var i = new Rune('\u4F60');
			Assert.AreEqual(2, Rune.ColumnWidth(i));
			Assert.AreEqual(1, i.ToString().Length);
			Assert.AreEqual("你", i.ToString());
			var j = new Rune('\u597D');
			Assert.AreEqual(2, Rune.ColumnWidth(j));
			Assert.AreEqual(1, j.ToString().Length);
			Assert.AreEqual("好", j.ToString());
			var k = new Rune('\ud83d', '\udc02');
			Assert.AreEqual(1, Rune.ColumnWidth(k));
			Assert.AreEqual(2, k.ToString().Length);
			Assert.AreEqual("🐂", k.ToString());
			var l = new Rune('\ud801', '\udcbb');
			Assert.AreEqual(1, Rune.ColumnWidth(l));
			Assert.AreEqual(2, l.ToString().Length);
			Assert.AreEqual("𐒻", l.ToString());
			var m = new Rune('\ud801', '\udccf');
			Assert.AreEqual(1, Rune.ColumnWidth(m));
			Assert.AreEqual(2, m.ToString().Length);
			Assert.AreEqual("𐓏", m.ToString());
			var n = new Rune('\u00e1');
			Assert.AreEqual(1, Rune.ColumnWidth(n));
			Assert.AreEqual(1, n.ToString().Length);
			Assert.AreEqual("á", n.ToString());
			var o = new Rune('\ud83d', '\udd2e');
			Assert.AreEqual(1, Rune.ColumnWidth(o));
			Assert.AreEqual(2, o.ToString().Length);
			Assert.AreEqual("🔮", o.ToString());

			PrintTextElementCount(ustring.Make('\u00e1'), "á", 1, 1, 1, 1);
			PrintTextElementCount(ustring.Make('\u0061', '\u0301'), "á", 1, 2, 2, 1);
			PrintTextElementCount(ustring.Make(new Rune[] { new Rune(0x1f469), new Rune(0x1f3fd), new Rune('\u200d'), new Rune(0x1f692) }),
				"👩🏽‍🚒", 3, 4, 7, 4);
			PrintTextElementCount(ustring.Make(new Rune[] { new Rune(0x1f469), new Rune(0x1f3fd), new Rune('\u200d'), new Rune(0x1f692) }),
				"\U0001f469\U0001f3fd\u200d\U0001f692", 3, 4, 7, 4);
			PrintTextElementCount(ustring.Make(new Rune('\ud801', '\udccf')),
				"\ud801\udccf", 1, 1, 2, 1);
		}

		void PrintTextElementCount(ustring us, string s, int consoleWidth, int runeCount, int stringCount, int txtElementCount)
		{
			Assert.AreEqual(us.ToString(), s);
			Assert.AreEqual(s, us.ToString());
			Assert.AreEqual(consoleWidth, us.ConsoleWidth);
			Assert.AreEqual(runeCount, us.RuneCount);
			Assert.AreEqual(stringCount, s.Length);

			TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(s);

			int textElementCount = 0;
			while (enumerator.MoveNext())
			{
				textElementCount++; // For versions prior to Net5.0 the StringInfo class might handle some grapheme clusters incorrectly.
			}

			Assert.AreEqual(txtElementCount, textElementCount);
		}

		[Test]
		public void TestRuneIsLetter()
		{
			Assert.AreEqual(5, CountLettersInString("Hello"));
			Assert.AreEqual(8, CountLettersInString("𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟"));
		}

		int CountLettersInString(string s)
		{
			int letterCount = 0;
			var us = ustring.Make(s);

			foreach (Rune rune in us.ToRunes())
			{
				if (Rune.IsLetter(rune))
				{ letterCount++; }
			}

			return letterCount;
		}

		[Test]
		public void TestSurrogatePair()
		{
			Assert.IsTrue(ProcessTestStringUseChar("𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟"));
			Assert.Throws<Exception>(() => ProcessTestStringUseChar("\ud801"));

			Assert.IsTrue(ProcessStringUseRune("𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟"));
			Assert.Throws<Exception>(() => ProcessStringUseRune("\ud801"));
		}

		bool ProcessTestStringUseChar(string s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (!char.IsSurrogate(s[i]))
				{
					var buff = new byte[4];
					Rune.EncodeRune(s[i], buff);
					Assert.AreEqual((int)(s[i]), buff[0]);
				}
				else if (i + 1 < s.Length && char.IsSurrogatePair(s[i], s[i + 1]))
				{
					int codePoint = char.ConvertToUtf32(s[i], s[i + 1]);
					Assert.AreEqual(codePoint, Rune.DecodeSurrogatePair(s[i], s[i + 1]));
					i++; // so that when the loop iterates it's actually +2
				}
				else
				{
					throw new Exception("String was not well-formed UTF-16.");
				}
			}
			return true;
		}

		bool ProcessStringUseRune(string s)
		{
			var us = ustring.Make(s);
			int i = 0;

			foreach (Rune rune in us.ToRunes())
			{
				if (rune == Rune.Error)
				{
					throw new Exception("String was not well-formed UTF-16.");
				}
				Assert.IsTrue(Rune.ValidRune(rune));
				i += Rune.ColumnWidth(rune); // increment the iterator by the number of chars in this Rune
			}
			Assert.AreEqual(us.ConsoleWidth, i);
			return true;
		}

		[Test]
		public void TestSplit()
		{
			string inputString = "🐂, 🐄, 🐆";
			string[] splitOnSpace = inputString.Split(' ');
			string[] splitOnComma = inputString.Split(',');
			Assert.AreEqual(3, splitOnSpace.Length);
			Assert.AreEqual(3, splitOnComma.Length);
		}
	}
}
