using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viralis.Data.Models
{
    public class ClassroomTeacher
    {
        public Guid ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = null!;

        public Guid TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; } = null!;
    }
}
