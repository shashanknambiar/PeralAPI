/*
 * ──────────────────────────────────────────────────────────────────
 *  TypeScript / SignalR client usage example
 * ──────────────────────────────────────────────────────────────────
 *
 *  npm install @microsoft/signalr
 *
 *  import * as signalR from "@microsoft/signalr";
 *
 *  const connection = new signalR.HubConnectionBuilder()
 *    .withUrl("https://localhost:7232/hubs/notifications", {
 *      accessTokenFactory: () => localStorage.getItem("accessToken") ?? "",
 *    })
 *    .withAutomaticReconnect()
 *    .build();
 *
 *  connection.on("ReceiveNotification", (notification) => {
 *    console.log("New notification:", notification);
 *    // notification: { id, type, userId, message, metadata, createdAt, isRead }
 *  });
 *
 *  connection.on("UnreadCountUpdated", (count: number) => {
 *    console.log("Unread count:", count);
 *  });
 *
 *  await connection.start();
 * ──────────────────────────────────────────────────────────────────
 */

namespace PeralAPI.Hubs
{
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;

    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId != null)
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId != null)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
