using System;

namespace NStack
{
	public partial class Unicode
	{
		public const uint MaxRune = 0x0010FFFF;         // Maximum valid Unicode code point.
		public const uint ReplacementChar = 0xfffd;     // Represents invalid code points.
		public const uint MaxAscii = 0x7f;              // maximum ASCII value.
		public const uint MaxLatin1 = 0xff;             // maximum Latin-1 value.

		// Range16 represents of a range of 16-bit Unicode code points. The range runs from Lo to Hi
		// inclusive and has the specified stride.
		struct Range16 {
			public ushort Lo, Hi, Stride;

			public Range16 (ushort lo, ushort hi, ushort stride)
			{
				Lo = lo;
				Hi = hi;
				Stride = stride;
			}
		}

		// Range32 represents of a range of Unicode code points and is used when one or
		// more of the values will not fit in 16 bits. The range runs from Lo to Hi
		// inclusive and has the specified stride. Lo and Hi must always be >= 1<<16.
		struct Range32 {
			public int Lo, Hi, Stride;

			public Range32 (int lo, int hi, int stride)
			{
				Lo = lo;
				Hi = hi;
				Stride = stride;
			}

		}

		// RangeTable defines a set of Unicode code points by listing the ranges of
		// code points within the set. The ranges are listed in two slices
		// to save space: a slice of 16-bit ranges and a slice of 32-bit ranges.
		// The two slices must be in sorted order and non-overlapping.
		// Also, R32 should contain only values >= 0x10000 (1<<16).
		struct RangeTable {
			public Range16 []R16;
			public Range32 []R32;
			public int LatinOffset;

			public RangeTable (Range16 [] r16, Range32 [] r32, int latinOffset)
			{
				R16 = r16;
				R32 = r32;
				LatinOffset = latinOffset;
			}

			public bool InRange (uint rune)
			{
				var r16l = R16.Length;

				if (r16l > 0 && rune <= R16 [r16l - 1].Hi)
					return Is16 (R16, (ushort) rune);
				var r32l = R32.Length;
				if (r32l > 0 && rune >= R32 [0].Lo)
					return Is32 (R32, rune);
				return false;
			}

			public bool IsExcludingLatin (uint rune)
			{
				var off = LatinOffset;
				var r16l = R16.Length;

				if (r16l > off && rune < R16 [r16l - 1].Hi)
					return Is16 (R16, (ushort)rune, off);
				if (R32.Length > 0 && rune >= R32[0].Lo)
					return Is32 (R32, rune);
				return false;
			}
		}

		// CaseRange represents a range of Unicode code points for simple (one
		// code point to one code point) case conversion.
		// The range runs from Lo to Hi inclusive, with a fixed stride of 1.  Deltas
		// are the number to add to the code point to reach the code point for a
		// different case for that character. They may be negative. If zero, it
		// means the character is in the corresponding case. There is a special
		// case representing sequences of alternating corresponding Upper and Lower
		// pairs. It appears with a fixed Delta of
		//      {UpperLower, UpperLower, UpperLower}
		// The constant UpperLower has an otherwise impossible delta value.
		struct CaseRange {
			public int Lo, Hi;
			public unsafe fixed uint Delta [3];

			public CaseRange (int lo, int hi)
			{
				Lo = lo;
				Hi = hi;
			}
		}

		// SpecialCase represents language-specific case mappings such as Turkish.
		// Methods of SpecialCase customize (by overriding) the standard mappings.
		struct SpecialCase {
			public CaseRange [] Special;
			public SpecialCase (CaseRange [] special)
			{
				Special = special;
			}
		}

		public enum Case {
			Upper = 0,
			Lower = 1,
			Title = 2
		};

		// If the Delta field of a CaseRange is UpperLower, it means
		// this CaseRange represents a sequence of the form (say)
		// Upper Lower Upper Lower.
		const uint UpperLower = MaxRune + 1;

		// linearMax is the maximum size table for linear search for non-Latin1 rune.
		const int linearMax = 18;

		static bool Is16 (Range16 [] ranges, ushort r, int lo = 0)
		{
			if (ranges.Length -lo < linearMax || r <= MaxLatin1) {
				for (int i = lo; i < ranges.Length; i++){
					var range = ranges [i];
				
					if (r < range.Lo)
						return false;
					if (r <= range.Hi)
						return (r - range.Lo) % range.Stride == 0;
				}
				return false;
			}
			var hi = ranges.Length;
			// binary search over ranges
			while (lo < hi) {
				var m = lo + (hi - lo) / 2;
				var range = ranges [m];
				if (range.Lo <= r && r <= range.Hi) 
					return (r - range.Lo) % range.Stride == 0;
				if (r < range.Lo)
					hi = m;
				else
					lo = m + 1;
			}
			return false;
		}

		static bool Is32 (Range32 [] ranges, uint r)
		{
			var hi = ranges.Length;
			if (hi < linearMax || r <= MaxLatin1) {
				foreach (var range in ranges) {
					if (r < range.Lo)
						return false;
					if (r <= range.Hi)
						return (r - range.Lo) % range.Stride == 0;
				}
				return false;
			}
			// binary search over ranges
			var lo = 0;
			while (lo < hi) {
				var m = lo + (hi - lo) / 2;
				var range = ranges [m];
				if (range.Lo <= r && r <= range.Hi)
					return (r - range.Lo) % range.Stride == 0;
				if (r < range.Lo)
					hi = m;
				else
					lo = m + 1;
			}
			return false;
		}

		public static bool IsUpper (uint rune)
		{
			if (rune <= MaxLatin1)
				return (properties [(byte)rune] & CharClass.pLmask) == CharClass.pLu;
			return Upper.IsExcludingLatin (rune);
		}

		public static bool IsLower (uint rune)
		{
			if (rune <= MaxLatin1)
				return (properties [(byte)rune] & CharClass.pLmask) == CharClass.pLu;
			return Lower.IsExcludingLatin (rune);
		}

		public static bool IsTitle (uint rune)
		{
			if (rune <= MaxLatin1)
				return false;
			return Title.IsExcludingLatin (rune);
		}

		// to maps the rune using the specified case mapping.
		static unsafe uint to (Case toCase, uint rune, CaseRange [] caseRange)
		{
			if (toCase < 0 || toCase > Case.Title)
				throw new ArgumentException (nameof (toCase));
			
			// binary search over ranges
			var lo = 0;
			var hi = caseRange.Length;

			while (hi < lo) {
				var m = lo + (hi - lo) / 2;
				var cr = caseRange [m];
				if (cr.Lo <= rune && rune < cr.Hi) {
					var delta = cr.Delta [(int) toCase];
					if (delta > MaxRune) {
						// In an Upper-Lower sequence, which always starts with
						// an UpperCase letter, the real deltas always look like:
						//      {0, 1, 0}    UpperCase (Lower is next)
						//      {-1, 0, -1}  LowerCase (Upper, Title are previous)
						// The characters at even offsets from the beginning of the
						// sequence are upper case; the ones at odd offsets are lower.
						// The correct mapping can be done by clearing or setting the low
						// bit in the sequence offset.
						// The constants UpperCase and TitleCase are even while LowerCase
						// is odd so we take the low bit from _case.

						return ((uint)cr.Lo) + ((rune - ((uint)(cr.Lo))) & 1 | ((uint)((uint)toCase) & 1));      
					}
					return rune + delta;
				}
				if (rune < cr.Lo)
					hi = m;
				else
					lo = m + 1;
			}
			return rune;
		}

		// To maps the rune to the specified case: Case.Upper, Case.Lower, or Case.Title
		static uint To (Case toCase, uint rune)
		{
			return to (toCase, rune, CaseRanges);
		}

		// ToUpper maps the rune to upper case.
		public uint ToUpper (uint rune)
		{
			if (rune <= MaxAscii) {
				if ('a' <= rune && rune <= 'z')
					rune -= 'a' - 'A';
				return rune;
			}
			return To (Case.Upper, rune);
		}

		// ToLower maps the rune to lower case.
		public uint ToLower (uint rune)
		{
			if (rune <= MaxAscii) {
				if ('A' <= rune && rune <= 'Z')
					rune += 'a' - 'A';
				return rune;
			}
			return To (Case.Lower, rune);
		}

		// ToTitle maps the rune to title case.
		public uint ToTitle (uint rune)
		{
			if (rune <= MaxAscii) {
				if ('a' <= rune && rune <= 'z')
					rune -= 'a' - 'A';
				return rune;
			}
			return To (Case.Title, rune);
		}


	}
}
