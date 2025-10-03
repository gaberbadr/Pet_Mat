using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Orders
{
    public class OrderAddress : BaseEntity<int>
    {

        [MaxLength(100)]
        public string FName { get; set; }

        [MaxLength(100)]
        public string LName { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(200)]
        public string Street { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }
    }
}
