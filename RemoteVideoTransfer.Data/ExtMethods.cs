using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;

namespace RemoteVideoTransfer.Data
{
    public static class ExtMethods
    {
        #region Byte[]压缩
        public const ushort COMPRESSION_FORMAT_LZNT1 = 2;
        public const ushort COMPRESSION_ENGINE_MAXIMUM = 0x100;

        [DllImport("ntdll.dll")]
        public static extern uint RtlGetCompressionWorkSpaceSize(ushort dCompressionFormat, out uint dNeededBufferSize, out uint dUnknown);

        [DllImport("ntdll.dll")]
        public static extern uint RtlCompressBuffer(ushort dCompressionFormat, byte[] dSourceBuffer, int dSourceBufferLength, byte[] dDestinationBuffer,
        int dDestinationBufferLength, uint dUnknown, out int dDestinationSize, IntPtr dWorkspaceBuffer);

        [DllImport("ntdll.dll")]
        public static extern uint RtlDecompressBuffer(ushort dCompressionFormat, byte[] dDestinationBuffer, int dDestinationBufferLength, byte[] dSourceBuffer, int dSourceBufferLength, out uint dDestinationSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LocalAlloc(int uFlags, IntPtr sizetdwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree(IntPtr hMem);


        public static byte[] DecompressBytesAPI(this byte[] bytes)
        {
            var outBuf = new byte[bytes.Length * 6];
            uint dwSize = 0, dwRet = 0;
            uint ret = RtlGetCompressionWorkSpaceSize(COMPRESSION_FORMAT_LZNT1, out dwSize, out dwRet);
            if (ret != 0) return null;

            ret = RtlDecompressBuffer(COMPRESSION_FORMAT_LZNT1, outBuf, outBuf.Length, bytes, bytes.Length, out dwRet);
            if (ret != 0) return null;

            Array.Resize(ref outBuf, (Int32)dwRet);
            return outBuf;
        }


        public static byte[] CompressBytesAPI(this byte[] bytes)
        {
            var outBuf = new byte[bytes.Length * 6];
            uint dwSize = 0, dwRet = 0;
            uint ret = RtlGetCompressionWorkSpaceSize(COMPRESSION_FORMAT_LZNT1 | COMPRESSION_ENGINE_MAXIMUM, out dwSize, out dwRet);
            if (ret != 0) return null;

            int dstSize = 0;
            IntPtr hWork = LocalAlloc(0, new IntPtr(dwSize));
            ret = RtlCompressBuffer(COMPRESSION_FORMAT_LZNT1 | COMPRESSION_ENGINE_MAXIMUM, bytes, bytes.Length, outBuf, outBuf.Length, 0, out dstSize, hWork);
            if (ret != 0) return null;

            LocalFree(hWork);

            Array.Resize(ref outBuf, dstSize);
            return outBuf;
        }

        public static byte[] CompressBytesSZL(this byte[] bytes)
        {
            MemoryStream ms = new MemoryStream();
            GZipOutputStream compressedzipStream = new GZipOutputStream(ms);
            compressedzipStream.SetLevel(1);
            compressedzipStream.Write(bytes, 0, bytes.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }

        public static byte[] DecompressBytesSZL(this byte[] bytes)
        {
            GZipInputStream gzi = new GZipInputStream(new MemoryStream(bytes));
            var bufferSize = 50000;
            MemoryStream re = new MemoryStream(bufferSize);
            int count;
            byte[] data = new byte[bufferSize];
            while ((count = gzi.Read(data, 0, data.Length)) != 0)
            {
                re.Write(data, 0, count);
            }
            byte[] overarr = re.ToArray();
            return overarr;
        }
        #endregion

        public static Byte[] StructToBytes(this object structure)
        {

            var size = Marshal.SizeOf(structure);
            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, buffer, false);
                var bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static T BytesToStruct<T>(this byte[] bytes)
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return (T)Marshal.PtrToStructure(buffer, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static byte[] CompressBytes(byte[] bytes, int compressLevel = 1)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                var zipStream = new GZipOutputStream(outStream);
                zipStream.SetLevel(compressLevel);
                zipStream.Write(bytes, 0, bytes.Length);
                zipStream.Close();
                zipStream.Dispose();
                byte[] rtnBytes = outStream.ToArray();
                outStream.Close();
                return rtnBytes;
            }
        }

        public static byte[] DecompressBytes(byte[] bytes)
        {
            byte[] writeData = new byte[2048];
            using (MemoryStream inStream = new MemoryStream(bytes))
            {
                Stream zipStream = new GZipInputStream(inStream);
                MemoryStream outStream = new MemoryStream();
                while (true)
                {
                    int size = zipStream.Read(writeData, 0, writeData.Length);
                    if (size > 0)
                    {
                        outStream.Write(writeData, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }
                zipStream.Close();
                zipStream.Dispose();
                var rtn = outStream.ToArray();
                outStream.Close();
                outStream.Dispose();
                inStream.Close();
                return rtn;
            }
        }

        /// <summary>
        /// 高速序列化（可能不稳定）
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] SerializeStruct(object obj)
        {
            var buffer = new byte[Marshal.SizeOf(obj.GetType())];
            var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pBuffer = gcHandle.AddrOfPinnedObject();
            Marshal.StructureToPtr(obj, pBuffer, false);
            gcHandle.Free();
            return buffer;
        }

        /// <summary>
        /// 高速反序列化（可能不稳定）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T DeserializeStruct<T>(byte[] data)
        {
            var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var obj = Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            gcHandle.Free();
            return (T)obj;
        }

        /// <summary>
        /// 把对象序列化并返回相应的字节
        /// </summary>
        /// <param name="pObj">需要序列化的对象</param>
        /// <returns>byte[]</returns>
        public static byte[] SerializeObject(object pObj)
        {
            if (pObj == null)
                return null;
            var memory = new System.IO.MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(memory, pObj);
            memory.Position = 0;
            var read = new byte[memory.Length];
            memory.Read(read, 0, read.Length);
            memory.Close();
            memory.Dispose();
            return read;
        }


        /// <summary>
        /// 把字节反序列化成相应的对象
        /// </summary>
        /// <param name="pBytes">字节流</param>
        /// <returns>object</returns>
        public static object DeserializeObject(byte[] pBytes)
        {
            if (pBytes == null)
            {
                return null;
            }
            var memory = new MemoryStream(pBytes) { Position = 0 };
            var formatter = new BinaryFormatter();
            var newOjb = formatter.Deserialize(memory);
            memory.Close();
            memory.Dispose();
            return newOjb;
        }

        /// <summary>
        /// 将32位整数转换为字节形式
        /// </summary>
        /// <param name="int32"></param>
        /// <returns></returns>
        public static byte[] ConvertIntToByteArray(this int int32)
        {
            return BitConverter.GetBytes(int32);
        }

        /// <summary>
        /// 将字节形式转换为32位整数
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int ConvertByteArrayToInt(this byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// 将字节形式转换为32位无符号整数
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static uint ConvertByteArrayToUInt(this byte[] bytes)
        {
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static byte[] CombineHeaderAndBody(HeaderStruct header, byte[] data)
        {
            var headerBytes = header.StructToBytes();
            var all = new byte[headerBytes.Length + data.Length];
            Buffer.BlockCopy(headerBytes, 0, all, 0, headerBytes.Length);
            Buffer.BlockCopy(data, 0, all, headerBytes.Length * sizeof(byte), data.Length);
            return all;
        }

        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Object2Json(this object obj)
        {
            var rtn = JsonConvert.SerializeObject(obj);
            return rtn;
        }

        /// <summary>
        /// JSON转换为指定类型的实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static T Json2Object<T>(this string jsonStr)
        {
            var rtn = JsonConvert.DeserializeObject<T>(jsonStr);
            return rtn;
        }

        /// <summary>
        /// JSON转换为弱类型的实体
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static dynamic Json2Object(this string jsonStr)
        {
            var rtn = JsonConvert.DeserializeObject(jsonStr) as dynamic;
            return rtn;
        }

        /// <summary>
        /// 创建对象的完全拷贝副本
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CloneObject<T>(this T obj)
        {
            var binaryData = SerializeObject(obj);
            var newObj = (T)DeserializeObject(binaryData);
            return newObj;
        }


    }
}
