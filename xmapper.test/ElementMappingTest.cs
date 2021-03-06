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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using XMapper.Test.Model;

namespace XMapper.Test
{
    [TestClass]
    public class ElementMappingTest : TestBase
    {
        [TestMethod]
        public void NewMapping_ShouldCreateInstances()
        {
            var mapping = new ElementMapping<Person>(Ns + "Person");

            mapping.NamespaceUri.ShouldBe(Ns.NamespaceName);
            mapping.LocalName.ShouldBe("Person");

            var actual = mapping.CreateInstance();

            actual.ShouldBeTypeOf(typeof(Person));
        }
    }
}
