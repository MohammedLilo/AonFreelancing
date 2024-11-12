﻿using AonFreelancing.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{

    [Table("Clients")]
    public class Client : User
    {
        public string CompanyName { get; set; }


        // Has many projects, 1-m
        public List<Project>? Projects { get; set; }

        public Client() { }
        public Client(UserRegistrationRequest registrationRequest)
        {
            Email = registrationRequest.Email;
            Name = registrationRequest.Name;
            PhoneNumber = registrationRequest.PhoneNumber;
            CompanyName = registrationRequest.CompanyName;
        }

        //public override void DisplayProfile()
        //{
        //    Console.WriteLine($"Client display profile, Company: {CompanyName}");
        //}
    }
}
