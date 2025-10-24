using Microsoft.EntityFrameworkCore;

namespace InteractionService.Data
{
    public class InteracDbContext: DbContext
    {
        public InteracDbContext(DbContextOptions<InteracDbContext> options) : base(options)
        {

        }
    }
}
