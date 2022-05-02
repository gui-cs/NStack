using System;
using System.Runtime.InteropServices;

namespace System {
	/// <summary>
	/// A Rune represents a Unicode CodePoint storing the contents in a 32-bit value
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Rune {
		// Stores the rune
		uint value;

		/// <summary>
		/// Gets the rune unsigned integer value.
		/// </summary>
		public uint Value => value;

		/// <summary>
		/// The "error" Rune or "Unicode replacement character"
		/// </summary>
		public static Rune Error = new Rune (0xfffd);

		/// <summary>
		/// Maximum valid Unicode code point.
		/// </summary>
		public static Rune MaxRune = new Rune (0x10ffff);

		/// <summary>
		/// Characters below RuneSelf are represented as themselves in a single byte
		/// </summary>
		public const byte RuneSelf = 0x80;

		/// <summary>
		/// Represents invalid code points.
		/// </summary>
		public static Rune ReplacementChar = new Rune (0xfffd);

		/// <summary>
		/// Maximum number of bytes required to encode every unicode code point.
		/// </summary>
		public const int Utf8Max = 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Rune"/> from a unsigned integer.
		/// </summary>
		/// <param name="rune">Unsigned integer.</param>
		/// <remarks>
		/// The value does not have to be a valid Unicode code point, this API
		/// will create an instance of Rune regardless of the whether it is in 
		/// range or not.
		/// </remarks>
		public Rune (uint rune)
		{
			if (rune > maxRune)
			{
				throw new ArgumentOutOfRangeException("Value is beyond the supplementary range!");
			}
			this.value = rune;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Rune"/> from a character value.
		/// </summary>
		/// <param name="ch">C# characters.</param>
		public Rune (char ch)
		{
			this.value = (uint)ch;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Rune"/> from a surrogate pair value.
		/// </summary>
		/// <param name="highSurrogate">The high surrogate code point.</param>
		/// <param name="lowSurrogate">The low surrogate code point.</param>
		public Rune (uint highSurrogate, uint lowSurrogate)
		{
			if (EncodeSurrogatePair(highSurrogate, lowSurrogate, out Rune rune))
			{
				this.value = rune;
			}
			else if (highSurrogate < highSurrogateMin || lowSurrogate > lowSurrogateMax)
			{
				throw new ArgumentOutOfRangeException($"Must be between {highSurrogateMin:x} and {lowSurrogateMax:x} inclusive!");
			}
			else
			{
				throw new ArgumentOutOfRangeException($"Resulted rune must be less or equal to {(uint)MaxRune:x}!");
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:System.Rune"/> can be encoded as UTF-8
		/// </summary>
		/// <value><c>true</c> if is valid; otherwise, <c>false</c>.</value>
		public bool IsValid => ValidRune(value);

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:System.Rune"/> is a surrogate code point.
		/// </summary>
		/// <returns><c>true</c>If is a surrogate code point, <c>false</c>otherwise.</returns>
		public bool IsSurrogate => IsSurrogateRune(value);

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:System.Rune"/> is a valid surrogate pair.
		/// </summary>
		/// <returns><c>true</c>If is a valid surrogate pair, <c>false</c>otherwise.</returns>
		public bool IsSurrogatePair => DecodeSurrogatePair(value, out _);

		/// <summary>
		/// Check if the rune is a non-spacing character.
		/// </summary>
		/// <returns>True if is a non-spacing character, false otherwise.</returns>
		public bool IsNonSpacing => IsNonSpacingChar(value);

		// Code points in the surrogate range are not valid for UTF-8.
		const uint highSurrogateMin = 0xd800;
		const uint highSurrogateMax = 0xdbff;
		const uint lowSurrogateMin = 0xdc00;
		const uint lowSurrogateMax = 0xdfff;

		const uint maxRune = 0x10ffff;

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
			a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, a1, // 0x10-0x1F
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

		struct AcceptRange {
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
		/// DecodeRune unpacks the first UTF-8 encoding in p and returns the rune and
		/// its width in bytes. 
		/// </summary>
		/// <returns>If p is empty it returns (RuneError, 0). Otherwise, if
		/// the encoding is invalid, it returns (RuneError, 1). Both are impossible
		/// results for correct, non-empty UTF-8.
		/// </returns>
		/// <param name="buffer">Byte buffer containing the utf8 string.</param>
		/// <param name="start">Starting offset to look into..</param>
		/// <param name="n">Number of bytes valid in the buffer, or -1 to make it the length of the buffer.</param>
		public static (Rune rune, int Size) DecodeRune (byte [] buffer, int start = 0, int n = -1)
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
				return (Error, 0);

			var p0 = buffer [start];
			var x = first [p0];
			if (x >= a1) {
				// The following code simulates an additional check for x == xx and
				// handling the ASCII and invalid cases accordingly. This mask-and-or
				// approach prevents an additional branch.
				uint mask = (uint)((((byte)x) << 31) >> 31); // Create 0x0000 or 0xFFFF.
				return (new Rune ((buffer [start]) & ~mask | Error.value & mask), 1);
			}

			var sz = x & 7;
			var accept = AcceptRanges [x >> 4];
			if (n < (int)sz)
				return (Error, 1);

			var b1 = buffer [start + 1];
			if (b1 < accept.Lo || accept.Hi < b1)
				return (Error, 1);

			if (sz == 2)
				return (new Rune ((uint)((p0 & mask2)) << 6 | (uint)((b1 & maskx))), 2);

			var b2 = buffer [start + 2];
			if (b2 < locb || hicb < b2)
				return (Error, 1);

			if (sz == 3)
				return (new Rune ((uint)((p0 & mask3)) << 12 | (uint)((b1 & maskx)) << 6 | (uint)((b2 & maskx))), 3);

			var b3 = buffer [start + 3];
			if (b3 < locb || hicb < b3) {
				return (Error, 1);
			}
			return (new Rune ((uint)(p0 & mask4) << 18 | (uint)(b1 & maskx) << 12 | (uint)(b2 & maskx) << 6 | (uint)(b3 & maskx)), 4);
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
		/// <param name="end">Scan up to that point, if the value is -1, it sets the value to the length of the buffer.</param>
		/// <remarks>
		/// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
		/// out of range, or is not the shortest possible UTF-8 encoding for the
		/// value. No other validation is performed.</remarks> 
		public static (Rune rune, int size) DecodeLastRune (byte [] buffer, int end = -1)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			if (buffer.Length == 0)
				return (Error, 0);
			if (end == -1)
				end = buffer.Length;
			else if (end > buffer.Length)
				throw new ArgumentException ("The end goes beyond the size of the buffer");

			var start = end - 1;
			uint r = buffer [start];
			if (r < RuneSelf)
				return (new Rune (r), 1);
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
			Rune r2;
			(r2, size) = DecodeRune (buffer, start, end - start);
			if (start + size != end)
				return (Error, 1);
			return (r2, size);
		}

		/// <summary>
		/// number of bytes required to encode the rune.
		/// </summary>
		/// <returns>The length, or -1 if the rune is not a valid value to encode in UTF-8.</returns>
		/// <param name="rune">Rune to probe.</param>
		public static int RuneLen (Rune rune)
		{
			var rvalue = rune.value;
			if (rvalue <= rune1Max)
				return 1;
			if (rvalue <= rune2Max)
				return 2;
			if (highSurrogateMin <= rvalue && rvalue <= lowSurrogateMax)
				return -1;
			if (rvalue <= rune3Max)
				return 3;
			if (rvalue <= MaxRune.value)
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
		public static int EncodeRune (Rune rune, byte [] dest, int offset = 0)
		{
			if (dest == null)
				throw new ArgumentNullException (nameof (dest));
			var runeValue = rune.value;
			if (runeValue <= rune1Max) {
				dest [offset] = (byte)runeValue;
				return 1;
			}
			if (runeValue <= rune2Max) {
				dest [offset++] = (byte)(t2 | (byte)(runeValue >> 6));
				dest [offset] = (byte)(tx | (byte)(runeValue & maskx));
				return 2;
			}
			if ((runeValue > MaxRune.value) || (highSurrogateMin <= runeValue && runeValue <= lowSurrogateMax)) {
				// error
				dest [offset++] = 0xef;
				dest [offset++] = 0x3f;
				dest [offset] = 0x3d;
				return 3;
			}
			if (runeValue <= rune3Max) {
				dest [offset++] = (byte)(t3 | (byte)(runeValue >> 12));
				dest [offset++] = (byte)(tx | (byte)(runeValue >> 6) & maskx);
				dest [offset] = (byte)(tx | (byte)(runeValue) & maskx);
				return 3;
			}
			dest [offset++] = (byte)(t4 | (byte)(runeValue >> 18));
			dest [offset++] = (byte)(tx | (byte)(runeValue >> 12) & maskx);
			dest [offset++] = (byte)(tx | (byte)(runeValue >> 6) & maskx);
			dest [offset++] = (byte)(tx | (byte)(runeValue) & maskx);
			return 4;
		}

		/// <summary>
		/// Returns the number of runes in a utf8 encoded buffer
		/// </summary>
		/// <returns>Number of runes.</returns>
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
		/// Reports whether p consists entirely of valid UTF-8-encoded runes.
		/// </summary>
		/// <param name="buffer">Byte buffer containing a utf8 string.</param>
		public static bool Valid (byte [] buffer)
		{
			return InvalidIndex (buffer) == -1;
		}

		/// <summary>
		/// Use to find the index of the first invalid utf8 byte sequence in a buffer
		/// </summary>
		/// <returns>The index of the first invalid byte sequence or -1 if the entire buffer is valid.</returns>
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
		///  ValidRune reports whether a rune can be legally encoded as UTF-8.
		/// </summary>
		/// <returns><c>true</c>, if rune was validated, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test.</param>
		public static bool ValidRune (Rune rune)
		{
			if ((0 <= (int)rune.value && rune.value < highSurrogateMin) ||
				(lowSurrogateMax < rune.value && rune.value <= MaxRune.value))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Reports whether a rune is a surrogate code point.
		/// </summary>
		/// <param name="rune">The rune.</param>
		/// <returns><c>true</c>If is a surrogate code point, <c>false</c>otherwise.</returns>
		public static bool IsSurrogateRune(uint rune)
		{
			return rune >= highSurrogateMin && rune <= lowSurrogateMax;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:System.Rune"/> can be encoded as UTF-16 from a surrogate pair or zero otherwise.
		/// </summary>
		/// <param name="highsurrogate">The high surrogate code point.</param>
		/// <param name="lowSurrogate">The low surrogate code point.</param>
		/// <param name="rune">The returning rune.</param>
		/// <returns><c>True</c>if the returning rune is greater than 0 <c>False</c>otherwise.</returns>
		public static bool EncodeSurrogatePair(uint highsurrogate, uint lowSurrogate, out Rune rune)
		{
			rune = 0;
			if (highsurrogate >= highSurrogateMin && highsurrogate <= highSurrogateMax &&
				lowSurrogate >= lowSurrogateMin && lowSurrogate <= lowSurrogateMax)
			{
				//return 0x10000 + ((highsurrogate - highSurrogateMin) * 0x0400) + (lowSurrogate - lowSurrogateMin);
				return (rune = 0x10000 + ((highsurrogate - highSurrogateMin) << 10) + (lowSurrogate - lowSurrogateMin)) > 0;
			}
			return false;
		}

		/// <summary>
		/// Reports whether this <see cref="T:System.Rune"/> is a valid surrogate pair and can be decoded from UTF-16.
		/// </summary>
		/// <param name="rune">The rune</param>
		/// <param name="chars">The chars if is valid. Empty otherwise.</param>
		/// <returns><c>true</c>If is a valid surrogate pair, <c>false</c>otherwise.</returns>
		public static bool DecodeSurrogatePair(uint rune, out char [] chars)
		{
			uint s = rune - 0x10000;
			uint h = highSurrogateMin + (s >> 10);
			uint l = lowSurrogateMin + (s & 0x3FF);

			if (EncodeSurrogatePair (h, l, out Rune dsp) && dsp == rune)
			{
				chars = new char [] { (char)h, (char)l };
				return true;
			}
			chars = null;
			return false;
		}

		/// <summary>
		/// Reports whether this <see cref="T:System.Rune"/> is a valid surrogate pair and can be decoded from UTF-16.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="chars">The chars if is valid. Empty otherwise.</param>
		/// <returns><c>true</c>If is a valid surrogate pair, <c>false</c>otherwise.</returns>
		public static bool DecodeSurrogatePair(string str, out char [] chars)
		{
			if (str.Length == 2)
			{
				chars = str.ToCharArray();
				if (EncodeSurrogatePair(chars[0], chars[1], out _))
				{
					return true;
				}
			}
			chars = null;
			return false;
		}

		/// <summary>
		/// Given one byte from a utf8 string, return the number of expected bytes that make up the sequence.
		/// </summary>
		/// <returns>The number of UTF8 bytes expected given the first prefix.</returns>
		/// <param name="firstByte">Is the first byte of a UTF8 sequence.</param>
		public static int ExpectedSizeFromFirstByte(byte firstByte)
		{
			var x = first[firstByte];

			// Invalid runes, just return 1 for byte, and let higher level pass to print
			if (x == xx)
				return -1;
			if (x == a1)
				return 1;
			return x & 0xf;
		}

		/// <summary>
		/// IsDigit reports whether the rune is a decimal digit.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		public static bool IsDigit (Rune rune) => NStack.Unicode.IsDigit (rune.value);

		/// <summary>
		/// IsGraphic reports whether the rune is defined as a Graphic by Unicode.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a lower case letter, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Such characters include letters, marks, numbers, punctuation, symbols, and
		/// spaces, from categories L, M, N, P, S, Zs.
		/// </remarks>
		public static bool IsGraphic (Rune rune) => NStack.Unicode.IsGraphic (rune.value);

		/// <summary>
		/// IsPrint reports whether the rune is defined as printable.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a lower case letter, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Such characters include letters, marks, numbers, punctuation, symbols, and the
		/// ASCII space character, from categories L, M, N, P, S and the ASCII space
		/// character. This categorization is the same as IsGraphic except that the
		/// only spacing character is ASCII space, U+0020.
		/// </remarks>
		public static bool IsPrint (Rune rune) => NStack.Unicode.IsPrint (rune.value);


		/// <summary>
		/// IsControl reports whether the rune is a control character.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a lower case letter, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// The C (Other) Unicode category includes more code points such as surrogates; use C.InRange (r) to test for them.
		/// </remarks>
		public static bool IsControl (Rune rune) => NStack.Unicode.IsControl (rune.value);

		/// <summary>
		/// IsLetter reports whether the rune is a letter (category L).
		/// </summary>
		/// <returns><c>true</c>, if the rune is a letter, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// </remarks>
		public static bool IsLetter (Rune rune) => NStack.Unicode.IsLetter (rune.value);

		/// <summary>
		/// IsLetterOrDigit reports whether the rune is a letter (category L) or a digit.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a letter or digit, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// </remarks>
		public static bool IsLetterOrDigit (Rune rune) => NStack.Unicode.IsLetter (rune.value) || NStack.Unicode.IsDigit (rune.value);

		/// <summary>
		/// IsLetterOrDigit reports whether the rune is a letter (category L) or a number (category N).
		/// </summary>
		/// <returns><c>true</c>, if the rune is a letter or number, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// </remarks>
		public static bool IsLetterOrNumber (Rune rune) => NStack.Unicode.IsLetter (rune.value) || NStack.Unicode.IsNumber (rune.value);

		/// <summary>
		/// IsMark reports whether the rune is a letter (category M).
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Reports whether the rune is a mark character (category M).
		/// </remarks>
		public static bool IsMark (Rune rune) => NStack.Unicode.IsMark (rune.value);

		/// <summary>
		/// IsNumber reports whether the rune is a letter (category N).
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Reports whether the rune is a mark character (category N).
		/// </remarks>
		public static bool IsNumber (Rune rune) => NStack.Unicode.IsNumber (rune.value);

		/// <summary>
		/// IsPunct reports whether the rune is a letter (category P).
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		/// <remarks>
		/// Reports whether the rune is a mark character (category P).
		/// </remarks>
		public static bool IsPunctuation (Rune rune) => NStack.Unicode.IsPunct (rune.value);

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
		public static bool IsWhiteSpace (Rune rune) => NStack.Unicode.IsSpace (rune.value);

		/// <summary>
		/// IsSymbol reports whether the rune is a symbolic character.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a mark, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		public static bool IsSymbol (Rune rune) => NStack.Unicode.IsSymbol (rune.value);

		/// <summary>
		/// Reports whether the rune is an upper case letter.
		/// </summary>
		/// <returns><c>true</c>, if the rune is an upper case letter, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		public static bool IsUpper (Rune rune) => NStack.Unicode.IsUpper (rune.value);

		/// <summary>
		/// Reports whether the rune is a lower case letter.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a lower case letter, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		public static bool IsLower (Rune rune) => NStack.Unicode.IsLower (rune.value);

		/// <summary>
		/// Reports whether the rune is a title case letter.
		/// </summary>
		/// <returns><c>true</c>, if the rune is a lower case letter, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test for.</param>
		public static bool IsTitle (Rune rune) => NStack.Unicode.IsTitle (rune.value);

		/// <summary>
		/// The types of cases supported.
		/// </summary>
		public enum Case {
			/// <summary>
			/// Upper case
			/// </summary>
			Upper = 0,

			/// <summary>
			/// Lower case
			/// </summary>
			Lower = 1,

			/// <summary>
			/// Title case capitalizes the first letter, and keeps the rest in lowercase.
			/// Sometimes it is not as straight forward as the uppercase, some characters require special handling, like
			/// certain ligatures and Greek characters.
			/// </summary>
			Title = 2
		};

		// To maps the rune to the specified case: Case.Upper, Case.Lower, or Case.Title
		/// <summary>
		/// To maps the rune to the specified case: Case.Upper, Case.Lower, or Case.Title
		/// </summary>
		/// <returns>The cased character.</returns>
		/// <param name="toCase">The destination case.</param>
		/// <param name="rune">Rune to convert.</param>
		public static Rune To (Case toCase, Rune rune)
		{
			uint rval = rune.value;
			switch (toCase) {
			case Case.Lower: 
				return new Rune (NStack.Unicode.To (NStack.Unicode.Case.Lower, rval));
			case Case.Title:
				return new Rune (NStack.Unicode.To (NStack.Unicode.Case.Title, rval));
			case Case.Upper:
				return new Rune (NStack.Unicode.To (NStack.Unicode.Case.Upper, rval));
			}
			return ReplacementChar;
		}


		/// <summary>
		/// ToUpper maps the rune to upper case.
		/// </summary>
		/// <returns>The upper cased rune if it can be.</returns>
		/// <param name="rune">Rune.</param>
		public static Rune ToUpper (Rune rune) => NStack.Unicode.ToUpper (rune.value);

		/// <summary>
		/// ToLower maps the rune to lower case.
		/// </summary>
		/// <returns>The lower cased rune if it can be.</returns>
		/// <param name="rune">Rune.</param>
		public static Rune ToLower (Rune rune) => NStack.Unicode.ToLower (rune.value);

		/// <summary>
		/// ToLower maps the rune to title case.
		/// </summary>
		/// <returns>The lower cased rune if it can be.</returns>
		/// <param name="rune">Rune.</param>
		public static Rune ToTitle (Rune rune) => NStack.Unicode.ToTitle (rune.value);

		/// <summary>
		/// SimpleFold iterates over Unicode code points equivalent under
		/// the Unicode-defined simple case folding.
		/// </summary>
		/// <returns>The simple-case folded rune.</returns>
		/// <param name="rune">Rune.</param>
		/// <remarks>
		/// SimpleFold iterates over Unicode code points equivalent under
		/// the Unicode-defined simple case folding. Among the code points
		/// equivalent to rune (including rune itself), SimpleFold returns the
		/// smallest rune > r if one exists, or else the smallest rune >= 0.
		/// If r is not a valid Unicode code point, SimpleFold(r) returns r.
		///
		/// For example:
		/// <code>
		///      SimpleFold('A') = 'a'
		///      SimpleFold('a') = 'A'
		///
		///      SimpleFold('K') = 'k'
		///      SimpleFold('k') = '\u212A' (Kelvin symbol, K)
		///      SimpleFold('\u212A') = 'K'
		///
		///      SimpleFold('1') = '1'
		///
		///      SimpleFold(-2) = -2
		/// </code>
		/// </remarks>
		public static Rune SimpleFold (Rune rune) => NStack.Unicode.SimpleFold (rune.value);

		/// <summary>
		/// Implicit operator conversion from a rune to an unsigned integer
		/// </summary>
		/// <returns>The unsigned integer representation.</returns>
		/// <param name="rune">Rune.</param>
		public static implicit operator uint (Rune rune) => rune.value;

		/// <summary>
		/// Implicit operator conversion from a C# char into a rune.
		/// </summary>
		/// <returns>Rune representing the C# character</returns>
		/// <param name="ch">16-bit Character.</param>
		public static implicit operator Rune (char ch) => new Rune (ch);

		/// <summary>
		/// Implicit operator conversion from an unsigned integer into a rune.
		/// </summary>
		/// <returns>Rune representing the C# character</returns>
		/// <param name="value">32-bit unsigned integer.</param>
		public static implicit operator Rune (uint value) => new Rune (value);

		/// <summary>
		/// Serves as a hash function for a <see cref="T:System.Rune"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return (int)value;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Rune"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:System.Rune"/>.</returns>
		public override string ToString ()
		{
			var buff = new byte [4];
			var size = EncodeRune (this, buff, 0);
			return System.Text.Encoding.UTF8.GetString(buff, 0, size);
		}

		/// <summary>
		/// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:System.Rune"/>.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:System.Rune"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="T:System.Rune"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (Object obj)
		{
			// Check for null values and compare run-time types.
			if (obj == null)
				return false;

			Rune p = (Rune)obj;
			return p.value == value;
		}
	}
}
