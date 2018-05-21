using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAPISerializer.Models
{
  
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ResourceAttribute : Attribute
    {
        public bool IsResource { get; set; }
        public string IncludeName { get; set; }
        public List<KeyValuePair<string,string>> Links { get; set; }
        public ResourceAttribute()
        {
            Links = new List<KeyValuePair<string, string>>();
        }
        public ResourceAttribute(string[] urls,bool IsResource=true,string IncludeName="")
        {
            this.Links = urls.Select(x => new KeyValuePair<string,string>(x.Split(':')[0], "#{BASE_URL}#" + x.Split(':')[1])).ToList();
            this.IsResource = IsResource;
            this.IncludeName = IncludeName;
        }
    }

}
