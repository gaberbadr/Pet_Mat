using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos;

namespace CoreLayer.Helper.EnumsHelperMethod
{
    public static class EnumHelper
    {
        public static List<EnumResponseDto> GetEnumValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new EnumResponseDto
                {
                    Id = Convert.ToInt32(e),
                    Name = e.GetType()
                            .GetMember(e.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString()
                })
                .ToList();
        }
    }
}
