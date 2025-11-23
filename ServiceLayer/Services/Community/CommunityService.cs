using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Community;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Community;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Community;
using CoreLayer.Specifications.Community;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Community
{
    public class CommunityService : ICommunityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public CommunityService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        // ==================== POSTS ====================

        public async Task<PaginationResponse<PostResponseDto>> GetAllPostsAsync(PostFilterParams filterParams, string? currentUserId = null)
        {
            if (filterParams.PageIndex < 1 || filterParams.PageSize < 1)
                throw new ArgumentException("PageIndex and PageSize must be greater than 0");

            var spec = new PostFilterSpecification(filterParams, currentUserId);
            var countSpec = new PostCountSpecification(filterParams);

            var posts = await _unitOfWork.Repository<Post, int>().GetAllWithSpecficationAsync(spec);
            var postDtos = _mapper.Map<IEnumerable<PostResponseDto>>(posts);

            // Set current user's reaction for each post
            if (!string.IsNullOrEmpty(currentUserId))
            {
                foreach (var postDto in postDtos)//make sure to set reaction of user for each post
                {
                    var post = posts.FirstOrDefault(p => p.Id == postDto.Id);//get the original post entity
                    var userReaction = post?.Reactions.FirstOrDefault(r => r.UserId == currentUserId);//get user's reaction
                    postDto.CurrentUserReaction = userReaction?.Type;
                }
            }

            var totalCount = await _unitOfWork.Repository<Post, int>().GetCountAsync(countSpec);

            return new PaginationResponse<PostResponseDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                postDtos
            );
        }

        public async Task<PostResponseDto> GetPostByIdAsync(int id, string? currentUserId = null)
        {
            var spec = new PostByIdSpecification(id);
            var post = await _unitOfWork.Repository<Post, int>().GetWithSpecficationAsync(spec);

            if (post == null)
                throw new KeyNotFoundException("Post not found or deleted");

            var postDto = _mapper.Map<PostResponseDto>(post);

            // Set current user's reaction
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var userReaction = post.Reactions.FirstOrDefault(r => r.UserId == currentUserId);
                postDto.CurrentUserReaction = userReaction?.Type;
            }

            return postDto;
        }

        public async Task<PostOperationResponseDto> CreatePostAsync(CreatePostDto dto, string userId)
        {
            // Validate species if provided
            if (dto.SpeciesId.HasValue)
            {
                var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId.Value);
                if (species == null || !species.IsActive)
                    throw new KeyNotFoundException("Species not found or inactive");
            }

            // Handle image uploads
            var imageUrls = new List<string>();
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var image in dto.Images)
                {
                    var fileName = DocumentSetting.Upload(image, "posts");
                    imageUrls.Add(fileName);
                }
            }

            var post = new Post
            {
                Content = dto.Content,
                UserId = userId,
                SpeciesId = dto.SpeciesId,
                ImageUrls = string.Join(",", imageUrls),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Post, int>().AddAsync(post);
            await _unitOfWork.CompleteAsync();

            return new PostOperationResponseDto
            {
                Success = true,
                Message = "Post created successfully",
                PostId = post.Id
            };
        }

        public async Task<PostOperationResponseDto> UpdatePostAsync(int id, UpdatePostDto dto, string userId)
        {
            var post = await _unitOfWork.Repository<Post, int>().GetAsync(id);
            if (post == null || !post.IsActive)
                throw new KeyNotFoundException("Post not found or deleted");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this post");

            // Validate species if provided
            if (dto.SpeciesId.HasValue)
            {
                var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId.Value);
                if (species == null || !species.IsActive)
                    throw new KeyNotFoundException("Species not found or inactive");
                post.SpeciesId = dto.SpeciesId;
            }

            // Handle image updates
            if (dto.Images != null && dto.Images.Any())
            {
                // Delete old images
                if (!string.IsNullOrEmpty(post.ImageUrls))
                {
                    var oldImages = post.ImageUrls.Split(',');
                    foreach (var oldImage in oldImages)
                    {
                        DocumentSetting.Delete(oldImage, "posts");
                    }
                }

                // Upload new images
                var imageUrls = new List<string>();
                foreach (var image in dto.Images)
                {
                    var fileName = DocumentSetting.Upload(image, "posts");
                    imageUrls.Add(fileName);
                }
                post.ImageUrls = string.Join(",", imageUrls);
            }

            if (!string.IsNullOrEmpty(dto.Content))
                post.Content = dto.Content;

            _unitOfWork.Repository<Post, int>().Update(post);
            await _unitOfWork.CompleteAsync();

            return new PostOperationResponseDto
            {
                Success = true,
                Message = "Post updated successfully",
                PostId = post.Id
            };
        }

        public async Task<PostOperationResponseDto> DeletePostAsync(int id, string userId)
        {
            var post = await _unitOfWork.Repository<Post, int>().GetAsync(id);
            if (post == null || !post.IsActive)
                throw new KeyNotFoundException("Post not found or already deleted");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this post");

            // Delete images
            if (!string.IsNullOrEmpty(post.ImageUrls))
            {
                var images = post.ImageUrls.Split(',');
                foreach (var image in images)
                {
                    DocumentSetting.Delete(image, "posts");
                }
            }

            post.IsActive = false;
            _unitOfWork.Repository<Post, int>().Update(post);
            await _unitOfWork.CompleteAsync();

            return new PostOperationResponseDto
            {
                Success = true,
                Message = "Post deleted successfully",
                PostId = id
            };
        }

        // ==================== COMMENTS ====================

        public async Task<CommentListDto> GetPostCommentsAsync(int postId)
        {
            var post = await _unitOfWork.Repository<Post, int>().GetAsync(postId);
            if (post == null || !post.IsActive)
                throw new KeyNotFoundException("Post not found or deleted");

            var spec = new CommentsByPostIdSpecification(postId);
            var comments = await _unitOfWork.Repository<Comment, int>().GetAllWithSpecficationAsync(spec);
            var commentDtos = _mapper.Map<IEnumerable<CommentResponseDto>>(comments);

            return new CommentListDto
            {
                Count = commentDtos.Count(),
                Data = commentDtos
            };
        }

        public async Task<CommentOperationResponseDto> CreateCommentAsync(CreateCommentDto dto, string userId)
        {
            var post = await _unitOfWork.Repository<Post, int>().GetAsync(dto.PostId);
            if (post == null || !post.IsActive)
                throw new KeyNotFoundException("Post not found or deleted");

            // Validate parent comment if provided
            if (dto.ParentCommentId.HasValue)
            {
                var parentComment = await _unitOfWork.Repository<Comment, int>().GetAsync(dto.ParentCommentId.Value);
                if (parentComment == null || parentComment.PostId != dto.PostId)
                    throw new InvalidOperationException("Invalid parent comment");
            }

            var comment = new Comment
            {
                Content = dto.Content,
                UserId = userId,
                PostId = dto.PostId,
                ParentCommentId = dto.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Comment, int>().AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            return new CommentOperationResponseDto
            {
                Success = true,
                Message = "Comment created successfully",
                CommentId = comment.Id
            };
        }

        public async Task<CommentOperationResponseDto> UpdateCommentAsync(int id, UpdateCommentDto dto, string userId)
        {
            var spec = new CommentByIdSpecification(id);
            var comment = await _unitOfWork.Repository<Comment, int>().GetWithSpecficationAsync(spec);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this comment");

            comment.Content = dto.Content;
            _unitOfWork.Repository<Comment, int>().Update(comment);
            await _unitOfWork.CompleteAsync();

            return new CommentOperationResponseDto
            {
                Success = true,
                Message = "Comment updated successfully",
                CommentId = comment.Id
            };
        }

        public async Task<CommentOperationResponseDto> DeleteCommentAsync(int id, string userId)
        {
            var comment = await _unitOfWork.Repository<Comment, int>().GetAsync(id);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this comment");

            _unitOfWork.Repository<Comment, int>().Delete(comment);
            await _unitOfWork.CompleteAsync();

            return new CommentOperationResponseDto
            {
                Success = true,
                Message = "Comment deleted successfully",
                CommentId = id
            };
        }

        // ==================== REACTIONS ====================

        public async Task<ReactionListDto> GetPostReactionsAsync(int postId)
        {
            var post = await _unitOfWork.Repository<Post, int>().GetAsync(postId);
            if (post == null || !post.IsActive)
                throw new KeyNotFoundException("Post not found or deleted");

            var spec = new ReactionsByPostIdSpecification(postId);
            var reactions = await _unitOfWork.Repository<PostReaction, int>().GetAllWithSpecficationAsync(spec);
            var reactionDtos = _mapper.Map<IEnumerable<ReactionResponseDto>>(reactions);

            var summary = reactions
                .GroupBy(r => r.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return new ReactionListDto
            {
                Count = reactionDtos.Count(),
                Summary = summary,
                Data = reactionDtos
            };
        }

        public async Task<ReactionOperationResponseDto> ToggleReactionAsync(CreateReactionDto dto, string userId)
        {
            var post = await _unitOfWork.Repository<Post, int>().GetAsync(dto.PostId);
            if (post == null || !post.IsActive)
                throw new KeyNotFoundException("Post not found or deleted");

            var spec = new UserReactionOnPostSpecification(dto.PostId, userId);
            var existingReaction = await _unitOfWork.Repository<PostReaction, int>()
                .GetWithSpecficationAsync(spec);

            if (existingReaction != null)
            {
                // If same type, remove reaction
                if (existingReaction.Type == dto.Type)
                {
                    _unitOfWork.Repository<PostReaction, int>().Delete(existingReaction);
                    await _unitOfWork.CompleteAsync();

                    return new ReactionOperationResponseDto
                    {
                        Success = true,
                        Message = "Reaction removed",
                        ReactionId = null
                    };
                }

                // Otherwise, update to new type
                existingReaction.Type = dto.Type;
                existingReaction.CreatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<PostReaction, int>().Update(existingReaction);
                await _unitOfWork.CompleteAsync();

                return new ReactionOperationResponseDto
                {
                    Success = true,
                    Message = "Reaction updated",
                    ReactionId = existingReaction.Id
                };
            }

            // Create new reaction
            var reaction = new PostReaction
            {
                UserId = userId,
                PostId = dto.PostId,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PostReaction, int>().AddAsync(reaction);
            await _unitOfWork.CompleteAsync();

            return new ReactionOperationResponseDto
            {
                Success = true,
                Message = "Reaction added",
                ReactionId = reaction.Id
            };
        }
    }
}
