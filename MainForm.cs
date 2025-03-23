using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

namespace XLineTracker
{
    public partial class MainForm : Form
    {
        private System.Threading.Timer _fileWatcherTimer;
        private string _selectedFilePath;
        private Dictionary<string, int> _channelCounts;
        private DateTime _currentDate;
        private Panel graphPanel;
        private string _validLogsPath;
        private HashSet<string> _reportedFileErrors = new HashSet<string>();
        private DateTime? _countingStartTime = null;
        private Button _startCountingButton;
        private Label _countingStatusLabel;
        private Label _rateLabel;
        private Label _totalXLabel;
        private HashSet<string> _playersWhoResponded = new HashSet<string>();
        private RichTextBox _playerTextBox;
    
        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            SetupFileWatcher();
        }
        
        // This method has already been defined earlier in the class
        // Keeping the first implementation
    
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.Text = "X-Line Tracker";
            this.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this.Text = "X-Line Tracker";
            this.Size = new Size(450, 300); // Much smaller default size
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            this.Padding = new Padding(8); // Reduced padding
        
            // Title panel - reduced height
            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40, // Reduced from 60
                BackColor = Color.FromArgb(24, 24, 24)
            };
            
            var titleLabel = new Label
            {
                Text = "X-Line Tracker",
                Font = new Font("Segoe UI Semibold", 12F), // Smaller font
                ForeColor = Color.FromArgb(0, 150, 215),
                AutoSize = true,
                Location = new Point(10, 10) // Adjusted position
            };
            titlePanel.Controls.Add(titleLabel);
            
            // Control panel - more compact
            var controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60, // Reduced from 80
                BackColor = Color.FromArgb(40, 40, 40),
                Padding = new Padding(8, 6, 8, 6) // Reduced padding
            };
            
            // More compact layout with smaller controls
            var channelLabel = new Label
            {
                Text = "Channel:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            
            var channelComboBox = new ComboBox
            {
                Location = new Point(65, 8),
                Width = 120, // Reduced width
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            
            var refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(195, 8),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(70, 24), // Smaller button
                Cursor = Cursors.Hand
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            
            var statusLabel = new Label
            {
                Text = "Ready",
                ForeColor = Color.Silver,
                AutoSize = true,
                Location = new Point(270, 10)
            };
            
            // Start/Stop Counting button - first row
            _startCountingButton = new Button
            {
                Text = "Start",
                Location = new Point(10, 35),
                BackColor = Color.FromArgb(76, 175, 80), // Green
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(60, 24), // Smaller button
                Cursor = Cursors.Hand
            };
            _startCountingButton.FlatAppearance.BorderSize = 0;
            
            // Reset button - first row
            var resetButton = new Button
            {
                Text = "Reset",
                Location = new Point(75, 35),
                BackColor = Color.FromArgb(100, 100, 100), // Gray 
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(60, 24), // Smaller button
                Cursor = Cursors.Hand
            };
            resetButton.FlatAppearance.BorderSize = 0;
            
            // Status label - second row
            _countingStatusLabel = new Label
            {
                Text = "Not counting",
                ForeColor = Color.Silver,
                AutoSize = true,
                Location = new Point(145, 39)
            };
            
            // Create a RichTextBox for players instead of a ListBox - more reliable for displaying text
            var playerTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Font = new Font("Segoe UI", 9F),
                Visible = true,
                DetectUrls = false,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Assign it to a class field so we can use it elsewhere
            _playerTextBox = playerTextBox;

            // Create a container panel for the player list with title
            var listPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 24, 24),
                Padding = new Padding(8),
                Visible = false // Hidden by default
            };

            var listTitleLabel = new Label
            {
                Text = "Players Who Responded",
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Color.FromArgb(0, 150, 215),
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Add controls in correct order
            listPanel.Controls.Add(playerTextBox);
            listPanel.Controls.Add(listTitleLabel);

            // Add the panel to the form
            this.Controls.Add(listPanel);
            this.Controls.SetChildIndex(listPanel, 0); // Make sure it's at the bottom of the z-order

            // Show Players button
            var showPlayersButton = new Button
            {
                Text = "Show Players",
                Location = new Point(270, 35),
                BackColor = Color.FromArgb(0, 150, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(90, 24),
                Cursor = Cursors.Hand
            };
            showPlayersButton.FlatAppearance.BorderSize = 0;

            // Stats panel - more compact
            var statsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30, // Reduced from 60
                BackColor = Color.FromArgb(40, 40, 40),
                Padding = new Padding(10, 5, 10, 5) // Reduced padding
            };
            
            _totalXLabel = new Label
            {
                Text = "Total Players: 0",
                Font = new Font("Segoe UI Semibold", 10F), // Smaller font
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 8)
            };
            
            statsPanel.Controls.Add(_totalXLabel);
            
            // Add controls to panels
            controlPanel.Controls.AddRange(new Control[] { 
                channelLabel, channelComboBox, refreshButton, statusLabel,
                _startCountingButton, resetButton, _countingStatusLabel, showPlayersButton
            });
            
            // Add everything to the form in the correct order
            this.Controls.AddRange(new Control[] { 
                listPanel, statsPanel, controlPanel, titlePanel 
            });
            
            // Set up event handlers for the Show Players button
            showPlayersButton.Click += (s, e) =>
            {
                if (!listPanel.Visible)
                {
                    // Show the player list
                    listPanel.Visible = true;
                    showPlayersButton.Text = "Hide Players";
                    this.Size = new Size(450, 500);
                    
                    // Force full redraw
                    listPanel.Refresh();
                    _playerTextBox.Refresh();
                }
                else
                {
                    // Hide the player list
                    listPanel.Visible = false;
                    showPlayersButton.Text = "Show Players";
                    this.Size = new Size(450, 300);
                }
            };
            
            // Set up event handlers for the Start Counting button
            _startCountingButton.Click += (s, e) => 
            {
                if (_countingStartTime.HasValue)
                {
                    // If already counting, stop counting
                    _countingStartTime = null;
                    _startCountingButton.Text = "Start";
                    _startCountingButton.BackColor = Color.FromArgb(76, 175, 80); // Green
                    _countingStatusLabel.Text = "Not counting";
                    _countingStatusLabel.ForeColor = Color.Silver;
                }
                else
                {
                    // Start counting
                    _countingStartTime = DateTime.Now;
                    _startCountingButton.Text = "Stop";
                    _startCountingButton.BackColor = Color.FromArgb(211, 47, 47); // Red
                    _countingStatusLabel.Text = $"Counting: {_countingStartTime:HH:mm:ss}";
                    _countingStatusLabel.ForeColor = Color.FromArgb(76, 175, 80); // Green
                    
                    // Reset the counters
                    _playersWhoResponded.Clear();
                    _playerTextBox.Clear(); // Clear the text box
                    _totalXLabel.Text = "Total Players: 0";
                    
                    UpdateGraph(); // Keep function name for compatibility
                }
            };
            
            // Set up event handlers for the Reset button
            resetButton.Click += (s, e) => 
            {
                _countingStartTime = null;
                _startCountingButton.Text = "Start";
                _startCountingButton.BackColor = Color.FromArgb(76, 175, 80); // Green
                _countingStatusLabel.Text = "Not counting";
                _countingStatusLabel.ForeColor = Color.Silver;
                
                // Reset the counters
                _playersWhoResponded.Clear();
                _playerTextBox.Clear(); // Clear the text box
                _totalXLabel.Text = "Total Players: 0";
            };
            
            // Set up remaining event handlers
            channelComboBox.SelectedIndexChanged += (s, e) => 
            {
                statusLabel.Text = "Loading...";
                statusLabel.ForeColor = Color.FromArgb(255, 200, 70);
                
                _selectedFilePath = GetLatestFileForChannel(channelComboBox.SelectedItem.ToString());
                UpdateGraph(); // This will now update player list if counting
                
                statusLabel.Text = "Ready";
                statusLabel.ForeColor = Color.FromArgb(100, 220, 100);
            };
            
            refreshButton.Click += (s, e) => 
            {
                if (channelComboBox.SelectedItem != null)
                {
                    statusLabel.Text = "Refreshing...";
                    statusLabel.ForeColor = Color.FromArgb(255, 200, 70);
                    
                    _selectedFilePath = GetLatestFileForChannel(channelComboBox.SelectedItem.ToString());
                    UpdateGraph(); // This will now update player list if counting
                    
                    statusLabel.Text = "Ready";
                    statusLabel.ForeColor = Color.FromArgb(100, 220, 100);
                }
            };
            
            // Load channels
            try
            {
                var channels = GetAvailableChannels();
                channelComboBox.Items.AddRange(channels);
                if (channels.Length > 0)
                    channelComboBox.SelectedIndex = 0;
            }
            catch (Exception)
            {
                statusLabel.Text = "Error";
                statusLabel.ForeColor = Color.FromArgb(220, 80, 80);
            }
        }

        private void DrawGraph(Graphics g)
        {
            if (_channelCounts == null || !_channelCounts.Any())
                return;
        
            int width = graphPanel.Width - 60;
            int height = graphPanel.Height - 60;
            int padding = 30;
            int maxCount = _channelCounts.Values.Max();
        
            // Draw background and border
            g.FillRectangle(new SolidBrush(Color.FromArgb(30, 30, 30)), padding, padding, width, height);
            g.DrawRectangle(new Pen(Color.FromArgb(60, 60, 60)), padding, padding, width, height);
        
            // Draw grid lines and labels
            using (var gridPen = new Pen(Color.FromArgb(50, 50, 50)))
            using (var labelBrush = new SolidBrush(Color.Silver))
            using (var font = new Font("Segoe UI", 8F))
            {
                // Horizontal grid lines
                for (int i = 0; i <= 5; i++)
                {
                    int y = padding + height - (height / 5 * i);
                    g.DrawLine(gridPen, padding, y, padding + width, y);
                    
                    // Y-axis labels
                    int value = (maxCount / 5) * i;
                    g.DrawString(value.ToString(), font, labelBrush, padding - 25, y - 7);
                }
                
                // Vertical grid lines
                int segments = Math.Min(10, _channelCounts.Count);
                for (int i = 0; i <= segments; i++)
                {
                    int x = padding + (width / segments * i);
                    g.DrawLine(gridPen, x, padding, x, padding + height);
                    
                    // X-axis labels (line numbers)
                    if (i < segments)
                    {
                        int index = (int)(_channelCounts.Count * (i / (double)segments));
                        string key = _channelCounts.Keys.ElementAt(Math.Min(index, _channelCounts.Count - 1));
                        g.DrawString(key, font, labelBrush, x - 10, padding + height + 5);
                    }
                }
            }
        
            // Draw line graph with gradient pen and points
            if (_channelCounts.Count > 1)
            {
                // Create gradient brush for line
                using (var gradientBrush = new LinearGradientBrush(
                    new Point(padding, padding), 
                    new Point(padding, padding + height), 
                    Color.FromArgb(0, 180, 255), 
                    Color.FromArgb(0, 120, 215)))
                using (var pen = new Pen(gradientBrush, 3))
                using (var pointBrush = new SolidBrush(Color.FromArgb(0, 180, 255)))
                {
                    Point? lastPoint = null;
                    int pointIndex = 0;
                    
                    foreach (var kvp in _channelCounts)
                    {
                        int x = padding + (int)((pointIndex / (double)(_channelCounts.Count - 1)) * width);
                        int y = padding + height - (int)((kvp.Value / (double)maxCount) * height);
                        Point currentPoint = new Point(x, y);
        
                        // Draw connection line
                        if (lastPoint.HasValue)
                            g.DrawLine(pen, lastPoint.Value, currentPoint);
                        
                        // Draw point
                        g.FillEllipse(pointBrush, x - 4, y - 4, 8, 8);
                        g.DrawEllipse(Pens.White, x - 4, y - 4, 8, 8);
                        
                        lastPoint = currentPoint;
                        pointIndex++;
                    }
                }
            }
        
            // Draw title and labels
            using (var titleBrush = new SolidBrush(Color.White))
            using (var titleFont = new Font("Segoe UI Semibold", 11F))
            {
                g.DrawString("X Count Over Time", titleFont, titleBrush, padding, 10);
            }
        }
        
        // Update the UpdateGraph method to properly track and display players
        private void UpdateGraph()
        {
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
                return;
        
            // Only process if we're actually counting
            if (!_countingStartTime.HasValue)
                return;
        
            HashSet<string> newPlayers = new HashSet<string>();
        
            try
            {
                using (FileStream fs = new FileStream(_selectedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fs, true)) // Auto-detect encoding
                {
                    string line;
                    
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Skip empty lines
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        
                        // Trim any invisible characters that might be at the start
                        line = line.Trim('\uFEFF', '\u200B', '\t', ' ');
                        
                        // Enhanced pattern matching for timestamp format: [ YYYY.MM.DD HH:MM:SS ]
                        int openBracketPos = line.IndexOf('[');
                        int closeBracketPos = line.IndexOf(']', openBracketPos + 1);
                        
                        if (openBracketPos >= 0 && closeBracketPos > openBracketPos && closeBracketPos - openBracketPos >= 20)
                        {
                            // Extract timestamp string
                            string timestampStr = line.Substring(openBracketPos + 1, closeBracketPos - openBracketPos - 1).Trim();
                            
                            // Try to parse the timestamp
                            if (DateTime.TryParseExact(timestampStr, "yyyy.MM.dd HH:mm:ss", 
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime timestamp))
                            {
                                // Convert to UTC if needed
                                DateTime utcTimestamp = timestamp.Kind == DateTimeKind.Local ? 
                                    timestamp.ToUniversalTime() : timestamp;
                                
                                // Only process messages after the counting start time
                                DateTime utcCountingStart = _countingStartTime.Value.ToUniversalTime();
                                if (utcTimestamp < utcCountingStart)
                                    continue;
                                
                                // Look for player name and message
                                int greaterThanPos = line.IndexOf('>', closeBracketPos);
                                
                                if (greaterThanPos > closeBracketPos)
                                {
                                    // Extract player name - between ] and >
                                    string playerName = line.Substring(closeBracketPos + 1, greaterThanPos - closeBracketPos - 1).Trim();
                                    
                                    // Get the chat message part (after the ">" character)
                                    string messageContent = line.Substring(greaterThanPos + 1).Trim();
                                    
                                    // Check if the message is just "x" (case insensitive) and player hasn't responded yet
                                    if (messageContent.Equals("x", StringComparison.OrdinalIgnoreCase) && 
                                        !_playersWhoResponded.Contains(playerName))
                                    {
                                        _playersWhoResponded.Add(playerName);
                                        newPlayers.Add(playerName);
                                    }
                                }
                            }
                        }
                    }
                }
        
                // Update the UI with any new players
                if (newPlayers.Count > 0)
                {
                    this.Invoke((MethodInvoker)delegate {
                        foreach (string player in newPlayers)
                        {
                            _playerTextBox.AppendText($"{_playersWhoResponded.Count}. {player}\n");
                        }
                        
                        _totalXLabel.Text = $"Total Players: {_playersWhoResponded.Count}";
                        
                        // Make sure the text is visible
                        _playerTextBox.SelectionStart = _playerTextBox.Text.Length;
                        _playerTextBox.ScrollToCaret();
                    });
                }
            }
            catch (IOException ex)
            {
                // Log the error but don't show dialog repeatedly
                Console.WriteLine($"Error accessing file: {ex.Message}");
                
                // Only show message box once per file
                if (!_reportedFileErrors.Contains(_selectedFilePath))
                {
                    _reportedFileErrors.Add(_selectedFilePath);
                    this.Invoke((MethodInvoker)delegate {
                        MessageBox.Show($"Cannot access file: {Path.GetFileName(_selectedFilePath)}\n\n" +
                                        $"Error: {ex.Message}\n\n" +
                                        "The application will continue to try reading the file.", 
                                        "File Access Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating player list: {ex.Message}");
            }
        }

        private void SetupFileWatcher()
        {
            _fileWatcherTimer = new System.Threading.Timer(state => 
            {
                if (!string.IsNullOrEmpty(_selectedFilePath) && File.Exists(_selectedFilePath))
                {
                    UpdateGraph();
                }
            }, null, 0, 1000); // Check every second
        }

        private string[] GetAvailableChannels()
        {
            // Try multiple possible paths for log files
            List<string> possiblePaths = new List<string>();
            
            // Standard Documents path
            string standardDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            possiblePaths.Add(Path.Combine(standardDocs, "Frontier", "logs"));
            possiblePaths.Add(Path.Combine(standardDocs, "Frontier", "logs", "Chatlogs"));
            
            // OneDrive Documents path (common pattern)
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string oneDriveDocs = Path.Combine(userFolder, "OneDrive", "Documents");
            possiblePaths.Add(Path.Combine(oneDriveDocs, "Frontier", "logs"));
            possiblePaths.Add(Path.Combine(oneDriveDocs, "Frontier", "logs", "Chatlogs"));
            
            // Try dates - both UTC and local time to cover all possibilities
            List<DateTime> datesToTry = new List<DateTime>
            {
                DateTime.UtcNow.Date,
                DateTime.Now.Date
            };
            
            // Also try yesterday's date in case of timezone differences
            datesToTry.Add(DateTime.UtcNow.Date.AddDays(-1));
            datesToTry.Add(DateTime.Now.Date.AddDays(-1));
            
            // Try each path and date combination
            foreach (string logsFolder in possiblePaths)
            {
                if (!Directory.Exists(logsFolder))
                    continue;
                    
                foreach (DateTime dateToTry in datesToTry)
                {
                    string datePattern = $"*_{dateToTry:yyyyMMdd}_*.txt";
                    
                    try
                    {
                        var files = Directory.GetFiles(logsFolder, datePattern);
                        if (files.Length > 0)
                        {
                            // Store the valid path and date for future use
                            _validLogsPath = logsFolder;
                            _currentDate = dateToTry;
                            
                            // Debug information
                            Console.WriteLine($"Found files using path: {logsFolder}");
                            Console.WriteLine($"Using date pattern: {datePattern}");
                            Console.WriteLine($"Found {files.Length} files");
                            
                            return files
                                .Select(f => Path.GetFileName(f))
                                .Select(f => f.Split('_')[0])
                                .Distinct()
                                .OrderBy(c => c)
                                .ToArray();
                        }
                    }
                    catch
                    {
                        // Just try the next combination
                        continue;
                    }
                }
            }
            
            // If we get here, try a broader search without date patterns
            foreach (string logsFolder in possiblePaths)
            {
                if (!Directory.Exists(logsFolder))
                    continue;
                    
                try
                {
                    // Look for any txt files that match the pattern with any date
                    var files = Directory.GetFiles(logsFolder, "*.txt")
                                        .Where(f => Path.GetFileName(f).Contains("_20") && 
                                                    Path.GetFileName(f).Split('_').Length >= 3)
                                        .ToArray();
                                        
                    if (files.Length > 0)
                    {
                        _validLogsPath = logsFolder;
                        
                        // Extract the date from the first file
                        string filename = Path.GetFileName(files[0]);
                        string[] parts = filename.Split('_');
                        if (parts.Length >= 2 && parts[1].Length == 8)
                        {
                            // Try to parse the date from the filename
                            if (DateTime.TryParseExact(parts[1], "yyyyMMdd", 
                                System.Globalization.CultureInfo.InvariantCulture, 
                                System.Globalization.DateTimeStyles.None, out DateTime fileDate))
                            {
                                _currentDate = fileDate;
                            }
                        }
                        
                        MessageBox.Show($"Found log files with different date pattern.\nUsing folder: {logsFolder}", 
                            "Files Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                        return files
                            .Select(f => Path.GetFileName(f))
                            .Select(f => f.Split('_')[0])
                            .Distinct()
                            .OrderBy(c => c)
                            .ToArray();
                    }
                }
                catch
                {
                    continue;
                }
            }
            
            // If we get here, we couldn't find any valid paths with files
            MessageBox.Show("No log files found. Please ensure your chat logs are in one of these locations:\n\n" +
                            string.Join("\n", possiblePaths) + 
                            "\n\nFile pattern should be: Channel_YYYYMMDD_*.txt", 
                            "No Files Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
            return new string[0];
        }

        private string GetLatestFileForChannel(string channel)
        {
            if (string.IsNullOrEmpty(_validLogsPath))
            {
                // If we don't have a valid path yet, attempt to find one
                GetAvailableChannels();
                
                if (string.IsNullOrEmpty(_validLogsPath))
                {
                    MessageBox.Show("Could not locate log files. Please check your Frontier logs folder.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            
            // Updated pattern to match your actual file format
            string pattern = $"{channel}_{_currentDate:yyyyMMdd}_*.txt";
            
            try
            {
                var files = Directory.GetFiles(_validLogsPath, pattern);
                
                if (files.Length == 0)
                {
                    // Try a more flexible search if specific pattern doesn't work
                    files = Directory.GetFiles(_validLogsPath, $"{channel}_*.txt")
                                   .Where(f => Path.GetFileName(f).Split('_').Length >= 3)
                                   .ToArray();
                }
                
                string latestFile = files.OrderByDescending(f => File.GetLastWriteTime(f))
                                        .FirstOrDefault();
                                        
                if (latestFile != null)
                {
                    // Test if file can be accessed before returning it
                    try
                    {
                        using (FileStream fs = new FileStream(latestFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            // Just testing access, no need to read anything
                        }
                        
                        // File is accessible
                        return latestFile;
                    }
                    catch (IOException)
                    {
                        // File is locked, but we'll still return it and let UpdateGraph handle the issue
                        return latestFile;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting latest file: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _fileWatcherTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}