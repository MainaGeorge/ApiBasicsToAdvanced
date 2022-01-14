using System;
using System.Collections.Generic;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
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

        [HttpGet("{employeeId:guid}", Name = nameof(GetEmployeeForCompany))]
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

        [HttpPost]
        public IActionResult CreateEmployeeForCompany([FromRoute] Guid companyId, [FromBody] EmployeeForCreationDto employeeDto)
        {
            if (employeeDto is null)
            {
                _logger.LogError("EmployeeForCreationDto from the client was null");
                return BadRequest("EmployeeForCreationDto object is null");
            }

            var company = _repoManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database");
                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employeeDto);

            _repoManager.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            _repoManager.Save();

            var empToReturnDto = _mapper.Map<EmployeeDto>(employeeEntity);
            return CreatedAtRoute(nameof(GetEmployeeForCompany), new { employeeId = employeeEntity.Id, companyId }, empToReturnDto);
        }

        [HttpDelete("{employeeId:guid}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            var company = _repoManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database");
                return NotFound();
            }

            var employeeFromDb = _repoManager.Employee.GetEmployee(companyId, employeeId, false);

            if (employeeFromDb is null)
            {
                _logger.LogInfo($"employee with id {employeeId} does not exist in the database");
                return NotFound();
            }

            _repoManager.Employee.DeleteEmployee(employeeFromDb);
            _repoManager.Save();

            return NoContent();

        }

        [HttpPut("{employeeId:guid}")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid employeeId,
            [FromBody] EmployeeForUpdatingDto employeeForUpdatingDto)
        {
            if (employeeForUpdatingDto is null)
            {
                _logger.LogError("EmployeeForCreationDto from the client was null");
                return BadRequest("EmployeeForCreationDto object is null");
            }

            var company = _repoManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database");
                return NotFound();
            }

            var employeeEntity = _repoManager.Employee.GetEmployee(companyId, employeeId, true);
            if (employeeEntity is null)
            {
                _logger.LogWarning($"Employee with id: {employeeId} doesn't exist in the database");
                return NotFound();
            }

            _mapper.Map(employeeForUpdatingDto, employeeEntity);
            _repoManager.Save();

            return NoContent();
        }

        [HttpPatch("{employeeId:guid}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId, Guid employeeId,
            [FromBody] JsonPatchDocument<EmployeeForUpdatingDto> patchDoc)
        {
            if (patchDoc is null)
            {
                _logger.LogError("EmployeeForCreationDto from the client was null");
                return BadRequest("EmployeeForCreationDto object is null");
            }

            var company = _repoManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database");
                return NotFound();
            }

            var employeeEntity = _repoManager.Employee.GetEmployee(companyId, employeeId, true);
            if (employeeEntity is null)
            {
                _logger.LogWarning($"Employee with id: {employeeId} doesn't exist in the database");
                return NotFound();
            }

            var employeeToPatch = _mapper.Map<EmployeeForUpdatingDto>(employeeEntity);
            patchDoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(ModelState);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeEntity);
            _repoManager.Save();

            return NoContent();
        }
    }
}
