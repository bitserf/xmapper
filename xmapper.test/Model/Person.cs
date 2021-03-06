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
using System.Xml.Linq;

namespace XMapper.Test.Model
{
    internal class Person
    {
        public long? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsEnabled { get; set; }
        public Address Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public TimeSpan? TimeSinceLastLogin { get; set; }
        public List<ContactMethod> ContactMethods { get; set; }
        public List<XAttribute> CustomAttributes { get; set; }
        public List<string> CustomStringAttributes { get; set; }
        public List<int> CustomIntegerElements { get; set; }
    }
}
