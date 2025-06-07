using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class RegisterDocument
    {
        public int Id { get; set; }
        public string RegNumber { get; set; }
        public DateTime RegDate { get; set; }
        public string DocType { get; set; }
    }
}
