﻿using System;
using System.Collections.Generic;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId:guid}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repoManager;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public EmployeesController(IRepositoryManager repoManager, ILoggerManager logger, IMapper mapper)
        {
            _repoManager = repoManager;
            _logger = logger;
            _mapper = mapper;
        }

        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var company = _repoManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database");
                return NotFound();
            }

            var employeesFromDb = _repoManager.Employee.GetEmployees(companyId, false);
            return Ok(_mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb));
        }

        [HttpGet("{employeeId:guid}")]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            var company = _repoManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database");
                return NotFound();
            }

            var employeeFromDb = _repoManager.Employee.GetEmployee(companyId, employeeId, false);

            if (employeeFromDb != null) return Ok(_mapper.Map<EmployeeDto>(employeeFromDb));

            _logger.LogInfo($"employee with id {employeeId} does not exist in the database");
            return NotFound();

        }
    }
}