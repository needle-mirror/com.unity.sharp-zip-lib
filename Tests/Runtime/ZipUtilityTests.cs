#if UNITY_2019_1_OR_NEWER

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Unity.SharpZipLib.Utils.Tests {

public class ZipUtilityTests {

    [Test]
    public void CompressTemporaryCacheToPersistentData() {

        string tempCacheFolder = WriteDummyDataInTemporaryCache("SwordTextures", CreateDummyData());
        string outputZipPath = CompressTemporaryCacheToPersistentData("SwordTextures.zip");
        Assert.True(File.Exists(outputZipPath));

        //Clean up
        Directory.Delete(tempCacheFolder,true);
        File.Delete(outputZipPath);
    }

    #region APIDOC-CompressFolderToZip
    /// <summary>
    /// Compresses all files and directories within the temporary cache path into a zip file, and saves it in the persistent data path.
    /// </summary>
    /// <param name="outputZipFileName">The name of the output zip file, with its extension</param>
    /// <returns>The full file path to the created zip archive in the persistent data directory.</returns>
    private string CompressTemporaryCacheToPersistentData(string outputZipFileName) {
        string outputZipPath = Path.Combine(Application.persistentDataPath, outputZipFileName);
        ZipUtility.CompressFolderToZip(outputZipPath,null, Application.temporaryCachePath);
        return outputZipPath;
    }
    #endregion

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Test]
    public void CompressAndUncompressFolder() {
        string tempCacheFolderName = "SwordTextures";
        Dictionary<string, string> dummyData = CreateDummyData();
        string tempCachePath = WriteDummyDataInTemporaryCache(tempCacheFolderName, dummyData);

        string persistentDataZipFileName = "SwordTextures.zip";
        string outputZipPath = CompressTemporaryCacheToPersistentData(persistentDataZipFileName);
        Assert.True(File.Exists(outputZipPath));

        //Delete cache
        Directory.Delete(tempCachePath, recursive:true);

        const string EXTRACTED_PREFIX = "Extracted";
        string extractedFilesRoot = ExtractPersistentDataZipToTemporaryCache(persistentDataZipFileName,EXTRACTED_PREFIX);
        string extractedFilesPath = Path.Combine(extractedFilesRoot, tempCacheFolderName); //"Extracted/SwordTextures"

        //Check extracted contents
        using (var enumerator = dummyData.GetEnumerator()) {
            while (enumerator.MoveNext()) {
                KeyValuePair<string, string> kv = enumerator.Current;
                string filePath = Path.Combine(extractedFilesPath, kv.Key);
                Assert.True(File.Exists(filePath));
                Assert.AreEqual(kv.Value, File.ReadAllText(filePath));
            }
        }

        //Cleanup
        Directory.Delete(extractedFilesRoot,true);
        File.Delete(outputZipPath);
    }

    #region APIDOC-UncompressFromZip
    /// <summary>
    /// Extracts the contents of a specified zip file from the persistent data path to a designated folder within the temporary cache directory.
    /// </summary>
    /// <param name="persistentDataZipFileName">The name of the zip file, with extension, located in the persistent data directory</param>
    /// <param name="tempCacheFolderName">The name of the folder within the temporary cache where the zip file's contents will be extracted</param>
    /// <returns>The full file path to the root folder containing the extracted files in the temporary cache directory.</returns>
    private string ExtractPersistentDataZipToTemporaryCache(string persistentDataZipFileName, string tempCacheFolderName) {

        string zipPath = Path.Combine(Application.persistentDataPath, persistentDataZipFileName);
        string extractedFilesRoot = Path.Combine(Application.temporaryCachePath, tempCacheFolderName);
        ZipUtility.UncompressFromZip(zipPath, null, extractedFilesRoot);
        return extractedFilesRoot;
    }
    #endregion

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    private Dictionary<string, string> CreateDummyData() {
        Dictionary<string, string> fileContentDictionary = new Dictionary<string, string>() {
            {"Foo.txt", "FooFoo"},
            {"Bar.txt", "BarBar"}
        };
        return fileContentDictionary;
    }

    private string WriteDummyDataInTemporaryCache(string tempCacheFolderName, Dictionary<string, string> fileContentDictionary) {
        string folder = Path.Combine(Application.temporaryCachePath, tempCacheFolderName);
        Directory.CreateDirectory(folder);
        using Dictionary<string, string>.Enumerator enumerator = fileContentDictionary.GetEnumerator();
        while (enumerator.MoveNext()) {
            KeyValuePair<string, string> kv = enumerator.Current;
            string filePath = Path.Combine(folder, kv.Key);
            File.WriteAllText(filePath, kv.Value);
            Assert.True(File.Exists(filePath));
        }

        return folder;
    }

}

} //end namespace

#endif
