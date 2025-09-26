using AutoMapper;
using Shareds.DTOs.Novel;
using Shareds.DTOs.Category;
using Shareds.DTOs.Chapter;
using Shareds.DTOs.NovelStatus;
using Shareds.DTOs.Tag;
using Shareds.DTOs;
using NovelService.Models;

namespace NovelService.Mappings
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            // Novel
            CreateMap<Novel, NovelDetailDto>()               
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.NovelTags.Select(nt => nt.Tag.tagName)))
                .ForMember(dest => dest.categoryName, o => o.MapFrom(s => s.category != null ? s.category.category_name : null))
                .ForMember(d => d.statusName, o => o.MapFrom(s => s.status != null ? s.status.statusName : null))
                .ForMember(d => d.Chapters, o => o.MapFrom(s => s.Chapters.OrderBy(c => c.chapter_number))); 

            
            CreateMap<Novel, NovelListDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.category != null ? src.category.category_name : ""))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.status != null ? src.status.statusName : ""))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.NovelTags.Select(nt => nt.Tag.tagName)));
   

            //Input
            CreateMap<CreateNovelDto, Novel>()

                //Igore

                .ForMember(d => d.novel_Id, opt => opt.Ignore())
                .ForMember(d => d.created_at, opt => opt.Ignore())
                .ForMember(d => d.updated_at, opt => opt.Ignore())
                .ForMember(d => d.NovelTags, o => o.Ignore())
                .ForMember(d => d.Chapters, o => o.Ignore()); ;
                
            CreateMap<NovelUpdateDto, Novel>()
           
                 //Igore

                 .ForMember(d => d.novel_Id, opt => opt.Ignore())
                 .ForMember(d => d.created_at, opt => opt.Ignore())
                 .ForMember(d => d.updated_at, opt => opt.Ignore())
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

