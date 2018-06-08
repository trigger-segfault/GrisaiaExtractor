using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor {
	/// <summary>An exception thrown when trying to automatically locate the Grisaia
	/// executable.</summary>
	public class GrisaiaExeNotFoundException : Exception {
		public GrisaiaExeNotFoundException()
			: base("Could not find Grisaia executable file!") { }
	}

	/// <summary>An exception thrown when an enum is missing it's code attribute.</summary>
	public class CodeNotFoundException : Exception {
		public CodeNotFoundException(FieldInfo field)
			: base($"No code attribute found for {field.DeclaringType.Name}.{field.Name}!") { }
	}

	/// <summary>The result of an .hg3 extract operation.</summary>
	public enum ExtractHg3Result {
		Success,
		Hg3ConvertFailed,
		BmpConvertFailed,
		PngSaveFailed,
		BmpDeleteFailed,
		Unknown,
	}

	/// <summary>An exception thrown during an hg3 extraction failure.</summary>
	public class ExtractHg3Exception : Exception {

		/// <summary>The result of the .hg3 extract operation.</summary>
		public ExtractHg3Result State { get; }
		/// <summary>The file associated with the error.</summary>
		public string File { get; }

		/// <summary>Constructs the exception with a result and associated file.</summary>
		public ExtractHg3Exception(ExtractHg3Result result, string file)
			: base(WriteMessage(result, file)) {
			State = result;
			File = file;
		}

		/// <summary>Constructs the exception with a result, associated file, and inner
		/// exception.</summary>
		public ExtractHg3Exception(ExtractHg3Result result, string file,
			Exception innerException)
			: base(WriteMessage(result, file, innerException), innerException) {
			State = result;
			File = file;
		}

		/// <summary>Writes the exception message.</summary>
		private static string WriteMessage(ExtractHg3Result result, string file,
			Exception innerException = null) {
			string name = Path.GetFileName(file);
			string error = innerException?.GetType().Name ?? "Unknown Error";
			switch (result) {
			case ExtractHg3Result.Hg3ConvertFailed:
				return $"hgx2bmp.exe returned with an error while trying to convert '{name}'!";
			case ExtractHg3Result.BmpConvertFailed:
				return $"{error} occurred while trying to convert '{name}' to a png!";
			case ExtractHg3Result.PngSaveFailed:
				return $"{error} occurred while trying to save '{name}'!";
			case ExtractHg3Result.BmpDeleteFailed:
				return $"{error} occurred while trying to delete leftover `{name}`!";
			case ExtractHg3Result.Unknown:
				return $"{error} occurred during an unknown point in the operation with '{name}'!";
			default:
				return "No error occurred.";
			}
		}
	}

	/// <summary>An exception thrown during a failure with a resource.</summary>
	public class ResourceException : Exception {
		/// <summary>The name of the resource. May be null.</summary>
		public string Name { get; }
		/// <summary>The type of the resource. May be null.</summary>
		public string Type { get; }

		public ResourceException(string name, string type, string action)
			: base($"Failed to {action} resource '{name}:{type}'!")
		{
			Name = name;
			Type = type;
		}
	}

	/// <summary>An exception thrown during a failure to load a library.</summary>
	public class LoadLibraryException : Exception {
		/// <summary>The name of the library file.</summary>
		public string Library { get; }

		public LoadLibraryException(string library)
			: base($"Failed to load '{Path.GetFileName(library)}'!")
		{
			Library = Path.GetFileName(library);
		}
	}

	/// <summary>An exception thrown when the file is not of the valid type.</summary>
	public class InvalidFileException : Exception {
		/// <summary>The name of the invalid file.</summary>
		public string FileName { get; }

		public InvalidFileException(string file, string validType)
			: base($"'{Path.GetFileName(file)}' is not a valid {validType} file!")
		{
		}
	}
}
