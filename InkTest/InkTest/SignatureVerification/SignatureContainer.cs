using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace SignatureVerification
{
    [Serializable]
    public class SignatureContainer : ISerializable
    {
        private readonly Dictionary<Guid, double> _comparerWeights;
        private  List<Stroke> _patterns;
        private  SecureString _token;
        private readonly string _name;

        private double _threshold;

        public SignatureContainer(SecureString token = null)
        {
            _patterns = new List<Stroke>();
            _comparerWeights = new Dictionary<Guid, double>();
            _token = token;
            _name = SupportFunctions.GetRandomName(8);

        }

        public void ChangeToken(SecureString oldToken, SecureString newToken)
        {
            if (_token == oldToken)
            {
                _token = newToken;
            }
        }
        public SignatureContainer(SerializationInfo info, StreamingContext ctxt)
        {
            var stream = (MemoryStream) info.GetValue("Patterns", typeof (MemoryStream));
            stream.Position = 0;
            var sc = new StrokeCollection(stream);
            _patterns = sc.ToList();

            _comparerWeights =
                (Dictionary<Guid, double>) info.GetValue("ComparerWeights", typeof (Dictionary<Guid, double>));
            _threshold = (double) info.GetValue("Treshold", typeof (double));

            _name = (string) info.GetValue("Name",typeof(string));
            var ts = (string)info.GetValue("Token", typeof(string));
            _token = SupportFunctions.convertToSecureString(ts);
        }

        public double Threshold
        {
            get { return _threshold; }
        }

        public object Name
        {
            get { return _name; }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var stream = new MemoryStream();
            var sc = new StrokeCollection(_patterns);
            sc.Save(stream);

            info.AddValue("Patterns", stream);
            info.AddValue("Name", _name);
            info.AddValue("Token", _token.SecureStringToString());

            info.AddValue("ComparerWeights", _comparerWeights);
            info.AddValue("Treshold", _threshold);
        }

        public double Compare(Stroke signature, List<SignatureComparer> comparers)
        {
            if (CanCompare())
                return _patterns.Select(l => ComparePatern(signature, l, comparers)).Min();
            return double.NaN;
        }

        private double ComparePatern(Stroke signature, Stroke patern, List<SignatureComparer> comparers)
        {
            double result = 0d;
            double comparerWeight = 0d;
            foreach (SignatureComparer signatureComparer in comparers)
            {
                if (_comparerWeights.ContainsKey(signatureComparer.Id))
                {
                     comparerWeight = _comparerWeights[signatureComparer.Id];
                }
                
                result = result + signatureComparer.Compare(signature, patern, comparerWeight);
            }
            return result;
        }

        public bool CanCompare()
        {
            if (_patterns.Count < 2)
                return false;
            return true;
        }

        public void AddPattern(Stroke patern, List<SignatureComparer> comparers, SecureString token)
        {
            if (!_token.IsEqualTo(token) && _token.Length > 0)
                return;
            
            _patterns.Add(patern);
            if (_patterns.Count >= 3)
            {
                RecalculateWeights(comparers);
                _threshold = CalculateThreshold(comparers);
            }
        }

        public void ClearPatterns(SecureString token)
        {
            if (!_token.IsEqualTo(token))
                return;
            _patterns = new List<Stroke>();
        }

        private void RecalculateWeights(List<SignatureComparer> comparers)
        {
            foreach (SignatureComparer signatureComparer in comparers)
            {
                _comparerWeights[signatureComparer.Id] = signatureComparer.CalculateWeights(_patterns);
            }
        }

        private double CalculateThreshold(List<SignatureComparer> comparers)
        {
            var results = new List<double>();
            for (int i = 0; i < _patterns.Count; i++)
            {
                for (int j = 0; j < _patterns.Count; j++)
                {
                    if (i != j)
                        results.Add(ComparePatern(_patterns[i], _patterns[j], comparers));
                }
            }

            return results.Average();
        }

        public bool isSimilar(Stroke signature, List<SignatureComparer> comparers)
        {
            double signatureSimilarity = Compare(signature, comparers);

            return signatureSimilarity <= _threshold;
        }

        public bool isSimilar(Stroke signature, List<SignatureComparer> comparers, out double threshold,
            out SecureString token)
        {
            double signatureSimilarity = Compare(signature, comparers);
            token = _token;
            threshold = signatureSimilarity;
            return signatureSimilarity <= _threshold;
        }

       
        public Task<bool> IsSimilarAsync(Stroke signature, List<SignatureComparer> comparers)
        {
            return Task.Run(() => isSimilar(signature, comparers));
        }


        public UIElement GetUiInfo(List<SignatureComparer> comparers)
        {
            var sp = new StackPanel();

            sp.Children.Add(new TextBlock {Text = "Comparers weights:", Margin = new Thickness(10)});

            foreach (SignatureComparer signatureComparer in comparers)
            {
                var text = new TextBlock();
                string Weight = "Unknown";

                if (_comparerWeights.ContainsKey(signatureComparer.Id))
                {
                    Weight = _comparerWeights[signatureComparer.Id].ToString("F");
                }

                text.Text = String.Format("Name:{0}|Weight:{1}", signatureComparer.GetName(), Weight);
                sp.Children.Add(text);
            }


            return sp;
        }

        public bool Check(SecureString token)
        {
            return token.IsEqualTo(_token);
        }

        public List<UIElement> DrawnComparerUi(Stroke signature, List<SignatureComparer> comparers)
        {
            if (CanCompare())
            {
                Stroke mostSimilarPatern = _patterns.OrderBy(l => ComparePatern(signature, l, comparers)).First();

                List<UIElement> UiElements =
                    comparers.Select(
                        signatureComparer =>
                            signatureComparer.DrawnGui(signature, mostSimilarPatern,
                                _comparerWeights[signatureComparer.Id])).ToList();

                return UiElements;
            }

            return new List<UIElement>();
        }
    }
}