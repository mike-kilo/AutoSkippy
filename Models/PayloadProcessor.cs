using AutoSkippy.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSkippy.Models;

public partial class PayloadProcessor(ComPortComm communicator) : ObservableObject
{
    public class LineReceivedEventArgs : EventArgs
    {
        public required string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public event EventHandler<LineReceivedEventArgs>? LineReceived;

    public event EventHandler? Progressed;

    public event EventHandler<string>? RecentCommand;

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private bool _isBreakRequested = false;

    public ComPortComm Communicator { get; private set; } = communicator;

    private async Task<string?> ProcessScpiLine(string line)
    {
        if (!Communicator.IsConnected) return null;
        RecentCommand?.Invoke(this, line);
        await Task.Run(() => Communicator.Send(line));
        var received = string.Empty;
        if (line.EndsWith('?'))
        {
            await Task.Delay(ComPortComm.TIMEOUT / 2);
            received = await Communicator.ReadAsync();
        }
        else
        {
            await Task.Delay(ComPortComm.TIMEOUT);
        }

        Progressed?.Invoke(this, new EventArgs());
        if (!string.IsNullOrEmpty(received))
        { 
            LineReceived?.Invoke(this, new LineReceivedEventArgs() { Text = received, Timestamp = DateTime.Now }); 
        }
        
        return string.IsNullOrEmpty(received) ? null : received.Trim();
    }

    public async Task Process(ScpiPayload payload)
    {
        if (!Communicator.IsConnected) return;
        if (payload is null) return;

        IsProcessing = true;

        foreach (var line in payload.SetupLines.Split(Environment.NewLine))
        {
            if (IsBreakRequested) break;
            await ProcessScpiLine(line);
        }

        for (int l = 0; l < payload.LoopCount; l++)
        {
            if (IsBreakRequested) break;
            foreach (var line in payload.LoopLines.Split(Environment.NewLine))
            {
                var trimmed = new string([ .. line.TakeWhile(c => c != '?' && c != ' ')]);
                if (trimmed is not null && ScpiPayload.IsInSet(trimmed, payload.PreFetchAppliedCommands.Split(Environment.NewLine)))
                {
                    for (int w = 0; w < payload.PreFetchDelay; w++)
                    {
                        if (IsBreakRequested) break;
                        await Task.Delay(1000);
                    }
                }

                if (IsBreakRequested) break;
                await ProcessScpiLine(line);
            }
        }

        foreach (var line in payload.TeardownLines.Split(Environment.NewLine))
        {
            if (IsBreakRequested) break;
            await ProcessScpiLine(line);
        }

        IsProcessing = false;
        IsBreakRequested = false;
    }

    partial void OnIsBreakRequestedChanged(bool value)
    {
        if (value && Communicator.IsConnected) Communicator.Cancel();
    }
}
