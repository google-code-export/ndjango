using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bistro.Validation;
using Bistro.Extensions.Validation;

namespace NDjango.BistroIntegration.Validation
{
    /// <summary>
    /// Default javascript validation emitter
    /// </summary>
    public class JSEmitter
    {
        static JSEmitter instance = new JSEmitter();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static JSEmitter Instance { get { return instance; } }

        /// <summary>
        /// Emit implementation starting point
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        protected StringBuilder DoEmit(IValidator v, StringBuilder sb)
        {
            if (v.GetType().IsGenericType && typeof(ValidationSite<,>).IsAssignableFrom(v.GetType().GetGenericTypeDefinition()))
                return EmitSites(v, sb);
            else
                return EmitNamespace(v, sb);
        }

        /// <summary>
        /// Emits a namespace.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private StringBuilder EmitNamespace(IValidator v, StringBuilder sb)
        {
            sb
                .Append("{ ns: \"")
                .Append(v.Name)
                .Append("\", rules: [");

            bool found = false;
            foreach (IValidator child in v.Children)
            {
                sb = DoEmit(child, sb);
                sb.Append(",");
                found = true;
            }

            if (found)
                sb.Remove(sb.Length - 1, 1);

            return sb.Append("]}");
        }

        /// <summary>
        /// Emits the sites.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private StringBuilder EmitSites(IValidator v, StringBuilder sb)
        {
            sb
                .Append("{ field: \"")
                .Append(v.Name)
                .Append("\", attributes: [");

            bool found = false;
            foreach (IValidator child in v.Children)
            {
                sb.Append("{ name: \"");
                sb.Append(GetSimpleTypeName(child));
                sb.Append("\"");

                foreach (string key in child.DefiningParams.Keys)
                {
                    sb.Append(",");
                    sb.Append(key);
                    sb.Append(": \"");
                    sb.Append(stripQuotes(Convert.ToString(child.DefiningParams[key])));
                    sb.Append("\"");
                }

                sb.Append("},");

                found = true;
            }

            if (found)
                sb.Remove(sb.Length - 1, 1);

            return sb.Append("]}");
        }

        private string stripQuotes(string p)
        {
            return p.Replace("\"", "\\\"");
        }

        /// <summary>
        /// Gets the simple name of the type.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <returns></returns>
        private string GetSimpleTypeName(IValidator child)
        {
            var name = child.GetType().FullName;
            var ind = name.IndexOf('`');

            if (ind > -1)
                return name.Substring(0, ind);

            return name;
        }

        /// <summary>
        /// Emits the javascript representation of validator <c>v</c>, and assigns it to variable <c>varName</c>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="varName">Name of the var.</param>
        /// <returns></returns>
        public string Emit(IValidator v, string varName)
        {
            StringBuilder sb = DoEmit(v, new StringBuilder());

            sb.Insert(0, varName + " = ");
            sb.AppendLine(";");

            return sb.ToString();
        }
    }
}
