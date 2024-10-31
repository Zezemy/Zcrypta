﻿using BlazorWebAppTest1.Context;
using Microsoft.AspNetCore.SignalR;
using Zcda.Entities.Dtos;

namespace BlazorWebAppTest1.Hubs
{
    public class ChatHub : Hub
    {
        private ApplicationDbContext _dbContext { get; }
        public ChatHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task SendMessage(string user, string message)
        {
            var now = DateTime.Now;
            _dbContext.ChatMessages.Add(new ChatMessage() { Message = message, SenderUserId = long.Parse(user), CreateDate = now, ReceivedDateTime = now, SentDateTime = now, ReceiverUserId = 0 });
            _dbContext.SaveChanges();
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
