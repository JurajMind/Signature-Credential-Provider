using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using SignatureVerification;

namespace WpfWcfNamedPipeBinding
{
    class ComparePerUserPerPatern
    {
        private List<SignatureComparer> _signatureComparers;
        private SignatureContainer _signatureContainer;
        private string _name;
        private int _patternCount;

        public ComparePerUserPerPatern(SignatureContainer container, List<SignatureComparer> signatureComparers,string name,int patternCount)
        {
            _signatureContainer = container;
            _signatureComparers = signatureComparers;
            _name = name;
            _patternCount = patternCount;
        }

        public async Task<ExternalSignatureCompareResult> Compare(List<Tuple<Stroke,int>> signatures )
        {
            var partialResults = new ExternalSignatureCompareResult[signatures.Count];

            var offset = signatures.Min(o => o.Item2);

            for (int index = 0; index < signatures.Count; index++)
            {
                var signature = signatures[index];
                partialResults[index] = await CompareOne(signature);
            }


            int allSignatures = (signatures.Count + _patternCount);
            int genuedSignature = (allSignatures/2)- _patternCount;
            int fakeSignatures = allSignatures/2;

            return partialResults.Aggregate((acc, cur) => acc + cur)
                .CalculateAverageResult(genuedSignature, fakeSignatures);

        }

        private async Task<ExternalSignatureCompareResult> CompareOne(Tuple<Stroke, int> signature)
        {
            var result = await _signatureContainer.IsSimilarAsync( signature.Item1,_signatureComparers);

        
            int falsePositive = 0;
            int truePositive = 0;
            int falseNegative = 0;
            int trueNegative = 0;

            if (signature.Item2 <= 20)
            {
                if (result)
                {
                    truePositive++;
                }
                else
                {
                    falseNegative++;
                }
            }
            else
            {
                if (result)
                {
                    falsePositive++;
                }
                else
                {
                    trueNegative++;
                }
            }

            return new ExternalSignatureCompareResult()
            {
                falsePositive = falsePositive,
                falseNegative = falseNegative,
                truePositive = truePositive,
                trueNegative = trueNegative,
                User = _name
            };
            ;

        }
    }
}
