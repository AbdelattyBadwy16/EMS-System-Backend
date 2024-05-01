using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS_SYSTEM.DOMAIN.DTO.Faculty
{
    public class FacultyDTO
    {
        public string? FacultyName { get; set; }
        public string ? BYlaw {  get; set; }

        public string ? StudyMethod {  get; set; }

        public List<string>? facultyNode { get; set; }

        public List<string> ?facultyPhase { get; set; }


        public List<string>? subjects { get; set; }


    }
}
