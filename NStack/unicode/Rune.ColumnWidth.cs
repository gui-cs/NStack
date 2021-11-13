//
// Contains a port of this code:
// https://www.cl.cam.ac.uk/%7Emgk25/ucs/wcwidth.c
//
using NStack;

namespace System
{
	public partial struct Rune
	{
		static uint[,] combining = new uint[,] {
			{ 0x0300, 0x036F, 1 }, { 0x0483, 0x0486, 1 }, { 0x0488, 0x0489, 1 },
			{ 0x0591, 0x05BD, 1 }, { 0x05BF, 0x05BF, 1 }, { 0x05C1, 0x05C2, 1 },
			{ 0x05C4, 0x05C5, 1 }, { 0x05C7, 0x05C7, 1 }, { 0x0600, 0x0603, 1 },
			{ 0x0610, 0x0615, 1 }, { 0x064B, 0x065E, 1 }, { 0x0670, 0x0670, 1 },
			{ 0x06D6, 0x06E4, 1 }, { 0x06E7, 0x06E8, 1 }, { 0x06EA, 0x06ED, 1 },
			{ 0x070F, 0x070F, 1 }, { 0x0711, 0x0711, 1 }, { 0x0730, 0x074A, 1 },
			{ 0x07A6, 0x07B0, 1 }, { 0x07EB, 0x07F3, 1 }, { 0x0901, 0x0902, 1 },
			{ 0x093C, 0x093C, 1 }, { 0x0941, 0x0948, 1 }, { 0x094D, 0x094D, 1 },
			{ 0x0951, 0x0954, 1 }, { 0x0962, 0x0963, 1 }, { 0x0981, 0x0981, 1 },
			{ 0x09BC, 0x09BC, 1 }, { 0x09C1, 0x09C4, 1 }, { 0x09CD, 0x09CD, 1 },
			{ 0x09E2, 0x09E3, 1 }, { 0x0A01, 0x0A02, 1 }, { 0x0A3C, 0x0A3C, 1 },
			{ 0x0A41, 0x0A42, 1 }, { 0x0A47, 0x0A48, 1 }, { 0x0A4B, 0x0A4D, 1 },
			{ 0x0A70, 0x0A71, 1 }, { 0x0A81, 0x0A82, 1 }, { 0x0ABC, 0x0ABC, 1 },
			{ 0x0AC1, 0x0AC5, 1 }, { 0x0AC7, 0x0AC8, 1 }, { 0x0ACD, 0x0ACD, 1 },
			{ 0x0AE2, 0x0AE3, 1 }, { 0x0B01, 0x0B01, 1 }, { 0x0B3C, 0x0B3C, 1 },
			{ 0x0B3F, 0x0B3F, 1 }, { 0x0B41, 0x0B43, 1 }, { 0x0B4D, 0x0B4D, 1 },
			{ 0x0B56, 0x0B56, 1 }, { 0x0B82, 0x0B82, 1 }, { 0x0BC0, 0x0BC0, 1 },
			{ 0x0BCD, 0x0BCD, 1 }, { 0x0C3E, 0x0C40, 1 }, { 0x0C46, 0x0C48, 1 },
			{ 0x0C4A, 0x0C4D, 1 }, { 0x0C55, 0x0C56, 1 }, { 0x0CBC, 0x0CBC, 1 },
			{ 0x0CBF, 0x0CBF, 1 }, { 0x0CC6, 0x0CC6, 1 }, { 0x0CCC, 0x0CCD, 1 },
			{ 0x0CE2, 0x0CE3, 1 }, { 0x0D41, 0x0D43, 1 }, { 0x0D4D, 0x0D4D, 1 },
			{ 0x0DCA, 0x0DCA, 1 }, { 0x0DD2, 0x0DD4, 1 }, { 0x0DD6, 0x0DD6, 1 },
			{ 0x0E31, 0x0E31, 1 }, { 0x0E34, 0x0E3A, 1 }, { 0x0E47, 0x0E4E, 1 },
			{ 0x0EB1, 0x0EB1, 1 }, { 0x0EB4, 0x0EB9, 1 }, { 0x0EBB, 0x0EBC, 1 },
			{ 0x0EC8, 0x0ECD, 1 }, { 0x0F18, 0x0F19, 1 }, { 0x0F35, 0x0F35, 1 },
			{ 0x0F37, 0x0F37, 1 }, { 0x0F39, 0x0F39, 1 }, { 0x0F71, 0x0F7E, 1 },
			{ 0x0F80, 0x0F84, 1 }, { 0x0F86, 0x0F87, 1 }, { 0x0F90, 0x0F97, 1 },
			{ 0x0F99, 0x0FBC, 1 }, { 0x0FC6, 0x0FC6, 1 }, { 0x102D, 0x1030, 1 },
			{ 0x1032, 0x1032, 1 }, { 0x1036, 0x1037, 1 }, { 0x1039, 0x1039, 1 },
			{ 0x1058, 0x1059, 1 }, { 0x1160, 0x11FF, 1 }, { 0x135F, 0x135F, 1 },
			{ 0x1712, 0x1714, 1 }, { 0x1732, 0x1734, 1 }, { 0x1752, 0x1753, 1 },
			{ 0x1772, 0x1773, 1 }, { 0x17B4, 0x17B5, 1 }, { 0x17B7, 0x17BD, 1 },
			{ 0x17C6, 0x17C6, 1 }, { 0x17C9, 0x17D3, 1 }, { 0x17DD, 0x17DD, 1 },
			{ 0x180B, 0x180D, 1 }, { 0x18A9, 0x18A9, 1 }, { 0x1920, 0x1922, 1 },
			{ 0x1927, 0x1928, 1 }, { 0x1932, 0x1932, 1 }, { 0x1939, 0x193B, 1 },
			{ 0x1A17, 0x1A18, 1 }, { 0x1B00, 0x1B03, 1 }, { 0x1B34, 0x1B34, 1 },
			{ 0x1B36, 0x1B3A, 1 }, { 0x1B3C, 0x1B3C, 1 }, { 0x1B42, 0x1B42, 1 },
			{ 0x1B6B, 0x1B73, 1 }, { 0x1DC0, 0x1DCA, 1 }, { 0x1DFE, 0x1DFF, 1 },
			{ 0x200B, 0x200F, 1 }, { 0x202A, 0x202E, 1 }, { 0x2060, 0x2063, 1 },
			{ 0x206A, 0x206F, 1 }, { 0x20D0, 0x20EF, 1 }, { 0x302A, 0x302F, 2 },
			{ 0x3099, 0x309A, 2 }, { 0xA806, 0xA806, 1 }, { 0xA80B, 0xA80B, 1 },
			{ 0xA825, 0xA826, 1 }, { 0xFB1E, 0xFB1E, 1 }, { 0xFE00, 0xFE0F, 1 },
			{ 0xFE20, 0xFE23, 1 }, { 0xFEFF, 0xFEFF, 1 }, { 0xFFF9, 0xFFFB, 1 },
			{ 0x10A01, 0x10A03, 1 }, { 0x10A05, 0x10A06, 1 }, { 0x10A0C, 0x10A0F, 1 },
			{ 0x10A38, 0x10A3A, 1 }, { 0x10A3F, 0x10A3F, 1 }, { 0x1D167, 0x1D169, 2 },
			{ 0x1D173, 0x1D182, 2 }, { 0x1D185, 0x1D18B, 2 }, { 0x1D1AA, 0x1D1AD, 2 },
			{ 0x1D242, 0x1D244, 2 }, { 0xE0001, 0xE0001, 1 }, { 0xE0020, 0xE007F, 1 },
			{ 0xE0100, 0xE01EF, 1 }
		};

		static uint[,] combiningWideChars = new uint[,] {
			/* Hangul Jamo init. consonants - 0x1100, 0x11ff */
			/* Miscellaneous Technical - 0x2300, 0x23ff */
			/* Hangul Syllables - 0x11a8, 0x11c2 */
			/* CJK Compatibility Ideographs - f900, fad9 */
			/* Vertical forms - fe10, fe19 */
			/* CJK Compatibility Forms - fe30, fe4f */
			/* Fullwidth Forms - ff01, ffee */
			/* Alphabetic Presentation Forms - 0xFB00, 0xFb4f */
			/* Chess Symbols - 0x1FA00, 0x1FA0f */

			{ 0x1100, 0x115f, 2 }, { 0x2329, 0x232a, 2 }, { 0x2e80, 0x303e, 2 },
			{ 0x3041, 0x3096, 2 }, { 0x3099, 0x30ff, 2 }, { 0x3105, 0x312e, 2 },
			{ 0x3131, 0x318e, 2 }, { 0x3190, 0x3247, 2 }, { 0x3250, 0x4dbf, 2 },
			{ 0x4e00, 0xa4c6, 2 }, { 0xa960, 0xa97c, 2 }, { 0xac00 ,0xd7a3, 2 },
			{ 0xf900, 0xfaff, 2 }, { 0xfe10, 0xfe1f, 2 }, { 0xfe30 ,0xfe6b, 2 },
			{ 0xff01, 0xff60, 2 }, { 0xffe0, 0xffe6, 2 }
		};

		static int bisearch(uint rune, uint[,] table, int max)
		{
			int min = 0;
			int mid;

			if (rune < table[0, 0] || rune > table[max, 1])
				return 0;
			while (max >= min)
			{
				mid = (min + max) / 2;
				if (rune > table[mid, 1])
					min = mid + 1;
				else if (rune < table[mid, 0])
					max = mid - 1;
				else
					return 1;
			}

			return 0;
		}

		static bool bisearch(uint rune, uint[,] table, out int width)
		{
			width = 0;
			var length = table.GetLength(0);
			if (length == 0 || rune < table[0, 0] || rune > table[length - 1, 1])
				return false;

			for (int i = 0; i < length; i++)
			{
				if (rune >= table[i, 0] && rune <= table[i, 1]) {
					width = (int)table[i, 2];
					return true;
				}
			}

			return false;
		}

		static uint gethexaformat(uint rune, int length)
		{
			var hex = rune.ToString($"x{length}");
			var hexstr = hex.Substring(hex.Length - length, length);
			return (uint)int.Parse(hexstr, System.Globalization.NumberStyles.HexNumber);
		}

		/// <summary>
		/// Check if the rune is a non-spacing character.
		/// </summary>
		/// <param name="rune">The rune.</param>
		/// <param name="width">The width.</param>
		/// <returns>True if is a non-spacing character, false otherwise.</returns>
		public static bool IsNonSpacingChar(uint rune, out int width)
        {
			return bisearch(rune, combining, out width);
        }

		/// <summary>
		/// Check if the rune is a wide character.
		/// </summary>
		/// <param name="rune">The rune.</param>
		/// <returns>True if is a wide character, false otherwise.</returns>
		public static bool IsWideChar(uint rune)
        {
			return bisearch(gethexaformat(rune, 4), combiningWideChars, out _);
        }

		/// <summary>
		/// Number of column positions of a wide-character code.   This is used to measure runes as displayed by text-based terminals.
		/// </summary>
		/// <returns>The width in columns, 0 if the argument is the null character, -1 if the value is not printable, otherwise the number of columns that the rune occupies.</returns>
		/// <param name="rune">The rune.</param>
		public static int ColumnWidth(Rune rune)
		{
			uint irune = (uint)rune;
			if (irune < 32 || (irune >= 0x7f && irune < 0xa0))
				return -1;
			if (irune < 127)
				return 1;
			/* binary search in table of non-spacing characters */
			if (bisearch(irune, combining, out int width))
				// if (bisearch(irune, combining, combining.GetLength(0) - 1) != 0)
				// return 0;
				return width;
			/* if we arrive here, ucs is not a combining or C0/C1 control character */
			return bisearch(gethexaformat(irune, 4), combiningWideChars, out width) ? width : 1;
		}
	}
}
