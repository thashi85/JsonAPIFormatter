using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonAPISerializer.Models;
using Newtonsoft.Json.Serialization;
using JsonAPISerializer.ReferenceResolver;
using JsonAPISerializer.Util;
using System.Reflection;
using JsonApiSerializer.Util;

namespace JsonAPISerializer.JsonConverters
{
    public class DocumentRootConverter : JsonConverter
    {
        private string[] _href;
        public DocumentRootConverter(string[] _href)
        {
            this._href = _href;
            
        }
        public override bool CanConvert(Type objectType)
        {
            return CanConvert(objectType);
        }
        public static bool CanConvertStatic(Type objectType)
        {
            return TypeInfoShim.GetInterfaces(objectType.GetTypeInfo())
               .Select(x => x.GetTypeInfo())
               .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDocumentRoot<>));
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(objectType);
            var rootObject = contract.DefaultCreator();
            serializer.ReferenceResolver.AddReference(null, IncludedReferenceResolver.RootReference, rootObject);
            //var includedConverter = new IncludedConverter();

            foreach (var propName in ReaderUtil.IterateProperties(reader))
            {
                switch (propName)
                {
                    case PropertyNames.Data:

                        var documentRootInterfaceType = TypeInfoShim.GetInterfaces(objectType.GetTypeInfo())
                            .Select(x => x.GetTypeInfo())
                            .FirstOrDefault(x =>
                                x.IsGenericType
                                && x.GetGenericTypeDefinition() == typeof(IDocumentRoot<>));
                        var dataType = documentRootInterfaceType.GenericTypeArguments[0];

                        var dataObj = serializer.Deserialize(reader, dataType);
                        contract.Properties.GetClosestMatchProperty(PropertyNames.Data).ValueProvider.SetValue(rootObject, dataObj);
                        break;
                    case PropertyNames.Included:

                        //    //if our object has an included property we will do our best to populate it
                        //    var property = contract.Properties.GetClosestMatchProperty(propName);
                        //    //if (ReaderUtil.CanPopulateProperty(property))
                        //    //{
                        //    //    ReaderUtil.TryPopulateProperty(serializer, rootObject, contract.Properties.GetClosestMatchProperty(propName), ((ForkableJsonReader)reader).Fork());
                        //    //}

                        //still need to read our values so they are updated
                        foreach (var obj in ReaderUtil.IterateList(reader))
                        {
                            var type = "";
                            var id = "";
                            //read untill id and type
                            foreach (var innerPropName in ReaderUtil.IterateProperties(reader))
                            {
                                switch (innerPropName)
                                {
                                    case PropertyNames.Type:
                                        type= reader.Value.ToString();
                                        break;
                                    case PropertyNames.Id:
                                        id = reader.Value.ToString();
                                        break;
                                    default:
                                        break;

                                }
                                if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(id))
                                {
                                    break;
                                }                                      
                            }
                            var existingObject = serializer.ReferenceResolver.ResolveReference(null, type+":"+id);
                            if (existingObject!=null)
                            {
                                //We have an existing object, its likely our included data has more detail than what
                                //is currently on the object so we will pass the reader so it can be deserialized again
                                var inc_type = existingObject.GetType();
                                var existingObjectContract = serializer.ContractResolver.ResolveContract(inc_type);
                                existingObjectContract.Converter.ReadJson(reader, inc_type, existingObject, serializer);
                            }
                            //contract.Converter.ReadJson(reader, typeof(object), rootObject, serializer);
                            //var includedObject = includedConverter.ReadJson(reader, typeof(object), null, serializer);

                        }

                        break;
                    default:
                        ReaderUtil.TryPopulateProperty(serializer, rootObject, contract.Properties.GetClosestMatchProperty(propName), reader);
                        break;
                }
            }
            return rootObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //Add root reference
            serializer.ReferenceResolver.AddReference(null, IncludedReferenceResolver.RootReference, value);

            var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());
            writer.WriteStartObject();

          

            var propertiesOutput = new HashSet<string>();
            foreach (var prop in contract.Properties)
            {
                //we will do includes last, so we we can ensure all the references have been added
                if (prop.PropertyName == PropertyNames.Included)
                    continue;

                //respect the serializers null handling value
                var propValue = prop.ValueProvider.GetValue(value);
                if (propValue == null && (prop.NullValueHandling ?? serializer.NullValueHandling) == NullValueHandling.Ignore)
                    continue;

                //main object links
                if (_href != null && prop.PropertyName==PropertyNames.Data)
                {
                    ResourceAttribute resA = new ResourceAttribute(_href);
                    WriterUtil.GenerateResourceLinks(serializer,writer, propValue, resA, propValue.GetType().Name.ToLower());//todo
                }

                //A document MAY contain any of these top-level members: jsonapi, links, included
                //We are also allowing everything else they happen to have on the root document
                writer.WritePropertyName(prop.PropertyName);
                serializer.Serialize(writer, propValue);
                propertiesOutput.Add(prop.PropertyName);
            }

           
            //A document MUST contain one of the following (data, errors, meta)
            //so if we do not have one of them we will output a null data
            if (!propertiesOutput.Contains(PropertyNames.Data)
                && !propertiesOutput.Contains(PropertyNames.Errors)
                && !propertiesOutput.Contains(PropertyNames.Meta))
            {
                propertiesOutput.Add(PropertyNames.Data);
                writer.WritePropertyName(PropertyNames.Data);
                writer.WriteNull();
            }
            //Handle Include
            //If a document does not contain a top-level data key, the included member MUST NOT be present
            if (propertiesOutput.Contains(PropertyNames.Data))
            {
                //output the included. If we have a specified included field we will out everything in there
                //and we will also output all the references defined in our reference resolver
                var resolver = (serializer.ReferenceResolver as IncludedReferenceResolver);
                var renderedReferences = resolver?.RenderedReferences ?? new HashSet<string>();
                var includedReferences = serializer.ReferenceResolver as IDictionary<string, object> ?? Enumerable.Empty<KeyValuePair<string, object>>();


                var referencesToInclude = includedReferences
                    .Where(x => x.Key != IncludedReferenceResolver.RootReference)
                    .Where(x => !renderedReferences.Contains(x.Key))
                    .Where(x=>resolver.ResourceToInclude.Contains(x.Key))
                    .ToList(); //dont output values we have already output
                var includedProperty = contract.Properties.GetClosestMatchProperty(PropertyNames.Included);
                var includedValues = includedProperty?.ValueProvider?.GetValue(value) as IEnumerable<object> ?? Enumerable.Empty<object>();

                //if we have some references we will output them
                if (referencesToInclude.Any() || includedValues.Any())
                {
                    writer.WritePropertyName(PropertyNames.Included);
                    writer.WriteStartArray();

                    foreach (var includedValue in includedValues)
                    {
                        serializer.Serialize(writer, includedValue);
                    }

                    //I know we can alter the OrderedDictionary while enumerating it, otherwise this would error
                    //foreach (var includedReference in includedReferences)
                    //for (int i = 0; i < includedReferences.Count(); i++)
                    //{
                    //    var includedReference = includedReferences.ElementAt(i);
                    //    serializer.Serialize(writer, includedReference.Value);
                    //}
                    
                    while (referencesToInclude.Count>0)
                    {
                        for (int i = 0; i < referencesToInclude.Count(); i++)
                        {
                            var includedReference = referencesToInclude.ElementAt(i);
                            serializer.Serialize(writer, includedReference.Value);
                        }
                        referencesToInclude= includedReferences
                                            .Where(x => x.Key != IncludedReferenceResolver.RootReference)
                                            .Where(x => !renderedReferences.Contains(x.Key))
                                            .Where(x => resolver.ResourceToInclude.Contains(x.Key))
                                            .ToList(); //dont output values we have already output

                    }


                    writer.WriteEndArray();
                }
            }          
            
            writer.WriteEndObject();
         

 }

        internal static bool TryResolveAsRootData(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //if we already have a root object then we dont need to resolve the root object
            if (serializer.ReferenceResolver.ResolveReference(null, IncludedReferenceResolver.RootReference) != null)
            {
                return false;
            }
            //var rosolver = (serializer.ContractResolver as JsonApiContractResolver);
            //if (rosolver != null)
            //{
            //    if (ListUtil.IsList(value.GetType()))
            //    {
            //        var enumerable = value as IEnumerable<object> ?? Enumerable.Empty<object>();
            //        if (enumerable!=null && enumerable.Count()>0)
            //        {
            //            rosolver.populateFields(value);
            //        }
            //    }else
            //    {
            //        rosolver.populateFields(value);
            //    }
            //}
            //we do not have a root object, so this is probably the entry point, so we will resolve
            //it as a document root
            var documentRootType = typeof(DocumentRoot<,>).MakeGenericType(value.GetType(), typeof(Error));
            var objContract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(documentRootType);
            var rootObj = objContract.DefaultCreator();

            //set the data property to be our current object
            var dataProp = objContract.Properties.GetClosestMatchProperty("data");
            dataProp.ValueProvider.SetValue(rootObj, value);

            serializer.Serialize(writer, rootObj);
            return true;
        }
        internal static bool TryResolveAsRootData(JsonReader reader, Type objectType, JsonSerializer serializer, out object obj)
        {
            //if we already have a root object then we dont need to resolve the root object
            if (serializer.ReferenceResolver.ResolveReference(null, IncludedReferenceResolver.RootReference) != null)
            {
                obj = null;
                return false;
            }

            //we do not have a root object, so this is probably the entry point, so we will resolve
            //a document root and return the data object
            var documentRootType = typeof(DocumentRoot<,>).MakeGenericType(objectType, typeof(Error));
            var objContract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(documentRootType);
            var dataProp = objContract.Properties.GetClosestMatchProperty("data");

            var root = serializer.Deserialize(reader, documentRootType);
            obj = dataProp.ValueProvider.GetValue(root);
            return true;
        }
    }

    public class DocumentRoot<TData, TError> : IDocumentRoot<TData> where TError : IError
    {
        public TData Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<TError> Errors { get; set; }
    }
}
