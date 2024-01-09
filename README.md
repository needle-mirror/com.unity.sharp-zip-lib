
# Latest official docs
- [English](https://docs.unity3d.com/Packages/com.unity.sharp-zip-lib@latest)
 
[![](https://badge-proxy.cds.internal.unity3d.com/78081939-b2a0-4fc9-bebf-901b49fd954c)](https://badges.cds.internal.unity3d.com/packages/com.unity.sharp-zip-lib/build-info?branch=master&testWorkflow=package-isolation)
[![](https://badge-proxy.cds.internal.unity3d.com/faf61743-4d21-479c-b01c-ab63561e27d9)](https://badges.cds.internal.unity3d.com/packages/com.unity.sharp-zip-lib/dependencies-info?branch=master&testWorkflow=updated-dependencies)
[![](https://badge-proxy.cds.internal.unity3d.com/e5917bce-0357-4f49-a9c3-c356b9de832c)](https://badges.cds.internal.unity3d.com/packages/com.unity.sharp-zip-lib/dependants-info)
[![](https://badge-proxy.cds.internal.unity3d.com/f2066c51-3423-424d-a58f-24a64683cf57)](https://badges.cds.internal.unity3d.com/packages/com.unity.sharp-zip-lib/warnings-info?branch=master)

![ReleaseBadge](https://badge-proxy.cds.internal.unity3d.com/3b04c8fe-9005-4b46-848e-cb0199e49a2e)
![ReleaseBadge](https://badge-proxy.cds.internal.unity3d.com/9a481f99-fa9b-4716-8409-69bb63fedbd7)
# com.unity.sharp-zip-lib

`com.unity.sharp-zip-lib` is a package that wraps [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) to be used inside Unity,
and provides various compression/uncompression utility functions.

Currently, this package uses [SharpZipLib v1.3.1](https://github.com/icsharpcode/SharpZipLib/releases/tag/v1.3.1).  
Please refer to the [installation](Documentation~/Installation.md) page to install this package.
 
> The version numbering of this package itself and the version of SharpZipLib used in the package may look similar, 
but they are not related.


## How to Use

* All [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) APIs are available under `Unity.SharpZipLib` namespace. For example: 
  ```csharp
    using System.IO;
    using Unity.SharpZipLib.GZip;
  
    ...
  
    public void Foo() {
        MemoryStream ms = new MemoryStream();
        GZipOutputStream outStream = new GZipOutputStream(ms);
        ...
    }

  ```

  Please refer to the API documentation of the [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) version used 
  in this package for more details.

* In addition, `com.unity.sharp-zip-lib` also provides additional utility APIs:
  * `ZipUtility.CompressFolderToZip()`: Compresses the files in the nominated folder, and creates a zip file on disk. 
  * `ZipUtility.UncompressFromZip()`: Uncompress the contents of a zip file into the specified folder.
 
  As an example:
  ```csharp
  
  [Test]
  public void Foo() {
      //Compress 
      string tempZipPath = FileUtil.GetUniqueTempPathInProject();
      string folderToCompress ="Bar";
      ZipUtility.CompressFolderToZip(tempZipPath,null, folderToCompress);
  
      //Uncompress
      string tempExtractPath = FileUtil.GetUniqueTempPathInProject();
      Directory.CreateDirectory(tempExtractPath);
      ZipUtility.UncompressFromZip(tempZipPath, null, tempExtractPath);
  
  }
  ```


## Supported Unity Versions

* Unity `2018.4.36` or higher.
## Steps to update SharpZipLib

### Windows

#### Requirements
1. Visual Studio 2019 (16.11.13) or later
   * Check ".Net desktop development"

#### Steps

1. Download the source from https://github.com/icsharpcode/SharpZipLib/releases
1. Extract the source into "SharpZipLibSrc" folder
1. Open Developer Command Prompt for Visual Studio
1. Execute `update_sharp-zip-lib.cmd`
1. Open SharpZipLib~ test project, ensure everything compiles and the tests are successful



*Auto-generated on Tue Jan  9 09:37:14 UTC 2024*
