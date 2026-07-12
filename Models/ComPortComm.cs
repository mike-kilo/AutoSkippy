using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoSkippy.Models;

public static class ComPortComm
{
    public static readonly int TIMEOUT = 1000;

    private static CancellationTokenSource _cancellationTokenSource = new(TIMEOUT);

    public static string[] GetPorts() => SerialPort.GetPortNames();

    private static SerialPort? _serialPort;

    public static bool IsConnected => _serialPort is not null;

    public static bool OpenConnection(string portName, int baudRate = 9600, bool rts = false)
    {
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

    public static void CloseConnection()
    {
        if (_serialPort?.IsOpen == true)
        {
            _serialPort.Close();
            _serialPort.Dispose();
            Debug.WriteLine("Serial connection closed successfully.");
        }
    }

    public static string Read()
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

    public static async Task<string> ReadAsync()
    {
        if (_serialPort is null) return string.Empty;
        var buffer = new Memory<byte>();
        int count = 0;
        try
        {
            count = await _serialPort.BaseStream.ReadAsync(buffer, _cancellationTokenSource.Token);
        }
        catch
        { 
            return string.Empty; 
        }

        if (count == 0) return string.Empty;
        
        return new string([ .. buffer[..count].ToArray().Select(b => (char)b)]);
    }

    public static bool Send(string data)
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

    public static void Cancel() => _cancellationTokenSource.Cancel();
}
