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
using System.Linq;
using System.Xml.Linq;
using XMapper.Util;

namespace XMapper
{
    /// <summary>
    /// Represents a mapping of an XML element to a type.
    /// </summary>
    /// <typeparam name="TTarget">The type that this mapping will be associated with.</typeparam>
    public class ElementMapping<TTarget> : MappingBase, IElementMapping
    {
        #region Fields
        readonly Func<TTarget> _constructor;
        IAttributeMapping[] _attributes;
        IChildElementMapping[] _childElements;
        ITextContentMapping _textContent;
        ITextContentMapping[] _childTextElements;
        IDictionary<string, IDictionary<string, IAttributeMapping>> _attributesByNamespaceAndName;
        IDictionary<string, IDictionary<string, IChildElementMapping>> _childElementsByNamespaceAndName;
        IDictionary<string, IDictionary<string, ITextContentMapping>> _childTextElementsByNamespaceAndName;
        #endregion

        /// <summary>
        /// Creates a new XML element mapping.
        /// </summary>
        /// <param name="name">The XML element name.</param>
        public ElementMapping(XName name)
            : base(typeof(TTarget), name)
        {
            // HACK
            if (typeof(TTarget) != typeof(string))
                _constructor = ReflectionHelper.GetTypedConstructorDelegate<TTarget>();

            _attributes = NoAttributes;
            _childElements = NoChildElements;
        }

        public virtual object CreateInstance()
        {
            if (_constructor == null)
                throw new InvalidOperationException(string.Format("No constructor available for type {0}", typeof(TTarget)));

            return _constructor();
        }

        public override IAttributeMapping[] Attributes
        {
            get { return _attributes; }
            internal set
            {
                _attributes = value;
                _attributesByNamespaceAndName = BuildMappingLookupTableByNamespaceAndName(_attributes);
            }
        }

        public override IChildElementMapping[] ChildElements
        {
            get { return _childElements; }
            internal set
            {
                _childElements = value;
                _childElementsByNamespaceAndName = BuildMappingLookupTableByNamespaceAndName(_childElements);
            }
        }

        public override ITextContentMapping TextContent { get { return _textContent; } internal set { _textContent = value; } }

        public override ITextContentMapping[] ChildTextElements
        {
            get { return _childTextElements; }
            internal set
            {
                _childTextElements = value;
                _childTextElementsByNamespaceAndName = BuildMappingLookupTableByNamespaceAndName(_childTextElements);
            }
        }

        public IAttributeMapping TryFindAttributeMapping(string localName)
        {
            return TryFindAttributeMapping("", localName);
        }

        public IAttributeMapping TryFindAttributeMapping(string namespaceUri, string localName)
        {
            if (_attributesByNamespaceAndName == null)
                return null;

            IDictionary<string, IAttributeMapping> propertiesByName;
            if (!_attributesByNamespaceAndName.TryGetValue(namespaceUri, out propertiesByName))
                return null;

            IAttributeMapping attributeMapping;
            if (!propertiesByName.TryGetValue(localName, out attributeMapping))
                return null;

            return attributeMapping;
        }

        public IChildElementMapping TryFindChildElementMapping(string localName)
        {
            return TryFindChildElementMapping("", localName);
        }

        public IChildElementMapping TryFindChildElementMapping(string namespaceUri, string localName)
        {
            if (_childElementsByNamespaceAndName == null)
                return null;

            IDictionary<string, IChildElementMapping> childrenByName;
            if (!_childElementsByNamespaceAndName.TryGetValue(namespaceUri, out childrenByName))
                return null;

            IChildElementMapping childElementMapping;
            if (!childrenByName.TryGetValue(localName, out childElementMapping))
                return null;

            return childElementMapping;
        }

        public ITextContentMapping TryFindChildTextElementMapping(string localName)
        {
            return TryFindChildTextElementMapping("", localName);
        }

        public ITextContentMapping TryFindChildTextElementMapping(string namespaceUri, string localName)
        {
            if (_childTextElementsByNamespaceAndName == null)
                return null;

            IDictionary<string, ITextContentMapping> childrenByName;
            if (!_childTextElementsByNamespaceAndName.TryGetValue(namespaceUri, out childrenByName))
                return null;

            ITextContentMapping childTextElementMapping;
            if (!childrenByName.TryGetValue(localName, out childTextElementMapping))
                return null;

            return childTextElementMapping;
        }

        Dictionary<string, IDictionary<string, T>> BuildMappingLookupTableByNamespaceAndName<T>(IEnumerable<T> mappings)
            where T : IMapping
        {
            var mappingsByNamespace = from mapping in mappings
                                      let ns = mapping.NamespaceUri ?? ""
                                      group mapping by ns into g
                                      select new {Namespace = g.Key, Items = g};

            var mappingsByNamespaceAndName = new Dictionary<string, IDictionary<string, T>>();

            foreach (var mappingGrouping in mappingsByNamespace)
            {
                IDictionary<string, T> mappingsByName;
                if (!mappingsByNamespaceAndName.TryGetValue(mappingGrouping.Namespace, out mappingsByName))
                    mappingsByNamespaceAndName[mappingGrouping.Namespace] = mappingsByName = new Dictionary<string, T>();

                foreach (var groupedDescriptor in mappingGrouping.Items)
                {
                    if (mappingsByName.ContainsKey(groupedDescriptor.LocalName))
                        throw new ArgumentException(string.Format("'{0}' contains multiple mappings with name '{1}'", LocalName, groupedDescriptor.LocalName));
                    mappingsByName[groupedDescriptor.LocalName] = groupedDescriptor;
                }
            }

            return mappingsByNamespaceAndName;
        }
    }
}
