﻿using HR.DTO;
using HR.Helper;
using HR.Models;
using HR.Repository;
using HR.serviec;
using HR.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace HR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize(Roles = "Admin")]
    public class RoleNameController : ControllerBase
    {
        public RoleManager<IdentityRole> roleManager { get; }
        public readonly HRDbcontext db;
        public readonly IRoleNameRepository _roleNameRepository;
        UserManager<ApplictionUsers> _userManager;
        public RoleNameController(UserManager<ApplictionUsers> _userManager, RoleManager<IdentityRole> roleManager, HRDbcontext _dbl, IRoleNameRepository _roleNameRepository)
        {
            this._userManager = _userManager;
            this.roleManager = roleManager;
            this.db = _dbl;
            this._roleNameRepository = _roleNameRepository;
        }
        [HttpGet("GetAllRoles")]
      //[Authorize(Roles = "Permissions.View")]
        public async Task<IActionResult> GetAllRoles()
        {

            List<RoleName> rolesWithPermissions = await _roleNameRepository.GetAllRoles();
                return Ok(rolesWithPermissions);
            
        }
        [HttpPost("CreateRole")]
        //[Authorize(Roles = "Permissions.Create")]
        public async Task<IActionResult> CreateRoleName(RoleDTO roledto)
        {
           
            

                if (ModelState.IsValid)
                {


                    RoleName roleName = new RoleName
                    {
                        GroupName = roledto.Name,
                        Permissions = new List<permission>()
                    };



                    ////
                    foreach (var permissionDto in roledto.Permissions)
                    {
                        // Convert PermissionDTO to Permission entity
                        permission perm = new permission
                        {
                            name = permissionDto.name,
                            create = permissionDto.Create ?? false,
                            delete = permissionDto.Delete ?? false,
                            view = permissionDto.View ?? false,
                            update = permissionDto.Update ?? false
                        };


                        roleName.Permissions.Add(perm);


                    }

                    RoleName createdRole = await _roleNameRepository.RoleNameCreate(roleName);
                    if (createdRole != null)
                    {
                        return Ok(createdRole);
                    }
                    else
                    {
                        return BadRequest("Failed to create role.");
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            
        }
        [HttpGet("GetGroupById/{id:int}")]
      //  [Authorize(Roles = "Permissions.View")]
        public async Task<IActionResult> GetGroupById(int id )
        {
            var group = await _roleNameRepository.GetRoleNameById(id);
            if (group == null)
            {
                return NotFound();
            }
            return Ok(group);
        }
        [HttpPut("UpdateRole/{id}")]
      //  [Authorize(Roles = "Permissions.Update")]
        public async Task<IActionResult> UpdateRole(int id, RoleDTO roledto)
        {
            var existingRole = await _roleNameRepository.GetRoleNameById(id);
            if (existingRole == null)
            {
                return NotFound();
            }
          
            existingRole.GroupName = roledto.Name;
            if (roledto.Permissions != null)
            {
                existingRole.Permissions.Clear();

                foreach (var permissionDto in roledto.Permissions)
                {
                    var permission = new permission
                    {
                        name = permissionDto.name,
                        create = permissionDto.Create,
                        view = permissionDto.View,
                        update = permissionDto.Update,
                        delete = permissionDto.Delete
                    };
                    existingRole.Permissions.Add(permission);
                }
            }

            await _roleNameRepository.RoleNameUpdate(existingRole);
            // Update the roles of users associated with the updated role name
            var users = db.Users.Include(u => u.RoleNames).Where(u => u.roleId == existingRole.Id).ToList();
            foreach (var user in users)
            {
             
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    //RoleName roleName = await _roleNameRepository.GetRoleNameById(user.roleId);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles.ToArray());
                    List<string> nameoftableperm = new List<string>();
                    // Add the user to the updated role
                    foreach (var permission in existingRole.Permissions)
                    {
                        var p = PermissionGeneret.GeneratePermissionsList(permission.name, permission.create, permission.update, permission.delete, permission.view);
                        foreach (var per in p)
                        {
                            if (!await roleManager.RoleExistsAsync(per))
                                await roleManager.CreateAsync(new IdentityRole(per));
                            nameoftableperm.Add(per);
                        }
                    }
                    List<string> roles = nameoftableperm;
                    foreach(var role in roles)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                }
            
            return Ok(existingRole);
        }
        [HttpDelete("DeleteRole/{id}")]
       // [Authorize(Roles = "Permissions.Delete")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var roleToDelete = await _roleNameRepository.GetRoleNameById(id);
            if (roleToDelete == null)
            {
                return NotFound();
            }
            await _roleNameRepository.RoleNameDelete(roleToDelete);
            return Ok();
        }
        [HttpGet("GetByName/{name:alpha}")]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            var ExistinRole= await _roleNameRepository.GetRoleNameByName(name);
            if (ExistinRole != null) return BadRequest("role name is already exist");
            return Ok();
        }
       
    }





    }

