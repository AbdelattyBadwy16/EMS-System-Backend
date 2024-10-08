﻿using EMS_SYSTEM.APPLICATION.Repositories.Interfaces;
using EMS_SYSTEM.APPLICATION.Repositories.Interfaces.IUnitOfWork;
using EMS_SYSTEM.DOMAIN.DTO;
using EMS_SYSTEM.DOMAIN.DTO.Committee;
using EMS_SYSTEM.DOMAIN.Enums;
using EMS_SYSTEM.DOMAIN.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EMS_SYSTEM.APPLICATION.Repositories.Services
{
    public class CommitteeService : GenericRepository<Committee>, ICommitteeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public CommitteeService(UnvcenteralDataBaseContext Db, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : base(Db)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ResponseDTO> Distributions(int observerId, List<int> noticers, CommitteeDTO model)
        {
            var committee = new Committee
            {
                Name = model.Name,
                Date = model.Date.Date,
                Interval = model.Interval,
                From = model.From,
                To = model.To,
                Place = model.Place,
                SubjectName = model.SubjectsName,
                StudyMethod = model.StudyMethod,
                Status = model.Status,
                Day = model.Day,
                ByLaw = model.ByLaw,
                FacultyNode = model.FacultyNode,
                FacultyPhase = model.FacultyPhase,
                PlaceID = model.PlaceID
            };

            var isSubjectIdExist = await _unitOfWork.Subject.IsExistAsync(s => s.Id == model.SubjectID);
            if (isSubjectIdExist == null)
            {
                return new ResponseDTO
                {
                    Message = "Subject ID does not exist",
                    IsDone = false,
                    StatusCode = 400
                };
            }

            await _unitOfWork.Committees.AddAsync(committee);
            await _unitOfWork.SaveAsync();

            await _unitOfWork.SubjectCommittees.AddAsync(new SubjectCommittee
            {
                SubjectId = model.SubjectID,
                CommitteeId = committee.Id
            });
            await _unitOfWork.SaveAsync();

            var allStudents = await _context.Students
                .Join(_context.StudentSemesters, s => s.Id, ss => ss.StuentId, (s, ss) => new { Student = s, StudentSemester = ss })
                .Join(_context.StudentSemesterSubjects, sss => sss.StudentSemester.Id, s => s.StudentSemestersId, (sss, s) => new { sss.Student, StudentSemester = s })
                .Join(_context.Subjects, ss => ss.StudentSemester.SubjectId, su => su.Id, (ss, su) => new { ss.Student, Subject = su })
                .Where(ss => ss.Subject.Id == model.SubjectID)
                .OrderBy(s => s.Student.Name)
                .ToListAsync();

            var existingStudent = await _context.StudentsCommittees
                .Select(s => s.StudentID)
                .ToListAsync();

            var availableStudents = allStudents
                .Where(s => !existingStudent.Contains(s.Student.Id))
                .Take(model.StudentNumber)
                .ToList();

            if (model.StudentNumber > availableStudents.Count || availableStudents.Count == 0)
            {
                _context.Committees.Remove(committee);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    Message = $"{availableStudents.Count}",
                    IsDone = false,
                    StatusCode = 400,
                };
            }
            var studentCommittees = availableStudents.Select(student => new StudentsCommittees
            {
                CommitteeID = committee.Id,
                StudentID = student.Student.Id
            }).ToList();

            await _context.StudentsCommittees.AddRangeAsync(studentCommittees);
            await _context.SaveChangesAsync();

            var staffCommittees = noticers.Select(observer => new StaffCommittees
            {
                CommitteeID = committee.Id,
                StaffID = observer
            }).ToList();

            staffCommittees.Add(new StaffCommittees
            {
                CommitteeID = committee.Id,
                StaffID = observerId
            });

            await _context.AddRangeAsync(staffCommittees);
            await _context.SaveChangesAsync();

            return new ResponseDTO
            {
                Message = "Committee Created Successfully",
                IsDone = true,
                StatusCode = 200
            };
        }


        public async Task<ResponseDTO> GetCommitteesSchedule(int Id)
        {
            var phase = _context.Faculties
                .Where(f => f.Id == Id)
                .Join(_context.FacultyPhases, f => f.Id, p => p.FacultyId, (f, p) => new { FacultyPhase = p })    
                .Select(f=>f.FacultyPhase.Id)
                .ToList();
  
            CommiteLevels levels = new CommiteLevels();

            for (var i=0; i<phase.Count; i++)
            {
                var sub = _context.FacultyPhases
                    .Where(p => p.Id == phase[i])
                    .Join(_context.FacultyHieryicals, p => p.Id, h => h.PhaseId, (p, h) => new { h.Id })
                    .Join(_context.Subjects, p => p.Id, s => s.FacultyHieryricalId, (h, s) => new { s.Id })
                    .Join(_context.SubjectCommittees, s => s.Id, sc => sc.SubjectId, (s, sc) => new { sc.CommitteeId })
                    .Join(_context.Committees, sc => sc.CommitteeId, c => c.Id, (sc, c) => new { c })
                    .OrderBy(p=>p.c.Date).ThenBy(p=>p.c.From)
                    .ToList();
                switch(i+1)
                {
                    case 1:
                        levels.level1 = sub; break;
                    case 2:
                        levels.level2 = sub; break;
                    case 3:
                        levels.level3 = sub; break;
                    case 4:
                        levels.level4 = sub; break;
                    case 5:
                        levels.level5 = sub; break;
                    case 6:
                        levels.level6 = sub; break;
                    case 7:
                        levels.level7 = sub; break;
                   
                }

            }


            levels.len = phase.Count;

            return new ResponseDTO
            {
                Model = levels,
                StatusCode = 200,
                IsDone = true
            };
            
        }

        public async Task<ResponseDTO> GetCommitteeDetails(int id)
        {
            var staff = await _context.Committees.Where(c => c.Id == id)
                .Select(c => new
                {
                    Invigilators = c.StaffCommittees.Where(st => st.Staff != null)
                                               .Select(st => st.Staff).ToList(),
                    Observers = c.StaffCommittees.Where(st => st.Staff != null)
                                               .Select(st => st.Staff).ToList()
                }).FirstOrDefaultAsync();
            List<string> newInv = new List<string>();
            List<string> newOp = new List<string>();
            foreach(var item in staff.Invigilators) {
                var user = await _userManager.FindByNameAsync(item.NID);
                var Roles = await _userManager.GetRolesAsync(user);
                if (Roles[0] == "Invigilators") newInv.Add(item.Name);
            }
            foreach (var item in staff.Observers)
            {
                var user = await _userManager.FindByNameAsync(item.NID);
                var Roles = await _userManager.GetRolesAsync(user);
                if (Roles[0] == "Observers") newOp.Add(item.Name);
            }
            var committee = await _context.Committees.Where(c => c.Id == id)
                .Select(c => new
                {
                    Name = c.Name,
                    SubjectsName = c.SubjectName,
                    FacultyNode = c.FacultyNode,
                    FacultyPhase = c.FacultyPhase,
                    Day = c.Day,
                    Date = c.Date,
                    Interval = c.Interval,
                    From = c.From,
                    To = c.To,
                    Status = c.Status,
                    Place = c.Place,
                    StudentNumber = c.StudentsCommittees.Count(),
                    Invigilators = newInv,
                    Observers = newOp
                }).FirstOrDefaultAsync();
            if (committee != null)
            {
                return new ResponseDTO
                {
                    StatusCode = 200,
                    IsDone = true,
                    Model = committee
                };
            }

            return new ResponseDTO
            {
                StatusCode = 400,
                IsDone = false,
                Message="There is no committee with this Id."
            };
        }


        public async Task<ResponseDTO> DeleteCommittee(int CommiteeID)
        {
            var Commitee = await _context.Committees.FirstOrDefaultAsync(c => c.Id == CommiteeID);
            if (Commitee == null)
            {
                return new ResponseDTO
                {
                    IsDone = false,
                    Message = $"There is No Committee was found with ID {CommiteeID}",
                    StatusCode = 404,
                    Model = null,
                };
            }

            _context.Committees.Remove(Commitee);
            await _unitOfWork.SaveAsync();

            return new ResponseDTO
            {
                IsDone = true,
                Message = $"Commitee with ID {CommiteeID} was Deleted",
                StatusCode = 200,
                Model = Commitee,
            };
        }

        public async Task<ResponseDTO> DeleteAllFacultyCommitee(int FacultyID)
        {
            var isfacultyexist = await _unitOfWork.Faculty.IsExistAsync(s => s.Id == FacultyID);
            // if facultyid is wrong
            if (isfacultyexist is null)
            {
                return new ResponseDTO
                {
                    Message = "There is No Faculty with this ID",
                    IsDone = false,
                    Model = null,
                    StatusCode = 404
                };
            }


            var Committees = await _context.Committees
               .Join(_context.SubjectCommittees, c => c.Id, sc => sc.CommitteeId, (c, sc) => new { Committee = c, SubjectCommittee = sc })
               .Join(_context.Subjects, cs => cs.SubjectCommittee.SubjectId, s => s.Id, (cs, s) => new { cs.Committee, Subject = s })
               .Join(_context.FacultyNodes, csc => csc.Subject.FacultyNodeId, fn => fn.FacultyNodeId, (csc, fn) => new { csc.Committee, FacultyNode = fn })
               .Join(_context.Faculties, cscfn => cscfn.FacultyNode.FacultyId, f => f.Id, (cscfn, f) => new { cscfn.Committee, Faculty = f })
               .Where(c => c.Faculty.Id == FacultyID)
               .Select(c => c.Committee)
               .ToListAsync();
            // no committees found
            if (Committees.Count == 0)
            {

                return new ResponseDTO
                {
                    IsDone = false,
                    Message = $"There is No Committees in the faculty with id {FacultyID} to Delete",
                    StatusCode = 404
                };
            }

            foreach (var committee in Committees)
            {
                _context.Committees.Remove(committee);
            }
            await _unitOfWork.SaveAsync();

            return new ResponseDTO
            {
                IsDone = true,
                Message = $"All Committees For Faculty {FacultyID} Was Deleted Successfully",
                StatusCode = 200,
                Model = Committees.ToList(),
            };
        }
        public async Task<ResponseDTO> FilterFacultyCommittees(int facultyID, int level, string committeeName , string subjectName)
        {
            var CommitteesDetails = _context.Committees
               .Join(_context.SubjectCommittees, c => c.Id, sc => sc.CommitteeId, (c, sc) => new { Committee = c, SubjectCommittee = sc })
               .Join(_context.Subjects, sc => sc.SubjectCommittee.SubjectId, s => s.Id, (sc, s) => new { sc, Subject = s })
               .Join(_context.FacultyHieryicals, s => s.Subject.FacultyHieryricalId, fh => fh.Id, (s, fh) => new { s, FacultyHieryical = fh })
               .Join(_context.FacultyPhases, fh => fh.FacultyHieryical.PhaseId, fp => fp.Id, (fh, fp) => new { fh, FacultyPhase = fp });
            if (facultyID == 0)
            {
                return new ResponseDTO()
                {
                    IsDone = false,
                    StatusCode = 400,
                    Model = null,
                    Message = "Faculty Id is Required!"
                };
            }
            else
            {
                if (facultyID != 0)
                {
                    CommitteesDetails = CommitteesDetails.Where(f => f.FacultyPhase.FacultyId == facultyID);
                }

                if (level != 0)
                {
                    CommitteesDetails = CommitteesDetails.Where(f => f.FacultyPhase.Id == level);
                }

                if (!string.IsNullOrEmpty(subjectName))
                {
                    CommitteesDetails = CommitteesDetails.Where(f => f.fh.s.sc.Committee.SubjectName.Contains(subjectName));
                }

                if (!string.IsNullOrEmpty(committeeName))
                {
                    CommitteesDetails = CommitteesDetails.Where(f => f.fh.s.sc.Committee.Name.Contains(committeeName));
                }

                return new ResponseDTO
                {
                    IsDone = true,
                    Model = CommitteesDetails.OrderBy(s => s.fh.s.sc.Committee.Date).ThenBy(s => s.fh.s.sc.Committee.From).Select(s => s.fh.s.sc.Committee)  ,
                    StatusCode = 200,
                    Message = "Success Response"
                };
            }
        }

        public async Task<ResponseDTO> UpdateCommitee(int committeeID, CommitteeDTO model)
        {
            if (model is not null)
            {
                var existingCommittee = await _unitOfWork.Committees.IsExistAsync(c => c.Id == committeeID);
                // if committee found
                if (existingCommittee != null)
                {
                    // if the user doen't enter value it take the already exist
                    if (model.Name != null)
                        existingCommittee.Name = model.Name;

                    if (model.Date != default(DateTime))
                        existingCommittee.Date = model.Date.Date;
                    if (model.Interval != null)
                        existingCommittee.Interval = model.Interval;

                    if (model.From != null)
                        existingCommittee.From = model.From;

                    if (model.To != null)
                        existingCommittee.To = model.To;


                    if (model.Place != null)
                        existingCommittee.Place = model.Place;

                    if (model.SubjectsName != null)
                        existingCommittee.SubjectName = model.SubjectsName;

                    if (model.StudyMethod != null)
                        existingCommittee.StudyMethod = model.StudyMethod;


                    if (model.Status != null)
                        existingCommittee.Status = model.Status;

                    if (model.Day != default(Days))
                        existingCommittee.Day = model.Day;

                    if (model.ByLaw != null)
                        existingCommittee.ByLaw = model.ByLaw;

                    if (model.FacultyNode != null)
                        existingCommittee.FacultyNode = model.FacultyNode;

                    if (model.FacultyPhase != null)
                        existingCommittee.FacultyPhase = model.FacultyPhase;


                    await _unitOfWork.Committees.UpdateAsync(existingCommittee, committeeID);
                    await _unitOfWork.SaveAsync();

                    return new ResponseDTO
                    {
                        Message = "Committee Updated Successfully",
                        IsDone = true,
                        Model = existingCommittee,
                        StatusCode = 200
                    };
                }
                else
                {
                    return new ResponseDTO
                    {
                        Message = "Committee not found",
                        IsDone = false,
                        Model = null,
                        StatusCode = 404
                    };
                }
            }
            else
            {
                return new ResponseDTO
                {
                    Message = "Invalid Data in Model",
                    IsDone = false,
                    Model = null,
                    StatusCode = 400
                };
            }
        }

        public async Task<ResponseDTO> GetObserversInFaculty(int facultyId)
        {
            var observers = await _userManager.GetUsersInRoleAsync("Observers");

            if (observers.Count == 0)
            {
                return new ResponseDTO
                {
                    StatusCode = 404,
                    IsDone = false,
                    Message = "No observers found for the specified faculty."
                };
            }

            var observerDTOs = observers.Select(user => new ObserverDTO
            {
                Id = _context.Staff.Where(s => s.NID == user.NID && s.FacultyId ==facultyId).Select(s => s.Id).FirstOrDefault(),
                Name = _context.Staff.Where(s => s.NID == user.NID && s.FacultyId ==facultyId).Select(s => s.Name).FirstOrDefault()
            }).ToList();

            observerDTOs = observerDTOs.Where(s => s.Id != 0).ToList();

            return new ResponseDTO
            {
                Model = observerDTOs,
                StatusCode = 200,
                IsDone = true
            };
        }
        public async Task<ResponseDTO> GetInvigilatorsInFaculty(int facultyId)
        {
            var Invigilators = await _userManager.GetUsersInRoleAsync("Invigilators");
            
                 
            if (Invigilators.Count == 0)
            {
                return new ResponseDTO
                {
                    StatusCode = 404,
                    IsDone = false,
                    Message = "No Invigilators found for the specified faculty."
                };
            }

            var InvigilatorDTOs = Invigilators.Select(user => new InvigilatorDTO
            {
                Id = _context.Staff.Where(s => s.NID == user.NID && s.FacultyId == facultyId).Select(s => s.Id).FirstOrDefault(),
                Name = _context.Staff.Where(s => s.NID == user.NID && s.FacultyId == facultyId).Select(s => s.Name).FirstOrDefault()
            }).ToList();


            InvigilatorDTOs = InvigilatorDTOs.Where(s => s.Id != 0).ToList();
            return new ResponseDTO
            {
                Model = InvigilatorDTOs,
                StatusCode = 200,
                IsDone = true
            };
        }


        public async Task<ResponseDTO> GetCommiteStudents(int comid)
        {
            var data = _context.StudentsCommittees.Where(item => item.CommitteeID == comid).ToList();
            List<string> Commitestudents = new List<string>();
            foreach(var item in data)
            {
                Student user = _context.Students.FirstOrDefault(s => s.Id == item.StudentID);
                Commitestudents.Add(user.Name);
            }
            return new ResponseDTO
            {
                Model = Commitestudents,
                StatusCode = 200,
                IsDone = true
            };
        }

    }

}
