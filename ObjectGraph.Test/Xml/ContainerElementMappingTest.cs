﻿//
// Copyright (C) 2010-2011 Leon Breedt
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectGraph.Test.Xml.Model;
using ObjectGraph.Xml;
using Shouldly;

namespace ObjectGraph.Test.Xml
{
    [TestClass]
    public class ContainerElementMappingTest : TestBase
    {
        [TestMethod]
        public void GetCollectionFromTarget_ShouldReadFromContainer()
        {
            var person = new Person {ContactMethods = new ItemCollection<ContactMethod>()};
            var mapping = new ContainerElementMapping<Person, ContactMethod>(Ns + "ContactMethods", x => x.ContactMethods);

            mapping.NamespaceUri.ShouldBe(Ns.NamespaceName);
            mapping.LocalName.ShouldBe("ContactMethods");
            mapping.GetCollectionFromTarget(person).ShouldBe(person.ContactMethods);
        }

        [TestMethod]
        public void SetCollectionOnTarget_ShouldWriteToContainer()
        {
            var person = new Person();
            var expected = new ItemCollection<ContactMethod>();
            var mapping = new ContainerElementMapping<Person, ContactMethod>(Ns + "ContactMethods", x => x.ContactMethods);

            mapping.SetCollectionOnTarget(person, expected);
            mapping.GetCollectionFromTarget(person).ShouldBe(expected);
        }

        [TestMethod]
        public void CreateInstance_ShouldCreateInstanceOfPropertyType()
        {
            var mapping = new ContainerElementMapping<Person, ContactMethod>(Ns + "ContactMethods", x => x.ContactMethods);

            var actual = mapping.CreateInstance();

            actual.ShouldBeTypeOf(typeof(ItemCollection<ContactMethod>));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void SetCollectionOnTarget_PropertyTypeMismatch_ShouldThrowCastException()
        {
            var person = new Person();
            var mapping = new ContainerElementMapping<Person, ContactMethod>(Ns + "ContactMethods", x => x.ContactMethods);

            mapping.SetCollectionOnTarget(person, new List<ContactMethod>());
        }
    }
}
