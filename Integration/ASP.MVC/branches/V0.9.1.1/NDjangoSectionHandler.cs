using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace NDjango.ASPMVCIntegration
{
    class NDjangoSectionHandler : ConfigurationSection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SectionHandler"/> class.
        /// </summary>
        public NDjangoSectionHandler() { }

        [ConfigurationCollection(typeof(NameValueElementCollection), AddItemName = "add-tag", ClearItemsName = "clearadd-tag", RemoveItemName = "removeadd-tag")]
        [ConfigurationProperty("NDJangoTagCollection", IsDefaultCollection = true)]
        public NameValueElementCollection NDJangoTagSectionCollection
        {
            get
            {
                return (NameValueElementCollection)base["NDJangoTagCollection"];
            }

        }


        [ConfigurationCollection(typeof(NameValueElementCollection), AddItemName = "add-filter", ClearItemsName = "clearadd-filter", RemoveItemName = "removeadd-filter")]
        [ConfigurationProperty("NDJangoFilterCollection", IsDefaultCollection = true)]
        public NameValueElementCollection NDJangoFilterSectionCollection
        {
            get
            {
                return (NameValueElementCollection)base["NDJangoFilterCollection"];
            }

        }
        [ConfigurationCollection(typeof(NameValueElementCollection), AddItemName = "add-setting", ClearItemsName = "clearsetting", RemoveItemName = "removesetting")]
        [ConfigurationProperty("NDJangoSettingsCollection", IsDefaultCollection = true)]
        public NameValueElementCollection NDJangoSettingsSectionCollection
        {
            get
            {
                return (NameValueElementCollection)base["NDJangoSettingsCollection"];
            }

        }
           
    }

    public class NameValueElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public NameValueElement this[int index]
        {
            get { return (NameValueElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(NameValueElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NameValueElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NameValueElement)element).Name;
        }

        public void Remove(NameValueElement element)
        {
            BaseRemove(element.Name);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
    }

    public class NameValueElement : ConfigurationElement
    {
        public NameValueElement() { }

        public NameValueElement(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        [ConfigurationProperty("name", IsRequired = true, DefaultValue="ALL")]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }

        //[ConfigurationProperty("NDJangoTagSection", IsDefaultCollection = true)]
        //[ConfigurationCollection(typeof(NameValueElementCollection), AddItemName = "add-tag")]
        //public NameValueElementCollection NameValues
        //{
        //    get { return (NameValueElementCollection)base["NDJangoTagSection"]; }
        //}
        
        //[ConfigurationProperty("NDJangoTagSection", IsDefaultCollection = true)]
        //[ConfigurationCollection(typeof(NameValueElementCollection), AddItemName = "add-tag")]
        //public NameValueElementCollection Value
        //{
        //    get { return (NameValueElementCollection)base["NDJangoTagSection"]; }
        //}
    }
}
