using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class InternalDocument
    {
        public int Id { get; set; }
        public string RegNumber { get; set; }
        public DateTime RegDate { get; set; }
        public string DocType { get; set; }
        public string Destination { get; set; }
        public string ShortInfo { get; set; }
        public int SheetsNumber { get; set; }
        public int Quantity { get; set; }
    }
}
