﻿using LNF.CommonTools;
using SevenZip;
using System;
using System.Configuration;
using System.IO;

namespace sselFinOps.AppCode
{
    public static class ZipUtility
    {
        public static string CreateArchive(string workPath, string name)
        {
            string libPath = Utility.GetRequiredAppSetting("7zLibPath");
            
            if (!File.Exists(libPath))
                throw new Exception($"File not found: {libPath}");

            SevenZipBase.SetLibraryPath(libPath);
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
