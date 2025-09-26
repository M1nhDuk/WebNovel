using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using Shareds.DTOs.Novel;
using System.Transactions;

namespace NovelService.Service
{
    public class NovelService : INoveLService
    {
        private readonly NovelDbContext _db;
        private readonly IMapper _mapper;

        public NovelService(NovelDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        //Create
        public async Task<NovelDetailDto> CreateNovelAsync (CreateNovelDto dto)
        {
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var novel = _mapper.Map<Novel>(dto);

                novel.views = 0;
                novel.word_count = 0;

                _db.Novels.Add(novel);
                await _db.SaveChangesAsync();

                if (dto.TagIds != null && dto.TagIds.Any())
                {
                    var tags = await _db.Tags.Where(t => dto.TagIds.Contains(t.tagId)).ToListAsync();
                    foreach (var t in tags)
                    {
                        var nt = new NovelTag
                        {
                            novelID = novel.novel_Id,
                            novelTagId = t.tagId,
                            Tag = t
                        };
                        _db.Novel_Tags.Add(nt);
                    }
                    await _db.SaveChangesAsync();
                }

                if (dto.Chapters != null && dto.Chapters.Any())
                {
                    foreach (var chapterDto in dto.Chapters)
                    {
                        await ShiftChaptersForInsert(novel.novel_Id, chapterDto.chapter_number);
                    }
                }
            }
            catch (Exception ex) 
            {
            }
        }
    }



}
