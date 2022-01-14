using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
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

        [HttpGet("{companyId:guid}", Name = nameof(GetCompany))]
        public IActionResult GetCompany(Guid companyId)
        {
            var company = _repoManager.Company.GetCompany(companyId, false);
            if (company is not null) return Ok(_mapper.Map<CompanyDto>(company));

            _logger.LogInfo($"company with id {companyId} doesn't exist in the database");
            return NotFound();
        }

        [HttpGet("collection/({companyIds})", Name = nameof(GetCompanyCollection))]
        public IActionResult GetCompanyCollection(
            [ModelBinder(BinderType = typeof(ConvertGuidIdsToIEnumerableOfGuidModelBinder))] IEnumerable<Guid> companyIds)
        {
            if (companyIds is not Guid[] idsArray)
            {
                _logger.LogError("Parameter 'companyIds' is null");
                return BadRequest("Parameter 'companyIds' is null");
            }

            var companyEntities = _repoManager.Company.GetByIds(idsArray, false);

            if (idsArray.Length != companyEntities.Count())
            {
                _logger.LogError("Some ids in the companyIds parameter are invalid");
                return NotFound();
            }

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            return Ok(companiesToReturn);
        }

        [HttpPost]
        public IActionResult CreateCompany([FromBody] CompanyForCreationDto company)
        {
            if (company is null)
            {
                _logger.LogError("CompanyForCreationDto sent from the client is null");
                return BadRequest("CompanyForCreation object is null");
            }

            var companyEntity = _mapper.Map<Company>(company);
            _repoManager.Company.CreateCompany(companyEntity);
            _repoManager.Save();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);

            return CreatedAtRoute(nameof(GetCompany), new { companyId = companyEntity.Id }, companyToReturn);
        }

        [HttpPost("collection")]
        public IActionResult CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if (companyCollection is null)
            {
                _logger.LogError("Company collection sent from client is null");
                return BadRequest("companyCollection is null");
            }

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            var createdCompanies = companyEntities as List<Company> ?? companyEntities.ToList();
            createdCompanies.ForEach(c => _repoManager.Company.CreateCompany(c));
            _repoManager.Save();

            var ids = string.Join(',', createdCompanies.Select(c => c.Id));
            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(createdCompanies);

            return CreatedAtRoute(nameof(GetCompanyCollection), new { companyIds = ids }, companiesToReturn);
        }

        [HttpDelete("{companyId:guid}")]
        public IActionResult DeleteCompany(Guid companyId)
        {
            var company = _repoManager.Company.GetCompany(companyId, false);
            if (company is null)
            {
                _logger.LogInfo($"company with id {companyId} doesn't exist in the database");
                return NotFound();
            };

            _repoManager.Company.DeleteCompany(company);
            _repoManager.Save();

            return NoContent();
        }

        [HttpPut("{companyId:guid}")]
        public IActionResult UpdateCompany(Guid companyId, [FromBody] CompanyForUpdatingDto companyForUpdatingDto)
        {
            if (companyForUpdatingDto is null)
            {
                _logger.LogError("CompanyForUpdatingDto from the client was null");
                return BadRequest($"object {nameof(companyForUpdatingDto)} can not be null");
            }

            var company = _repoManager.Company.GetCompany(companyId, true);
            if (company is null)
            {
                _logger.LogInfo($"company with id {companyId} doesn't exist in the database");
                return NotFound();
            }

            _mapper.Map(companyForUpdatingDto, company);
            _repoManager.Save();

            return NoContent();
        }
    }
}
