//
// Code that interoperates with NStack.ustring.
using System;
using NStack;

namespace System {
	public partial struct Rune {
		/// <summary>
		/// FullRune reports whether the ustring begins with a full UTF-8 encoding of a rune.
		/// An invalid encoding is considered a full Rune since it will convert as a width-1 error rune.
		/// </summary>
		/// <returns><c>true</c>, if the bytes in p begin with a full UTF-8 encoding of a rune, <c>false</c> otherwise.</returns>
		/// <param name="str">The string to check.</param>
		public static bool FullRune (ustring str)
		{
			if ((object)str == null)
				throw new ArgumentNullException (nameof (str));
			var n = str.Length;

			if (n == 0)
				return false;
			var x = first [str [0]];
			if (n >= (x & 7)) {
				// ascii, invalid or valid
				return true;
			}
			// must be short or invalid
			if (n > 1) {
				var accept = AcceptRanges [x >> 4];
				var c = str [1];
				if (c < accept.Lo || accept.Hi < c)
					return true;
				else if (n > 2 && (str [2] < locb || hicb < str [2]))
					return true;
			}
			return false;
		}

		/// <summary>
		/// DecodeRune unpacks the first UTF-8 encoding in the ustring returns the rune and
		/// its width in bytes. 
		/// </summary>
		/// <returns>If p is empty it returns (RuneError, 0). Otherwise, if
		/// the encoding is invalid, it returns (RuneError, 1). Both are impossible
		/// results for correct, non-empty UTF-8.
		/// </returns>
		/// <param name="str">ustring to decode.</param>
		/// <param name="start">Starting offset to look into..</param>
		/// <param name="n">Number of bytes valid in the buffer, or -1 to make it the lenght of the buffer.</param>
		public static (Rune rune, int size) DecodeRune (ustring str, int start = 0, int n = -1)
		{
			if ((object)str == null)
				throw new ArgumentNullException (nameof (str));
			if (start < 0)
				throw new ArgumentException ("invalid offset", nameof (start));
			if (n < 0)
				n = str.Length - start;
			if (start > str.Length - n)
				throw new ArgumentException ("Out of bounds");

			if (n < 1)
				return (Error, 0);

			var p0 = str [start];
			var x = first [p0];
			if (x >= a1) {
				// The following code simulates an additional check for x == xx and
				// handling the ASCII and invalid cases accordingly. This mask-and-or
				// approach prevents an additional branch.
				uint mask = (uint)((((byte)x) << 31) >> 31); // Create 0x0000 or 0xFFFF.
				return (new Rune ((str [start]) & ~mask | Error.value & mask), 1);
			}

			var sz = x & 7;
			var accept = AcceptRanges [x >> 4];
			if (n < (int)sz)
				return (Error, 1);

			var b1 = str [start + 1];
			if (b1 < accept.Lo || accept.Hi < b1)
				return (Error, 1);

			if (sz == 2)
				return (new Rune ((uint)((p0 & mask2)) << 6 | (uint)((b1 & maskx))), 2);

			var b2 = str [start + 2];
			if (b2 < locb || hicb < b2)
				return (Error, 1);

			if (sz == 3)
				return (new Rune ((uint)((p0 & mask3)) << 12 | (uint)((b1 & maskx)) << 6 | (uint)((b2 & maskx))), 3);

			var b3 = str [start + 3];
			if (b3 < locb || hicb < b3) {
				return (Error, 1);
			}
			return (new Rune ((uint)(p0 & mask4) << 18 | (uint)(b1 & maskx) << 12 | (uint)(b2 & maskx) << 6 | (uint)(b3 & maskx)), 4);
		}


		/// <summary>
		/// DecodeLastRune unpacks the last UTF-8 encoding in the ustring.
		/// </summary>
		/// <returns>The last rune and its width in bytes.</returns>
		/// <param name="str">String to decode rune from;   if it is empty,
		/// it returns (RuneError, 0). Otherwise, if
		/// the encoding is invalid, it returns (RuneError, 1). Both are impossible
		/// results for correct, non-empty UTF-8.</param>
		/// <param name="end">Scan up to that point, if the value is -1, it sets the value to the lenght of the buffer.</param>
		/// <remarks>
		/// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
		/// out of range, or is not the shortest possible UTF-8 encoding for the
		/// value. No other validation is performed.</remarks> 
		public static (Rune rune, int size) DecodeLastRune (ustring str, int end = -1)
		{
			if ((object)str == null)
				throw new ArgumentNullException (nameof (str));
			if (str.Length == 0)
				return (Error, 0);
			if (end == -1)
				end = str.Length;
			else if (end > str.Length)
				throw new ArgumentException ("The end goes beyond the size of the buffer");

			var start = end - 1;
			uint r = str [start];
			if (r < RuneSelf)
				return (new Rune (r), 1);
			// guard against O(n^2) behavior when traversing
			// backwards through strings with long sequences of
			// invalid UTF-8.
			var lim = end - Utf8Max;

			if (lim < 0)
				lim = 0;

			for (start--; start >= lim; start--) {
				if (RuneStart (str [start])) {
					break;
				}
			}
			if (start < 0)
				start = 0;
			int size;
			Rune r2;
			(r2, size) = DecodeRune (str, start, end - start);
			if (start + size != end)
				return (Error, 1);
			return (r2, size);
		}


		/// <summary>
		/// Returns the number of runes in a ustring.
		/// </summary>
		/// <returns>Numnber of runes.</returns>
		/// <param name="str">utf8 string.</param>
		public static int RuneCount (ustring str)
		{
			if ((object)str == null)
				throw new ArgumentNullException (nameof (str));
			var count = str.Length;
			int n = 0;
			for (int i = 0; i < count;) {
				n++;
				var c = str [i];

				if (c < RuneSelf) {
					// ASCII fast path
					i++;
					continue;
				}
				var x = first [c];
				if (x == xx) {
					i++; // invalid.
					continue;
				}

				var size = (int)(x & 7);

				if (i + size > count) {
					i++; // Short or invalid.
					continue;
				}
				var accept = AcceptRanges [x >> 4];
				c = str [i + 1];
				if (c < accept.Lo || accept.Hi < c) {
					i++;
					continue;
				}
				if (size == 2) {
					i += 2;
					continue;
				}
				c = str [i + 2];
				if (c < locb || hicb < c) {
					i++;
					continue;
				}
				if (size == 3) {
					i += 3;
					continue;
				}
				c = str [i + 3];
				if (c < locb || hicb < c) {
					i++;
					continue;
				}
				i += size;

			}
			return n;
		}


		/// <summary>
		/// Use to find the index of the first invalid utf8 byte sequence in a buffer
		/// </summary>
		/// <returns>The index of the first insvalid byte sequence or -1 if the entire buffer is valid.</returns>
		/// <param name="str">String containing the utf8 buffer.</param>
		public static int InvalidIndex (ustring str)
		{
			if ((object)str == null)
				throw new ArgumentNullException (nameof (str));
			var n = str.Length;

			for (int i = 0; i < n;) {
				var pi = str [i];

				if (pi < RuneSelf) {
					i++;
					continue;
				}
				var x = first [pi];
				if (x == xx)
					return i; // Illegal starter byte.
				var size = (int)(x & 7);
				if (i + size > n)
					return i; // Short or invalid.
				var accept = AcceptRanges [x >> 4];

				var c = str [i + 1];

				if (c < accept.Lo || accept.Hi < c)
					return i;

				if (size == 2) {
					i += 2;
					continue;
				}
				c = str [i + 2];
				if (c < locb || hicb < c)
					return i;
				if (size == 3) {
					i += 3;
					continue;
				}
				c = str [i + 3];
				if (c < locb || hicb < c)
					return i;
				i += size;
			}
			return -1;
		}

		/// <summary>
		/// Reports whether the ustring consists entirely of valid UTF-8-encoded runes.
		/// </summary>
		/// <param name="str">String to validate.</param>
		public static bool Valid (ustring str)
		{
			return InvalidIndex (str) == -1;
		}


	}
}
