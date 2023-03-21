using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RemoteVideoTransfer.Data
{
    /// <summary>
    /// 包头结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential,Pack = 1)]
    public struct HeaderStruct
    {
        /// <summary>
        /// 包类型
        /// </summary>
        public byte PackageType;
        /// <summary>
        /// 额外信息
        /// </summary>
        public ulong ExtraData;
        /// <summary>
        /// 包长度
        /// </summary>
        public uint BodyLength;
    }
}
