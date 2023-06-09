﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using User.Management.API.Models;
using User.Management.API.Models.Authentication.Login;
using User.Management.API.Models.Authentication.SignUp;
using User.Management.API.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace User.Management.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;
        public AuthenticationController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager , IConfiguration configuration
            , JwtService jwtService)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._configuration = configuration;
            this._jwtService = jwtService;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody]RegisterUser registerUser,string role)
        {
            //Check User Exist
            if(registerUser == null)
            {
                throw new ArgumentNullException(nameof(registerUser));
            }

            //Add the User in the database
            IdentityUser user = new IdentityUser()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.UserName,
            };
            if (await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.CreateAsync(user, registerUser.Password);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new Response { Status = "Error", Message = string.Join(",", result.Errors) });
                }

                await _userManager.AddToRoleAsync(user, role);
                return StatusCode(StatusCodes.Status201Created,
                    new Response { Status = "Success", Message = "User Create Successfully" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "This Role doesnt exist" });
            }


            //Assign a role
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Username);

            if(user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                };
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach(var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var jwtToken = _jwtService.GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo
                });

            }


            return Unauthorized();
        }

        [HttpGet("test")]
        [Authorize]
        public string test()
        {
            return "1234";
        }

        [HttpGet("randomnumber")]
        [Authorize]
        public IActionResult GetRandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(1111111, 9999999);
            return Ok(randomNumber.ToString());
        }

    }
}
