# com.unity.sharp-zip-lib

`com.unity.sharp-zip-lib` is a package that wraps [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) to be used inside Unity,
and provides various compression/uncompression utility functions.

Currently, this package uses [SharpZipLib v1.3.3](https://github.com/icsharpcode/SharpZipLib/releases/tag/v1.3.3).
Please refer to the [installation](Installation.md) page to install this package.

> The version numbering of this package itself and the version of SharpZipLib used in the package may look similar,
but they are not related.


## How to Use

* All [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) APIs are available under `Unity.SharpZipLib` namespace. For example:
  ```csharp
    using System.IO;
    using Unity.SharpZipLib.GZip;

    ...

    public void UseSharpZipLibAPIs() {
        MemoryStream ms = new MemoryStream();
        GZipOutputStream outStream = new GZipOutputStream(ms);
        ...
    }
  ```

  Please refer to the API documentation of the [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) version used
  in this package for more details.

* In addition, `com.unity.sharp-zip-lib` also provides additional utility APIs:
  * `ZipUtility.CompressFolderToZip()`: Creates a zip file on disk containing the contents of the nominated folder.
  * `ZipUtility.UncompressFromZip()`: Uncompress the contents of a zip file into the specified folder.

  As an example:
  ```csharp
  static void CompressAndUncompress() {
      //Compress
      string folderToCompress = "Assets";
      string zipPath = Path.Combine(Application.temporaryCachePath, "UnityAssets.zip");
      ZipUtility.CompressFolderToZip(zipPath,null, folderToCompress);
      Debug.Log($"{folderToCompress} folder compressed to: " + zipPath);

      //Uncompress
      string extractPath = Path.Combine(Application.temporaryCachePath, "UnityAssetsExtracted");
      ZipUtility.UncompressFromZip(zipPath, null, extractPath);
      Debug.Log($"Uncompressed to: " + extractPath);
  }
  ```


## Supported Unity Versions

* Unity `2021.3.45` or higher.
