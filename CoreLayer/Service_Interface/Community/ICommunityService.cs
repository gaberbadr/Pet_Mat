using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Community;
using CoreLayer.Helper.Pagination;

namespace CoreLayer.Service_Interface.Community
{
    public interface ICommunityService
    {
        // Posts
        Task<PaginationResponse<PostResponseDto>> GetAllPostsAsync(PostFilterParams filterParams, string? currentUserId = null);
        Task<PostResponseDto> GetPostByIdAsync(int id, string? currentUserId = null);
        Task<PostOperationResponseDto> CreatePostAsync(CreatePostDto dto, string userId);
        Task<PostOperationResponseDto> UpdatePostAsync(int id, UpdatePostDto dto, string userId);
        Task<PostOperationResponseDto> DeletePostAsync(int id, string userId);

        // Comments
        Task<CommentListDto> GetPostCommentsAsync(int postId);
        Task<CommentOperationResponseDto> CreateCommentAsync(CreateCommentDto dto, string userId);
        Task<CommentOperationResponseDto> UpdateCommentAsync(int id, UpdateCommentDto dto, string userId);
        Task<CommentOperationResponseDto> DeleteCommentAsync(int id, string userId);

        // Reactions
        Task<ReactionListDto> GetPostReactionsAsync(int postId);
        Task<ReactionOperationResponseDto> ToggleReactionAsync(CreateReactionDto dto, string userId);
    }
}
