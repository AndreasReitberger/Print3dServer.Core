using System.Diagnostics;

namespace Print3dServer.Core.Test
{
    public class Tests
    {
        private readonly string tokenString = SecretAppSettingReader.ReadSection<SecretAppSetting>("TestSetup")?.ApiKey ?? "";
        private readonly string target = SecretAppSettingReader.ReadSection<SecretAppSetting>("TestSetup")?.TargetUri ?? "";
        private readonly string targetWS = SecretAppSettingReader.ReadSection<SecretAppSetting>("TestSetup")?.TargetUriWebsocket ?? "";
        private MyOwnPrintServerClient? client;

        [SetUp]
        public void Setup()
        {
            client = new MyOwnPrintServerClient.MyOwnPrintServerConnectionBuilder()
                .AsRepetierServer(target, tokenString)
                .WithWebSocket(targetWS)
                .Build();
            client.Error += (sender, args) =>
            {
                if (!client.ReThrowOnError)
                {
                    Assert.Fail($"Error: {args?.ToString()}");
                }
            };
            client.RestApiError += (sender, args) =>
            {
                if (!client.ReThrowOnError)
                {
                    Assert.Fail($"REST-Error: {args?.ToString()}");
                }
            };
        }

        [Test]
        public async Task TestWebSocketAsync()
        {
            try
            {
                // Tested with a Repetier Server pro connection
                if (client is null) throw new NullReferenceException($"The client was null!");
                await client.SetPrinterActiveAsync();
                await client.CheckOnlineAsync(commandBase: client.FullWebAddress, authHeaders: client.AuthHeaders, $"printer/api/info", timeout: 10000);

                DateTime start = DateTime.Now;

                CancellationTokenSource cts = new(new TimeSpan(0, 15, 0));
                client.WebSocketError += (sender, args) =>
                {
                    cts.Cancel();
                    Assert.Fail($"WebSocket Error: {args?.ToString()}");
                };
                client.WebSocketDisconnected += (sender, args) =>
                {
                    cts.Cancel();
                    Assert.Fail($"WebSocket Disconnected: {args?.ToString()}");
                };
                client.WebSocketMessageReceived += (sender, args) =>
                {
                    // Handle incoming WebSocket messages here
                    Debug.WriteLine($"WebSocket Message Received: {args?.Message}");
                };
                client.WebSocketPingSent += (sender, args) =>
                {
                    Debug.WriteLine($"WebSocket: Ping Sent {args?.PingCommand} ({args?.Timestamp})");
                };

                await client.StartListeningAsync(); 
                Assert.That(client.IsListening);
                while (cts.IsCancellationRequested == false)
                {
                    // Keep the WebSocket connection alive for 15 minutes
                    await Task.Delay(1000, cts.Token).ConfigureAwait(false);
                }
                Assert.That(client.IsListening);
            }
            catch(Exception ex)
            {
                Assert.Fail($"Exception during WebSocket test: {ex.Message}");
            }
        }

        #region Cleanup
        [TearDown]
        public void BaseTearDown()
        {
            client?.Dispose();
        }
        #endregion
    }
}