using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoSkippy.Models;

public partial class ComPortComm : ObservableObject
{
    public static readonly int TIMEOUT = 1000;

    private readonly CancellationTokenSource _cancellationTokenSource = new(TIMEOUT);

    public static string[] GetPorts() => [ .. SerialPort.GetPortNames().Distinct()];

    private SerialPort? _serialPort;

    [ObservableProperty]
    public partial bool IsConnected { get; set; } = false;

    public bool OpenConnection(string portName, int baudRate = 9600, bool rts = false)
    {
        IsConnected = false;

        try
        {
            _serialPort = new SerialPort(portName)
            {
                BaudRate = baudRate,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                RtsEnable = rts,
                ReadTimeout = TIMEOUT,
                WriteTimeout = TIMEOUT,
            };

            if (!_serialPort.IsOpen)
            {
                try
                { 
                    _serialPort.Open(); 
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return false;
                }

                IsConnected = true;
                return true;
            }
        }
        catch (UnauthorizedAccessException)
        {
            Debug.WriteLine($"Access denied to {portName}. Port may be in use.");
            return false;
        }
        catch (ArgumentException)
        {
            Debug.WriteLine($"Invalid port name: {portName}");
            return false;
        }
        catch (InvalidOperationException)
        {
            Debug.WriteLine($"Port {portName} is already open.");
            return false;
        }

        return false;
    }

    public void CloseConnection()
    {
        IsConnected = false;
        if (_serialPort?.IsOpen == true)
        {
            _serialPort.Close();
            _serialPort.Dispose();
            Debug.WriteLine("Serial connection closed successfully.");
        }
    }

    public string Read()
    {
        if (!IsConnected || _serialPort is null)
        {
            Debug.WriteLine("Port is not open for sending data.");
            return string.Empty;
        }

        try
        {
            var data = _serialPort.ReadLine();
            Debug.WriteLine($"Received: {data}");
            return data;
        }
        catch (TimeoutException)
        {
            Debug.WriteLine("Timeout occurred while sending data.");
            return string.Empty;
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"Error sending data: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<string> ReadAsync()
    {
        if (_serialPort is null) return string.Empty;
        var buffer = new char[4096];
        int count = 0;
        try
        {
            _cancellationTokenSource.TryReset();
            count = await Task.Run(() => count = _serialPort.Read(buffer, 0, 4096))
                .ContinueWith(t => t.Status == TaskStatus.RanToCompletion ? t.Result : 0, TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
            return string.Empty; 
        }

        if (count == 0) return string.Empty;

        return new string([.. buffer.Take(count)]);
    }

    public bool Send(string data)
    {
        if (!IsConnected || _serialPort is null)
        {
            Debug.WriteLine("Port is not open for sending data.");
            return false;
        }

        try
        {
            _serialPort.WriteLine(data);
            Debug.WriteLine($"Sent: {data}");
            return true;
        }
        catch (IOException)
        {
            Debug.WriteLine("IOException when sending data.");
            return false;
        }
        catch (TimeoutException)
        {
            Debug.WriteLine("Timeout occurred while sending data.");
            return false;
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"Error sending data: {ex.Message}");
            return false;
        }
    }

    public void Cancel() => _cancellationTokenSource.Cancel();
}
