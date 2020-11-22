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
		string h = "\U0001fa01";
		string i = "\U000e0fe1";
		Rune j = '\u20D0';
		Rune k = '\u25a0';
		Rune l = '\u25a1';
		Rune m = '\uf61e';
		byte[] n = new byte[4] { 0xf0, 0x9f, 0x8d, 0x95 }; // UTF-8 Encoding
		Rune o = new Rune('\ud83c', '\udf55'); // UTF-16 Encoding;
		string p = "\U0001F355"; // UTF-32 Encoding
		Rune q = '\u2103';
		Rune r = '\u1100';
		Rune s = '\u2501';

		[Test]
		public void TestColumnWidth()
		{
			Assert.AreEqual(1, Rune.ColumnWidth(a));
			Assert.AreEqual("a", a.ToString());
			Assert.AreEqual(1, a.ToString().Length);
			Assert.AreEqual(1, Rune.RuneLen(a));
			Assert.AreEqual(1, Rune.ColumnWidth(b));
			Assert.AreEqual("b", b.ToString());
			Assert.AreEqual(1, b.ToString().Length);
			Assert.AreEqual(1, Rune.RuneLen(b));
			var rl = a < b;
			Assert.IsTrue(rl);
			Assert.AreEqual(1, Rune.ColumnWidth(c));
			Assert.AreEqual("{", c.ToString());
			Assert.AreEqual(1, c.ToString().Length);
			Assert.AreEqual(1, Rune.RuneLen(c));
			Assert.AreEqual(2, Rune.ColumnWidth(d));
			Assert.AreEqual("ᅐ", d.ToString());
			Assert.AreEqual(1, d.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(d));
			Assert.AreEqual(0, Rune.ColumnWidth(e));
			Assert.AreEqual("ᅡ", e.ToString());
			Assert.AreEqual(1, e.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(e));
			Assert.AreEqual(-1, Rune.ColumnWidth(f));
			Assert.AreEqual(1, f.ToString().Length);
			Assert.AreEqual(1, Rune.RuneLen(f));
			Assert.AreEqual(-1, Rune.ColumnWidth(g));
			Assert.AreEqual(1, g.ToString().Length);
			Assert.AreEqual(1, Rune.RuneLen(g));
			var uh = ustring.Make(h);
			(var runeh, var sizeh) = Rune.DecodeRune(uh);
			Assert.AreEqual(2, Rune.ColumnWidth(runeh));
			Assert.AreEqual("🨁", h);
			Assert.AreEqual(2, runeh.ToString().Length);
			Assert.AreEqual(4, Rune.RuneLen(runeh));
			Assert.AreEqual(sizeh, Rune.RuneLen(runeh));
			for (int i = 0; i < uh.Length - 1; i++)
			{
				Assert.False(Rune.DecodeSurrogatePair(uh[i], uh[i + 1]) > 0);
			}
			Assert.IsTrue(Rune.ValidRune(runeh));
			Assert.True(Rune.Valid(uh.ToByteArray()));
			Assert.True(Rune.FullRune(uh.ToByteArray()));
			Assert.AreEqual(1, Rune.RuneCount(uh));
			(var runelh, var sizelh) = Rune.DecodeLastRune(uh);

			Assert.AreEqual(2, Rune.ColumnWidth(runelh));
			Assert.AreEqual(2, runelh.ToString().Length);
			Assert.AreEqual(4, Rune.RuneLen(runelh));
			Assert.AreEqual(sizelh, Rune.RuneLen(runelh));
			Assert.IsTrue(Rune.ValidRune(runelh));

			var ui = ustring.Make(i);
			(var runei, var sizei) = Rune.DecodeRune(ui);
			Assert.AreEqual(1, Rune.ColumnWidth(runei));
			Assert.AreEqual("󠿡", i);
			Assert.AreEqual(2, runei.ToString().Length);
			Assert.AreEqual(4, Rune.RuneLen(runei));
			Assert.AreEqual(sizei, Rune.RuneLen(runei));
			for (int i = 0; i < ui.Length - 1; i++)
			{
				Assert.False(Rune.DecodeSurrogatePair(ui[i], ui[i + 1]) > 0);
			}
			Assert.IsTrue(Rune.ValidRune(runei));
			Assert.True(Rune.Valid(ui.ToByteArray()));
			Assert.True(Rune.FullRune(ui.ToByteArray()));
			(var runeli, var sizeli) = Rune.DecodeLastRune(ui);
			Assert.AreEqual(1, Rune.ColumnWidth(runeli));
			Assert.AreEqual(2, runeli.ToString().Length);
			Assert.AreEqual(4, Rune.RuneLen(runeli));
			Assert.AreEqual(sizeli, Rune.RuneLen(runeli));
			Assert.IsTrue(Rune.ValidRune(runeli));

			Assert.AreNotEqual(Rune.ColumnWidth(runeh), Rune.ColumnWidth(runei));
			Assert.AreNotEqual(h, i);
			Assert.AreEqual(runeh.ToString().Length, runei.ToString().Length);
			Assert.AreEqual(Rune.RuneLen(runeh), Rune.RuneLen(runei));
			Assert.AreEqual(Rune.RuneLen(runeh), Rune.RuneLen(runei));
			var uj = ustring.Make(j);
			(var runej, var sizej) = Rune.DecodeRune(uj);
			Assert.AreEqual(0, Rune.ColumnWidth(j));
			Assert.AreEqual(0, Rune.ColumnWidth(uj.RuneAt (0)));
			Assert.AreEqual(j, uj.RuneAt(0));
			Assert.AreEqual("⃐", j.ToString());
			Assert.AreEqual("⃐", uj.ToString());
			Assert.AreEqual(1, j.ToString().Length);
			Assert.AreEqual(1, runej.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(j));
			Assert.AreEqual(sizej, Rune.RuneLen(runej));
			Assert.AreEqual(1, Rune.ColumnWidth(k));
			Assert.AreEqual("■", k.ToString());
			Assert.AreEqual(1, k.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(k));
			Assert.AreEqual(1, Rune.ColumnWidth(l));
			Assert.AreEqual("□", l.ToString());
			Assert.AreEqual(1, l.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(l));
			Assert.AreEqual(1, Rune.ColumnWidth(m));
			Assert.AreEqual("", m.ToString());
			Assert.AreEqual(1, m.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(m));
			var rn = Rune.DecodeRune(ustring.Make(n)).rune;
			Assert.AreEqual(1, Rune.ColumnWidth(rn));
			Assert.AreEqual("🍕", rn.ToString());
			Assert.AreEqual(2, rn.ToString().Length);
			Assert.AreEqual(4, Rune.RuneLen(rn));
			Assert.AreEqual(1, Rune.ColumnWidth(o));
			Assert.AreEqual("🍕", o.ToString());
			Assert.AreEqual(2, o.ToString().Length);
			Assert.AreEqual(4, Rune.RuneLen(o));
			var rp = Rune.DecodeRune(ustring.Make(p)).rune;
			Assert.AreEqual(1, Rune.ColumnWidth(rp));
			Assert.AreEqual("🍕", p);
			Assert.AreEqual(2, p.Length);
			Assert.AreEqual(4, Rune.RuneLen(rp));
			Assert.AreEqual(1, Rune.ColumnWidth(q));
			Assert.AreEqual("℃", q.ToString());
			Assert.AreEqual(1, q.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(q));
			var rq = Rune.DecodeRune(ustring.Make(q)).rune;
			Assert.AreEqual(1, Rune.ColumnWidth(rq));
			Assert.AreEqual("℃", rq.ToString());
			Assert.AreEqual(1, rq.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(rq));
			Assert.AreEqual(2, Rune.ColumnWidth(r));
			Assert.AreEqual("ᄀ", r.ToString());
			Assert.AreEqual(1, r.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(r));
			Assert.AreEqual(1, Rune.ColumnWidth(s));
			Assert.AreEqual("━", s.ToString());
			Assert.AreEqual(1, s.ToString().Length);
			Assert.AreEqual(3, Rune.RuneLen(s));
			var buff = new byte[4];
			var sb = Rune.EncodeRune('\u2503', buff);
			var scb = char.ConvertToUtf32("℃", 0);
			var scr = '℃'.ToString().Length;

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
			Assert.False(Rune.DecodeSurrogatePair('\ud799', '\udc21') > 0);
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
			var p = new Rune('\u2329');
			Assert.AreEqual(2, Rune.ColumnWidth(p));
			Assert.AreEqual(1, p.ToString().Length);
			Assert.AreEqual("〈", p.ToString());
			var q = new Rune('\u232a');
			Assert.AreEqual(2, Rune.ColumnWidth(q));
			Assert.AreEqual(1, q.ToString().Length);
			Assert.AreEqual("〉", q.ToString());
			var r = Rune.DecodeRune(ustring.Make("\U0000232a")).rune;
			Assert.AreEqual(2, Rune.ColumnWidth(r));
			Assert.AreEqual(1, r.ToString().Length);
			Assert.AreEqual("〉", r.ToString());

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

			foreach (Rune rune in us)
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
			string rs = "";

			foreach (Rune rune in us)
			{
				if (rune == Rune.Error)
				{
					throw new Exception("String was not well-formed UTF-16.");
				}
				Assert.IsTrue(Rune.ValidRune(rune));
				i += Rune.ColumnWidth(rune); // increment the iterator by the number of chars in this Rune
				rs += rune.ToString();
			}
			Assert.AreEqual(us.ConsoleWidth, i);
			Assert .AreEqual(s, rs);
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

		[Test]
		public void TestValidRune()
		{
			Assert.IsTrue(Rune.ValidRune(new Rune('\ud83c', '\udf39')));
			Assert.IsFalse(Rune.ValidRune((uint)'\ud801')); // To avoid throwing an exception pass as uint parameter instead Rune.
			Assert.Throws<ArgumentException>(() => Rune.ValidRune('\ud801'));
		}

		[Test]
		public void TestValid()
		{
			var rune1 = new Rune('\ud83c', '\udf39');
			var buff1 = new byte[4];
			Assert.AreEqual(4, Rune.EncodeRune(rune1, buff1));
			Assert.IsTrue(Rune.Valid(buff1));
			Assert.AreEqual(2, rune1.ToString().Length);
			Assert.AreEqual(4, Rune.RuneLen(rune1));
			var rune2 = (uint)'\ud801'; // To avoid throwing an exception set as uint instead a Rune instance.
			var buff2 = new byte[4];
			Assert.AreEqual(3, Rune.EncodeRune(rune2, buff2));
			Assert.IsFalse(Rune.Valid(buff2)); // To avoid throwing an exception pass as uint parameter instead Rune.
			Assert.AreEqual(5, rune2.ToString().Length); // Invalid string. It returns the decimal 55297 representation of the 0xd801 hexadecimal.
			Assert.AreEqual(-1, Rune.RuneLen(rune2));
			Assert.Throws<ArgumentException>(() => Rune.EncodeRune(new Rune('\ud801'), buff2));
		}
	}
}
