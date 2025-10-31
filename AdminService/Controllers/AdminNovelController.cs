using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/publication")]
    [Authorize(Roles = "Admin")]
    public class AdminNovelController: ControllerBase
    {
    }
}
