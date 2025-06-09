
# Latest official docs
- [English](https://docs.unity3d.com/Packages/com.unity.sharp-zip-lib@latest)
# com.unity.sharp-zip-lib

`com.unity.sharp-zip-lib` is a package that wraps [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) to be used inside Unity,
and provides various compression/uncompression utility functions.

Currently, this package uses [SharpZipLib v1.4.2](https://github.com/icsharpcode/SharpZipLib/releases/tag/v1.4.2).
Please refer to the [installation](Documentation~/Installation.md) page to install this package.

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

* Unity `2022.3.62` or higher.
## Steps to update SharpZipLib

### Windows

#### Environment Prerequisites
1. Visual Studio 2022 (17.14.0) or later
   * Check ".Net desktop development" during the installation of Visual Studio
2. Make sure that .Net SDK 9.0 is installed as follows:
   * Open "Developer Command Prompt for VS 2022"
   * Type: `dotnet --list-sdks`

#### Steps

1. Download the source from https://github.com/icsharpcode/SharpZipLib/releases
1. Extract the source into a folder, e.g:  `Src\SharpZipLib-1.4.2`
1. Open "Developer Command Prompt for VS 2022"
   1. Execute `update_sharp-zip-lib.cmd [src_folder]`
      * E.g: `update_sharp-zip-lib.cmd Src\SharpZipLib-1.4.2`
      * Executing `update_sharp-zip-lib.cmd` without arguments will print the list of acceptable arguments
1. Open SharpZipLib~ test project, ensure everything compiles and the tests are successful.
   * We may need to fix/remove some tests that were copied from the source.
     For example: [async tests](https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/reference-async-tests.html)
     which are not supported in Unity 2018 and earlier.
1. Open a shell / Git Bash / terminal
   1. Execute `sh Scripts~/clean_up_text.sh` to clean up cs source code.



*Auto-generated on Thu Jun  5 23:35:42 UTC 2025*
