using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using FuckingBilibili.Models;
using FuckingBilibili.Services;

namespace FuckingBilibili
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly GamePathService _gamePathService;
        private readonly ConfigService _configService;
        private readonly BackupService _backupService;
        private readonly GameLauncherService _launcherService;

        private string _gamePath = string.Empty;
        private ServerType _currentServer;
        private bool _isLoading;

        public string GamePath
        {
            get => _gamePath;
            set
            {
                _gamePath = value;
                OnPropertyChanged();
            }
        }

        public ServerType CurrentServer
        {
            get => _currentServer;
            set
            {
                _currentServer = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _gamePathService = new GamePathService();
            _configService = new ConfigService();
            _backupService = new BackupService();
            _launcherService = new GameLauncherService();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() => TxtStatus.Text = "正在扫描游戏目录...");

                var path = _gamePathService.AutoDetectGamePath();

                Dispatcher.Invoke(() =>
                {
                    if (path != null)
                    {
                        GamePath = path;
                        TxtStatus.Text = "✅ 已找到游戏目录";
                        TxtStatus.Foreground = FindResource("SuccessColor") as Brush;
                        RefreshCurrentServer();
                    }
                    else
                    {
                        TxtStatus.Text = "⚠️ 未找到游戏目录，请手动选择";
                        TxtStatus.Foreground = FindResource("WarningColor") as Brush;
                    }
                });
            });
        }

        private void RefreshCurrentServer()
        {
            try
            {
                if (string.IsNullOrEmpty(GamePath))
                    return;

                var config = _configService.ReadConfig(GamePath);
                if (config != null)
                {
                    CurrentServer = config.GetServerType();
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"❌ 读取配置失败: {ex.Message}";
                TxtStatus.Foreground = FindResource("ErrorColor") as Brush;
            }
        }


        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "选择游戏目录",
                FolderName = "Genshin Impact Game"
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.FolderName;

                string gamePath = Path.Combine(selectedPath, "Genshin Impact Game");
                if (Directory.Exists(gamePath))
                {
                    selectedPath = gamePath;
                }

                if (_gamePathService.ValidatePath(selectedPath))
                {
                    GamePath = selectedPath;
                    TxtStatus.Text = "✅ 目录有效";
                    TxtStatus.Foreground = FindResource("SuccessColor") as Brush;
                    RefreshCurrentServer();
                }
                else
                {
                    MessageBox.Show("选择的目录无效，请确保目录包含 config.ini 和 YuanShen.exe 文件", 
                        "无效目录", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnOfficial_Click(object sender, RoutedEventArgs e)
        {
            SwitchServer(ServerType.Official);
        }

        private void BtnBilibili_Click(object sender, RoutedEventArgs e)
        {
            SwitchServer(ServerType.Bilibili);
        }

        private void SwitchServer(ServerType targetServer)
        {
            if (string.IsNullOrEmpty(GamePath))
            {
                MessageBox.Show("请先选择游戏目录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CurrentServer == targetServer)
            {
                MessageBox.Show($"当前已经是{GetServerName(targetServer)}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                _backupService.CreateBackup(GamePath);
                _configService.WriteConfig(GamePath, targetServer);
                CurrentServer = targetServer;

                TxtStatus.Text = $"✅ 已切换到{GetServerName(targetServer)}";
                TxtStatus.Foreground = FindResource("SuccessColor") as Brush;

                MessageBox.Show($"已成功切换到{GetServerName(targetServer)}！", 
                    "切换成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"❌ 切换失败: {ex.Message}";
                TxtStatus.Foreground = FindResource("ErrorColor") as Brush;
                MessageBox.Show($"切换失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetServerName(ServerType server)
        {
            return server == ServerType.Official ? "官服" : "B服";
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GamePath))
            {
                MessageBox.Show("请先选择游戏目录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                if (_launcherService.IsGameRunning())
                {
                    var result = MessageBox.Show("检测到游戏已在运行，是否重新启动？", 
                        "游戏运行中", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                        return;
                }

                _launcherService.LaunchGame(GamePath);
                TxtStatus.Text = "🚀 游戏已启动";
                TxtStatus.Foreground = FindResource("SuccessColor") as Brush;
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"❌ 启动失败: {ex.Message}";
                TxtStatus.Foreground = FindResource("ErrorColor") as Brush;
                MessageBox.Show($"启动游戏失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GamePath))
            {
                MessageBox.Show("请先选择游戏目录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                string backupName = _backupService.CreateBackup(GamePath);
                TxtStatus.Text = $"✅ 备份已创建: {backupName}";
                TxtStatus.Foreground = FindResource("SuccessColor") as Brush;
                MessageBox.Show($"配置备份成功！\n文件名: {backupName}", 
                    "备份成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"❌ 备份失败: {ex.Message}";
                TxtStatus.Foreground = FindResource("ErrorColor") as Brush;
                MessageBox.Show($"备份失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GamePath))
            {
                MessageBox.Show("请先选择游戏目录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var backups = _backupService.GetBackupList(GamePath);
            if (backups.Count == 0)
            {
                MessageBox.Show("没有找到备份文件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }


            var restoreWindow = new Window
            {
                Title = "选择要恢复的备份",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow
            };

            var stackPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };
            var listBox = new System.Windows.Controls.ListBox 
            { 
                Height = 150,
                DisplayMemberPath = "DisplayName"
            };
            listBox.ItemsSource = backups;

            var btnRestore = new System.Windows.Controls.Button
            {
                Content = "恢复选中备份",
                Margin = new Thickness(0, 10, 0, 0),
                Height = 35
            };

            btnRestore.Click += (s, ev) =>
            {
                if (listBox.SelectedItem is BackupInfo selectedBackup)
                {
                    try
                    {
                        _backupService.RestoreBackup(GamePath, selectedBackup.FileName);
                        TxtStatus.Text = $"✅ 已恢复到: {selectedBackup.FileName}";
                        TxtStatus.Foreground = FindResource("SuccessColor") as Brush;
                        RefreshCurrentServer();
                        MessageBox.Show("配置恢复成功！", "恢复成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        restoreWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"恢复失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };

            stackPanel.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "选择要恢复的备份文件:", 
                Margin = new Thickness(0, 0, 0, 10) 
            });
            stackPanel.Children.Add(listBox);
            stackPanel.Children.Add(btnRestore);

            restoreWindow.Content = stackPanel;
            restoreWindow.ShowDialog();
        }
    }
}
