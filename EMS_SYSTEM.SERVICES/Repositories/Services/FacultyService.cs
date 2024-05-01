using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMS_SYSTEM.APPLICATION.Repositories.Interfaces;
using EMS_SYSTEM.APPLICATION.Repositories.Interfaces.IUnitOfWork;
using EMS_SYSTEM.DOMAIN.DTO;
using EMS_SYSTEM.DOMAIN.DTO.Faculty;
using EMS_SYSTEM.DOMAIN.DTO.Student;
using Microsoft.EntityFrameworkCore;

namespace EMS_SYSTEM.APPLICATION.Repositories.Services
{
    public class FacultyService : GenericRepository<Faculty>, IFacultyService
    {

        private readonly IUnitOfWork _unitOfWork;
        public FacultyService(UnvcenteralDataBaseContext Db) : base(Db)
        {
        }

        public async Task<ResponseDTO> GetFacultyDataByID(int Id)
        {
            var departmentnames = _context.FacultyNodes.Where(d => d.FacultyId == Id).Select(d => d.Name).ToList();
            var facultyphases=_context.FacultyPhases.Where(p=>p.FacultyId==Id).Select(p => p.Name).ToList();
            var faculty = await _context.Faculties
                .Where(f => f.Id == Id).SelectMany
                (faculty => faculty.Bylaws.Select(Bylaw => new FacultyDTO

                {
                    FacultyName = faculty.FacultyName,
                    BYlaw=Bylaw.Name,
                    StudyMethod=Bylaw.CodeStudyMethod!.Name,
                    facultyNode=departmentnames!,
                    facultyPhase=facultyphases!,

                }
                )).FirstOrDefaultAsync();
            if (faculty != null)
            {
                return new ResponseDTO
                {
                    Model = faculty,
                    StatusCode = 200,
                    IsDone = true
                };
            }
            else
            {
                return new ResponseDTO
                {
                    StatusCode = 400,
                    IsDone = false,
                    Message = "We Couldn't Find The Faculty"
                };
            }
        }
    }
}
