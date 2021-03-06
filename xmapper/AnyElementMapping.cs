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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;

namespace XMapper
{
    /// <summary>
    /// Represents a mapping of an XML any element to a collection object property. That is, any XML element
    /// that is not explicitly declared to be supported will be added to the collection on the specified property. 
    /// </summary>
    /// <typeparam name="TContainer">The type that contains the property this mapping is associated with.</typeparam>
    public class AnyElementMapping<TContainer> : AnyMappingBase<TContainer, XElement>, IAnyElementMapping
    {
        /// <summary>
        /// Creates a new XML attribute mapping for custom attributes.
        /// </summary>
        /// <param name="propertyExpression">A simple member expression referencing the property to associate this mapping with.</param>
        public AnyElementMapping(Expression<Func<TContainer, IList<XElement>>> propertyExpression)
            : base(propertyExpression)
        {
        }

        public object DeserializeElement(XmlReader reader)
        {
            return XElement.Load(reader.ReadSubtree());
        }

        public void SerializeElement(XmlWriter writer, object element)
        {
            ((XElement)element).WriteTo(writer);
        }

        public IList GetElements(object target)
        {
            return GetCustomNodeList(target);
        }

        public void AddToElements(object target, object element)
        {
            AddToCustomNodeList(target, (XElement)element);
        }
    }

    /// <summary>
    /// Represents a mapping of an XML any element to a collection object property. That is, any XML element
    /// that is not explicitly declared to be supported will be added to the collection on the specified property.
    /// </summary>
    /// <typeparam name="TContainer">The type that contains the property this mapping is associated with.</typeparam>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    public class AnyElementMapping<TContainer, TElement> : AnyMappingBase<TContainer, TElement>, IAnyElementMapping
    {
        #region Fields
        readonly Func<XmlReader, TElement> _customDeserializer;
        readonly Action<XmlWriter, TElement> _customSerializer;
        #endregion

        /// <summary>
        /// Creates a new XML attribute mapping for custom attributes.
        /// </summary>
        /// <param name="propertyExpression">A simple member expression referencing the property to associate this mapping with.</param>
        /// <param name="customDeserializer">Custom deserialization function.</param>
        /// <param name="customSerializer">Custom serialization function.</param>
        public AnyElementMapping(Expression<Func<TContainer, IList<TElement>>> propertyExpression,
                                 Func<XmlReader, TElement> customDeserializer,
                                 Action<XmlWriter, TElement> customSerializer)
            : base(propertyExpression)
        {
            if (customDeserializer == null)
                throw new ArgumentNullException("customDeserializer");
            if (customSerializer == null)
                throw new ArgumentNullException("customSerializer");

            _customDeserializer = customDeserializer;
            _customSerializer = customSerializer;
        }

        public object DeserializeElement(XmlReader reader)
        {
            return _customDeserializer(reader);
        }

        public void SerializeElement(XmlWriter writer, object element)
        {
            _customSerializer(writer, (TElement)element);
        }

        public IList GetElements(object target)
        {
            return GetCustomNodeList(target);
        }

        public void AddToElements(object target, object element)
        {
            AddToCustomNodeList(target, (TElement)element);
        }
    }
}