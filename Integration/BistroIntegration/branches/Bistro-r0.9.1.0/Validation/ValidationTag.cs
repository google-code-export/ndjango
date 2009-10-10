using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bistro.Validation;
using NDjango.Interfaces;

namespace NDjango.BistroIntegration.Validation
{
    /// <summary>
    /// Default implementation of the {% validate %} tag
    /// </summary>
    [Name("validate")]
    public class ValidationTag: NDjango.Compatibility.SimpleTag
    {
        public ValidationTag() : base(false, "validate", -1) { }

        /// <summary>
        /// Processes the tag.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        public override string ProcessTag(NDjango.Interfaces.IContext context, string content, object[] parms)
        {
            IValidator v = null;
            foreach (string ns in parms)
                v =
                    v == null ?
                    ValidationRepository.Instance.GetValidatorForNamespace(ns) :
                    v.Merge(ValidationRepository.Instance.GetValidatorForNamespace(ns));

            if (v == null)
                throw new ArgumentException("Provided namespaces aren't valid", parms.Aggregate("[", (s, e) => s + e + ";") + "]");

            return
                new StringBuilder()
                    .AppendLine("\r\n<script type=\"text/javascript\">")
                    .AppendLine("if (validation == undefined) var validation = new Array();")
                    .AppendLine(JSEmitter.Instance.Emit(v, "validation[\"" + v.Name + "\"]"))
                    .AppendLine("</script>")
                    .ToString();
        }
    }
}
