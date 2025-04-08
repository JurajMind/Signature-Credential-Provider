using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using NDtw;
using SignatureVerification;
using WcfServiceLibraryNamedPipe;

namespace WpfWcfNamedPipeBinding
{
    public partial class MainWindow : Window
    {
        private readonly Host _host;
        private readonly ServiceController _sc = new ServiceController("Signature Autentificator");

        private readonly FileSystemWatcher watcher = new FileSystemWatcher();
        private NamedPipeServer PServer2;
        private DispatcherTimer _dispatcherTimer;
        private DateTime _maxDateTime = DateTime.Now;
        public List<Stroke> colectedSignature = new List<Stroke>();

        public List<SignatureComparer> signatureComparers = new List<SignatureComparer>();
        public List<SignatureContainer> signatureContainers = new List<SignatureContainer>();
        private int strokes = 0;
        private readonly SignatureContainer testContainer = new SignatureContainer(new SecureString());


        public MainWindow()
        {
            InitializeComponent();
            _host = Host.Instance();
            LoadUserSignature();
            AddComparers();
            if (DoesServiceExist("Signature Autentificator") && _sc.Status == ServiceControllerStatus.Running)
            {
                ListBox_Messages.Items.Add("Signature Autentificator service running");
                LoadSignatures(10);
                //LoadExternalSignatures("USER3");
            }
            else
            {
                Loaded += MainWindow_Loaded;
            }
        }

        private bool DoesServiceExist(string serviceName)
        {
            return
                ServiceController.GetServices()
                    .Any(serviceController => serviceController.ServiceName.Equals(serviceName));
        }

        private void LoadUserSignature()
        {
            var directoryPath = @"C:\Signature\UserPaterns";
            var di = new DirectoryInfo(directoryPath);

            if (!di.Exists)
            {
                Directory.CreateDirectory(directoryPath);
            }

            var files = di.GetFileSystemInfos();
            signatureComparers = new List<SignatureComparer>();
            foreach (var file in files)
            {
                try
                {
                    using (var stream = File.Open(file.FullName, FileMode.Open))
                    {
                        var bformatter = new BinaryFormatter();
                        signatureContainers.Add((SignatureContainer) bformatter.Deserialize(stream));
                    }
                }
                catch (Exception)
                {
                }
            }

            if (!signatureContainers.Any())
            {
                signatureContainers.Add(new SignatureContainer());
            }
        }

        private void LoadSignatures(int count)
        {
            var directoryPath = @"C:\Signature\SavedSignatures";
            var di = new DirectoryInfo(directoryPath);

            watcher.Path = directoryPath;
            watcher.NotifyFilter = NotifyFilters.FileName;
            watcher.Created += OnChanged;
            watcher.EnableRaisingEvents = true;

            var files = di.GetFileSystemInfos();
            var fileEntries = files.OrderByDescending(f => f.CreationTime)
                .Select(l => l.FullName).ToList();


            var signatureCount = Math.Min(count, fileEntries.Count());
            fileEntries = fileEntries.Take(signatureCount).Reverse().ToList();

            foreach (var fileEntry in fileEntries)
            {
                LoadOneSignature(fileEntry);
            }
        }

        private async Task LoadExternalSignatures(int iteration, int users ,int patternsCount,string path = @"B:\Task2\Task2")
        {
            var _path = @"B:\" +String.Format("p{0}_i{1}_forger",patternsCount,iteration)+ DateTime.Now.ToString("hh_mm") + ".csv";
            using (var outfile = new StreamWriter(_path))
            {
                outfile.WriteLine("User(Avrg);True Positive;False Negative;True Negative;False Positive;" +
                                  ";User(Best);True Positive(Max);False Negative;True Negative;False Positive" +
                                  ";User(Worst);True Positive(Max);False Negative;True Negative;False Positive"
                    );

                
                var parialResults = new List<ExternalSignatureCompareResult>[users];

                for (int u = 0; u < users; u++)
                {
                    parialResults[u] = new List<ExternalSignatureCompareResult>();
                }
               
                for (var i = 1; i < users + 1; i++)
                {
                    var parialResult = new List<ExternalSignatureCompareResult>();
            
                    for (var j = 0; j < iteration; j++)
                    {
                        parialResult.Add( await LoadExternalSignaturesPerUser(path, "U" + i,patternsCount));
                        if ( (j % 10) == 0)
                        {
                            Debug.WriteLine("Done " +i +"Iteration "+j);
                        }
                    }

                    //Priemerna
                    parialResults[i - 1].Add(parialResult.Aggregate(new ExternalSignatureCompareResult() { User = "U" + i }, (a, b) => a + b) / iteration);


                    // Najlepsi vysledok
                    parialResults[i - 1].Add(parialResult.OrderByDescending(p => (p.truePositive  + p.trueNegative)).First());

                    //Najhorsi vysledok

                    parialResults[i - 1].Add(parialResult.OrderByDescending(p => (p.falsePositive + p.falseNegative)).First());

                   
                    Debug.WriteLine("Done " + i);
                }

                foreach (var externalSignatureCompareResult in parialResults)
                {
                   var stringToOutPut =  externalSignatureCompareResult.Aggregate("",(a,b) => a.ToString() + ";" + b.ToString());
                    outfile.WriteLine(stringToOutPut.Remove(0, 1));
                }


                var ftruePositive = parialResults.First().Average(pr => pr.truePositive)*100;
                var ffalseNegative = parialResults.First().Average(pr => pr.falseNegative)*100;
                var ftrueNegative = parialResults.First().Average(pr => pr.trueNegative) * 100;
                var ffalsePositive = parialResults.First().Average(pr => pr.falsePositive) * 100;


                outfile.WriteLine(string.Format("{0};{1};{2};{3};{4}", "U" + " ", ftruePositive.ToString("F4"),
                    ffalseNegative.ToString("F4"), ftrueNegative.ToString("F4"), ffalsePositive.ToString("F4")));
            }
            Debug.WriteLine("DONE");
        }

        private async Task LoadExternalSignaturesMissInput(int iteration , int users ,int patternsCount,string path = @"B:\Task2\Task2")
        {
           var _path = @"B:\" +String.Format("p_{0}_i_{1}_miss",patternsCount,iteration)+ DateTime.Now.ToString("hh_mm") + ".csv";
            using (var outfile = new StreamWriter(_path))
            {
                outfile.WriteLine("UserTrue Negative;False Positive");

                var parialResult = new ExternalSignatureCompareResult[users];

                for (var i = 1; i < users + 1; i++)
                {
                    parialResult[i - 1] = new ExternalSignatureCompareResult() { User = "U" + i };
                    for (var j = 0; j < iteration; j++)
                    {
                        parialResult[i - 1] = parialResult[i - 1] + await CompareExternalSignatureMissInputPerUser(path, "U" + i,patternsCount);
                    }

                    parialResult[i - 1] = parialResult[i - 1] / iteration;
                    Debug.WriteLine("Done user" + i + ":run " + patternsCount);
                }

                foreach (var externalSignatureCompareResult in parialResult)
                {
                    outfile.WriteLine(externalSignatureCompareResult.ToString());
                }


              
                var ftrueNegative = parialResult.Average(pr => pr.trueNegative) * 100;
                var ffalsePositive = parialResult.Average(pr => pr.falsePositive) * 100;


                outfile.WriteLine(string.Format("{0};{1};{2}", "U" + " ", ftrueNegative.ToString("F4"), ffalsePositive.ToString("F4")));
            }
            Debug.WriteLine("DONE");
        }


        private async Task<ExternalSignatureCompareResult> LoadExternalSignaturesPerUser(string directoryPath,
            string userName,int patternCount)
        {
            var di = new DirectoryInfo(directoryPath);
            var files = di.GetFileSystemInfos();
            var fileEntries = files.Where(f => f.Name.Contains(userName))
                .Select(l => l.FullName).ToList();

            //var patterns = RandomList(1, 20, patternCount);
            var patterns = RandomListContinuous(1, 20, patternCount);
            var signatures = Enumerable.Range(1, 40).Except(patterns).ToList();

            signatureContainers.Clear();
            var temporarySecureToken = SupportFunctions.convertToSecureString("temp");
            var sc = new SignatureContainer(temporarySecureToken);


            foreach (var pattern in patterns)
            {
                sc.AddPattern(LoadOneExternalSignature(GetPathOfExternalSignature(directoryPath, userName, pattern)),
                    signatureComparers, temporarySecureToken);
            }

            var ExternalUserAsyncComparer = new ComparePerUserPerPatern(sc, signatureComparers, userName, patterns.Count);

            var signatureToCompare = new List<Tuple<Stroke, int>>();

            foreach (var signature in signatures)
            {
                signatureToCompare.Add(
                    new Tuple<Stroke, int>(
                        LoadOneExternalSignature(GetPathOfExternalSignature(directoryPath, userName, signature)),
                        signature));
            }


            return await ExternalUserAsyncComparer.Compare(signatureToCompare);
        }

        private async Task<ExternalSignatureCompareResult> CompareExternalSignatureMissInputPerUser(
            string directoryPath, string userName,int patternsCount)
        {
            var di = new DirectoryInfo(directoryPath);
            var files = di.GetFileSystemInfos();
            var fileEntries = files.Where(f => f.Name.Contains(userName))
                .Select(l => l.FullName).ToList();

            var patterns = RandomList(1, 20, patternsCount);

          
            signatureContainers.Clear();
            var temporarySecureToken = SupportFunctions.convertToSecureString("temp");
            var sc = new SignatureContainer(temporarySecureToken);


            foreach (var pattern in patterns)
            {
                sc.AddPattern(LoadOneExternalSignature(GetPathOfExternalSignature(directoryPath, userName, pattern)),
                    signatureComparers, temporarySecureToken);
            }

            var ExternalUserAsyncComparer = new ComparePerUserPerPatern(sc, signatureComparers, userName, patterns.Count);

            var signatureToCompare = new List<Tuple<Stroke, int>>();

            var userRelatedSignatures =
                Enumerable.Range(1, 40).Select(p => String.Format(userName + "S" + p + ".TXT")).ToList();

            var otherSignaturesPaths =
                files.Where(f => !userRelatedSignatures.Contains(f.Name)).Select(f => f.FullName).ToList();

            var otherSignatures = otherSignaturesPaths.Select(LoadOneExternalSignature).ToList();


            foreach (var signature in otherSignatures)
            {
                signatureToCompare.Add(
                    new Tuple<Stroke, int>(signature, int.MaxValue));

            }

            return await ExternalUserAsyncComparer.Compare(signatureToCompare);
        }

        private string GetPathOfExternalSignature(string path, string name, int index)
        {
            return path + "\\" + name + "S" + index + ".TXT";
        }

        private List<int> RandomList(int i, int i1, int i2)
        {
            var result = new List<int>();
            var r = new Random();

            while (result.Count != i2)
            {
                var posibleNumber = r.Next(i, i1); //for doubles

                if (!result.Contains(posibleNumber))
                {
                    result.Add(posibleNumber);
                }
            }

            return result;
        }

        private List<int> RandomListContinuous(int i, int i1, int i2)
        {
            var result = new List<int>();
            var r = new Random();

            var posibleNumber = r.Next(i, 4); //for doubles
            result.Add(posibleNumber);
            while (result.Count != i2)
            {
                posibleNumber++;
                if (!result.Contains(posibleNumber))
                {
                    result.Add(posibleNumber);
                }
            }

            return result;
        }


        private class protoPoint
        {
            private readonly double scale = 30d;

            public protoPoint(int x, int y, int originPresure, int timeStamp)
            {
                X = x/scale;
                Y = y/scale;
                OriginPresure = originPresure;
                TimeStamp = timeStamp;
            }

            public double X { get; set; }
            public double Y { get; set; }

            public float Presure { get; set; }

            public int TimeStamp { get; set; }
            public int OriginPresure { get; set; }
        }

        private Stroke LoadOneExternalSignature(string path)
        {
            FileStream fs = null;
            var strokeCollection = new StrokeCollection();
            try
            {
                var fileInfo = new FileInfo(path);

                var lines = File.ReadLines(path);
                var protoPoints = new List<protoPoint>();
                var pointCollection = new StylusPointCollection();

                foreach (var line in lines)
                {
                    var chunk = line.Split();
                    if (chunk.Length < 2)
                        continue;

                    protoPoints.Add(new protoPoint(int.Parse(chunk[0]), int.Parse(chunk[1]), int.Parse(chunk[6]),
                        int.Parse(chunk[2])));
                    if (chunk[3] == "0")
                    {
                        protoPoints.Last().OriginPresure = 0;
                    }
                }


                var minimalPresure = 0;
                var maximalPresuler = protoPoints.Max(p => p.OriginPresure);


                for (var index = 0; index < protoPoints.Count; index++)
                {
                    protoPoints[index].Presure = protoPoints[index].OriginPresure/(float) maximalPresuler;
                }

                foreach (var protoPoint in protoPoints)
                {
                    pointCollection.Add(new StylusPoint(protoPoint.X, protoPoint.Y, protoPoint.Presure));
                }

                return new Stroke(pointCollection);
                //return strokeCollection = new StrokeCollection(new List<Stroke>(){});
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        private bool CompareSignature(StrokeCollection reCreateStrokes, string addInfo, out SecureString token)
        {
            var threshold = 0d;
            var _token = new SecureString();
            var tokens = new List<SecureString>();
            var thresholds = new List<double>();
            var similarityResult = new List<bool>();

            foreach (
                var similarity in
                    signatureContainers.Select(
                        container => container.isSimilar(reCreateStrokes.First(), signatureComparers, out threshold,
                            out _token)))
            {
                tokens.Add(_token);
                thresholds.Add(threshold);
                similarityResult.Add(similarity);
            }


            var similarSignatureFound = similarityResult.Count(i => i) == 1;

            var treshold = similarSignatureFound ? thresholds[similarityResult.IndexOf(true)] : 0d;
            token = similarSignatureFound ? tokens[similarityResult.IndexOf(true)] : new SecureString();

            DrawnSignature(reCreateStrokes, similarSignatureFound, treshold, addInfo);

            return similarSignatureFound;
        }

        private bool CompareSignatureP(StrokeCollection reCreateStrokes)
        {
            var threshold = 0d;
            var _token = new SecureString();
            var tokens = new List<SecureString>();
            var thresholds = new List<double>();
            var similarityResult = new List<bool>();

            similarityResult =
                signatureContainers.AsParallel()
                    .Select(c => c.isSimilar(reCreateStrokes.First(), signatureComparers))
                    .ToList();


            var similarSignatureFound = similarityResult.Count(i => i) == 1;


            return similarSignatureFound;
        }


        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => LoadOneSignature(e.FullPath));
        }

        private void LoadOneSignature(string fileEntry)
        {
            FileStream fs = null;
            var strokeCollection = new StrokeCollection();
            try
            {
                fs = new FileStream(fileEntry, FileMode.Open);
                var fileInfo = new FileInfo(fileEntry);

                strokeCollection = new StrokeCollection(fs);
                var token = new SecureString();
                CompareSignature(strokeCollection, fileInfo.CreationTime.ToLongDateString(), out token);
            }
            catch (Exception)
            {
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        private void AddComparers()
        {
            signatureComparers.Add(new _accelerationPresure().SetSettings(DistanceMeasure.Euclidean, true, false));
            signatureComparers.Add(new _accelerationX().SetSettings(DistanceMeasure.Euclidean, true, false));
            signatureComparers.Add(new _accelerationY().SetSettings(DistanceMeasure.Euclidean, true, false));
            signatureComparers.Add(new _dynamicX().SetSettings(DistanceMeasure.Euclidean, true, false));
            signatureComparers.Add(new _dynamicY().SetSettings(DistanceMeasure.Euclidean, true, false));
            signatureComparers.Add(new _dynamicPresure().SetSettings(DistanceMeasure.Euclidean, true, false));
            signatureComparers.Add(new _effectiveAverageSpeed());

            foreach (var signatureComparer in signatureComparers)
            {
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PServer2 = new NamedPipeServer(@"\\.\pipe\zero", 1);
            PServer2.Start();
            _host.OpenHost(
                () => { Dispatcher.Invoke(() => { ListBox_Messages.Items.Add("Host started"); }); });
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var newMessages = DataPool.Instance().GetNewData(_maxDateTime);
            if (newMessages == null) return;
            foreach (var msg in newMessages)
            {
                var decodetMsg = Encoding.Default.GetBytes(msg.Message);
                var reCreate = new MemoryStream(decodetMsg);
                var reCreateStrokes = new StrokeCollection(reCreate);


                if (signatureContainers.Any(sc => sc.CanCompare()))
                {
                    var token = new SecureString();
                    var treshold = 0d;
                    var isSimilar = CompareSignature(reCreateStrokes, msg.DateTime.ToLongDateString(), out token);

                    try
                    {
                        if (isSimilar)
                        {
                            NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "SignatureVerifiedSilent"});
                            PServer2.SendMessage(token.ToString(), PServer2.clientse);
                        }
                        else
                        {
                            NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "SignatureDenied"});
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    signatureContainers.First().AddPattern(reCreateStrokes.First(), signatureComparers, null);
                    NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "SignatureRecorded"});
                    DrawnSignature(reCreateStrokes, null, 0d, msg.DateTime);
                }
            }
            _maxDateTime = newMessages.Max(p => p.DateTime);
        }


        private void DrawnSignature(StrokeCollection signature, bool compareResult, double treshold, string addText)
        {
            var InkCanvas = new InkCanvas();
            var inkBorder = new Border();
            inkBorder.BorderThickness = new Thickness(1);
            inkBorder.BorderBrush = Brushes.Black;
            InkCanvas.EditingMode = InkCanvasEditingMode.None;
            InkCanvas.Strokes = signature;
            inkBorder.Child = InkCanvas;


            ListBox_Messages.Items.Add(addText + " : " + compareResult + " (" + treshold + ")");
            ListBox_Messages.Items.Add(inkBorder);
            ListBox_Messages.ScrollIntoView(inkBorder);

            if (compareResult)
            {
                InkCanvas.Strokes.First().DrawingAttributes.Color = Colors.Green;
            }
            else
            {
                InkCanvas.Strokes.First().DrawingAttributes.Color = Colors.Red;
            }
        }

        private void DrawnSignature(StrokeCollection signature, bool? compareResult, double treshold, DateTime date)
        {
            var InkCanvas = new InkCanvas();
            var inkBorder = new Border();
            inkBorder.BorderThickness = new Thickness(1);
            inkBorder.BorderBrush = Brushes.Black;
            InkCanvas.EditingMode = InkCanvasEditingMode.None;
            InkCanvas.Strokes = signature;
            inkBorder.Child = InkCanvas;

            ListBox_Messages.Items.Add(date + " : " + compareResult + " (" + treshold + ")");
            ListBox_Messages.Items.Add(inkBorder);
            ListBox_Messages.ScrollIntoView(inkBorder);

            if (compareResult.HasValue && compareResult.Value)
            {
                InkCanvas.Strokes.First().DrawingAttributes.Color = Colors.Green;
            }
            else
            {
                InkCanvas.Strokes.First().DrawingAttributes.Color = Colors.Red;
            }

            if (!compareResult.HasValue)
                InkCanvas.Strokes.First().DrawingAttributes.Color = Colors.DodgerBlue;
        }

        private static Color SignatureDenied()
        {
            //NamedPipeBindingService.CallClients(new CommunicatUnit() { Message = "False" });
            return Colors.Red;
        }

        private static Color SignatureVerified()
        {
            //pokraNamedPipeBindingService.CallClients(new CommunicatUnit() { Message = "True" });
            return Colors.Green;
        }


        private void Button_SendClientList_Click(object sender, RoutedEventArgs e)
        {
            NamedPipeBindingService.DiffuseClientList();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Signature Container (*.sc)|*.sc|All(*.*)|*",
                DefaultExt = ".sc",
                AddExtension = true
            };

            if (dialog.ShowDialog() == true)
            {
                using (Stream stream = File.Create(dialog.FileName))
                {
                    var bformatter = new BinaryFormatter();

                    bformatter.Serialize(stream, testContainer);
                }

                MessageBox.Show("Signatures saved");
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Signature Container (*.sc)|*.sc|All(*.*)|*",
                DefaultExt = ".sc",
                AddExtension = true
            };
            if (dialog.ShowDialog() == true)
            {
                var stream = File.Open(dialog.FileName, FileMode.Open);
                var bformatter = new BinaryFormatter();

                Console.WriteLine("Reading signature container");
                signatureContainers.Clear();
                signatureContainers.Add((SignatureContainer) bformatter.Deserialize(stream));
                stream.Close();
                MessageBox.Show("Signatures loaded");
            }
        }


        private void Vizualize_Click(object sender, RoutedEventArgs e)
        {
            Stroke firstStroke = null;
            Stroke secondStroke = null;
            if (ListBox_Messages.SelectedItems.Count == 2)
            {
                var first = (Border) ListBox_Messages.SelectedItems[0];
                var second = (Border) ListBox_Messages.SelectedItems[1];

                var firstCanvas = (InkCanvas) first.Child;
                var secondCanvas = (InkCanvas) second.Child;

                firstStroke = firstCanvas.Strokes.First();
                secondStroke = secondCanvas.Strokes.First();

                var test = new GuiTest();

                foreach (var signatureComparer in signatureComparers)
                {
                    try
                    {
                        //var s = signatureComparer.drawnGui(firstStroke, secondStroke);
                        //test.GuiTestPanel.Children.Add(s);
                    }
                    catch (Exception)
                    {
                    }
                }

                test.Show();
            }

            if (ListBox_Messages.SelectedItems.Count == 1)
            {
                var first = (Border) ListBox_Messages.SelectedItems[0];
                var firstCanvas = (InkCanvas) first.Child;

                firstStroke = firstCanvas.Strokes.First();
                secondStroke = firstCanvas.Strokes.First();

                var test = new GuiTest();

                var PaternsUiElements =
                    signatureContainers.SelectMany(sc => sc.DrawnComparerUi(firstStroke, signatureComparers)).ToList();

                foreach (var paternsUiElement in PaternsUiElements)
                {
                    test.GuiTestPanel.Children.Add(paternsUiElement);
                }

                test.Show();
            }
        }

        private void SignatureContainerInfo(object sender, RoutedEventArgs e)
        {
            var guiTest = new GuiTest();

            var PaternsUiElements =
                    signatureContainers.Select(sc => sc.GetUiInfo(signatureComparers)).ToList();

            foreach (var paternsUiElement in PaternsUiElements)
            {
                guiTest.GuiTestPanel.Children.Add(paternsUiElement);
            }

            guiTest.GuiTestPanel.Children.Add(new TextBox
            {
                Margin = new Thickness(10),
                Text = string.Format("Treshold:{0}", testContainer.Threshold)
            });
            guiTest.Show();
            ;
        }


        private async void LoadExternalPattern_Click(object sender, RoutedEventArgs e)
        {
             for (int i =3; i < 7; i++)
            {
                await LoadExternalSignatures(10, 40, i);
                         }




        }

        private async void MissInput_Click(object sender, RoutedEventArgs e)
        {
            for (int i =3; i < 7; i++)
            {
                await LoadExternalSignaturesMissInput(2,40,i);
            }
            
        }

        private void SignForm_Click(object sender, RoutedEventArgs e)
        {
            string cInstalation = @"C:\Signature\SignForm\SignForm.exe";
            if (File.Exists(cInstalation))
            {
                System.Diagnostics.Process.Start(cInstalation);
            }
            else
            {
                string baseInstalation = @"SignForm.exe";
                if (File.Exists(baseInstalation))
                {
                    System.Diagnostics.Process.Start(baseInstalation);
                }
            }

        }
    }
}