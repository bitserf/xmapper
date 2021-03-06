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
using System.Reflection;
using System.Xml.Linq;
using XMapper.Util;

namespace XMapper
{
    /// <summary>
    /// Represents a mapping of an XML child element to a collection. Each instance of the child element encountered will be
    /// added to the collection.
    /// </summary>
    /// <typeparam name="TContainer">The type that contains the collection that will be read and written, or the collection itself.</typeparam>
    /// <typeparam name="TMember">The type of a collection member.</typeparam>
    public class CollectionChildElementMapping<TContainer, TMember> : ElementMapping<TMember>, ICollectionChildElementMapping
    {
        #region Fields
        readonly PropertyInfo _propertyInfo;
        readonly Func<IList<TMember>> _collectionConstructor;
        readonly Func<TContainer, IList<TMember>> _collectionGetter;
        readonly Action<TContainer, IList<TMember>> _collectionSetter;
        #endregion

        /// <summary>
        /// Creates a new child element mapping.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="propertyExpression">A simple member expression referencing the collection property that will be read from and written to. If <c>null</c>,
        /// the <typeparamref name="TContainer"/> type is assumed to be the collection itself.</param>
        public CollectionChildElementMapping(XName name, Expression<Func<TContainer, IList<TMember>>> propertyExpression)
            : base(name)
        {
            if (propertyExpression != null)
            {
                _propertyInfo = ReflectionHelper.GetPropertyInfoFromExpression(propertyExpression);
                _collectionConstructor = ReflectionHelper.GetCollectionConstructorDelegate<TMember>(_propertyInfo.PropertyType);
                _collectionGetter = ReflectionHelper.GetCollectionPropertyGetterDelegate<TContainer, TMember>(_propertyInfo);
                _collectionSetter = ReflectionHelper.GetCollectionPropertySetterDelegate<TContainer, TMember>(_propertyInfo);
            }
        }

        public void AddToCollection(object container, object member)
        {
            IList collection = null;
            IList<TMember> typedCollection;

            bool isContainerTheCollection = _propertyInfo == null;
            if (isContainerTheCollection)
            {
                typedCollection = container as IList<TMember>;
                if (typedCollection == null)
                    collection = container as IList;
            }
            else
            {
                typedCollection = _collectionGetter((TContainer)container);
            }

            if (collection == null && typedCollection == null)
            {
                if (!isContainerTheCollection)
                {
                    typedCollection = _collectionConstructor();
                    _collectionSetter((TContainer)container, typedCollection);
                }
                else
                    throw new InvalidOperationException(string.Format("Unable to instantiate a new {0} collection", typeof(TMember)));
            }

            if (collection != null)
                collection.Add(member);
            else if (typedCollection != null)
                typedCollection.Add((TMember)member);
        }

        public IList GetCollection(object container)
        {
            if (_propertyInfo == null)
                return (IList)container;
            return (IList)_collectionGetter((TContainer)container);
        }

        object IChildElementMapping.GetFromContainer(object target)
        {
            throw new NotSupportedException();
        }

        void IChildElementMapping.SetOnContainer(object target, object item)
        {
            throw new NotSupportedException();
        }
    }
}
