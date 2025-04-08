using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfWcfNamedPipeBinding
{
    public class ExternalSignatureCompareResult
    {
        public string User { get; set; }
        public double falsePositive { get; set; }
        public double truePositive { get; set; }
        public double falseNegative { get; set; }
        public double trueNegative { get; set; }

        public override string ToString()
        {
            return string.Format("{0};{1};{2};{3};{4}", User, truePositive.ToString("F"),
           falseNegative.ToString("F"), trueNegative.ToString("F"), falsePositive.ToString("F"));

        }

        public static ExternalSignatureCompareResult operator +(
            ExternalSignatureCompareResult a, ExternalSignatureCompareResult b)
        {
            return new ExternalSignatureCompareResult()
            {
               trueNegative = a.trueNegative + b.trueNegative,
               truePositive = a.truePositive + b.truePositive,
               falsePositive = a.falsePositive + b.falsePositive,
               falseNegative = a.falseNegative + b.falseNegative,
               User = a.User
            };
        }

        public static ExternalSignatureCompareResult operator /(ExternalSignatureCompareResult a, int count)
        {
            return new ExternalSignatureCompareResult()
            {
                trueNegative = a.trueNegative / count,
                truePositive = a.truePositive / count,
                falsePositive = a.falsePositive / count,
                falseNegative = a.falseNegative  / count,
                User = a.User
            };
        }

        public ExternalSignatureCompareResult CalculateAverageResult(int countOfGenuedSignature, int countOfFakedSignatures)
        {
            return new ExternalSignatureCompareResult()
            {
                trueNegative = trueNegative / countOfFakedSignatures,
                truePositive = truePositive / countOfGenuedSignature,

                falsePositive = falsePositive / countOfFakedSignatures,
                falseNegative = falseNegative / countOfGenuedSignature,
                User = User
            };
        }
    }
}
