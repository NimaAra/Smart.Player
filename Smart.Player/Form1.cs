namespace Smart.Player
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Easy.MessageHub;
    using Vlc.DotNet.Forms;

    public partial class Form1 : Form
    {
        private readonly Label _infoLabel;
        private readonly MessageHub _hub;
        private readonly SchedulerService _schedulerSvc;
        private readonly MediaSourceDiscoveryService _discoverSvc;

        private VlcControl _vlcControl;
        private Channel _currentChannel;
        private bool _isFullscreen;

        public Form1()
        {
            _hub = MessageHub.Instance;

            _hub.Subscribe<SchedulerMessage>(OnSchedulerMessage);

            _discoverSvc = new MediaSourceDiscoveryService();
            _schedulerSvc = new SchedulerService(_hub);
            _schedulerSvc.Start();


            InitializeComponent();
            InitializeVLCControl();
            AddChannelButtons();

            Text = @"Smart Player";

            KeyPreview = true;
            KeyPress += OnKeyPressed;

            _infoLabel = new Label
            {
                Visible = false,
                AutoSize = true,
                Location = new Point(5, 5),
                ForeColor = Color.LimeGreen,
                Font = new Font("Arial", 20),
                Text = @"BBC1"
            };

            Controls.Add(_infoLabel);
            _infoLabel.BringToFront();

            Play(Channel.BBC1);
        }

        private async void OnSchedulerMessage(SchedulerMessage message)
        {
            DisplayInfo($"Switching to: {message.Channel}");
            await Task.Delay(5000);
            Play(message.Channel);
        }

        protected override void OnDeactivate(EventArgs e) => LockWorkStation();
        [DllImport("user32")]
        public static extern void LockWorkStation();

        private void AddChannelButtons()
        {
            var buttons = ((Channel[])Enum.GetValues(typeof(Channel)))
                .Select(c =>
                {
                    var btn = new Button
                    {
                        Width = 75,
                        Height = 75,
                        AutoSize = false,
                        Tag = c,
                        BackgroundImage = GetLogo(c),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Cursor = Cursors.Hand
                    };
                    btn.Click += OnChannelButtonClick;

                    toolTip.SetToolTip(btn, c.ToString());

                    return btn;
                });

            SuspendLayout();

            foreach (var btn in buttons)
            {
                flowPanel.Controls.Add(btn);
            }
            ResumeLayout();
        }

        private static Image GetLogo(Channel channel)
        {
            switch (channel)
            {
                case Channel.BBC1:
                    return new Bitmap(@".\icons\BBC1.png");
                case Channel.BBC2:
                    return new Bitmap(@".\icons\BBC2.gif");
                case Channel.BBC4:
                    return new Bitmap(@".\icons\BBC4.png");
                case Channel.BBCAlba:
                    return new Bitmap(@".\icons\BBCAlba.png");
                case Channel.BBC1Wales:
                    return new Bitmap(@".\icons\BBC1Wales.png");
                case Channel.BBC1Scotland:
                    return new Bitmap(@".\icons\BBC1Scotland.png");
                case Channel.BBC1NI:
                    return new Bitmap(@".\icons\BBC1NI.png");
                case Channel.BBCNews:
                    return new Bitmap(@".\icons\BBCNews.png");
                case Channel.BBCParliament:
                    return new Bitmap(@".\icons\BBCParliament.png");
                case Channel.BBCRedButton:
                    return new Bitmap(@".\icons\BBCRedButton.png");
                case Channel.CBBC:
                    return new Bitmap(@".\icons\CBBC.jpeg");
                case Channel.CBeebies:
                    return new Bitmap(@".\icons\CBeebies.png");
                case Channel.RT:
                    return new Bitmap(@".\icons\RT.png");
                case Channel.ITV1:
                    return new Bitmap(@".\icons\ITV1.png");
                case Channel.Five:
                    return new Bitmap(@".\icons\Five.png");
                case Channel.Channel4:
                    return new Bitmap(@".\icons\Channel4.png");
                case Channel.France24:
                    return new Bitmap(@".\icons\France24.png");
                case Channel.Aljazeera:
                    return new Bitmap(@".\icons\Aljazeera.png");
                case Channel.S4C:
                    return new Bitmap(@".\icons\S4C.png");
                case Channel.Quest:
                    return new Bitmap(@".\icons\Quest.png");
                case Channel.Together:
                    return new Bitmap(@".\icons\Together.png");
                case Channel.MilleniumTV:
                    return new Bitmap(@".\icons\MilleniumTV.png");
                case Channel.TVWarehouse:
                    return new Bitmap(@".\icons\TVWarehouse.png");
                case Channel.QVC:
                    return new Bitmap(@".\icons\QVC.png");
                case Channel.QVCBeauty:
                    return new Bitmap(@".\icons\QVCBeauty.png");
                case Channel.QVCStyle:
                    return new Bitmap(@".\icons\QVCStyle.png");
                case Channel.QVCExtra:
                    return new Bitmap(@".\icons\QVCExtra.png");
                case Channel.CGTN:
                    return new Bitmap(@".\icons\CGTN.png");
                case Channel.IdealWorld:
                    return new Bitmap(@".\icons\IdealWorld.png");
                case Channel.IdealExtra:
                    return new Bitmap(@".\icons\IdealWorldExtra.png");
                case Channel.CreateAndCraft:
                    return new Bitmap(@".\icons\CreateAndCraft.png");
                case Channel.CraftExtra:
                    return new Bitmap(@".\icons\CraftExtra.png");
                default:
                    return new Bitmap(@".\icons\BBC2.gif");
            }
        }

        private async void OnChannelButtonClick(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            var channel = (Channel) btn.Tag;

            if (_currentChannel == channel) { return; }

            await Play(channel);
        }

        private void OnKeyPressed(object sender, KeyPressEventArgs eArgs)
        {
            
            if (eArgs.KeyChar == 'f' || eArgs.KeyChar == 'F') { ToggleFullscreen(); }
            else if (eArgs.KeyChar == 'i' || eArgs.KeyChar == 'I') { DisplayInfo(null); }

        }

        protected override void OnClosing(CancelEventArgs eArgs)
        {
            _discoverSvc.Dispose();
            _schedulerSvc.Dispose();
            _vlcControl.Dispose();
            _hub.Dispose();
            
            base.OnClosing(eArgs);
        }

        private void InitializeVLCControl()
        {
            var vlcControl = new VlcControl
            {
                BackColor = Color.Black,
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Size = new Size(944, 501),
                Spu = -1,
                TabIndex = 0,
                VlcLibDirectory = null,
                VlcMediaplayerOptions = null
            };

            vlcControl.BeginInit();
            SuspendLayout();

            vlcControl.VlcLibDirectoryNeeded += OnVLCLibDirectoryNeeded;
            vlcControl.EncounteredError += (sender, args) =>
            {
                Invoke((MethodInvoker)delegate
                {
                    DisplayInfo("Error");
                });
            };

            vlcControl.Stopped += (sender, args) =>
            {
                Invoke((MethodInvoker)delegate
                {
                    DisplayInfo("Stopped");
                });

                vlcControl.Play();
            };

            vlcControl.Paused += (sender, args) =>
            {
                Invoke((MethodInvoker)delegate
                {
                    DisplayInfo("Paused");
                });
            };

            vlcControl.Buffering += (sender, args) =>
            {
                
            };

            vlcControl.Playing += (sender, args) =>
            {
                Invoke((MethodInvoker) delegate
                {
                    DisplayInfo(_currentChannel.ToString());
                });

            };

            vlcPanel.Controls.Add(vlcControl);

            vlcControl.EndInit();

            ResumeLayout(false);

            _vlcControl = vlcControl;
        }

        private async Task DisplayInfo(string message)
        {
            if (message != null)
            {
                _infoLabel.Visible = false;
                _infoLabel.Text = message;
            }
            
            _infoLabel.Visible = true;
            await Task.Delay(5000);
            _infoLabel.Visible = false;
        }

        private static void OnVLCLibDirectoryNeeded(object sender, VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            var libPath = Path.Combine(currentDirectory, "libvlc");
            e.VlcLibDirectory = new DirectoryInfo(libPath);
        }

        private async Task Play(Channel channel)
        {
            var src = await _discoverSvc.GetSource(channel);
            _currentChannel = channel;
            _vlcControl.Play(src[0]);
        }

        private void ToggleFullscreen()
        {
            if (_isFullscreen)
            {
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.Sizable;
                _isFullscreen = false;
                flowPanel.Visible = true;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                _isFullscreen = true;
                flowPanel.Visible = false;
            }
        }
    }
}