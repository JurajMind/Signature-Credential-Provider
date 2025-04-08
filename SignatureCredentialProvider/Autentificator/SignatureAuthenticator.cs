using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Windows.Ink;
using System.Windows.Threading;
using SignatureVerification;
using WcfServiceLibraryNamedPipe;
using WpfWcfNamedPipeBinding;
using NamedPipeBindingService = WpfWcfNamedPipeBinding.NamedPipeBindingService;

namespace Autentificator
{
    public partial class SignatureAuthenticator : ServiceBase
    {
        private readonly Host _host;
        private readonly List<SignatureContainer> signatureContainers = new List<SignatureContainer>();
        private DispatcherTimer _dispatcherTimer;
        private DateTime _maxDateTime = DateTime.Now;
        private bool _newSignatureRecording;
        private Timer _recordTimer;
        private Timer _timer;
        private SignatureContainer editedContainer = new SignatureContainer();
        private SecureString editedToken = new SecureString();
        private NamedPipeServer namePipeServer;
        public List<SignatureComparer> signatureComparers = new List<SignatureComparer>();

        public SignatureAuthenticator()
        {
            InitializeComponent();
            _host = Host.Instance();
        }

        /// <summary>
        ///     Requered function for services
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            //Debugger.Launch();
            namePipeServer = new NamedPipeServer(@"\\.\pipe\zero", 1);
            namePipeServer.Start();
            _host.OpenHost(() => { });
            CreateTimer();

            LoadUserPatterns();
            AddComparers();
        }

        /// <summary>
        ///     Function responsible for loading user patterns
        /// </summary>
        private void LoadUserPatterns()
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
        }

        /// <summary>
        ///     Function that save all user patterns
        /// </summary>
        private void SaveUserPattern()
        {
            using (var stream = File.OpenWrite(string.Format(@"C:\Signature\UserPaterns\{0}.sc", editedContainer.Name)))
            {
                lock (stream)
                {
                    var bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, editedContainer);
                    NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "User patterns saved"});

                    LoadUserPatterns();
                }
            }
        }

        private void CreateTimer()
        {
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Elapsed += _dispatcherTimer_Tick;
            _timer.Enabled = true;
        }

        /// <summary>
        ///     Function that create record timer ,that will reset service to authentificaion state
        /// </summary>
        private void CreateRecordTimer()
        {
            _recordTimer = new Timer();
            _recordTimer.Interval = 40000;
            _recordTimer.Elapsed += _RecordTimeEnd;
            _recordTimer.Enabled = true;
        }

        /// <summary>
        ///     Function that send mesege to sign form to provide visual feedback when signature is denied
        /// </summary>
        private static void SignatureDenied()
        {
            NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "SignatureDenied"});
        }

        /// <summary>
        ///     Function that send mesege to sign form to provide visual feedback when signature is verified
        /// </summary>
        private static void SignatureVerified()
        {
            NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "SignatureVerified"});
        }

        /// <summary>
        ///     Function that send mesege to sign form to provide visual feedback when signature is recorded
        /// </summary>
        private void SignatureRecorded()
        {
            NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "SignatureRecorded"});
        }

        protected override void OnStop()
        {
        }

        /// <summary>
        ///     Initialization of comparers
        /// </summary>
        private void AddComparers()
        {
            signatureComparers.Add(new _accelerationPresure());
            signatureComparers.Add(new _accelerationX());
            signatureComparers.Add(new _accelerationY());
            signatureComparers.Add(new _dynamicX());
            signatureComparers.Add(new _dynamicY());
            signatureComparers.Add(new _dynamicPresure());
            signatureComparers.Add(new _effectiveAverageSpeed());
        }

        /// <summary>
        ///     Function for saving signature for research purpouse
        /// </summary>
        /// <param name="stroke">Signature stroke that will be saved</param>
        /// <param name="autentification">if stroke is valid or invalid</param>
        private void SaveSignature(StrokeCollection stroke, bool autentification)
        {
            // Debugger.Launch();
            var directory = @"C:\Signature\SavedSignatures";
            var fileEntries = Directory.GetFiles(directory);
            var dateStamp = DateTime.Now.ToString("dd-mm-yyyy");

            var count = fileEntries.Count(e => e.Contains(dateStamp));

            var fileName = directory + "\\" + dateStamp + "_" + count + "_" + autentification;
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Create);
                stroke.Save(fs, true);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            EventLog.Source = ServiceName;
            EventLog.Log = @"Application";
            var newMessages = DataPool.Instance().GetNewData(_maxDateTime);
            if (newMessages == null) return;
            foreach (var msg in newMessages)
            {
                EventLog.WriteEntry("Signature received", EventLogEntryType.Information);

                //Service msg from credentialManager
                if (msg.Sender.Contains("Manager"))
                {
                    ManagerCommands(msg);
                }

                if (msg.Sender.Contains("Form"))
                {
                    var incomingMemStream = Encoding.Default.GetBytes(msg.Message);
                    var reCreate = new MemoryStream(incomingMemStream);
                    var reCreateStrokes = new StrokeCollection(reCreate);

                    if (_newSignatureRecording)
                    {
                        editedContainer.AddPattern(reCreateStrokes.First(), signatureComparers, editedToken);
                        SignatureRecorded();
                    }
                    else
                    {
                        if (signatureContainers.Any(sc => sc.CanCompare()))
                        {
                            CompareSignature(reCreateStrokes);
                        }
                    }
                }
                _maxDateTime = newMessages.Max(p => p.DateTime);
            }
        }

        /// <summary>
        ///     Record timer elapse function, server for proper set to authentification state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _RecordTimeEnd(object sender, EventArgs e)
        {
            if (_newSignatureRecording)
            {
                _newSignatureRecording = false;
                LoadUserPatterns();
            }
            _recordTimer.Enabled = false;
        }

        private void CompareSignature(StrokeCollection reCreateStrokes)
        {
            var threshold = 0d;
            var token = new SecureString();
            var tokens = new List<SecureString>();
            var thresholds = new List<double>();
            var similarityResult = new List<bool>();

            foreach (
                var similarity in
                    signatureContainers.Select(
                        container => container.isSimilar(reCreateStrokes.First(), signatureComparers, out threshold,
                            out token)))
            {
                tokens.Add(token);
                thresholds.Add(threshold);
                similarityResult.Add(similarity);
            }


            var similarSignatureFound = similarityResult.Count(i => i) == 1;


            try
            {
                if (similarSignatureFound)
                {
                    var indexOfSimilarPatern = similarityResult.IndexOf(true);
                    SaveSignature(reCreateStrokes, true);

                    token = tokens[indexOfSimilarPatern];

                    EventLog.WriteEntry("Access granted", EventLogEntryType.Information);
                    try
                    {
                        namePipeServer.SendMessage(token.SecureStringToString(), namePipeServer.clientse);
                    }
                    catch (NullReferenceException)
                    {
                    }

                    SignatureVerified();
                }
                else
                {
                    SignatureDenied();
                    EventLog.WriteEntry("Access denied!");

                    if (similarityResult.Count(i => i) > 1)
                    {
                        EventLog.WriteEntry("Multiple similar  user patterns!");
                    }
                }
            }
            catch (Exception ex)
            {
                SignatureDenied();
                EventLog.WriteEntry("Error!: " + ex.Message, EventLogEntryType.Information);
            }
        }

        /// <summary>
        ///     Function that encapsuleta manage commands
        /// </summary>
        /// <param name="msg"> message from Credential manager</param>
        private void ManagerCommands(DataUnit msg)
        {
            while (true)
            {
                if (msg.Message.StartsWith("Record"))
                {
                    var msgs = msg.Message.Split('#');
                    editedToken = SupportFunctions.convertToSecureString(msgs[1]);
                    _newSignatureRecording = true;
                    CreateRecordTimer();

                    NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "ACK Record "});
                    var containerFound = false;

                    foreach (var container in signatureContainers)
                    {
                        if (container.Check(editedToken))
                        {
                            containerFound = true;
                            editedContainer = container;
                            editedContainer.ClearPatterns(editedToken);

                            if (msgs.Length > 2)
                            {
                                //used for changing token
                                editedContainer.ChangeToken(editedToken,
                                    SupportFunctions.convertToSecureString(msgs[2]));
                            }
                        }
                    }

                    //Container not found, new container
                    if (!containerFound)
                    {
                        editedContainer = new SignatureContainer(editedToken);
                    }


                    break;
                }

                if (msg.Message.StartsWith("Change"))
                {
                    _newSignatureRecording = false;
                    NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "ACK Change "});
                    var msgs = msg.Message.Split('#');
                    editedToken = SupportFunctions.convertToSecureString(msgs[1]);
                    _newSignatureRecording = false;
                    CreateRecordTimer();

                    var containerFound = false;

                    foreach (var container in signatureContainers)
                    {
                        if (container.Check(editedToken))
                        {
                            containerFound = true;
                            editedContainer = container;

                            if (msgs.Length > 2)
                            {
                                //used for changing token
                                editedContainer.ChangeToken(editedToken,
                                    SupportFunctions.convertToSecureString(msgs[2]));
                            }
                        }
                    }

                    //Container not found, new container
                    if (!containerFound)
                    {
                        editedContainer = new SignatureContainer(editedToken);
                    }


                    break;
                }


                if (msg.Message.StartsWith("StopRecord"))
                {
                    _newSignatureRecording = false;
                    NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "ACK StopRecord "});
                    break;
                }


                if (msg.Message.StartsWith("Save"))
                {
                    _newSignatureRecording = false;
                    SaveUserPattern();
                    NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "ACK Save "});
                    break;
                }
                if (msg.Message.StartsWith("Discard"))
                {
                    _newSignatureRecording = false;
                    NamedPipeBindingService.CallClients(new CommunicatUnit {Message = "ACK Discard "});
                    LoadUserPatterns();
                    _recordTimer.Enabled = false;
                }
                break;
            }
        }
    }
}