using Shareds.DTOs.NovelSeries;
using Shareds.DTOs.Comment;

namespace InteractionService.Service.Inteface
{ 

    public interface ICommentService
    {
        Task<CommentDto> CreateCommentAsync(Guid userId, int? seriesId, int? chapterId, CreateCommentDto dto);
        Task<PagedResult<CommentDto>> GetCommentsAsync(int? seriesId, int? chapterId, int pageNumber = 1, int pageSize = 20);
        Task<PagedResult<CommentDto>> GetRepliesAsync(Guid parentCommentId, int pageNumber = 1, int pageSize = 10);
        Task<CommentDto?> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentDto dto);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
        Task<bool> DeleteCommentsBySeriesAsync(int seriesId);
        Task<bool> DeleteCommentsByChapterAsync(int chapterId);

         Task<bool> AdminDeleteCommentAsync(Guid commentId);
    }
}
