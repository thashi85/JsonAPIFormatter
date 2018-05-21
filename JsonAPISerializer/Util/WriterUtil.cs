using JsonAPIFormatSerializer.Models;
using JsonAPIFormatSerializer.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JsonAPIFormatSerializer.ReferenceResolver;
namespace JsonAPIFormatSerializer.Util
{
    internal static class WriterUtil
    {
        internal static void WriteIntoElement(JsonWriter writer, Regex pathCondition, string element, Action action)
        {
            if (pathCondition.IsMatch(writer.Path))
            {
                action();
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName(element);
                action();
                writer.WriteEndObject();
            }
        }

        // propertyName and relationshipObj need to be passed when relationships links generated, relationship resource id might be needed to generate the link
        internal static void GenerateResourceLinks(JsonSerializer serializer, JsonWriter writer, object obj, ResourceAttribute resAttr, string reftype, string propertyName = "", object relationshipObj = null)
        {

            var probe = writer as AttributeOrRelationshipProbe;

            if (resAttr != null && resAttr.Links.Count > 0)
            {
                if (probe != null)
                {
                    probe.AddPropertyName(PropertyNames.Links);
                    probe.AddStartObject();
                }
                else
                {
                    writer.WritePropertyName(PropertyNames.Links);
                    writer.WriteStartObject();
                }


                //var typeProp = obj.GetType().GetProperties().Where(p => p.Name.ToLower() == PropertyNames.Type).FirstOrDefault();
                // var reftype = defaultTypeName; //typeProp == null ? defaultTypeName : typeProp.GetValue(obj).ToString();
                var relationshipRefType = string.IsNullOrEmpty(resAttr.IncludeName) ? propertyName: resAttr.IncludeName;
                var id = obj.GetType().GetProperties().Where(p => p.Name.ToLower() == PropertyNames.Id).FirstOrDefault()?.GetValue(obj);
                var reference = ("#{" + (reftype.ToString().ToLower()) + ".id}#");
                var relatedObjRef = "#{" + (relationshipRefType) + ".id}#";
                var relatedObjId = "";
                if (!string.IsNullOrEmpty(relationshipRefType) && relationshipObj != null)
                {
                    var childProp = relationshipObj.GetType().GetProperties().Where(p => p.Name.ToLower() == PropertyNames.Id).FirstOrDefault();
                    if (childProp != null)
                    {
                        relatedObjId = childProp.GetValue(relationshipObj).ToString();
                    }
                }
                foreach (var entry in resAttr.Links)
                {
                    //var lk = entry.Value.Replace(("#{id}#"), id.ToString());
                    var lk = entry.Value.Replace(reference, id?.ToString()).Replace(relatedObjRef, relatedObjId.ToString());
                    var resolver = (serializer.ReferenceResolver as IncludedReferenceResolver);
                    lk=lk.Replace("#{BASE_URL}#", resolver.BaseUrl);

                    Regex rg = new Regex("(?>(#{)[^},(BASE_URL)]*(}#))");
                    var arr = rg.Matches(lk);
                    if (arr.Count > 0)
                    {                     
                        foreach (Match m in arr)
                        {
                            var tag = m.Value.Replace("#{", "").Replace("}#", "");
                            var s = tag.Split('.');
                            var key = string.Join(".", s.Take(s.Length - 1));

                            //loop through link object list to map parent ids
                            var linkedObj = resolver.RefList.Where(r => r.Reference == reftype + ":" + id).SingleOrDefault();
                            if (linkedObj!=null)
                            {
                                var parentObj= resolver.RefList.Where(r => r.Reference == linkedObj.ParentReference).SingleOrDefault();
                                bool found = false;
                                while (!found || parentObj!=null)
                                {
                                    if (parentObj.Reference.ToLower().StartsWith(key + ":")) {
                                        var val = parentObj.Reference.Replace(key + ":", "");
                                        lk = lk.Replace(("#{" + tag.ToString() + "}#"), val.ToString());
                                        found = true;
                                        parentObj = null;
                                    }
                                    else
                                    {
                                        parentObj = resolver.RefList.Where(r => r.Reference == parentObj.ParentReference).SingleOrDefault();
                                    }
                                }
                            }
                           
                        }

                    }
                    if (probe != null)
                    {
                        probe.AddPropertyName(entry.Key);
                        probe.AddValue(lk);
                    }
                    else
                    {
                        writer.WritePropertyName(entry.Key);
                        writer.WriteValue(lk);
                    }
                }
                if (probe != null)
                    probe.AddEndObject();
                else
                    writer.WriteEndObject();

            }
        }

    }
}
