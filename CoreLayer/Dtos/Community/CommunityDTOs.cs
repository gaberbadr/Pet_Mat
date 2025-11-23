using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Animals;
using CoreLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Dtos.Community
{
    // ==================== POST DTOs ====================

    public class PostResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorProfilePicture { get; set; }
        public int? SpeciesId { get; set; }
        public string SpeciesName { get; set; }
        public List<string> ImageUrls { get; set; }
        public int CommentsCount { get; set; }
        public int ReactionsCount { get; set; }
        public Dictionary<string, int> ReactionsSummary { get; set; }
        public ReactionType? CurrentUserReaction { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePostDto
    {
        [Required]
        public string Content { get; set; }

        public int? SpeciesId { get; set; }

        public List<IFormFile> Images { get; set; }
    }

    public class UpdatePostDto
    {
        public string Content { get; set; }
        public int? SpeciesId { get; set; }
        public List<IFormFile>? Images { get; set; }
    }

    public class PostFilterParams
    {
        public int? SpeciesId { get; set; }
        public string? UserId { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PostOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? PostId { get; set; }
    }

    // ==================== COMMENT DTOs ====================

    public class CommentResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorProfilePicture { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CommentResponseDto> Replies { get; set; }
    }

    public class CreateCommentDto
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        public string Content { get; set; }

        public int? ParentCommentId { get; set; }
    }

    public class UpdateCommentDto
    {
        [Required]
        public string Content { get; set; }
    }

    public class CommentListDto
    {
        public int Count { get; set; }
        public IEnumerable<CommentResponseDto> Data { get; set; }
    }

    public class CommentOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? CommentId { get; set; }
    }

    // ==================== REACTION DTOs ====================

    public class ReactionResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
        public ReactionType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReactionDto
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        public ReactionType Type { get; set; }
    }

    public class ReactionListDto
    {
        public int Count { get; set; }
        public Dictionary<string, int> Summary { get; set; }
        public IEnumerable<ReactionResponseDto> Data { get; set; }
    }

    public class ReactionOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ReactionId { get; set; }
    }
}
