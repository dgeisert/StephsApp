using System.IO;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ES2FileUtility
{
	public static void Delete(string path)
	{
		File.Delete(path);
	}

	public static bool Exists(string path)
	{
		return File.Exists(path);
	}

	public static void Move(string from, string to)
	{
		File.Move(from, to);
	}

	public static byte[] ReadAllBytes(string path)
	{
		return File.ReadAllBytes(path);
	}

	public static Stream CreateFileStream(string path, ES2Settings.ES2FileMode filemode, int bufferSize)
	{
		if(filemode == ES2Settings.ES2FileMode.Create)
			return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);
		else if(filemode == ES2Settings.ES2FileMode.Append)
			return new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize);
		else // ES2FileMode.Open
			return new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, bufferSize);
	}
}

