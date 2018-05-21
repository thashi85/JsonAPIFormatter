using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections;
using JsonAPISerializer.Models;

namespace JsonAPISerializer.ReferenceResolver
{
    public class IncludedReferenceResolver :Reference, IReferenceResolver
    {
        public static string RootReference = "root";
        public string[] Includes;
        internal string BaseUrl = "";
        /// <summary>
        /// List to keep track of which references we have outputted during serialization
        /// </summary>
        public HashSet<string> RenderedReferences = new HashSet<string>();
        //unit id list for link genartion
        internal List<LinkObjects> RefList = new List<LinkObjects>();
        //filter based on includes
        internal List<string> ResourceToInclude = new List<string>();
        public IncludedReferenceResolver(string[] Includes,string BaseUrl="")
        {
            this.Includes = Includes?.Select(s=>s.ToLower()).ToArray();
            this.BaseUrl = BaseUrl;
        }
        public static string GetReferenceValue(string id, string type)
        {
            return $"{type}:{id}";
        }


        public void AddReference(object context, string reference, object value)
        {
            this[reference] = value;
            if (RefList.Where(s=>s.Reference==reference).Count()==0) {
                RefList.Add(new LinkObjects() { Reference = reference });
            }
        }
        
        public string GetReference(object context, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsReferenced(object context, object value)
        {
            throw new NotImplementedException();
        }       

        public object ResolveReference(object context, string reference)
        {
            object result;
            this.TryGetValue(reference, out result);
            return result;
        }

       public void ClearReferences()
        {
            this.Clear();
            RenderedReferences.Clear();
        }
      
    }

    internal class LinkObjects
    {
        public string ParentReference { get; set; }
        public string Reference { get; set; }
    }
}
