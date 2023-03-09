using System.IO;
using System.Text;

namespace CSharpMate.Core
{
    public static class FileHelper
    {
        internal static void WriteAllTextWithCreateDirectory(string filePath, string content, Encoding encoding = null)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            WriteAllText(filePath, content, encoding);
        }
        internal static void WriteAllText(string filePath, string content, Encoding encoding = null)
        {
            if (encoding is null)
                encoding = Encoding.UTF8;

            RemoveReadOnlyAttribute(filePath);
            File.WriteAllText(filePath, content, encoding);
        }
        internal static bool RemoveReadOnlyAttribute(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileAttributes = File.GetAttributes(filePath);
                if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    File.SetAttributes(filePath, (fileAttributes & ~FileAttributes.ReadOnly) | FileAttributes.Normal);
                return true;
            }
            return false;
        }
        internal static void CreateFile(string filePath, string content)
        {
            var path = Path.GetDirectoryName(filePath);
            DirectoryInfo oDirectoryInfo = new DirectoryInfo(path);
            if (!oDirectoryInfo.Exists)
                Directory.CreateDirectory(path);

            if (oDirectoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
                oDirectoryInfo.Attributes = FileAttributes.Normal;
            File.WriteAllText(filePath, content);
        }
    }
}
