using AutoMapper;
using EMS_SYSTEM.APPLICATION.Repositories.Interfaces.IUnitOfWork;
using EMS_SYSTEM.DOMAIN.DTO;
using Microsoft.AspNetCore.Mvc;

namespace EMS_SYSTEM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacultyController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private ResponseDTO _responseDTO;
        private readonly IMapper _mapper;
        public FacultyController(IUnitOfWork unitOfWork , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _responseDTO = new ResponseDTO();
            _mapper = mapper;
        }

        [HttpGet("GetFaculty/{Id}")]
        public async Task<IActionResult> GetFacultyById(int Id)
        {
            if (ModelState.IsValid)
            {
                _responseDTO = await _unitOfWork.Faculty.GetFacultyDataByID(Id);
                if (_responseDTO.IsDone)
                {
                    return StatusCode(_responseDTO.StatusCode, _responseDTO.Model);
                }
                return StatusCode(_responseDTO.StatusCode, _responseDTO.Message);
            }
            return BadRequest(ModelState);
        }
    }
}
