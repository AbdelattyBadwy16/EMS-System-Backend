﻿using EMS_SYSTEM.DOMAIN.DTO.LogIn;
using EMS_SYSTEM.DOMAIN.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMS_SYSTEM.DOMAIN.Models;
using EMS_SYSTEM.DOMAIN.DTO.PasswordSettings;

namespace EMS_SYSTEM.APPLICATION.Repositories.Interfaces
{
    public interface IAccountService
    {
       public Task<AuthModel> LogIn(LogInDTO model);
       public Task<ApplicationUser> GetCurrentUserAsync();
       public Task<ResponseDTO> ChangePasswordAsync(ChangePasswordDTO model);
    }
}
