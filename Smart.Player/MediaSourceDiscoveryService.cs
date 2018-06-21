namespace Smart.Player
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public sealed class MediaSourceDiscoveryService : IDisposable
    {
        private readonly WebBrowser _browser;
        private readonly Dictionary<Channel, Uri> _channelsMap;
        private readonly Regex _sourceRegex;
        private readonly ManualResetEventSlim _waitHandle;

        private TaskCompletionSource<Uri> _reqeust;

        public MediaSourceDiscoveryService()
        {
            _sourceRegex = new Regex(@"<source\ssrc=""(?<src>.*m3u8\?.*)""\stype");
            _waitHandle = new ManualResetEventSlim(true);

            _browser = new WebBrowser
            {
                Visible = true,
                ScriptErrorsSuppressed = true
            };

            _browser.DocumentCompleted += OnDocumentCompleted;

            _channelsMap = new Dictionary<Channel, Uri>
            {
                [Channel.BBC1] = new Uri(@"http://www.tvcatchup.com/watch/bbcone"),
                [Channel.BBC2] = new Uri(@"http://www.tvcatchup.com/watch/bbctwo"),
                [Channel.BBC4] = new Uri(@"http://www.tvcatchup.com/watch/bbcfour"),
                [Channel.BBCAlba] = new Uri(@"http://www.tvcatchup.com/watch/bbcalba"),
                [Channel.BBC1Wales] = new Uri(@"http://www.tvcatchup.com/watch/bbconewales"),
                [Channel.BBC1Scotland] = new Uri(@"http://www.tvcatchup.com/watch/bbconescotland"),
                [Channel.BBC1NI] = new Uri(@"http://www.tvcatchup.com/watch/bbconeni"),
                [Channel.BBCNews] = new Uri(@"http://www.tvcatchup.com/watch/bbcnews"),
                [Channel.BBCParliament] = new Uri(@"http://www.tvcatchup.com/watch/bbcparliament"),
                [Channel.BBCRedButton] = new Uri(@"http://www.tvcatchup.com/watch/bbcredbutton"),
                [Channel.CBBC] = new Uri(@"http://www.tvcatchup.com/watch/cbbc"),
                [Channel.CBeebies] = new Uri(@"http://www.tvcatchup.com/watch/cbeebies"),
                [Channel.RT] = new Uri(@"http://www.tvcatchup.com/watch/rt"),
                [Channel.ITV1] = new Uri(@"http://www.tvcatchup.com/watch/itv"),
                [Channel.Five] = new Uri(@"http://www.tvcatchup.com/watch/five"),
                [Channel.Channel4] = new Uri(@"http://www.tvcatchup.com/watch/channel4"),
                [Channel.France24] = new Uri(@"http://www.tvcatchup.com/watch/france24"),
                [Channel.Aljazeera] = new Uri(@"http://www.tvcatchup.com/watch/aljazeera"),
                [Channel.S4C] = new Uri(@"http://www.tvcatchup.com/watch/s4c"),
                [Channel.Quest] = new Uri(@"http://www.tvcatchup.com/watch/quest"),
                [Channel.Together] = new Uri(@"http://www.tvcatchup.com/watch/together"),
                [Channel.MilleniumTV] = new Uri(@"http://www.tvcatchup.com/watch/millenniumtv"),
                [Channel.TVWarehouse] = new Uri(@"http://www.tvcatchup.com/watch/tvwarehouse"),
                [Channel.QVC] = new Uri(@"http://www.tvcatchup.com/watch/qvc"),
                [Channel.QVCBeauty] = new Uri(@"http://www.tvcatchup.com/watch/qvcbeauty"),
                [Channel.QVCStyle] = new Uri(@"http://www.tvcatchup.com/watch/qvcstyle"),
                [Channel.QVCExtra] = new Uri(@"http://www.tvcatchup.com/watch/qvcextra"),
                [Channel.CGTN] = new Uri(@"http://www.tvcatchup.com/watch/cgtn"),
                [Channel.IdealWorld] = new Uri(@"http://www.tvcatchup.com/watch/idealworld"),
                [Channel.IdealExtra] = new Uri(@"http://www.tvcatchup.com/watch/idealextra"),
                [Channel.CreateAndCraft] = new Uri(@"http://www.tvcatchup.com/watch/createandcraft"),
                [Channel.CraftExtra] = new Uri(@"http://www.tvcatchup.com/watch/craftextra")
            };
        }

        public Task<Uri> GetSource(Channel channel)
        {
            _waitHandle.Wait();

            _reqeust = new TaskCompletionSource<Uri>();

            _browser.Navigate(_channelsMap[channel]);
            _waitHandle.Reset();

            return _reqeust.Task;
        }

        public void Dispose()
        {
            _browser?.Dispose();
            _waitHandle?.Dispose();
        }

        private void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = (WebBrowser)sender;

            var src = browser.DocumentText;
            var result = _sourceRegex.Match(src);

            try
            {
                if (!result.Success)
                {
                    throw new InvalidOperationException("Unable to match the input: " + src);
                }

                _reqeust.SetResult(new Uri(result.Groups["src"].Value));
            }
            finally
            {
                _waitHandle.Set();
            }
        }
    }

    public enum Channel
    {
        BBC1,
        BBC2,
        BBC4,
        BBCAlba,
        BBC1Wales,
        BBC1Scotland,
        BBC1NI,
        BBCNews,
        BBCParliament,
        BBCRedButton,
        CBBC,
        CBeebies,

        RT,
        ITV1,
        Five,
        Channel4,
        France24,
        Aljazeera,

        S4C,
        Quest,
        Together,
        MilleniumTV,
        TVWarehouse,
        QVC,
        QVCBeauty,
        QVCStyle,
        QVCExtra,
        CGTN,
        IdealWorld,
        IdealExtra,
        CreateAndCraft,
        CraftExtra
    }
}