﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
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
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _repoManager
                .Company
                .GetAllCompanies(trackChanges: false);
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesDto);
        }

        [HttpGet("{companyId:guid}", Name = nameof(GetCompany))]
        public async Task<IActionResult> GetCompany(Guid companyId)
        {
            var company = await _repoManager.Company.GetCompany(companyId, false);
            if (company is not null) return Ok(_mapper.Map<CompanyDto>(company));

            _logger.LogInfo($"company with id {companyId} doesn't exist in the database");
            return NotFound();
        }

        [HttpGet("collection/({companyIds})", Name = nameof(GetCompanyCollection))]
        public async Task<IActionResult> GetCompanyCollection(
            [ModelBinder(BinderType = typeof(ConvertGuidIdsToIEnumerableOfGuidModelBinder))] IEnumerable<Guid> companyIds)
        {
            if (companyIds is not Guid[] idsArray)
            {
                _logger.LogError("Parameter 'companyIds' is null");
                return BadRequest("Parameter 'companyIds' is null");
            }

            var companyEntities = await _repoManager.Company.GetByIds(idsArray, false);

            if (idsArray.Length != companyEntities.Count())
            {
                _logger.LogError("Some ids in the companyIds parameter are invalid");
                return NotFound();
            }

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            return Ok(companiesToReturn);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidateModelState))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {
            var companyEntity = _mapper.Map<Company>(company);
            _repoManager.Company.CreateCompany(companyEntity);
            await _repoManager.SaveAsync();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);

            return CreatedAtRoute(nameof(GetCompany), new { companyId = companyEntity.Id }, companyToReturn);
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if (companyCollection is null)
            {
                _logger.LogError("Company collection sent from client is null");
                return BadRequest("companyCollection is null");
            }

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            var createdCompanies = companyEntities as List<Company> ?? companyEntities.ToList();
            createdCompanies.ForEach(c => _repoManager.Company.CreateCompany(c));
            await _repoManager.SaveAsync();

            var ids = string.Join(',', createdCompanies.Select(c => c.Id));
            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(createdCompanies);

            return CreatedAtRoute(nameof(GetCompanyCollection), new { companyIds = ids }, companiesToReturn);
        }

        [HttpDelete("{companyId:guid}")]
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            var company = await _repoManager.Company.GetCompany(companyId, false);
            if (company is null)
            {
                _logger.LogInfo($"company with id {companyId} doesn't exist in the database");
                return NotFound();
            };

            _repoManager.Company.DeleteCompany(company);
            await _repoManager.SaveAsync();

            return NoContent();
        }

        [HttpPut("{companyId:guid}")]
        [ServiceFilter(typeof(ValidateModelState))]
        public async Task<IActionResult> UpdateCompany(Guid companyId, [FromBody] CompanyForUpdatingDto companyForUpdatingDto)
        {
            var company = await _repoManager.Company.GetCompany(companyId, true);
            if (company is null)
            {
                _logger.LogInfo($"company with id {companyId} doesn't exist in the database");
                return NotFound();
            }

            _mapper.Map(companyForUpdatingDto, company);
            await _repoManager.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{companyId:guid}")]
        [ServiceFilter(typeof(ValidateModelState))]
        public async Task<IActionResult> PartiallyUpdateCompany(Guid companyId,
            [FromBody] JsonPatchDocument<CompanyForUpdatingDto> patchDoc)
        {
            if (patchDoc is null)
            {
                _logger.LogError("CompanyForUpdatingDto from the client was null");
                return BadRequest($"object {nameof(patchDoc)} can not be null");
            }

            var company = await _repoManager.Company.GetCompany(companyId, true);
            if (company is null)
            {
                _logger.LogInfo($"company with id {companyId} doesn't exist in the database");
                return NotFound();
            }

            var companyToPatch = _mapper.Map<CompanyForUpdatingDto>(company);

            patchDoc.ApplyTo(companyToPatch, ModelState);

            TryValidateModel(ModelState);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(companyToPatch, company);
            await _repoManager.SaveAsync();

            return NoContent();
        }
    }
}
