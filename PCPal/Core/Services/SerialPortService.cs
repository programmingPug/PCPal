using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace PCPal.Core.Services;

public interface ISerialPortService
{
    bool IsConnected { get; }
    string CurrentPort { get; }
    Task<bool> ConnectAsync(string port);
    Task<bool> ConnectToFirstAvailableAsync();
    Task DisconnectAsync();
    Task<bool> SendCommandAsync(string command);
    Task<string> AutoDetectDeviceAsync();
    Task<bool> CheckConnectionAsync();
    event EventHandler<bool> ConnectionStatusChanged;
}

public class SerialPortService : ISerialPortService, IDisposable
{
    private SerialPort _serialPort;
    private bool _isConnected;
    private string _currentPort;
    private readonly ILogger<SerialPortService> _logger;
    private bool _disposed = false;

    public bool IsConnected => _isConnected;
    public string CurrentPort => _currentPort;

    public event EventHandler<bool> ConnectionStatusChanged;

    public SerialPortService(ILogger<SerialPortService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _isConnected = false;
        _currentPort = string.Empty;
    }

    public async Task<bool> ConnectAsync(string port)
    {
        if (string.IsNullOrEmpty(port))
        {
            _logger.LogWarning("Cannot connect to null or empty port name");
            return false;
        }

        try
        {
            // Disconnect if already connected
            if (_isConnected)
            {
                await DisconnectAsync();
            }

            // Connect to the specified port
            _serialPort = new SerialPort(port, 115200)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };

            _serialPort.Open();

            // Verify that this is a PCPal device
            _serialPort.WriteLine("CMD:GET_DISPLAY_TYPE");
            await Task.Delay(500); // Wait for response

            string response = _serialPort.ReadExisting();
            if (response.Contains("DISPLAY_TYPE:"))
            {
                _isConnected = true;
                _currentPort = port;
                ConnectionStatusChanged?.Invoke(this, true);
                _logger.LogInformation($"Connected to device on port {port}");
                return true;
            }

            // Not a valid PCPal device, close the connection
            _serialPort.Close();
            _serialPort.Dispose();
            _serialPort = null;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error connecting to port {port}");

            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }

            return false;
        }
    }

    public async Task<bool> ConnectToFirstAvailableAsync()
    {
        string devicePort = await AutoDetectDeviceAsync();

        if (!string.IsNullOrEmpty(devicePort))
        {
            return await ConnectAsync(devicePort);
        }

        return false;
    }

    public async Task DisconnectAsync()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from device");
            }
            finally
            {
                _serialPort = null;
                _isConnected = false;
                _currentPort = string.Empty;
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        await Task.CompletedTask;
    }

    public async Task<bool> SendCommandAsync(string command)
    {
        if (string.IsNullOrEmpty(command))
        {
            _logger.LogWarning("Cannot send null or empty command");
            return false;
        }

        if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
        {
            _logger.LogWarning("Cannot send command: Device not connected");
            return false;
        }

        try
        {
            await Task.Run(() => _serialPort.WriteLine(command));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending command to device");

            // Mark as disconnected on error
            _isConnected = false;
            _currentPort = string.Empty;
            ConnectionStatusChanged?.Invoke(this, false);

            return false;
        }
    }

    public async Task<string> AutoDetectDeviceAsync()
    {
        string[] ports = SerialPort.GetPortNames();

        foreach (string port in ports)
        {
            SerialPort testPort = null;
            try
            {
                testPort = new SerialPort(port, 115200)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                testPort.Open();
                testPort.WriteLine("CMD:GET_DISPLAY_TYPE");

                await Task.Delay(500); // Wait for response

                string response = testPort.ReadExisting();
                if (response.Contains("DISPLAY_TYPE:1602A") ||
                    response.Contains("DISPLAY_TYPE:OLED") ||
                    response.Contains("DISPLAY_TYPE:TFT"))
                {
                    // Make sure to close and dispose the port before returning
                    testPort.Close();
                    testPort.Dispose();
                    return port;
                }

                testPort.Close();
                testPort.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, $"Failed to check port {port}");

                // Clean up the port if an exception occurs
                if (testPort != null)
                {
                    try
                    {
                        if (testPort.IsOpen)
                            testPort.Close();
                        testPort.Dispose();
                    }
                    catch (Exception disposalEx)
                    {
                        _logger.LogDebug(disposalEx, $"Error disposing test port {port}");
                    }
                }
            }
        }

        return string.Empty;
    }

    public async Task<bool> CheckConnectionAsync()
    {
        if (_serialPort == null || !_serialPort.IsOpen)
        {
            if (_isConnected)
            {
                _isConnected = false;
                _currentPort = string.Empty;
                ConnectionStatusChanged?.Invoke(this, false);
            }

            // Try to reconnect
            return await ConnectToFirstAvailableAsync();
        }

        try
        {
            // Send a ping command to verify connection
            await Task.Run(() => _serialPort.WriteLine("CMD:PING"));
            await Task.Delay(100);

            string response = _serialPort.ReadExisting();
            bool connected = response.Contains("PONG");

            if (_isConnected != connected)
            {
                _isConnected = connected;
                if (!connected)
                {
                    _currentPort = string.Empty;
                }
                ConnectionStatusChanged?.Invoke(this, connected);
            }

            return connected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking connection");

            if (_isConnected)
            {
                _isConnected = false;
                _currentPort = string.Empty;
                ConnectionStatusChanged?.Invoke(this, false);
            }

            return false;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    try
                    {
                        _serialPort.Close();
                        _serialPort.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error disposing serial port");
                    }
                    _serialPort = null;
                }
            }

            _disposed = true;
        }
    }

    ~SerialPortService()
    {
        Dispose(false);
    }
}