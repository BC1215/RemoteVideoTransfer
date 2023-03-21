using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace RemoteVideoTransfer.Server.Socket.Commands
{
    /// <summary>
    /// 命令输出
    /// </summary>
    public class PictureDataCommand : CommandBase<SocketSession, BinaryRequestInfo>
    {
        /// <summary>
        /// 命令输出命令执行
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        public override void ExecuteCommand(SocketSession session, BinaryRequestInfo requestInfo)
        {
            //将数据转换为命令实体
            var data = requestInfo.Body;
            //触发命令到达事件
            ((SocketServer)session.AppServer).RaiseOnCommandArrivedEvent(session, data, 0);
        }
    }

}
