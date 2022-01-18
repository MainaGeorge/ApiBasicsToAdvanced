using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Utility;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestParameters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId:guid}/employees")]
    [ApiVersion("1.0")]
    [ApiController]
    [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repoManager;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        private readonly EmployeeLinks _employeeLinks;

        public EmployeesController(IRepositoryManager repoManager, ILoggerManager logger, IMapper mapper,
            IDataShaper<EmployeeDto> dataShaper, EmployeeLinks employeeLinks)
        {
            _repoManager = repoManager;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
            _employeeLinks = employeeLinks;
        }

        [HttpGet]
        [HttpHead]
        [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId,
            [FromQuery] EmployeeRequestParameters requestParameters)
        {
            if (!requestParameters.IsValidAgeRage)
                return BadRequest($"{requestParameters.MaxAge} can't be less than {requestParameters.MinAge}");

            var _ = HttpContext.Items["company"] as Company;
            var employeesFromDb = await _repoManager.Employee.GetEmployeesAsync(companyId, requestParameters, false);
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeesFromDb.MetaData));

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
            var shapedEmployees = _dataShaper.ShapeData(employeesDto, requestParameters.Fields).Select(e => e.Entity);

            return Ok(shapedEmployees);
        }

        [HttpGet("{employeeId:guid}", Name = nameof(GetEmployeeForCompany))]
        [ServiceFilter(typeof(ValidateEmployeeExistsAttribute))]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid employeeId, [FromQuery] EmployeeRequestParameters param)
        {
            var _ = HttpContext.Items["company"] as Company;
            var employeeFromDb = HttpContext.Items["employee"] as Employee;

            return Ok(_dataShaper.ShapeData(_mapper.Map<EmployeeDto>(employeeFromDb), param.Fields));

        }

        [HttpPost]
        [ServiceFilter(typeof(ValidateModelState))]
        public async Task<IActionResult> CreateEmployeeForCompany([FromRoute] Guid companyId, [FromBody] EmployeeForCreationDto employeeDto)
        {
            var _ = HttpContext.Items["company"] as Company;
            var employeeEntity = _mapper.Map<Employee>(employeeDto);

            _repoManager.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repoManager.SaveAsync();

            var empToReturnDto = _mapper.Map<EmployeeDto>(employeeEntity);
            return CreatedAtRoute(nameof(GetEmployeeForCompany), new { employeeId = employeeEntity.Id, companyId }, empToReturnDto);
        }

        [HttpDelete("{employeeId:guid}")]
        [ServiceFilter(typeof(ValidateEmployeeExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            var _ = HttpContext.Items["company"] as Company;
            var employeeFromDb = HttpContext.Items["employee"] as Employee;

            _repoManager.Employee.DeleteEmployee(employeeFromDb);
            await _repoManager.SaveAsync();

            return NoContent();

        }

        [HttpPut("{employeeId:guid}")]
        [ServiceFilter(typeof(ValidateModelState))]
        [ServiceFilter(typeof(ValidateEmployeeExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid employeeId,
            [FromBody] EmployeeForUpdatingDto employeeForUpdatingDto)
        {

            var _ = HttpContext.Items["company"] as Company;
            var employeeEntity = HttpContext.Items["employee"] as Employee;
            _mapper.Map(employeeForUpdatingDto, employeeEntity);
            await _repoManager.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{employeeId:guid}")]
        [ServiceFilter(typeof(ValidateModelState))]
        [ServiceFilter(typeof(ValidateEmployeeExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid employeeId,
            [FromBody] JsonPatchDocument<EmployeeForUpdatingDto> patchDoc)
        {
            var _ = HttpContext.Items["company"] as Company;
            var employeeEntity = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeForUpdatingDto>(employeeEntity);
            patchDoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(ModelState);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeEntity);
            await _repoManager.SaveAsync();

            return NoContent();
        }
    }
}
