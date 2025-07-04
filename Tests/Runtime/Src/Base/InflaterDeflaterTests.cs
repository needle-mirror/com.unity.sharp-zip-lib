using Unity.SharpZipLib.Tests.TestSupport;
using Unity.SharpZipLib.Zip.Compression;
using Unity.SharpZipLib.Zip.Compression.Streams;
using NUnit.Framework;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;
using Range = NUnit.Framework.RangeAttribute;

namespace Unity.SharpZipLib.Tests.Base
{
    /// <summary>
    /// This class contains test cases for Deflater/Inflater streams.
    /// </summary>
    [TestFixture]
    internal class InflaterDeflaterTestSuite
    {
        // Use the same random seed to guarantee all the code paths are followed
        const int RandomSeed = 5;

        private void Inflate(MemoryStream ms, byte[] original, int level, bool zlib)
        {
            byte[] buf2 = new byte[original.Length];
            using (var inStream = GetInflaterInputStream(ms, zlib))
            {
                int currentIndex = 0;
                int count = buf2.Length;
                while (true)
                {
                    int numRead = inStream.Read(buf2, currentIndex, count);
                    if (numRead <= 0)
                    {
                        break;
                    }
                    currentIndex += numRead;
                    count -= numRead;
                }
                Assert.That(currentIndex, Is.EqualTo(original.Length), "Decompressed data must have the same length as the original data");
            }
            VerifyInflatedData(original, buf2, level, zlib);
        }
        private MemoryStream Deflate(byte[] data, int level, bool zlib)
        {
            var memoryStream = new MemoryStream();
            var deflater = new Deflater(level, !zlib);
            using (DeflaterOutputStream outStream = new DeflaterOutputStream(memoryStream, deflater))
            {
                outStream.IsStreamOwner = false;
                outStream.Write(data, 0, data.Length);
                outStream.Flush();
                outStream.Finish();
            }
            return memoryStream;
        }
        private void RandomDeflateInflate(int size, int level, bool zlib)
        {
            var buffer =  Unity.SharpZipLib.Tests.TestSupport.Utils.GetDummyBytes(size, RandomSeed);
            var ms = Deflate(buffer, level, zlib);
            Inflate(ms, buffer, level, zlib);
        }

        private static InflaterInputStream GetInflaterInputStream(Stream compressedStream, bool zlib)
        {
            compressedStream.Seek(0, SeekOrigin.Begin);
            var inflater = new Inflater(!zlib);
            var inStream = new InflaterInputStream(compressedStream, inflater);
            return inStream;
        }

        private async Task InflateAsync(MemoryStream ms, byte[] original, int level, bool zlib)
        {
            byte[] buf2 = new byte[original.Length];
            using (var inStream = GetInflaterInputStream(ms, zlib))
            {
                int currentIndex = 0;
                int count = buf2.Length;
                while (true)
                {
                    int numRead = await inStream.ReadAsync(buf2, currentIndex, count);
                    if (numRead <= 0)
                    {
                        break;
                    }
                    currentIndex += numRead;
                    count -= numRead;
                }
                Assert.That(currentIndex, Is.EqualTo(original.Length), "Decompressed data must have the same length as the original data");
            }
            VerifyInflatedData(original, buf2, level, zlib);
        }

        private async Task<MemoryStream> DeflateAsync(byte[] data, int level, bool zlib)
        {
            var memoryStream = new MemoryStream();
            var deflater = new Deflater(level, !zlib);
            using (DeflaterOutputStream outStream = new DeflaterOutputStream(memoryStream, deflater))
            {
                outStream.IsStreamOwner = false;
                await outStream.WriteAsync(data, 0, data.Length);
                await outStream.FlushAsync();
                await outStream.FinishAsync(CancellationToken.None);
            }
            return memoryStream;
        }

        private async Task RandomDeflateInflateAsync(int size, int level, bool zlib)
        {
            var buffer = Unity.SharpZipLib.Tests.TestSupport.Utils.GetDummyBytes(size, RandomSeed);
            var ms = await DeflateAsync(buffer, level, zlib);
            await InflateAsync(ms, buffer, level, zlib);
        }

        private void VerifyInflatedData(byte[] original, byte[] buf2, int level, bool zlib)
        {
            for (int i = 0; i < original.Length; ++i)
            {
                if (buf2[i] != original[i])
                {
                    string description = string.Format("Difference at {0} level {1} zlib {2} ", i, level, zlib);
                    if (original.Length < 2048)
                    {
                        var builder = new StringBuilder(description);
                        for (int d = 0; d < original.Length; ++d)
                        {
                            builder.AppendFormat("{0} ", original[d]);
                        }
                        Assert.Fail(builder.ToString());
                    }
                    else
                    {
                        Assert.Fail(description);
                    }
                }
            }
        }
        /// <summary>
        /// Basic inflate/deflate test
        /// </summary>
        [Test]
        [Category("Base")]
        public void InflateDeflateZlib([Range(0, 9)] int level)
        {
            RandomDeflateInflate(100000, level, true);
        }
        /// <summary>
        /// Basic async inflate/deflate test
        /// </summary>
        // [Test]
        // [Category("Base")]
        // [Category("Async")]
        // public async Task InflateDeflateZlibAsync([Range(0, 9)] int level)
        // {
        //     await RandomDeflateInflateAsync(size: 100000, level, zlib: true);
        // }
        private delegate void RunCompress(byte[] buffer);
        private int _runLevel;
        private bool _runZlib;
        private void DeflateAndInflate(byte[] buffer)
        {
            var ms = Deflate(buffer, _runLevel, _runZlib);
            Inflate(ms, buffer, _runLevel, _runZlib);
        }
        private void TryVariants(RunCompress test, byte[] buffer, Random random, int index)
        {
            int worker = 0;
            while (worker <= 255)
            {
                buffer[index] = (byte)worker;
                if (index < buffer.Length - 1)
                {
                    TryVariants(test, buffer, random, index + 1);
                }
                else
                {
                    test(buffer);
                }
                worker += random.Next(maxValue: 256);
            }
        }
        private void TryManyVariants(int level, bool zlib, RunCompress test, byte[] buffer)
        {
            var random = new Random(RandomSeed);
            _runLevel = level;
            _runZlib = zlib;
            TryVariants(test, buffer, random, 0);
        }
        // TODO: Fix this
        [Test]
        [Category("Base")]
        [Explicit("Long-running")]
        public void SmallBlocks()
        {
            var buffer = new byte[10];
            TryManyVariants(level: 0, zlib: false, DeflateAndInflate, buffer);
        }
        /// <summary>
        /// Basic inflate/deflate test
        /// </summary>
        [Test]
        [Category("Base")]
        public void InflateDeflateNonZlib([Range(0, 9)] int level)
        {
            RandomDeflateInflate(100000, level, false);
        }
        /// <summary>
        /// Basic async inflate/deflate test
        /// </summary>
        // [Test]
        // [Category("Base")]
        // [Category("Async")]
        // public async Task InflateDeflateNonZlibAsync([Range(0, 9)] int level)
        // {
        //     await RandomDeflateInflateAsync(100000, level, false);
        // }
        [Test]
        [Category("Base")]
        public void CloseDeflatorWithNestedUsing()
        {
            string tempFile = null;
            try
            {
                tempFile = Application.temporaryCachePath;
            }
            catch (SecurityException)
            {
            }
            Assert.IsNotNull(tempFile, "No permission to execute this test?");
            tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
            using (FileStream diskFile = File.Create(tempFile))
            using (DeflaterOutputStream deflator = new DeflaterOutputStream(diskFile))
            using (StreamWriter txtFile = new StreamWriter(deflator))
            {
                txtFile.Write("Hello");
                txtFile.Flush();
            }
            File.Delete(tempFile);
        }
        [Test]
        [Category("Base")]
        public void DeflatorStreamOwnership()
        {
            var memStream = new TrackedMemoryStream();
            var s = new DeflaterOutputStream(memStream);
            Assert.IsFalse(memStream.IsClosed, "Shouldnt be closed initially");
            Assert.IsFalse(memStream.IsDisposed, "Shouldnt be disposed initially");
            s.Close();
            Assert.IsTrue(memStream.IsClosed, "Should be closed after parent owner close");
            Assert.IsTrue(memStream.IsDisposed, "Should be disposed after parent owner close");
            memStream = new TrackedMemoryStream();
            s = new DeflaterOutputStream(memStream);
            Assert.IsFalse(memStream.IsClosed, "Shouldnt be closed initially");
            Assert.IsFalse(memStream.IsDisposed, "Shouldnt be disposed initially");
            s.IsStreamOwner = false;
            s.Close();
            Assert.IsFalse(memStream.IsClosed, "Should not be closed after parent owner close");
            Assert.IsFalse(memStream.IsDisposed, "Should not be disposed after parent owner close");
        }
        [Test]
        [Category("Base")]
        public void InflatorStreamOwnership()
        {
            var memStream = new TrackedMemoryStream();
            var s = new InflaterInputStream(memStream);
            Assert.IsFalse(memStream.IsClosed, "Shouldnt be closed initially");
            Assert.IsFalse(memStream.IsDisposed, "Shouldnt be disposed initially");
            s.Close();
            Assert.IsTrue(memStream.IsClosed, "Should be closed after parent owner close");
            Assert.IsTrue(memStream.IsDisposed, "Should be disposed after parent owner close");
            memStream = new TrackedMemoryStream();
            s = new InflaterInputStream(memStream);
            Assert.IsFalse(memStream.IsClosed, "Shouldnt be closed initially");
            Assert.IsFalse(memStream.IsDisposed, "Shouldnt be disposed initially");
            s.IsStreamOwner = false;
            s.Close();
            Assert.IsFalse(memStream.IsClosed, "Should not be closed after parent owner close");
            Assert.IsFalse(memStream.IsDisposed, "Should not be disposed after parent owner close");
        }
        [Test]
        [Category("Base")]
        public void CloseInflatorWithNestedUsing()
        {
            string tempFile = null;
            try
            {
                tempFile = Application.temporaryCachePath;
            }
            catch (SecurityException)
            {
            }
            Assert.IsNotNull(tempFile, "No permission to execute this test?");
            tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
            using (FileStream diskFile = File.Create(tempFile))
            using (DeflaterOutputStream deflator = new DeflaterOutputStream(diskFile))
            using (StreamWriter textWriter = new StreamWriter(deflator))
            {
                textWriter.Write("Hello");
                textWriter.Flush();
            }
            using (FileStream diskFile = File.OpenRead(tempFile))
            using (InflaterInputStream deflator = new InflaterInputStream(diskFile))
            using (StreamReader textReader = new StreamReader(deflator))
            {
                char[] buffer = new char[5];
                int readCount = textReader.Read(buffer, 0, 5);
                Assert.AreEqual(5, readCount);
                var b = new StringBuilder();
                b.Append(buffer);
                Assert.AreEqual("Hello", b.ToString());
            }
            File.Delete(tempFile);
        }
    }
}
