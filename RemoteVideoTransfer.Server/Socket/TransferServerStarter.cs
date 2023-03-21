using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Config;
using System.Net;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using Newtonsoft.Json;

namespace RemoteVideoTransfer.Server.Socket
{
    /// <summary>
    /// 数据传输服务器启动器
    /// </summary>
    public class TransferServerStarter
    {
        public List<SocketServer> AllSocketServer { get; set; }
        IBootstrap _bootstrap;

        /// <summary>
        /// （通过配置文件）启动所有Socket服务器，如果全部成功：返回Socket服务器列表，如果任一失败：返回null
        /// </summary>
        /// <param name="mode">启动模式：0标准模式（支持所有配置，启动速度慢），1兼容模式（启动速度快，只支持TextEncoding、Ip、Port、Mode、MaxRequestLength、Name配置,2快速配置(只支持端口配置其他默认)）</param>
        /// <param name="configFileAbsPath">指定EXE配置文件路径，如果未指定则读取默认配置文件(仅在兼容模式生效)</param>
        /// <returns>配置文件中已配置的Socket服务器的实例</returns>
        public List<SocketServer> Start(int mode = 0, string configFileAbsPath = null)
        {
            try
            {
                if (mode == 0)
                {
                    _bootstrap = BootstrapFactory.CreateBootstrap();
                    if (!_bootstrap.Initialize())
                    {
                        return null;
                    }
                    var result = _bootstrap.Start();
                    if (result == StartResult.Failed)
                    {
                        return null;
                    }
                    AllSocketServer = _bootstrap.AppServers.Cast<SocketServer>().ToList();
                }
                else if (mode == 1)
                {

                    ExeConfigurationFileMap ecf = new ExeConfigurationFileMap();
                    SocketServiceConfig config;
                    if (string.IsNullOrWhiteSpace(configFileAbsPath))
                    {
                        config = ConfigurationManager.GetSection("superSocket") as SocketServiceConfig;
                    }
                    else
                    {
                        if (File.Exists(configFileAbsPath))
                        {
                            ecf.ExeConfigFilename = configFileAbsPath;
                            config =
                                ConfigurationManager.OpenMappedExeConfiguration(ecf, ConfigurationUserLevel.None)
                                    .GetSection("superSocket") as SocketServiceConfig;
                        }
                        else
                        {
                            throw new Exception(string.Format("指定的配置文件路径（configFileAbsPath:{0}）不存在", configFileAbsPath));
                        }
                    }


                    if (config != null)
                    {
                        var tempServerList = new List<SocketServer>();
                        foreach (var serverConfig in config.Servers)
                        {
                            var tempServer = new SocketServer();
                            ServerConfig sc = new ServerConfig
                            {
                                TextEncoding = "UTF-8",
                                Ip = serverConfig.Ip,
                                Port = serverConfig.Port,
                                Mode = SocketMode.Tcp,
                                MaxRequestLength = 1024000,
                                Name = serverConfig.Name,
                                SendBufferSize = serverConfig.SendBufferSize,
                                SendingQueueSize = serverConfig.SendingQueueSize,
                                SyncSend = serverConfig.SyncSend
                            };
                            tempServer.Setup(sc);
                            tempServerList.Add(tempServer);
                        }
                        AllSocketServer = tempServerList;
                        if (!tempServerList.TrueForAll(s => s.Start()))
                            AllSocketServer = null;
                    }
                }
                return AllSocketServer;
            }
            catch (Exception ex)
            {
                var exs = JsonConvert.SerializeObject(ex, Formatting.Indented);
                File.WriteAllText("ex.txt", exs);
                return null;
            }
        }

        /// <summary>
        /// 停止所有Socket服务器
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            try
            {

                AllSocketServer.ForEach(s =>
                {
                    var sessions = s.GetAllSessions();
                    foreach (var session in sessions)
                    {
                        session.Close(); //关闭会话
                    }
                    s.Stop(); //停止会话
                });

                if (_bootstrap != null)
                {
                    _bootstrap.Stop();
                }

                return true;
            }
            catch (Exception ex)
            {
                var exs = JsonConvert.SerializeObject(ex, Formatting.Indented);
                File.WriteAllText("ex.txt", exs);
                return false;
            }
        }
    }
}
