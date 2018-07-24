﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VikingQuiz.Api.Models;
using VikingQuiz.Api.Repositories;
using VikingQuiz.Api.Utilities;
using VikingQuiz.Api.ViewModels;

namespace VikingQuiz.Api.Controllers
{
    [Route("api/[controller]")]
    public class SessionController : Controller
    {
        private readonly UserRepo userRepo;
        private readonly IConfiguration _config;

        public SessionController(
            UserRepo userRepo,
            IConfiguration configuration)
        {
            this._config = configuration;
            this.userRepo = userRepo;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateToken([FromBody]LoginViewModel login)
        {
            var user = userRepo.Authenticate(login);
            if (user != null)
            {
                string str = TokenGenerator.BuildToken(user, _config);
                return Ok(new { token = str });
            }
            else
                return Unauthorized();
        }
    }
}