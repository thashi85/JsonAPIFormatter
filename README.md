# JsonAPIFormatter
## Json API Format: Serializer and Deserializer 

###### JsonAPIFormatSerializer
The JsonApiFormatSerializer serialize  C# objects into the json:api format and deserialize Json API format data to C# objects.

It supports sparse fields and includes
Easily links can be set up for resources and relationships
Deserialization support for collection of different types of derived classes  

Json API Format serializer settings can be initialize without passing any parameter or passing following parameters
>    string[] _includes  
>    string[] _fields   
>   string[] _href	

Base URL need to be set to generate links
```c#
var settings = new JsonApiFormatSerializer();
settings.BaseUrl = “http://localhost/”;
```           
###### Models
Example 1:  Article

    [Resource(new string[] { "self:articles/#{article.id}#/" })]
    public class Article
    {
        public string Id { get; set; }

        public string Title { get; set; }
        [Resource(new string[] { "self:people/relationships/#{author.id}#/", "related:people/#{author.id}#/" })]
        public Person Author { get; set; }
        [Resource(new string[] { "self:articles/#{article.id}#/relationships/comments", "related:articles/#{article.id}#/comments" })]
        public List<Comment> Comments { get; set; }
    }
    [Resource(new string[] { "self:articles/#{article.id}#/comments/#{comment.id}#" })]
    public class Comment
    {
        public string Id { get; set; }

        public string Body { get; set; }

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
    
    
Sample Data
```c#
var author = new Person
            {
                Id = "9",
                FirstName = "Dan",
                LastName = "Gebhardt",
                Twitter = "dgeb",
                Type="people"
            };

var article = 
                    new Article
                    {
                        Id = "1",
                        Title = "JSON API sample!",
                        Author = author,
                        Comments = new List<Comment>
                        {
                            new Comment
                            {
                                Id = "5",
                                Body = "First!",
                                Author = new Person
                                {
                                    Id = "2",
                                    Type="people"
                                },
                            },
                            new Comment
                            {
                                Id = "12",
                                Body = "I like XML better",
                                Author = author,
                            }
                        }

                    };
```                   
###### Single Resource

Serialize using following, Base url need to be passed to generate the  links.
```c#
var settings = new JsonApiFormatSerializer()
              {
                 BaseUrl = "http://localhost/"
              };
//To serialize json:api format
string json_article = JsonConvert.SerializeObject(article, settings);

//To deserialize from json:api format
Article[] arr = JsonConvert.DeserializeObject<Article[]>(json_article, new JsonApiFormatSerializer());

```
By default type of the class is considered as the type, by defining an attribute called type, 
that can be override
Ex:  type will be people
```c#
var author = new Person
            {
                Id = "9",
                FirstName = "Dan",
                LastName = "Gebhardt",
                Twitter = "dgeb",
                Type="people"
            };
```
Json Output
>Note: comments and author details are not return as include since it is not defined as include
>When includes are not defined only id and type will be return as relationships
```Json
{  
   "data":{  
      "type":"article",
      "id":"1",
      "attributes":{  
         "title":"JSON API sample!"
      },
      "relationships":{  
         "author":{  
            "data":{  
               "type":"people",
               "id":"9"
            },
            "links":{  
               "self":"http://localhost/people/relationships/9/",
               "related":"http://localhost/people/9/"
            }
         },
         "comments":{  
            "data":[  
               {  
                  "type":"comment",
                  "id":"5"
               },
               {  
                  "type":"comment",
                  "id":"12"
               }
            ],
            "links":{  
               "self":"http://localhost/articles/1/relationships/comments",
               "related":"http://localhost/articles/1/comments"
            }
         }
      },
      "links":{  
         "self":"http://localhost/articles/1/"
      }
   }
}
```
###### List of Resources

Serialize list of resources as below sample
Multiple links can be pass to the Json API format serializer
```c#
  var settings = new JsonApiFormatSerializer(null, null,
                new string[] { "self:articles", 
                               "next:articles?page[offset]=2",     
                               "last:articles?page[offset]=10" });

  settings.BaseUrl = "http://localhost/";              
          
  //To serialize json:api format
  string json_article = JsonConvert.SerializeObject(articles, settings);
```
Json Output
```Json
{  
   "links":{  
      "self":"http://localhost/articles",
      "next":"http://localhost/articles?page[offset]=2",
      "last":"http://localhost/articles?page[offset]=10"
   },
   "data":[  
      {  
         "type":"article",
         "id":"1",
         "attributes":{  
            "title":"JSON API sample!"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               },
               "links":{  
                  "self":"http://localhost/people/relationships/9/",
                  "related":"http://localhost/people/9/"
               }
            },
            "comments":{  
               "data":[  
                  {  
                     "type":"comment",
                     "id":"5"
                  },
                  {  
                     "type":"comment",
                     "id":"12"
                  }
               ],
               "links":{  
                  "self":"http://localhost/articles/1/relationships/comments",
                  "related":"http://localhost/articles/1/comments"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/"
         }
      },
      {  
         "type":"article",
         "id":"2",
         "attributes":{  
            "title":"Second article data"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               },
               "links":{  
                  "self":"http://localhost/people/relationships/9/",
                  "related":"http://localhost/people/9/"
               }
            },
            "comments":{  
               "data":[  
                  {  
                     "type":"comment",
                     "id":"6"
                  }
               ],
               "links":{  
                  "self":"http://localhost/articles/2/relationships/comments",
                  "related":"http://localhost/articles/2/comments"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/2/"
         }
      }
   ]
}
```
###### Links
API specific links need to be setup with JsonApiFormatSerializer instantiate. 
This may be self-link to the current API end point, next and last page URL of search API.
```c#
var settings = new JsonApiFormatSerializer(null, null,
                new string[] { "self:articles",
                               "next:articles?page[offset]=2",
                               "last:articles?page[offset]=10" });
            settings.BaseUrl = "http://localhost/";
```
Links can be defined in class level or attribute level (Resource Links)
```c#
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
```
For class level links, “#{[Type of the class].id}#” is used to generate actual value at the serialization
Ex:  “#{article.id}#” will be replaced with Id of the article
For attribute level links, “#{[Property Name].id}#” is used
```c#
        [Resource(new string[] { "self:people/relationships/#{author.id}#/",
                                  "related:people/#{author.id}#/" })]
        public Person Author { get; set; }
```
“#{author.id}#” will be replaced with id of the author 

###### Includes
Include and fields can be defined when instantiate the JsonApiFormatSerializer
Property name is used to identify the include
```c#
  public List<Comment> Comments { get; set; }
```
Ex: Include comments
```c#
var settings = new JsonApiFormatSerializer(new string[] { "comments" }, null,
                new string[] { "self:articles",
                               "next:articles?page[offset]=2",
                               "last:articles?page[offset]=10" });
settings.BaseUrl = "http://localhost/";
               
          
//To serialize json:api format
string json_article = JsonConvert.SerializeObject(articles, settings);

//To deserialize from json:api format
Article[] arr = JsonConvert.DeserializeObject<Article[]>(json_article, new JsonApiFormatSerializer());

```
JSON output with includes
```JSON
{  
   "links":{  
      "self":"http://localhost/articles",
      "next":"http://localhost/articles?page[offset]=2",
      "last":"http://localhost/articles?page[offset]=10"
   },
   "data":[  
      {  
         "type":"article",
         "id":"1",
         "attributes":{  
            "title":"JSON API sample!"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               },
               "links":{  
                  "self":"http://localhost/people/relationships/9/",
                  "related":"http://localhost/people/9/"
               }
            },
            "comments":{  
               "data":[  
                  {  
                     "type":"comment",
                     "id":"5"
                  },
                  {  
                     "type":"comment",
                     "id":"12"
                  }
               ],
               "links":{  
                  "self":"http://localhost/articles/1/relationships/comments",
                  "related":"http://localhost/articles/1/comments"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/"
         }
      },
      {  
         "type":"article",
         "id":"2",
         "attributes":{  
            "title":"Second article data"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               },
               "links":{  
                  "self":"http://localhost/people/relationships/9/",
                  "related":"http://localhost/people/9/"
               }
            },
            "comments":{  
               "data":[  
                  {  
                     "type":"comment",
                     "id":"6"
                  }
               ],
               "links":{  
                  "self":"http://localhost/articles/2/relationships/comments",
                  "related":"http://localhost/articles/2/comments"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/2/"
         }
      }
   ],
   "included":[  
      {  
         "type":"comment",
         "id":"5",
         "attributes":{  
            "body":"First!"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"person",
                  "id":"2"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/5"
         }
      },
      {  
         "type":"comment",
         "id":"12",
         "attributes":{  
            "body":"I like XML better"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/12"
         }
      },
      {  
         "type":"comment",
         "id":"6",
         "attributes":{  
            "body":"test"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"person",
                  "id":"7"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/2/comments/6"
         }
      }
   ]
}
```
Since include is identified using property name, If we include both comments and author, article related authors and comment related authors will be return
```c#
var settings = new JsonApiFormatSerializer(new string[] {"comments", "author" }, null,
                new string[] { "self:articles",
                               "next:articles?page[offset]=2",
                               "last:articles?page[offset]=10" });
settings.BaseUrl = "http://localhost/";
```
Json output
```JSON
{  
   "links":{  
      "self":"http://localhost/articles",
      "next":"http://localhost/articles?page[offset]=2",
      "last":"http://localhost/articles?page[offset]=10"
   },
   "data":[  
      {  
         "type":"article",
         "id":"1",
         "attributes":{  
            "title":"JSON API sample!"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               },
               "links":{  
                  "self":"http://localhost/people/relationships/9/",
                  "related":"http://localhost/people/9/"
               }
            },
            "comments":{  
               "data":[  
                  {  
                     "type":"comment",
                     "id":"5"
                  },
                  {  
                     "type":"comment",
                     "id":"12"
                  }
               ],
               "links":{  
                  "self":"http://localhost/articles/1/relationships/comments",
                  "related":"http://localhost/articles/1/comments"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/"
         }
      }
   ],
   "included":[  
      {  
         "id":"9",
         "type":"people",
         "attributes":{  
            "first-name":"Dan",
            "last-name":"Gebhardt",
            "twitter":"dgeb"
         },
         "links":{  
            "self":"http://localhost/people/9"
         }
      },
      {  
         "type":"comment",
         "id":"5",
         "attributes":{  
            "body":"First!"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"2"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/5"
         }
      },
      {  
         "type":"comment",
         "id":"12",
         "attributes":{  
            "body":"I like XML better"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/12"
         }
      },
      {  
         "id":"2",
         "type":"people",
         "links":{  
            "self":"http://localhost/people/2"
         }
      }
   ]
}
```
If we need to differentiate two different includes “IncludeName” attribute value can be used to set different name for the includes.
```c#
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
```
Ex:
If we define the include as comment and author, all the comments of the article and article related authors will be return
Sample data
```C#
var author = new Person
         {
             Id = "9",
             FirstName = "Dan",
             LastName = "Gebhardt",
             Twitter = "dgeb",
             Type="people"
         };
 
         var articles = new Article[] {
                 new Article
                 {
                     Id = "1",
                     Title = "JSON API sample!",
                     Author = author,
                     Comments = new List<Comment>
                     {
                         new Comment
                         {
                             Id = "5",
                             Body = "First!",
                             Author = new Person
                             {
                                 Id = "2",
                                 Type="people",
                                 FirstName="test"
                             },
                         },
                         new Comment
                         {
                             Id = "12",
                             Body = "I like XML better",
                             Author =new Person
                             {
                                 Id = "12",
                                 Type="people",
                                 FirstName="test12"
                             },
                         }
                     }
                 },
                
         };
```

Include comments and author, this will not return the authors related to the comments
```C#
var settings = new JsonApiFormatSerializer(new string[] {"comments" ,"author"}, 
                                                      new string[] {  },
                                                      new string[] { "self:articles",
                                                                      "next:articles?page[offset]=2",
                                                                      "last:articles?page[offset]=10" });
settings.BaseUrl = "http://localhost/";
 //To serialize json:api format
string json_article = JsonConvert.SerializeObject(articles, settings);
```
JSON Output
```JSON
{  
   "links":{  
      "self":"http://localhost/articles",
      "next":"http://localhost/articles?page[offset]=2",
      "last":"http://localhost/articles?page[offset]=10"
   },
   "data":[  
      {  
         "type":"article",
         "id":"1",
         "attributes":{  
            "title":"JSON API sample!"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               },
               "links":{  
                  "self":"http://localhost/people/relationships/9/",
                  "related":"http://localhost/people/9/"
               }
            },
            "comments":{  
               "data":[  
                  {  
                     "type":"comment",
                     "id":"5"
                  },
                  {  
                     "type":"comment",
                     "id":"12"
                  }
               ],
               "links":{  
                  "self":"http://localhost/articles/1/relationships/comments",
                  "related":"http://localhost/articles/1/comments"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/"
         }
      }
   ],
   "included":[  
      {  
         "id":"9",
         "type":"people",
         "attributes":{  
            "first-name":"Dan",
            "last-name":"Gebhardt",
            "twitter":"dgeb"
         },
         "links":{  
            "self":"http://localhost/people/9"
         }
      },
      {  
         "type":"comment",
         "id":"5",
         "attributes":{  
            "body":"First!",
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"2"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/5"
         }
      },
      {  
         "type":"comment",
         "id":"12",
         "attributes":{  
            "body":"I like XML better",
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/12"
         }
      }
   ]
}
```
Include “CommentAuthor”, this will not include the author of the article
```C#
var settings = new JsonApiFormatSerializer(new string[] {"comments" , "CommentAuthor" }, 
                                                      new string[] {  },
                                                      new string[] { "self:articles",
                                                                      "next:articles?page[offset]=2",
                                                                      "last:articles?page[offset]=10" });
settings.BaseUrl = "http://localhost/";
//To serialize json:api format
string json_article = JsonConvert.SerializeObject(articles, settings);
```

JSON Output
```JSON
{  
   "links":{  
      "self":"http://localhost/articles",
      "next":"http://localhost/articles?page[offset]=2",
      "last":"http://localhost/articles?page[offset]=10"
   },
   "data":[  
      {  
         "type":"article",
         "id":"1",
         "attributes":{  
            "title":"JSON API sample!"
         },
         "relationships":{  
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"9"
               },
               "links":{  
                  "self":"http://localhost/people/relationships/9/",
                  "related":"http://localhost/people/9/"
               }
            },
            "comments":{  
               "data":[  
                  {  
                     "type":"comment",
                     "id":"5"
                  },
                  {  
                     "type":"comment",
                     "id":"12"
                  }
               ],
               "links":{  
                  "self":"http://localhost/articles/1/relationships/comments",
                  "related":"http://localhost/articles/1/comments"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/"
         }
      }
   ],
   "included":[  
      {  
         "type":"comment",
         "id":"5",
         "attributes":{  
            "body":"First!",
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"2"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/5"
         }
      },
      {  
         "type":"comment",
         "id":"12",
         "attributes":{  
            "body":"I like XML better",
            "author":{  
               "data":{  
                  "type":"people",
                  "id":"12"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/12"
         }
      },
      {  
         "id":"2",
         "type":"people",
         "attributes":{  
            "first-name":"test"
         },
         "links":{  
            "self":"http://localhost/people/2"
         }
      },
      {  
         "id":"12",
         "type":"people",
         "attributes":{  
            "first-name":"test12"
         },
         "links":{  
            "self":"http://localhost/people/12"
         }
      }
   ]
}
```
###### Sparse Fields

Similar to Includes, fields can be defined when instantiate the JsonApiFormatSerializer
If the fields are defined only those will be return

Ex: This will return title of the article and body of the comments as include
When defining field of the include, format should be “[Type of the class].[attribute name]” 

```C#
var settings = new JsonApiFormatSerializer(new string[] {"comments"  }, 
                         new string[] { "Title", "comments " ,"comment.body" },
                         new string[] { "self:articles",
                                         "next:articles?page[offset]=2",
                                         "last:articles?page[offset]=10" });
```                                      
JSON Output
```JSON 
{  
   "links":{  
      "self":"http://localhost/articles",
      "next":"http://localhost/articles?page[offset]=2",
      "last":"http://localhost/articles?page[offset]=10"
   },
   "data":[  
      {  
         "type":"article",
         "id":"1",
         "attributes":{  
            "title":"JSON API sample!"
         },
         "relationships":{  
            "comments":{  
               "data":[  
                  {  
                     "type":"comment",
                     "id":"5"
                  },
                  {  
                     "type":"comment",
                     "id":"12"
                  }
               ],
               "links":{  
                  "self":"http://localhost/articles/1/relationships/comments",
                  "related":"http://localhost/articles/1/comments"
               }
            }
         },
         "links":{  
            "self":"http://localhost/articles/1/"
         }
      }
   ],
   "included":[  
      {  
         "type":"comment",
         "id":"5",
         "attributes":{  
            "body":"First!"
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/5"
         }
      },
      {  
         "type":"comment",
         "id":"12",
         "attributes":{  
            "body":"I like XML better"
         },
         "links":{  
            "self":"http://localhost/articles/1/comments/12"
         }
      }
   ]
}
```
###### Derived class(Mixed Resource Type search)

If the class inherited from base class base on the derived class type attribute will be output
Models

```C#
   public class Client 
   {
       public int? Id { get; set; }
       public string ClientRef { get; set; }       
       public List<Contact> Contacts { get; set; }
   }
 
   public class CorporateClient:Client
   {
       public string CompanyName { get; set; }
   }
   public class IndividualClient : Client
   {
       public string PhoneNo { get; set; }
   }

   public class Contact 
   {
       public int? Id { get; set; }
       public string ContactRef { get; set; }
       public string FirstName { get; set; }
       public string Initial { get; set; }
       public string LastName { get; set; }
       public int? TitleId { get; set; }
       public string TitleName { get; set; }      
       public Client Client { get; set; }
 
   }
 

```
Sample Data
```C#
var corporate = new CorporateClient()
            {
                Id = 1,
                ClientRef = "CR001",
                CompanyName = "Test Company",
                Contacts = new List<Contact>()
                {
                    new Contact()
                    {
                        Id=12,
                        ContactRef="CN0012",
                        FirstName="FirstName12"
                    }
                }
            };
 
 
            var individual = new IndividualClient()
            {
                Id = 2,
                ClientRef = "IND002",
                PhoneNo = "07852639635"
 
            };
```
JSON Output
```Json
{  
   "links":{  
      "self":"http://localhost/clients"
   },
   "data":[  
      {  
         "type":"corporateclient",
         "id":1,
         "attributes":{  
            "companyName":"Test Company",
            "clientRef":"CR001"
         },
         "relationships":{  
            "contacts":{  
               "data":[  
                  {  
                     "type":"contact",
                     "id":12
                  }
               ]
            }
         }
      },
      {  
         "type":"individualclient",
         "id":2,
         "attributes":{  
            "phoneNo":"07852639635",
            "clientRef":"IND002"
         }
      }
   ]
}
```

