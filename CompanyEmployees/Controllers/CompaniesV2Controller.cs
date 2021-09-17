﻿using Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [ApiVersion("2.0", Deprecated = true)]
    [Route("api/companies")]
    [ApiController]
    public class CompaniesV2Controller : ControllerBase
    {
        private readonly IRepositoryManager _repository;

        public CompaniesV2Controller(IRepositoryManager repository)
        {
            _repository = repository;
        }

        [MapToApiVersion("2.0")]
        [HttpGet(Name = "GetCompanies")]
        public async Task<IActionResult> GetCompanies()
        {
            var compaies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);

            return Ok(compaies);
        }
    }
}
