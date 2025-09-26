using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NovelService.Data;
using System;

namespace PublicationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NovelController : ControllerBase
    {
        private readonly NovelDbContext _context;
        private readonly IMapper _mapper;

        public NovelService(NovelDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


    }


}
