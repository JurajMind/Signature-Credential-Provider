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
    /// <summary>
    ///     Signature container
    ///     It hold user patterns and provide functions fror comparers to compare
    ///     given signature with storen one
    /// </summary>
    [Serializable]
    public class SignatureContainer : ISerializable
    {
        private readonly Dictionary<Guid, double> _comparerWeights;
        private readonly string _name;
        private List<Stroke> _patterns;
        private SecureString _token;

        /// <summary>
        ///     Creator of signature container, for test purpoise token is optional parrameter
        ///     by defaild is null
        /// </summary>
        /// <param name="token">Secure token for signature container</param>
        public SignatureContainer(SecureString token = null)
        {
            _patterns = new List<Stroke>();
            _comparerWeights = new Dictionary<Guid, double>();
            _token = token;
            _name = SupportFunctions.GetRandomName(8);
        }

        /// <summary>
        ///     Deserialize class for signature containers
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public SignatureContainer(SerializationInfo info, StreamingContext ctxt)
        {
            var stream = (MemoryStream) info.GetValue("Patterns", typeof (MemoryStream));
            stream.Position = 0;
            var sc = new StrokeCollection(stream);
            _patterns = sc.ToList();

            _comparerWeights =
                (Dictionary<Guid, double>) info.GetValue("ComparerWeights", typeof (Dictionary<Guid, double>));
            Threshold = (double) info.GetValue("Treshold", typeof (double));

            _name = (string) info.GetValue("Name", typeof (string));
            var ts = (string) info.GetValue("Token", typeof (string));
            _token = SupportFunctions.convertToSecureString(ts);
        }

        /// <summary>
        ///     Thhreshold of
        /// </summary>
        public double Threshold { get; private set; }

        /// <summary>
        ///     Name
        /// </summary>
        public object Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Function that prvide object data for serializable
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var stream = new MemoryStream();
            var sc = new StrokeCollection(_patterns);
            sc.Save(stream);

            info.AddValue("Patterns", stream);
            info.AddValue("Name", _name);
            info.AddValue("Token", _token.SecureStringToString());

            info.AddValue("ComparerWeights", _comparerWeights);
            info.AddValue("Treshold", Threshold);
        }

        /// <summary>
        ///     Function for changing token
        ///     Use when user change password and new one is used
        /// </summary>
        /// <param name="oldToken">Old token,muse be the same as signature container token</param>
        /// <param name="newToken">New token that will replace old one</param>
        public void ChangeToken(SecureString oldToken, SecureString newToken)
        {
            if (_token == oldToken)
            {
                _token = newToken;
            }
        }

        /// <summary>
        ///     This return numered similarity between given signatures and stored patterns
        /// </summary>
        /// <param name="signature">Signature to be measure</param>
        /// <param name="comparers"></param>
        /// <returns></returns>
        public double Compare(Stroke signature, List<SignatureComparer> comparers)
        {
            if (CanCompare())
                return _patterns.Select(l => ComparePatern(signature, l, comparers)).Min();
            return double.NaN;
        }

        /// <summary>
        ///     Compare one patten to one signature
        /// </summary>
        /// <param name="signature">Signature to compare</param>
        /// <param name="patern">Patern to compare</param>
        /// <param name="comparers">Comparers</param>
        /// <returns></returns>
        private double ComparePatern(Stroke signature, Stroke patern, List<SignatureComparer> comparers)
        {
            var result = 0d;
            var comparerWeight = 0d;
            foreach (var signatureComparer in comparers)
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
            if (_patterns.Count < 3)
                return false;
            return true;
        }

        /// <summary>
        ///     Function foadding new user pattens to comparer
        /// </summary>
        /// <param name="patern">User pattern, that will be added</param>
        /// <param name="comparers">Comparers</param>
        /// <param name="token">Token serve as password</param>
        public void AddPattern(Stroke patern, List<SignatureComparer> comparers, SecureString token)
        {
            if (!_token.IsEqualTo(token) && _token.Length > 0)
                return;

            _patterns.Add(patern);
            if (_patterns.Count >= 3)
            {
                RecalculateWeights(comparers);
                Threshold = CalculateThreshold(comparers);
            }
        }

        /// <summary>
        ///     Clean saved data, this happen when new pattens are recorded
        /// </summary>
        /// <param name="token">Token serve as password to perform this action</param>
        public void ClearPatterns(SecureString token)
        {
            if (!_token.IsEqualTo(token))
                return;
            _patterns = new List<Stroke>();
            _comparerWeights.Clear();
        }

        /// <summary>
        ///     Function that recalculate stored comparer weight, in case some comparer is changed
        /// </summary>
        /// <param name="comparers"></param>
        private void RecalculateWeights(List<SignatureComparer> comparers)
        {
            foreach (var signatureComparer in comparers)
            {
                _comparerWeights[signatureComparer.Id] = signatureComparer.CalculateWeights(_patterns);
            }
        }

        /// <summary>
        ///     Function that calculate treshold by comparers, it pick average similatitysh between pattenrs
        /// </summary>
        /// <param name="comparers"></param>
        /// <returns></returns>
        private double CalculateThreshold(List<SignatureComparer> comparers)
        {
            var results = new List<double>();
            for (var i = 0; i < _patterns.Count; i++)
            {
                for (var j = 0; j < _patterns.Count; j++)
                {
                    if (i != j)
                        results.Add(ComparePatern(_patterns[i], _patterns[j], comparers));
                }
            }

            return results.Average();
        }

        /// <summary>
        ///     Basic similar function
        /// </summary>
        /// <param name="signature">Signature to be compare to stored signature patterns</param>
        /// <param name="comparers">Comparers that compare sugnature</param>
        /// <returns></returns>
        public bool isSimilar(Stroke signature, List<SignatureComparer> comparers)
        {
            var signatureSimilarity = Compare(signature, comparers);

            return signatureSimilarity <= Threshold;
        }

        /// <summary>
        ///     Similarity calculation of given signature to storad signature,
        ///     this is enhacnment version of comparer
        /// </summary>
        /// <param name="signature">Signature that will be compare</param>
        /// <param name="comparers">Comparers that compare signatures</param>
        /// <param name="threshold">output treshold of signatures by comparers to most similar stored signature</param>
        /// <param name="token">Output token to authentizate</param>
        /// <returns></returns>
        public bool isSimilar(Stroke signature, List<SignatureComparer> comparers, out double threshold,
            out SecureString token)
        {
            var signatureSimilarity = Compare(signature, comparers);
            token = _token;
            threshold = signatureSimilarity;
            return signatureSimilarity <= Threshold;
        }

        /// <summary>
        ///     Asynchronou similarity comparer.
        /// </summary>
        /// <param name="signature">Signature to compare</param>
        /// <param name="comparers">Compares that compare signatures</param>
        /// <returns></returns>
        public Task<bool> IsSimilarAsync(Stroke signature, List<SignatureComparer> comparers)
        {
            return Task.Run(() => isSimilar(signature, comparers));
        }

        public UIElement GetUiInfo(List<SignatureComparer> comparers)
        {
            var sp = new StackPanel();

            sp.Children.Add(new TextBlock {Text = "Comparers weights:", Margin = new Thickness(10)});

            foreach (var signatureComparer in comparers)
            {
                var text = new TextBlock();
                var Weight = "Unknown";

                if (_comparerWeights.ContainsKey(signatureComparer.Id))
                {
                    Weight = _comparerWeights[signatureComparer.Id].ToString("F");
                }

                text.Text = string.Format("Name:{0}|Weight:{1}", signatureComparer.GetName(), Weight);
                sp.Children.Add(text);
            }


            return sp;
        }

        public bool Check(SecureString token)
        {
            return token.IsEqualTo(_token);
        }

        /// <summary>
        ///     Shown purpouse fucntion, it show information about container stored signatures
        ///     Each comparer provide informatiu about itself
        /// </summary>
        /// <param name="signature">Signature that will be compare</param>
        /// <param name="comparers">Signature comparers</param>
        /// <returns></returns>
        public List<UIElement> DrawnComparerUi(Stroke signature, List<SignatureComparer> comparers)
        {
            if (CanCompare())
            {
                var mostSimilarPatern = _patterns.OrderBy(l => ComparePatern(signature, l, comparers)).First();

                var UiElements =
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