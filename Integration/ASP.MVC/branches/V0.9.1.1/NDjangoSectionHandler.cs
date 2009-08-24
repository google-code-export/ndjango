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

        [ConfigurationCollection(typeof(NameValueElementCollection<NameValueElementAssembly>), AddItemName = "NDJangoImport")]
        [ConfigurationProperty("NDJangoCollection", IsDefaultCollection = true)]
        public NameValueElementCollection<NameValueElementAssembly> NDJangoSectionCollection
        {
            get
            {
                return (NameValueElementCollection<NameValueElementAssembly>)base["NDJangoCollection"];
            }

        }

        [ConfigurationCollection(typeof(NameValueElementCollection<NameValueClassElement>), AddItemName = "import", ClearItemsName = "clearadd-tag", RemoveItemName = "removeadd-tag")]
        [ConfigurationProperty("NDJangoTagFilterCollection", IsDefaultCollection = true)]
        public NameValueElementCollection<NameValueClassElement> NDJangoTagFilterSectionCollection
        {
            get
            {
                return (NameValueElementCollection<NameValueClassElement>)base["NDJangoTagFilterCollection"];
            }

        }

        [ConfigurationCollection(typeof(NameValueElementCollection<NameValueElement>), AddItemName = "add-tag", ClearItemsName = "clearadd-tag", RemoveItemName = "removeadd-tag")]
        [ConfigurationProperty("NDJangoTagCollection", IsDefaultCollection = true)]
        public NameValueElementCollection<NameValueElement> NDJangoTagSectionCollection
        {
            get
            {
                return (NameValueElementCollection<NameValueElement>)base["NDJangoTagCollection"];
            }

        }


        [ConfigurationCollection(typeof(NameValueElementCollection<NameValueElement>), AddItemName = "add-filter", ClearItemsName = "clearadd-filter", RemoveItemName = "removeadd-filter")]
        [ConfigurationProperty("NDJangoFilterCollection", IsDefaultCollection = true)]
        public NameValueElementCollection<NameValueElement> NDJangoFilterSectionCollection
        {
            get
            {
                return (NameValueElementCollection<NameValueElement>)base["NDJangoFilterCollection"];
            }

        }
        [ConfigurationCollection(typeof(NameValueElementCollection<NameValueElement>), AddItemName = "add-setting", ClearItemsName = "clearsetting", RemoveItemName = "removesetting")]
        [ConfigurationProperty("NDJangoSettingsCollection", IsDefaultCollection = true)]
        public NameValueElementCollection<NameValueElement> NDJangoSettingsSectionCollection
        {
            get
            {
                return (NameValueElementCollection<NameValueElement>)base["NDJangoSettingsCollection"];
            }

        }

    }

    #region generic
    public class NameValueElementCollection<T> : ConfigurationElementCollection where T : ConfigurationElement, INameElement
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public T this[int index]
        {
            get { return (T)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(T element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return Activator.CreateInstance<T>();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((T)element).Name;
        }

        public void Remove(T element)
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

    public interface INameElement
    {
        string Name { get; set; }
    }


    #endregion

    public class NameValueElementAssembly : ConfigurationElement, INameElement
    {
        public NameValueElementAssembly() { }

        public NameValueElementAssembly(string assembly)
        {
            this.Name = assembly;
        }



        [ConfigurationProperty("assembly", IsRequired = true)]
        public string Name
        {
            get { return (string)this["assembly"]; }
            set { this["assembly"] = value; }
        }

        [ConfigurationCollection(typeof(NameValueElementCollection<NameValueElementAssembly>), AddItemName = "import")]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public NameValueElementCollection<NameValueElementImport> ImportCollection
        {
            get
            {
                return (NameValueElementCollection<NameValueElementImport>)base[""];
            }

        }

    }

    public class NameValueClassElement : ConfigurationElement, INameElement
    {
        public NameValueClassElement() { }


        public NameValueClassElement(string classtype)
        {
            this.Name = classtype;
        }

        [ConfigurationProperty("classtype", IsRequired = true, DefaultValue = "ALL")]
        public string Name
        {
            get { return (string)this["classtype"]; }
            set { this["classtype"] = value; }
        }
    }

    public class NameValueElement : ConfigurationElement, INameElement
    {
        public NameValueElement() { }


        public NameValueElement(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        [ConfigurationProperty("name", IsRequired = true, DefaultValue = "ALL")]
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

    }
    public class NameValueElementImport : ConfigurationElement, INameElement
    {
        public NameValueElementImport() { }


        public NameValueElementImport(string name)
        {
            this.Name = name;
        }

        [ConfigurationProperty("name", IsRequired = true, DefaultValue = "ALL")]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

    }

}
