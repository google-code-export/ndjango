﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NDjango.UnitTests.Data;
using NDjango.FiltersCS;
using System.Diagnostics;
using NUnit.Framework;
using NDjango.Interfaces;


namespace NDjango.UnitTests
{
    public struct DesignerData
    {
        public DesignerData(int position, int length, string[] values, int severity, string errorMessage)
        {
            this.Position = position;
            this.Length = length;
            this.Values = values;
            this.Severity = severity;
            this.ErrorMessage = errorMessage;
        }

        public int Position;
        public int Length;
        public string[] Values;
        public int Severity;
        public string ErrorMessage;
    }

    public class TestDescriptor
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public object[] ContextValues { get; set; }
        public object[] Result { get; set; }
        public List<DesignerData> ResultForDesigner { get; set; }
        public string[] Vars { get; set; }
        ResultGetter resultGetter;

        public override string ToString()
        {
            return Name;
        }

        public TestDescriptor(string name, string template, List<DesignerData> designResult)
        {
            Name = name;
            Template = template;
            ResultForDesigner = designResult;
        }

        public TestDescriptor(string name, string template, object[] values, object[] result, List<DesignerData> designResult, params string[] vars)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            Result = result;
            Vars = vars;
            ResultForDesigner = designResult;
        }

        public TestDescriptor(string name, string template, object[] values, object[] result, params string[] vars)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            Result = result;
            Vars = vars;
        }

        public delegate object[] ResultGetter();

        public TestDescriptor(string name, string template, object[] values, ResultGetter resultGetter, params string[] vars)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            this.resultGetter = resultGetter;
            Vars = vars;
        }


        public string runTemplate(NDjango.Interfaces.ITemplateManager manager, string templateName, IDictionary<string,object> context)
        {
            Stopwatch stopwatch = new Stopwatch();
            string retStr = "";
            stopwatch.Start();
            for (int i = 0; i < 1; i++)
            {
                var template = manager.RenderTemplate(templateName, context);
                retStr = template.ReadToEnd();
            }
            using (TextWriter stream = System.IO.File.AppendText("Timers.txt"))
                stream.WriteLine(Name + "," + stopwatch.ElapsedTicks); 
            return retStr;
        }

		public class SimpleNonNestedTag : NDjango.Compatibility.SimpleTag
        {
            public SimpleNonNestedTag() : base(false, "non-nested", 2) { }

            public override string ProcessTag(NDjango.Interfaces.IContext context, string contents, object[] parms)
            {
                StringBuilder res = new StringBuilder();
                foreach (object o in parms)
                    res.Append(o);

                return res
                    .Append(contents)
                    .ToString();
            }
        }

        public class SimpleNestedTag : NDjango.Compatibility.SimpleTag
        {
            public SimpleNestedTag() : base(true, "nested", 2) { }

            public override string ProcessTag(NDjango.Interfaces.IContext context, string contents, object[] parms)
            {
                StringBuilder res = new StringBuilder();
                foreach (object o in parms)
                    res.Append(o);

                return res
                    .Append("start")
                    .Append(contents)
                    .Append("end")
                    .ToString();
            }
        }
        public void AnalyzeBlockNameNode(NDjango.Interfaces.ITemplateManager manager)
        {
            ITemplate template = manager.GetTemplate(Template);
            INode bn_node = GetNodes(template.Nodes.ToList<INodeImpl>().ConvertAll
                    (node => (INode)node)).Find(node => node.NodeType == NodeType.BlockName);
            var value_provider = bn_node as ICompletionValuesProvider;
            var values = (value_provider == null) ? new List<string>() : value_provider.Values;
            List<string> blockNames = new List<string>(values);
            Assert.Greater(0, blockNames.Count(), "The dropdown with block names is empty");
            foreach(string name in Result) 
                Assert.Contains(name, blockNames, "Invalid block names list: there is no " + name);

        }

        public void Run(NDjango.Interfaces.ITemplateManager manager)
        {
            if (ResultForDesigner != null)
            {
                ValidateSyntaxTree(manager);
                return;
            }

            var context = new Dictionary<string, object>();

            if (ContextValues != null)
                for (int i = 0; i <= ContextValues.Length - 2; i += 2)
                    context.Add(ContextValues[i].ToString(), ContextValues[i + 1]);

            try
            {
                if (resultGetter != null)
                    Result = resultGetter();
                Assert.AreEqual(Result[0], runTemplate(manager, Template, context), "** Invalid rendering result");
                //if (Vars.Length != 0)
                //    Assert.AreEqual(Vars, manager.GetTemplateVariables(Template), "** Invalid variable list");
            }
            catch (Exception ex)
            {
                // Result[0] is either some object, in which case this shouldn't have happened
                // or it's the type of the exception the calling code expects.
                if (resultGetter != null)
                    Result = resultGetter();
                Assert.AreEqual(Result[0], ex.GetType(), "Exception: " + ex.Message);
            }
        }

        private void ValidateSyntaxTree(NDjango.Interfaces.ITemplateManager manager)
        {
            ITemplate template = manager.GetTemplate(Template, new TestTyperesolver(),
                new NDjango.TypeResolver.ModelDescriptor(new IDjangoType[] 
                    {
                        new NDjango.TypeResolver.CLRTypeDjangoType("Standard", typeof(EmptyClass))
                    }));
            
            //the same logic responsible for retriving nodes as in NodeProvider class (DjangoDesigner).
            List<INode> nodes = GetNodes(template.Nodes.ToList<INodeImpl>().ConvertAll
                (node => (INode)node)).FindAll(node =>
                    (node is ICompletionValuesProvider) 
                    || (node.NodeType == NodeType.ParsingContext) 
                    || (node.ErrorMessage.Message != ""));
            List<DesignerData> actualResult = nodes.ConvertAll(
                node =>
                {
                    var value_provider = node as ICompletionValuesProvider;
                    var values =
                        value_provider == null ?
                            new List<string>()
                            : value_provider.Values;
                    List<string> contextValues = new List<string>(values);
                    if (node.NodeType == NodeType.ParsingContext)
                    {
                        contextValues.InsertRange(0 ,(node.Context.TagClosures));
                        return new DesignerData(node.Position, node.Length, contextValues.ToArray(), node.ErrorMessage.Severity, node.ErrorMessage.Message);
                    }
                    else if (node.NodeType == NodeType.Reference)
                    {
                        return new DesignerData(node.Position, node.Length, GetModelValues(node.Context.Model, 2).ToArray(), node.ErrorMessage.Severity, node.ErrorMessage.Message);
                    }
                    else
                        return new DesignerData(node.Position, node.Length, new List<string>(values).ToArray(), node.ErrorMessage.Severity, node.ErrorMessage.Message);
                });
            
            for (int i = 0; i < actualResult.Count; i++)
            {
                if (actualResult[i].Values.Length == 0)
                    continue;

                Assert.AreEqual(ResultForDesigner[i].Length, actualResult[i].Length, "Invalid Length");
                Assert.AreEqual(ResultForDesigner[i].Position, actualResult[i].Position, "Invalid Position");
                Assert.AreEqual(ResultForDesigner[i].Severity, actualResult[i].Severity, "Invalid Severity");
                Assert.AreEqual(ResultForDesigner[i].ErrorMessage, actualResult[i].ErrorMessage, "Invalid ErrorMessage");
                Assert.AreEqual(ResultForDesigner[i].Values, actualResult[i].Values, "Invalid Values Array");
            }            
        }

        private static List<string> GetModelValues(IDjangoType model, int recursionDepth)
        {
            List<string> result = new List<string>();
            int remainingSteps = recursionDepth - 1;
            foreach (IDjangoType member in model.Members)
            {
                if (remainingSteps > 0)
                {
                    result.Add(member.Name);
                    result.AddRange(GetModelValues(member, remainingSteps));
                }
                else
                {
                    result.Add(member.Name);
                }
            }
            return result;
        }

        //the same logic responsible for retriving nodes as in NodeProvider class (DjangoDesigner).
        private static List<INode> GetNodes(IEnumerable<INode> nodes)
        {
            List<INode> result = new List<INode>();

            foreach (INode ancestor in nodes)
	        {
                result.Add(ancestor);
                foreach (IEnumerable<INode> list in ancestor.Nodes.Values)
                {
                    result.AddRange(GetNodes(list));
                }
	        }
            return result;
        }

        //the same list as in Defaults.standardTags
        
    }

}
