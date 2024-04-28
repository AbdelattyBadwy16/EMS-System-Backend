﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EMS_SYSTEM.APPLICATION.Repositories.Interfaces;
using EMS_SYSTEM.DOMAIN.DTO;
using EMS_SYSTEM.DOMAIN.DTO.LogIn;
using EMS_SYSTEM.DOMAIN.DTO.PasswordSettings;
using EMS_SYSTEM.DOMAIN.DTO.Register;
using EMS_SYSTEM.DOMAIN.Models;
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
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(IAccountService _accountService , IHttpContextAccessor httpContextAccessor ,UserManager<ApplicationUser> userManager)
        {
            this._accountService=_accountService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }
        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody]LogInDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response= await _accountService.LogIn(model);
                if (Response.IsAuthenticated==true)
                {
                    return Ok(new {Response.Message,Response.Token , Response.RefreshToken , Response.RefreshTokenExpiration});
                }
                return BadRequest(new {Response.Message});
            }
            return BadRequest(ModelState);
        }

        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshTokenAsync(string token)
        {
            if (ModelState.IsValid)
            {
                var Response = await _accountService.NewRefreshToken(token);
                if (Response.IsAuthenticated == true)
                {
                    return Ok(new { Response.Message, Response.Token });
                }
                return BadRequest(new { Response.Message });
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
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]RegisterDto dto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);  
            var user = new ApplicationUser
            {
                NID = dto.NID, 
            };
            var result = await userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                return Ok(new UserDto
                {
                    NID = dto.NID,
                    Password = dto.Password,

                });
              
            }
            else
            {
                //return BadRequest("Failed to register user.");
                
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }
            
        }
    }
}
