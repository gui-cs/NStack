﻿// 
// ustring.cs: UTF8 String representation
//
// Based on the Go UTF8 code
// 
// C# ification by Miguel de Icaza
//
using System;

namespace NStack
{
	/// <summary>
	/// UTF8 Helper methods and routines.
	/// </summary>
	/// <remarks>
	/// The term "rune" is used to represent a Unicode code point merely because it is a shorter way of talking about it.
	/// </remarks>
	public class Utf8
	{
		/// <summary>
		/// The "error" Rune or "Unicode replacement character"
		/// </summary>
		public static uint RuneError = 0xfffd;

		/// <summary>
		/// Characters below RuneSelf are represented as themselves in a single byte
		/// </summary>
		public const byte RuneSelf = 0x80;

		/// <summary>
		/// Maximum number of bytes required to encode every unicode code point.
		/// </summary>
		public const int Utf8Max = 4;

		/// <summary>
		/// Maximum valid Unicode code point.
		/// </summary>
		public const uint MaxRune = 0x10ffff;

		// Code points in the surrogate range are not valid for UTF-8.
		const uint surrogateMin = 0xd800;
		const uint surrogateMax = 0xdfff;

		const byte t1 = 0x00; // 0000 0000
		const byte tx = 0x80; // 1000 0000
		const byte t2 = 0xC0; // 1100 0000
		const byte t3 = 0xE0; // 1110 0000
		const byte t4 = 0xF0; // 1111 0000
		const byte t5 = 0xF8; // 1111 1000

		const byte maskx = 0x3F; // 0011 1111
		const byte mask2 = 0x1F; // 0001 1111
		const byte mask3 = 0x0F; // 0000 1111
		const byte mask4 = 0x07; // 0000 0111

		const uint rune1Max = (1 << 7) - 1;
		const uint rune2Max = (1 << 11) - 1;
		const uint rune3Max = (1 << 16) - 1;

		// The default lowest and highest continuation byte.
		const byte locb = 0x80; // 1000 0000
		const byte hicb = 0xBF; // 1011 1111

		// These names of these constants are chosen to give nice alignment in the
		// table below. The first nibble is an index into acceptRanges or F for
		// special one-byte ca1es. The second nibble is the Rune length or the
		// Status for the special one-byte ca1e.
		const byte xx = 0xF1; // invalid: size 1
		const byte a1 = 0xF0; // a1CII: size 1
		const byte s1 = 0x02; // accept 0, size 2
		const byte s2 = 0x13; // accept 1, size 3
		const byte s3 = 0x03; // accept 0, size 3
		const byte s4 = 0x23; // accept 2, size 3
		const byte s5 = 0x34; // accept 3, size 4
		const byte s6 = 0x04; // accept 0, size 4
		const byte s7 = 0x44; // accept 4, size 4

		static byte [] first = new byte [256]{
			//   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
			a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, // 0x00-0x0F
			a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1,a1, a1, a1, a1, a1, // 0x10-0x1F
			a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, // 0x20-0x2F
			a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, // 0x30-0x3F
			a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, // 0x40-0x4F
			a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, // 0x50-0x5F
			a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, // 0x60-0x6F
			a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, // 0x70-0x7F

			//   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
			xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0x80-0x8F
			xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0x90-0x9F
			xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xA0-0xAF
			xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xB0-0xBF
			xx, xx, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, // 0xC0-0xCF
			s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, // 0xD0-0xDF
			s2, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s4, s3, s3, // 0xE0-0xEF
			s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xF0-0xFF
		};

		struct AcceptRange
		{
			public byte Lo, Hi;
			public AcceptRange (byte lo, byte hi)
			{
				Lo = lo;
				Hi = hi;
			}
		}

		static AcceptRange [] AcceptRanges = new AcceptRange [] {
			new AcceptRange (locb, hicb),
			new AcceptRange (0xa0, hicb),
			new AcceptRange (locb, 0x9f),
			new AcceptRange (0x90, hicb),
			new AcceptRange (locb, 0x8f),
		};

		/// <summary>
		/// FullRune reports whether the bytes in p begin with a full UTF-8 encoding of a rune.
		/// An invalid encoding is considered a full Rune since it will convert as a width-1 error rune.
		/// </summary>
		/// <returns><c>true</c>, if the bytes in p begin with a full UTF-8 encoding of a rune, <c>false</c> otherwise.</returns>
		/// <param name="p">byte array.</param>
		public static bool FullRune (byte [] p)
		{
			if (p == null)
				throw new ArgumentNullException (nameof (p));
			var n = p.Length;

			if (n == 0)
				return false;
			var x = first [p [0]];
			if (n >= (x & 7)) {
				// ascii, invalid or valid
				return true;
			}
			// must be short or invalid
			if (n > 1) {
				var accept = AcceptRanges [x >> 4];
				var c = p [1];
				if (c < accept.Lo || accept.Hi < c)
					return true;
				else if (n > 2 && (p [2] < locb || hicb < p [2]))
					return true;
			}
			return false;
		}

		/// <summary>
		/// FullRune reports whether the ustring begins with a full UTF-8 encoding of a rune.
		/// An invalid encoding is considered a full Rune since it will convert as a width-1 error rune.
		/// </summary>
		/// <returns><c>true</c>, if the bytes in p begin with a full UTF-8 encoding of a rune, <c>false</c> otherwise.</returns>
		/// <param name="str">The string to check.</param>
		public static bool FullRune (ustring str)
		{
			if (str == null)
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
		/// DecodeRune unpacks the first UTF-8 encoding in p and returns the rune and
		/// its width in bytes. 
		/// </summary>
		/// <returns>If p is empty it returns (RuneError, 0). Otherwise, if
		/// the encoding is invalid, it returns (RuneError, 1). Both are impossible
		/// results for correct, non-empty UTF-8.
		/// </returns>
		/// <param name="buffer">Byte buffer containing the utf8 string.</param>
		/// <param name="start">Starting offset to look into..</param>
		/// <param name="n">Number of bytes valid in the buffer, or -1 to make it the lenght of the buffer.</param>
		public static (uint Rune, int Size) DecodeRune (byte [] buffer, int start = 0, int n = -1)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			if (start < 0)
				throw new ArgumentException ("invalid offset", nameof (start));
			if (n < 0)
				n = buffer.Length - start;
			if (start > buffer.Length - n)
				throw new ArgumentException ("Out of bounds");

			if (n < 1)
				return (RuneError, 0);

			var p0 = buffer [start];
			var x = first [p0];
			if (x >= a1) {
				// The following code simulates an additional check for x == xx and
				// handling the ASCII and invalid cases accordingly. This mask-and-or
				// approach prevents an additional branch.
				uint mask = (uint)((((byte)x) << 31) >> 31); // Create 0x0000 or 0xFFFF.
				return (((buffer [start]) & ~mask | RuneError & mask), 1);
			}

			var sz = x & 7;
			var accept = AcceptRanges [x >> 4];
			if (n < (int)sz)
				return (RuneError, 1);

			var b1 = buffer [start + 1];
			if (b1 < accept.Lo || accept.Hi < b1)
				return (RuneError, 1);

			if (sz == 2)
				return ((uint)((p0 & mask2)) << 6 | (uint)((b1 & maskx)), 2);

			var b2 = buffer [start + 2];
			if (b2 < locb || hicb < b2)
				return (RuneError, 1);

			if (sz == 3)
				return (((uint)((p0 & mask3)) << 12 | (uint)((b1 & maskx)) << 6 | (uint)((b2 & maskx))), 3);

			var b3 = buffer [start + 3];
			if (b3 < locb || hicb < b3) {
				return (RuneError, 1);
			}
			return ((uint)(p0 & mask4) << 18 | (uint)(b1 & maskx) << 12 | (uint)(b2 & maskx) << 6 | (uint)(b3 & maskx), 4);
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
		public static (uint Rune, int size) DecodeRune (ustring str, int start = 0, int n = -1)
		{
			if (str == null)
				throw new ArgumentNullException (nameof (str));
			if (start < 0)
				throw new ArgumentException ("invalid offset", nameof (start));
			if (n < 0)
				n = str.Length - start;
			if (start > str.Length - n)
				throw new ArgumentException ("Out of bounds");

			if (n < 1)
				return (RuneError, 0);

			var p0 = str [start];
			var x = first [p0];
			if (x >= a1) {
				// The following code simulates an additional check for x == xx and
				// handling the ASCII and invalid cases accordingly. This mask-and-or
				// approach prevents an additional branch.
				uint mask = (uint)((((byte)x) << 31) >> 31); // Create 0x0000 or 0xFFFF.
				return (((str [start]) & ~mask | RuneError & mask), 1);
			}

			var sz = x & 7;
			var accept = AcceptRanges [x >> 4];
			if (n < (int)sz)
				return (RuneError, 1);

			var b1 = str [start + 1];
			if (b1 < accept.Lo || accept.Hi < b1)
				return (RuneError, 1);

			if (sz == 2)
				return ((uint)((p0 & mask2)) << 6 | (uint)((b1 & maskx)), 2);

			var b2 = str [start + 2];
			if (b2 < locb || hicb < b2)
				return (RuneError, 1);

			if (sz == 3)
				return (((uint)((p0 & mask3)) << 12 | (uint)((b1 & maskx)) << 6 | (uint)((b2 & maskx))), 3);

			var b3 = str [start + 3];
			if (b3 < locb || hicb < b3) {
				return (RuneError, 1);
			}
			return ((uint)(p0 & mask4) << 18 | (uint)(b1 & maskx) << 12 | (uint)(b2 & maskx) << 6 | (uint)(b3 & maskx), 4);
		}

		// RuneStart reports whether the byte could be the first byte of an encoded,
		// possibly invalid rune. Second and subsequent bytes always have the top two
		// bits set to 10.
		static bool RuneStart (byte b) => (b & 0xc0) != 0x80;

		/// <summary>
		/// DecodeLastRune unpacks the last UTF-8 encoding in buffer
		/// </summary>
		/// <returns>The last rune and its width in bytes.</returns>
		/// <param name="buffer">Buffer to decode rune from;   if it is empty,
		/// it returns (RuneError, 0). Otherwise, if
		/// the encoding is invalid, it returns (RuneError, 1). Both are impossible
		/// results for correct, non-empty UTF-8.</param>
		/// <param name="end">Scan up to that point, if the value is -1, it sets the value to the lenght of the buffer.</param>
		/// <remarks>
		/// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
		/// out of range, or is not the shortest possible UTF-8 encoding for the
		/// value. No other validation is performed.</remarks> 
		public static (uint Rune, int size) DecodeLastRune (byte [] buffer, int end = -1)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			if (buffer.Length == 0)
				return (RuneError, 0);
			if (end == -1)
				end = buffer.Length;
			else if (end > buffer.Length)
				throw new ArgumentException ("The end goes beyond the size of the buffer");

			var start = end - 1;
			uint r = buffer [start];
			if (r < RuneSelf)
				return (r, 1);
			// guard against O(n^2) behavior when traversing
			// backwards through strings with long sequences of
			// invalid UTF-8.
			var lim = end - Utf8Max;

			if (lim < 0)
				lim = 0;

			for (start--; start >= lim; start--) {
				if (RuneStart (buffer [start])) {
					break;
				}
			}
			if (start < 0)
				start = 0;
			int size;
			(r, size) = DecodeRune (buffer, start, end - start);
			if (start + size != end)
				return (RuneError, 1);
			return (r, size);
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
		public static (uint Rune, int size) DecodeLastRune (ustring str, int end = -1)
		{
			if (str == null)
				throw new ArgumentNullException (nameof (str));
			if (str.Length == 0)
				return (RuneError, 0);
			if (end == -1)
				end = str.Length;
			else if (end > str.Length)
				throw new ArgumentException ("The end goes beyond the size of the buffer");

			var start = end - 1;
			uint r = str [start];
			if (r < RuneSelf)
				return (r, 1);
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
			(r, size) = DecodeRune (str, start, end - start);
			if (start + size != end)
				return (RuneError, 1);
			return (r, size);			
		}

		/// <summary>
		/// number of bytes required to encode the rune.
		/// </summary>
		/// <returns>The length, or -1 if the rune is not a valid value to encode in UTF-8.</returns>
		/// <param name="rune">Rune to probe.</param>
		public static int RuneLen (uint rune)
		{
			if (rune <= rune1Max)
				return 1;
			if (rune <= rune2Max)
				return 2;
			if (surrogateMin <= rune && rune <= surrogateMax)
				return -1;
			if (rune <= rune3Max)
				return 3;
			if (rune <= MaxRune)
				return 4;
			return -1;
		}

		/// <summary>
		/// Writes into the destination buffer starting at offset the UTF8 encoded version of the rune
		/// </summary>
		/// <returns>The number of bytes written into the destination buffer.</returns>
		/// <param name="rune">Rune to encode.</param>
		/// <param name="dest">Destination buffer.</param>
		/// <param name="offset">Offset into the destination buffer.</param>
		public static int EncodeRune (uint rune, byte [] dest, int offset = 0)
		{
			if (dest == null)
				throw new ArgumentNullException (nameof (dest));
			if (rune <= rune1Max) {
				dest [offset] = (byte)rune;
				return 1;
			}
			if (rune <= rune2Max) {
				dest [offset++] = (byte)(t2 | (byte)(rune >> 6));
				dest [offset] = (byte)(tx | (byte)(rune & maskx));
				return 2;
			}
			if ((rune > MaxRune) || (surrogateMin <= rune && rune <= surrogateMax)) {
				// error
				dest [offset++] = 0xef;
				dest [offset++] = 0x3f;
				dest [offset] = 0x3d;
				return 3;
			}
			if (rune <= rune3Max) {
				dest [offset++] = (byte)(t3 | (byte)(rune >> 12));
				dest [offset++] = (byte)(tx | (byte)(rune >> 6) & maskx);
				dest [offset] = (byte)(tx | (byte)(rune) & maskx);
				return 3;
			}
			dest [offset++] = (byte)(t4 | (byte)(rune >> 18));
			dest [offset++] = (byte)(tx | (byte)(rune >> 12) & maskx);
			dest [offset++] = (byte)(tx | (byte)(rune >> 6) & maskx);
			dest [offset++] = (byte)(tx | (byte)(rune) & maskx);
			return 4;
		}

		/// <summary>
		/// Returns the number of runes in a utf8 encoded buffer
		/// </summary>
		/// <returns>Numnber of runes.</returns>
		/// <param name="buffer">Byte buffer containing a utf8 string.</param>
		/// <param name="offset">Starting offset in the buffer.</param>
		/// <param name="count">Number of bytes to process in buffer, or -1 to process until the end of the buffer.</param>
		public static int RuneCount (byte [] buffer, int offset = 0, int count = -1)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			if (count == -1)
				count = buffer.Length;
			int n = 0;
			for (int i = offset; i < count;) {
				n++;
				var c = buffer [i];

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
				c = buffer [i + 1];
				if (c < accept.Lo || accept.Hi < c) {
					i++;
					continue;
				}
				if (size == 2) {
					i += 2;
					continue;
				}
				c = buffer [i + 2];
				if (c < locb || hicb < c) {
					i++;
					continue;
				}
				if (size == 3) {
					i += 3;
					continue;
				}
				c = buffer [i + 3];
				if (c < locb || hicb < c) {
					i++;
					continue;
				}
				i += size;

			}
			return n;
		}

		/// <summary>
		/// Returns the number of runes in a ustring.
		/// </summary>
		/// <returns>Numnber of runes.</returns>
		/// <param name="str">utf8 string.</param>
		public static int RuneCount (ustring str)
		{
			if (str == null)
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
		/// Reports whether p consists entirely of valid UTF-8-encoded runes.
		/// </summary>
		/// <param name="buffer">Byte buffer containing a utf8 string.</param>
		public static bool Valid (byte [] buffer)
		{
			return InvalidIndex (buffer) == -1;
		}

		/// <summary>
		/// Reports whether the ustring consists entirely of valid UTF-8-encoded runes.
		/// </summary>
		/// <param name="str">String to validate.</param>
		public static bool Valid (ustring str)
		{
			return InvalidIndex (str) == -1;
		}

		/// <summary>
		/// Use to find the index of the first invalid utf8 byte sequence in a buffer
		/// </summary>
		/// <returns>The index of the first insvalid byte sequence or -1 if the entire buffer is valid.</returns>
		/// <param name="buffer">Buffer containing the utf8 buffer.</param>
		public static int InvalidIndex (byte [] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			var n = buffer.Length;

			for (int i = 0; i < n;) {
				var pi = buffer [i];

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

				var c = buffer [i + 1];

				if (c < accept.Lo || accept.Hi < c)
					return i;

				if (size == 2) {
					i += 2;
					continue;
				}
				c = buffer [i + 2];
				if (c < locb || hicb < c)
					return i;
				if (size == 3) {
					i += 3;
					continue;
				}
				c = buffer [i + 3];
				if (c < locb || hicb < c)
					return i;
				i += size;
			}
			return -1;
		}

		/// <summary>
		/// Use to find the index of the first invalid utf8 byte sequence in a buffer
		/// </summary>
		/// <returns>The index of the first insvalid byte sequence or -1 if the entire buffer is valid.</returns>
		/// <param name="str">String containing the utf8 buffer.</param>
		public static int InvalidIndex (ustring str)
		{
			if (str == null)
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
		///  ValidRune reports whether a rune can be legally encoded as UTF-8.
		/// </summary>
		/// <returns><c>true</c>, if rune was valided, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test.</param>
		public static bool ValidRune (uint rune)
		{
			if (0 <= rune && rune < surrogateMin)
				return true;
			if (surrogateMax < rune && rune <= MaxRune)
				return true;
			return false;
		}
	}
}
