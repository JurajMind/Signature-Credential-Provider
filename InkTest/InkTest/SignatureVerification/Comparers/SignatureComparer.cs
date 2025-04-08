using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace SignatureVerification
{
    public class SignatureComparer
    {
        private readonly Guid _id;

        public SignatureComparer(Guid guid = new Guid())
        {
            _id = guid;
        }

        public Guid Id
        {
            get { return _id; }
        }

        public virtual double CalculateWeights(List<Stroke> patterns)
        {
            return 1d;
        }


        public virtual double Compare(Stroke signature, Stroke patern, double weight)
        {
            return 0d;
        }

        public virtual UIElement DrawnGui(Stroke signature, Stroke patern, double weight)
        {
            var result = new Border();
            var text = new TextBlock();
            text.Text = "Gui not implemented";
            result.Child = text;
            return result;
        }
    

        public virtual string GetName()
        {
            return "Generic signature comparer";
        }
    }
}