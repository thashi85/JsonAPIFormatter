using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using JsonAPISerializer.JsonConverters;
using JsonAPISerializer.Models;
using JsonAPISerializer.Util;
using System.Reflection;

namespace JsonAPISerializer
{
    public class JsonApiContractResolver: DefaultContractResolver
    {
        public readonly JsonConverter ResourceObjectConverter;

        private readonly JsonConverter _resourceListConverter;
        private string[] _includes;
        private string[] _fields;
        private string[] _href;
       

        private List<Field> Fields = new List<JsonAPISerializer.Field>();
        public JsonApiContractResolver(JsonConverter resourceObjectConverter, string[] _includes=null, string[] _fields=null,string[] _href=null)
        {
            ResourceObjectConverter = resourceObjectConverter;
            _resourceListConverter = new ResourceListConverter(ResourceObjectConverter);

            this.NamingStrategy = new CamelCaseNamingStrategy();
            this._fields = _fields;
            this._includes = _includes;            
            this._href = _href;           
           
        }

        public JsonApiContractResolver()
            : this(new ResourceConverter())
        {
        }

        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            //if (ErrorConverter.CanConvertStatic(objectType))
            //    return new ErrorConverter();

            //if (ErrorListConverter.CanConvertStatic(objectType))
            //    return new ErrorListConverter();

            if (ResourceObjectConverter.CanConvert(objectType))
                return ResourceObjectConverter;

            if (_resourceListConverter.CanConvert(objectType))
                return _resourceListConverter;
           
            if (DocumentRootConverter.CanConvertStatic(objectType))
                return new DocumentRootConverter(_href);

            return base.ResolveContractConverter(objectType);
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            if (_fields != null && _fields.Count()>0 && Fields.Count()==0)
            {
                populateFields(type);
            }
            var ret= base.CreateProperties(type, memberSerialization);
            var tp = type.Name.ToLower();
            var res = ((type.GetCustomAttributes(typeof(ResourceAttribute))).SingleOrDefault() as ResourceAttribute);

            if (!DocumentRootConverter.CanConvertStatic(type) && (res==null || res.IsResource)) {
                ret = ret.Where(p =>
                                (Fields == null || Fields.Count() == 0 ||
                                  p.PropertyName.ToLower() == PropertyNames.Id || p.PropertyName.ToLower() == PropertyNames.Type ||
                                  Fields.Any(r => r.Name == p.PropertyName.ToLower() && (r.Type==tp || r.SubTypes.Contains(tp))) )
                                ).ToList();
            }
            return ret;
        }
           
        internal void populateFields(Type baseObjType)
        {
            if (_fields != null)
            {
                var baseType = baseObjType.Name.ToLower();
                foreach (string fl in _fields)
                {
                    Field field = new Field();
                    if (fl.Contains("."))
                    {
                        var arr = fl.Split('.');
                        field.Name = arr[1].ToLower();
                        field.Type = arr[0].ToLower();
                    }
                    else
                    {
                        field.Name = fl.ToLower();
                        field.Type = baseType;
                    }

                    Assembly[] assembly = Utility.GetAssemblies();
                    var type = assembly.SelectMany(s => s.GetTypes()).ToList().Where(t => t.Name.ToLower() == field.Type).SingleOrDefault();
                    if (type != null)
                        field.SubTypes = assembly.SelectMany(s => s.GetTypes()).Where(t => t.IsClass && t.IsSubclassOf(type)).Select(t => t.Name.ToLower()).ToList();//t.BaseType == parentType

                    Fields.Add(field);

                }
            }
        }
    }

    public class Field
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public List<string> SubTypes { get; set; }
        public Field()
        {
            SubTypes = new List<string>();
        }
    }
}
