using JsonAPIFormatSerializer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestJsonAPIFormat.Models
{
    [Resource(new string[] { "self:articles/#{article.id}#/" })]
    public class Article
    {
        public string Id { get; set; }

        public string Title { get; set; }
        [Resource(new string[] { "self:people/relationships/#{author.id}#/",
                                  "related:people/#{author.id}#/" })]
        public Person Author { get; set; }
        [Resource(new string[] { "self:articles/#{article.id}#/relationships/comments",
                                 "related:articles/#{article.id}#/comments" })]
        public List<Comment> Comments { get; set; }
    }

    [Resource(new string[] { "self:articles/#{article.id}#/comments/#{comment.id}#" })]
    public class Comment
    {
        public string Id { get; set; }

        public string Body { get; set; }
        [Resource(IncludeName ="CommentAuthor")]
        public Person Author { get; set; }
    }
    [Resource(new string[] { "self:people/#{person.id}#" })]
    public class Person
    {
        public string Id { get; set; }
        public string Type { get; set; }

        [JsonProperty(propertyName: "first-name")] //uses standard Json.NET attributes to control serialization
        public string FirstName { get; set; }

        [JsonProperty(propertyName: "last-name")]
        public string LastName { get; set; }

        public string Twitter { get; set; }
    }
}
