﻿using HR.Contant;
using HR.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace HR.Seed
{
    public static class DefaultRole
    {
       // public static readonly HRDbcontext db;
      //  public  static readonly RoleManager<IdentityRole> _roleMaeger;

        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                
                await roleManager.CreateAsync(new IdentityRole(UserRole.Admin.ToString()));

               
            }
        }

    }
}
