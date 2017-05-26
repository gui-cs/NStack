using System;
namespace NStack {
	public partial class Unicode {
		/// <summary>
		/// IsDigit reports whether the rune is a decimal digit.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		public static bool IsDigit (uint rune)
		{
			if (rune < MaxLatin1)
				return '0' <= rune && rune <= '9';
			return Digit.IsExcludingLatin (rune);
		}	
	}
}
