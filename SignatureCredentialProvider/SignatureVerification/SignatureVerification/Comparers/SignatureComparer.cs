using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace SignatureVerification
{
    /// <summary>
    ///     This is base class for all signature comparers
    /// </summary>
    public class SignatureComparer
    {
        /// <summary>
        ///     GUID of comparer
        /// </summary>
        private readonly Guid _id;

        public SignatureComparer(Guid guid = new Guid())
        {
            _id = guid;
        }

        public Guid Id
        {
            get { return _id; }
        }

        /// <summary>
        ///     Function that serve for calculating weight of comparer by given patterns
        /// </summary>
        /// <param name="patterns"></param>
        /// <returns></returns>
        public virtual double CalculateWeights(List<Stroke> patterns)
        {
            return 1d;
        }

        /// <summary>
        ///     Function that return similarity by this comparer
        /// </summary>
        /// <param name="signature">Signature to compare similarity</param>
        /// <param name="patern">all user patters</param>
        /// <param name="weight"> weight of this comparer</param>
        /// <returns></returns>
        public virtual double Compare(Stroke signature, Stroke patern, double weight)
        {
            return 0d;
        }

        /// <summary>
        ///     Return vizualization of this comparer
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="patern"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public virtual UIElement DrawnGui(Stroke signature, Stroke patern, double weight)
        {
            var result = new Border();
            var text = new TextBlock();
            text.Text = "Gui not implemented";
            result.Child = text;
            return result;
        }

        /// <summary>
        ///     Return name of this comparer
        /// </summary>
        /// <returns></returns>
        public virtual string GetName()
        {
            return "Generic signature comparer";
        }
    }
}