using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Doctors
{
    public class SubscriptionPackage : BaseEntity<int>
    {
        public string Name { get; set; }           // e.g. "Standard", "Premium"
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }    // e.g. 30, 365
        public bool IsActive { get; set; } = true;
        public List<string> Features { get; set; } = new(); // stored as JSON column

        public ICollection<DoctorSubscription> Subscriptions { get; set; }
    }
}
