using AutoMapper;
using NovelService.Models;
using Shared.DTOs.Novel;
using Shared.DTOs.Chapter;
using Shared.DTOs.Category;
using Shared.DTOs.Tag;
using Shared.DTOs.NovelStatus;

namespace Shared.Mappings
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            // Novel
            CreateMap<Novel, NovelDetailDto>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_Id))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.category != null ? src.category.category_name : ""))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.status != null ? src.status.statusName : ""))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.NovelTags.Select(nt => nt.Tag.tagName)))
                .ForMember(dest => dest.Chapters, opt => opt.MapFrom(src => src.Chapters));

            CreateMap<Novel, NovelListDto>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_Id))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.category != null ? src.category.category_name : ""))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.status != null ? src.status.statusName : ""));

            CreateMap<NovelCreateDto, Novel>()
                .ForMember(dest => dest.novel_Id, opt => opt.Ignore())
                .ForMember(dest => dest.created_at, opt => opt.Ignore())
                .ForMember(dest => dest.updated_at)
            CreateMap<NovelUpdateDto, Novel>();

            // Chapter
            CreateMap<Chapter, ChapterDetailDto>()
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at));

            CreateMap<Chapter, ChapterSummaryDto>()
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id));

            CreateMap<ChapterCreateDto, Chapter>();
            CreateMap<ChapterUpdateDto, Chapter>();

            // Category
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>();

            // Tag
            CreateMap<Tag, TagDto>();
            CreateMap<TagDto, Tag>();

            // Status
            CreateMap<NovelStatus, NovelStatusDto>();
            CreateMap<NovelStatusDto, NovelStatus>();
        }
    }
}
