using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class IncomingDocument
    {
        public int Id { get; set; }
        public string RegNumber { get; set; }
        public DateTime RegDate { get; set; }
        public string Sender { get; set; }
        public string DocType { get; set; }
        public string Destination { get; set; }
        public string DestinationData { get; set; }
        public bool DestinationCheck { get; set; }
        public string ShortInfo { get; set; }
        public string AddInfo { get; set; }
    }
}
