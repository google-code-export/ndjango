﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using System.IO;
using Microsoft.FSharp.Collections;
using FSStringList = Microsoft.FSharp.Collections.FSharpList<string>;
using StringList = System.Collections.Generic.List<string>;
using System.Text.RegularExpressions;
using System.Xml;
using NDjango.FiltersCS;
using NDjango.Interfaces;

namespace NDjango.UnitTests
{
    [TestFixture]
    public partial class Tests
    {
        public class Loader : NDjango.Interfaces.ITemplateLoader
        {

            public Loader()
            {
                templates.Add("t1", "insert1--{% block b1 %}to be replaced{% endblock %}--insert2");
                templates.Add("t22", "insert1--{% block b1 %}to be replaced22{% endblock %}{% block b2 %}to be replaced22{% endblock %}--insert2");
                templates.Add("t21", "{% extends \"t22\" %}skip - b21{% block b1 %}to be replaced21{% endblock %}skip-b21");
                templates.Add("tBaseNested",
@"{% block outer %}
{% block inner1 %}
this is inner1
{% endblock inner1 %}
{% block inner2 %}
this is inner2
{% endblock inner2 %}
{% endblock outer %}");
                templates.Add("include-name", "inside included template {{ value }}");
            }
            Dictionary<string, string> templates = new Dictionary<string, string>();

            #region ITemplateLoader Members

            public TextReader GetTemplate(string name)
            {
                if (templates.ContainsKey(name))
                    return new StringReader(templates[name]);
                return new StringReader(name);
            }

            public bool IsUpdated(string source, DateTime ts)
            {
                // alternate
                //return ts.Second % 2 == 0;
                return false;
            }

            #endregion
        }

        public class TestUrlTag : NDjango.Tags.Abstract.UrlTag
        {
            public override string GenerateUrl(string formatString, string[] parameters, NDjango.Interfaces.IContext context)
            {
                return "/appRoot/" + String.Format(formatString.Trim('/'), parameters);
            }
        }

        NDjango.Interfaces.ITemplateManager manager;
        TemplateManagerProvider provider;
        public ICollection<string> standardTags = new List<string>();
        public ICollection<string> standardFilters = new List<string>();

        [TestFixtureSetUp]
        public void Setup()
        {
            provider = new TemplateManagerProvider()
                .WithLoader(new Loader())
                .WithTag("non-nested", new TestDescriptor.SimpleNonNestedTag())
                .WithTag("nested", new TestDescriptor.SimpleNestedTag())
                .WithTag("url", new TestUrlTag())
                ;
            provider = FilterManager.Initialize(provider);
            manager = provider.GetNewManager();
        }

        public struct StringTest 
        {
            public StringTest(string name, string provided, string[] expected)
            {
                this.expected = expected;
                this.provided = provided;
                this.name = name;
            }
            string name;
            public string [] expected;
            public string provided;
            public override string ToString()
            {
                return name;
            }
        }

        /// <summary>
        /// <see cref=""/>
        /// </summary>
        /// <param name="test"></param>
        //[Test, TestCaseSource("smart_split_tests")]
        public void TestSmartSplit(StringTest test)
        {
            Regex r = new Regex(@"(""(?:[^""\\]*(?:\\.[^""\\]*)*)""|'(?:[^'\\]*(?:\\.[^'\\]*)*)'|[^\s]+)", RegexOptions.Compiled);
            MatchCollection m = r.Matches(@"'\'funky\' style'");
            
            Func<string[], FSStringList> of_array = (array) => ListModule.of_array<string>(array);

            Func<FSStringList, string[]> to_string_array = (string_list) =>
            {
                var tl = string_list;
                var res = new StringList();
                while (ListModule.length<string>(tl) > 0)
                {
                    res.Add(ListModule.hd<string>(tl));
                    tl = ListModule.tl<string>(tl);
                }

                return res.ToArray();
            };

   //         Assert.AreEqual(to_string_array(of_array(test.expected)), to_string_array(OutputHandling.smart_split(test.provided)));
        }

        public IEnumerable<StringTest> smart_split_tests()
        {
            System.Collections.Generic.List<StringTest> result = new System.Collections.Generic.List<StringTest>();
            result.Add(new StringTest("smart split-01",
                @"This is ""a person\'s"" test.",
                new string[] { "This", "is", @"""a person\'s""", "test." }
            ));

            result.Add(new StringTest("smart split-02",
                @"Another 'person\'s' test.",
                new string[] { "Another", @"'person's'", "test." }
            ));

            result.Add(new StringTest("smart split-03",
                "A \"\\\"funky\\\" style\" test.",
                new string[] { "A", "\"\"funky\" style\"", "test." }
            ));

            result.Add(new StringTest("smart split-04",
                @"A '\'funky\' style' test.",
                new string[] { "A", @"''funky' style'", "test." }
            ));

            return result;
        }

        //[Test, TestCaseSource("split_token_tests")]
        public void TestSplitContent(StringTest test)
        {
            Func<string[], FSStringList> of_array = (array) => ListModule.of_array<string>(array);

            Func<FSStringList, string[]> to_string_array = (string_list) =>
            {
                var tl = string_list;
                var res = new StringList();
                while (ListModule.length<string>(tl) > 0)
                {
                    res.Add(ListModule.hd<string>(tl));
                    tl = ListModule.tl<string>(tl);
                }

                return res.ToArray();
            };

            //Assert.AreEqual(to_string_array(of_array(test.expected)), to_string_array(OutputHandling.split_token_contents(test.provided)));
        }

        public IEnumerable<StringTest> split_token_tests()
        {
            System.Collections.Generic.List<StringTest> result = new System.Collections.Generic.List<StringTest>();
            result.Add(new StringTest("split token-01",
                @"This is _(""a person\'s"") test.",
                new string[] { "This", "is", @"_(""a person\'s"")", "test." }
            ));

            result.Add(new StringTest("split token-02",
                @"Another 'person\'s' test.",
                new string[] { "Another", @"'person's'", "test." }
            ));

            result.Add(new StringTest("split token-03",
                "A \"\\\"funky\\\" style\" test.",
                new string[] { "A", "\"\"funky\" style\"", "test." }
            ));
/*
            result.Add(new StringTest("split token-04",
                "This is _(\"a person's\" test).",
                new string[] { "This", "is", "\"a person's\" test)." }
            ));
// */
            return result;
        }

        //    Assert.AreEqual(to_string_array(of_array(new string[] { "This", "is", "\"a person's\" test)." })),
        //        to_string_array(OutputHandling.split_token_contents("This is _(\"a person's\" test).")));

        //    Assert.AreEqual(to_string_array(of_array(new string[] { "Another", "_('person\'s')", "test." })),
        //        to_string_array(OutputHandling.split_token_contents("Another '_(person\'s)' test.")));

        //    Assert.AreEqual(to_string_array(of_array(new string[] { "A", "_(\"\\\"funky\\\" style\" test.)" })),
        //        to_string_array(OutputHandling.split_token_contents("A _(\"\\\"funky\\\" style\" test.)")));
        //}

        private Dictionary<string,object> CreateContext(string path)
        {
            var result = new Dictionary<string, object>();
            if (File.Exists(path))
            {
                XmlDocument data = new XmlDocument();
                data.Load(path);
                foreach (XmlElement variable in data.DocumentElement)
                {
                    object value = null;
                    if (variable.Attributes["type"] != null && variable.Attributes["value"] != null)
                        switch (variable.Attributes["type"].Value)
                        {
                            case "integer": value = int.Parse(variable.Attributes["value"].Value);
                                break;
                            case "string": value = variable.Attributes["value"].Value;
                                break;
                            case "boolean": value = bool.Parse(variable.Attributes["value"].Value);
                                break;
                        }

                    if (variable.Attributes["type"] == null && variable.Attributes["value"] != null)
                        value = variable.Attributes["value"].Value;

                    if (!result.ContainsKey(variable.Name))
                        result.Add(variable.Name, value);
                    else
                        if (result[variable.Name] is IList)
                            ((IList)result[variable.Name]).Add(value);
                        else
                           result[variable.Name] = new System.Collections.Generic.List<object>(new object[] {result[variable.Name], value});

                }
            }
            return result;
        }

//        [Test, TestCaseSource("TemplateTestEnum1")]
        //public void Test1(string path)
        //{
        //    string retVal = TestDescriptor.runTemplate(manager, File.ReadAllText(path + ".django"), CreateContext(path + ".xml"));
        //    string retBase = File.ReadAllText(path + ".htm");
        //    Assert.AreEqual(retBase, retVal, String.Format("RESULT!!!!!!!!!!!!!!!!:\r\n{0}", retVal));
        //}


        //public IEnumerable<string> TemplateTestEnum1
        //{
        //    get
        //    {
        //        var result = new System.Collections.Generic.List<string>();
        //        result.Add("../Tests/Templates/Test1____/Scripts/create");
        //        return result;
        //    }
        //}




////        [Test, TestCaseSource("TestEnumerator")]
//        public void Test(string path)
//        {
//            string retVal = TestDescriptor.runTemplate(manager, File.ReadAllText(path + ".django"), CreateContext(path + ".xml"));
//            string retBase = File.ReadAllText(path + ".htm"); 
//            Assert.AreEqual(retBase, retVal,String.Format("RESULT!!!!!!!!!!!!!!!!:\r\n{0}",retVal));
//        }

//        public IEnumerable<string> TestEnumerator
//        {
//            get 
//            {
//                var result = new System.Collections.Generic.List<string>();
//                foreach (string file in Directory.GetFiles("../../Tests", "*.django", SearchOption.AllDirectories))
//                    result.Add(file.Substring(0, file.LastIndexOf(".")));
//                return result; 
//            }
//        }
    }
}
