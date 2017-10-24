using SevenZip;
using System.Configuration;
using System.IO;

namespace sselFinOps.AppCode
{
    public static class ZipUtility
    {
        public static string CreateArchive(string workPath, string name)
        {
            SevenZipBase.SetLibraryPath(ConfigurationManager.AppSettings["7zLibPath"]);
            var zip = new SevenZipCompressor();
            zip.ArchiveFormat = OutArchiveFormat.Zip;

            string archiveName = name + ".zip";
            string archivePath = Path.Combine(workPath, archiveName);
            string directory = Path.Combine(workPath, "Zip");
            zip.CompressDirectory(directory, archivePath);

            return archivePath;
        }

    }
}
