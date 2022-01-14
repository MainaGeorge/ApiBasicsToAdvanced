using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects
{
    public abstract class CompanyForManipulationDto
    {
        [Required(ErrorMessage = "Company Name is a required field")]
        [MaxLength(60, ErrorMessage = "Maximum Length for the Name is 60 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Company Address is a required field")]
        [MaxLength(60, ErrorMessage = "Maximum Length for the Name is 60 characters")]
        public string Address { get; set; }

        public IEnumerable<EmployeeForCreationDto> Employees { get; set; }

    }
}
