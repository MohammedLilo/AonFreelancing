﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs
{
    public class UserDTO
    {
        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(32)]
        public string Username { get; set; }

        [StringLength(32)]
        public string PhoneNumber { get; set; }

        [MinLength(4,ErrorMessage ="Too short password")]
        public string Password { get; set; }


    }

    public class UserOutDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public string Username { get; set; }

    }

    public class UserProfileDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string? Email {  get; set; }
        public string? About {  get; set; }
    
    public UserProfileDTO() { }
    public UserProfileDTO(User user) {
        
            Id = user.Id;
            Name = user.Name;
            PhoneNumber = user.PhoneNumber;
            Email = user.Email;
            About = user.About;
        }

    
    }
}
