using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace Goliath.Data.CodeGenerator.Actions
{
    public class ExportedDataModel
    {
        public string Name { get; set; }
        public string EntityName { get; set; }
        public string PreRunCommand { get; set; }
        public string PostRunCommand { get; set; }
        public int RowCount { get; set; }

        public IList<IDictionary<string, object>> DataRows { get; set; } = new List<IDictionary<string, object>>();

        public XmlNode SerializeToXml(XmlDocument doc)
        {
            var tableElement = doc.CreateElement("table");

            if (!string.IsNullOrWhiteSpace(Name))
                tableElement.SetAttribute("name", Name);

            if (!string.IsNullOrWhiteSpace(EntityName))
                tableElement.SetAttribute("entity", EntityName);

            tableElement.SetAttribute("rows", DataRows.Count.ToString());

            if (!string.IsNullOrWhiteSpace(PreRunCommand))
            {
                var preRunElement = doc.CreateElement("preCommand");
                preRunElement.InnerText = PreRunCommand;
                tableElement.AppendChild(preRunElement);
            }

            if (!string.IsNullOrWhiteSpace(PostRunCommand))
            {
                var postRunelement = doc.CreateElement("postCommand");
                postRunelement.InnerText = PostRunCommand;
                tableElement.AppendChild(postRunelement);
            }

            foreach (var dataRow in DataRows)
            {
                var rowElement = doc.CreateElement("row");
                foreach (var dr in dataRow)
                {
                    if (dr.Value == null)
                        continue;

                    var fieldElement = doc.CreateElement("field");
                    fieldElement.SetAttribute("name", dr.Key);
                    fieldElement.InnerText = dr.Value.ToString();
                    rowElement.AppendChild(fieldElement);
                }

                tableElement.AppendChild(rowElement);
            }

            return tableElement;
        }

        public void LoadXml(XContainer data)
        {
            var elm = data as XElement;

            if (elm == null)
                throw new SerializationException("Could not load table. Please make sure the XML is in correct format.");

            var name = elm.Attribute("name");
            if (name == null)
                throw new SerializationException("table name attribute is mandatory.");

            var rowCountAtt = elm.Attribute("rows");
            if (rowCountAtt != null && int.TryParse(rowCountAtt.Value, out var rowCount))
            {
                RowCount = rowCount;
            }

            var entityName = elm.Attribute("entity");
            Name = name.Value;
            EntityName = entityName?.Value;

            var pres = elm.Descendants("preCommand").FirstOrDefault();
            PreRunCommand = pres?.Value;

            var posts = elm.Descendants("postCommand").FirstOrDefault();
            PostRunCommand = posts?.Value;

            var rows = elm.Descendants("row");
            foreach (var row in rows)
            {
                var rowData = new Dictionary<string, object>();
                var fields = row.Descendants("field");

                foreach (var fieldElm in fields)
                {
                    var keyAttr = fieldElm.Attribute("name");
                    if (keyAttr == null)
                        throw new SerializationException("1 field is missing a name. The name attribute is mandatory for element field.");

                    rowData.Add(keyAttr.Value, fieldElm.Value);
                }

                DataRows.Add(rowData);
            }

        }

        public void Save(string filePath)
        {
            var doc = new XmlDocument();
            var node = SerializeToXml(doc);
            doc.AppendChild(node);
            doc.Save(filePath);
        }

        /// <summary>
        /// Loads from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static ExportedDataModel LoadFromFile(string filePath)
        {
            using (var xmlStream = new FileStream(filePath, FileMode.Open))
            {
                return Load(xmlStream);
            }
        }

        /// <summary>
        /// Loads the specified XML stream.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">xmlStream</exception>
        public static ExportedDataModel Load(Stream xmlStream)
        {
            if (xmlStream == null) throw new ArgumentNullException(nameof(xmlStream));
            var doc = XDocument.Load(xmlStream);
            return LoadDataFromXml(doc);
        }


        /// <summary>
        /// Loads the data from XML.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">doc</exception>
        static ExportedDataModel LoadDataFromXml(XDocument doc)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            ExportedDataModel model = new ExportedDataModel();
            var table = doc.Descendants("table").First();
            model.LoadXml(table);
            return model;
        }
    }
}