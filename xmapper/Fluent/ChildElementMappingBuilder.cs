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
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;

namespace XMapper.Fluent
{
    internal class ChildElementMappingBuilder<TContainer, TElement, TParentBuilder> : IChildElementMappingBuilder<TElement, TParentBuilder>
    {
        #region Fields
        readonly XName _name;
        readonly TParentBuilder _parentBuilderScope;
        readonly Expression<Func<TContainer, TElement>> _propertyInParent;
        readonly List<IAttributeMapping> _attrs;
        IAnyAttributeMapping _anyAttr;
        readonly List<Func<IChildElementMapping>> _elements;
        IAnyElementMapping _anyElement;
        ITextContentMapping _textContent;
        readonly List<ITextContentMapping> _childTextElements;
        #endregion

        public ChildElementMappingBuilder(TParentBuilder parentBuilderScope, XName name, Expression<Func<TContainer, TElement>> propertyInParent)
        {
            _name = name;
            _parentBuilderScope = parentBuilderScope;
            _propertyInParent = propertyInParent;
            _attrs = new List<IAttributeMapping>();
            _elements = new List<Func<IChildElementMapping>>();
            _childTextElements = new List<ITextContentMapping>();
        }

        public virtual IChildElementMapping Build()
        {
            return new ChildElementMapping<TContainer, TElement>(_name, _propertyInParent)
                   {
                       Attributes = _attrs.ToArray(),
                       AnyAttribute = _anyAttr,
                       ChildElements = _elements.Select(f => f()).ToArray(),
                       AnyChildElement = _anyElement,
                       TextContent = _textContent,
                       ChildTextElements = _childTextElements.ToArray(),
                   };
        }

        public IChildElementMappingBuilder<TElement, TParentBuilder> Attribute<TProperty>(XName name, Expression<Func<TElement, TProperty>> property)
        {
            _attrs.Add(new AttributeMapping<TElement, TProperty>(name, property));
            return this;
        }

        public IChildElementMappingBuilder<TElement, TParentBuilder> Attribute<TProperty>(XName name, Expression<Func<TElement, TProperty>> property, Func<string, TProperty> customDeserializer, Func<TProperty, string> customSerializer)
        {
            _attrs.Add(new AttributeMapping<TElement, TProperty>(name, property, customDeserializer, customSerializer));
            return this;
        }

        public IChildElementMappingBuilder<TElement, TParentBuilder> AnyAttribute(Expression<Func<TElement, IList<XAttribute>>> attributeProperty)
        {
            if (_anyAttr != null)
                throw new ArgumentException("Only one AnyAttribute() is allowed for a particular element.");
            _anyAttr = new AnyAttributeMapping<TElement>(attributeProperty);
            return this;
        }

        public IChildElementMappingBuilder<TElement, TParentBuilder> AnyAttribute<TAttribute>(Expression<Func<TElement, IList<TAttribute>>> attributeProperty,
                                                                                              Func<XmlReader, TAttribute> customDeserializer,
                                                                                              Action<XmlWriter, TAttribute> customSerializer)
        {
            if (_anyAttr != null)
                throw new ArgumentException("Only one AnyAttribute() is allowed for a particular element.");
            _anyAttr = new AnyAttributeMapping<TElement, TAttribute>(attributeProperty,
                                                                     customDeserializer,
                                                                     customSerializer);
            return this;
        }

        public IChildElementMappingBuilder<TElement, TParentBuilder> TextContent<TProperty>(Expression<Func<TElement, TProperty>> property)
        {
            if (_textContent != null)
                throw new ArgumentException("Only one TextContent() is allowed for a particular element.");
            _textContent = new TextContentMapping<TElement, TProperty>(property);
            return this;
        }

        public IChildElementMappingBuilder<TElement, TParentBuilder> TextElement<TChildElement>(XName name, Expression<Func<TElement, TChildElement>> property)
        {
            _childTextElements.Add(new TextContentMapping<TElement, TChildElement>(name, property));
            return this;
        }

        public IChildElementMappingBuilder<TChildElement, IChildElementMappingBuilder<TElement, TParentBuilder>> Element<TChildElement>(XName name, Expression<Func<TElement, TChildElement>> propertyInParent)
        {
            var builder = new ChildElementMappingBuilder<TElement, TChildElement, IChildElementMappingBuilder<TElement, TParentBuilder>>(this, name, propertyInParent);
            _elements.Add(builder.Build);
            return builder;
        }

        public IChildElementMappingBuilder<TElement, TParentBuilder> AnyElement(Expression<Func<TElement, IList<XElement>>> customElementsProperty)
        {
            if (_anyElement != null)
                throw new ArgumentException("Only one AnyElement() is allowed for a particular element.");
            _anyElement = new AnyElementMapping<TElement>(customElementsProperty);
            return this;
        }

        public IChildElementMappingBuilder<TElement, TParentBuilder> AnyElement<TCustomElement>(Expression<Func<TElement, IList<TCustomElement>>> customElementsProperty, Func<XmlReader, TCustomElement> customDeserializer, Action<XmlWriter, TCustomElement> customSerializer)
        {
            if (_anyElement != null)
                throw new ArgumentException("Only one AnyElement() is allowed for a particular element.");
            _anyElement = new AnyElementMapping<TElement, TCustomElement>(customElementsProperty,
                                                                          customDeserializer,
                                                                          customSerializer);
            return this;
        }

        public ICollectionChildElementMappingBuilder<TElement, TChildElement, IChildElementMappingBuilder<TElement, TParentBuilder>> CollectionElement<TChildElement>(XName name, Expression<Func<TElement, IList<TChildElement>>> propertyInParent)
        {
            var builder = new CollectionChildElementMappingBuilder<TElement, TChildElement, IChildElementMappingBuilder<TElement, TParentBuilder>>(this, name, propertyInParent);
            _elements.Add(builder.Build);
            return builder;
        }

        public ICollectionChildElementMappingBuilder<TElement, TChildElement, IChildElementMappingBuilder<TElement, TParentBuilder>> CollectionElement<TChildElement>(XName name)
        {
            var builder = new CollectionChildElementMappingBuilder<TElement, TChildElement, IChildElementMappingBuilder<TElement, TParentBuilder>>(this, name);
            _elements.Add(builder.Build);
            return builder;
        }

        public TParentBuilder EndElement()
        {
            return _parentBuilderScope;
        }
    }
}