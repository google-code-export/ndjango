using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NDjango.UnitTests.Data;
using NDjango.FiltersCS;


namespace NDjango.UnitTests
{
    public class TestDescriptor
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public object[] ContextValues { get; set; }
        public object[] Result { get; set; }
        ResultGetter resultGetter;

        public override string ToString()
        {
            return Name;
        }

        public TestDescriptor(string name, string template, object[] values, object[] result)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            Result = result;
        }

        public delegate object[] ResultGetter();

        public TestDescriptor(string name, string template, object[] values, ResultGetter resultGetter)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            this.resultGetter = resultGetter;
        }

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
                return true;
            }

            #endregion
        }

        public static string runTemplate(string templateName, IDictionary<string,object> context)
        {
            NDjango.Interfaces.ITemplateManager manager = NDjango.Template.Manager.RegisterLoader(new Loader());
            FilterManager.Instance.Initialize();
            var template = manager.RenderTemplate(templateName, context);
            System.IO.TextReader reader = template.Item2;
            return reader.ReadToEnd();
        }

        public bool Run(out string received)
        {
            var context = new Dictionary<string, object>();

            if (ContextValues != null)
                for (int i = 0; i <= ContextValues.Length - 2; i += 2)
                    context.Add(ContextValues[i].ToString(), ContextValues[i + 1]);

            try
            {
                received = runTemplate(Template, context);
                if (resultGetter != null)
                    Result = resultGetter();
                return received.Equals(Result[0]);
            }
            catch (Exception ex)
            {
                received = ex.Message;

                // Result[0] is either some object, in which case this shouldn't have happened
                // or it's the type of the exception the calling code expects.
                if (resultGetter != null)
                    Result = resultGetter();
                return Result[0].Equals(ex.GetType());
            }
        }

    }

}
