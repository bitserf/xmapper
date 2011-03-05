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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace XMapper
{
    /// <summary>
    /// XML serializer for schemas described using element mappings.
    /// </summary>
    public class Serializer
    {
        #region Fields
        readonly SchemaDescription _schemaDescription;
        readonly XmlSchemaSet _schemaSet;
        #endregion

        /// <summary>
        /// Creates a new serializer.
        /// </summary>
        /// <param name="description">The schema description to support.</param>
        public Serializer(SchemaDescription description)
            : this(description, null)
        {
        }

        /// <summary>
        /// Creates a new serializer.
        /// </summary>
        /// <param name="description">The schema description to support.</param>
        /// <param name="schemaSet">The set of compiled XML schemas to use if validation is requested.</param>
        public Serializer(SchemaDescription description, XmlSchemaSet schemaSet)
        {
            _schemaDescription = description;
            _schemaSet = schemaSet;
        }

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <typeparam name="TItem">The type of the object to deserialize, must have an associated element mapping in the schema description.</typeparam>
        /// <param name="reader">The XML reader, currently positioned on an XML element corresponding to <typeparamref name="TItem"/></param>
        /// <returns>Returns the deserialized object.</returns>
        public TItem Deserialize<TItem>(XmlReader reader)
        {
            return (TItem)ReadItem(GetMapping<TItem>(), reader);
        }

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <typeparam name="TItem">The type of the object to deserialize, must have an associated element mapping in the schema description.</typeparam>
        /// <param name="stream">A stream currently positioned on an XML element corresponding to <typeparamref name="TItem"/></param>
        /// <returns>Returns the deserialized object.</returns>
        public TItem Deserialize<TItem>(Stream stream)
        {
            return Deserialize<TItem>(stream, false);
        }

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <typeparam name="TItem">The type of the object to deserialize, must have an associated element mapping in the schema description.</typeparam>
        /// <param name="stream">A stream currently positioned on an XML element corresponding to <typeparamref name="TItem"/></param>
        /// <param name="validate">Whether or not to validate the XML during deserialization. Requires that a schema set be provided.</param>
        /// <returns>Returns the deserialized object.</returns>
        public TItem Deserialize<TItem>(Stream stream, bool validate)
        {
            if (validate && _schemaSet == null)
                throw new ArgumentException("XML validation requested, but no schema set");

            XmlReader reader;

            if (validate)
            {
                var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = _schemaSet };
                reader = XmlReader.Create(stream, settings);
            }
            else
                reader = XmlReader.Create(stream);

            // Skip over leading non-elements.
            while (reader.Read() && reader.NodeType != XmlNodeType.Element) { }

            // Don't dispose of reader, we don't own stream.
            return Deserialize<TItem>(reader);
        }

        public void Serialize<TItem>(XmlWriter writer, TItem item)
        {
            WriteItem(GetMapping<TItem>(), writer, item);
        }

        public void Serialize<TItem>(Stream stream, TItem item)
        {
            var writer = XmlWriter.Create(stream);

            Serialize(writer, item);

            // Don't dispose of writer, we don't own stream.
            writer.Flush(); 
        }

        static object ReadItem(IElementMapping mapping, XmlReader reader)
        {
            if (!reader.LocalName.Equals(mapping.LocalName))
                throw new XmlFormatException(string.Format("Expected element <{0}> at this position", mapping.LocalName), reader as IXmlLineInfo);
            if (!string.IsNullOrEmpty(mapping.NamespaceUri) && !reader.NamespaceURI.Equals(mapping.NamespaceUri))
                throw new XmlFormatException(string.Format("Expected element <{0}> to have a namespace of '{1}' at this position", mapping.LocalName, mapping.NamespaceUri), reader as IXmlLineInfo);

            var item = mapping.CreateInstance();

            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    var attributeMapping =
                        string.IsNullOrEmpty(reader.NamespaceURI)
                            ? mapping.TryFindAttributeMapping(reader.LocalName)
                            : mapping.TryFindAttributeMapping(reader.NamespaceURI, reader.LocalName);

                    if (attributeMapping != null)
                        attributeMapping.SetValueFromXmlForm(item, reader.Value);

                } while (reader.MoveToNextAttribute());
                reader.MoveToElement();
            }

            if (!reader.IsEmptyElement)
            {
                bool skipped = false;

                while (skipped || reader.Read())
                {
                    skipped = false;

                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        var childElementMapping =
                            string.IsNullOrEmpty(reader.NamespaceURI)
                                ? mapping.TryFindChildElementMapping(reader.LocalName)
                                : mapping.TryFindChildElementMapping(reader.NamespaceURI, reader.LocalName);

                        if (childElementMapping != null)
                        {
                            var child = ReadItem(childElementMapping, reader);

                            if (childElementMapping is ICollectionChildElementMapping)
                            {
                                var collectionMapping = (ICollectionChildElementMapping)childElementMapping;
                                collectionMapping.AddToCollection(item, child);
                            }
                            else
                                childElementMapping.SetOnContainer(item, child);
                        }
                        else
                        {
                            reader.Skip();
                            skipped = true;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                        break;
                }
            }

            return item;
        }

        static void WriteItem(IElementMapping mapping, XmlWriter writer, object item)
        {
            writer.WriteStartElement(mapping.LocalName, mapping.NamespaceUri);

            if (mapping.Attributes != null)
            {
                foreach (var attrMapping in mapping.Attributes)
                {
                    var value = attrMapping.GetValueInXmlForm(item);
                    if (value != null)
                        writer.WriteAttributeString(attrMapping.LocalName, attrMapping.NamespaceUri, value);
                }
            }

            if (mapping.ChildElements != null && mapping.ChildElements.Length > 0)
            {
                // This is a HACK. We need to instead have a lookup table we can use to look up the element type of
                // a collection element, and then use the appropriate descriptor, on a PER-ELEMENT basis.
                // to allow things like <Address /><CustomAddress /> in the same list.

                var seenCollections = new HashSet<IList>();

                foreach (var childElementMapping in mapping.ChildElements)
                {
                    if (childElementMapping is ICollectionChildElementMapping)
                    {
                        var collection = ((ICollectionChildElementMapping)childElementMapping).GetCollection(item);
                        if (!seenCollections.Contains(collection))
                        {
                            foreach (var child in collection)
                                WriteItem(childElementMapping, writer, child);
                        }
                        seenCollections.Add(collection);

                    }
                    else
                    {
                        var child = childElementMapping.GetFromContainer(item);
                        if (child != null)
                            WriteItem(childElementMapping, writer, child);
                    }
                }
            }

            writer.WriteEndElement();
        }

        IElementMapping GetMapping<T>()
        {
            var mapping = _schemaDescription.TryFindMappingForType<T>();
            if (mapping == null)
                throw new ArgumentException(string.Format("Unable to determine how to serialize/deserialize objects of type {0}", typeof(T)));
            return mapping;
        }
    }
}
