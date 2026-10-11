using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RealtimeChat.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private string CurrentUserName => Context.User?.Identity?.Name ?? "Unknown";

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync(
            "ReceiveMessage",
            "System",
            $"Bạn đã kết nối. ConnectionId: {CurrentUserName}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync(
            "ReceiveMessage",
            "System",
            $"{CurrentUserName} đã tham gia phòng {roomName}");
    }

    public async Task LeaveRoom(string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync(
            "ReceiveMessage",
            "System",
            $"{CurrentUserName} đã rời phòng {roomName}");
    }

    public async Task SendMessageToRoom(string roomName, string message)
    {
        if (string.IsNullOrWhiteSpace(roomName) ||
            string.IsNullOrWhiteSpace(message))
            return;

        if (message.Length > 2000)
            message = message[..2000];

        // Có thể thêm validate message ở đây
        await Clients.Group(roomName).SendAsync(
            "ReceiveMessage",
            CurrentUserName,
            message);
    }
}