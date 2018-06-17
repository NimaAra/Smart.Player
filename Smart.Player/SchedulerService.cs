namespace Smart.Player
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Easy.MessageHub;

    public sealed class SchedulerService : IDisposable
    {
        private readonly IMessageHub _hub;
        private readonly Timer _timer;
        private readonly List<KeyValuePair<DateTime, Channel>> _schedules;

        public SchedulerService(IMessageHub hub)
        {
            _hub = hub;
            _timer = new Timer { Interval = 1000 };
            _timer.Tick += OnTimerTick;
            _schedules = new List<KeyValuePair<DateTime, Channel>>(GetSchedule());
        }

        public void Start() => _timer.Enabled = true;
        public void Stop() => _timer.Enabled = false;
        public void Dispose() => _timer?.Dispose();

        private void OnTimerTick(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            for (var i = 0; i < _schedules.Count; i++)
            {
                var item = _schedules[i];
                var dateTime = item.Key;
                var channel = item.Value;

                var delta = dateTime - now;
                var isDueSoon = delta <= TimeSpan.FromMinutes(15);
                var isStillPlaying = dateTime.AddMinutes(105) >= now;

                if (isDueSoon)
                {
                    _schedules.Remove(item);

                    if (isStillPlaying)
                    {
                        _hub.Publish(new SchedulerMessage(dateTime, channel));
                        break;
                    }
                }
            }
        }

        private static KeyValuePair<DateTime, Channel>[] GetSchedule()
            => File.ReadAllLines(new FileInfo("Schedule.txt").FullName)
                .Select(l =>
                {
                    var fields = l.Split(',');
                    var dateTimeStr = fields[0].Trim();
                    var dateTime = DateTime.ParseExact(
                        dateTimeStr, "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture);

                    var channelStr = fields[1].Trim();
                    Channel channel;
                    if (channelStr.Equals("BBC", StringComparison.OrdinalIgnoreCase))
                    {
                        channel = Channel.BBC1;
                    }
                    else if (channelStr.Equals("ITV", StringComparison.OrdinalIgnoreCase))
                    {
                        channel = Channel.ITV1;
                    }
                    else
                    {
                        throw new InvalidDataException("Invalid channel: " + channelStr);
                    }

                    return new KeyValuePair<DateTime, Channel>(dateTime, channel);
                })
                .ToArray();
    }

    public sealed class SchedulerMessage
    {
        public SchedulerMessage(DateTime schedule, Channel channel)
        {
            Schedule = schedule;
            Channel = channel;
        }

        public DateTime Schedule { get; }
        public Channel Channel { get; }
    }
}