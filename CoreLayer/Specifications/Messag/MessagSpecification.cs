using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Messag;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Messages;

namespace CoreLayer.Specifications.Messag
{
    // Messages with pagination
    public class MessagesBetweenUsersPaginatedSpecification : BaseSpecifications<Message, int>
    {
        public MessagesBetweenUsersPaginatedSpecification(
            string userId1,
            string userId2,
            MessageFilterParams filterParams)
            : base(m =>
                ((m.SenderId == userId1 && m.ReceiverId == userId2) ||
                 (m.SenderId == userId2 && m.ReceiverId == userId1)) &&
                !m.IsDeleted)
        {
            Includes.Add(m => m.Sender);
            Includes.Add(m => m.Receiver);

            // Order by most recent first (descending)
            OrderByDescending = m => m.SentAt;

            // Apply pagination
            applyPagnation(
                skip: (filterParams.PageIndex - 1) * filterParams.PageSize,
                take: filterParams.PageSize
            );
        }
    }

    // Count messages between users
    public class MessagesBetweenUsersCountSpecification : BaseSpecifications<Message, int>
    {
        public MessagesBetweenUsersCountSpecification(string userId1, string userId2)
            : base(m =>
                ((m.SenderId == userId1 && m.ReceiverId == userId2) ||
                 (m.SenderId == userId2 && m.ReceiverId == userId1)) &&
                !m.IsDeleted)
        {
        }
    }

    // Latest message per conversation
    public class LatestMessagePerConversationSpecification : BaseSpecifications<Message, int>
    {
        public LatestMessagePerConversationSpecification(string userId)
            : base(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted)
        {
            Includes.Add(m => m.Sender);
            Includes.Add(m => m.Receiver);
            OrderByDescending = m => m.SentAt;
        }
    }

    // Unread messages from specific user
    public class UnreadMessagesFromUserSpecification : BaseSpecifications<Message, int>
    {
        public UnreadMessagesFromUserSpecification(string receiverId, string senderId)
            : base(m => m.ReceiverId == receiverId &&
                       m.SenderId == senderId &&
                       !m.IsRead &&
                       !m.IsDeleted)
        {
        }
    }

    // All unread messages count
    public class UnreadMessagesCountSpecification : BaseSpecifications<Message, int>
    {
        public UnreadMessagesCountSpecification(string userId)
            : base(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted)
        {
        }
    }

    // Block specifications
    public class ActiveBlockSpecification : BaseSpecifications<UserBlock, int>
    {
        public ActiveBlockSpecification(string blockerId, string blockedId)
            : base(b => b.BlockerId == blockerId &&
                       b.BlockedId == blockedId &&
                       b.IsActive == true)
        {
        }
    }

    public class BlockedUsersSpecification : BaseSpecifications<UserBlock, int>
    {
        public BlockedUsersSpecification(string userId)
            : base(b => b.BlockerId == userId && b.IsActive == true)
        {
            Includes.Add(b => b.Blocked);
            OrderByDescending = b => b.CreatedAt;
        }
    }

    public class IsUserBlockedSpecification : BaseSpecifications<UserBlock, int>
    {
        public IsUserBlockedSpecification(string blockerId, string blockedId)
            : base(b =>
                ((b.BlockerId == blockerId && b.BlockedId == blockedId) ||
                 (b.BlockerId == blockedId && b.BlockedId == blockerId)) &&
                b.IsActive == true)
        {
        }
    }
}
