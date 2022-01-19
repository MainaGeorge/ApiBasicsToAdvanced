using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthenticationController(ILoggerManager logger, IMapper mapper, UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [ServiceFilter(typeof(ValidateModelState))]
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserForCreationDto userForRegistration)
        {
            var user = _mapper.Map<User>(userForRegistration);
            var createUser = await _userManager.CreateAsync(user, userForRegistration.Password);

            if (!createUser.Succeeded)
            {
                foreach (var error in createUser.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return BadRequest(ModelState);
            }

            if (!userForRegistration.Roles.Any()) return StatusCode(StatusCodes.Status201Created);

            foreach (var role in userForRegistration.Roles)
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return StatusCode(StatusCodes.Status201Created);
        }
    }
}
