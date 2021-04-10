using SimpleSockets;
using SimpleSockets.Messaging.Metadata;
using SimpleSockets.Server;
using System.Collections.Generic;

namespace BackToTheFutureV
{
    internal static class WaybackServer
    {
        private static SimpleSocketListener wServer = new SimpleSocketTcpListener();

        public static bool Connected => Clients.Count > 0;

        public static IDictionary<int, IClientInfo> Clients => wServer.GetConnectedClients();

        static WaybackServer()
        {
            wServer.ClientConnected += WServer_ClientConnected;
            wServer.ClientDisconnected += WServer_ClientDisconnected;
            wServer.BytesReceived += WServer_BytesReceived;
        }

        private static void WServer_BytesReceived(IClientInfo client, byte[] messageData)
        {
            if (!WaybackSystem.AddFromData(messageData))
                WaybackSystem.RecordFromData(messageData);
        }

        private static void WServer_ClientDisconnected(IClientInfo client, DisconnectReason reason)
        {
            foreach (int id in Clients.Keys)
                wServer?.Close(id);
        }

        private static void WServer_ClientConnected(IClientInfo clientInfo)
        {
            wServer.SendBytes(clientInfo.Id, WaybackSystem.CurrentRecording);
        }

        public static void Send(byte[] data)
        {
            foreach (int id in Clients.Keys)
                wServer.SendBytes(id, data);
        }

        public static void StartServer()
        {
            foreach (int id in Clients.Keys)
                wServer?.Close(id);

            wServer.StartListening(13000);

            GTA.UI.Notification.Show("Server Started");
        }

        public static void Dispose()
        {
            foreach (int id in Clients.Keys)
                wServer?.Close(id);

            wServer?.Dispose();
        }
    }
}