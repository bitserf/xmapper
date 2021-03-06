﻿//
// Copyright (C) 2010-2012 Leon Breedt
// ljb -at- bitserf [dot] org
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.using System;
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using XMapper.Fluent;
using XMapper.Test.Model;

namespace XMapper.Test
{
    [TestClass]
    public class SerializerTest : TestBase
    {
        [TestMethod]
        [ExpectedException(typeof(XmlFormatException))]
        public void DeserializeFragment_InvalidLocalName_ShouldFail()
        {
            const string fragment = @"<Invalid StreetName='231 Queen Street' City='Auckland' />";
            var serializer = new Serializer(FullSchema());

            serializer.Deserialize<Address>(fragment.ToStream());
        }

        [TestMethod]
        [ExpectedException(typeof(XmlFormatException))]
        public void DeserializeFragment_InvalidNamespace_ShouldFail()
        {
            const string fragment = @"<Address StreetName='231 Queen Street' City='Auckland' />";
            var serializer = new Serializer(FullSchema());

            serializer.Deserialize<Address>(fragment.ToStream());
        }

        [TestMethod]
        public void DeserializeFragment_ShouldSucceed()
        {
            const string fragment = @"<Address xmlns='http://test.com' StreetName='231 Queen Street' City='Auckland' />";
            var serializer = new Serializer(FullSchema());

            var actual = serializer.Deserialize<Address>(fragment.ToStream());

            actual.StreetName.ShouldBe("231 Queen Street");
            actual.City.ShouldBe("Auckland");
        }

        [TestMethod]
        public void DeserializeDocument_ShouldSucceed()
        {
            const string document = @"<Document xmlns='http://test.com' xmlns:dms='http://dms.com'>
                                        <dms:CustomElement1>
                                            <dms:Child1 />
                                            <dms:Child2 Attr1='Value1' />
                                        </dms:CustomElement1>
                                        <Person Id='123' FirstName='James' LastName='Jefferson' DateOfBirth='2010-02-12T23:59:59Z' TimeSinceLastLogin='00:20:00' Custom1='CustomValue1' dms:Custom2='CustomValue2'>
                                          <IsEnabled>true</IsEnabled>
                                          <Address StreetName='231 Queen Street' City='Auckland'>Some comments</Address>
                                          <ContactMethods>
                                              <ContactMethod Type='Email' Value='james@jefferson.com' />
                                              <AddressContactMethod Type='Address' Value='Auckland City' StreetName='232 Queen Street' />
                                              <ContactMethod Type='HomePhone' OptionalType='HomePhone' Value='555-1234' />
                                          </ContactMethods>
                                        </Person>
                                        <CustomElement2 Name='Value2' />
                                        <Person Id='124' FirstName='Paul' LastName='Jefferson' IsEnabled='false'>
                                          <Address StreetName='500 Dominion Road' City='Auckland' />
                                        </Person>
                                        <CustomElement3 Name='Value3' />
                                      </Document>";
            var serializer = new Serializer(FullSchema());

            var actual = serializer.Deserialize<Document>(document.ToStream());

            actual.Persons.Count.ShouldBe(2);
            actual.CustomElements.Count.ShouldBe(3);

            actual.CustomElements[0].Name.Namespace.NamespaceName.ShouldBe("http://dms.com");
            actual.CustomElements[0].Name.LocalName.ShouldBe("CustomElement1");
            actual.CustomElements[0].Elements().Count().ShouldBe(2);
            actual.CustomElements[0].Elements().ElementAt(0).Name.LocalName.ShouldBe("Child1");
            actual.CustomElements[0].Elements().ElementAt(1).Name.LocalName.ShouldBe("Child2");
            actual.CustomElements[0].Elements().ElementAt(1).Attributes().First().Value.ShouldBe("Value1");
            actual.CustomElements[1].Name.Namespace.NamespaceName.ShouldBe("http://test.com");
            actual.CustomElements[1].Name.LocalName.ShouldBe("CustomElement2");
            actual.CustomElements[2].Name.Namespace.NamespaceName.ShouldBe("http://test.com");
            actual.CustomElements[2].Name.LocalName.ShouldBe("CustomElement3");

            var person1 = actual.Persons[0];
            var person2 = actual.Persons[1];

            person1.Id.ShouldBe(123);
            person1.DateOfBirth.HasValue.ShouldBe(true);
            person1.DateOfBirth.Value.Kind.ShouldBe(DateTimeKind.Utc);
            person1.DateOfBirth.ShouldBe(new DateTime(2010, 02, 12, 23, 59, 59));
            person1.TimeSinceLastLogin.ShouldBe(TimeSpan.FromMinutes(20));
            person1.IsEnabled.ShouldBe(true);
            person1.Address.StreetName.ShouldBe("231 Queen Street");
            person1.Address.City.ShouldBe("Auckland");
            person1.Address.Comments.ShouldBe("Some comments");
            person1.ContactMethods.Count.ShouldBe(3);
            person1.ContactMethods[0].Type.ShouldBe(ContactMethodType.Email);
            person1.ContactMethods[0].OptionalType.ShouldBe(null);
            person1.ContactMethods[0].Value.ShouldBe("james@jefferson.com");
            person1.ContactMethods[1].Type.ShouldBe(ContactMethodType.Address);
            person1.ContactMethods[1].Value.ShouldBe("Auckland City");
            person1.ContactMethods[1].ShouldBeTypeOf(typeof(AddressContactMethod));
            person1.ContactMethods[1].As<AddressContactMethod>().StreetName.ShouldBe("232 Queen Street");
            person1.ContactMethods[2].Type.ShouldBe(ContactMethodType.HomePhone);
            person1.ContactMethods[2].OptionalType.ShouldBe(ContactMethodType.HomePhone);
            person1.ContactMethods[2].Value.ShouldBe("555-1234");
            person1.CustomAttributes.Count.ShouldBe(2);
            person1.CustomAttributes[0].Name.Namespace.NamespaceName.ShouldBe("");
            person1.CustomAttributes[0].Name.LocalName.ShouldBe("Custom1");
            person1.CustomAttributes[0].Value.ShouldBe("CustomValue1");
            person1.CustomAttributes[1].Name.Namespace.NamespaceName.ShouldBe("http://dms.com");
            person1.CustomAttributes[1].Name.LocalName.ShouldBe("Custom2");
            person1.CustomAttributes[1].Value.ShouldBe("CustomValue2");

            person2.Id.ShouldBe(124);
            person2.FirstName.ShouldBe("Paul");
            person2.LastName.ShouldBe("Jefferson");
            person2.IsEnabled.ShouldBe(false);
            person2.Address.StreetName.ShouldBe("500 Dominion Road");
            person2.Address.City.ShouldBe("Auckland");
        }

        [TestMethod]
        public void SerializeFragment_ShouldSucceed()
        {
            var stream = new MemoryStream();
            var address = new Address {StreetName = "231 Queen Street", City = "Auckland"};
            var serializer = new Serializer(FullSchema());

            serializer.Serialize(stream, address);

            var expected = XDocument.Parse(@"<Address StreetName='231 Queen Street' City='Auckland' xmlns='http://test.com' />");

            XAssert.AreEqual(expected, stream.ToXDocument());
        }

        [TestMethod]
        public void SerializeDocument_ShouldSucceed()
        {
            var stream = new MemoryStream();
            XNamespace dmsNs = "http://dms.com";
            XNamespace testNs = "http://test.com";
            var document = new Document
                           {
                               CustomElements = new List<XElement>
                                   {
                                       new XElement(dmsNs + "CustomElement1",
                                           new XElement(dmsNs + "Child1"),
                                           new XElement(dmsNs + "Child2",
                                               new XAttribute("Attr1", "Value1"))),
                                       new XElement(testNs + "CustomElement2",
                                           new XAttribute("Name", "Value2")),
                                       new XElement(testNs + "CustomElement3",
                                           new XAttribute("Name", "Value3")),
                                   },
                               Persons = new List<Person>
                                         {
                                             new Person
                                             {
                                                 Id = 123,
                                                 FirstName = "James",
                                                 LastName = "Jefferson",
                                                 IsEnabled = true,
                                                 DateOfBirth = new DateTime(2010, 02, 12, 23, 59, 59, DateTimeKind.Utc),
                                                 TimeSinceLastLogin = TimeSpan.FromMinutes(20),
                                                 Address = new Address {StreetName = "231 Queen Street", City = "Auckland", Comments = "Some comments"},
                                                 CustomAttributes = 
                                                    new List<XAttribute>
                                                        {
                                                            new XAttribute("Custom1", "CustomValue1"),
                                                            new XAttribute(dmsNs + "Custom2", "CustomValue2")
                                                        },
                                                 ContactMethods =
                                                     new List<ContactMethod>
                                                     {
                                                         new ContactMethod {Type = ContactMethodType.Email, Value = "james@jefferson.com"},
                                                         new AddressContactMethod
                                                         {
                                                             Type = ContactMethodType.Address,
                                                             OptionalType = ContactMethodType.Email,
                                                             Value = "Auckland City",
                                                             StreetName = "232 Queen Street"
                                                         },
                                                         new ContactMethod {Type = ContactMethodType.HomePhone, Value = "555-1234"}
                                                     }
                                             },
                                             new Person
                                             {
                                                 Id = 124,
                                                 FirstName = "Paul",
                                                 LastName = "Jefferson",
                                                 IsEnabled = false,
                                                 Address = new Address {StreetName = "500 Dominion Road", City = "Auckland"},
                                             }
                                         }
                           };
            var serializer = new Serializer(FullSchema());

            serializer.Serialize(stream, document);

            var expected = XDocument.Parse(@"<Document xmlns='http://test.com' xmlns:dms='http://dms.com'>
                                              <Person Id='123' FirstName='James' LastName='Jefferson' DateOfBirth='2010-02-12T23:59:59Z' TimeSinceLastLogin='00:20:00'  Custom1='CustomValue1' dms:Custom2='CustomValue2'>
                                                <IsEnabled>true</IsEnabled>
                                                <Address StreetName='231 Queen Street' City='Auckland'>Some comments</Address>
                                                <ContactMethods>
                                                    <ContactMethod Type='Email' Value='james@jefferson.com' />
                                                    <AddressContactMethod Type='Address' OptionalType='Email' Value='Auckland City' StreetName='232 Queen Street' />
                                                    <ContactMethod Type='HomePhone' Value='555-1234' />
                                                </ContactMethods>
                                              </Person>
                                              <Person Id='124' FirstName='Paul' LastName='Jefferson'>
                                                <IsEnabled>false</IsEnabled>
                                                <Address StreetName='500 Dominion Road' City='Auckland' />
                                              </Person>
                                              <dms:CustomElement1>
                                                <dms:Child1 />
                                                <dms:Child2 Attr1='Value1' />
                                              </dms:CustomElement1>
                                              <CustomElement2 Name='Value2' />
                                              <CustomElement3 Name='Value3' />
                                            </Document>");

            Debug.WriteLine(stream.ToXDocument());

            XAssert.AreEqual(expected, stream.ToXDocument());
        }

        [TestMethod]
        public void CustomAnyAttributeSerializer_ShouldDeserializeToCustomValue()
        {
            var description = new FluentSchemaDescription();
            description.Element<Person>("Person")
                       .AnyAttribute<string>(x => x.CustomStringAttributes,
                                             reader => "Attribute:" + reader.LocalName + "=" + reader.Value,
                                             (writer, attr) => writer.WriteAttributeString("Oops", "Lost Information: " + attr));

            var schema = description.Build();

            var serializer = new Serializer(schema);

            var person = serializer.Deserialize<Person>("<Person Custom1='Value1' Custom2='Value2' />".ToStream());

            person.CustomStringAttributes.Count.ShouldBe(2);
            person.CustomStringAttributes[0].ShouldBe("Attribute:Custom1=Value1");
            person.CustomStringAttributes[1].ShouldBe("Attribute:Custom2=Value2");
        }

        [TestMethod]
        public void CustomAnyAttributeSerializer_ShouldSerializeToCustomValue()
        {
            int i = 0;
            var description = new FluentSchemaDescription();
            description.Element<Person>("Person")
                       .AnyAttribute<string>(x => x.CustomStringAttributes,
                                             reader => "Attribute:" + reader.LocalName + "=" + reader.Value,
                                             (writer, attr) => writer.WriteAttributeString("CustomAttr" + (++i), "MissingValue"));

            var schema = description.Build();

            var serializer = new Serializer(schema);

            var person = serializer.Deserialize<Person>("<Person Custom1='Value1' Custom2='Value2' />".ToStream());
            var stream = new MemoryStream();
            serializer.Serialize(stream, person);

            XAssert.AreEqual("<Person CustomAttr1='MissingValue' CustomAttr2='MissingValue' />",
                             stream.ToXDocument().ToString());
        }

        [TestMethod]
        public void CustomAnyElementSerializer_ShouldDeserializeToCustomValue()
        {
            var description = new FluentSchemaDescription();
            description.Element<Person>("Person")
                       .AnyElement<int>(x => x.CustomIntegerElements,
                                             reader => reader.ReadElementContentAsInt(),
                                             (writer, value) => writer.WriteElementString("NewName", value.ToString()));

            var schema = description.Build();

            var serializer = new Serializer(schema);

            var person = serializer.Deserialize<Person>(
                @"<Person>
                    <Custom1>34</Custom1>
                    <Custom2>35</Custom2>
                  </Person>".ToStream());

            person.CustomIntegerElements.Count.ShouldBe(2);
            person.CustomIntegerElements[0].ShouldBe(34);
            person.CustomIntegerElements[1].ShouldBe(35);
        }

        [TestMethod]
        public void CustomAnyElementSerializer_ShouldSerializeToCustomValue()
        {
            var description = new FluentSchemaDescription();
            description.Element<Person>("Person")
                       .AnyElement<int>(x => x.CustomIntegerElements,
                                             reader => reader.ReadElementContentAsInt(),
                                             (writer, value) => writer.WriteElementString("NewName", value.ToString()));

            var schema = description.Build();

            var serializer = new Serializer(schema);

            var person = new Person {CustomIntegerElements = new List<int> {1, 2, 3}};
            var stream = new MemoryStream();
            serializer.Serialize(stream, person);

            XAssert.AreEqual(@"<Person>
                                 <NewName>1</NewName>
                                 <NewName>2</NewName>
                                 <NewName>3</NewName>
                               </Person>",
                             stream.ToXDocument().ToString());
        }

        internal static SchemaDescription FullSchema()
        {
            var description = new FluentSchemaDescription();

            description.Element<Document>(Ns + "Document")
                           .AnyElement(x => x.CustomElements)
                           .CollectionElement(Ns + "Person", x => x.Persons)
                               .Attribute("Id", x => x.Id)
                               .Attribute("FirstName", x => x.FirstName)
                               .Attribute("LastName", x => x.LastName)
                               .Attribute("DateOfBirth", x => x.DateOfBirth)
                               .Attribute("TimeSinceLastLogin", x => x.TimeSinceLastLogin, 
                                                                x => x != null ? TimeSpan.Parse(x) : (TimeSpan?)null, 
                                                                x => x != null ? x.ToString() : (string)null)
                               .AnyAttribute(x => x.CustomAttributes)
                               .TextElement(Ns + "IsEnabled", x => x.IsEnabled)
                               .Element(Ns + "Address", x => x.Address)
                                   .Attribute("StreetName", x => x.StreetName)
                                   .Attribute("City", x => x.City)
                                   .TextContent(x => x.Comments)
                               .EndElement()
                               .Element(Ns + "ContactMethods", x => x.ContactMethods)
                                   .CollectionElement<ContactMethod>(Ns + "ContactMethod")
                                       .Attribute("Type", x => x.Type)
                                       .Attribute("OptionalType", x => x.OptionalType)
                                       .Attribute("Value", x => x.Value)
                                   .EndElement()
                                   .CollectionElement<AddressContactMethod>(Ns + "AddressContactMethod")
                                       .Attribute("Type", x => x.Type)
                                       .Attribute("OptionalType", x => x.OptionalType)
                                       .Attribute("Value", x => x.Value)
                                       .Attribute("StreetName", x => x.StreetName)
                                   .EndElement()
                               .EndElement()
                           .EndElement();

            return description.Build();
        }
    }
}
