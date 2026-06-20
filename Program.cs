    internal class Program
    {
        private static HttpClient? _httpClient;
        private static int _currentMcNumber;
        private static bool _shouldStop = false;
        private static readonly ExcelWriter _excelWriter = new();
        private static readonly USDOTExtractor _extractor = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine("FMCSA MC Number Checker");
            Console.WriteLine("=======================");

            // Initialize HTTP client with proxy/VPN support
            _httpClient = await HttpClientFactory.CreateHttpClientAsync();

            Console.Write("Enter starting MC number (just the numbers, e.g., 1339151): ");
            if (!int.TryParse(Console.ReadLine(), out int startMcNumber))
            {
                Console.WriteLine("Invalid MC number. Exiting.");
                return;
            }

            Console.WriteLine($"Starting check from MC-{startMcNumber}...");
            Console.WriteLine("Type 'ruk ja bhai' (without quotes) and press Enter to stop.\n");

            // Start the stop listener in a separate task
            _ = Task.Run(ListenForStopCommand);

            // Start checking MC numbers
            _currentMcNumber = startMcNumber;
            while (!_shouldStop)
            {
                await CheckCurrentMcNumber();
                _currentMcNumber++;

                // Small delay to avoid hitting the server too hard
                await Task.Delay(1000);
            }

            Console.WriteLine("\nStopped by user. Press any key to exit.");
            Console.ReadKey();
        }

        private static void ListenForStopCommand()
        {
            while (true)
            {
                string? input = Console.ReadLine();
                if (input?.Equals("ruk ja bhai", StringComparison.OrdinalIgnoreCase) == true)
                {
                    _shouldStop = true;
                    break;
                }
            }
        }

        private static async Task CheckCurrentMcNumber()
        {
            try
            {
                var url = $" i scraped and built it after doing extensive api inspection and pen testing";

                // Create a new request message with proper headers
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                // custom headers which i also found out after pen testing

                using var response = await _httpClient!.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        Console.WriteLine($"Error checking MC-{_currentMcNumber}: Access Forbidden (403). The website might be blocking automated requests.");

                        // If we get a 403, wait longer before next attempt
                        await Task.Delay(5000);
                    }
                    else
                    {
                        Console.WriteLine($"Error checking MC-{_currentMcNumber}: Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).");
                    }
                    return;
                }

                var html = await response.Content.ReadAsStringAsync();
                var info = _extractor.ExtractFields(html);

                // Check if it's a valid carrier with phone
                if (IsValidCarrier(info))
                {
                    Console.WriteLine($"✓ MC-{_currentMcNumber} - {info.LegalName} - {info.Phone}");
                    _excelWriter.WriteToExcel(info.ToDictionary());
                }
                else
                {
                    // Optionally show why it was rejected
                    if (info.EntityType != "CARRIER")
                        Console.WriteLine($"- MC-{_currentMcNumber} - Not a carrier (Type: {info.EntityType})");
                    else if (info.USDOTStatus != "ACTIVE")
                        Console.WriteLine($"- MC-{_currentMcNumber} - Not active (Status: {info.USDOTStatus})");
                    else if (string.IsNullOrWhiteSpace(info.Phone) || info.Phone.Equals("N/A"))
                        Console.WriteLine($"- MC-{_currentMcNumber} - No phone number");
                    else
                        Console.WriteLine($"- MC-{_currentMcNumber} - Invalid carrier");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking MC-{_currentMcNumber}: {ex.Message}");
            }
        }

        private static bool IsValidCarrier(USDOTInfo info)
        {
            // Check if entity is a carrier, status is active, and phone exists
            return info.EntityType.Equals("CARRIER", StringComparison.OrdinalIgnoreCase) &&
                   info.USDOTStatus.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(info.Phone) &&
                   !info.Phone.Equals("N/A", StringComparison.OrdinalIgnoreCase);
        }
    }
