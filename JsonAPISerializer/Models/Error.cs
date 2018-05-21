using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAPIFormatSerializer.Models
{
    public class Error : IError
    {
        public string Id { get; set; }

        public string Status { get; set; }

        public string Code { get; set; }
    }
}
