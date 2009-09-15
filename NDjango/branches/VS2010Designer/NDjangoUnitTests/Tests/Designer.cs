using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NDjango.UnitTests.Data;
using NDjango.Interfaces;
using System.Collections;
using NDjango.FiltersCS;

namespace NDjango.UnitTests
{
    public partial class Tests
    {
        List<TestDescriptor> tests = new List<TestDescriptor>();

        public void SetupStandartdValues()
        {
            provider = new TemplateManagerProvider()
                .WithLoader(new Loader())
                .WithTag("non-nested", new TestDescriptor.SimpleNonNestedTag())
                .WithTag("nested", new TestDescriptor.SimpleNestedTag())
                .WithTag("url", new TestUrlTag())
                ;
            provider = FilterManager.Initialize(provider);
            this.standardTags = ((IDictionary<string, ITag>)((ITemplateManagerProvider)provider).Tags).Keys;
            this.standardFilters = ((IDictionary<string, ISimpleFilter>)((ITemplateManagerProvider)provider).Filters).Keys;
        }
      

        [Test, TestCaseSource("GetDesignerTests")]
        public void DesignerTests(TestDescriptor test)
        {
            InternalFilterProcess(test);
        }

        private DesignerData[] Nodes(params DesignerData[] nodes) { return nodes; }

        private string[] EmptyList
        {
            get
            {
                return new string[] { };
            }
        }

        public IList<TestDescriptor> GetDesignerTests()
        {
            SetupStandartdValues();

            //NewTest("if-tag-designer", "{% if foo %}yes{% else %}no{% endif %}" 
            //    , Nodes 
            //    (
            //        Node(27, 11, "endif"),
            //        Node(15, 10, "else", "endif"),
            //        StandardNode(0, 12)
            //    ));
            NewTest("ifequal-tag-designer", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}"
                , Nodes 
                (
                    StandardNode(0, 52),
                    Node(34, 18, "endifequal"),
                    StandardNode(39, 10),
                    Node(21, 13, "else", "endifequal"),
                    StandardNode(27, 4),
                    Node(21, 13, "else", "endifequal"),
                    StandardNode(27, 4),
                    Node(34, 18, "endifequal"),
                    StandardNode(39, 10),
                    StandardFilter(14, 0),
                    StandardFilter(18, 0),
                    StandardNode(3, 7)
                ));
            //NewTest("add-filter-designer", "{{ value| add:\"2\" }}"
            //    , Nodes 
            //    (
            //        StandardFilter(9, 3)
            //    ));
            //NewTest("fortag-designer", "{% for i in test %}{% ifchanged %}nothing changed{%else%}same {% endifchanged %}{{ forloop.counter }},{% endfor %}"
            //    , Nodes 
            //    (
            //        Node(0, 0),
            //        Node(0, 19),
            //        Node(19, 0),
            //        Node(19, 15),
            //        Node(57, 5),
            //        Node(62, 18),
            //        Node(64, 13, AddToStandardList("endif")),
            //        Node(62, 2),
            //        Node(78, 2),
            //        Node(34, 15),
            //        Node(49, 8),
            //        Node(51, 4, AddToStandardList("else", "endif")),
            //        Node(49, 2),
            //        Node(55, 2),
            //        Node(21, 10, standardTags.ToArray()),
            //        Node(19, 2),
            //        Node(32, 2),
            //        Node(80, 0),
            //        Node(80, 21),
            //        Node(80, 2),
            //        Node(99, 2),
            //        Node(101, 1),
            //        Node(102, 12),
            //        Node(104, 7, standardTags.ToArray()),
            //        Node(102, 2),
            //        Node(112, 2),
            //        Node(2, 4, standardTags.ToArray()),
            //        Node(0, 2),
            //        Node(17, 2),
            //        Node(114, 0)
            //    ));

            return tests;
        }

        //The following 'standard' methods are for nodes, which have standard Values list without any additions.
        private DesignerData StandardFilter(int position, int length, int errorSeverity, string errorMessage)
        {
            return new DesignerData(position, length, standardFilters.ToArray(), errorSeverity, errorMessage);
        }

        private DesignerData StandardNode(int position, int length, int errorSeverity, string errorMessage)
        {
            return new DesignerData(position, length, standardTags.ToArray(), errorSeverity, errorMessage);
        }

        private DesignerData StandardFilter(int position, int length)
        {
            return new DesignerData(position, length, standardFilters.ToArray(), -1, String.Empty);
        }

        private DesignerData StandardNode(int position, int length)
        {
            return new DesignerData(position, length, standardTags.ToArray(), -1, String.Empty);
        }

        //method for some nodes with error message
        private DesignerData ErrorNode(int position, int length, string[] values, int errorSeverity, string errorMessage)
        {
            if (values.Length == 0)
                return new DesignerData(position, length, EmptyList, errorSeverity, errorMessage);
            else
                return new DesignerData(position, length, AddToStandardList(values), errorSeverity, errorMessage);
        }

        private DesignerData Node(int position, int length, params string[] values)
        {
            return ErrorNode(position, length, values, -1, String.Empty);
        }

        private void NewTest(string name, string template, DesignerData[] nodeList)
        {
            tests.Add(new TestDescriptor(name, template, nodeList.ToList<DesignerData>()));
        }

        private string[] AddToStandardList(params string[] tags)
        {
            List<string> result = new List<string>(standardTags);
            result.InsertRange(0, tags);
            return result.ToArray();
        }
    }
}
