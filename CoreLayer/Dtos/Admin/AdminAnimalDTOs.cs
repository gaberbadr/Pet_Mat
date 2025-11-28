using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Dtos.Admin
{
    // ==================== INPUT DTOs ====================



    public class SpeciesAdminDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class SubSpeciesAdminDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public int SpeciesId { get; set; }
    }

    public class ColorAdminDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }

    // ==================== OUTPUT DTOs ====================


    public class SpeciesResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class SubSpeciesResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SpeciesId { get; set; }
        public string SpeciesName { get; set; }
        public string Message { get; set; }
    }

    public class ColorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class DeleteResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? DeletedId { get; set; }
    }


}
