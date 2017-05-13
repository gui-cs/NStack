// 
// ustring.cs: UTF8 String representation
//
// Based on the Go strings code
// 
// C# ification by Miguel de Icaza
//
// TODO:
//   Provide a string to utf8 conversion
//
// Topics:
//   * Should ustring use a Span-like container instead of a direct byte[]
//   * It would allow easy slicing at a cost.
//   * Perhaps use the slow-perf Span implementation?
//   * Should IndexOf, Contains, etc take C# strings as well or rely on implicit conversion?
//   
//
// ustring as a class: 
//     * Advanrage: could provide factory methods that return subclasses with
//       different behaviors, in particular to allow things like passing
//       deallocator methods for scenarios where we would allow byte * or IntPtr buffers
// 
// ustring as struct:
//     * If we only have the "buffer" field below, passing ustrings would be
//       very cheap
// 

using System;
namespace NStack
{

	/// <summary>
	/// Utf8 string representation
	/// </summary>
	public class ustring
	{
		byte [] buffer;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NStack.ustring"/> class from a byte array.
		/// </summary>
		/// <param name="buffer">Buffer containing the utf8 encoded string.</param>
		/// <remarks>
		/// No validation is performed on the contents of the byte buffer, so it
		/// might contains invalid UTF-8 sequences.
		/// </remarks>
		public ustring (byte [] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			this.buffer = buffer;
		}

		public ustring (uint rune)
		{
			var len = Utf8.RuneLen (rune);
			buffer = new byte [len];
			Utf8.EncodeRune (rune, buffer, 0);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="T:NStack.ustring"/> class from a byte array.
		/// </summary>
		/// <param name="buffer">Buffer containing the utf8 encoded string.</param>
		/// <param name="start">Starting offset into the buffer.</param>
		/// <param name="count">Number of bytes to consume from the buffer.</param>
		/// <remarks>
		/// No validation is performed on the contents of the byte buffer, so it
		/// might contains invalid UTF-8 sequences.
		/// 
		/// This will make a copy of the buffer range.
		/// </remarks>
		public ustring (byte [] buffer, int start, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			if (start < 0)
				throw new ArgumentNullException ("Expected a positive start value", nameof (start));

			if (count < 0)
				throw new ArgumentException ("Expected a positive value", nameof (count));
			this.buffer = new byte [count];
			Array.Copy (buffer, start, this.buffer, 0, count);
		}

		/// <summary>
		/// Returns the underlying array storying the utf8 encoded byte buffer.
		/// </summary>
		/// <value>The bytes.</value>
		public byte [] Bytes => buffer;

		/// <summary>
		/// Clone this instance and duplicates the undelying byte buffer contents.
		/// </summary>
		/// <returns>The clone.</returns>
		public ustring Clone ()
		{
			return new ustring ((byte [])buffer.Clone ());
		}

		public static ustring Concat (params ustring [] args)
		{
			if (args == null)
				throw new ArgumentNullException (nameof (args));
			var t = 0;
			foreach (var x in args)
				t += x.buffer.Length;
			var copy = new byte [t];
			int p = 0;

			foreach (var x in args) {
				var n = x.buffer.Length;
				Array.Copy (x.buffer, 0, copy, p, n);
				p += n;
			}
			return new ustring (copy);
		}

		/// <summary>
		/// Explode splits the string into a slice of UTF-8 strings
		/// </summary>
		/// <returns>, one string per unicode character, 
		/// up to the specified limit.</returns>
		/// <param name="limit">Maximum number of entries to return, or -1 for no limits.</param>
		public ustring [] Explode (int limit = -1)
		{
			var n = Utf8.RuneCount (this);
			if (n < 0 || n > limit)
				n = limit;
			var result = new ustring [n];
			int offset = 0;
			for (int i = 0; i < n - 1; i++) {
				(var rune, var size) = Utf8.DecodeRune (buffer, offset);
				if (rune == Utf8.RuneError)
					result [i] = new ustring (Utf8.RuneError);
				else {
					var substr = new byte [size];
					Array.Copy (buffer, offset, substr, 0, size);
					result [i] = new ustring (substr);
				}
				offset += size;
			}
			if (n > 0) {
				var r = new byte [buffer.Length - offset];
				Array.Copy (buffer, offset, r, 0, buffer.Length - offset);
				result [n - 1] = new ustring (r);
			}
			return result;
		}

		// primeRK is the prime base used in Rabin-Karp algorithm.
		const uint primeRK = 16777619;

		// hashStr returns the hash and the appropriate multiplicative
		// factor for use in Rabin-Karp algorithm.
		(uint hash, uint multFactor) HashStr ()
		{
			uint hash = 0;
			for (int i = 0; i < buffer.Length; i++)
				hash = hash * primeRK + (uint)(buffer [i]);

			uint pow = 0, sq = 1;
			for (int i = buffer.Length; i > 0; i >>= 1) {
				if ((i & 1) != 0)
					pow *= sq;
				sq *= sq;
			}
			return (hash, pow);
		}

		// hashStrRev returns the hash of the reverse of sep and the
		// appropriate multiplicative factor for use in Rabin-Karp algorithm.
		(uint hash, uint multFactor) RevHashStr ()
		{
			uint hash = 0;

			for (int i = buffer.Length - 1; i >= 0; i--) {
				hash = hash * primeRK + (uint)(buffer [i]);
			}

			uint pow = 0, sq = 1;

			for (int i = buffer.Length; i > 0; i >>= 1) {
				if ((i & 1) != 0) {
					pow *= sq;
				}
				sq *= sq;
			}
			return (hash, pow);
		}

		/// <summary>
		/// Count the number of non-overlapping instances of substr in the string.
		/// </summary>
		/// <returns>If substr is an empty string, Count returns 1 + the number of Unicode code points in the string, otherwise the count of non-overlapping instances in string.</returns>
		/// <param name="substr">Substr.</param>
		public int Count (ustring substr)
		{
			if (substr == null)
				throw new ArgumentNullException (nameof (substr));
			int n = 0;
			if (substr.buffer.Length == 0)
				return Utf8.RuneCount (buffer) + 1;
			int offset = 0;
			while (true) {
				var i = IndexOf (substr, offset);
				if (i == -1)
					return n;
				n++;
				offset += i + substr.buffer.Length;
			}
		}

		/// <summary>
		/// Returns a value indicating whether a specified substring occurs within this string.
		/// </summary>
		/// <returns>true if the <paramref name="substr" /> parameter occurs within this string, or if <paramref name="substr" /> is the empty string (""); otherwise, false.</returns>
		/// <param name="substr">The string to seek.</param>
		public bool Contains (ustring substr)
		{
			if (substr == null)
				throw new ArgumentNullException (nameof (substr));
			return IndexOf (substr) >= 0;
		}

		/// <summary>
		/// Returns a value indicating whether a specified rune occurs within this string.
		/// </summary>
		/// <returns>true if the <paramref name="rune" /> parameter occurs within this string; otherwise, false.</returns>
		/// <param name="rune">The rune to seek.</param>
		public bool Contains (uint rune)
		{
			return IndexOf (rune) >= 0;
		}

		/// <summary>
		/// Returns a value indicating whether any of the characters in the provided string occurs within this string.
		/// </summary>
		/// <returns>true if any of the characters in <paramref name="chars" /> parameter occurs within this string; otherwise, false.</returns>
		/// <param name="chars">string contanining one or more characters.</param>
		public bool ContainsAny (ustring chars)
		{
			if (chars == null)
				throw new ArgumentNullException (nameof (chars));
			return IndexOfAny (chars) >= 0;
		}

		/// <summary>
		/// Returns a value indicating whether any of the runes occurs within this string.
		/// </summary>
		/// <returns>true if any of the runes in <paramref name="runes" /> parameter occurs within this string; otherwise, false.</returns>
		/// <param name="runes">one or more runes.</param>
		public bool ContainsAny (params uint [] runes)
		{
			return IndexOfAny (runes) >= 0;
		}

		static bool CompareArrayRange (byte [] first, int firstStart, int firstCount, byte [] second)
		{
			for (int i = 0; i < firstCount; i++)
				if (first [i + firstStart] != second [i])
					return false;
			return true;
		}

		public int IndexOf (ustring substr, int offset = 0)
		{
			if (substr == null)
				throw new ArgumentNullException (nameof (substr));
			
			var n = substr.buffer.Length;
			if (n == 0)
				return 0;
			var blen = buffer.Length;
			if (offset >= blen)
				throw new ArgumentException ("The offset is larger than the ustring size");
			
			if (n == 1)
				return IndexByte (substr.buffer [offset]);
			blen -= offset;
			if (n == blen) {
				// If the offset is zero, we can compare identity
				if (offset == 0) {
					if (substr == this || substr.buffer == buffer)
						return 0;
				}

				if (CompareArrayRange (substr.buffer, offset, n, buffer))
					return 0;
				return -1;
			}
			if (n > blen)
				return -1;
			// Rabin-Karp search
			(var hashss, var pow) = HashStr ();
			uint h = 0;

			for (int i = 0; i < n; i++)
				h = h * primeRK + (uint)(buffer [i + offset]);

			if (h == hashss && CompareArrayRange (buffer, offset, n, substr.buffer))
				return 0;
			
			for (int i = n; i < blen;) {
				var reali = offset + i;
				h *= primeRK;

				h += (uint)buffer [reali];

				h -= pow * (uint)(buffer [reali - n]);
				i++;

				if (h == hashss && CompareArrayRange (buffer, reali - n, n, substr.buffer))
					return reali - n;
			}
			return -1;
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence of a specified substring within this instance
		/// </summary>
		/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
		/// <param name="substr">The ustring to seek.</param>
		public int LastIndexOf (ustring substr)
		{
			if (substr == null)
				throw new ArgumentNullException (nameof (substr));
			var n = substr.buffer.Length;
			if (n == 0)
				return buffer.Length;
			if (n == 1)
				return LastIndexByte (substr.buffer [0]);
			if (n == buffer.Length) {
				if (substr == this || substr.buffer == buffer)
					return 0;

				if (CompareArrayRange (substr.buffer, 0, n, buffer))
					return 0;
				return -1;
			}
			if (n > buffer.Length)
				return -1;

			// Rabin-Karp search from the end of the string
			(var hashss, var pow) = substr.RevHashStr ();
			var last = buffer.Length - n;
			uint h = 0;
			for (int i = buffer.Length - 1; i >= last; i--)
				h = h * primeRK + (uint)buffer [i];

			if (h == hashss && CompareArrayRange (buffer, last, buffer.Length - last, substr.buffer))
				return last;

			for (int i = last - 1; i >= 0; i--) {
				h *= primeRK;
				h += (uint)(buffer [i]);
				h -= pow * (uint)(buffer [i + n]);
				if (h == hashss && CompareArrayRange (buffer, i, n, substr.buffer))
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence of a specified byte on the underlying byte buffer.
		/// </summary>
		/// <returns>The zero-based index position of <paramref name="b" /> if that byte is found, or -1 if it is not.  </returns>
		/// <param name="b">The byte to seek.</param>
		public int LastIndexByte (byte b)
		{
			for (int i = buffer.Length - 1; i >= 0; i--)
				if (buffer [i] == b)
					return i;
			return -1;
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified Unicode rune in this string
		/// </summary>
		/// <returns>The zero-based index position of <paramref name="rune" /> if that character is found, or -1 if it is not.  If the rune is Utf8.RuneError, it returns the first instance of any invalid UTF-8 byte sequence.</returns>
		/// <param name="rune">Rune.</param>
		public int IndexOf (uint rune)
		{
			if (0 <= rune && rune < Utf8.RuneSelf)
				return IndexByte ((byte)rune);
			if (rune == Utf8.RuneError)
				return Utf8.InvalidIndex (buffer);
			if (!Utf8.ValidRune (rune))
				return -1;
			return IndexOf (new ustring (rune));
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified byte in the underlying byte buffer.
		/// </summary>
		/// <returns>The zero-based index position of <paramref name="b" /> if that byte is found, or -1 if it is not.  </returns>
		/// <param name="b">The byte to seek.</param>
		public int IndexByte (byte b)
		{
			var t = buffer.Length;
			unsafe
			{
				fixed (byte* p = &buffer [0]) {
					for (int i = 0; i < t; i++)
						if (p [i] == b)
							return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence in this instance of any character in the provided string
		/// </summary>
		/// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="chars" /> was found; -1 if no character in <paramref name="chars" /> was found.</returns>
		/// <param name="chars">ustring containing characters to seek.</param>
		public int IndexOfAny (ustring chars)
		{
			if (chars == null)
				throw new ArgumentNullException (nameof (chars));
			if (chars.buffer.Length == 0)
				return -1;
			var blen = buffer.Length;
			if (blen > 8) {
				AsciiSet aset;
				if (AsciiSet.MakeAsciiSet (ref aset, chars)) {
					for (int i = 0; i < blen; i++)
						if (AsciiSet.Contains (ref aset, buffer [i]))
							return i;
					return -1;
				}
			}
			var clen = chars.buffer.Length;

			for (int i = 0; i < blen;) {
				(var rune, var size) = Utf8.DecodeRune (buffer, i, i - blen);
				for (int j = 0; j < clen; ) {
					(var crune, var csize) = Utf8.DecodeRune (chars.buffer, j, j - clen);
					if (crune == rune)
						return i;
					j += csize;
				}
				i += size;
			}
			return -1;
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence in this instance of any runes in the provided string
		/// </summary>
		/// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="runes" /> was found; -1 if no character in <paramref name="runes" /> was found.</returns>
		/// <param name="runes">ustring containing runes.</param>
		public int IndexOfAny (params uint [] runes)
		{
			if (runes == null)
				throw new ArgumentNullException (nameof (runes));
			if (runes.Length == 0)
				return -1;
			var blen = buffer.Length;
			if (blen > 8) {
				AsciiSet aset;
				if (AsciiSet.MakeAsciiSet (ref aset, runes)) {
					for (int i = 0; i < blen; i++)
						if (AsciiSet.Contains (ref aset, buffer [i]))
							return i;
					return -1;
				}
			}
			var clen = runes.Length;
			for (int i = 0; i < blen;) {
				(var rune, var size) = Utf8.DecodeRune (buffer, i, i - blen);
				for (int j = 0; j < clen; j++) {
					if (rune == runes [j])
						return i;
				}
				i += size;
			}

			return -1;
		}

		/// <summary>
		/// Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in the uustring.
		/// </summary>
		/// <returns>The index position of the last occurrence in this instance where any character in <paramref name="chars" /> was found; -1 if no character in <paramref name="chars" /> was found.</returns>
		/// <param name="chars">The string containing characters to seek.</param>
		public int LastIndexOfAny (ustring chars)
		{
			if (chars == null)
				throw new ArgumentNullException (nameof (chars));
			if (chars.buffer.Length == 0)
				return -1;
			var blen = buffer.Length;
			if (blen > 8) {
				AsciiSet aset;
				if (AsciiSet.MakeAsciiSet (ref aset, chars)) {
					for (int i = blen - 1; i >= 0; i--)
						if (AsciiSet.Contains (ref aset, buffer [i]))
					    		return i;
					return -1;
				}
			}
			var clen = chars.buffer.Length;
			for (int i = blen - 1; i >= 0;) {
				(var rune, var size) = Utf8.DecodeLastRune (buffer, i);
				i -= size;

				for (int j = 0; j < clen;) {
					(var crune, var csize) = Utf8.DecodeRune (chars.buffer, j, j - clen);
					if (crune == rune)
						return i;
					j += csize;
				}
			}
			return -1;
		}

		// asciiSet is a 32-byte value, where each bit represents the presence of a
		// given ASCII character in the set. The 128-bits of the lower 16 bytes,
		// starting with the least-significant bit of the lowest word to the
		// most-significant bit of the highest word, map to the full range of all
		// 128 ASCII characters. The 128-bits of the upper 16 bytes will be zeroed,
		// ensuring that any non-ASCII character will be reported as not in the set.
		struct AsciiSet
		{
			unsafe internal fixed uint ascii [8];

			public static bool MakeAsciiSet (ref AsciiSet aset, ustring chars)
			{
				var n = chars.buffer.Length;
				unsafe
				{
					fixed (uint* ascii = aset.ascii) {
						for (int i = 0; i < n; i++) {
							var c = chars.buffer [i];
							if (c >= Utf8.RuneSelf)
								return false;

							var t = (uint)(1 << (c & 31));
							ascii [c >> 5] |= t;
						}
					}
				}
				return true;
			}

			public static bool MakeAsciiSet (ref AsciiSet aset, uint [] runes)
			{
				var n = runes.Length;
				unsafe
				{
					fixed (uint* ascii = aset.ascii) {
						for (int i = 0; i < n; i++) {
							var r = runes [i];
							if (r >= Utf8.RuneSelf)
								return false;
							byte c = (byte)r;
							var t = (uint)(1 << (c & 31));
							ascii [c >> 5] |= t;
						}
					}
				}
				return true;
			}
			public static bool Contains (ref AsciiSet aset, byte b)
			{
				unsafe
				{
					fixed (uint* ascii = aset.ascii) {
						return (ascii [b >> 5] & (1 << (b & 31))) != 0;
					}
				}
			}
		}
	}
}
