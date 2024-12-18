using Unity.SharpZipLib.Core;
using Unity.SharpZipLib.Zip;
using System;
using System.IO;

namespace Unity.SharpZipLib.Utils {

/// <summary>
/// Provides utility methods for compressing and decompressing zip files.
/// </summary>
/// <remarks>
/// This class provides facilities to compress entire folders into a zip file and to uncompress a zip file into folders.
/// </remarks>
/// <example>
/// <para> The following example adds a menu item named "CompressAndUncompress" to Debug in the menu bar that compresses the Assets folder to UnityAssets.zip
/// and extracts the contents back.
/// </para>
/// <code>
/// using System.IO;
/// using Unity.SharpZipLib.Utils;
/// using UnityEditor;
/// using UnityEngine;
///
/// public static class DebugMenu {
///     [MenuItem("Debug/CompressAndUncompress")]
///     static void CompressAndUncompress() {
///
///         //Compress
///         string folderToCompress = "Assets";
///         string zipPath = Path.Combine(Application.temporaryCachePath, "UnityAssets.zip");
///         ZipUtility.CompressFolderToZip(zipPath,null, folderToCompress);
///         Debug.Log($"{folderToCompress} folder compressed to: " + zipPath);
///
///         //Uncompress
///         string extractPath = Path.Combine(Application.temporaryCachePath, "UnityAssetsExtracted");
///         ZipUtility.UncompressFromZip(zipPath, null, extractPath);
///         Debug.Log($"Uncompressed to: " + extractPath);
///     }
/// }
/// </code>
/// </example>
public static class ZipUtility {

    /// <summary>
    /// Uncompress the contents of a zip file into the specified folder.
    /// </summary>
    /// <param name="archivePath">The path to the zip file to be extracted</param>
    /// <param name="password">The password required to open the zip file. Set to <c>null</c> if the zip file is not encrypted</param>
    /// <param name="outFolder">The output folder where the contents will be extracted</param>
    /// <remarks>
    /// If the output folder already exists, its contents will be deleted before the extraction.
    /// </remarks>
    /// <example>
    /// <code source="../../Tests/Runtime/ZipUtilityTests.cs" region="APIDOC-UncompressFromZip" title="Uncompress From Zip"/>
    /// </example>
    public static void UncompressFromZip(string archivePath, string password, string outFolder) {
        if (Directory.Exists(outFolder))  {
            Directory.Delete(outFolder,true);
        }

        Directory.CreateDirectory(outFolder);

        using(Stream fs = File.OpenRead(archivePath))
        using(ZipFile zf = new ZipFile(fs)){

            if (!String.IsNullOrEmpty(password)) {
                // AES encrypted entries are handled automatically
                zf.Password = password;
            }

            foreach (ZipEntry zipEntry in zf) {
                if (!zipEntry.IsFile) {
                    // Ignore directories
                    continue;
                }
                String entryFileName = zipEntry.Name;
                // to remove the folder from the entry:
                //entryFileName = Path.GetFileName(entryFileName);
                // Optionally match entrynames against a selection list here
                // to skip as desired.
                // The unpacked length is available in the zipEntry.Size property.

                // Manipulate the output filename here as desired.
                var fullZipToPath = Path.Combine(outFolder, entryFileName);
                var directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0) {
                    Directory.CreateDirectory(directoryName);
                }

                // 4K is optimum
                var buffer = new byte[4096];

                // Unzip file in buffered chunks. This is just as fast as unpacking
                // to a buffer the full size of the file, but does not waste memory.
                // The "using" will close the stream even if an exception occurs.
                using(Stream zipStream = zf.GetInputStream(zipEntry))
                using (Stream fsOutput = File.Create(fullZipToPath)) {
                    StreamUtils.Copy(zipStream, fsOutput, buffer);
                }
            }
        }
    }

//---------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates a zip file on disk containing the contents of the nominated folder.
    /// </summary>
    /// <param name="outPathname">The path where the created zip file will be saved</param>
    /// <param name="password">The password required to open the zip file. Set to <c>null</c> if not required</param>
    /// <param name="folderName">The folder to be compressed</param>
    /// <remarks>
    /// This method recursively compresses all files within the specified folder.
    /// You can specify a password to encrypt the target zip file.
    /// </remarks>
    /// <example>
    /// <code source="../../Tests/Runtime/ZipUtilityTests.cs" region="APIDOC-CompressFolderToZip" title="Compress Folder To Zip"/>
    /// </example>
    public static void CompressFolderToZip(string outPathname, string password, string folderName) {

        using(FileStream fsOut = File.Create(outPathname))
        using(var zipStream = new ZipOutputStream(fsOut)) {

            //0-9, 9 being the highest level of compression
            zipStream.SetLevel(3);

            // optional. Null is the same as not setting. Required if using AES.
            zipStream.Password = password;

            // This setting will strip the leading part of the folder path in the entries,
            // to make the entries relative to the starting folder.
            // To include the full path for each entry up to the drive root, assign to 0.
            int folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);

            CompressFolderToZipInternal(folderName, zipStream, folderOffset);

        }

    }

//---------------------------------------------------------------------------------------------------------------------

    // Recursively compresses a folder structure
    private static void CompressFolderToZipInternal(string path, ZipOutputStream zipStream, int folderOffset) {

        var files = Directory.GetFiles(path);

        foreach (var filename in files) {

            var fi = new FileInfo(filename);

            // Make the name in zip based on the folder
            var entryName = filename.Substring(folderOffset);

            // Remove drive from name and fixe slash direction
            entryName = ZipEntry.CleanName(entryName);

            var newEntry = new ZipEntry(entryName);

            // Note the zip format stores 2 second granularity
            newEntry.DateTime = fi.LastWriteTime;

            // Specifying the AESKeySize triggers AES encryption.
            // Allowable values are 0 (off), 128 or 256.
            // A password on the ZipOutputStream is required if using AES.
            //   newEntry.AESKeySize = 256;

            // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003,
            // WinZip 8, Java, and other older code, you need to do one of the following:
            // Specify UseZip64.Off, or set the Size.
            // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility,
            // you do not need either, but the zip will be in Zip64 format which
            // not all utilities can understand.
            //   zipStream.UseZip64 = UseZip64.Off;
            newEntry.Size = fi.Length;

            zipStream.PutNextEntry(newEntry);

            // Zip the file in buffered chunks
            // the "using" will close the stream even if an exception occurs
            var buffer = new byte[4096];
            using (FileStream fsInput = File.OpenRead(filename)) {
                StreamUtils.Copy(fsInput, zipStream, buffer);
            }
            zipStream.CloseEntry();
        }

        // Recursively call CompressFolder on all folders in path
        var folders = Directory.GetDirectories(path);
        foreach (var folder in folders) {
            CompressFolderToZipInternal(folder, zipStream, folderOffset);
        }
    }

}

} //end namespace

