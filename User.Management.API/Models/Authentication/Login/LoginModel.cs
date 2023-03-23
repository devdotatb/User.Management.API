﻿using System.ComponentModel.DataAnnotations;

namespace User.Management.API.Models.Authentication.Login
{
    public class LoginModel
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; } 
    }
}
