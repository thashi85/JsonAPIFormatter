using JsonAPIFormatSerializer.Util;
using JsonAPIFormatSerializer.Models;
using JsonAPIFormatSerializer.ReferenceResolver;
using JsonAPIFormatSerializer.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JsonAPIFormatSerializer.JsonConverters
{
    public class ResourceConverter : JsonConverter
    {
        private static readonly Regex DataReadPathRegex = new Regex($@"^$|{PropertyNames.Included}(\[\d+\])?$|{PropertyNames.Data}(\[\d+\])?$");
        private static readonly Regex DataWritePathRegex = new Regex($@"{PropertyNames.Included}(\[\d+\])?$|{PropertyNames.Data}(\[\d+\])?$");
        private static readonly Regex DataPathRegex = new Regex($@"{PropertyNames.Data}$");

        public override bool CanConvert(Type objectType)
        {
            var res = ((objectType.GetCustomAttributes(typeof(ResourceAttribute))).SingleOrDefault() as ResourceAttribute);
            if (res != null && !res.IsResource)
                return false;
            return TypeInfoShim.GetPropertyFromInhertianceChain(objectType.GetTypeInfo(), PropertyNames.Id) != null ||
                 TypeInfoShim.GetPropertyFromInhertianceChain(objectType.GetTypeInfo(), PropertyNames.Tid) != null;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //we may be starting the deserialization here, if thats the case we need to resolve this object as the root
            object obj;
            if (DocumentRootConverter.TryResolveAsRootData(reader, objectType, serializer, out obj))
                return obj;

            JsonObjectContract contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(objectType);
            var isNew = false;
            if (existingValue==null)
            {
                existingValue = contract.DefaultCreator();
                isNew = true;                
            }

            existingValue=PopulateProperties(serializer, existingValue, reader, contract);
            if (isNew)
            {
                var id = contract.Properties.GetClosestMatchProperty(PropertyNames.Id).ValueProvider.GetValue(existingValue);
                var type = existingValue.GetType().Name.ToLower();
                var typeProp = contract.Properties.GetClosestMatchProperty(PropertyNames.Type);
                if (typeProp != null)
                {
                    type = typeProp.ValueProvider.GetValue(existingValue).ToString();
                }
                    
                serializer.ReferenceResolver.AddReference(null, type+":"+id, existingValue);
            }
            return existingValue;
        }
        protected object PopulateProperties(JsonSerializer serializer, object obj, JsonReader reader, JsonObjectContract contract)
        {
          
            foreach (var propName in ReaderUtil.IterateProperties(reader))
            {
               
                if (propName == PropertyNames.Type)
                {
                    var type = reader.Value;
                    if (obj.GetType().Name.ToLower() != type.ToString().ToLower())
                    {
                        Assembly[] assembly = Utility.GetAssemblies();
                        var tp = assembly.SelectMany(s => s.GetTypes()).ToList().Where(t => t.Name.ToLower() == type.ToString().ToLower()).SingleOrDefault();
                        if (tp != null)
                        {
                            obj = Activator.CreateInstance(tp);
                            
                        }
                    }
                    //retObj.id = obj;
                }
                var successfullyPopulateProperty = ReaderUtil.TryPopulateProperty(
                   serializer,
                   obj,
                   contract.Properties.GetClosestMatchProperty(propName),
                   reader);

                if (!successfullyPopulateProperty)
                {
                                   
                    //flatten out attributes onto the object
                    if (propName == "attributes")
                    {
                        foreach (var innerPropName in ReaderUtil.IterateProperties(reader))
                        {
                            ReaderUtil.TryPopulateProperty(
                               serializer,
                               obj,
                               contract.Properties.GetClosestMatchProperty(innerPropName),
                               reader);
                        }
                    }

                    //flatten out relationships onto the object
                    if (propName == "relationships")
                    {   
                        foreach (var innerPropName in ReaderUtil.IterateProperties(reader))
                        {
                            //read into the 'Data' path
                            var preDataPath = ReaderUtil.ReadUntilStart(reader, DataPathRegex);

                            ReaderUtil.TryPopulateProperty(
                                serializer,
                                obj,
                                contract.Properties.GetClosestMatchProperty(innerPropName),
                                reader);
                            //read out of the 'Data' path
                            ReaderUtil.ReadUntilEnd(reader, preDataPath);
                        }
                   
                    }
                }
            }
            return obj;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //Check Document Root is exits
            if (DocumentRootConverter.TryResolveAsRootData(writer, value, serializer))
            {
                return;
            }
            WriterUtil.WriteIntoElement(writer, DataWritePathRegex, PropertyNames.Data, () =>
            {
                var probe = writer as AttributeOrRelationshipProbe;
                if (probe != null)
                {
                    //if someone is sending a probe its because we are in a relationship property.
                    //let the probe know we are in a relationship and write the reference element
                    probe.PropertyType = AttributeOrRelationshipProbe.Type.Relationship;
                    WriteRelationshipJson(writer, value, serializer);
                }
                else
                {
                    WriteResourceJson(writer, value, serializer);
                }
            });

          

        }
        private void WriteResourceJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resolver = (serializer.ReferenceResolver as IncludedReferenceResolver);
            var currentIndex = resolver.RefList.Count;
            var valueType = value.GetType();

            //Serialize object
            var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(valueType);
            writer.WriteStartObject();

            //will capture id and type as we go through
            object id = null;
            object type = null;

            //Handle link
            var linkProps = valueType.GetProperties().Where(p => p.GetCustomAttributes(typeof(ResourceAttribute), false).Count() != 0).ToList();

            //A resource object MUST contain at least the following top-level members: type
            var typeProp = contract.Properties.GetClosestMatchProperty("type");
            if (typeProp == null)
            {
                writer.WritePropertyName("type");
                type = GenerateDefaultTypeName(valueType);
                serializer.Serialize(writer, type);
            }

            List<JsonWriterCapture> attributes = new List<JsonWriterCapture>();
            List<JsonWriterCapture> relationships = new List<JsonWriterCapture>();
            foreach (var prop in contract.Properties.Where(x => !x.Ignored))
            {
                var propValue = prop.ValueProvider.GetValue(value);
                if (propValue == null && (prop.NullValueHandling ?? serializer.NullValueHandling) == NullValueHandling.Ignore)
                    continue;

                switch (prop.PropertyName)
                {
                    //In addition, a resource object MAY contain any of these top - level members: links, meta, attributes, relationships
                    case PropertyNames.Id: //Id is optional on base objects
                    case PropertyNames.Tid://
                        id = propValue;
                        writer.WritePropertyName(prop.PropertyName);
                        serializer.Serialize(writer, id);
                        break;
                    case PropertyNames.Meta:
                        writer.WritePropertyName(prop.PropertyName);
                        serializer.Serialize(writer, propValue);
                        break;
                    case PropertyNames.Type:
                        writer.WritePropertyName(PropertyNames.Type);
                        type = typeProp?.ValueProvider?.GetValue(value) ?? GenerateDefaultTypeName(valueType);
                        serializer.Serialize(writer, type);
                        break;
                    default:
                        var resAttr = linkProps?.Where(l => l.Name.ToLower() == prop.PropertyName.ToLower()).SingleOrDefault();

                        //we do not know if it is an Attribute or a Relationship
                        //so we will send out a probe to determine which one it is
                        var probe = new AttributeOrRelationshipProbe();
                        probe.WritePropertyName(prop.PropertyName);
                        serializer.Serialize(probe, propValue);

                        //handle relationship links start
                        if (probe.PropertyType == AttributeOrRelationshipProbe.Type.Relationship)
                        {
                            ResourceAttribute link=null;
                            if (resAttr != null)
                            {
                                link = resAttr.GetCustomAttributes(typeof(ResourceAttribute), false)[0] as ResourceAttribute;
                                if (link.Links != null && link.Links.Count > 0)
                                {
                                    //probe.WriteStartObject();
                                    var act = probe.GetActions();
                                    //001:remove last entry and add links 
                                    act.RemoveAt(act.Count - 1);

                                    WriterUtil.GenerateResourceLinks(serializer, probe, value, link, type.ToString(), prop.PropertyName, propValue);
                                    //additional end object since it is removed to add links in 001 section
                                    probe.AddEndObject();
                                }
                                if (!link.IsResource)
                                {
                                    probe.PropertyType = AttributeOrRelationshipProbe.Type.Attribute;
                                }
                                                               
                            }

                            //Handle Includes
                            if (resolver.Includes != null && resolver.Includes.Count() > 0)
                            {
                                if (ListUtil.IsList(propValue.GetType()))
                                {
                                    var enumerable = propValue as IEnumerable<object> ?? Enumerable.Empty<object>();
                                    var include_name = ((link != null && !string.IsNullOrEmpty(link.IncludeName)) ? link.IncludeName : prop.PropertyName);
                                    if (enumerable.Count() > 0 && resolver.Includes.Contains(include_name.ToLower()))
                                    {
                                        foreach (var valueElement in enumerable)
                                        {
                                            var include_id = valueElement.GetType().GetProperties().Where(p => p.Name.ToLower() == PropertyNames.Id).FirstOrDefault()?.GetValue(valueElement);
                                            var include_type = valueElement.GetType().GetProperties().Where(p => p.Name.ToLower() == PropertyNames.Type).FirstOrDefault()?.GetValue(valueElement);
                                            if (include_type == null)
                                            {
                                                include_type = valueElement.GetType().Name.ToLower();
                                            }
                                            if (include_id != null && include_type != null)
                                            {
                                                resolver.ResourceToInclude.Add(IncludedReferenceResolver.GetReferenceValue(include_id.ToString(), include_type.ToString()));
                                            }

                                        }
                                    }

                                }
                                else
                                {
                                    var include_id = propValue.GetType().GetProperties().Where(p => p.Name.ToLower() == PropertyNames.Id).FirstOrDefault()?.GetValue(propValue);
                                    var include_type = propValue.GetType().GetProperties().Where(p => p.Name.ToLower() == PropertyNames.Type).FirstOrDefault()?.GetValue(propValue);
                                    if (include_type == null)
                                    {
                                        include_type = propValue.GetType().Name.ToLower();
                                    }
                                    if (include_id != null && include_type != null)
                                    {
                                        var include_name = ((link != null && !string.IsNullOrEmpty(link.IncludeName)) ? link.IncludeName : prop.PropertyName);
                                        if (resolver.Includes != null && resolver.Includes.Contains(include_name.ToLower()))
                                            resolver.ResourceToInclude.Add(IncludedReferenceResolver.GetReferenceValue(include_id.ToString(), include_type.ToString()));
                                    }
                                }
                            }
                        }
                        //handle relationship links end

                        (probe.PropertyType == AttributeOrRelationshipProbe.Type.Attribute
                                   ? attributes
                                   : relationships).Add(probe);

                        break;
                }
            }

            //add reference to this type, so others can reference it
            var referenceValue = IncludedReferenceResolver.GetReferenceValue(id?.ToString(), type?.ToString());
            serializer.ReferenceResolver.AddReference(null, referenceValue, value);
            resolver?.RenderedReferences?.Add(referenceValue);

            //set parent reference for generating link parameters
            for (int i = currentIndex; i < resolver.RefList.Count; i++)
            {
                if (resolver.RefList[i].Reference != referenceValue)
                    resolver.RefList[i].ParentReference = referenceValue;
            }
            //output our attibutes in an attribute tag
            if (attributes.Count > 0)
            {
                writer.WritePropertyName(PropertyNames.Attributes);
                writer.WriteStartObject();
                foreach (var attribute in attributes)
                    attribute.ApplyCaptured(writer);
                writer.WriteEndObject();
            }

            //output our relationships in a relationship tag
            if (relationships.Count > 0)
            {
                writer.WritePropertyName(PropertyNames.Relationships);
                writer.WriteStartObject();

                foreach (var relationship in relationships)
                {
                    relationship.ApplyCaptured(writer);
                }
                writer.WriteEndObject();
            }
            //Links    
            var res = ((value.GetType().GetCustomAttributes(typeof(ResourceAttribute))).SingleOrDefault() as ResourceAttribute);
            WriterUtil.GenerateResourceLinks(serializer,writer, value, res,type.ToString());

            writer.WriteEndObject();
        }
        private void WriteRelationshipJson(JsonWriter writer, object value, JsonSerializer serializer, JsonObjectContract contract = null)
        {
            var resolver = (serializer.ReferenceResolver as IncludedReferenceResolver);
          
            contract = contract ?? (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

            writer.WriteStartObject();

            writer.WritePropertyName(PropertyNames.Type);
            var typeProp = contract.Properties.GetClosestMatchProperty(PropertyNames.Type);
            var typeVal = typeProp?.ValueProvider?.GetValue(value) ?? GenerateDefaultTypeName(value.GetType());
            serializer.Serialize(writer, typeVal);


            //A "resource identifier object" MUST contain type and id members.
            writer.WritePropertyName(PropertyNames.Id);
            var idProp = contract.Properties.GetClosestMatchProperty(PropertyNames.Id);
            var idVal = idProp?.ValueProvider?.GetValue(value) ?? string.Empty;
            serializer.Serialize(writer, idVal);


            var tidProp = contract.Properties.GetClosestMatchProperty(PropertyNames.Tid);
            var tidVal = tidProp?.ValueProvider?.GetValue(value) ?? string.Empty;
            if (tidProp != null)
            {
                writer.WritePropertyName(PropertyNames.Tid);
                serializer.Serialize(writer, tidVal);
            }


           


            //we will only write the object to included if there are properties that have have data
            //that we cant include within the reference
            var willWriteObjectToIncluded = contract.Properties.Any(prop =>
            {
                //ignore id, type, meta and ignored properties
                if (prop.PropertyName == PropertyNames.Id
                    || prop.PropertyName == PropertyNames.Tid
                    || prop.PropertyName == PropertyNames.Type
                    || prop.PropertyName == PropertyNames.Meta
                    || prop.Ignored)
                    return false;

                //ignore null properties
                var propValue = prop.ValueProvider.GetValue(value);
                if (propValue == null)
                {
                    if (prop.NullValueHandling != null)
                    {
                        if (prop.NullValueHandling == NullValueHandling.Ignore)
                            return false;
                    }
                    else
                    {
                        if (serializer.NullValueHandling == NullValueHandling.Ignore)
                            return false;
                    }
                }
                //we have another property with a value
                return true;
            });

            //typeically we would just write the meta in the included. But if we are not going to
            //have something in included we will write the meta inline here
            if (!willWriteObjectToIncluded)
            {
                var metaProp = contract.Properties.GetClosestMatchProperty(PropertyNames.Meta);
                var metaVal = metaProp?.ValueProvider?.GetValue(value);
                if (metaVal != null)
                {
                    writer.WritePropertyName(PropertyNames.Meta);
                    serializer.Serialize(writer, metaVal);
                }
            }


            writer.WriteEndObject();


            if (willWriteObjectToIncluded)
            {
                var reference = "";
                if (string.IsNullOrEmpty(idVal.ToString()) && !string.IsNullOrEmpty(tidVal.ToString()))
                {
                    reference = IncludedReferenceResolver.GetReferenceValue(tidVal.ToString(), typeVal.ToString());
                }
                else
                {
                    reference = IncludedReferenceResolver.GetReferenceValue(idVal.ToString(), typeVal.ToString());
                }
                serializer.ReferenceResolver.AddReference(null, reference, value);

               
            }
        }

        /// <summary>
        /// If there is no Type property on the item then this is called to generate a default Type name
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual string GenerateDefaultTypeName(Type type)
        {
            return type.Name.ToLowerInvariant();
        }
    }
}
