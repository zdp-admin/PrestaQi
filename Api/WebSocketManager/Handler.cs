using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WebSocketManager
{
    public abstract class WebSocketHandler
    {
        protected ConnectionManager WebSocketConnectionManager { get; set; }

        public WebSocketHandler(ConnectionManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            WebSocketConnectionManager.AddSocket(socket);
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));
        }

        public async Task SendMessageAsync(WebSocket socket, object data)
        {
            if (socket.State != WebSocketState.Open)
                return;

            string message = JsonConvert.SerializeObject(data);

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                                                                  offset: 0,
                                                                  count: message.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string socketId, string message)
        {
            await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);
        }

        public async Task SendMessageToAllAsync(object data)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            };

            string message =  JsonConvert.SerializeObject(data, jsonSerializerSettings);
            byte[] string_to_send = UTF8Encoding.UTF8.GetBytes(message);

            foreach (var pair in WebSocketConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, UTF8Encoding.UTF8.GetString(string_to_send));
            }
        }

        //TODO - decide if exposing the message string is better than exposing the result and buffer
        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}
