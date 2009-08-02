using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;

namespace NDjango.Designer.Intellisense
{
    [ContentType("text")]
    [Export(typeof(IKeyProcessorProvider))]
    [Name("Django Key Processor")]
    [Order(Before = Priority.High)]
    internal sealed class KeyProcessorProvider : IKeyProcessorProvider
    {
        public KeyProcessorProvider()
        {
        }

        public Microsoft.VisualStudio.Text.Editor.KeyProcessor GetAssociatedProcessor(IWpfTextViewHost wpfTextViewHost)
        {
            return (new KeyProcessor(wpfTextViewHost.TextView));
        }
    }
    internal class KeyProcessor : Microsoft.VisualStudio.Text.Editor.KeyProcessor
    {
        private IWpfTextView view;

        internal static event KeyEventHandler KeyDownEvent;
        internal static event KeyEventHandler KeyUpEvent;

        internal KeyProcessor(IWpfTextView view)
        {
            this.view = view;
        }

        public override void KeyDown(KeyEventArgs args)
        {
            this.OnKeyDown(args);
            base.KeyDown(args);
        }

        public override void KeyUp(KeyEventArgs args)
        {
            this.OnKeyUp(args);
            base.KeyUp(args);
        }

        protected virtual void OnKeyDown(KeyEventArgs args)
        {
            if (KeyDownEvent != null)
            {
                KeyDownEvent(view, args);
            }
        }

        protected virtual void OnKeyUp(KeyEventArgs args)
        {
            if (KeyUpEvent != null)
            {
                KeyUpEvent(view, args);
            }
        }
    }
}
