using System.IO;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ES2DirectoryUtility
{
	public static void Delete(string path, bool recursive)
	{
		Directory.Delete(path, recursive);
	}

	public static bool Exists(string path)
	{
		return Directory.Exists(path);
	}

	public static void Move(string from, string to)
	{
		Directory.Move(from, to);
	}

	public static void CreateDirectory(string path)
	{
		Directory.CreateDirectory(path);
	}

	public static string[] GetDirectories(string path)
	{
		return Directory.GetDirectories(path);
	}

	public static string[] GetFiles(string path, string searchPattern)
	{
		return Directory.GetFiles(path, searchPattern);
	}
}

