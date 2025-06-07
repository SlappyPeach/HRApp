using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class BusinessTrip
    {
        public int Id { get; set; }
        public DateTime TripStartDate { get; set; }
        public DateTime TripEndDate { get; set; }
        public string Destination { get; set; }
        public string Purpose { get; set; }
        public int EmployeeId { get; set; }
    }
}
