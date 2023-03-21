using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteVideoTransfer.Server.Socket
{
    public class SocketServer : AppServer<SocketSession, BinaryRequestInfo>
    {
        /// <summary>
        /// 命令到达事件
        /// </summary>
        public event OnCommandArrivedEventHandler OnCommandArrived;

        /// <summary>
        /// 不应直接实例化此类型，请使用TransferStarter
        /// </summary>
        public SocketServer()
            : base(new DefaultReceiveFilterFactory<MyReceiveFilter, BinaryRequestInfo>())
        {

        }

        /// <summary>
        /// 向当前所有已连接到此服务端的客户端发送命令
        /// </summary>
        /// <param name="command"></param>
        //public void Broadcast(ICommand command)
        //{
        //    try
        //    {
        //        foreach (var socketSession in GetAllSessions())
        //        {
        //            socketSession.SendCommand(command);
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        /// <summary>
        /// 触发命令到达事件
        /// </summary>
        /// <param name="clientSession"></param>
        /// <param name="command"></param>
        /// <param name="type">0picture,1audio</param>
        public void RaiseOnCommandArrivedEvent(SocketSession clientSession, byte[] data, int type)
        {
            OnCommandArrived?.Invoke(clientSession, data, type);
        }

        /// <summary>
        /// 新的客户端连接
        /// </summary>
        /// <param name="session"></param>
        //protected override void OnNewSessionConnected(SocketSession session)
        //{
        //    //设置回话的字符集
        //    session.Charset = Encoding.UTF8;
        //    base.OnNewSessionConnected(session);

        //    //发送欢迎信息
        //    var welcomeCommand = new Command(CommandType.ServerWelcome);

        //    //构建欢迎数据
        //    var sbWelcomeInfo = new StringBuilder();
        //    sbWelcomeInfo.Append("welcome to ");
        //    sbWelcomeInfo.Append(session.AppServer.Name);
        //    welcomeCommand.Data = sbWelcomeInfo.ToString();

        //    //发送欢迎命令
        //    session.SendCommand(welcomeCommand);
        //}

    }
}
