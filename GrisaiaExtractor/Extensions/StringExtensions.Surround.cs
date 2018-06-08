using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Extensions {
	public static partial class StringExtensions {

		/// <summary>Returns true if the string starts and ends with the
		/// <paramref name="openClose"/> parts.</summary>
		/// <param name="openClose">The left and right parts of the surrounding.</param>
		/// <param name="noOverlap">The two <paramref name="openClose"/> parts must not
		/// overlap each other.</param>
		/// <returns>Returns true if the string starts and ends with the
		/// <paramref name="openClose"/> parts.</returns>
		public static bool IsSurrounded(this string str, char openClose,
			bool noOverlap = true)
		{
			return str.IsSurrounded(openClose, openClose, noOverlap);
		}

		/// <summary>Returns true if the string starts with <paramref name="open"/> and
		/// ends with <paramref name="close"/>.</summary>
		/// <param name="open">The left part of the surrounding.</param>
		/// <param name="close">The right part of the surrounding.</param>
		/// <param name="noOverlap">The <paramref name="open"/> and <paramref name="close"/>
		/// parts must not overlap each other.</param>
		/// <returns>Returns true if the string starts with <paramref name="open"/> and
		/// ends with <paramref name="close"/>.</returns>
		public static bool IsSurrounded(this string str, char open, char close,
			bool noOverlap = true)
		{
			return ((!noOverlap && str.Length >= 1) || str.Length >= 2) &&
				str[0] == open && str[str.Length - 1] == close;
		}

		/// <summary>Returns true if the string starts and ends with the
		/// <paramref name="openClose"/> parts.</summary>
		/// <param name="openClose">The left and right parts of the surrounding.</param>
		/// <param name="noOverlap">The two <paramref name="openClose"/> parts must not
		/// overlap each other.</param>
		/// <returns>Returns true if the string starts and ends with the
		/// <paramref name="openClose"/> parts.</returns>
		public static bool IsSurrounded(this string str, string openClose,
			bool noOverlap = true)
		{
			return str.IsSurrounded(openClose, openClose, noOverlap);
		}

		/// <summary>Returns true if the string starts with <paramref name="open"/> and
		/// ends with <paramref name="close"/>.</summary>
		/// <param name="open">The left part of the surrounding.</param>
		/// <param name="close">The right part of the surrounding.</param>
		/// <param name="noOverlap">The <paramref name="open"/> and
		/// <paramref name="close"/> parts must not overlap each other.</param>
		/// <returns>Returns true if the string starts with <paramref name="open"/> and
		/// ends with <paramref name="close"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="open"/> or
		/// <paramref name="close"/> is null.</exception>
		public static bool IsSurrounded(this string str, string open, string close,
			bool noOverlap = true)
		{
			return (!noOverlap || str.Length >= open.Length + close.Length) &&
				str.StartsWith(open) && str.EndsWith(close);
		}
	}
}
