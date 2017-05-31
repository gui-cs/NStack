// 
// ustring.cs: UTF8 String representation
//
// Based on the Go strings code
// 
// C# ification by Miguel de Icaza
//
// TODO:
//   Clone()?
//   Implement GetHashCode for the subclasses
// 
// genSplit
// 
// TODO from .NET API:
// String.Split members (array of strings, StringSplitOptions)
// 


using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace NStack {
	class ubstring : ustring, IDisposable {
		IntPtr block;
		readonly int size;
		Action<ustring, IntPtr> release;

		public ubstring (IntPtr block, int size, Action<ustring,IntPtr> releaseFunc = null)
		{
			if (block == IntPtr.Zero)
				throw new ArgumentException ("Null pointer passed", nameof (block));
			if (size < 0)
				throw new ArgumentException ("Invalid size passed", nameof (size));
			this.size = size;
			this.block = block;
			this.release = releaseFunc;
		}

		public override int Length => size;
		public override byte this [int index] {
			get {
				if (index < 0 || index > size)
					throw new ArgumentException (nameof (index));
				return Marshal.ReadByte (block, index);
			}
		}

		public override void CopyTo (int offset, byte [] target, int targetOffset, int count)
		{
			if (offset < 0 || offset >= size)
				throw new ArgumentException (nameof (offset));
			if (count < 0)
				throw new ArgumentException (nameof (count));
			if (offset + count > size)
				throw new ArgumentException (nameof (count));
			unsafe
			{
				byte* p = ((byte*)block) + offset;

				for (int i = 0; i < count; i++, p++)
					target [i] = *p;
			}
		}

		public override string ToString ()
		{
			unsafe
			{
				return Encoding.UTF8.GetString ((byte*)block, size);
			}
		}

		protected override ustring GetRange (int start, int end)
		{
			unsafe {
				return new ubstring ((IntPtr)((byte*)block + start), end - start, null);
			}
		}

		public override int IndexByte (byte b)
		{
			var t = size;
			unsafe {
				byte* p = (byte*)block;
				for (int i = 0; i < t; i++){
					if (p [i] == b)
						return i;
				}
			}
			return -1;
		}

		public override byte [] ToByteArray ()
		{
			var copy = new byte [size];
			Marshal.Copy (block, copy, 0, size);
			return copy;
		}

		#region IDisposable Support
		protected virtual void Dispose (bool disposing)
		{
			if (block != IntPtr.Zero) {
				if (release != null)
					release (this, block);
				release = null;
				block = IntPtr.Zero;
			}
		}

		~ubstring() {
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}

	class sstring : ustring {
		readonly byte [] buffer;

		public sstring (byte [] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			this.buffer = buffer;
		}

		public sstring (uint rune)
		{
			var len = Utf8.RuneLen (rune);
			buffer = new byte [len];
			Utf8.EncodeRune (rune, buffer, 0);
		}

		public sstring (string str)
		{
			if (str == null)
				throw new ArgumentNullException (nameof (str));
			buffer = Encoding.UTF8.GetBytes (str);
		}

		public sstring (params char [] chars)
		{
			if (chars == null)
				throw new ArgumentNullException (nameof (chars));
			buffer = Encoding.UTF8.GetBytes (chars);
		}

		public override int Length => buffer.Length;
		public override byte this [int index] {
			get {
				return buffer [index];
			}
		}

		public override void CopyTo (int offset, byte [] target, int targetOffset, int count)
		{
			Array.Copy (buffer, offset, target, targetOffset, count);
		}

		public override string ToString ()
		{
			return Encoding.UTF8.GetString (buffer);
		}

		public override int IndexByte (byte b)
		{
			var t = Length;
			unsafe	{
				fixed (byte* p = &buffer [0]) {
					for (int i = 0; i < t; i++)
						if (p [i] == b)
							return i;
				}
			}
			return -1;
		}

		protected override ustring GetRange (int start, int end)
		{
			return new rstring (buffer, start, end-start);
		}

		public override byte [] ToByteArray ()
		{
			return buffer;
		}
	}

	class rstring : ustring {
		readonly byte [] buffer;
		readonly int start, count;

		public rstring (byte [] buffer, int start, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			if (start < 0)
				throw new ArgumentException (nameof (start));
			if (count < 0)
				throw new ArgumentException (nameof (count));
			if (start >= buffer.Length)
				throw new ArgumentException (nameof (start));
			if (buffer.Length - count < start)
				throw new ArgumentException (nameof (count));
			this.start = start;
			this.count = count;
			this.buffer = buffer;
		}

		public override int Length => count;
		public override byte this [int index] {
			get {
				if (index < 0 || index >= count)
					throw new ArgumentException (nameof (index));
				return buffer [start + index];
			}
		}

		public override void CopyTo (int offset, byte [] target, int targetOffset, int count)
		{
			if (offset < 0)
				throw new ArgumentException (nameof (offset));

			Array.Copy (buffer, offset + start, target, targetOffset, count);
		}

		public override string ToString ()
		{
			return Encoding.UTF8.GetString (buffer, start, count);
		}

		public override int IndexByte (byte b)
		{
			var t = count;
			unsafe
			{
				fixed (byte* p = &buffer [start]) {
					for (int i = 0; i < t; i++)
						if (p [i] == b)
							return i;
				}
			}
			return -1;
		}

		protected override ustring GetRange (int start, int end)
		{
			return new rstring (buffer, start + this.start, end - start);
		}

		public override byte [] ToByteArray ()
		{
			var copy = new byte [count];
			Array.Copy (buffer, sourceIndex: start, destinationArray: copy, destinationIndex: 0, length: count);
			return copy;
		}
	}

	/// <summary>
	/// ustrings provide a series of operations on a series of bytes that contain either valid or invalid UTF8 byte sequences.
	/// </summary>
	/// <remarks>
	/// The ustring provides a series of string-like operations over an array of bytes.   The buffer
	/// is expected to contain an UTF8 encoded string, but if the buffer contains an invalid utf8
	/// sequence many of the operations will continue to work.
	/// 
	/// The strings can be created either from byte arrays, a range within a byte array, or from a 
	/// block of unmanaged memory.  The ustrings are created using the <see cref="Make"/> methods in the
	/// class, not by invoking the new operator on the class.
	/// 
	/// The Length property describes the lenght in bytes of the underlying array, while the RuneCount 
	/// property describes the number of code points (or runes) that are reprenseted by the underlying 
	/// utf8 encoded buffer.
	/// 
	/// The ustring supports slicing by calling the indexer with two arguments, the argument represent
	/// indexes into the underlying byte buffer.  The starting index is inclusive, while the ending index
	/// is exclusive.   Negative values can be used to index the string from the end.  See the documentation
	/// for the indexer for more details.
	/// 
	/// </remarks>
	public abstract class ustring : IComparable {
		/// <summary>
		/// The empty ustring.
		/// </summary>
		public static ustring Empty = new sstring (Array.Empty<byte> ());

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NStack.ustring"/> class using the provided byte array for its storage.
		/// </summary>
		/// <param name="buffer">Buffer containing the utf8 encoded string.</param>
		/// <remarks>
		/// No validation is performed on the contents of the byte buffer, so it
		/// might contains invalid UTF-8 sequences.
		/// 
		/// No copy is made of the incoming byte buffer, so changes to it will be visible on the ustring.
		/// </remarks>
		public static ustring Make (params byte [] buffer)
		{
			return new sstring (buffer);
		}

		/// <summary>
		/// Initializes a new instance using the provided rune as the sole character in the string.
		/// </summary>
		/// <param name="rune">Rune (short name for Unicode code point).</param>
		public static ustring Make (uint rune)
		{
			return new sstring (rune);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NStack.ustring"/> class from a string.
		/// </summary>
		/// <param name="str">C# String.</param>
		public static ustring Make (string str)
		{
			return new sstring (str);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NStack.ustring"/> class from an array of C# characters.
		/// </summary>
		/// <param name="chars">Characters.</param>
		public static ustring Make (params char [] chars)
		{
			return new sstring (chars);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NStack.ustring"/> class from a block of memory and a size.
		/// </summary>
		/// <param name="block">Pointer to a block of memory.</param>
		/// <param name="size">Number of bytes in the block to treat as a string.</param>
		/// <param name="releaseFunc">Optional method to invoke to release when this string is finalized to clear the associated resources, you can use this for example to release the unamanged resource to which the block belongs.</param>
		/// <remarks>
		/// This will return a ustring that represents the block of memory provided.
		/// 
		/// The returned object will be a subclass of ustring that implements IDisposable, which you can use
		/// to trigger the synchronous execution of the <paramref name="releaseFunc"/>.   If you do not call
		/// Dispose manually, the provided release function will be invoked from the finalizer thread.
		/// </remarks>
		public static ustring Make (IntPtr block, int size, Action<ustring, IntPtr> releaseFunc = null)
		{
			return new ubstring (block, size, releaseFunc);
		}

		/// <summary>
		/// Determines whether a specified instance of <see cref="NStack.ustring"/> is equal to another specified <see cref="NStack.ustring"/>, this means that the contents of the string are identical
		/// </summary>
		/// <param name="a">The first <see cref="NStack.ustring"/> to compare.</param>
		/// <param name="b">The second <see cref="NStack.ustring"/> to compare.</param>
		/// <returns><c>true</c> if <c>a</c> and <c>b</c> are equal; otherwise, <c>false</c>.</returns>
		public static bool operator == (ustring a, ustring b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals (a, b)) {
				return true;
			}

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null)) {
				return false;
			}

			var alen = a.Length;
			var blen = b.Length;
			if (alen != blen)
				return false;
			for (int i = 0; i < alen; i++)
				if (a [i] != b [i])
					return false;
			return true;
		}

		/// <summary>
		/// Determines whether a specified instance of <see cref="NStack.ustring"/> is not equal to another specified <see cref="NStack.ustring"/>.
		/// </summary>
		/// <param name="a">The first <see cref="NStack.ustring"/> to compare.</param>
		/// <param name="b">The second <see cref="NStack.ustring"/> to compare.</param>
		/// <returns><c>true</c> if <c>a</c> and <c>b</c> are not equal; otherwise, <c>false</c>.</returns>
		public static bool operator != (ustring a, ustring b)
		{
			// If both are null, or both are same instance, return false
			if (System.Object.ReferenceEquals (a, b)) {
				return false;
			}

			// If one is null, but not both, return true.
			if (((object)a == null) || ((object)b == null)) {
				return true;
			}

			var alen = a.Length;
			var blen = b.Length;
			if (alen != blen)
				return true;
			for (int i = 0; i < alen; i++)
				if (a [i] == b [i])
					return false;
			return true;
		}

		/// <summary>
		/// Implicit conversion from a C# string into a ustring.
		/// </summary>
		/// <returns>The ustring with the same contents as the string.</returns>
		/// <param name="str">The string to encode as a ustring.</param>
		/// <remarks>
		/// This will allocate a byte array and copy the contents of the 
		/// string encoded as UTF8 into it.
		/// </remarks>
		public static implicit operator ustring (string str)
		{
			return new sstring (str);
		}

		/// <summary>
		/// Implicit conversion from a byte array into a ustring.
		/// </summary>
		/// <returns>The ustring wrapping the existing byte array.</returns>
		/// <param name="buffer">The buffer containing the data.</param>
		/// <remarks>
		/// The returned string will keep a reference to the buffer, which 
		/// means that changes done to the buffer will be reflected into the
		/// ustring.
		/// </remarks>
		public static implicit operator ustring (byte [] buffer)
		{
			return new sstring (buffer);
		}

		public override int GetHashCode ()
		{
			return (int)HashStr ().hash;
		}

		/// <summary>
		/// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:NStack.ustring"/>.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:NStack.ustring"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="T:NStack.ustring"/>;
		/// otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			// If parameter is null return false.
			if (obj == null)
				return false;

			// If parameter cannot be cast to Point return false.
			ustring p = obj as ustring;
			if ((object)p == null)
				return false;

			return this == p;
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
		public static ustring Make (byte [] buffer, int start, int count)
		{
			return new rstring (buffer, start, count);
		}

		/// <summary>
		/// Gets the length in bytes of the byte buffer.
		/// </summary>
		/// <value>The length in bytes of the encoded UTF8 string, does not represent the number of runes.</value>
		/// <remarks>To obtain the number of runes in the string, use the <see cref="P:System.ustring.RuneCount"/> property.</remarks>
		public abstract int Length { get; }

		/// <summary>
		/// Returns the byte at the specified position.
		/// </summary>
		/// <value>The byte encoded at the specified position.</value>
		/// <remarks>The index value shoudl be between 0 and Length-1.</remarks>
		public abstract byte this [int index] { get; }

		protected abstract ustring GetRange (int start, int end);

		/// <summary>
		/// Returns a slice of the ustring delimited by the [start, end) range.  If the range is invalid, the return is the Empty string.
		/// </summary>
		/// <param name="start">Start index, this value is inclusive.   If the value is negative, the value is added to the length, allowing this parameter to count to count from the end of the string.</param>
		/// <param name="end">End index, this value is exclusive.   If the value is negative, the value is added to the length, plus one, allowing this parameter to count from the end of the string.   If the value is zero, the end is computed as the last index of the string.</param>
		/// <remarks>
		/// Some examples given the string "1234567890":
		/// 
		/// The range [0, 4] produces "1234"
		/// The range [8, 10] produces "90"
		/// The range [8, 0] produces "90"
		/// The range [-2, 0] produces "90"
		/// The range [8, 9] produces "9"
		/// The range [-4, -1] produces "789"
		/// The range [-4, 0] produces "7890"
		/// The range [-4, 0] produces "7890"
		/// The range [-9, -3] produces "234567"
		/// 
		/// This indexer does not raise exceptions for invalid indexes, instead the value 
		/// returned is the ustring.Empty value:
		/// 
		/// The range [100, 200] produces the ustring.Empty
		/// The range [-100, 0] produces ustring.Empty
		/// </remarks>
		public ustring this [int start, int end] {
			get {
				int size = Length;
				if (end <= 0) {
					if (end == 0)
						end = size;
					else
						end = size + end;
				}
				if (start < 0)
					start = size + start;

				if (start < 0 || start >= size || start >= end)
					return Empty;
				if (end < 0 || end > size)
					return Empty;
				return GetRange (start, end);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:NStack.ustring"/> is empty.
		/// </summary>
		/// <value><c>true</c> if is empty (Lenght is zero); otherwise, <c>false</c>.</value>
		public bool IsEmpty => Length == 0;

		/// <summary>
		/// Gets the rune count of the string.
		/// </summary>
		/// <value>The rune count.</value>
		public int RuneCount => Utf8.RuneCount (this);

		/// <summary>
		/// Copies the specified number of bytes from the the underlying ustring representation to the target array at the specified offset.
		/// </summary>
		/// <param name="offset">Offset in the underlying ustring buffer to copy from.</param>
		/// <param name="target">Target array where the buffer contents will be copied to.</param>
		/// <param name="targetOffset">Offset into the target array where this will be copied to.</param>
		/// <param name="count">Number of bytes to copy.</param>
		public abstract void CopyTo (int offset, byte [] target, int targetOffset, int count);

		/// <summary>
		/// Returns a version of the ustring as a byte array, it might allocate or return the internal byte buffer, depending on the backing implementation.
		/// </summary>
		/// <returns>A byte array containing the contents of the ustring.</returns>
		/// <remarks>
		/// The byte array contains either a copy of the underlying data, in the cases where the ustring was created
		/// from an unmanaged pointer or when the ustring was created by either slicing or from a range withing a byte
		/// array.   Otherwise the returned array that is used by the ustring itself.
		/// </remarks>
		public abstract byte [] ToByteArray ();

		/// <summary>
		/// Concatenates the provided ustrings into a new ustring.
		/// </summary>
		/// <returns>A new ustring that contains the concatenation of all the ustrings.</returns>
		/// <param name="args">One or more ustrings.</param>
		public static ustring Concat (params ustring [] args)
		{
			if (args == null)
				throw new ArgumentNullException (nameof (args));
			var t = 0;
			foreach (var x in args)
				t += x.Length;
			var copy = new byte [t];
			int p = 0;

			foreach (var x in args) {
				var n = x.Length;
				x.CopyTo (offset: 0, target: copy, targetOffset: p, count: n);
				p += n;
			}
			return new sstring (copy);
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
			if (limit < 0 || n > limit)
				limit = n;
			var result = new ustring [limit];
			int offset = 0;
			for (int i = 0; i < limit - 1; i++) {
				(var rune, var size) = Utf8.DecodeRune (this, offset);
				if (rune == Utf8.RuneError)
					result [i] = new sstring (Utf8.RuneError);
				else {
					var substr = new byte [size];

					CopyTo (offset: offset, target: substr, targetOffset: 0, count: size);
					result [i] = new sstring (substr);
				}
				offset += size;
			}
			if (n > 0) {
				var r = new byte [Length - offset];

				CopyTo (offset: offset, target: r, targetOffset: 0, count: Length - offset);
				result [n - 1] = new sstring (r);
			}
			return result;
		}

		/// <summary>
		/// Converts a ustring into a rune array.
		/// </summary>
		/// <returns>An array containing the runes for the string up to the specified limit.</returns>
		/// <param name="limit">Maximum number of entries to return, or -1 for no limits.</param>
		public uint [] ToRunes (int limit = -1)
		{
			var n = Utf8.RuneCount (this);
			if (limit < 0 || n > limit)
				limit = n;
			var result = new uint [limit];
			int offset = 0;
			for (int i = 0; i < limit; i++) {
				(var rune, var size) = Utf8.DecodeRune (this, offset);
				result [i] = rune;
				offset += size;
			}
			return result;
		}

		// primeRK is the prime base used in Rabin-Karp algorithm.
		const uint primeRK = 16777619;

		// hashStr returns the hash and the appropriate multiplicative
		// factor for use in Rabin-Karp algorithm.
		internal (uint hash, uint multFactor) HashStr ()
		{
			uint hash = 0;
			int count = Length;
			for (int i = 0; i < count; i++)
				hash = hash * primeRK + (uint)(this [i]);

			uint pow = 0, sq = 1;
			for (int i = count; i > 0; i >>= 1) {
				if ((i & 1) != 0)
					pow *= sq;
				sq *= sq;
			}
			return (hash, pow);
		}

		// hashStrRev returns the hash of the reverse of sep and the
		// appropriate multiplicative factor for use in Rabin-Karp algorithm.
		internal (uint hash, uint multFactor) RevHashStr ()
		{
			uint hash = 0;

			int count = Length;
			for (int i = count - 1; i >= 0; i--) {
				hash = hash * primeRK + (uint)(this [i]);
			}

			uint pow = 0, sq = 1;

			for (int i = count; i > 0; i >>= 1) {
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
			if (substr.Length == 0)
				return Utf8.RuneCount (substr) + 1;
			int offset = 0;
			while (true) {
				var i = IndexOf (substr, offset);
				if (i == -1)
					return n;
				n++;
				offset += i + substr.Length;
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

		static bool CompareStringRange (ustring first, int firstStart, int firstCount, ustring second)
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
			
			var n = substr.Length;
			if (n == 0)
				return 0;
			var blen = Length;
			if (offset >= blen)
				throw new ArgumentException ("The offset is larger than the ustring size");
			
			if (n == 1)
				return IndexByte (substr [offset]);
			blen -= offset;
			if (n == blen) {
				// If the offset is zero, we can compare identity
				if (offset == 0) {
					if (((object)substr == (object)this))
						return 0;
				}

				if (CompareStringRange (substr, offset, n, this))
					return 0;
				return -1;
			}
			if (n > blen)
				return -1;
			// Rabin-Karp search
			(var hashss, var pow) = HashStr ();
			uint h = 0;

			for (int i = 0; i < n; i++)
				h = h * primeRK + (uint)(this [i + offset]);

			if (h == hashss && CompareStringRange (this, offset, n, substr))
				return 0;
			
			for (int i = n; i < blen;) {
				var reali = offset + i;
				h *= primeRK;

				h += (uint)this [reali];

				h -= pow * (uint)(this [reali - n]);
				i++;

				if (h == hashss && CompareStringRange (this, reali - n, n, substr))
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
			var n = substr.Length;
			if (n == 0)
				return Length;
			if (n == 1)
				return LastIndexByte (substr [0]);
			if (n == Length) {
				if (((object)substr == (object) this))
					return 0;

				if (CompareStringRange (substr, 0, n, this))
					return 0;
				return -1;
			}
			if (n > Length)
				return -1;

			// Rabin-Karp search from the end of the string
			(var hashss, var pow) = substr.RevHashStr ();
			var last = Length - n;
			uint h = 0;
			for (int i = Length - 1; i >= last; i--)
				h = h * primeRK + (uint)this [i];

			if (h == hashss && CompareStringRange (this, last, Length - last, substr))
				return last;

			for (int i = last - 1; i >= 0; i--) {
				h *= primeRK;
				h += (uint)(this [i]);
				h -= pow * (uint)(this [i + n]);
				if (h == hashss && CompareStringRange (this, i, n, substr))
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
			for (int i = Length - 1; i >= 0; i--)
				if (this [i] == b)
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
				return Utf8.InvalidIndex (this);
			if (!Utf8.ValidRune (rune))
				return -1;
			return IndexOf (new sstring (rune));
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified byte in the underlying byte buffer.
		/// </summary>
		/// <returns>The zero-based index position of <paramref name="b" /> if that byte is found, or -1 if it is not.  </returns>
		/// <param name="b">The byte to seek.</param>
		public abstract int IndexByte (byte b);

		/// <summary>
		/// Reports the zero-based index of the first occurrence in this instance of any character in the provided string
		/// </summary>
		/// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="chars" /> was found; -1 if no character in <paramref name="chars" /> was found.</returns>
		/// <param name="chars">ustring containing characters to seek.</param>
		public int IndexOfAny (ustring chars)
		{
			if (chars == null)
				throw new ArgumentNullException (nameof (chars));
			if (chars.Length == 0)
				return -1;
			var blen = Length;
			if (blen > 8) {
				AsciiSet aset;
				if (AsciiSet.MakeAsciiSet (ref aset, chars)) {
					for (int i = 0; i < blen; i++)
						if (AsciiSet.Contains (ref aset, this [i]))
							return i;
					return -1;
				}
			}
			var clen = chars.Length;

			for (int i = 0; i < blen;) {
				(var rune, var size) = Utf8.DecodeRune (this, i, i - blen);
				for (int j = 0; j < clen; ) {
					(var crune, var csize) = Utf8.DecodeRune (this, j, j - clen);
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
			var blen = Length;
			if (blen > 8) {
				AsciiSet aset;
				if (AsciiSet.MakeAsciiSet (ref aset, runes)) {
					for (int i = 0; i < blen; i++)
						if (AsciiSet.Contains (ref aset, this [i]))
							return i;
					return -1;
				}
			}
			var clen = runes.Length;
			for (int i = 0; i < blen;) {
				(var rune, var size) = Utf8.DecodeRune (this, i, i - blen);
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
			if (chars.Length == 0)
				return -1;
			var blen = Length;
			if (blen > 8) {
				AsciiSet aset;
				if (AsciiSet.MakeAsciiSet (ref aset, chars)) {
					for (int i = blen - 1; i >= 0; i--)
						if (AsciiSet.Contains (ref aset, this [i]))
					    		return i;
					return -1;
				}
			}
			var clen = chars.Length;
			for (int i = blen - 1; i >= 0;) {
				(var rune, var size) = Utf8.DecodeLastRune (this, i);
				i -= size;

				for (int j = 0; j < clen;) {
					(var crune, var csize) = Utf8.DecodeRune (this, j, j - clen);
					if (crune == rune)
						return i;
					j += csize;
				}
			}
			return -1;
		}

		/// <summary>
		/// Implements the IComparable.CompareTo method
		/// </summary>
		/// <returns>Less than zero if this instance is less than value, zero if they are the same, and higher than zero if the instance is greater.</returns>
		/// <param name="value">Value.</param>
		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			var other = value as ustring;
			if (other == null)
				throw new ArgumentException ("Argument must be a ustring");
			var blen = Length;
			var olen = other.Length;
			if (blen == 0) {
				if (olen == 0)
					return 0;
				return -1;
			} else if (olen == 0)
				return 1;

			// Most common case, first character is different
			var e = this [0] - other [0];
			if (e != 0)
				return e;
			for (int i = 1; i < blen; i++) {
				if (i >= olen)
					return 1;
				e = this [i] - other [i];
				if (e == 0)
					continue;
				return e;
			}
			if (olen > blen)
				return -1;
			return 0;
		}

		// Generic split: splits after each instance of sep,
		// including sepSave bytes of sep in the subarrays.
		ustring [] GenSplit (ustring sep, int sepSave, int n = -1)
		{
			if (n == 0)
				return Array.Empty<ustring> ();
			if (sep == "")
				return Explode (n);
			if (n < 0)
				n = Count (sep) + 1;
			var result = new ustring [n];
			n--;
			int offset = 0, i = 0;
			while (i < n) {
				var m = IndexOf (sep);
				if (m < 0)
					break;
				result [i] = this [offset, m+sepSave];
				offset += m + sep.Length;
				i++;
			}
			result [i] = this [offset, Length - offset];
			return result;
		}

		/// <summary>
		/// Determines whether the beginning of this string instance matches the specified string.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="value" /> matches the beginning of this string; otherwise, <c>false</c>.</returns>
		/// <param name="prefix">Prefix.</param>
		public bool StartsWith (ustring prefix)
		{
			if (prefix == null)
				throw new ArgumentNullException (nameof (prefix));
			if (Length < prefix.Length)
				return false;
			return CompareStringRange (this, 0, prefix.Length, prefix);	
		}

		/// <summary>
		/// Determines whether the end of this string instance matches the specified string.
		/// </summary>
		/// <returns>true if <paramref name="suffix" /> matches the end of this instance; otherwise, false.</returns>
		/// <param name="suffix">The string to compare to the substring at the end of this instance.</param>
		public bool EndsWith (ustring suffix)
		{
			if (suffix == null)
				throw new ArgumentNullException (nameof (suffix));
			if (Length < suffix.Length)
				return false;
			return CompareStringRange (this, Length - suffix.Length, suffix.Length, suffix);
		}

		/// <summary>
		/// Concatenates all the elements of a ustring array, using the specified separator between each element.
		/// </summary>
		/// <returns>A string that consists of the elements in <paramref name="values" /> delimited by the <paramref name="separator" /> string. If <paramref name="values" /> is an empty array, the method returns <see cref="F:System.ustring.Empty" />.</returns>
		/// <param name="separator">Separator.</param>
		/// <param name="values">Values.</param>
		public static ustring Join (ustring separator, params ustring [] values)
		{
			if (separator == null)
				separator = Empty;
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length == 0)
				return Empty;
			int size = 0, items = 0;
			foreach (var t in values) {
				if (t == null)
					continue;
				size += t.Length;
				items++;
			}
			if (items == 1) {
				foreach (var t in values)
					if (t != null)
						return t;
			}
			var slen = separator.Length;
			size += (items - 1) * slen;
			var result = new byte [size];
			int offset = 0;
			foreach (var t in values) {
				if (t == null)
					continue;
				var tlen = t.Length;
				t.CopyTo (offset: 0, target: result, targetOffset: offset, count: tlen);
				offset += tlen;
				separator.CopyTo (offset: 0, target: result, targetOffset: offset, count: slen);
				offset += slen;
			}
			return new sstring (result);
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
				var n = chars.Length;
				unsafe
				{
					fixed (uint* ascii = aset.ascii) {
						for (int i = 0; i < n; i++) {
							var c = chars [i];
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

		/// <summary>
		/// Concatenates the contents of two <see cref="NStack.ustring"/> instances.
		/// </summary>
		/// <param name="u1">The first <see cref="NStack.ustring"/> to add, can be null.</param>
		/// <param name="u2">The second <see cref="NStack.ustring"/> to add, can be null.</param>
		/// <returns>The <see cref="T:NStack.ustring"/> that is the concatenation of the strings of <c>u1</c> and <c>u2</c>.</returns>
		public static ustring operator + (ustring u1, ustring u2)
		{
			var u1l = u1 == null ? 0 : u1.Length;
			var u2l = u2 == null ? 0 : u2.Length;
			var copy = new byte [u1l + u2l];
			if (u1 != null)
				u1.CopyTo (offset: 0, target: copy, targetOffset: 0, count: u1l);
			if (u2 != null)
				u2.CopyTo (offset: 0, target: copy, targetOffset: u1l, count: u2l);
			return new sstring (copy);
		}

		/// <summary>
		/// An enumerator that returns the index within the string, and the rune found at that location
		/// </summary>
		/// <returns>Enumerable object that can be used to iterate and get the index of the values at the same time.</returns>
		/// <remarks>
		/// This is useful to iterate over the string and obtain both the index of the rune and the rune
		/// in the same call.  
		/// </remarks>
		public IEnumerable<(int index, uint rune)> Range ()
		{
			int blen = Length;
			for (int i = 0; i < blen;) {
				(var rune, var size) = Utf8.DecodeRune (this, i, i - blen);
				yield return (i, rune);
				i += size;
			}
			yield break;
		}

		// Map returns a copy of the string s with all its characters modified
		// according to the mapping function. If mapping returns a negative value, the character is
		// dropped from the string with no replacement.
		static ustring Map (Func<uint, uint> mapping, ustring s, Action scanReset = null)
		{
			// In the worst case, the string can grow when mapped, making
			// things unpleasant. But it's so rare we barge in assuming it's
			// fine. It could also shrink but that falls out naturally.

			// nbytes is the number of bytes needed to encode the string
			int nbytes = 0;

			bool modified = false;
			int blen = s.Length;
			for (int offset = 0; offset < blen;) {
				(var rune, var size) = Utf8.DecodeRune (s, offset);
				var mapped = mapping (rune);
				if (mapped == rune) {
					nbytes++;
					continue;
				}
				modified = true;
				var mappedLen = Utf8.RuneLen (mapped);
				if (mappedLen == -1)
					mappedLen = 3; // Errors are encoded with 3 bytes
				nbytes += mappedLen;

				if (rune == Utf8.RuneError) {
					// RuneError is the result of either decoding
					// an invalid sequence or '\uFFFD'. Determine
					// the correct number of bytes we need to advance.
					(_, size) = Utf8.DecodeRune (s [offset, 0]);

				}
				offset += size;
			}


			if (!modified)
				return s;

			scanReset?.Invoke ();

			var result = new byte [nbytes];
			int targetOffset = 0;
			for (int offset = 0; offset < blen;) {
				(var rune, var size) = Utf8.DecodeRune (s, offset);
				offset += size;

				var mapped = mapping (rune);

				// common case
				if (0 < mapped && mapped <= Utf8.RuneSelf){
					result [targetOffset] = (byte)mapped;
					targetOffset++;
					continue;
				}

				targetOffset += Utf8.EncodeRune (mapped, dest: result, offset: targetOffset);
			}
			return new sstring (result);
		}

		/// <summary>
		/// Returns a copy of the string s with all Unicode letters mapped to their upper case.
		/// </summary>
		/// <returns>The string to uppercase.</returns>
		public ustring ToUpper ()
		{
			return Map (Unicode.ToUpper, this);
		}

		/// <summary>
		/// Returns a copy of the string s with all Unicode letters mapped to their upper case giving priority to the special casing rules.
		/// </summary>
		/// <returns>The string to uppercase.</returns>
		public ustring ToUpper (Unicode.SpecialCase specialCase)
		{
			return Map ((rune) => specialCase.ToUpper (rune), this);
		}

		/// <summary>
		/// Returns a copy of the string s with all Unicode letters mapped to their lower case.
		/// </summary>
		/// <returns>The lowercased string.</returns>
		public ustring ToLower ()
		{
			return Map (Unicode.ToLower, this);
		}

		/// <summary>
		/// Returns a copy of the string s with all Unicode letters mapped to their lower case giving priority to the special casing rules.
		/// </summary>
		/// <returns>The string to uppercase.</returns>
		public ustring ToLower (Unicode.SpecialCase specialCase)
		{
			return Map ((rune) => specialCase.ToLower (rune), this);
		}
		/// <summary>
		/// Returns a copy of the string s with all Unicode letters mapped to their title case.
		/// </summary>
		/// <returns>The title-cased string.</returns>
		public ustring ToTitle ()
		{
			return Map (Unicode.ToTitle, this);
		}

		/// <summary>
		/// Returns a copy of the string s with all Unicode letters mapped to their title case giving priority to the special casing rules.
		/// </summary>
		/// <returns>The string to uppercase.</returns>
		public ustring ToTitle (Unicode.SpecialCase specialCase)
		{
			return Map ((rune) => specialCase.ToTitle (rune), this);
		}

		/// <summary>
		/// IsSeparator reports whether the rune could mark a word boundary.
		/// </summary>
		/// <returns><c>true</c>, if the rune can be considered a word boundary, <c>false</c> otherwise.</returns>
		/// <param name="rune">The rune to test.</param>
		public static bool IsSeparator (uint rune)
		{
			if (rune <= 0x7f) {
				// ASCII alphanumerics and underscore are not separators
				if ('0' <= rune && rune <= '9')
					return false;
				if ('a' <= rune && rune <= 'z')
					return false;
				if ('A' <= rune && rune <= 'Z')
					return false;
				if (rune == '_')
					return false;
				return true;
			}
			// Letters and digits are not separators
			if (Unicode.IsLetter (rune) || Unicode.IsDigit (rune))
				return false;
			// Otherwise, all we can do for now is treat spaces as separators.
			return Unicode.IsSpace (rune);
		}

		public ustring Title ()
		{
			uint prev = ' ';
			return Map ((rune) => {
				if (IsSeparator (prev)) {
					prev = rune;
					return Unicode.ToTitle (rune);
				}
				prev = rune;
				return rune;
			}, this, () => { prev = ' '; });
		}

		// IndexFunc returns the index into s of the first Unicode
		// code point satisfying f(c), or -1 if none do.

		/// <summary>
		/// Rune predicate functions take a rune as input and return a boolean determining if the rune matches or not.
		/// </summary>
		public delegate bool RunePredicate (uint rune);

		/// <summary>
		/// IndexOf returns the index into s of the first Unicode rune satisfying matchFunc(rune), or -1 if none do.
		/// </summary>
		/// <returns>The index inside the string where the rune is found, or -1 on error.</returns>
		/// <param name="matchFunc">Match func, it receives a rune as a parameter and should return true if it matches, false otherwise.</param>
		public int IndexOf (RunePredicate matchFunc)
		{
			return FlexIndexOf (matchFunc, true);
		}

		/// <summary>
		/// LastIndexOf returns the index into s of the last Unicode rune satisfying matchFunc(rune), or -1 if none do.
		/// </summary>
		/// <returns>The last index inside the string where the rune is found, or -1 on error.</returns>
		/// <param name="matchFunc">Match func, it receives a rune as a parameter and should return true if it matches, false otherwise.</param>
		public int LastIndexOf (RunePredicate matchFunc)
		{
			return FlexLastIndexOf (matchFunc, true);
		}


		/// <summary>
		/// Returns a slice of the string with all leading runes matching the predicate removed.
		/// </summary>
		/// <returns>The current string if the predicate does not match anything, or a slice of the string starting in the first rune after the predicate matched.</returns>
		/// <param name="predicate">Function that determines whether this character must be trimmed.</param>
		public ustring TrimStart (RunePredicate predicate)
		{
			var i = IndexOf (predicate);
			if (i == -1)
				return this;
			return this [i, 0];
		}

		RunePredicate MakeCutSet (ustring cutset)
		{
			if (cutset.Length == 1 && cutset [0] < Utf8.RuneSelf)
				return (x) => x == (uint)cutset [0];
			AsciiSet aset;
			if (AsciiSet.MakeAsciiSet (ref aset, cutset)) {
				return (x) => x < Utf8.RuneSelf && AsciiSet.Contains (ref aset, (byte)x);
			}
			return (x) => cutset.IndexOf (x) >= 0;
		}

		/// <summary>
		/// TrimStarts returns a slice of the string with all leading characters in cutset removed.
		/// </summary>
		/// <returns>The slice of the string with all cutset characters removed.</returns>
		/// <param name="cutset">Characters to remove.</param>
		public ustring TrimStart (ustring cutset)
		{
			if (IsEmpty || cutset == null || cutset.IsEmpty)
				return this;
			return TrimStart (MakeCutSet (cutset));
		}

		/// <summary>
		/// TrimEnd returns a slice of the string with all leading characters in cutset removed.
		/// </summary>
		/// <returns>The slice of the string with all cutset characters removed.</returns>
		/// <param name="cutset">Characters to remove.</param>
		public ustring TrimEnd (ustring cutset)
		{
			if (IsEmpty || cutset == null || cutset.IsEmpty)
				return this;
			return TrimEnd (MakeCutSet (cutset));
		}

		public ustring TrimSpace ()
		{
			return Trim (Unicode.IsSpace);
		}

		/// <summary>
		/// Returns a slice of the string with all trailing runes matching the predicate removed.
		/// </summary>
		/// <returns>The current string if the predicate does not match anything, or a slice of the string starting in the first rune after the predicate matched.</returns>
		/// <param name="predicate">Function that determines whether this character must be trimmed.</param>
		public ustring TrimEnd (RunePredicate predicate)
		{
			var i = LastIndexOf (predicate);
			if (i >= 0 && this [i] >= Utf8.RuneSelf) {
				(var rune, var wid) = Utf8.DecodeRune (this [i, 0]);
				i += wid;
			} else
				i++;
			return this [0, i];
		}

		/// <summary>
		/// Returns a slice of the string with all leading and trailing runes matching the predicate removed.
		/// </summary>
		/// <returns>The trim.</returns>
		/// <param name="predicate">Predicate.</param>
		public ustring Trim (RunePredicate predicate)
		{
			return TrimStart (predicate).TrimEnd (predicate);
		}

		// FlexIndexOf is a generalization of IndexOf that allows
		// the desired result of the predicate to be specified.
		int FlexIndexOf (RunePredicate matchFunc, bool expected)
		{
			int blen = Length;
			for (int i = 0; i < blen;) {
				(var rune, var size) = Utf8.DecodeRune (this, i, i - blen);
				if (matchFunc (rune) == expected)
					return i;
				i += size;
			}
			return -1;
		}

		// FlexLastIndexOf is a generalization of IndexOf that allows
		// the desired result of the predicate to be specified.
		int FlexLastIndexOf (RunePredicate matchFunc, bool expected)
		{
			int blen = Length;
			for (int i = blen; i > 0; ){
				(var rune, var size) = Utf8.DecodeLastRune (this, i);
				i -= size;
				if (matchFunc (rune) == expected)
					return i;
			}
			return -1;
		}

	}
}
