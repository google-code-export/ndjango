using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NDjango.UnitTests.Data;

namespace NDjango.UnitTests
{
    public partial class Tests
    {
        List<TestDescriptor> tests = new List<TestDescriptor>();

        //[Test, TestCaseSource("GetDesignerTests")]
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
            NewTest("if-tag-designer", "{% if foo %}yes{% else %}no{% endif %}" 
                , Nodes 
                (
                    Node(0, 0, EmptyList),
                    Node(0, 12, EmptyList),
                    Node(25, 2, EmptyList),
                    Node(27, 11, EmptyList),
                    Node(27, 11, TestDescriptor.standardValues),
                    Node(27, 2, EmptyList),
                    Node(36, 2, EmptyList),
                    Node(12, 3, EmptyList),
                    Node(15, 10, EmptyList),
                    Node(17, 5, TestDescriptor.standardValues),
                    Node(15, 2, EmptyList),
                    Node(23, 2, EmptyList),
                    Node(2, 3, EmptyList),
                    Node(0, 2, EmptyList),
                    Node(10, 2, EmptyList),
                    Node(38, 0, EmptyList)
                ));
            NewTest("ifequal-tag-designer", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}"
                , Nodes 
                (
                    Node(0, 0, EmptyList),
                    Node(0, 21, EmptyList),
                    Node(34, 2, EmptyList),
                    Node(36, 16, EmptyList),
                    Node(38, 11, TestDescriptor.standardValues),
                    Node(36, 2, EmptyList),
                    Node(50, 2, EmptyList),
                    Node(21, 3, EmptyList),
                    Node(24, 10, EmptyList),
                    Node(26, 5, TestDescriptor.standardValues),
                    Node(24, 2, EmptyList),
                    Node(32, 2, EmptyList),
                    Node(2, 8, TestDescriptor.standardValues),
                    Node(0, 2, EmptyList),
                    Node(19, 2, EmptyList),
                    Node(52, 0, EmptyList)
                ));
            NewTest("add-filter-designer", "{{ value| add:\"2\" }}"
                , Nodes 
                (
                    Node(0, 0, EmptyList),
                    Node(0, 20, EmptyList),
                    Node(0, 2, EmptyList),
                    Node(18, 2, EmptyList),
                    Node(20, 0, EmptyList)
                ));
            NewTest("fortag-designer", "{% for i in test %}{% ifchanged %}nothing changed{%else%}same {% endifchanged %}{{ forloop.counter }},{% endfor %}"
                , Nodes 
                (
                    Node(0, 0, EmptyList),
                    Node(0, 19, EmptyList),
                    Node(19, 0, EmptyList),
                    Node(19, 15, EmptyList),
                    Node(57, 5, EmptyList),
                    Node(62, 18, EmptyList),
                    Node(64, 13, TestDescriptor.standardValues),
                    Node(62, 2, EmptyList),
                    Node(78, 2, EmptyList),
                    Node(34, 15, EmptyList),
                    Node(49, 8, EmptyList),
                    Node(51, 4, TestDescriptor.standardValues),
                    Node(49, 2, EmptyList),
                    Node(55, 2, EmptyList),
                    Node(21, 10, TestDescriptor.standardValues),
                    Node(19, 2, EmptyList),
                    Node(32, 2, EmptyList),
                    Node(80, 0, EmptyList),
                    Node(80, 21, EmptyList),
                    Node(80, 2, EmptyList),
                    Node(99, 2, EmptyList),
                    Node(101, 1, EmptyList),
                    Node(102, 12, EmptyList),
                    Node(104, 7, TestDescriptor.standardValues),
                    Node(102, 2, EmptyList),
                    Node(112, 2, EmptyList),
                    Node(2, 4, TestDescriptor.standardValues),
                    Node(0, 2, EmptyList),
                    Node(17, 2, EmptyList),
                    Node(114, 0, EmptyList)
                ));

            return tests;
        }

        private DesignerData ErrorNode(int position, int length, string[] values, int errorSeverity, string errorMessage)
        {
            return new DesignerData(position, length, values, errorSeverity, errorMessage);
        }

        private DesignerData Node(int position, int length, string[] values)
        {
            return ErrorNode(position, length, values, -1, String.Empty);
        }

        private void NewTest(string name, string template, DesignerData[] nodeList)
        {
            tests.Add(new TestDescriptor(name, template, nodeList.ToList<DesignerData>()));
        }

    }
}
