using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Text;

namespace EveningStudyManager
{
    public partial class MainForm : Form
    {
        private Dictionary<string, string> homeworkData = new Dictionary<string, string>();
        private Image customImage;
        private bool darkMode = false;
        private readonly Color lightBg = Color.White;
        private readonly Color darkBg = Color.FromArgb(30, 30, 30);
        private readonly Color lightCard = Color.FromArgb(249, 249, 249);
        private readonly Color darkCard = Color.FromArgb(45, 45, 45);

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            LoadData();
            SetupEventHandlers();
            UpdateTime();
        }

        private void InitializeUI()
        {
            // 主窗体设置
            Text = "晚自习管理系统";
            Size = new Size(1200, 800);
            BackColor = lightBg;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // 顶部导航栏
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(52, 152, 219)
            };

            // 当前时间标签
            var timeLabel = new Label
            {
                Text = "当前时间:",
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10),
                Location = new Point(20, 20),
                AutoSize = true
            };

            currentTimeLabel = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
                Location = new Point(timeLabel.Right + 5, 20),
                AutoSize = true
            };

            // 剩余时间标签
            var countdownLabel = new Label
            {
                Text = "剩余时间:",
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10),
                Location = new Point(currentTimeLabel.Right + 20, 20),
                AutoSize = true
            };

            countdownTimerLabel = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
                Location = new Point(countdownLabel.Right + 5, 20),
                AutoSize = true
            };

            // 导航按钮
            studentBtn = new Button
            {
                Text = "学生端",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 255, 255, 80),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10),
                Size = new Size(100, 35),
                Location = new Point(Width - 300, 15)
            };

            teacherBtn = new Button
            {
                Text = "布置作业",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 255, 255, 80),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10),
                Size = new Size(100, 35),
                Location = new Point(studentBtn.Right + 10, 15)
            };

            // 深色模式切换按钮
            themeToggleBtn = new Button
            {
                Text = "🌙",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14),
                Size = new Size(40, 35),
                Location = new Point(teacherBtn.Right + 10, 15)
            };

            headerPanel.Controls.AddRange(new Control[] {
                timeLabel, currentTimeLabel, countdownLabel, 
                countdownTimerLabel, studentBtn, teacherBtn, themeToggleBtn
            });

            // 主容器
            mainContainer = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.FlatButtons,
                ItemSize = new Size(0, 1),
                SizeMode = TabSizeMode.Fixed
            };

            // 学生端页面
            studentPage = new TabPage();
            SetupStudentPage();

            // 教师端页面
            teacherPage = new TabPage();
            SetupTeacherPage();

            mainContainer.TabPages.Add(studentPage);
            mainContainer.TabPages.Add(teacherPage);
            mainContainer.SelectedIndex = 0;

            // 页脚
            footerLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Text = "© 2025 自习管理系统 ZFTONY制 | 版本 Beta0.3.7",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei", 9),
                ForeColor = Color.Gray
            };

            Controls.AddRange(new Control[] { headerPanel, mainContainer, footerLabel });
        }

        private void SetupStudentPage()
        {
            studentPage.BackColor = lightBg;
            studentPage.Padding = new Padding(20);

            // 主布局
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Absolute, 250),
                    new ColumnStyle(SizeType.Percent, 100)
                }
            };

            // 左侧规则区域
            var rulesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var rulesTitle = new Label
            {
                Text = "自习守则",
                Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219),
                AutoSize = true
            };

            var rulesList = new ListBox
            {
                Location = new Point(0, rulesTitle.Bottom + 10),
                Size = new Size(230, 200),
                BorderStyle = BorderStyle.None,
                Items = {
                    "保持安静，专注学习📕",
                    "有问题憋着下课问‍",
                    "合理规划自习时间⏰",
                    "今天不学习，明天变垃圾🚮",
                    "珍惜每分每秒🕙",
                    "物品轻拿轻放🐾🐈🐑🦘🦥🦛",
                    "作业做完了吗就讲话，闭嘴👊🔥"
                }
            };

            // 图片区域
            customImagePanel = new Panel
            {
                Location = new Point(0, rulesList.Bottom + 20),
                Size = new Size(230, 150),
                BackColor = Color.FromArgb(249, 249, 249),
                BorderStyle = BorderStyle.FixedSingle
            };

            noImageLabel = new Label
            {
                Text = "暂无图片提示",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Microsoft YaHei", 10)
            };

            customImagePanel.Controls.Add(noImageLabel);
            rulesPanel.Controls.AddRange(new Control[] { rulesTitle, rulesList, customImagePanel });

            // 右侧作业区域
            var homeworkPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var homeworkTitle = new Label
            {
                Text = "今日作业",
                Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219),
                AutoSize = true
            };

            homeworkFlowPanel = new FlowLayoutPanel
            {
                Location = new Point(0, homeworkTitle.Bottom + 15),
                Size = new Size(homeworkPanel.Width, homeworkPanel.Height - homeworkTitle.Height - 20),
                AutoScroll = true,
                WrapContents = true
            };

            homeworkPanel.Controls.Add(homeworkTitle);
            homeworkPanel.Controls.Add(homeworkFlowPanel);

            mainLayout.Controls.Add(rulesPanel, 0, 0);
            mainLayout.Controls.Add(homeworkPanel, 1, 0);

            studentPage.Controls.Add(mainLayout);
        }

        private void SetupTeacherPage()
        {
            teacherPage.BackColor = lightBg;
            teacherPage.Padding = new Padding(20);

            // 主布局
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                RowStyles = {
                    new RowStyle(SizeType.Percent, 40),
                    new RowStyle(SizeType.Absolute, 150),
                    new RowStyle(SizeType.Percent, 60)
                }
            };

            // 作业表单区域
            var formPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            var subjectLabel = new Label
            {
                Text = "选择科目",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold)
            };

            subjectComboBox = new ComboBox
            {
                Location = new Point(subjectLabel.Left, subjectLabel.Bottom + 10),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "语文", "数学", "英语", "物理", "化学", "生物" }
            };
            subjectComboBox.SelectedIndex = 0;

            var homeworkLabel = new Label
            {
                Text = "作业内容",
                Location = new Point(subjectLabel.Left, subjectComboBox.Bottom + 20),
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold)
            };

            homeworkTextBox = new TextBox
            {
                Multiline = true,
                Location = new Point(subjectLabel.Left, homeworkLabel.Bottom + 10),
                Size = new Size(350, 80)
            };

            // 按钮区域
            var buttonPanel = new Panel
            {
                Location = new Point(subjectComboBox.Right + 20, 20),
                Size = new Size(200, 120)
            };

            submitBtn = new Button
            {
                Text = "布置作业",
                Size = new Size(180, 35),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White
            };

            clearBtn = new Button
            {
                Text = "清空所有",
                Size = new Size(180, 35),
                Location = new Point(0, submitBtn.Bottom + 10),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White
            };

            // 图片上传区域
            var imagePanel = new Panel
            {
                Location = new Point(subjectLabel.Left, homeworkTextBox.Bottom + 20),
                Size = new Size(350, 80)
            };

            var imageLabel = new Label
            {
                Text = "上传提示图片",
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold)
            };

            uploadImageBtn = new Button
            {
                Text = "选择图片",
                Size = new Size(100, 30),
                Location = new Point(0, imageLabel.Bottom + 10),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White
            };

            clearImageBtn = new Button
            {
                Text = "清除图片",
                Size = new Size(100, 30),
                Location = new Point(uploadImageBtn.Right + 10, imageLabel.Bottom + 10),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White
            };

            // 导入导出按钮
            var importExportPanel = new Panel
            {
                Location = new Point(buttonPanel.Left, clearBtn.Bottom + 20),
                Size = new Size(200, 80)
            };

            importBtn = new Button
            {
                Text = "导入作业",
                Size = new Size(180, 35),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White
            };

            exportBtn = new Button
            {
                Text = "导出作业",
                Size = new Size(180, 35),
                Location = new Point(0, importBtn.Bottom + 10),
                BackColor = Color.FromArgb(243, 156, 18),
                ForeColor = Color.White
            };

            // 作业预览区域
            var previewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var previewLabel = new Label
            {
                Text = "已布置作业",
                Font = new Font("Microsoft YaHei", 12, FontStyle.Bold),
                AutoSize = true
            };

            previewFlowPanel = new FlowLayoutPanel
            {
                Location = new Point(0, previewLabel.Bottom + 15),
                Size = new Size(previewPanel.Width, previewPanel.Height - previewLabel.Height - 20),
                AutoScroll = true,
                WrapContents = true
            };

            // 组装表单区域
            imagePanel.Controls.Add(imageLabel);
            imagePanel.Controls.Add(uploadImageBtn);
            imagePanel.Controls.Add(clearImageBtn);

            buttonPanel.Controls.Add(submitBtn);
            buttonPanel.Controls.Add(clearBtn);

            importExportPanel.Controls.Add(importBtn);
            importExportPanel.Controls.Add(exportBtn);

            formPanel.Controls.Add(subjectLabel);
            formPanel.Controls.Add(subjectComboBox);
            formPanel.Controls.Add(homeworkLabel);
            formPanel.Controls.Add(homeworkTextBox);
            formPanel.Controls.Add(buttonPanel);
            formPanel.Controls.Add(imagePanel);
            formPanel.Controls.Add(importExportPanel);

            // 组装预览区域
            previewPanel.Controls.Add(previewLabel);
            previewPanel.Controls.Add(previewFlowPanel);

            // 组装主布局
            mainLayout.Controls.Add(formPanel, 0, 0);
            mainLayout.Controls.Add(previewPanel, 0, 2);

            teacherPage.Controls.Add(mainLayout);
        }

        private void SetupEventHandlers()
        {
            // 导航按钮
            studentBtn.Click += (s, e) => mainContainer.SelectedIndex = 0;
            teacherBtn.Click += (s, e) => mainContainer.SelectedIndex = 1;
            
            // 主题切换
            themeToggleBtn.Click += ToggleTheme;
            
            // 作业操作
            submitBtn.Click += SubmitHomework;
            clearBtn.Click += ClearAllHomework;
            
            // 图片操作
            uploadImageBtn.Click += UploadImage;
            clearImageBtn.Click += ClearImage;
            
            // 导入导出
            importBtn.Click += ImportHomework;
            exportBtn.Click += ExportHomework;
            
            // 定时器
            var timer = new Timer { Interval = 1000 };
            timer.Tick += (s, e) => UpdateTime();
            timer.Start();
        }

        private void UpdateTime()
        {
            currentTimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            
            // 计算剩余时间（假设自习结束时间为21:50）
            var now = DateTime.Now;
            var endTime = new DateTime(now.Year, now.Month, now.Day, 21, 50, 0);
            
            if (now > endTime)
            {
                countdownTimerLabel.Text = "自习结束";
                return;
            }
            
            var remaining = endTime - now;
            countdownTimerLabel.Text = $"{remaining.Hours}时{remaining.Minutes}分{remaining.Seconds}秒";
        }

        private void ToggleTheme(object sender, EventArgs e)
        {
            darkMode = !darkMode;
            themeToggleBtn.Text = darkMode ? "☀️" : "🌙";
            
            var bgColor = darkMode ? darkBg : lightBg;
            var textColor = darkMode ? Color.LightGray : Color.Black;
            var cardColor = darkMode ? darkCard : lightCard;
            
            // 更新主窗体
            BackColor = bgColor;
            studentPage.BackColor = bgColor;
            teacherPage.BackColor = bgColor;
            
            // 更新标签
            currentTimeLabel.ForeColor = Color.White;
            countdownTimerLabel.ForeColor = Color.White;
            footerLabel.ForeColor = darkMode ? Color.DarkGray : Color.Gray;
            
            // 更新作业卡片
            UpdateHomeworkCards();
        }

        private void SubmitHomework(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(homeworkTextBox.Text))
            {
                MessageBox.Show("请输入作业内容");
                return;
            }
            
            var subject = subjectComboBox.SelectedItem.ToString();
            homeworkData[subject] = homeworkTextBox.Text;
            homeworkTextBox.Clear();
            
            SaveData();
            UpdateHomeworkCards();
            MessageBox.Show($"{subject}作业已布置成功！");
        }

        private void ClearAllHomework(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空所有已布置的作业吗？", "确认", 
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                homeworkData.Clear();
                SaveData();
                UpdateHomeworkCards();
            }
        }

        private void UploadImage(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    customImage = Image.FromFile(openFileDialog.FileName);
                    noImageLabel.Visible = false;
                    
                    if (customImagePanel.Controls.Count > 1)
                        customImagePanel.Controls.RemoveAt(1);
                    
                    var pictureBox = new PictureBox
                    {
                        Image = customImage,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Dock = DockStyle.Fill
                    };
                    
                    customImagePanel.Controls.Add(pictureBox);
                    SaveData();
                }
            }
        }

        private void ClearImage(object sender, EventArgs e)
        {
            if (customImage != null)
            {
                customImage = null;
                customImagePanel.Controls.Clear();
                customImagePanel.Controls.Add(noImageLabel);
                noImageLabel.Visible = true;
                SaveData();
            }
        }

        private void ExportHomework(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON文件|*.json";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var data = new ExportData
                    {
                        Homework = homeworkData,
                        ImageBase64 = customImage != null ? ImageToBase64(customImage) : null
                    };
                    
                    File.WriteAllText(saveFileDialog.FileName, 
                        JsonConvert.SerializeObject(data, Formatting.Indented));
                    
                    MessageBox.Show("作业和图片已成功导出为JSON文件");
                }
            }
        }

        private void ImportHomework(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON文件|*.json";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var json = File.ReadAllText(openFileDialog.FileName);
                    var data = JsonConvert.DeserializeObject<ExportData>(json);
                    
                    homeworkData = data.Homework ?? new Dictionary<string, string>();
                    
                    if (!string.IsNullOrEmpty(data.ImageBase64))
                    {
                        customImage = Base64ToImage(data.ImageBase64);
                        noImageLabel.Visible = false;
                        
                        var pictureBox = new PictureBox
                        {
                            Image = customImage,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Dock = DockStyle.Fill
                        };
                        
                        customImagePanel.Controls.Clear();
                        customImagePanel.Controls.Add(pictureBox);
                    }
                    
                    SaveData();
                    UpdateHomeworkCards();
                    MessageBox.Show("数据导入成功！");
                }
            }
        }

        private void UpdateHomeworkCards()
        {
            homeworkFlowPanel.Controls.Clear();
            previewFlowPanel.Controls.Clear();
            
            var subjects = new[] { "语文", "数学", "英语", "物理", "化学", "生物" };
            var cardColor = darkMode ? darkCard : lightCard;
            var textColor = darkMode ? Color.LightGray : Color.Black;
            
            foreach (var subject in subjects)
            {
                homeworkData.TryGetValue(subject, out var content);
                CreateHomeworkCard(subject, content, homeworkFlowPanel, cardColor, textColor);
                CreateHomeworkCard(subject, content, previewFlowPanel, cardColor, textColor);
            }
        }

        private void CreateHomeworkCard(string subject, string content, Control container, Color bgColor, Color textColor)
        {
            var card = new Panel
            {
                Size = new Size(250, 180),
                BackColor = bgColor,
                Margin = new Padding(10),
                Padding = new Padding(15)
            };
            
            // 科目标题
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(0, 0, 0, 10)
            };
            
            var iconLabel = new Label
            {
                Text = GetSubjectIcon(subject),
                Font = new Font("Segoe UI", 16),
                Dock = DockStyle.Left,
                AutoSize = true
            };
            
            var subjectLabel = new Label
            {
                Text = subject,
                Font = new Font("Microsoft YaHei", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = GetSubjectColor(subject),
                Padding = new Padding(10, 0, 0, 0)
            };
            
            header.Controls.Add(iconLabel);
            header.Controls.Add(subjectLabel);
            
            // 作业内容
            var contentLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = string.IsNullOrEmpty(content) ? "今日无作业" : content,
                Font = new Font("Microsoft YaHei", 14),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = textColor
            };
            
            card.Controls.Add(header);
            card.Controls.Add(contentLabel);
            container.Controls.Add(card);
        }

        private string GetSubjectIcon(string subject)
        {
            return subject switch
            {
                "语文" => "📖",
                "数学" => "🧮",
                "英语" => "🔠",
                "物理" => "⚛️",
                "化学" => "🧪",
                "生物" => "🧬",
                _ => "📚"
            };
        }

        private Color GetSubjectColor(string subject)
        {
            return subject switch
            {
                "语文" => Color.FromArgb(231, 76, 60),
                "数学" => Color.FromArgb(52, 152, 219),
                "英语" => Color.FromArgb(243, 156, 18),
                "物理" => Color.FromArgb(155, 89, 182),
                "化学" => Color.FromArgb(26, 188, 156),
                "生物" => Color.FromArgb(46, 204, 113),
                _ => Color.Black
            };
        }

        private void LoadData()
        {
            var dataFile = Path.Combine(Application.StartupPath, "studydata.json");
            
            if (File.Exists(dataFile))
            {
                var json = File.ReadAllText(dataFile);
                var data = JsonConvert.DeserializeObject<ExportData>(json);
                
                homeworkData = data.Homework ?? new Dictionary<string, string>();
                
                if (!string.IsNullOrEmpty(data.ImageBase64))
                {
                    customImage = Base64ToImage(data.ImageBase64);
                    noImageLabel.Visible = false;
                    
                    var pictureBox = new PictureBox
                    {
                        Image = customImage,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Dock = DockStyle.Fill
                    };
                    
                    customImagePanel.Controls.Clear();
                    customImagePanel.Controls.Add(pictureBox);
                }
            }
            
            UpdateHomeworkCards();
        }

        private void SaveData()
        {
            var data = new ExportData
            {
                Homework = homeworkData,
                ImageBase64 = customImage != null ? ImageToBase64(customImage) : null
            };
            
            File.WriteAllText(Path.Combine(Application.StartupPath, "studydata.json"), 
                JsonConvert.SerializeObject(data));
        }

        private string ImageToBase64(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private Image Base64ToImage(string base64String)
        {
            var bytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }

        // 控件声明
        private TabControl mainContainer;
        private TabPage studentPage;
        private TabPage teacherPage;
        private Label currentTimeLabel;
        private Label countdownTimerLabel;
        private Button studentBtn;
        private Button teacherBtn;
        private Button themeToggleBtn;
        private FlowLayoutPanel homeworkFlowPanel;
        private FlowLayoutPanel previewFlowPanel;
        private Panel customImagePanel;
        private Label noImageLabel;
        private ComboBox subjectComboBox;
        private TextBox homeworkTextBox;
        private Button submitBtn;
        private Button clearBtn;
        private Button uploadImageBtn;
        private Button clearImageBtn;
        private Button importBtn;
        private Button exportBtn;
        private Label footerLabel;
    }

    public class ExportData
    {
        public Dictionary<string, string> Homework { get; set; }
        public string ImageBase64 { get; set; }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}