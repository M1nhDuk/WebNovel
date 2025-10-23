using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shareds.DTOs.AuthService
{
    public class UserDeleteEvent
    {
        public Guid UserId { get; set; }
    }
}
