using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestParameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CompanyEmployees.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/companies")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repoManager;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IDataShaper<CompanyDto> _dataShaper;

        public CompaniesController(IRepositoryManager repoManager, IMapper mapper, ILoggerManager logger, IDataShaper<CompanyDto> dataShaper)
        {
            _repoManager = repoManager;
            _mapper = mapper;
            _logger = logger;
            _dataShaper = dataShaper;
        }

        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST, PUT, PATCH");
            Response.Headers.ContentLength = 0;
            return Ok();
        }

        [HttpGet]
        [HttpHead]
        [ProducesResponseType(typeof(IEnumerable<Entity>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCompanies([FromQuery] CompanyRequestParameter reqParam)
        {
            var companies = await _repoManager
                .Company
                .GetAllCompanies(reqParam, trackChanges: false);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(companies.MetaData));

            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            var shapedEntities = _dataShaper.ShapeData(companiesDto, reqParam.Fields).Select(e => e.Entity);
            return Ok(shapedEntities);
        }

        [HttpGet("{companyId:guid}", Name = nameof(GetCompany))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Entity), StatusCodes.Status200OK)]
        public IActionResult GetCompany(Guid companyId, [FromQuery] CompanyRequestParameter parameter)
        {
            var company = HttpContext.Items["company"] as Company;
            var companyDto = _mapper.Map<CompanyDto>(company);

            return Ok(_dataShaper.ShapeData(companyDto, parameter.Fields));
        }

        [HttpGet("collection/({companyIds})", Name = nameof(GetCompanyCollection))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IEnumerable<CompanyDto>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status201Created)]
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            var company = HttpContext.Items["company"] as Company;
            _repoManager.Company.DeleteCompany(company);
            await _repoManager.SaveAsync();

            return NoContent();
        }

        [HttpPut("{companyId:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ServiceFilter(typeof(ValidateModelState))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid companyId, [FromBody] CompanyForUpdatingDto companyForUpdatingDto)
        {
            var company = HttpContext.Items["company"] as Company;
            _mapper.Map(companyForUpdatingDto, company);
            await _repoManager.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{companyId:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ServiceFilter(typeof(ValidateModelState))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateCompany(Guid companyId,
            [FromBody] JsonPatchDocument<CompanyForUpdatingDto> patchDoc)
        {
            var company = HttpContext.Items["company"] as Company;

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
