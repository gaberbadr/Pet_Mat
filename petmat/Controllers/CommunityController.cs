using System.Security.Claims;
using CoreLayer.Dtos.Community;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [Authorize]
    public class CommunityController : BaseApiController
    {
        private readonly ICommunityService _communityService;

        public CommunityController(ICommunityService communityService)
        {
            _communityService = communityService;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ==================== POSTS ====================


        /// Get all posts with filtering and pagination
        [HttpGet("posts")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginationResponse<PostResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResponse<PostResponseDto>>> GetAllPosts(
            [FromQuery] PostFilterParams filterParams)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
                var result = await _communityService.GetAllPostsAsync(filterParams, userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Get post by ID
        [HttpGet("post/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostResponseDto>> GetPostById(int id)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
                var result = await _communityService.GetPostByIdAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Create a new post
        [HttpPost("post")]
        [ProducesResponseType(typeof(PostOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PostOperationResponseDto>> CreatePost([FromForm] CreatePostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _communityService.CreatePostAsync(dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Update an existing post
        [HttpPut("post/{id}")]
        [ProducesResponseType(typeof(PostOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostOperationResponseDto>> UpdatePost(int id, [FromForm] UpdatePostDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _communityService.UpdatePostAsync(id, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiErrorResponse(403, ex.Message));
            }
        }


        /// Delete a post
        [HttpDelete("post/{id}")]
        [ProducesResponseType(typeof(PostOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostOperationResponseDto>> DeletePost(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _communityService.DeletePostAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiErrorResponse(403, ex.Message));
            }
        }

        // ==================== COMMENTS ====================


        /// Get all comments for a post (includes nested replies)
        [HttpGet("post/{postId}/comments")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommentListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentListDto>> GetPostComments(int postId)
        {
            try
            {
                var result = await _communityService.GetPostCommentsAsync(postId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Create a comment or reply
        [HttpPost("comment")]
        [ProducesResponseType(typeof(CommentOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommentOperationResponseDto>> CreateComment([FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _communityService.CreateCommentAsync(dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Update a comment
        [HttpPut("comment/{id}")]
        [ProducesResponseType(typeof(CommentOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentOperationResponseDto>> UpdateComment(
            int id, [FromBody] UpdateCommentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _communityService.UpdateCommentAsync(id, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiErrorResponse(403, ex.Message));
            }
        }


        /// Delete a comment
        [HttpDelete("comment/{id}")]
        [ProducesResponseType(typeof(CommentOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentOperationResponseDto>> DeleteComment(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _communityService.DeleteCommentAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiErrorResponse(403, ex.Message));
            }
        }

        // ==================== REACTIONS ====================


        /// Get all reactions for a post
        [HttpGet("post/{postId}/reactions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ReactionListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReactionListDto>> GetPostReactions(int postId)
        {
            try
            {
                var result = await _communityService.GetPostReactionsAsync(postId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Toggle reaction on a post (add/update/remove)
        [HttpPost("reaction")]
        [ProducesResponseType(typeof(ReactionOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReactionOperationResponseDto>> ToggleReaction([FromBody] CreateReactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _communityService.ToggleReactionAsync(dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }
    }
}
