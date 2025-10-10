using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Admin;

namespace CoreLayer.Service_Interface
{
    public interface IAdminService
    {
        // User Management
        Task<UserBlockResponseDto> BlockUserAsync(string userId);
        Task<UserBlockResponseDto> UnblockUserAsync(string userId);

        // Species Management
        Task<SpeciesResponseDto> AddSpeciesAsync(SpeciesAdminDto dto);
        Task<DeleteResponseDto> DeleteSpeciesAsync(int id);

        // SubSpecies Management
        Task<SubSpeciesResponseDto> AddSubSpeciesAsync(SubSpeciesAdminDto dto);
        Task<DeleteResponseDto> DeleteSubSpeciesAsync(int id);

        // Color Management
        Task<ColorResponseDto> AddColorAsync(ColorAdminDto dto);
        Task<DeleteResponseDto> DeleteColorAsync(int id);
    }
}
