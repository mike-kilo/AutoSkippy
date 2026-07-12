using AutoSkippy.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoSkippy.Models;

public partial class PayloadProcessor(ComPortComm communicator) : ObservableObject
{
    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private int _progressStep = 0;

    [ObservableProperty]
    private bool _isBreakRequested = false;

    public ComPortComm Communicator { get; private set; } = communicator;

    private async Task<string?> ProcessScpiLine(string line)
    {
        if (!Communicator.IsConnected) return null;
        Communicator.Send(line);
        var received = string.Empty;
        if (line.EndsWith('?'))
        {
            Thread.Sleep(ComPortComm.TIMEOUT / 2);
            received = await Communicator.ReadAsync();
        }
        else
        {
            Thread.Sleep(ComPortComm.TIMEOUT);
        }

        ProgressStep++;
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
