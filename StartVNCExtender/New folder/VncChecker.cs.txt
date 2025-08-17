using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StartVNCExtender
{
    public class VncChecker
    {
        /// <summary>
        /// Polls a host and reports the connection status via a callback action.
        /// </summary>
        /// <param name="hostName">The host name or IP address of the VNC server.</param>
        /// <param name="port">The port number of the VNC server.</param>
        /// <param name="pollIntervalMilliseconds">The delay in milliseconds between each polling attempt.</param>
        /// <param name="statusCallback">An action to be invoked with the connection status.</param>
        /// <returns>A Task that completes when a connection is successful.</returns>
        public static async Task PollForVncServerAsync(string hostName, int port, int pollIntervalMilliseconds, Action<bool> statusCallback)
        {
            while (true)
            {
                bool isConnected = false;
                try
                {
                    using (var tcpClient = new TcpClient())
                    {
                        // Attempt to connect with a short timeout to prevent blocking
                        var connectTask = tcpClient.ConnectAsync(hostName, port);
                        var individualTimeoutTask = Task.Delay(pollIntervalMilliseconds);

                        var completedTask = await Task.WhenAny(connectTask, individualTimeoutTask);

                        if (completedTask == connectTask)
                        {
                            await connectTask;
                            isConnected = true;
                        }
                    }
                }
                catch (SocketException)
                {
                    // Expected, isConnected remains false
                }
                catch (Exception)
                {
                    // Other exceptions, isConnected remains false
                }

                // Report the status to the caller
                statusCallback(isConnected);

                if (isConnected)
                {
                    return; // Exit the method if a connection was made
                }

                // Await a delay before the next polling attempt
                await Task.Delay(pollIntervalMilliseconds);
            }
        }

        /// <summary>
        /// Polls a host indefinitely until a connection to the specified port is successful.
        /// </summary>
        /// <param name="hostName">The host name or IP address of the VNC server.</param>
        /// <param name="port">The port number of the VNC server.</param>
        /// <param name="pollIntervalMilliseconds">The delay in milliseconds between each polling attempt.</param>
        public static async Task WaitForVncServerForeverAsync(string hostName, int port, int pollIntervalMilliseconds)
        {
            while (true)
            {
                try
                {
                    using (var tcpClient = new TcpClient())
                    {
                        // Attempt to connect with a short timeout to prevent blocking
                        var connectTask = tcpClient.ConnectAsync(hostName, port);
                        var individualTimeoutTask = Task.Delay(pollIntervalMilliseconds);

                        var completedTask = await Task.WhenAny(connectTask, individualTimeoutTask);

                        if (completedTask == connectTask)
                        {
                            // The connection was successful, so we await it to re-throw any exceptions
                            // and then break the loop.
                            await connectTask;
                            return; // Connection successful, exit the method
                        }
                    }
                }
                catch (SocketException)
                {
                    // The connection failed, which is expected.
                    // The loop will continue to the next iteration.
                }
                catch (Exception)
                {
                    // Catch any other exceptions and continue polling.
                }

                // Await a delay before the next polling attempt
                await Task.Delay(pollIntervalMilliseconds);
            }
        }

        public static async Task<bool> WaitForVncServerAsync(string hostName, int port, int totalTimeoutMilliseconds, int pollIntervalMilliseconds)
        {
            var cts = new CancellationTokenSource();
            var timeoutTask = Task.Delay(totalTimeoutMilliseconds, cts.Token);

            while (!timeoutTask.IsCompleted)
            {
                try
                {
                    using (var tcpClient = new TcpClient())
                    {
                        // Attempt to connect with a small, individual timeout
                        var connectTask = tcpClient.ConnectAsync(hostName, port);
                        var individualTimeoutTask = Task.Delay(pollIntervalMilliseconds);

                        var completedTask = await Task.WhenAny(connectTask, individualTimeoutTask);

                        if (completedTask == connectTask)
                        {
                            // If the connection task completed first, it's a success
                            cts.Cancel(); // Stop the overall timeout
                            await connectTask; // Re-await to throw any exceptions
                            return true;
                        }
                    }
                }
                catch (SocketException)
                {
                    // The connection failed, continue to the next poll attempt
                }
                catch (Exception)
                {
                    // General error, also continue
                }

                // Await a delay before the next polling attempt
                await Task.Delay(pollIntervalMilliseconds, cts.Token);
            }

            // If the loop finished due to the timeout, the server was not found
            return false;
        }

        public static async Task<bool> IsVncServerActiveAsync(string hostName, int port)
        {
            try
            {
                // Resolve the host name to an IP address
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(hostName);

                if (addresses.Length == 0)
                {
                    return false;
                }

                // Using the first IP address found
                IPAddress ipAddress = addresses[0];

                using (var tcpClient = new TcpClient())
                {
                    // Set a timeout for the connection attempt
                    var timeoutTask = Task.Delay(5000); // 5 seconds

                    // Attempt to connect to the VNC server port asynchronously
                    var connectTask = tcpClient.ConnectAsync(ipAddress, port);

                    // Wait for either the connection or the timeout to complete
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                    // If the connect task didn't complete, it means the timeout happened
                    if (completedTask != connectTask)
                    {
                        return false;
                    }

                    // If the connection task completed, await it to throw any exceptions
                    await connectTask;

                    return true;
                }
            }
            catch (SocketException)
            {
                // A SocketException usually means the connection failed
                return false;
            }
            catch (Exception)
            {
                // Catch any other exceptions
                return false;
            }
        }

        public static bool IsVncServerActive(string hostName, int port)
        {
            try
            {
                // Resolve the host name to an IP address
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                if (addresses.Length == 0)
                {
                    return false;
                }

                IPAddress ipAddress = addresses[0];
                IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

                // Use a Socket object to gain more control
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    // Set the timeout in milliseconds for the connection attempt
                    int timeoutMilliseconds = 0; // 5 seconds
                    socket.SendTimeout = timeoutMilliseconds;
                    socket.ReceiveTimeout = timeoutMilliseconds;

                    // Attempt to connect. This call will throw a SocketException on failure or timeout.
                    socket.Connect(endPoint);

                    // If we get here, the connection was successful
                    return true;
                }
            }
            catch (SocketException)
            {
                // A SocketException (including a timeout) means the connection failed
                return false;
            }
            catch (Exception)
            {
                // Catch any other exceptions
                return false;
            }
        }
    }
}
