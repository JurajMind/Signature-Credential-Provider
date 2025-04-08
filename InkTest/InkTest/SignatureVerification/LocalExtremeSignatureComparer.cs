using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;

namespace SignatureVerification
{
    public class LocalExtremeSignatureComparer : SignatureComparer
    {
        public LocalExtremeSignatureComparer() : base(new Guid("b02f1be4-f444-44fa-a16b-02cc61f33b51"))
        {
        }

        public override string getName()
        {
            return "Recognition based of numbers of local extremes";
        }

        public override double CalculateWeights(List<Stroke> patterns)
        {
            return 0.5d;
        }

        public override double Compare(Stroke signature, Stroke patern, double weight)
        {
            List<double> signatureXvalues = signature.StylusPoints.Select(p => p.X).ToList();
            List<double> paternXvalues = patern.StylusPoints.Select(p => p.X).ToList();

            List<int> signatureXMin = findLocal(signatureXvalues, (d, d1) => d < d1);
            List<int> paternXMin = findLocal(paternXvalues, (d, d1) => d < d1);

            bool result = signatureXMin.Count == paternXMin.Count;

            if (result)
            {
                return 1;
            }
            return 0;
        }


        private List<int> findLocal(List<double> values, Func<double, double, bool> compFunc)
        {
            var result = new List<int>();
            for (int i = 1; i < values.Count - 1; i++)
            {
                if (compFunc(values[i - 1], values[i]) && compFunc(values[i + 1], values[i]))
                {
                    result.Add(i);
                }
            }
            return result;
        }
    }
}