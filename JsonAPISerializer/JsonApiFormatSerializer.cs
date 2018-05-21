using JsonAPIFormatSerializer.JsonConverters;
using JsonAPIFormatSerializer.ReferenceResolver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAPIFormatSerializer
{
    public class JsonApiFormatSerializer : JsonSerializerSettings
    {
        private string[] Includes;
        private string[] Fields;
        private string[] Href;
        public string BaseUrl;
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonApiFormatSerializer"/> class.
        /// </summary>
        /// <param name="resourceObjectConverter">The converter to use when serializing/deserializing a JsonApi resource object</param>
        public JsonApiFormatSerializer(JsonConverter resourceObjectConverter, string[] _includes = null, string[] _fields = null, string[] _href=null) : base()
        {
            this.NullValueHandling = NullValueHandling.Ignore;
            this.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            this.ReferenceResolverProvider = () => new IncludedReferenceResolver(_includes,BaseUrl);
            this.ContractResolver = new JsonApiContractResolver(resourceObjectConverter,_includes,_fields,_href);
            this.DateParseHandling = DateParseHandling.None;
            //this.TypeNameHandling = TypeNameHandling.All;

            this.Includes = _includes;
            this.Fields = _fields;
            this.Href = _href;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonApiFormatSerializer"/> class.
        /// </summary>
        public JsonApiFormatSerializer(string[] _includes = null, string[] _fields = null, string[] _href = null) : this(new ResourceConverter(),_includes,_fields,_href)
        {

        }
        public JsonApiFormatSerializer() : this(new ResourceConverter(), null, null, null)
        {

        }
        public void ValidateIncludes()
        {

        }
        public void ValidateFields()
        {

        }
    }


}
