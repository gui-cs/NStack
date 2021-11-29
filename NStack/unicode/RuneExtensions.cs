//
// Code that interoperates with NStack.ustring.
using NStack;

namespace System
{
	/// <summary>
	/// Helper class that implements <see cref="System.Rune"/> extensions for the <see cref="NStack.ustring"/> type.
	/// </summary>
	public static class RuneExtensions
	{
		/// <summary>
		/// FullRune reports whether the ustring begins with a full UTF-8 encoding of a rune.
		/// An invalid encoding is considered a full Rune since it will convert as a width-1 error rune.
		/// </summary>
		/// <returns><c>true</c>, if the bytes in p begin with a full UTF-8 encoding of a rune, <c>false</c> otherwise.</returns>
		/// <param name="str">The string to check.</param>
		public static bool FullRune(this ustring str)
		{
			if ((object)str == null)
				throw new ArgumentNullException(nameof(str));

			foreach (var rune in str)
			{
				if (rune == Rune.Error)
				{
					return false;
				}
				ustring us = ustring.Make(rune);
				if (!Rune.FullRune(us.ToByteArray()))
				{
					return false;
				}
			}
			return true;
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
		/// <param name="n">Number of bytes valid in the buffer, or -1 to make it the length of the buffer.</param>
		public static (Rune rune, int size) DecodeRune(this ustring str, int start = 0, int n = -1)
		{
			if ((object)str == null)
				throw new ArgumentNullException(nameof(str));
			if (start < 0)
				throw new ArgumentException("invalid offset", nameof(start));
			if (n < 0)
				n = str.Length - start;
			if (start > str.Length - n)
				throw new ArgumentException("Out of bounds");

			return Rune.DecodeRune(str.ToByteArray(), start, n);
		}

		/// <summary>
		/// DecodeLastRune unpacks the last UTF-8 encoding in the ustring.
		/// </summary>
		/// <returns>The last rune and its width in bytes.</returns>
		/// <param name="str">String to decode rune from;   if it is empty,
		/// it returns (RuneError, 0). Otherwise, if
		/// the encoding is invalid, it returns (RuneError, 1). Both are impossible
		/// results for correct, non-empty UTF-8.</param>
		/// <param name="end">Scan up to that point, if the value is -1, it sets the value to the length of the buffer.</param>
		/// <remarks>
		/// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
		/// out of range, or is not the shortest possible UTF-8 encoding for the
		/// value. No other validation is performed.</remarks> 
		public static (Rune rune, int size) DecodeLastRune(this ustring str, int end = -1)
		{
			if ((object)str == null)
				throw new ArgumentNullException(nameof(str));
			if (str.Length == 0)
				return (Rune.Error, 0);
			if (end == -1)
				end = str.Length;
			else if (end > str.Length)
				throw new ArgumentException("The end goes beyond the size of the buffer");

			return Rune.DecodeLastRune(str.ToByteArray(), end);
		}

		/// <summary>
		/// Returns the number of runes in a ustring.
		/// </summary>
		/// <returns>Number of runes.</returns>
		/// <param name="str">utf8 string.</param>
		public static int RuneCount(this ustring str)
		{
			if ((object)str == null)
				throw new ArgumentNullException(nameof(str));

			return Rune.RuneCount(str.ToByteArray());
		}

		/// <summary>
		/// Use to find the index of the first invalid utf8 byte sequence in a buffer
		/// </summary>
		/// <returns>The index of the first invalid byte sequence or -1 if the entire buffer is valid.</returns>
		/// <param name="str">String containing the utf8 buffer.</param>
		public static int InvalidIndex(this ustring str)
		{
			if ((object)str == null)
				throw new ArgumentNullException(nameof(str));

			return Rune.InvalidIndex(str.ToByteArray());
		}

		/// <summary>
		/// Reports whether the ustring consists entirely of valid UTF-8-encoded runes.
		/// </summary>
		/// <param name="str">String to validate.</param>
		public static bool Valid(this ustring str)
		{
			return Rune.Valid(str.ToByteArray());
		}
	}
}
