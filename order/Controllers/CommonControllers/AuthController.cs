﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using order.IRepository.ICommonRepositorys;
using order.IRepository.IAdminRepositorys;
using order.Repository;
using order.Utils;

namespace order.Controllers.CommonControllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepo _authRepo;
        private readonly SecurityUtils _securityUtils;
        private readonly IEmployeeRepo _userRepo;
        public AuthController(IAuthRepo authRepo, SecurityUtils securityUtils, IEmployeeRepo userRepo)
        {
            _authRepo = authRepo;
            _securityUtils = securityUtils;
            _userRepo = userRepo;
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(string user_name, string password)
        {
            try
            {
                var (status, access_token) = await _authRepo.Login(user_name, password, 0);
                if (status)
                {
                    return Ok(new { data = access_token, message = "Successfully logged in " });
                }
                return BadRequest(new { data = string.Empty, message = access_token });

            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("admin-login")]
        public async Task<IActionResult> AdminLogin(string user_name, string password)
        {
            try
            {
                var (status, access_token) = await _authRepo.Login(user_name, password, 1);
                if (status)
                {
                    return Ok(new { data = access_token, message = "Successfully logged in " });
                }
                return BadRequest(new { data = string.Empty, message = access_token });

            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string user_name)
        {
            try
            {

                var (status, massage) = await _authRepo.ForgotPassword(user_name);
                if (status)
                {
                    return Ok(new { data = massage, message = "Successfully send mail " });
                }
                return BadRequest(new { data = string.Empty, message = massage });

            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("varification-otp")]
        public async Task<IActionResult> VarificationOtp(string data, int otp)
        {
            try
            {

                var (status, massage) = await _authRepo.VarificationOtp(data, otp);
                if (status)
                {
                    return Ok(new { data = massage, message = StatusUtils.SUCCESS });
                }
                return BadRequest(new { data = string.Empty, message = massage });

            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut]
        [Route("reset-password")]
        public async Task<IActionResult> RestPassword(string data, string password)
        {
            try
            {

                var (status, message) = await _authRepo.RestPassword(data, password);
                if (status)
                {
                    return Ok(new { data = string.Empty, message });
                }


                return BadRequest(new { data = string.Empty, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}