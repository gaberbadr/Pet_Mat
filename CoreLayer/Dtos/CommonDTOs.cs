using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Dtos
{
    public class SuccessResponseDto
    {
        public string Message { get; set; }
    }

    public class EnumResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
