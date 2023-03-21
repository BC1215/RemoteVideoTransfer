using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using RemoteVideoTransfer.Data;
using SuperSocket.Common;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase.Protocol;

namespace RemoteVideoTransfer.Server.Socket
{
    /// <summary>
    /// FixedHeaderReceiveFilter协议
    /// </summary>
    internal class MyReceiveFilter : FixedHeaderReceiveFilter<BinaryRequestInfo>
    {
        /// <summary>
        /// 1byte包类型，8byte附加数据，4byte包长度（不含头），实际数据
        /// 
        /// </summary>
        public MyReceiveFilter()
            : base(13)
        {

        }
        /// <summary>
        /// 返回数据体长度
        /// </summary>
        /// <param name="header">头数据</param>
        /// <param name="offset">消息体长度数据位置偏移</param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            Console.WriteLine("=========================");
            Console.WriteLine($"类型{ToHexString(header.CloneRange(offset, 1))}，{header.CloneRange(offset, 1)[0]}");
            Console.WriteLine($"长度{Convert.ToString(BitConverter.ToUInt32(header.CloneRange(offset + 9, 4), 0), 16)}，{header.CloneRange(offset + 9, 4).ConvertByteArrayToInt()}");
            Console.WriteLine("=========================");
            return (int)header.CloneRange(offset + 9, 4).ConvertByteArrayToUInt();
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="header"></param>
        /// <param name="bodyBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override BinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset,
            int length)
        {
            //var commandTypeHash = header.CloneRange(0, 4).ConvertByteArrayToInt();
            //var commandType = (CommandType)Enum.Parse(typeof(CommandType), commandTypeHash.ToString(CultureInfo.InvariantCulture));

            var packageType = header.CloneRange(0, 1)[0];
            switch (packageType)
            {
                case 0:
                    return new BinaryRequestInfo("PictureDataCommand", bodyBuffer.CloneRange(offset, length));
                    break;
                case 1:
                    return new BinaryRequestInfo("AudioDataCommand", bodyBuffer.CloneRange(offset, length));
                    break;
                default:
                    throw new ArgumentException();
                    break;
            }
        }

        public static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "

        {
            string hexString = string.Empty;

            if (bytes != null)

            {

                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)

                {

                    strB.Append(bytes[i].ToString("X2"));

                }

                hexString = strB.ToString();

            }
            return hexString;

        }



    }
}
