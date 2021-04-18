﻿using API.DTOs;
using API.Extensions;
using API.Interface;
using API.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
            .Include(c => c.Connections)
            .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                      .Include(u => u.Sender)
                      .Include(u => u.Recipient)
                      .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Group> GetmessageGroup(string groupName)
        {
            return await _context.Groups
                           .Include(x => x.Connections)
                           .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
               .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                       && m.Sender.UserName == recipientUsername
                       || m.Recipient.UserName == recipientUsername
                       && m.Sender.UserName == currentUsername && m.SenderDelete == false
               )
               .MarkUnreadAsRead(currentUsername)
               .OrderBy(m => m.MessageSent)
               .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
               .ToListAsync();

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}