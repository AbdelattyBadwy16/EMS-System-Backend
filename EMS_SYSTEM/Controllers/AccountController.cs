﻿using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using Azure;
using EMS_SYSTEM.APPLICATION.Repositories.Interfaces;
using EMS_SYSTEM.APPLICATION.Repositories.Services;
using EMS_SYSTEM.DOMAIN.DTO;
using EMS_SYSTEM.DOMAIN.DTO.LogIn;
using EMS_SYSTEM.DOMAIN.DTO.NewFolder;
using EMS_SYSTEM.DOMAIN.DTO.PasswordSettings;
using EMS_SYSTEM.DOMAIN.DTO.Register;
using EMS_SYSTEM.DOMAIN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace EMS_SYSTEM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(IAccountService _accountService , IEmailService _emailService , IHttpContextAccessor httpContextAccessor ,UserManager<ApplicationUser> userManager)
        {
            this._accountService = _accountService;
            this._emailService = _emailService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (ModelState.IsValid)
            {
                var Response = await _accountService.RegisterAsync(dto);
                if (Response.IsDone == true)
                {
                    return Ok(Response);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
            return BadRequest(ModelState);

        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody]LogInDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response= await _accountService.LogIn(model);
                if (Response.IsAuthenticated==true)
                {
                    return Ok(Response);
                }
                return BadRequest(Response);
            }
            return BadRequest(ModelState);
        }

        [HttpPost("RefreshToken")]
        [Authorize] 
        public async Task<IActionResult> RefreshTokenAsync([FromBody]RefreshDTO dto)
        {
            if (ModelState.IsValid)
            {
                var Response = await _accountService.NewRefreshToken(dto.token);
                if (Response.IsAuthenticated == true)
                {
                    return Ok(new { Response });
                }
                return BadRequest(new { Response.Message });
            }
            return BadRequest(ModelState);
        }


      
        [HttpDelete("DeleteUser")]
        [Authorize(Roles = "GlobalAdmin , FacultyAdmin")]
        public async Task<IActionResult> DeleteUserAsync(string NID)
        {
            if (ModelState.IsValid)
            {
                var Response = await _accountService.DeleteUserAsync(NID);
                if (Response.IsDone == true)
                {
                    return StatusCode(Response.StatusCode, Response.Message);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
            return BadRequest(ModelState);

        }


        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO ChangePassword)
        {
            if (ModelState.IsValid)
            {
                var Response = await _accountService.ChangePasswordAsync(ChangePassword);
                if (Response.IsDone == true)
                {
                    return StatusCode(Response.StatusCode, Response.Message);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
            return BadRequest(ModelState);

        }

        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail(string NID)
        {
            if (ModelState.IsValid)
            {
                var Response = await _emailService.HandelSendEmail(NID);
                if (Response.IsDone == true)
                {
                    return Ok(Response.Model);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
            return BadRequest(ModelState);

        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = await _accountService.ResetPassword(model);
                if (Response.IsDone == true)
                {
                    return Ok(Response);
                }
                return StatusCode(Response.StatusCode, Response.Message);
            }
            return BadRequest(ModelState);
        }
    }
}
