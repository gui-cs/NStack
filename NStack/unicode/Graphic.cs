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

#if false
		static RangeTable [] GraphicRanges {
			L, M, N, P, S, Zs
		};

		static RangeTable [] PrintRanges {
			L, M, N, P, S
		};

#endif
	}
}
