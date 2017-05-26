using System;
namespace NStack {
	public partial class Unicode {
		[Flags]
		enum CharClass : byte {
			pC = 1 << 0, // a control character.
			pP = 1 << 1, // a punctuation character.
			pN = 1 << 2, // a numeral.
			pS = 1 << 3, // a symbolic character.
			pZ = 1 << 4, // a spacing character.
			pLu = 1 << 5, // an upper-case letter.
			pLl = 1 << 6, // a lower-case letter.
			pp = 1 << 7, // a printable character according to Go's definition.
			pg = pp | pZ,   // a graphical character according to the Unicode definition.
			pLo = pLl | pLu, // a letter that is neither upper nor lower case.
			pLmask = pLo
		}

		public static RangeTable [] GraphicRanges = new [] {
			_L, _M, _N, _P, _S, _Zs
		};

		public static RangeTable [] PrintRanges = new [] {
			_L, _M, _N, _P, _S
		};

		/// <summary>
		/// Determines if a rune is on a set of ranges.
		/// </summary>
		/// <returns><c>true</c>, if rune in ranges was ised, <c>false</c> otherwise.</returns>
		/// <param name="rune">Rune.</param>
		/// <param name="inRanges">In ranges.</param>
		public static bool IsRuneInRanges (uint rune, params RangeTable [] inRanges)
		{
			foreach (var range in inRanges)
				if (range.InRange (rune))
					return true;
			return false;
		}

		/// <summary>
		/// IsGraphic reports whether the rune is defined as a Graphic by Unicode.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a lower case lette, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Such characters include letters, marks, numbers, punctuation, symbols, and
		/// spaces, from categories L, M, N, P, S, Zs.
		/// </remarks>
		public static bool IsGraphic (uint rune)
		{
			if (rune < MaxLatin1)
				return (properties [rune] & CharClass.pg) != 0;
			return IsRuneInRanges (rune, GraphicRanges);
		}

		/// <summary>
		/// IsPrint reports whether the rune is defined as printable.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a lower case lette, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Such characters include letters, marks, numbers, punctuation, symbols, and the
		/// ASCII space character, from categories L, M, N, P, S and the ASCII space
		/// character. This categorization is the same as IsGraphic except that the
		/// only spacing character is ASCII space, U+0020.
		/// </remarks>
		public static bool IsPrint (uint rune)
		{
			if (rune < MaxLatin1)
				return (properties [rune] & CharClass.pp) != 0;
			return IsRuneInRanges (rune, PrintRanges);
		}

		/// <summary>
		/// IsControl reports whether the rune is a control character.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a lower case lette, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// The C (Other) Unicode category includes more code points such as surrogates; use C.InRange (r) to test for them.
		/// </remarks>
		public static bool IsControl (uint rune)
		{
			if (rune < MaxLatin1)
				return (properties [rune] & CharClass.pC) != 0;
			// All control characters are < MaxLatin1.
			return false;
		}

		/// <summary>
		/// IsLetter reports whether the rune is a letter (category L).
		/// </summary>
		/// <returns><c>true</c>, if the rune is a letter, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// </remarks>
		public static bool IsLetter (uint rune)
		{
			if (rune < MaxLatin1)
				return (properties [rune] & CharClass.pLmask) != 0;
			return L.IsExcludingLatin (rune);
		}

		/// <summary>
		/// IsMark reports whether the rune is a letter (category M).
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Reports whether the rune is a mark character (category M).
		/// </remarks>
		public static bool IsMark (uint rune)
		{
			// There are no mark characters in Latin-1.
			return M.IsExcludingLatin (rune);
		}

		/// <summary>
		/// IsNumber reports whether the rune is a letter (category N).
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Reports whether the rune is a mark character (category N).
		/// </remarks>
		public static bool IsNumber (uint rune)
		{
			if (rune < MaxLatin1)
				return (properties [rune] & CharClass.pN) != 0;
			return N.IsExcludingLatin (rune);
		}

		/// <summary>
		/// IsPunct reports whether the rune is a letter (category P).
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Reports whether the rune is a mark character (category P).
		/// </remarks>
		public static bool IsPunct (uint rune)
		{
			if (rune < MaxLatin1)
				return (properties [rune] & CharClass.pP) != 0;
			return P.IsExcludingLatin (rune);
		}

		/// <summary>
		/// IsSpace reports whether the rune is a space character as defined by Unicode's White Space property.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// In the Latin-1 space, white space includes '\t', '\n', '\v', '\f', '\r', ' ', 
		/// U+0085 (NEL), U+00A0 (NBSP).
		/// Other definitions of spacing characters are set by category  Z and property Pattern_White_Space.
		/// </remarks>
		public static bool IsSpace (uint rune)
		{
			if (rune < MaxLatin1) {
				if (rune == '\t' || rune == '\n' || rune == '\v' || rune == '\f' || rune == '\r' || rune == ' ' || rune == 0x85 || rune == 0xa0)
					return true;
				return false;
			}
			return White_Space.IsExcludingLatin (rune);
		}

		/// <summary>
		/// IsSymbol reports whether the rune is a symbolic character.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		public static bool IsSymbol (uint rune)
		{
			if (rune < MaxLatin1)
				return (properties [rune] & CharClass.pS) != 0;
			return S.IsExcludingLatin (rune);
		}
	}
}
