using SuperSocket.SocketBase;

namespace RemoteVideoTransfer.Server.Socket
{
    public delegate void OnCommandArrivedEventHandler(SocketSession clientSession, byte[] data,int type);
}
