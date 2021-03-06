xmapper
=======

This library is an extraction of an approach I have been using with some success to process quite
large XML documents at speed.

It also includes a fluent interface for defining mappings from XML attributes and elements to .NET objects,
which should make consuming and producing arbitrary XML documents pretty easy and quick.


Features
--------

* Don't have to write your own boilerplate to convert from XML DOM (XmlDocument/XDocument) or XMLReader into
  your .NET model objects. Just describe your mapping from XML onto model objects using the fluent interface,
  and then use the Serializer class to read or write the XML.

* Performance is pretty decent. Specifically, we do not do any reflection at all during serialization or
  deserialization. Instead, we use strongly typed delegates bound to our model object properties to get and set
  property values, and pre-cache those at the time of you describing your mapping.

* Memory usage should be good. We never load the entire document into memory, since we use XMLReader to 
  traverse the document, and we traverse it only once. Additionally, we only load from XML into .NET model
  objects the data that you've described in your mapping, and ignore everything else.

* If you keep your object model simple, there is no reason you can't use the same classes to serialize to
  any other output format. I have done this with [protobuf-net](http://code.google.com/p/protobuf-net).


Unsupported Features
--------------------

* XML schemas where an element contains itself, to arbitrary levels of depth, e.g. for representing
  general purpose tree structures. You could probably hack it by duplicating the same child a couple of
  times using the fluent interface, but it will be ugly. Not quite sure how I want to represent this
  with mappings.
  

Examples (C#)
=============


Simple object with nested object
--------------------------------

XML:

    <Person Id='123' FirstName='John'>
        <LastName>Doe</LastName>
        <Address StreetName='32 Quay Street' City='Auckland'>1010</Address>
    </Person>

C#:

    class Person
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address Address { get; set; }
    }

    class Address
    {
        public string StreetName { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
    }

Mapping:
    
    var desc = new FluentSchemaDescription();

    desc.Element<Person>("Person")
            .Attribute("Id", x => x.Id)
            .Attribute("FirstName", x => x.FirstName)
            .TextElement("LastName", x => x.LastName)
            .Element("Address", x => x.Address)
                .Attribute("StreetName", x => x.StreetName)
                .Attribute("City", x => x.City)
                .TextContent(x => x.PostCode)
            .EndElement()


Simple object with two styles of child collection
-------------------------------------------------

XML:

    <Export JobId='15'>
        <Person Id='123' FirstName='John' LastName='Doe'>
            <Address StreetName='32 Quay Street' City='Auckland' />
            <Contacts>
                <Contact Type="Email" Value="john.doe@test.com" />
                <Contact Type="Phone" Value="555-1234" />
            </Contacts>
        </Person>
        <Person Id='123' FirstName='John' LastName='Doe'>
            <Address StreetName='32 Quay Street' City='Auckland' />
        </Person>
    </Export>

C#:

    class Export
    {
        public long JobId { get; set; }
        public List<Person> Persons { get;set; }
    }

    class Person
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address Address { get; set; }
        public List<Contact> Contacts { get; set; }
    }

    class Address
    {
        public string StreetName { get; set; }
        public string City { get; set; }
    }

    enum ContactType { Email, Phone }

    class Contact
    {
        public ContactType Type { get; set; }
        public string Value { get; set; }
    }

Mapping:
    
    var desc = new FluentSchemaDescription();

    desc.Element<Export>("Export")
            .Attribute("JobId", x => x.JobId)
            .CollectionElement("Person", x => x.Persons)
                .Attribute("Id", x => x.Id)
                .Attribute("FirstName", x => x.FirstName)
                .Attribute("LastName", x => x.LastName)
                .Element("Address", x => x.Address)
                    .Attribute("StreetName", x => x.StreetName)
                    .Attribute("City", x => x.City)
                .EndElement()
                .Element("Contacts", x => x.Contacts)
                    .CollectionElement<Contact>("Contact")
                        .Attribute("Type", x => x.StreetName)
                        .Attribute("Value", x => x.City)
                    .EndElement()
                .EndElement()
            .EndElement()