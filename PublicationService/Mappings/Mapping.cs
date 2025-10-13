using AutoMapper;
using Shareds.DTOs.Novel;
using Shareds.DTOs.Category;
using Shareds.DTOs.Chapter;
using Shareds.DTOs.NovelStatus;
using Shareds.DTOs.Tag;
using Shareds.DTOs;
using NovelService.Models;
using Shareds.DTOs.NovelSeries;

namespace NovelService.Mappings
{
    public class Mapping : Profile
    {
        public Mapping()
        {
         //Series
            CreateMap<NovelSeries, NovelSeriesDetailDto>()
                .ForMember(dest => dest.categoryName,
                   opt => opt.MapFrom(src => src.category != null ? src.category.category_name : null))

            
                 .ForMember(dest => dest.statusName,
                   opt => opt.MapFrom(src => src.status != null ? src.status.statusName : null))

    
                 .ForMember(dest => dest.Tags,
                   opt => opt.MapFrom(src => src.NovelTags.Select(nt => nt.Tag.tagName)))

                    .ForMember(dest => dest.Novels,
                   opt => opt.MapFrom(src => src.Novel))

                //Ignore do lấy từ service khác
                .ForMember(dest => dest.uploader_name, opt => opt.Ignore())
                .ForMember(dest => dest.uploader_avatar, opt => opt.Ignore());

            CreateMap<NovelSeries, NovelSeriesSummary>();

            CreateMap<NovelSeries, SeriesListDto>()
                .ForMember(dest => dest.categoryName,
                           opt => opt.MapFrom(src => src.category != null ? src.category.category_name : null))
                .ForMember(dest => dest.statusName,
                           opt => opt.MapFrom(src => src.status != null ? src.status.statusName : null))
                .ForMember(dest => dest.Tags,
                           opt => opt.MapFrom(src => src.NovelTags.Select(nt => nt.Tag.tagName)));


            CreateMap<CreateNovelService, NovelSeries>()

                 .ForMember(dest => dest.NovelTags,
                     opt => opt.MapFrom(src => src.TagIds != null
                         ? src.TagIds.Select(id => new NovelTag { tagID = id })
                         : new List<NovelTag>()))

                 .ForMember(dest => dest.series_Id, opt => opt.Ignore())
                 .ForMember(dest => dest.Novel, opt => opt.MapFrom(src => src.Novels))
                 .ForMember(dest => dest.word_count, opt => opt.Ignore())
                 .ForMember(dest => dest.views, opt => opt.Ignore())
                 .ForMember(dest => dest.created_at, opt => opt.Ignore())
                 .ForMember(dest => dest.updated_at, opt => opt.Ignore());



            CreateMap<UpdateNovelService, NovelSeries>()
                // Map tagIds sang NovelTags
                .ForMember(dest => dest.NovelTags,
                           opt => opt.MapFrom(src => src.TagIds != null
                               ? src.TagIds.Select(id => new NovelTag { novelTagId = id })
                               : new List<NovelTag>()))

                // Ignore những field không cho update trực tiếp
                .ForMember(dest => dest.series_Id, opt => opt.Ignore())
                .ForMember(dest => dest.word_count, opt => opt.Ignore())
                .ForMember(dest => dest.views, opt => opt.Ignore())
                .ForMember(dest => dest.created_at, opt => opt.Ignore())
                .ForMember(dest => dest.updated_at, opt => opt.Ignore())
                .ForMember(dest => dest.uploader_id, opt => opt.Ignore());



            // Novel
            CreateMap<Novel, NovelDetailDto>()
            // Map từ NovelSeries
            .ForMember(dest => dest.author,
                       opt => opt.MapFrom(src => src.NovelSeries.author))
            .ForMember(dest => dest.artist,
                       opt => opt.MapFrom(src => src.NovelSeries.artist))
            .ForMember(dest => dest.uploader_id,
                       opt => opt.MapFrom(src => src.NovelSeries.uploader_id));

            CreateMap<Novel, NovelListDto>();
            CreateMap<Novel, NovelSummary>();
            CreateMap<Novel, NovelReorder>();

            // Map từ item
            CreateMap<NovelReorderItem, Novel>()
                .ForMember(dest => dest.novel_Id, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.novel_number, opt => opt.MapFrom(src => src.new_position))
                .ForAllMembers(opt => opt.Ignore());


            //Input
            CreateMap<CreateNovelDto, Novel>()

                //Igore
                .ForMember(d => d.novel_Id, opt => opt.Ignore())
                .ForMember(dest => dest.novel_number, opt => opt.Ignore());
                
            CreateMap<NovelUpdateDto, Novel>()

                //Ignore
                .ForMember(dest => dest.novel_number, opt => opt.Ignore())
                .ForMember(dest => dest.novel_Id, opt => opt.Ignore())
                .ForMember(dest => dest.series_Id, opt => opt.Ignore())
                .ForMember(dest => dest.updated_at, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


            //Chapter
            CreateMap<Chapter, ChapterDetailDto>()
                .ForMember(c => c.created_at, opt => opt.Ignore())
                .ForMember(c => c.word_count, opt => opt.Ignore());

            CreateMap<Chapter, ChapterSummaryDto>();
               

            CreateMap<ChapterCreateDto, Chapter>()

                //Ignore
                 .ForMember(dest => dest.word_count, opt => opt.Ignore())
                 .ForMember(dest => dest.chapter_id, opt => opt.Ignore())
                 .ForMember(dest => dest.created_at, opt => opt.Ignore());

            CreateMap<ChapterUpdateDto, Chapter>()
                 
                 .ForMember(dest => dest.chapter_id, opt => opt.Ignore())
                 .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


            //Category
            CreateMap<Category, CategoryDto>();

            CreateMap<CategoryDto, Category>();




            //Tag
            CreateMap<Tag, TagDto>();


            CreateMap<TagDto, Tag>();


            //NovelStatus
            CreateMap<NovelStatus, NovelStatusDto>();

            CreateMap<NovelStatusDto, NovelStatus>();
                


        }
    }
}

