﻿//
// Copyright (C) 2010 Leon Breedt
// bitserf -at- gmail [dot] com
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
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectGraph.Xml;

namespace ObjectGraph.Test.Xml
{
    [TestClass]
    public class TypeSerializerTest
    {
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void NoDataContractAttributeThrowsException()
        {
            TypeSerializer.Build<MissingDataContract>();
        }

        [TestMethod]
        public void NameDefaultsToTypeName()
        {
            var serializer = TypeSerializer.Build<WithDataContract>();

            Assert.AreEqual("WithDataContract", serializer.Name);
        }

        [TestMethod]
        public void OverriddenNameIsUsed()
        {
            var serializer = TypeSerializer.Build<WithName>();

            Assert.AreEqual("NewName", serializer.Name);
        }

        [TestMethod]
        public void SerializerIsReusedForSameType()
        {
            var serializer1 = TypeSerializer.Build<WithDataContract>();
            var serializer2 = TypeSerializer.Build<WithDataContract>();
            var serializer3 = TypeSerializer.Build<WithDataContract>();
            var serializer4 = TypeSerializer.Build<WithName>();

            Assert.AreSame(serializer1, serializer2);
            Assert.AreSame(serializer1, serializer3);
            Assert.AreNotSame(serializer1, serializer4);
        }

        [TestMethod]
        public void PropertySerializersAreBuiltForAnnotatedPublicProperties()
        {
            var serializer = TypeSerializer.Build<WithProperties>();

            Assert.AreEqual(2, serializer.Properties.Count());
        }

        #region Helpers
        class MissingDataContract
        {
        }

        [DataContract]
        class WithDataContract
        {
        }

        [DataContract(Name="NewName")]
        class WithName
        {
        }

        [DataContract]
        class WithProperties
        {
            [DataMember]
            public string FirstName { get; set; }
            [DataMember]
            public string LastName { get; set; }

            public int Age { get; set; }
            bool IsPrivate { get; set; }
        }
        #endregion
    }
}
