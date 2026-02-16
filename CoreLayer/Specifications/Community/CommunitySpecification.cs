using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Community;
using CoreLayer.Entities.Community;

namespace CoreLayer.Specifications.Community
{
    // ==================== POST SPECIFICATIONS ====================

    public class PostFilterSpecification : BaseSpecifications<Post, int>
    {
        public PostFilterSpecification(PostFilterParams filterParams, string? currentUserId = null)
            : base(BuildCriteria(filterParams))
        {

            Includes.Add(dp => dp.User);
            Includes.Add(dp => dp.Species);
            Includes.Add(dp => dp.Comments);
            Includes.Add(dp => dp.Reactions);

            OrderByDescending = p => p.CreatedAt;

            applyPagnation(
                skip: (filterParams.PageIndex - 1) * filterParams.PageSize,
                take: filterParams.PageSize
            );
        }

        private static Expression<Func<Post, bool>> BuildCriteria(PostFilterParams filterParams)
        {
            return p =>
                p.IsActive == true &&
                (!filterParams.SpeciesId.HasValue || p.SpeciesId == filterParams.SpeciesId.Value) &&
                (string.IsNullOrEmpty(filterParams.UserId) || p.UserId == filterParams.UserId) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    p.Content.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class PostCountSpecification : BaseSpecifications<Post, int>
    {
        public PostCountSpecification(PostFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
        }

        private static Expression<Func<Post, bool>> BuildCriteria(PostFilterParams filterParams)
        {
            return p =>
                p.IsActive == true &&
                (!filterParams.SpeciesId.HasValue || p.SpeciesId == filterParams.SpeciesId.Value) &&
                (string.IsNullOrEmpty(filterParams.UserId) || p.UserId == filterParams.UserId) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    p.Content.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class PostByIdSpecification : BaseSpecifications<Post, int>
    {
        public PostByIdSpecification(int id)
            : base(p => p.Id == id && p.IsActive == true)
        {
            Includes.Add(dp => dp.User);
            Includes.Add(dp => dp.Species);
            Includes.Add(dp => dp.Comments);
            Includes.Add(dp => dp.Reactions);
        }
    }

    // ==================== COMMENT SPECIFICATIONS ====================

    public class CommentsByPostIdSpecification : BaseSpecifications<Comment, int>
    {
        public CommentsByPostIdSpecification(int postId)
            : base(c => c.PostId == postId && c.ParentCommentId == null) // Only top-level comments
        {
            Includes.Add(dp => dp.User); // Include comment author
            AddInclude("Replies.User"); // Include nested replies and their authors
            AddInclude("Replies.Replies.User"); // Two levels deep replies 
            AddInclude("Replies.Replies.Replies.User"); // threed levels deep replies

            #region
            /*
             * Step 1: Replies (first level)
                EF Core loads the main comment (ID = 1), then asks the database:
                “Bring me all comments where ParentCommentId = 1.”
                It finds comments 3 and 4.

                Step 2: Replies.Replies (second level)
                EF Core then looks at comments 3 and 4 and says:
                “Now bring me their children too.”
                It finds comments 5 and 6 (children of comment 3).

                Step 3: Replies.Replies.User 
                EF Core checks comments 5 and 6 and says:
                “Get the User who wrote each of these comments.”
                It loads their related User data.

                           
                 ┣ 💬 Comment #1: "who is agree with me?" (Parent: NULL)                  
                 ┃  ┣ ↪️ Reply #3: "i with you" (Parent: 1)                                                     level 1
                 ┃  ┃  ┣ ↪️ Reply #5: "are you blind mssi..." (Parent: 3)                           level 2       
                 ┃  ┃  ┗ ↪️ Reply #6: "think again" (Parent: 3)                                            level 2
                 ┃  ┃     ┗ ↪️ Reply #7: "nooooo cr7..." (Parent: 6)                                      level 3
                 ┃  ┗ ↪️ Reply #4: "i dont think that" (Parent: 1)                                        level 1
                 ┃
                 ┗ 💬 Comment #2: "I think so" (Parent: NULL)
                    ┗ (no replies)
            but in reply 7 if have more depth it will not load it because we stop just at 3 levels deep
            so if some one reply on #7 it will not loaded 
             */
            #endregion


            OrderBy = c => c.CreatedAt;
        }
    }

    public class CommentByIdSpecification : BaseSpecifications<Comment, int>
    {
        public CommentByIdSpecification(int id)
            : base(c => c.Id == id)
        {
            Includes.Add(dp => dp.User);
            Includes.Add(dp => dp.Post);
        }
    }

    // ==================== REACTION SPECIFICATIONS ====================

    public class ReactionsByPostIdSpecification : BaseSpecifications<PostReaction, int>
    {
        public ReactionsByPostIdSpecification(int postId)
            : base(r => r.PostId == postId)
        {
            Includes.Add(dp => dp.User);
            OrderByDescending = r => r.CreatedAt;
        }
    }

    public class UserReactionOnPostSpecification : BaseSpecifications<PostReaction, int>
    {
        public UserReactionOnPostSpecification(int postId, string userId)
            : base(r => r.PostId == postId && r.UserId == userId)
        {
        }
    }
}
