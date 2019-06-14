﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modelo.Domain.Entities;
using Modelo.Domain.Interfaces;
using Modelo.Service.Services;
using Modelo.Service.Validators;
using System;
using System.Linq;

namespace Modelo.Application.Controllers
{
    [Produces("application/json")]
    [Authorize]
    [Route("api/user")]
    public class UserController : Controller
    {
        private IService<UserEntity> _service;

        public UserController(IService<UserEntity> service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserEntity user)
        {
            var userDb = _service.Get().FirstOrDefault(s => s.Email == user.Email && s.Password == AuthorizationService.GenerateHashMd5(user.Password, Environment.GetEnvironmentVariable("SecretKey")));

            if (userDb == null)
                return NotFound();

            return new ObjectResult(AuthorizationService.GenerateToken(userDb.Email, Environment.GetEnvironmentVariable("SecretKey")));
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserEntity user)
        {
            try
            {
                var userDb = _service.Get().FirstOrDefault(s => s.Email == user.Email);

                if (userDb != null)
                    return BadRequest();

                user.Password = AuthorizationService.GenerateHashMd5(user.Password, Environment.GetEnvironmentVariable("SecretKey"));

                _service.Insert<UserValidator>(user);

                return new OkObjectResult(user.Id);
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(ex);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UserEntity item)
        {
            try
            {
                var old = _service.Get(id);

                if (old == null)
                    return NotFound();

                old.Cpf = item.Cpf;
                old.Name = item.Name;
                old.BirthDate = item.BirthDate;
                old.Password = AuthorizationService.GenerateHashMd5(item.Password, Environment.GetEnvironmentVariable("SecretKey"));

                _service.Update<UserValidator>(old);

                return new ObjectResult(item);
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(ex);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _service.Delete(id);

                return new NoContentResult();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        public IActionResult List()
        {
            try
            {
                return new ObjectResult(_service.Get());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                return new ObjectResult(_service.Get(id));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}