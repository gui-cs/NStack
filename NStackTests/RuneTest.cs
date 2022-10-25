using NStack;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NStackTests {
	public class RuneTest {
		[Test]
		public void TestColumnWidth ()
		{
			Rune a = 'a';
			Rune b = 'b';
			Rune c = 123;
			Rune d = '\u1150';  // 0x1150	ᅐ	Unicode Technical Report #11
			Rune e = '\u1161';  // 0x1161	ᅡ	Unicode Hangul Jamo for join with column width equal to 0 alone.
			Rune f = 31;    // non printable character
			Rune g = 127;   // non printable character
			string h = "\U0001fa01";
			string i = "\U000e0fe1";
			Rune j = '\u20D0';
			Rune k = '\u25a0';
			Rune l = '\u25a1';
			Rune m = '\uf61e';
			byte [] n = new byte [4] { 0xf0, 0x9f, 0x8d, 0x95 }; // UTF-8 Encoding
			Rune o = new Rune ('\ud83c', '\udf55'); // UTF-16 Encoding;
			string p = "\U0001F355"; // UTF-32 Encoding
			Rune q = '\u2103';
			Rune r = '\u1100';
			Rune s = '\u2501';

			Assert.AreEqual (1, Rune.ColumnWidth (a));
			Assert.AreEqual ("a", a.ToString ());
			Assert.AreEqual (1, a.ToString ().Length);
			Assert.AreEqual (1, Rune.RuneLen (a));
			Assert.AreEqual (1, Rune.ColumnWidth (b));
			Assert.AreEqual ("b", b.ToString ());
			Assert.AreEqual (1, b.ToString ().Length);
			Assert.AreEqual (1, Rune.RuneLen (b));
			var rl = a < b;
			Assert.IsTrue (rl);
			Assert.AreEqual (1, Rune.ColumnWidth (c));
			Assert.AreEqual ("{", c.ToString ());
			Assert.AreEqual (1, c.ToString ().Length);
			Assert.AreEqual (1, Rune.RuneLen (c));
			Assert.AreEqual (2, Rune.ColumnWidth (d));
			Assert.AreEqual ("ᅐ", d.ToString ());
			Assert.AreEqual (1, d.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (d));
			Assert.AreEqual (0, Rune.ColumnWidth (e));
			string join = "\u1104\u1161";
			Assert.AreEqual ("따", join);
			Assert.AreEqual (2, join.Sum (x => Rune.ColumnWidth (x)));
			Assert.IsFalse (Rune.DecodeSurrogatePair (join, out _));
			Assert.AreEqual (2, ((ustring)join).RuneCount);
			Assert.AreEqual (2, join.Length);
			Assert.AreEqual ("ᅡ", e.ToString ());
			Assert.AreEqual (1, e.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (e));
			string joinNormalize = join.Normalize ();
			Assert.AreEqual ("따", joinNormalize);
			Assert.AreEqual (2, joinNormalize.Sum (x => Rune.ColumnWidth (x)));
			Assert.IsFalse (Rune.DecodeSurrogatePair (joinNormalize, out _));
			Assert.AreEqual (1, ((ustring)joinNormalize).RuneCount);
			Assert.AreEqual (1, joinNormalize.Length);
			Assert.AreEqual (-1, Rune.ColumnWidth (f));
			Assert.AreEqual (1, f.ToString ().Length);
			Assert.AreEqual (1, Rune.RuneLen (f));
			Assert.AreEqual (-1, Rune.ColumnWidth (g));
			Assert.AreEqual (1, g.ToString ().Length);
			Assert.AreEqual (1, Rune.RuneLen (g));
			var uh = ustring.Make (h);
			(var runeh, var sizeh) = uh.DecodeRune ();
			Assert.AreEqual (2, Rune.ColumnWidth (runeh));
			Assert.AreEqual ("🨁", h);
			Assert.AreEqual (2, runeh.ToString ().Length);
			Assert.AreEqual (4, Rune.RuneLen (runeh));
			Assert.AreEqual (sizeh, Rune.RuneLen (runeh));
			for (int x = 0; x < uh.Length - 1; x++) {
				Assert.False (Rune.EncodeSurrogatePair (uh [x], uh [x + 1], out _));
			}
			Assert.IsTrue (Rune.ValidRune (runeh));
			Assert.True (Rune.Valid (uh.ToByteArray ()));
			Assert.True (Rune.FullRune (uh.ToByteArray ()));
			Assert.AreEqual (1, uh.RuneCount ());
			(var runelh, var sizelh) = uh.DecodeLastRune ();

			Assert.AreEqual (2, Rune.ColumnWidth (runelh));
			Assert.AreEqual (2, runelh.ToString ().Length);
			Assert.AreEqual (4, Rune.RuneLen (runelh));
			Assert.AreEqual (sizelh, Rune.RuneLen (runelh));
			Assert.IsTrue (Rune.ValidRune (runelh));

			var ui = ustring.Make (i);
			(var runei, var sizei) = ui.DecodeRune ();
			Assert.AreEqual (1, Rune.ColumnWidth (runei));
			Assert.AreEqual ("󠿡", i);
			Assert.AreEqual (2, runei.ToString ().Length);
			Assert.AreEqual (4, Rune.RuneLen (runei));
			Assert.AreEqual (sizei, Rune.RuneLen (runei));
			for (int x = 0; x < ui.Length - 1; x++) {
				Assert.False (Rune.EncodeSurrogatePair (ui [x], ui [x + 1], out _));
			}
			Assert.IsTrue (Rune.ValidRune (runei));
			Assert.True (Rune.Valid (ui.ToByteArray ()));
			Assert.True (Rune.FullRune (ui.ToByteArray ()));
			(var runeli, var sizeli) = ui.DecodeLastRune ();
			Assert.AreEqual (1, Rune.ColumnWidth (runeli));
			Assert.AreEqual (2, runeli.ToString ().Length);
			Assert.AreEqual (4, Rune.RuneLen (runeli));
			Assert.AreEqual (sizeli, Rune.RuneLen (runeli));
			Assert.IsTrue (Rune.ValidRune (runeli));

			Assert.AreNotEqual (Rune.ColumnWidth (runeh), Rune.ColumnWidth (runei));
			Assert.AreNotEqual (h, i);
			Assert.AreEqual (runeh.ToString ().Length, runei.ToString ().Length);
			Assert.AreEqual (Rune.RuneLen (runeh), Rune.RuneLen (runei));
			Assert.AreEqual (Rune.RuneLen (runeh), Rune.RuneLen (runei));
			var uj = ustring.Make (j);
			(var runej, var sizej) = uj.DecodeRune ();
			Assert.AreEqual (0, Rune.ColumnWidth (j));
			Assert.AreEqual (0, Rune.ColumnWidth (uj.RuneAt (0)));
			Assert.AreEqual (j, uj.RuneAt (0));
			Assert.AreEqual ("⃐", j.ToString ());
			Assert.AreEqual ("⃐", uj.ToString ());
			Assert.AreEqual (1, j.ToString ().Length);
			Assert.AreEqual (1, runej.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (j));
			Assert.AreEqual (sizej, Rune.RuneLen (runej));
			Assert.AreEqual (1, Rune.ColumnWidth (k));
			Assert.AreEqual ("■", k.ToString ());
			Assert.AreEqual (1, k.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (k));
			Assert.AreEqual (1, Rune.ColumnWidth (l));
			Assert.AreEqual ("□", l.ToString ());
			Assert.AreEqual (1, l.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (l));
			Assert.AreEqual (1, Rune.ColumnWidth (m));
			Assert.AreEqual ("", m.ToString ());
			Assert.AreEqual (1, m.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (m));
			var rn = ustring.Make (n).DecodeRune ().rune;
			Assert.AreEqual (1, Rune.ColumnWidth (rn));
			Assert.AreEqual ("🍕", rn.ToString ());
			Assert.AreEqual (2, rn.ToString ().Length);
			Assert.AreEqual (4, Rune.RuneLen (rn));
			Assert.AreEqual (1, Rune.ColumnWidth (o));
			Assert.AreEqual ("🍕", o.ToString ());
			Assert.AreEqual (2, o.ToString ().Length);
			Assert.AreEqual (4, Rune.RuneLen (o));
			var rp = ustring.Make (p).DecodeRune ().rune;
			Assert.AreEqual (1, Rune.ColumnWidth (rp));
			Assert.AreEqual ("🍕", p);
			Assert.AreEqual (2, p.Length);
			Assert.AreEqual (4, Rune.RuneLen (rp));
			Assert.AreEqual (1, Rune.ColumnWidth (q));
			Assert.AreEqual ("℃", q.ToString ());
			Assert.AreEqual (1, q.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (q));
			var rq = ustring.Make (q).DecodeRune ().rune;
			Assert.AreEqual (1, Rune.ColumnWidth (rq));
			Assert.AreEqual ("℃", rq.ToString ());
			Assert.AreEqual (1, rq.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (rq));
			Assert.AreEqual (2, Rune.ColumnWidth (r));
			Assert.AreEqual ("ᄀ", r.ToString ());
			Assert.AreEqual (1, r.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (r));
			Assert.AreEqual (1, Rune.ColumnWidth (s));
			Assert.AreEqual ("━", s.ToString ());
			Assert.AreEqual (1, s.ToString ().Length);
			Assert.AreEqual (3, Rune.RuneLen (s));
			var buff = new byte [4];
			var sb = Rune.EncodeRune ('\u2503', buff);
			Assert.AreEqual (1, Rune.ColumnWidth ('\u2503'));
			(var rune, var size) = ustring.Make ('\u2503').DecodeRune ();
			Assert.AreEqual (sb, size);
			Assert.AreEqual ('\u2503', (uint)rune);
			var scb = char.ConvertToUtf32 ("℃", 0);
			var scr = '℃'.ToString ().Length;
			Assert.AreEqual (scr, Rune.ColumnWidth ((uint)scb));
			buff = new byte [4];
			sb = Rune.EncodeRune ('\u1100', buff);
			Assert.AreEqual (2, Rune.ColumnWidth ('\u1100'));
			Assert.AreEqual (2, ustring.Make ('\u1100').ConsoleWidth);
			Assert.AreEqual (1, '\u1100'.ToString ().Length); // Length as string returns 1 but in reality it occupies 2 columns.
			(rune, size) = ustring.Make ('\u1100').DecodeRune ();
			Assert.AreEqual (sb, size);
			Assert.AreEqual ('\u1100', (uint)rune);
			string str = "\u2615";
			Assert.AreEqual ("☕", str);
			Assert.AreEqual (2, str.Sum (x => Rune.ColumnWidth (x)));
			Assert.AreEqual (2, ((ustring)str).ConsoleWidth);
			Assert.AreEqual (1, ((ustring)str).RuneCount ());
			Assert.AreEqual (1, str.Length);
			str = "\u2615\ufe0f"; // Identical but \ufe0f forces it to be rendered as a colorful image as compared to a monochrome text variant.
			Assert.AreEqual ("☕️", str);
			Assert.AreEqual (2, str.Sum (x => Rune.ColumnWidth (x)));
			Assert.AreEqual (2, ((ustring)str).ConsoleWidth);
			Assert.AreEqual (2, ((ustring)str).RuneCount ());
			Assert.AreEqual (2, str.Length);
			str = "\u231a";
			Assert.AreEqual ("⌚", str);
			Assert.AreEqual (2, str.Sum (x => Rune.ColumnWidth (x)));
			Assert.AreEqual (2, ((ustring)str).ConsoleWidth);
			Assert.AreEqual (1, ((ustring)str).RuneCount ());
			Assert.AreEqual (1, str.Length);
			str = "\u231b";
			Assert.AreEqual ("⌛", str);
			Assert.AreEqual (2, str.Sum (x => Rune.ColumnWidth (x)));
			Assert.AreEqual (2, ((ustring)str).ConsoleWidth);
			Assert.AreEqual (1, ((ustring)str).RuneCount ());
			Assert.AreEqual (1, str.Length);
			str = "\u231c";
			Assert.AreEqual ("⌜", str);
			Assert.AreEqual (1, str.Sum (x => Rune.ColumnWidth (x)));
			Assert.AreEqual (1, ((ustring)str).ConsoleWidth);
			Assert.AreEqual (1, ((ustring)str).RuneCount ());
			Assert.AreEqual (1, str.Length);
			str = "\u1dc0";
			Assert.AreEqual ("᷀", str);
			Assert.AreEqual (0, str.Sum (x => Rune.ColumnWidth (x)));
			Assert.AreEqual (0, ((ustring)str).ConsoleWidth);
			Assert.AreEqual (1, ((ustring)str).RuneCount ());
			Assert.AreEqual (1, str.Length);
			str = "\ud83e\udd16";
			Assert.AreEqual ("🤖", str);
			Assert.AreEqual (2, str.Sum (x => Rune.ColumnWidth (x)));
			Assert.AreEqual (2, ((ustring)str).ConsoleWidth);
			Assert.AreEqual (1, ((ustring)str).RuneCount ()); // Here returns 1 because is a valid surrogate pair resulting in only rune >=U+10000..U+10FFFF
			Assert.AreEqual (2, str.Length); // String always preserves the originals values of each surrogate pair
			str = "\U0001f9e0";
			Assert.AreEqual ("🧠", str);
			Assert.AreEqual (2, str.Sum (x => Rune.ColumnWidth (x)));
			Assert.AreEqual (2, ((ustring)str).ConsoleWidth);
			Assert.AreEqual (1, ((ustring)str).RuneCount ());
			Assert.AreEqual (2, str.Length);
		}

		[Test]
		public void TestRune ()
		{
			Rune a = new Rune ('a');
			Assert.AreEqual (1, Rune.ColumnWidth (a));
			Assert.AreEqual (1, a.ToString ().Length);
			Assert.AreEqual ("a", a.ToString ());
			Rune b = new Rune (0x0061);
			Assert.AreEqual (1, Rune.ColumnWidth (b));
			Assert.AreEqual (1, b.ToString ().Length);
			Assert.AreEqual ("a", b.ToString ());
			Rune c = new Rune ('\u0061');
			Assert.AreEqual (1, Rune.ColumnWidth (c));
			Assert.AreEqual (1, c.ToString ().Length);
			Assert.AreEqual ("a", c.ToString ());
			Rune d = new Rune (0x10421);
			Assert.AreEqual (1, Rune.ColumnWidth (d));
			Assert.AreEqual (2, d.ToString ().Length);
			Assert.AreEqual ("𐐡", d.ToString ());
			Assert.False (Rune.EncodeSurrogatePair ('\ud799', '\udc21', out _));
			Assert.Throws<ArgumentOutOfRangeException> (() => new Rune ('\ud799', '\udc21'));
			Rune e = new Rune ('\ud801', '\udc21');
			Assert.AreEqual (1, Rune.ColumnWidth (e));
			Assert.AreEqual (2, e.ToString ().Length);
			Assert.AreEqual ("𐐡", e.ToString ());
			Assert.False (new Rune ('\ud801').IsValid);
			Rune f = new Rune ('\ud83c', '\udf39');
			Assert.AreEqual (1, Rune.ColumnWidth (f));
			Assert.AreEqual (2, f.ToString ().Length);
			Assert.AreEqual ("🌹", f.ToString ());
			Assert.DoesNotThrow (() => new Rune (0x10ffff));
			Rune g = new Rune (0x10ffff);
			string s = "\U0010ffff";
			Assert.AreEqual (1, Rune.ColumnWidth (g));
			Assert.AreEqual (1, ustring.Make (s).ConsoleWidth);
			Assert.AreEqual (2, g.ToString ().Length);
			Assert.AreEqual (2, s.Length);
			Assert.AreEqual ("􏿿", g.ToString ());
			Assert.AreEqual ("􏿿", s);
			Assert.AreEqual (g.ToString (), s);
			Assert.Throws<ArgumentOutOfRangeException> (() => new Rune (0x12345678));
			var h = new Rune ('\u1150');
			Assert.AreEqual (2, Rune.ColumnWidth (h));
			Assert.AreEqual (1, h.ToString ().Length);
			Assert.AreEqual ("ᅐ", h.ToString ());
			var i = new Rune ('\u4F60');
			Assert.AreEqual (2, Rune.ColumnWidth (i));
			Assert.AreEqual (1, i.ToString ().Length);
			Assert.AreEqual ("你", i.ToString ());
			var j = new Rune ('\u597D');
			Assert.AreEqual (2, Rune.ColumnWidth (j));
			Assert.AreEqual (1, j.ToString ().Length);
			Assert.AreEqual ("好", j.ToString ());
			var k = new Rune ('\ud83d', '\udc02');
			Assert.AreEqual (1, Rune.ColumnWidth (k));
			Assert.AreEqual (2, k.ToString ().Length);
			Assert.AreEqual ("🐂", k.ToString ());
			var l = new Rune ('\ud801', '\udcbb');
			Assert.AreEqual (1, Rune.ColumnWidth (l));
			Assert.AreEqual (2, l.ToString ().Length);
			Assert.AreEqual ("𐒻", l.ToString ());
			var m = new Rune ('\ud801', '\udccf');
			Assert.AreEqual (1, Rune.ColumnWidth (m));
			Assert.AreEqual (2, m.ToString ().Length);
			Assert.AreEqual ("𐓏", m.ToString ());
			var n = new Rune ('\u00e1');
			Assert.AreEqual (1, Rune.ColumnWidth (n));
			Assert.AreEqual (1, n.ToString ().Length);
			Assert.AreEqual ("á", n.ToString ());
			var o = new Rune ('\ud83d', '\udd2e');
			Assert.AreEqual (1, Rune.ColumnWidth (o));
			Assert.AreEqual (2, o.ToString ().Length);
			Assert.AreEqual ("🔮", o.ToString ());
			var p = new Rune ('\u2329');
			Assert.AreEqual (2, Rune.ColumnWidth (p));
			Assert.AreEqual (1, p.ToString ().Length);
			Assert.AreEqual ("〈", p.ToString ());
			var q = new Rune ('\u232a');
			Assert.AreEqual (2, Rune.ColumnWidth (q));
			Assert.AreEqual (1, q.ToString ().Length);
			Assert.AreEqual ("〉", q.ToString ());
			var r = ustring.Make ("\U0000232a").DecodeRune ().rune;
			Assert.AreEqual (2, Rune.ColumnWidth (r));
			Assert.AreEqual (1, r.ToString ().Length);
			Assert.AreEqual ("〉", r.ToString ());

			PrintTextElementCount (ustring.Make ('\u00e1'), "á", 1, 1, 1, 1);
			PrintTextElementCount (ustring.Make ('\u0061', '\u0301'), "á", 1, 2, 2, 1);
			PrintTextElementCount (ustring.Make ('\u0065', '\u0301'), "é", 1, 2, 2, 1);
			PrintTextElementCount (ustring.Make (new Rune [] { new Rune (0x1f469), new Rune (0x1f3fd), new Rune ('\u200d'), new Rune (0x1f692) }),
				"👩🏽‍🚒", 3, 4, 7, 1);
			PrintTextElementCount (ustring.Make (new Rune [] { new Rune (0x1f469), new Rune (0x1f3fd), new Rune ('\u200d'), new Rune (0x1f692) }),
				"\U0001f469\U0001f3fd\u200d\U0001f692", 3, 4, 7, 1);
			PrintTextElementCount (ustring.Make (new Rune ('\ud801', '\udccf')),
				"\ud801\udccf", 1, 1, 2, 1);
		}

		void PrintTextElementCount (ustring us, string s, int consoleWidth, int runeCount, int stringCount, int txtElementCount)
		{
			Assert.AreNotEqual (us.Length, s.Length);
			Assert.AreEqual (us.ToString (), s);
			Assert.AreEqual (consoleWidth, us.ConsoleWidth);
			Assert.AreEqual (runeCount, us.RuneCount);
			Assert.AreEqual (stringCount, s.Length);

			TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator (s);

			int textElementCount = 0;
			while (enumerator.MoveNext ()) {
				textElementCount++; // For versions prior to Net5.0 the StringInfo class might handle some grapheme clusters incorrectly.
			}

			Assert.AreEqual (txtElementCount, textElementCount);
		}

		[Test]
		public void TestRuneIsLetter ()
		{
			Assert.AreEqual (5, CountLettersInString ("Hello"));
			Assert.AreEqual (8, CountLettersInString ("𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟"));
		}

		int CountLettersInString (string s)
		{
			int letterCount = 0;
			var us = ustring.Make (s);

			foreach (Rune rune in us) {
				if (Rune.IsLetter (rune)) { letterCount++; }
			}

			return letterCount;
		}

		[Test]
		public void Test_SurrogatePair_From_String ()
		{
			Assert.IsTrue (ProcessTestStringUseChar ("𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟"));
			Assert.Throws<Exception> (() => ProcessTestStringUseChar ("\ud801"));

			Assert.IsTrue (ProcessStringUseRune ("𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟"));
			Assert.Throws<Exception> (() => ProcessStringUseRune ("\ud801"));
		}

		bool ProcessTestStringUseChar (string s)
		{
			for (int i = 0; i < s.Length; i++) {
				Rune r = new Rune (s [i]);
				if (!char.IsSurrogate (s [i])) {
					var buff = new byte [4];
					Rune.EncodeRune (s [i], buff);
					Assert.AreEqual ((int)(s [i]), buff [0]);
					Assert.AreEqual (s [i], r.Value);
					Assert.True (r.IsValid);
					Assert.False (r.IsSurrogatePair);
				} else if (i + 1 < s.Length && char.IsSurrogatePair (s [i], s [i + 1])) {
					int codePoint = char.ConvertToUtf32 (s [i], s [i + 1]);
					Rune.EncodeSurrogatePair (s [i], s [i + 1], out Rune rune);
					Assert.AreEqual (codePoint, rune.Value);
					string sp = new string (new char [] { s [i], s [i + 1] });
					r = (uint)codePoint;
					Assert.AreEqual (sp, r.ToString ());
					Assert.True (r.IsSurrogatePair);
					i++; // Increment the iterator by the number of surrogate pair
				} else {
					Assert.False (r.IsValid);
					throw new Exception ("String was not well-formed UTF-16.");
				}
			}
			return true;
		}

		bool ProcessStringUseRune (string s)
		{
			var us = ustring.Make (s);
			string rs = "";
			Rune codePoint;
			List<Rune> runes = new List<Rune> ();
			int colWidth = 0;

			for (int i = 0; i < s.Length; i++) {
				Rune rune = new Rune (s [i]);
				if (rune.IsValid) {
					Assert.IsTrue (Rune.ValidRune (rune));
					runes.Add (rune);
					Assert.AreEqual ((uint)s [i], (uint)rune);
					Assert.False (rune.IsSurrogatePair);
				} else if (i + 1 < s.Length && (Rune.EncodeSurrogatePair (s [i], s [i + 1], out codePoint))) {
					Assert.IsFalse (Rune.ValidRune (rune));
					rune = codePoint;
					runes.Add (rune);
					string sp = new string (new char [] { s [i], s [i + 1] });
					Assert.AreEqual (sp, codePoint.ToString ());
					Assert.True (codePoint.IsSurrogatePair);
					i++; // Increment the iterator by the number of surrogate pair
				} else {
					Assert.False (rune.IsValid);
					throw new Exception ("String was not well-formed UTF-16.");
				}
				colWidth += Rune.ColumnWidth (rune); // Increment the column width of this Rune
				rs += rune.ToString ();
			}
			Assert.AreEqual (us.ConsoleWidth, colWidth);
			Assert.AreEqual (s, rs);
			Assert.AreEqual (s, ustring.Make (runes).ToString ());
			return true;
		}

		[Test]
		public void TestSplit ()
		{
			string inputString = "🐂, 🐄, 🐆";
			string [] splitOnSpace = inputString.Split (' ');
			string [] splitOnComma = inputString.Split (',');
			Assert.AreEqual (3, splitOnSpace.Length);
			Assert.AreEqual (3, splitOnComma.Length);
		}

		[Test]
		public void TestValidRune ()
		{
			Assert.IsTrue (Rune.ValidRune (new Rune ('\u1100')));
			Assert.IsTrue (Rune.ValidRune (new Rune ('\ud83c', '\udf39')));
			Assert.IsFalse (Rune.ValidRune ('\ud801'));
			Assert.IsFalse (Rune.ValidRune ((uint)'\ud801'));
			Assert.IsFalse (Rune.ValidRune ((Rune)'\ud801'));
		}

		[Test]
		public void TestValid ()
		{
			var rune1 = new Rune ('\ud83c', '\udf39');
			var buff1 = new byte [4];
			Assert.AreEqual (4, Rune.EncodeRune (rune1, buff1));
			Assert.IsTrue (Rune.Valid (buff1));
			Assert.AreEqual (2, rune1.ToString ().Length);
			Assert.AreEqual (4, Rune.RuneLen (rune1));
			var rune2 = (uint)'\ud801'; // To avoid throwing an exception set as uint instead a Rune instance.
			var buff2 = new byte [4];
			Assert.AreEqual (3, Rune.EncodeRune (rune2, buff2));
			Assert.IsFalse (Rune.Valid (buff2)); // To avoid throwing an exception pass as uint parameter instead Rune.
			Assert.AreEqual (5, rune2.ToString ().Length); // Invalid string. It returns the decimal 55297 representation of the 0xd801 hexadecimal.
			Assert.AreEqual (-1, Rune.RuneLen (rune2));
			Assert.AreEqual (Rune.EncodeRune (new Rune ('\ud801'), buff2), 3); // error
			Assert.AreEqual (new byte [] { 0xef, 0x3f, 0x3d, 0 }, buff2); // error
		}

		[Test]
		public void Test_IsNonSpacingChar ()
		{
			Rune l = '\u0370';
			Assert.False (Rune.IsNonSpacingChar (l));
			Assert.AreEqual (1, Rune.ColumnWidth (l));
			Assert.AreEqual (1, ustring.Make (l).ConsoleWidth);
			Rune ns = '\u302a';
			Assert.False (Rune.IsNonSpacingChar (ns));
			Assert.AreEqual (2, Rune.ColumnWidth (ns));
			Assert.AreEqual (2, ustring.Make (ns).ConsoleWidth);
			l = '\u006f';
			ns = '\u0302';
			var s = "\u006f\u0302";
			Assert.AreEqual (1, Rune.ColumnWidth (l));
			Assert.AreEqual (0, Rune.ColumnWidth (ns));
			var ul = ustring.Make (l);
			Assert.AreEqual ("o", ul);
			var uns = ustring.Make (ns);
			Assert.AreEqual ("̂", uns);
			var f = ustring.Make ($"{l}{ns}");
			Assert.AreEqual ("ô", f);
			Assert.AreEqual (f, s);
			Assert.AreEqual (1, f.ConsoleWidth);
			Assert.AreEqual (1, s.Sum (c => Rune.ColumnWidth (c)));
			Assert.AreEqual (2, s.Length);
			(var rune, var size) = f.DecodeRune ();
			Assert.AreEqual (rune, l);
			Assert.AreEqual (1, size);
			l = '\u0041';
			ns = '\u0305';
			s = "\u0041\u0305";
			Assert.AreEqual (1, Rune.ColumnWidth (l));
			Assert.AreEqual (0, Rune.ColumnWidth (ns));
			ul = ustring.Make (l);
			Assert.AreEqual ("A", ul);
			uns = ustring.Make (ns);
			Assert.AreEqual ("̅", uns);
			f = ustring.Make ($"{l}{ns}");
			Assert.AreEqual ("A̅", f);
			Assert.AreEqual (f, s);
			Assert.AreEqual (1, f.ConsoleWidth);
			Assert.AreEqual (1, s.Sum (c => Rune.ColumnWidth (c)));
			Assert.AreEqual (2, s.Length);
			(rune, size) = f.DecodeRune ();
			Assert.AreEqual (rune, l);
			Assert.AreEqual (1, size);
			l = '\u0061';
			ns = '\u0308';
			s = "\u0061\u0308";
			Assert.AreEqual (1, Rune.ColumnWidth (l));
			Assert.AreEqual (0, Rune.ColumnWidth (ns));
			ul = ustring.Make (l);
			Assert.AreEqual ("a", ul);
			uns = ustring.Make (ns);
			Assert.AreEqual ("̈", uns);
			f = ustring.Make ($"{l}{ns}");
			Assert.AreEqual ("ä", f);
			Assert.AreEqual (f, s);
			Assert.AreEqual (1, f.ConsoleWidth);
			Assert.AreEqual (1, s.Sum (c => Rune.ColumnWidth (c)));
			Assert.AreEqual (2, s.Length);
			(rune, size) = f.DecodeRune ();
			Assert.AreEqual (rune, l);
			Assert.AreEqual (1, size);
			l = '\u4f00';
			ns = '\u302a';
			s = "\u4f00\u302a";
			Assert.AreEqual (2, Rune.ColumnWidth (l));
			Assert.AreEqual (2, Rune.ColumnWidth (ns));
			ul = ustring.Make (l);
			Assert.AreEqual ("伀", ul);
			uns = ustring.Make (ns);
			Assert.AreEqual ("〪", uns);
			f = ustring.Make ($"{l}{ns}");
			Assert.AreEqual ("伀〪", f); // Occupies 4 columns.
			Assert.AreEqual (f, s);
			Assert.AreEqual (4, f.ConsoleWidth);
			Assert.AreEqual (4, s.Sum (c => Rune.ColumnWidth (c)));
			Assert.AreEqual (2, s.Length);
			(rune, size) = f.DecodeRune ();
			Assert.AreEqual (rune, l);
			Assert.AreEqual (3, size);
		}

		[Test]
		public void Test_IsWideChar ()
		{
			Assert.True (Rune.IsWideChar (0x115e));
			Assert.AreEqual (2, Rune.ColumnWidth (0x115e));
			Assert.False (Rune.IsWideChar (0x116f));
		}

		[Test]
		public void Test_MaxRune ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new Rune (500000000), "Value is beyond the supplementary range!");
			Assert.Throws<ArgumentOutOfRangeException> (() => new Rune (0xf801, 0xdfff), "Resulted rune must be less or equal to 0x10ffff!");
		}

		[Test]
		public void Sum_Of_ColumnWidth_Is_Not_Always_Equal_To_ConsoleWidth ()
		{
			const int start = 0x000000;
			const int end = 0x10ffff;

			for (int i = start; i <= end; i++) {
				Rune r = new Rune ((uint)i);
				if (!r.IsValid) {
					continue;
				}
				ustring us = ustring.Make (r);
				string hex = i.ToString ("x6");
				int v = int.Parse (hex, System.Globalization.NumberStyles.HexNumber);
				string s = char.ConvertFromUtf32 (v);

				if (!r.IsSurrogatePair) {
					Assert.AreEqual (r.ToString (), us);
					Assert.AreEqual (us, s);
					if (Rune.ColumnWidth (r) < 0) {
						Assert.AreNotEqual (Rune.ColumnWidth (r), us.ConsoleWidth);
						Assert.AreNotEqual (s.Sum (c => Rune.ColumnWidth (c)), us.ConsoleWidth);
					} else {
						Assert.AreEqual (Rune.ColumnWidth (r), us.ConsoleWidth);
						Assert.AreEqual (s.Sum (c => Rune.ColumnWidth (c)), us.ConsoleWidth);
					}
					Assert.AreEqual (us.RuneCount, s.Length);
				} else {
					Assert.AreEqual (r.ToString (), us.ToString ());
					Assert.AreEqual (us.ToString (), s);
					Assert.AreEqual (Rune.ColumnWidth (r), us.ConsoleWidth);
					Assert.AreEqual (s.Sum (c => Rune.ColumnWidth (c)), us.ConsoleWidth);
					Assert.AreEqual (1, us.RuneCount); // Here returns 1 because is a valid surrogate pair resulting in only rune >=U+10000..U+10FFFF
					Assert.AreEqual (2, s.Length); // String always preserves the originals values of each surrogate pair
				}
			}
		}

		[Test]
		public void Test_Right_To_Left_Runes ()
		{
			Rune r0 = 0x020000;
			Rune r7 = 0x020007;
			Rune r1b = 0x02001b;
			Rune r9b = 0x02009b;

			Assert.AreEqual (1, Rune.ColumnWidth (r0));
			Assert.AreEqual (1, Rune.ColumnWidth (r7));
			Assert.AreEqual (1, Rune.ColumnWidth (r1b));
			Assert.AreEqual (1, Rune.ColumnWidth (r9b));

			Rune.DecodeSurrogatePair ("𐨁", out char [] chars);
			var rtl = new Rune (chars [0], chars [1]);
			var rtlp = new Rune ('\ud802', '\ude01');
			var s = "\U00010a01";

			Assert.AreEqual (0, Rune.ColumnWidth (rtl));
			Assert.AreEqual (0, Rune.ColumnWidth (rtlp));
			Assert.AreEqual (2, s.Length);
		}

		[TestCase (0x20D0, 0x20EF)]
		[TestCase (0x2310, 0x231F)]
		[TestCase (0x1D800, 0x1D80F)]
		public void Test_Range (int start, int end)
		{
			for (int i = start; i <= end; i++) {
				Rune r = new Rune ((uint)i);
				ustring us = ustring.Make (r);
				string hex = i.ToString ("x6");
				int v = int.Parse (hex, System.Globalization.NumberStyles.HexNumber);
				string s = char.ConvertFromUtf32 (v);

				if (!r.IsSurrogatePair) {
					Assert.AreEqual (r.ToString (), us);
					Assert.AreEqual (us, s);
					Assert.AreEqual (Rune.ColumnWidth (r), us.ConsoleWidth);
					Assert.AreEqual (us.RuneCount, s.Length); // For not surrogate pairs ustring.RuneCount is always equal to String.Length
				} else {
					Assert.AreEqual (r.ToString (), us.ToString ());
					Assert.AreEqual (us.ToString (), s);
					Assert.AreEqual (Rune.ColumnWidth (r), us.ConsoleWidth);
					Assert.AreEqual (1, us.RuneCount); // Here returns 1 because is a valid surrogate pair resulting in only rune >=U+10000..U+10FFFF
					Assert.AreEqual (2, s.Length); // String always preserves the originals values of each surrogate pair
				}
				Assert.AreEqual (s.Sum (c => Rune.ColumnWidth (c)), us.ConsoleWidth);
			}
		}

		[Test]
		public void Test_IsSurrogate ()
		{
			Rune r = '\ue0fd';
			Assert.False (r.IsSurrogate);
			Assert.False (Rune.IsSurrogateRune (r));
			r = 0x927C0;
			Assert.False (r.IsSurrogate);
			Assert.False (Rune.IsSurrogateRune (r));

			r = '\ud800';
			Assert.True (r.IsSurrogate);
			Assert.True (Rune.IsSurrogateRune (r));
			r = '\udfff';
			Assert.True (r.IsSurrogate);
			Assert.True (Rune.IsSurrogateRune (r));
		}

		[Test]
		public void Test_EncodeSurrogatePair ()
		{
			Assert.IsFalse (Rune.EncodeSurrogatePair (0x40D7C0, 0xDC20, out _));
			Assert.IsFalse (Rune.EncodeSurrogatePair (0x0065, 0x0301, out _));
			Assert.IsTrue (Rune.EncodeSurrogatePair ('\ud83c', '\udf56', out Rune rune));
			Assert.AreEqual (0x1F356, rune.Value);
			Assert.AreEqual ("🍖", rune.ToString ());
		}

		[Test]
		public void Test_DecodeSurrogatePair ()
		{
			Assert.False (Rune.DecodeSurrogatePair ('\uea85', out char [] chars));
			Assert.IsNull (chars);
			Assert.True (Rune.DecodeSurrogatePair (0x1F356, out chars));
			Assert.AreEqual (2, chars.Length);
			Assert.AreEqual ('\ud83c', chars [0]);
			Assert.AreEqual ('\udf56', chars [1]);
			Assert.AreEqual ("🍖", new Rune (chars [0], chars [1]).ToString ());
		}

		[Test]
		public void Test_Surrogate_Pairs_Range ()
		{
			for (uint h = 0xd800; h <= 0xdbff; h++) {
				for (uint l = 0xdc00; l <= 0xdfff; l++) {
					Rune r = new Rune (h, l);
					ustring us = ustring.Make (r);
					string hex = ((uint)r).ToString ("x6");
					int v = int.Parse (hex, System.Globalization.NumberStyles.HexNumber);
					string s = char.ConvertFromUtf32 (v);

					Assert.True (v >= 0x10000 && v <= Rune.MaxRune);
					Assert.AreEqual (r.ToString (), us.ToString ());
					Assert.AreEqual (us.ToString (), s);
					Assert.AreEqual (Rune.ColumnWidth (r), us.ConsoleWidth);
					Assert.AreEqual (s.Sum (c => Rune.ColumnWidth (c)), us.ConsoleWidth);
					Assert.AreEqual (1, us.RuneCount); // Here returns 1 because is a valid surrogate pair resulting in only rune >=U+10000..U+10FFFF
					Assert.AreEqual (2, s.Length); // String always preserves the originals values of each surrogate pair
				}
			}
		}

		[Test]
		public void Test_ExpectedSizeFromFirstByte ()
		{
			Assert.AreEqual (-1, Rune.ExpectedSizeFromFirstByte (255));
			Assert.AreEqual (1, Rune.ExpectedSizeFromFirstByte (127));
			Assert.AreEqual (4, Rune.ExpectedSizeFromFirstByte (240));
		}

		[Test]
		public void Test_FullRune_Extension ()
		{
			ustring us = "Hello, 世界";
			Assert.True (us.FullRune ());
			us = $"Hello, {ustring.Make (new byte [] { 228 })}界";
			Assert.False (us.FullRune ());
		}

		[Test]
		public void Test_DecodeRune_Extension ()
		{
			ustring us = "Hello, 世界";
			List<Rune> runes = new List<Rune> ();
			int tSize = 0;
			for (int i = 0; i < us.RuneCount; i++) {
				(Rune rune, int size) = us.RuneSubstring (i, 1).DecodeRune ();
				runes.Add (rune);
				tSize += size;
			}
			ustring result = ustring.Make (runes);
			Assert.AreEqual ("Hello, 世界", result);
			Assert.AreEqual (13, tSize);
		}

		[Test]
		public void Test_DecodeLastRune_Extension ()
		{
			ustring us = "Hello, 世界";
			List<Rune> runes = new List<Rune> ();
			int tSize = 0;
			for (int i = us.RuneCount - 1; i >= 0; i--) {
				(Rune rune, int size) = us.RuneSubstring (i, 1).DecodeLastRune ();
				runes.Add (rune);
				tSize += size;
			}
			ustring result = ustring.Make (runes);
			Assert.AreEqual ("界世 ,olleH", result);
			Assert.AreEqual (13, tSize);
		}

		[Test]
		public void Test_InvalidIndex_Extension ()
		{
			ustring us = "Hello, 世界";
			Assert.AreEqual (-1, us.InvalidIndex ());
			us = ustring.Make (new byte [] { 0xff, 0xfe, 0xfd });
			Assert.AreEqual (0, us.InvalidIndex ());
		}

		[Test]
		public void Test_Valid_Extension ()
		{
			ustring us = "Hello, 世界";
			Assert.True (us.Valid ());
			us = ustring.Make (new byte [] { 0xff, 0xfe, 0xfd });
			Assert.False (us.Valid ());
		}

		[Test]
		public void Test_ExpectedSizeFromFirstByte_Extension ()
		{
			ustring us = ustring.Make (255);
			Assert.AreEqual (-1, us.ExpectedSizeFromFirstByte ());
			us = ustring.Make (127);
			Assert.AreEqual (1, us.ExpectedSizeFromFirstByte ());
			us = ustring.Make (240);
			Assert.AreEqual (4, us.ExpectedSizeFromFirstByte ());
		}

		[Test]
		public void Equals ()
		{
			var a = new List<List<Rune>> () { ustring.Make ("First line.").ToRuneList () };
			var b = new List<List<Rune>> () { ustring.Make ("First line.").ToRuneList (), ustring.Make ("Second line.").ToRuneList () };
			var c = new List<Rune> (a [0]);
			var d = a [0];

			Assert.AreEqual (a [0], b [0]);
			// Not the same reference
			Assert.False (a [0] == b [0]);
			Assert.AreNotEqual (a [0], b [1]);
			Assert.False (a [0] == b [1]);

			Assert.AreEqual (c, a [0]);
			Assert.False (c == a [0]);
			Assert.AreEqual (c, b [0]);
			Assert.False (c == b [0]);
			Assert.AreNotEqual (c, b [1]);
			Assert.False (c == b [1]);

			Assert.AreEqual (d, a [0]);
			// Is the same reference
			Assert.True (d == a [0]);
			Assert.AreEqual (d, b [0]);
			Assert.False (d == b [0]);
			Assert.AreNotEqual (d, b [1]);
			Assert.False (d == b [1]);

			Assert.True (a [0].SequenceEqual (b [0]));
			Assert.False (a [0].SequenceEqual (b [1]));

			Assert.True (c.SequenceEqual (a [0]));
			Assert.True (c.SequenceEqual (b [0]));
			Assert.False (c.SequenceEqual (b [1]));

			Assert.True (d.SequenceEqual (a [0]));
			Assert.True (d.SequenceEqual (b [0]));
			Assert.False (d.SequenceEqual (b [1]));
		}

		[Test]
		public void Rune_ColumnWidth_Versus_Ustring_ConsoleWidth_With_Non_Printable_Characters ()
		{
			int sumRuneWidth = 0;
			int sumConsoleWidth = 0;
			for (uint i = 0; i < 32; i++) {
				sumRuneWidth += Rune.ColumnWidth (i);
				sumConsoleWidth += ustring.Make (i).ConsoleWidth;
			}

			Assert.AreEqual (-32, sumRuneWidth);
			Assert.AreEqual (0, sumConsoleWidth);
		}

		[Test]
		public void Rune_ColumnWidth_Versus_Ustring_ConsoleWidth ()
		{
			ustring us = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
			Assert.AreEqual (200, us.Length);
			Assert.AreEqual (200, us.RuneCount);
			Assert.AreEqual (200, us.ConsoleWidth);
			int sumRuneWidth = us.Sum (x => Rune.ColumnWidth (x));
			Assert.AreEqual (200, sumRuneWidth);

			us = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789\n";
			Assert.AreEqual (201, us.Length);
			Assert.AreEqual (201, us.RuneCount);
			Assert.AreEqual (200, us.ConsoleWidth);
			sumRuneWidth = us.Sum (x => Rune.ColumnWidth (x));
			Assert.AreEqual (199, sumRuneWidth);
		}
	}
}
// A Unicode character is considered a bidirectional text control character if it falls into any of the following ranges: U+061c, U+200e-U+200f, U+202a-U+202e, U+2066-U+2069.