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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectGraph.Test.Xml.Model;
using ObjectGraph.Xml;
using Shouldly;

namespace ObjectGraph.Test.Xml
{
    [TestClass]
    public class ItemCollectionTest
    {
        [TestMethod]
        public void ItemWithIdAdded_IsTracked()
        {
            var coll = new ItemCollection<ItemWithId>();
            var expected = new ItemWithId {Id = "abc"};

            coll.Add(expected);

            coll["abc"].ShouldBe(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ItemWithIdRemoved_IsNoLongerTracked()
        {
            var coll = new ItemCollection<ItemWithId>();
            var expected = new ItemWithId {Id = "abc"};

            coll.Add(expected);
            coll.Remove(expected);

            var notReached = coll["abc"];
        }
    }
}