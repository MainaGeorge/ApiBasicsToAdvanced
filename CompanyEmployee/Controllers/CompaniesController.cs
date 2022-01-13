using System;
using System.Collections.Generic;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repoManager;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;

        public CompaniesController(IRepositoryManager repoManager, IMapper mapper, ILoggerManager logger)
        {
            _repoManager = repoManager;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetCompanies()
        {
            var companies = _repoManager
                .Company
                .GetAllCompanies(trackChanges: false);
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesDto);
        }

        [HttpGet("{companyId}")]
        public IActionResult GetCompany(Guid companyId)
        {
            var company = _repoManager.Company.GetCompany(companyId, false);
            if (company is not null) return Ok(_mapper.Map<CompanyDto>(company));

            _logger.LogInfo($"company with id {companyId} doesn't exist in the database");
            return NotFound();
        }
    }
}
