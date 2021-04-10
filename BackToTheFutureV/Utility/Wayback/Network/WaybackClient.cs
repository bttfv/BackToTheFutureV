using GTA;
using SimpleSockets.Client;

namespace BackToTheFutureV
{
    internal static class WaybackClient
    {
        private static SimpleSocketClient wClient = new SimpleSocketTcpClient();

        public static bool Connected => wClient.IsConnected();

        static WaybackClient()
        {
            wClient.ConnectedToServer += WClient_ConnectedToServer;
            wClient.DisconnectedFromServer += WClient_DisconnectedFromServer;
            wClient.BytesReceived += WClient_BytesReceived;
        }

        private static void WClient_BytesReceived(SimpleSocketClient client, byte[] messageBytes)
        {
            if (!WaybackSystem.AddFromData(messageBytes))
                WaybackSystem.RecordFromData(messageBytes);
        }

        private static void WClient_DisconnectedFromServer(SimpleSocketClient client)
        {
            wClient?.Close();
        }

        private static void WClient_ConnectedToServer(SimpleSocketClient client)
        {
            wClient.SendBytes(WaybackSystem.CurrentRecording);
        }

        public static void Send(byte[] data)
        {
            wClient.SendBytes(data);
        }

        public static void StartClient()
        {
            if (Connected)
                wClient?.Close();

            wClient.StartClient(Game.GetUserInput(WindowTitle.EnterCustomTeamName, "", 15), 13000);

            GTA.UI.Notification.Show("Client Started");
        }

        public static void Dispose()
        {
            wClient?.Close();
            wClient?.Dispose();
        }
    }
}
