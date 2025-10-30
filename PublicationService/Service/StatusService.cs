using Microsoft.EntityFrameworkCore;
using NovelService.Data;
using NovelService.Models;
using NovelService.Service.Interfaces;
using Shareds.DTOs.Category;
using Shareds.DTOs.NovelStatus;

namespace NovelService.Service
{
    public class StatusService : IStatusService
    {
        private readonly NovelDbContext _context;
        private readonly ILogger<StatusService> _logger;

        public StatusService(NovelDbContext context, ILogger<StatusService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private NovelStatusDto MapToDto(NovelStatus status)
        {
            return new NovelStatusDto
            {
                statusId = status.statusId,
                statusName = status.statusName,
            };
        }

        public async Task<IEnumerable<NovelStatusDto>> GetAllStatusesAsync()
        {
            var statuses = await _context.Novel_Statuses
                .AsNoTracking()
                .ToListAsync();

    
            return statuses.Select(MapToDto);
        }


        public async Task<NovelStatusDto?> GetStatusByIdAsync(int id)
        {
            var status = await _context.Novel_Statuses.AsNoTracking().FirstOrDefaultAsync(s => s.statusId == id);

            if (status == null)
            {
                return null;
            }

            return MapToDto(status);
        }

        public async Task<NovelStatusDto> CreateStatusAsync(StatusCreateDto dto)
        {
            bool nameExists  = await _context.Novel_Statuses.AnyAsync(s => s.statusName == dto.statusName);
            if (nameExists)
            {
                throw new InvalidOperationException("Status alreary exsists");
            }

            var newStatus = new NovelStatus
            {
                statusName = dto.statusName
            };

            _context.Novel_Statuses.Add(newStatus);
            await _context.SaveChangesAsync();

            return MapToDto(newStatus);
        }

        public async Task<bool> UpdateStatusAsync(int id, StatusUpdateDto dto)
        {
            var status = await _context.Novel_Statuses.FindAsync(id);

            if (status == null)
            {
                return false;
            }

            if (status.statusName != dto.statusName)
            {
                bool nameExists = await _context.Novel_Statuses.AnyAsync(s => s.statusName == dto.statusName && s.statusId != id);
                if (nameExists)
                {
                    throw new InvalidOperationException("Status alreary exsists");
                }
            }

            status.statusName = dto.statusName;

            _context.Entry(status).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrency exception updating status {StatusId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteStatusAsync(int id)
        {
            var status = await _context.Novel_Statuses.FindAsync(id);
            if (status == null)
            {
                return false;
            }

            bool isInUse = await _context.Novel_Series.AnyAsync(s => s.status_id == id);
            if (isInUse)
            {
                throw new InvalidOperationException("Status in use");
            }

            _context.Novel_Statuses.Remove(status);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
