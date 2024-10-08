﻿using EMS_SYSTEM.APPLICATION.Repositories.Interfaces;
using EMS_SYSTEM.APPLICATION.Repositories.Interfaces.IUnitOfWork;
using EMS_SYSTEM.APPLICATION.Repositories.Services;
using EMS_SYSTEM.DOMAIN.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMS_SYSTEM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalAdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGlobalService _globalService;
        private ResponseDTO _responseDTO;

        public GlobalAdminController(IUnitOfWork unitOfWork,IGlobalService _globalService)
        {
            _unitOfWork = unitOfWork;
            this._globalService = _globalService;
        }
        [HttpGet("GetAllFaculties")]
        public async Task<IActionResult> GetAllFaculties()
        {
            var Response = await _globalService.GetAllFaculties();
            if(Response.IsDone) {
                return StatusCode(Response.StatusCode, Response.Model);           
            }
            return BadRequest(ModelState);
        }
        [HttpGet("GetFacultyByName")]
        public async Task<IActionResult> GetFacultyByName(string FacultyName)
        {
            if (ModelState.IsValid)
            {
                var Response = await _globalService.GetFacultyByName(FacultyName);
                if ( Response.IsDone)
                {
                    return StatusCode( Response.StatusCode,  Response.Model);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
                return BadRequest(ModelState);
       }

        [HttpGet("CommitteesCount")]
        public async Task<IActionResult> GetFacultiesWithCommitteeCount()
        {
            if (ModelState.IsValid)
            {  
                var Response = await _globalService.GetFacultiesWithCommitteeCount();
                if (Response.IsDone)
                {
                    return StatusCode(Response.StatusCode, Response.Model);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
            return BadRequest(ModelState);
        }

        [HttpGet("CommitteesTodayCount")]

        public async Task<IActionResult> GetFacultiesWithCommitteeToday()
        {
            if (ModelState.IsValid)
            {
                var Response = await _globalService.GetFacultiesWithCommitteeToday();
                if (Response.IsDone)
                {
                    return StatusCode(Response.StatusCode, Response.Model);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
            return BadRequest(ModelState);
        }

        [HttpGet("FacultyByDate")]
        public async Task<IActionResult> GetFacultiesByDate(DateTime date)
        {
            if(ModelState.IsValid)
            {
                var response=await _globalService.GetFacultiesByDate(date);
                if (response.IsDone)
                    return StatusCode(response.StatusCode, response.Model);
                return StatusCode(response.StatusCode, response.Message);
            }
            return BadRequest(ModelState);
        }


        [HttpGet("StudentCountInActiveCommitteesToday")]

        public async Task<IActionResult> GetStudentCountInActiveCommittees()
        {
            if (ModelState.IsValid)
            {
                var Response = await _globalService.GetStudentCountInActiveCommitteesToday();
                if (Response.IsDone)
                {
                    return StatusCode(Response.StatusCode, Response.Model);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
            return BadRequest(ModelState);
        }
        [HttpGet("FacultyWithMostActiveCommitteesToday")]

        public async Task<IActionResult> GetFacultyWithMostActiveCommittees()
        {
            if (ModelState.IsValid)
            {
                var Response = await _globalService.GetFacultyWithMostActiveCommitteesToday();
                if (Response.IsDone)
                {
                    return StatusCode(Response.StatusCode, Response.Model);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
            return BadRequest(ModelState);
        }
    }
}
