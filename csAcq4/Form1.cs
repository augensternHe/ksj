using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Hamamatsu.DCAM4;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.NVRTC;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Python.Runtime;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace csAcq4
{
    public partial class Form1 : Form
    {
        private Logger logger;//日志记录
        public Form1()
        {
            InitializeComponent();
            InitializePython();
            AddAlgorithmToPythonPath();
            // 初始时禁用 funGroup 和 saveGroup 中的所有控件
            DisableAllControls(funGroup);
            DisableAllControls(saveGroup);
            //InitializePictureBoxEvents();//屏幕点击事件
            InitializesmallPicShowEvents();//小屏点击事件
            InitializeFontSizeContextMenu();  // 初始化文本字号选择右键菜单
            // 实例化 logger
            logger = new Logger();
            menuStrip1.Renderer = new CustomToolStripRenderer();

            // 为每个顶级菜单项添加 DropDownOpened 和 DropDownClosed 事件
            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                item.DropDownOpened += (sender, e) => { item.ForeColor = System.Drawing.Color.Black; };
                item.DropDownClosed += (sender, e) => { item.ForeColor = System.Drawing.Color.White; };
            }


            EnablePanelDragging(uiTitlePanel1);
            EnablePanelDragging(histogramPanel);
            //直方图显示
            // 初始化下拉框
            histSelect.Items.AddRange(new string[]
            {
            "PicDisplay1", "PicDisplay2", "PicDisplay3", "PicDisplay4",
            "PicDisplay5", "PicDisplay6", "PicDisplay7", "PicDisplay8", "PicDisplay9"
            });

            // 默认选中第一个
            histSelect.SelectedIndex = 0;
            exposureNum.ValueChanged += exposureNum_ValueChanged;
            InitializeZoomAndDragEvents();
            // 绑定 rightPanel 的 Resize 事件
            rightPanel.Resize += RightPanel_Resize;
            // 计算并保存 histogramPanel 的宽高比
            histogramAspectRatio = (float)histogramPanel.Height / histogramPanel.Width;
            histLineRatio = (float)histLine.Height / histLine.Width;

            colorToolSelec.Paint += colorToolSelec_Paint;
            pixelCombox.SelectedIndexChanged += pixelCombox_SelectedIndexChanged;

            initCudaProceImage();
            LoadModels();

            // 在主窗体显示后，关闭启动画面
            SplashForm splash = Application.OpenForms.OfType<SplashForm>().FirstOrDefault();
            if (splash != null && !splash.IsDisposed && splash.IsHandleCreated)
            {
                // 确保线程安全关闭并释放资源
                splash.BeginInvoke((Action)(() =>
                {
                    splash.Close();
                    splash.Dispose(); // 确保释放资源
                }));
            }
        }
        private List<TriangleButton> displayButtons = new List<TriangleButton>();
        private void initChangeBtn()
        {
            isInitBorderShow = true;
            for (int i = 1; i <= 9; i++)
            {
                PictureBox picDisplay = this.Controls.Find("PicDisplay" + i, true).FirstOrDefault() as PictureBox;
                if (picDisplay != null)
                {
                    picDisplay.MouseEnter += PicDisplay_MouseEnter;
                    picDisplay.MouseLeave += PicDisplay_MouseLeave;
                    picDisplay.Paint += PicDisplay_Paint;
                }
            }
            // 定义按钮的颜色
            System.Drawing.Color[] buttonColors =
            {
                System.Drawing.Color.FromArgb(220, 0, 255),
                System.Drawing.Color.FromArgb(100, 0, 255),
                System.Drawing.Color.FromArgb(0, 100, 255),
                System.Drawing.Color.Lime,
                System.Drawing.Color.FromArgb(0, 255, 220),
                System.Drawing.Color.FromArgb(255, 180, 0),
                System.Drawing.Color.FromArgb(255, 80, 0),
                System.Drawing.Color.FromArgb(240, 0, 0),
                System.Drawing.Color.FromArgb(160, 0, 0)
            };

            // 遍历 PicDisplay1-9
            for (int i = 1; i <= 9; i++)
            {
                PictureBox picDisplay = this.Controls.Find("PicDisplay" + i, true).FirstOrDefault() as PictureBox;

                if (picDisplay != null)
                {
                    // 创建三角形按钮
                    TriangleButton triangleButton = new TriangleButton
                    {
                        Name = "displayBtn" + i,
                        Size = new System.Drawing.Size(30, 30),
                        BackColor = buttonColors[i - 1],
                        FlatStyle = FlatStyle.Flat,
                        Location = new System.Drawing.Point(0, 0), // 左上角
                        FlatAppearance = { BorderSize = 0 }
                    };

                    // 将按钮添加到 PictureBox 上
                    picDisplay.Controls.Add(triangleButton);
                    triangleButton.BringToFront(); // 确保按钮在顶层

                    // 绑定点击事件
                    triangleButton.MouseDown += ChangeButtonColor;

                    // 将按钮添加到列表中
                    displayButtons.Add(triangleButton);
                }
            }
            PictureBox fullDisplay = this.Controls.Find("FullDisPlay", true).FirstOrDefault() as PictureBox;
            fullPicBtn = new TriangleButton
            {
                Name = "fullPicBtn",
                BackColor = System.Drawing.Color.Transparent,
                Size = new System.Drawing.Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(0, 0), // 左上角
                FlatAppearance = { BorderSize = 0 }
            };

            // 将按钮添加到 PictureBox 上
            fullDisplay.Controls.Add(fullPicBtn);
            fullPicBtn.BringToFront(); // 确保按钮在顶层
        }
        private void InitializesmallPicShowEvents()
        {
            //为显示屏幕添加点击世间：
            smallShow1.Click += new EventHandler(smallPic_Click);
            smallShow2.Click += new EventHandler(smallPic_Click);
            smallShow3.Click += new EventHandler(smallPic_Click);
            smallShow4.Click += new EventHandler(smallPic_Click);
            smallShow5.Click += new EventHandler(smallPic_Click);
            smallShow6.Click += new EventHandler(smallPic_Click);
            smallShow7.Click += new EventHandler(smallPic_Click);
            smallShow8.Click += new EventHandler(smallPic_Click);
            smallShow9.Click += new EventHandler(smallPic_Click);
        }

        private Mat originalFrame;  // 用于存储原始未处理的图像帧

        private int imageCounter = 0; // 用于跟踪保存的图片序号
        private string m_savePath;//用于保存图片文件存储地址
        private CancellationTokenSource cancellationTokenSource;

        private VideoWriter videoWriter;
        private bool isRecording = false;
        private int frameWidth, frameHeight;
        private double frameRate;

        private Bitmap backgroundImage;
        private bool isBackgroundSubtractionEnabled = false;

        private Dictionary<char, Mat> groupedFrames;
        private Mat loadedImage; // 用于存储加载的图片
        private List<Mat> videoFrames; // 用于存储加载的视频帧
        private Mat currentFrame; // 用于存储当前帧的图像（无论是从相机还是文件读取）
        List<List<string>> columnArrays;//用于保存切割定位点信息
        private Dictionary<int, System.Drawing.Color> pseudoColorButtonColors = new Dictionary<int, System.Drawing.Color>(); // 记录伪彩色按钮的颜色
        private Dictionary<int, bool> pseudoColorEnabled = new Dictionary<int, bool>(); // 记录每个按钮的伪彩状态
        private readonly object frameLock = new object(); // 锁对象
        private CancellationTokenSource cts; // 用于取消任务的令牌
        //用于读取视频并循环播放
        private System.Windows.Forms.Timer videoTimer; // 使用 Windows Forms 的 Timer
        private int currentFrameIndex = 0;
        private delegate void MyDelegate_UpdateDisplay();
        // 用于记录显示屏幕上一个绑定的事件处理函数
        private MouseEventHandler lastBoundHandler = null;

        // 存储每个 PictureBox 的点
        private Dictionary<PictureBox, List<System.Drawing.Point>> drawnPoints = new Dictionary<PictureBox, List<System.Drawing.Point>>();

        // 当前选中的点和 PictureBox
        private System.Drawing.Point? selectedPoint = null;
        private PictureBox selectedPictureBox = null;
        //用于控制拖动显示功能
        private bool isSlideDragging = false;
        private System.Drawing.Point dragCursorPoint;
        private System.Drawing.Point dragControlPoint;
        private System.Windows.Forms.Timer frameRateTimer; // 定时器用于实时获取帧率
        private PictureBox currentHoverPictureBox;//用于滚轮放大缩小
        private Dictionary<PictureBox, float> pictureBoxScales;
        private Dictionary<PictureBox, System.Drawing.Point> pictureBoxOffsets;
        private bool isDraggingImage = false;
        private System.Drawing.Point dragStartPoint;
        private float fullDisplayScale = 1.0f; // 初始缩放系数为1
        private System.Drawing.Point fullDisplayOffset = System.Drawing.Point.Empty; // 初始偏移量为0
        private bool isDraggingFullDisplay = false;
        private System.Drawing.Point dragStartPointFullDisplay;
        private PictureBox currentFullDisplayPictureBox = null; // 用于记录 FullDisplay 当前显示的 PictureBox
        private bool isDraggingFullDisplayImage = false;  // 标识是否正在拖动 FullDisplay
        private System.Drawing.Point fullDisplayDragStartPoint; // 记录拖动的起始点
                                                                // 获取当前设置的曝光时间
        double currentFrameRate = 0;//当前帧率
        // 保存 histogramPanel 的宽高比
        private float histogramAspectRatio;
        private float histLineRatio;
        private bool isVideoPlaying = false;
        private string activeCamModelSN = "相机未连接";
        private Bitmap shownPic;
        private double shownPicWidth = 512; //bitmap的大小，随意给个初始值
        private double shownPicHeight = 256; //bitmap的大小，随意给个初始值
        private double pixeltype;
        private TriangleButton fullPicBtn; // 全局变量
        //相机一台
        private HisCamera activeCam;
        //含原始数据的图片一张
        private HisPic activePic = new HisPic();
        //用于实时预览的新线程，与用户界面可以并行，使得在实时预览时依然可以接受响应用户界面发生的新事件
        Thread hisThread;

        private bool resizing = false;
        private System.Drawing.Point previousMousePosition;
        private Panel rightEdgePanel;
        private Panel leftEdgePanel;
        private Panel topEdgePanel;
        private Panel bottomEdgePanel;

        private const int MinFormWidth = 200;  // 设置窗体的最小宽度
        private const int MinFormHeight = 150; // 设置窗体的最小高度

        private bool isInitBorderShow = false;
        private void InitializeEdgePanel()
        {
            // 初始化边缘 Panel
            rightEdgePanel = new Panel
            {
                Width = 2,
                Height = this.Height,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Right
            };

            leftEdgePanel = new Panel
            {
                Width = 2,
                Height = this.Height,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Left
            };

            topEdgePanel = new Panel
            {
                Width = this.Width,
                Height = 2,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Top
            };

            bottomEdgePanel = new Panel
            {
                Width = this.Width,
                Height = 2,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Bottom
            };

            // 将边缘 Panel 添加到窗体中
            this.Controls.Add(rightEdgePanel);
            this.Controls.Add(leftEdgePanel);
            this.Controls.Add(topEdgePanel);
            this.Controls.Add(bottomEdgePanel);

            // 为每个边缘 Panel 添加鼠标事件
            AddMouseEvents(rightEdgePanel);
            AddMouseEvents(leftEdgePanel);
            AddMouseEvents(topEdgePanel);
            AddMouseEvents(bottomEdgePanel);
        }
        private void UninitializeEdgePanel()
        {
            // 移除边缘 Panel 的鼠标事件
            RemoveMouseEvents(rightEdgePanel);
            RemoveMouseEvents(leftEdgePanel);
            RemoveMouseEvents(topEdgePanel);
            RemoveMouseEvents(bottomEdgePanel);

            // 从窗体中移除边缘 Panel
            this.Controls.Remove(rightEdgePanel);
            this.Controls.Remove(leftEdgePanel);
            this.Controls.Remove(topEdgePanel);
            this.Controls.Remove(bottomEdgePanel);

            // 释放 Panel 的资源
            rightEdgePanel.Dispose();
            leftEdgePanel.Dispose();
            topEdgePanel.Dispose();
            bottomEdgePanel.Dispose();

            // 将边缘 Panel 设置为 null，便于判断是否已取消初始化
            rightEdgePanel = null;
            leftEdgePanel = null;
            topEdgePanel = null;
            bottomEdgePanel = null;
        }
        private void RemoveMouseEvents(Panel edgePanel)
        {
            if (edgePanel != null)
            {
                edgePanel.MouseDown -= EdgePanel_MouseDown;
                edgePanel.MouseMove -= EdgePanel_MouseMove;
                edgePanel.MouseUp -= EdgePanel_MouseUp;
                edgePanel.MouseEnter -= EdgePanel_MouseEnter;
                edgePanel.MouseLeave -= EdgePanel_MouseLeave;
            }
        }

        private void AddMouseEvents(Panel edgePanel)
        {
            edgePanel.MouseDown += EdgePanel_MouseDown;
            edgePanel.MouseMove += EdgePanel_MouseMove;
            edgePanel.MouseUp += EdgePanel_MouseUp;
            edgePanel.MouseEnter += EdgePanel_MouseEnter;
            edgePanel.MouseLeave += EdgePanel_MouseLeave;
        }

        private void EdgePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                resizing = true;
                previousMousePosition = Cursor.Position;
            }
        }

        private void EdgePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (resizing)
            {
                System.Drawing.Point currentMousePosition = Cursor.Position;
                int dx = currentMousePosition.X - previousMousePosition.X;
                int dy = currentMousePosition.Y - previousMousePosition.Y;

                Panel panel = sender as Panel;
                if (panel != null)
                {
                    if (panel.Dock == DockStyle.Right)
                    {
                        // 右侧边缘，改变窗体宽度
                        int newWidth = this.Width + dx;
                        if (newWidth >= MinFormWidth)
                        {
                            this.Width = newWidth;
                            previousMousePosition = currentMousePosition;
                        }
                    }
                    else if (panel.Dock == DockStyle.Left)
                    {
                        // 左侧边缘，改变窗体宽度
                        int newWidth = this.Width - dx;
                        if (newWidth >= MinFormWidth)
                        {
                            this.Width = newWidth;
                            this.Left += dx;
                            previousMousePosition = currentMousePosition;
                        }
                    }
                    else if (panel.Dock == DockStyle.Top)
                    {
                        // 顶部边缘，改变窗体高度
                        int newHeight = this.Height - dy;
                        if (newHeight >= MinFormHeight)
                        {
                            this.Height = newHeight;
                            this.Top += dy;
                            previousMousePosition = currentMousePosition;
                        }
                    }
                    else if (panel.Dock == DockStyle.Bottom)
                    {
                        // 底部边缘，改变窗体高度
                        int newHeight = this.Height + dy;
                        if (newHeight >= MinFormHeight)
                        {
                            this.Height = newHeight;
                            previousMousePosition = currentMousePosition;
                        }
                    }
                }
            }
        }

        private void EdgePanel_MouseUp(object sender, MouseEventArgs e)
        {
            resizing = false;
        }

        private void EdgePanel_MouseEnter(object sender, EventArgs e)
        {
            // 当鼠标进入边缘 panel 时，改变光标为相应的拖动样式
            Panel panel = sender as Panel;
            if (panel != null)
            {
                if (panel.Dock == DockStyle.Right || panel.Dock == DockStyle.Left)
                {
                    this.Cursor = Cursors.SizeWE; // 左右拖动样式
                }
                else if (panel.Dock == DockStyle.Top || panel.Dock == DockStyle.Bottom)
                {
                    this.Cursor = Cursors.SizeNS; // 上下拖动样式
                }
            }
        }

        private void EdgePanel_MouseLeave(object sender, EventArgs e)
        {
            // 当鼠标离开边缘 panel 时，恢复光标为默认样式
            if (!resizing)
            {
                this.Cursor = Cursors.Default;
            }
        }
        private void InitializePython()
        {
            Console.WriteLine("python 初始化");
            string pythonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "python-embed");
            Environment.SetEnvironmentVariable("PYTHONHOME", pythonPath);
            Environment.SetEnvironmentVariable("PYTHONPATH", Path.Combine(pythonPath, "Lib\\site-packages"));

            // 添加这行
            Runtime.PythonDLL = Path.Combine(pythonPath, "python311.dll");  // 替换 X 为你的 Python 版本，例如 python39.dll

            PythonEngine.PythonHome = pythonPath;
            PythonEngine.Initialize();
        }
        private void AddAlgorithmToPythonPath()
        {
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                string basePath = AppDomain.CurrentDomain.BaseDirectory;

                // 添加 algorithm 目录
                string algorithmPath = Path.Combine(basePath, "algorithm");
                sys.path.append(algorithmPath);

                // 添加 lineDraw 和 enhance 目录
                string lineDrawPath = Path.Combine(algorithmPath, "lineDraw");
                string enhancePath = Path.Combine(algorithmPath, "enhance");
                sys.path.append(lineDrawPath);
                sys.path.append(enhancePath);
            }
        }
        // ---------------- 私有类----------------
        // 递归查找并为所有 GroupBox 控件绑定 Paint 事件
        private void ApplyCustomPaintToGroupBoxes(System.Windows.Forms.Control parent)
        {
            foreach (System.Windows.Forms.Control control in parent.Controls)
            {
                if (control is GroupBox groupBox)
                {
                    // 绑定 Paint 事件
                    groupBox.Paint += new PaintEventHandler(groupBox_Paint);
                }

                // 递归遍历子控件
                if (control.Controls.Count > 0)
                {
                    ApplyCustomPaintToGroupBoxes(control);
                }
            }
        }
        void groupBox_Paint(object sender, PaintEventArgs e)
        {
            GroupBox gBox = (GroupBox)sender;

            // 清除默认绘制的背景，去掉默认边框
            e.Graphics.Clear(gBox.BackColor);

            // 获取标题文本的尺寸
            var vSize = e.Graphics.MeasureString(gBox.Text, gBox.Font);

            // 绘制新的灰色线条 (40, 40, 40)
            //using (Pen pen = new Pen(System.Drawing.Color.FromArgb(50, 50, 50)))
            using (Pen pen = new Pen(System.Drawing.Color.Black))
            {
                // 绘制线条
                e.Graphics.DrawLine(pen, 1, vSize.Height / 2, 8, vSize.Height / 2); // 左标题线
                e.Graphics.DrawLine(pen, vSize.Width + 8, vSize.Height / 2, gBox.Width - 2, vSize.Height / 2); // 右标题线
                e.Graphics.DrawLine(pen, 1, vSize.Height / 2, 1, gBox.Height - 2); // 左边框线
                e.Graphics.DrawLine(pen, 1, gBox.Height - 2, gBox.Width - 2, gBox.Height - 2); // 底边框线
                e.Graphics.DrawLine(pen, gBox.Width - 2, vSize.Height / 2, gBox.Width - 2, gBox.Height - 2); // 右边框线
            }
            //根据 GroupBox 的启用状态设置文本颜色
            System.Drawing.Color textColor = gBox.Enabled ? System.Drawing.Color.White : System.Drawing.Color.Black;

            // 绘制标题文本
            using (Brush brush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(gBox.Text, gBox.Font, brush, new PointF(8, 0));
            }
        }

        public class TriangleButton : Button
        {
            protected override void OnPaint(PaintEventArgs e)
            {
                // 清除按钮背景，保持透明
                e.Graphics.Clear(this.Parent.BackColor);

                // 创建三角形路径
                System.Drawing.Point[] trianglePoints =
                {
                    new System.Drawing.Point(0, 0),       // 左上角
                    new System.Drawing.Point(this.Width, 0),  // 右上角
                    new System.Drawing.Point(0, this.Height)  // 左下角
                };

                // 设置按钮的区域为三角形
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddPolygon(trianglePoints);
                    this.Region = new Region(path);
                }

                // 使用指定的颜色填充三角形
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPolygon(brush, trianglePoints);
                }
            }

            protected override void OnPaintBackground(PaintEventArgs pevent)
            {
                // 禁用默认背景绘制，保持透明
            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
            }
        }
        public class CustomToolStripRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                // 调用默认背景渲染
                base.OnRenderMenuItemBackground(e);

                // 如果是顶级菜单项
                if (e.Item.OwnerItem == null)
                {
                    // 如果选中或子菜单已展开，设置前景色为黑色
                    if (e.Item.Selected || (e.Item is ToolStripMenuItem menuItem && menuItem.DropDown.Visible))
                    {
                        e.Item.ForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        // 未选中时前景色为白色
                        e.Item.ForeColor = System.Drawing.Color.White;
                    }
                }
                else
                {
                    // 子菜单项始终保持黑色
                    e.Item.ForeColor = System.Drawing.Color.Black;
                }
            }
        }

        // ---------------- 日志处理类----------------
        public class Logger
        {
            public string logFilePath;
            public Logger()
            {
                // 设置日志文件的路径
                logFilePath = GetLogFilePath();
            }
            private string GetLogFilePath()
            {
                // 1. 确保创建 log 目录
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // 2. 获取当前日期的文件名，例如：20240926.txt
                string date = DateTime.Now.ToString("yyyyMMdd");
                string logFileName = $"{date}.txt";
                string fullPath = Path.Combine(logDirectory, logFileName);

                // 3. 返回文件路径，如果文件已存在，日志将自动追加
                return fullPath;
            }
            public void LogButtonClick(string buttonName)
            {
                string logEntry = $"{DateTime.Now}: {buttonName} clicked\n";
                File.AppendAllText(logFilePath, logEntry);  // 将日志追加到文件中
            }
            // 写日志到txt文件
            private void WriteLog(string logEntry)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(logFilePath, true)) // true 表示追加写入
                    {
                        sw.WriteLine(logEntry);
                    }
                }
                catch (Exception ex)
                {
                    // 捕获写入日志时可能出现的错误
                    Console.WriteLine($"写入日志失败: {ex.Message}");
                }
            }
        }
        // ---------------- 私有类 end----------------

        //-----------------窗体句柄处理操作——————

        private void initCudaProceImage()
        {
            // 初始化 CUDA 上下文和内核，只执行一次
            cudaContext = new CudaContext(0, CUCtxFlags.MapHost);

            string cudaKernelCode = @"
    extern ""C""
    __global__ void ProcessImageKernel(const unsigned char* inputImage, unsigned char* outputImage, int width, int height, int centerX, int centerY)
    {
        int x = blockIdx.x * blockDim.x + threadIdx.x;
        int y = blockIdx.y * blockDim.y + threadIdx.y;

        if (x < width && y < height)
        {
            int idx = y * width + x;
            float i = static_cast<float>(y);
            float j = static_cast<float>(x);
            float centerXF = static_cast<float>(centerX);
            float centerYF = static_cast<float>(centerY);

            float distanceToCenter = __fsqrt_rn((i - centerYF) * (i - centerYF) + (j - centerXF) * (j - centerXF));
            float yValue = -4e-12f * __powf(distanceToCenter, 6) 
                           + 9e-10f * __powf(distanceToCenter, 5)
                           - 9e-8f * __powf(distanceToCenter, 4) 
                           + 6e-6f * __powf(distanceToCenter, 3)
                           - 0.0004f * (distanceToCenter * distanceToCenter) 
                           + 0.0019f * distanceToCenter + 1.0004f;

            int originalValue = inputImage[idx];
            int newValue = (int)(originalValue/yValue);
            newValue = max(0, min(newValue, 255));

            outputImage[idx] = static_cast<unsigned char>(newValue);
        }
    }";

            // 编译和加载 CUDA 内核
            CudaRuntimeCompiler rtc = new CudaRuntimeCompiler(cudaKernelCode, "ProcessImageKernel");
            rtc.Compile(new string[] { });
            CUmodule cudaModule = cudaContext.LoadModulePTX(rtc.GetPTX());
            processImageKernel = new CudaKernel("ProcessImageKernel", cudaModule, cudaContext);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Update window title
            if (IntPtr.Size == 4)
            {
                Text = "csAcq4 (32 bit)";
            }
            else
            if (IntPtr.Size == 8)
            {
                Text = "csAcq4 (64 bit)";
            }
            // 获取应用程序的执行路径
            string applicationPath = Application.StartupPath;

            // 设置 pathLabel 的文本为应用程序路径
            pathLabel.Text = applicationPath;

            // 同时更新 m_savePath 变量
            m_savePath = applicationPath;

            string excelFilePath = "data/loc.xlsx";
            if (string.IsNullOrEmpty(excelFilePath) || !File.Exists(excelFilePath))
            {
                Console.WriteLine("Excel 文件路径无效或文件不存在。");
                return;
            }
            columnArrays = ReadExcelFileToColumnArrays(excelFilePath);
            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                pictureBox.MouseMove += PictureBox_MouseMove;
            }
            // 在表单或初始化函数中进行绑定
            textToolBtn.MouseUp += textToolBtn_MouseUp;

            // 限制最大化时的区域为屏幕的工作区
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            // 设置窗口状态为最大化
            this.WindowState = FormWindowState.Maximized;
            InitializeHistLine();
            middlePanel.Resize += MiddlePanel_Resize;
            MiddlePanel_Resize(null, null);

            // 递归查找并绑定所有 GroupBox 的 Paint 事件
            ApplyCustomPaintToGroupBoxes(this);
        }
        private void Form1_Shown(object sender, EventArgs e)
        {

            // 设置窗体的默认字体
            this.Font = new System.Drawing.Font("Segoe UI", this.Font.Size);

        }

        //-----------------窗体句柄处理操作 end——————

        //-----------------状态判断功能----------------
        //窗口重绘

        //自定义状态栏
        private bool dragging = false;
        private System.Drawing.Point cursorStartPoint;
        private System.Drawing.Point formStartPoint;
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            cursorStartPoint = Cursor.Position;
            formStartPoint = this.Location;
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                System.Drawing.Point diff = System.Drawing.Point.Subtract(Cursor.Position, new System.Drawing.Size(cursorStartPoint));
                this.Location = System.Drawing.Point.Add(formStartPoint, new System.Drawing.Size(diff));
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void MyShowStatus(string text) { bottomLabelRight.Text = text; }

        //------------------状态判断 end---------------

        // ---------------- 设备读取播放功能 ----------------

        //程序是否正在预览状态
        private static bool liveStatus = false;
        private void pushLive_Click(object sender, EventArgs e)
        {
            if (!isInitBorderShow)
                initChangeBtn();
            if (liveStatus == false)
            {
                if (isVideoPlaying) {
                    endPlatingVideo();
                }
                logger.LogButtonClick("pushLive_Click");
                liveStatus = true;
                //检查当前的pixelType
                activeCam.getPixelType(ref pixeltype);
                Console.WriteLine($"pixeltype:{pixeltype}");


                //在下面能实现时间的显示，以及再次点击时停止显示就行了。

                hisThread = new Thread(new ThreadStart(LiveData));
                hisThread.Start();

                // 启用 saveGroup 中的控件
                EnableAllControls(saveGroup);

                //InitializeFrameRateTimer();

                pixelCombox.Enabled = false;
                isVideoPlaying = false;

                pushLive.Text = "Pause";
            }
            else
            {
                liveStatus = false;

                pushLive.Text = "Live";
                MyShowStatus("播放已停止");
            }
        }


        //一个委托类型，新线程hisThread可以通过委托让Form1主线程干活，实现两个线程之间的控制；
        //两个线程之间的参数（如代表预览状态的bool liveStatus）传递通过Form1类的各属性进行
        private delegate void hisDelegate();

        //由新线程hisThread通过invoke和委托触发、由主线程执行的代码；从相机/DCAM控制的内存中拿出最新一张图片的数据，转化成Bitmap并显示，参见说明中的第2.1.3和2.1.4部分
        private void ShowLiveImage()
        {
            // 确保 liveStatus 为 true
            if (!liveStatus)
            {
                return;
            }

            // 清空 Bitmap，以防止之前的数据叠加
            if (shownPic != null)
            {
                shownPic.Dispose();  // 释放之前的 Bitmap
                shownPic = new Bitmap((int)shownPicWidth, (int)shownPicHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            }
            MyShowStatus(activeCam.GetPicFromBuf(ref activePic));//从内存中取出图片数据，返回一个HisPic实例

            activePic.PixelSize = (int)pixeltype;
            activePic.TransferFromHisPicTo24RGBBitMap(ref shownPic, lutMin, lutMax, shownPicWidth, shownPicHeight);   //将HisPic activePic中的数据倒到Bitmap shownPic中，同时顾及LUT的参数

            currentFrame = BitmapConverter.ToMat(shownPic);
            originalFrame = currentFrame.Clone();
            ProcessVideoFrames();
        }
        private void LiveData()
        {
            //先设定一下，保证相机的DCAM_IDPROP_CAPTUREMODE是默认的DCAMPROP_CAPTUREMODE__NORMAL

            activeCam.SetToNormalCaptureMode();


            activeCam.DoNotUseShadingCalibration();

            //循环采集dcambuf中一张张图片的第一个像素灰度值，并在ProgramInfo.Text里面显示出来

            activeCam.AllocBuffer(10);   //分配能存图片的内存

            string camERR = activeCam.StartCapture(DCAMCAP_START.SEQUENCE);  //开始拍照

            //让主线程执行ShowLiveImage()，实时显示图像；或者用ShowPixelData代替ShowLiveImage实时显示每张采集图像的第一个像素值供测试用
            hisDelegate DoShowLiveImage = new hisDelegate(ShowLiveImage);

            ////这里在while循环前面先用三行代码来一张图像，是为了在没有Live的时候点击Capture1时能够获取最新的图像
            //activeCam.DcamWait(10000, DCAMWAIT.CAPEVENT.FRAMEREADY);
            //Invoke(DoShowLiveImage);
            //Thread.Sleep(100);

            while (liveStatus)
            {
                activeCam.DcamWait(10000, DCAMWAIT.CAPEVENT.FRAMEREADY);
                Invoke(DoShowLiveImage);
                Thread.Sleep(100);
            }

            activeCam.StopSequenceCapture();
            activeCam.ReleaseBuffer();

        }


        private void pixelCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pixelCombox.SelectedItem == null) return;
            int selectedBitDepth;
            if (int.TryParse(pixelCombox.SelectedItem.ToString(), out selectedBitDepth))
            {
                // 成功转换为int
            }
            else
            {
                // 处理转换失败的情况
                MessageBox.Show("Invalid bit depth selection");
            }

            // 根据选择的位深度设置相应的 PIXELTYPE
            switch (selectedBitDepth)
            {
                case 8:
                    MyShowStatus(activeCam.SetDCAMIDPROP(DCAMIDPROP.IMAGE_PIXELTYPE, (int)DCAM_PIXELTYPE.MONO8));
                    break;
                case 12:
                    MyShowStatus(activeCam.SetDCAMIDPROP(DCAMIDPROP.IMAGE_PIXELTYPE, (int)DCAM_PIXELTYPE.MONO16));
                    break;
                case 16:
                    MyShowStatus(activeCam.SetDCAMIDPROP(DCAMIDPROP.IMAGE_PIXELTYPE, (int)DCAM_PIXELTYPE.MONO16));
                    break;
                default:
                    return; // 不支持的位深度
            }
            MyShowStatus(activeCam.setBitDepth(selectedBitDepth));

            lutMin = 0;
            lutMax = (float)(Math.Pow(2, selectedBitDepth) - 1);    //例如，相机是16bit的，那么灰度值最大就是 2^16 - 1 = 65535
        }


        private void ConvertFrameToGrayscale(ref Mat frame)
        {
            // 如果图像是彩色图像 (8UC3 或 16UC3)，将其转换为灰度图 (8UC1)
            if (frame.Type() == MatType.CV_8UC3 || frame.Type() == MatType.CV_16UC3)
            {
                Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY); // 转换为灰度图
            }
            // 如果图像是16位单通道图像 (16UC1)，将其转换为8位灰度图 (8UC1)
            if (frame.Type() == MatType.CV_16UC1)
            {
                frame.ConvertTo(frame, MatType.CV_8UC1, 255.0 / 65535.0); // 缩放到8-bit范围
            }
            // 如果已经是8位单通道灰度图像 (8UC1)，不做处理

        }

        private CudaContext cudaContext;
        private CudaKernel processImageKernel;

        public Mat ProcessImageWithCuda(Mat img)
        {
            if (img.Empty() || img.Type() != MatType.CV_8UC1)
                throw new ArgumentException("图像应为灰度图且不为空。");

            int width = img.Cols;
            int height = img.Rows;
            int blockWidth = width / 8;
            int blockHeight = height / 8;

            // 初始化输出图像
            Mat processedImg = new Mat(height, width, MatType.CV_8UC1);

            // 对每个8x8的块进行处理
            for (int by = 0; by < 8; by++)
            {
                for (int bx = 0; bx < 8; bx++)
                {
                    // 获取当前块的ROI区域
                    Rect roi = new Rect(bx * blockWidth, by * blockHeight, blockWidth, blockHeight);
                    Mat block = new Mat(img, roi);  // 从原图提取小块

                    // 将块传输到CUDA设备进行处理
                    Mat processedBlock = ProcessBlockWithCuda(block, blockWidth, blockHeight);

                    // 将处理后的块复制回输出图像中的相应位置
                    processedBlock.CopyTo(new Mat(processedImg, roi));
                }
            }

            return processedImg;
        }

        // 处理单个小块的CUDA处理函数
        private Mat ProcessBlockWithCuda(Mat block, int blockWidth, int blockHeight)
        {
            int numPixels = blockWidth * blockHeight;
            int centerX = blockWidth / 2;
            int centerY = blockHeight / 2;

            // 将输入块数据传输到CUDA设备
            byte[] inputArray = new byte[numPixels];
            Marshal.Copy(block.Data, inputArray, 0, numPixels);
            using (CudaDeviceVariable<byte> deviceInputImage = new CudaDeviceVariable<byte>(numPixels))
            using (CudaDeviceVariable<byte> deviceOutputImage = new CudaDeviceVariable<byte>(numPixels))
            {
                deviceInputImage.CopyToDevice(inputArray);

                // 设置线程和块的维度
                uint threadsPerBlockX = 16;
                uint threadsPerBlockY = 16;
                uint blocksPerGridX = (uint)((blockWidth + threadsPerBlockX - 1) / threadsPerBlockX);
                uint blocksPerGridY = (uint)((blockHeight + threadsPerBlockY - 1) / threadsPerBlockY);

                processImageKernel.BlockDimensions = new ManagedCuda.VectorTypes.dim3(threadsPerBlockX, threadsPerBlockY, 1);
                processImageKernel.GridDimensions = new ManagedCuda.VectorTypes.dim3(blocksPerGridX, blocksPerGridY, 1);

                // 执行CUDA内核
                processImageKernel.Run(deviceInputImage.DevicePointer, deviceOutputImage.DevicePointer, blockWidth, blockHeight, centerX, centerY);

                // 将结果复制回主机
                byte[] outputArray = new byte[numPixels];
                deviceOutputImage.CopyToHost(outputArray);

                // 创建输出块图像
                Mat processedBlock = new Mat(blockHeight, blockWidth, MatType.CV_8UC1);
                Marshal.Copy(outputArray, 0, processedBlock.Data, numPixels);

                return processedBlock;
            }
        }

        private void ProcessVideoFrames()
        {
            if (currentFrame == null || currentFrame.Empty())
            {
                MessageBox.Show("没有可用的图像帧进行处理。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (isRecording && videoWriter != null && videoWriter.IsOpened())
            {
                for (int i = 0; i < frameRate / currentFrameRate; i++)
                {
                    videoWriter.Write(currentFrame);
                }
            }

            groupedFrames = new Dictionary<char, Mat>();

            ConvertFrameToGrayscale(ref currentFrame);
            // 执行背景减除（如果启用）
            if (isBackgroundSubtractionEnabled && backgroundImage != null)
            {
                ApplyBackgroundSubtractionToFrame(ref currentFrame); // 应用背景减除
            }
            currentFrame = ProcessImageWithCuda(currentFrame);

            // 获取裁剪和拼接的位置信息
            List<string> xCoords = columnArrays[0];
            List<string> yCoords = columnArrays[1];
            List<string> widths = columnArrays[2];
            List<string> heights = columnArrays[3];
            List<string> positions = columnArrays[4];

            int outputWidth = int.Parse(widths[0]) * 6;
            int outputHeight = int.Parse(heights[0]) * 6;

            for (int i = 0; i < xCoords.Count; i++)
            {
                int x = int.Parse(xCoords[i]);
                int y = int.Parse(yCoords[i]);
                int w = int.Parse(widths[i]); // 切割出来的单个图像宽度
                int h = int.Parse(heights[i]); // 切割出来的单个图像高度

                // 裁剪当前帧的指定区域
                Rect roi = new Rect(x, y, w, h);
                Mat croppedFrame = new Mat(currentFrame, roi);
                string position = positions[i];
                char groupLetter = position.Last();
                int gridPosition = int.Parse(position.Substring(0, position.Length - 1)); // 网格位置

                // 计算拼接的行和列位置
                int row = (gridPosition - 1) / 6; // 3x3网格
                int col = (gridPosition - 1) % 6;
                // 计算目标位置
                Rect gridRoi = new Rect(col * w, row * h, w, h);

                // 检查目标矩形是否超出拼接图像的范围
                if (!groupedFrames.ContainsKey(groupLetter))
                {
                    groupedFrames[groupLetter] = new Mat(outputHeight, outputWidth, MatType.CV_8UC1, Scalar.All(0)); // 初始化拼接图像
                }

                // 检查 gridRoi 的范围是否合法
                if (gridRoi.X + gridRoi.Width <= groupedFrames[groupLetter].Cols &&
                    gridRoi.Y + gridRoi.Height <= groupedFrames[groupLetter].Rows)
                {
                    // 将裁剪的图像复制到拼接图像中
                    croppedFrame.CopyTo(groupedFrames[groupLetter][gridRoi]);
                }
                else
                {
                    // 输出调试信息，说明目标区域超出范围
                    MessageBox.Show($"gridRoi 超出范围: {gridRoi}, 图片大小: {groupedFrames[groupLetter].Cols}x{groupedFrames[groupLetter].Rows}");
                }
            }

            ShowGroupFrames();
        }

        private void ShowGroupFrames()
        {
            PictureBox[] pictureBoxes = { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 };
            PictureBox[] smallPicBoxes = { smallShow1, smallShow2, smallShow3, smallShow4, smallShow5, smallShow6, smallShow7, smallShow8, smallShow9 };

            for (int i = 0; i < pictureBoxes.Length; i++)
            {
                int index = i;
                char groupLetter = (char)('A' + index);

                if (groupedFrames.ContainsKey(groupLetter))
                {
                    Mat histMat = groupedFrames[groupLetter];

                    float scale = pictureBoxScales[pictureBoxes[index]];
                    float widthRatio = (float)pictureBoxes[index].Width / groupedFrames[groupLetter].Width;
                    float heightRatio = (float)pictureBoxes[index].Height / groupedFrames[groupLetter].Height;
                    float baseScale = Math.Min(widthRatio, heightRatio);
                    float finalScale = baseScale * scale;

                    Mat resizedFrame = new Mat();
                    Cv2.Resize(groupedFrames[groupLetter], resizedFrame,
                       new OpenCvSharp.Size(groupedFrames[groupLetter].Width * finalScale,
                                            groupedFrames[groupLetter].Height * finalScale),
                       0, 0, InterpolationFlags.Linear);

                    System.Drawing.Point offset = pictureBoxOffsets[pictureBoxes[index]];
                    Bitmap canvas = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(resizedFrame);
                    Bitmap bitmap = new Bitmap(pictureBoxes[index].Width, pictureBoxes[index].Height);
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.Clear(System.Drawing.Color.Black);
                        g.DrawImage(canvas, offset);
                    }

                    Bitmap pseMap = pseudoColorEnabled.ContainsKey(index) && pseudoColorEnabled[index] && pseudoColorButtonColors.ContainsKey(index)
                        ? ApplyPseudoColor(bitmap, pseudoColorButtonColors[index])
                        : bitmap;

                    pictureBoxes[index].Image = pseMap;
                    pictureBoxes[index].Refresh();
                    smallPicBoxes[index].Image = bitmap;
                    smallPicBoxes[index].Refresh();

                    // 仅在 currentFullDisplayPictureBox 不为空时才更新 FullDisplay
                    if (currentFullDisplayPictureBox == pictureBoxes[index])
                    {
                        Bitmap fullDisplayBitmap = new Bitmap(FullDisPlay.Width, FullDisPlay.Height);
                        // 创建 pseMap 的副本
                        using (Graphics g = Graphics.FromImage(fullDisplayBitmap))
                        {
                            g.Clear(System.Drawing.Color.Black);
                            g.DrawImage(canvas, offset);
                        }
                        FullDisPlay.Image = fullDisplayBitmap;
                        FullDisPlay.Refresh();
                    }

                    string selectedStr = histSelect.SelectedItem?.ToString();
                    Match match = Regex.Match(selectedStr, @"\d+");
                    if (match.Success)
                    {
                        int number = int.Parse(match.Value) - 1;
                        if (index == number)
                        {
                            if (isVideoPlaying)
                                DisplayHistogram_Video(histMat);
                            else
                            {
                                DisplayHistogram_live(histMat);
                            }
                        }
                    }
                }
            }
        }

        private void RestoreBtn_Click(object sender, EventArgs e)
        {
            // 检查 FullDisplay 是否有内容显示
            if (FullDisPlay.Image != null)
            {
                // 重置 FullDisplay 的 SizeMode 和缩放、偏移
                FullDisPlay.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxScales[currentFullDisplayPictureBox] = 1.0f;  // 重置缩放比例
                pictureBoxOffsets[currentFullDisplayPictureBox] = System.Drawing.Point.Empty;  // 重置偏移
            }
            else
            {
                // 遍历 PicDisplay1-9 进行处理
                PictureBox[] pictureBoxes = { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 };

                foreach (var pictureBox in pictureBoxes)
                {
                    if (pictureBox.Image != null)  // 确保当前 PictureBox 有内容
                    {
                        // 设置铺满模式
                        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

                        // 重置缩放比例和偏移量
                        pictureBoxScales[pictureBox] = 1.0f;
                        pictureBoxOffsets[pictureBox] = System.Drawing.Point.Empty;
                    }
                }
            }
        }
        private void enlargeBtn_Click(object sender, EventArgs e)
        {
            float scaleFactor = 1.1f; // 放大因子
            ApplyScaleToCurrentDisplay(scaleFactor);
        }

        private void narrowBtn_Click(object sender, EventArgs e)
        {
            float scaleFactor = 0.9f; // 缩小因子
            ApplyScaleToCurrentDisplay(scaleFactor);
        }

        private void ApplyScaleToCurrentDisplay(float scaleFactor)
        {
            if (FullDisPlay.Image != null)
            {
                // FullDisplay 显示内容时放缩
                pictureBoxScales[currentFullDisplayPictureBox] *= scaleFactor;
            }
            else
            {
                // 全部 PicDisplay1-9 放缩
                PictureBox[] pictureBoxes = { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 };

                foreach (var pictureBox in pictureBoxes)
                {
                    if (pictureBox.Image != null) // 确保有内容
                    {
                        pictureBoxScales[pictureBox] *= scaleFactor;
                    }
                }
            }
        }

        // 辅助方法：禁用指定 GroupBox 中的所有控件
        private void DisableAllControls(GroupBox groupBox)
        {
            foreach (System.Windows.Forms.Control control in groupBox.Controls)
            {
                control.Enabled = false;
            }
        }

        // 辅助方法：启用指定 GroupBox 中的所有控件
        private void EnableAllControls(GroupBox groupBox)
        {
            foreach (System.Windows.Forms.Control control in groupBox.Controls)
            {
                control.Enabled = true;
            }
        }

        // ---------------- 设备读取播放  end------------

        // ---------------- 左侧窗体图片存储-----------
        private void exposureNum_ValueChanged(object sender, EventArgs e)
        {
            // 获取当前的曝光时间值
            double newExposureTime = (double)exposureNum.Value;
            activeCam.SetDCAMIDPROP(DCAMIDPROP.EXPOSURETIME, newExposureTime);

            activeCam.getFrameRate(ref currentFrameRate);
            bottomLabelMiddle.Text = $"currentFrameRate:{currentFrameRate}";
            Console.WriteLine($"currentFrameRate{currentFrameRate}");

        }
        // 定时器的 Tick 事件处理程序

        private void SaveMergedImage(string filePath)
        {
            // 假设每个 PictureBox 的宽度和高度相同
            int picWidth = PicDisplay1.Width;
            int picHeight = PicDisplay1.Height;

            // 创建一个 Bitmap，用于存储拼接后的图片 (3x3)
            Bitmap mergedImage = new Bitmap(picWidth * 3, picHeight * 3);

            using (Graphics g = Graphics.FromImage(mergedImage))
            {
                // 设置白色背景，避免没有图片时的空白
                g.Clear(System.Drawing.Color.White);

                // 定义 PictureBox 数组
                var pictureBoxes = new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 };

                // 遍历每个 PictureBox，按照 3x3 格式绘制到大图片上
                for (int i = 0; i < pictureBoxes.Length; i++)
                {
                    // 获取当前的 PictureBox
                    PictureBox currentPicBox = pictureBoxes[i];

                    // 计算图片在大图中的位置
                    int x = (i % 3) * picWidth;  // 列索引
                    int y = (i / 3) * picHeight; // 行索引

                    // 如果 PictureBox 中有图片，绘制图片内容
                    if (currentPicBox.Image != null)
                    {
                        g.DrawImage(currentPicBox.Image, new Rectangle(x, y, picWidth, picHeight));
                    }

                    // 通过偏移量在大图上重新绘制形状
                    DrawShapesWithOffset(currentPicBox, g, x, y);
                }
            }

            // 保存最终的大图片
            mergedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            Console.WriteLine($"拼接图片已保存，路径：{filePath}");
        }

        // 重新绘制形状，带有偏移量
        private void DrawShapesWithOffset(PictureBox pictureBox, Graphics g, int offsetX, int offsetY)
        {
            if (drawnShapes.ContainsKey(pictureBox))
            {
                foreach (var shapeInfo in drawnShapes[pictureBox])
                {
                    using (Pen pen = new Pen(shapeInfo.ShapeColor, 2))
                    {
                        // 根据偏移量绘制每个形状
                        switch (shapeInfo.ShapeType)
                        {
                            case DrawType.Point:
                                int pointSize = 5;
                                g.FillEllipse(new SolidBrush(shapeInfo.ShapeColor),
                                    shapeInfo.Location.X + offsetX - pointSize / 2,
                                    shapeInfo.Location.Y + offsetY - pointSize / 2,
                                    pointSize, pointSize);
                                break;
                            case DrawType.Rectangle:
                                g.DrawRectangle(pen,
                                    shapeInfo.Location.X + offsetX - shapeInfo.Width / 2,
                                    shapeInfo.Location.Y + offsetY - shapeInfo.Height / 2,
                                    shapeInfo.Width, shapeInfo.Height);
                                break;
                            case DrawType.Circle:
                                g.DrawEllipse(pen,
                                    shapeInfo.Location.X + offsetX - shapeInfo.Width / 2,
                                    shapeInfo.Location.Y + offsetY - shapeInfo.Height / 2,
                                    shapeInfo.Width, shapeInfo.Height);
                                break;
                            case DrawType.Line:
                                g.DrawLine(pen,
                                    shapeInfo.Location.X + offsetX, shapeInfo.Location.Y + offsetY,
                                    shapeInfo.Location.X + offsetX + shapeInfo.Width,
                                    shapeInfo.Location.Y + offsetY + shapeInfo.Height);
                                break;
                            case DrawType.SingleArrowLine:
                            case DrawType.ArrowLine:
                                // 对箭头线也应用偏移
                                DrawArrowLine(g, pen,
                                    new System.Drawing.Point(shapeInfo.Location.X + offsetX, shapeInfo.Location.Y + offsetY),
                                    new System.Drawing.Point(shapeInfo.Location.X + offsetX + shapeInfo.Width,
                                    shapeInfo.Location.Y + offsetY + shapeInfo.Height),
                                    shapeInfo.ShapeType == DrawType.ArrowLine);
                                break;
                            case DrawType.calLine:
                                g.DrawLine(pen,
                                    shapeInfo.Location.X + offsetX, shapeInfo.Location.Y + offsetY,
                                    shapeInfo.Location.X + offsetX + shapeInfo.Width,
                                    shapeInfo.Location.Y + offsetY + shapeInfo.Height);

                                // 在中点绘制线条长度
                                System.Drawing.Point middlePoint = new System.Drawing.Point(
                                    shapeInfo.Location.X + offsetX + shapeInfo.Width / 2,
                                    shapeInfo.Location.Y + offsetY + shapeInfo.Height / 2
                                );
                                string lengthText = $"{shapeInfo.Length:F2} px";
                                SizeF textSize = g.MeasureString(lengthText, SystemFonts.DefaultFont);
                                g.DrawString(lengthText, SystemFonts.DefaultFont, Brushes.Black,
                                    middlePoint.X - textSize.Width / 2, middlePoint.Y - textSize.Height / 2);
                                break;
                            case DrawType.Text:
                                // 绘制保存的文本内容
                                using (Brush textBrush = new SolidBrush(shapeInfo.ShapeColor))
                                {
                                    using (System.Drawing.Font textFont = new System.Drawing.Font("Microsoft YaHei", shapeInfo.FontSize))
                                    {
                                        g.DrawString(shapeInfo.TextContent, textFont, textBrush,
                                            shapeInfo.Location.X + offsetX, shapeInfo.Location.Y + offsetY);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (m_bitmap == null)
            //{
            //    MessageBox.Show("没有可保存的图像。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            if (string.IsNullOrEmpty(m_savePath))
            {
                MessageBox.Show("请先选择保存目录。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string prefix = preName.Text.Trim(); // 获取 preName TextBox 的内容
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = "image"; // 如果 preName 为空，使用默认前缀
            }

            imageCounter++; // 增加计数器
            string filename = Path.Combine(m_savePath, $"{prefix}_{imageCounter:D4}.png");

            try
            {
                SaveMergedImage(filename);
                MyShowStatus($"图像成功保存：{filename}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存图像时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void savePathChoose_Click(object sender, EventArgs e)
        {

            logger.LogButtonClick("savePathChoose_Click");
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "请选择一个文件夹";

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;
                    pathLabel.Text = selectedPath;
                    m_savePath = selectedPath;
                }
            }

        }

        private void saveImg_Click(object sender, EventArgs e)
        {
            SaveCurrentImage();
            //saveToolStripMenuItem_Click(null, null);
        }
        bool isShootStart = false;
        private async void shootStart_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("shootStart_Click");
            if (!isShootStart)
            {
                isShootStart = true;
                shootStart.Text = "Cancel";
                int delay = (int)delayVal.Value * 1000; // 将秒转换为毫秒
                int interval = (int)periodVal.Value * 1000; // 将秒转换为毫秒
                int totalShots = (int)numVal.Value;

                if (totalShots <= 0)
                {
                    MessageBox.Show("请设置有效的拍摄次数。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 禁用开始按钮，防止重复点击
                //shootStart.Enabled = false;

                // 创建取消令牌源
                cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    if (delay > 0)
                    {
                        MyShowStatus($"连拍将在 {delay / 1000} 秒后开始");
                        await Task.Delay(delay, cancellationTokenSource.Token);
                    }

                    for (int i = 0; i < totalShots; i++)
                    {
                        if (cancellationTokenSource.Token.IsCancellationRequested)
                            break;

                        SaveCurrentImage();
                        //saveToolStripMenuItem_Click(null, null);
                        //Console.WriteLine($"已拍摄 {i + 1}/{totalShots} 张");

                        if (i < totalShots - 1) // 不是最后一张时才等待
                        {
                            await Task.Delay(interval, cancellationTokenSource.Token);
                        }
                    }

                    MyShowStatus($"连拍完成！共拍摄 {totalShots} 张照片。");
                    shootStart.Text="Start";
                }
                catch (OperationCanceledException)
                {
                    MyShowStatus("连拍已取消。");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"连拍过程中发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            }
            else
            {
                isShootStart = false;

                shootStart.Text = "Start";
                cancellationTokenSource?.Cancel();
            }
        }

        // 添加一个取消连拍的方法
        private void shootCancel_Click(object sender, EventArgs e)
        {
        }

        //--------视频录制---------------
        private string recordedVideoPath; // 用于存储录制的视频路径

        private void StartRecording()
        {
            if (currentFrame == null)
            {
                MessageBox.Show("No image available for recording.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(m_savePath))
            {
                MessageBox.Show("Please select a save directory first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frameWidth = currentFrame.Width;
            frameHeight = currentFrame.Height;
            frameRate = currentFrameRate;
            if (frameRate <= 0)
            {
                frameRate = 30; // 设置一个默认值，比如30fps
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            recordedVideoPath = Path.Combine(m_savePath, $"video_{timestamp}.mp4");

            videoWriter = new VideoWriter(recordedVideoPath, FourCC.H264, frameRate, new OpenCvSharp.Size(frameWidth, frameHeight));

            if (!videoWriter.IsOpened())
            {
                MessageBox.Show("Failed to create video file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isRecording = true;
            videoRecorder.Text = "Stop Recording";
            MyShowStatus($"Started recording video: {recordedVideoPath}");
        }

        private void StopRecording()
        {
            if (videoWriter != null)
            {
                videoWriter.Release();
                videoWriter.Dispose();
                videoWriter = null;
            }

            isRecording = false;
            videoRecorder.Text = "Start Recording";
            MyShowStatus("Video recording stopped");

            // 在停止录制后对视频进行异或加密并保存为.ksjv
            SaveAsKsjvFile();
        }

        private void SaveAsKsjvFile()
        {
            if (string.IsNullOrEmpty(recordedVideoPath) || !File.Exists(recordedVideoPath))
            {
                MessageBox.Show("Recorded video file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 读取录制的视频文件的二进制数据
            byte[] videoData = File.ReadAllBytes(recordedVideoPath);

            // 执行异或计算
            byte key = 0x5A; // 可以根据需要调整异或密钥
            byte[] xorData = PerformXOR(videoData, key);

            // 保存为.ksjv文件
            string ksjvFilename = Path.ChangeExtension(recordedVideoPath, ".ksjv");
            File.WriteAllBytes(ksjvFilename, xorData);
            File.Delete(recordedVideoPath);
            MessageBox.Show($"Video saved as: {ksjvFilename}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void videoRecorder_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("videoRecorder_Click");
            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        private void StopVideoDisplay()
        {
            //m_cap_stopping = true;
            //if (mydcam != null)
            //{
            //    mydcam.cap_stop();
            //}
            //MyThreadCapture_Abort();

            //if (mydcam != null)
            //{
            //    mydcam.buf_release();
            //}

            //MyFormStatus_Opened();
            MyShowStatus("视频显示已停止");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("exitToolStripMenuItem_Click");
            if (liveStatus)
                pushLive_Click(sender, e);
            if (activeCam != null)
                activeCam.dev_exit();
            // 禁用两个控件组
            DisableAllControls(funGroup);
            DisableAllControls(saveGroup);

            MyShowStatus("已退出相机操作");
        }

        // ---------------- 左侧窗体图片存储 end-----------

        // ----------------主体屏幕处理功能----------
        // 处理屏幕点击事件
        private void PicDisplay_MouseEnter(object sender, EventArgs e)
        {
            PictureBox picDisplay = sender as PictureBox;

            if (picDisplay != null)
            {
                // 设置重绘标记
                picDisplay.Tag = true;
                picDisplay.Invalidate(); // 触发重绘
            }
        }

        private void PicDisplay_MouseLeave(object sender, EventArgs e)
        {
            PictureBox picDisplay = sender as PictureBox;

            if (picDisplay != null)
            {
                // 重置重绘标记
                picDisplay.Tag = false;
                picDisplay.Invalidate(); // 触发重绘
            }
        }

        private void PicDisplay_Paint(object sender, PaintEventArgs e)
        {
            PictureBox picDisplay = sender as PictureBox;

            if (picDisplay != null && picDisplay.Tag is bool isHovered && isHovered)
            {
                // 绘制外边框，颜色为红色
                using (Pen pen = new Pen(System.Drawing.Color.Yellow, 3)) // 边框颜色和宽度
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, picDisplay.Width - 1, picDisplay.Height - 1);
                }
            }
        }

        private void smallPic_Click(object sender, EventArgs e)
        {
            PictureBox clickedPictureBox = sender as PictureBox;

            if (clickedPictureBox != null)
            {
                // 提取 Name 中的数字部分
                string smallNumber = Regex.Match(clickedPictureBox.Name, @"\d+").Value;
                // 根据提取到的数字生成对应的 PicDisplay 控件名称
                string picDisplayName = "PicDisplay" + smallNumber;

                // 查找对应的 PicDisplay 控件
                System.Windows.Forms.Control foundControl = this.Controls.Find(picDisplayName, true).FirstOrDefault();

                if (foundControl != null && foundControl is PictureBox)
                {
                    PictureBox picShow = foundControl as PictureBox;

                    // 更新 FullDisplay 的图像
                    FullDisPlay.Image = picShow.Image;
                    FullDisPlay.SizeMode = PictureBoxSizeMode.StretchImage;

                    // 将 FullDisplay 置于顶层
                    FullDisPlay.BringToFront();

                    // 更新 currentFullDisplayPictureBox 为 PicDisplay
                    currentFullDisplayPictureBox = picShow;

                    // 更新 FullDisplay 的缩放和拖动事件
                    FullDisPlay.MouseWheel += FullDisPlay_MouseWheel;
                    FullDisPlay.MouseDown += FullDisPlay_MouseDown;
                    FullDisPlay.MouseMove += FullDisPlay_MouseMove;
                    FullDisPlay.MouseUp += FullDisPlay_MouseUp;

                    // 查找并更新对应的按钮颜色
                    string btnName = "displayBtn" + smallNumber;
                    TriangleButton btn = this.Controls.Find(btnName, true).FirstOrDefault() as TriangleButton;
                    if (btn != null)
                    {
                        System.Drawing.Color btnColor = btn.BackColor;
                        Console.WriteLine($"btnColor{btnColor}");
                        fullPicBtn.BackColor = btnColor;
                        fullPicBtn.BringToFront();
                    }
                    else
                    {
                        MessageBox.Show("未找到按钮: " + btnName);
                    }
                }
                else
                {
                    MessageBox.Show("找不到对应的 PicDisplay 控件");
                }
            }
        }
        private void ChangeButtonColor(object sender, MouseEventArgs e)
        {
            // 判断是否为左键点击
            if (e.Button == MouseButtons.Left)
            {
                if (sender is TriangleButton button)
                {
                    dynamic btn = button;
                    System.Drawing.Color buttonColor = btn.BackColor;
                    Console.WriteLine($"buttonColor:{buttonColor}");
                    // 获取 RGB 值
                    //int red = buttonColor.R;
                    //int green = buttonColor.G;
                    //int blue = buttonColor.B;

                    // 提取 Name 中的数字部分
                    string number = System.Text.RegularExpressions.Regex.Match(btn.Name, @"\d+").Value;
                    int numberInt = int.Parse(number) - 1; // 按钮数字
                    // 拼接成对应的值，例如 "disPlay" + 数字
                    String showName = "PicDisplay" + number;

                    //切换伪彩色状态
                    if (!pseudoColorEnabled.ContainsKey(numberInt))
                    {
                        pseudoColorEnabled[numberInt] = false; // 初始化为未启用伪彩
                    }
                    pseudoColorEnabled[numberInt] = !pseudoColorEnabled[numberInt]; // 切换伪彩色状态

                    // 如果启用伪彩色，记录按钮颜色
                    if (pseudoColorEnabled[numberInt])
                    {
                        pseudoColorButtonColors[numberInt] = buttonColor; // 记录启用伪彩的颜色
                    }
                    else
                    {
                        pseudoColorButtonColors.Remove(numberInt); // 取消伪彩时移除记录
                    }
                    string status = pseudoColorEnabled[numberInt] ? "启用伪彩" : "取消伪彩";

                    MyShowStatus(status);
                }
            }
            // 判断是否为右键点击
            else if (e.Button == MouseButtons.Right)
            {
                using (ColorDialog colorDialog = new ColorDialog())
                {
                    // 显示颜色对话框，并判断用户是否选择了颜色
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        // 获取选中的颜色
                        System.Drawing.Color selectedColor = colorDialog.Color;

                        // 确认 sender 是按钮
                        if (sender is TriangleButton button)
                        {
                            // 假设按钮有自定义属性 SymbolColor 和 SymbolPressColor
                            dynamic btn = button;
                            btn.BackColor = selectedColor;
                        }
                    }
                }
            }
        }

        private void recoveryBtn_Click(object sender, EventArgs e)
        {
            // 清除 fullDisplay 的显示内容
            FullDisPlay.Image = null;

            // 取消 FullDisplay 的所有事件绑定，防止缩放、拖动等操作
            FullDisPlay.MouseWheel -= FullDisPlay_MouseWheel;
            FullDisPlay.MouseDown -= FullDisPlay_MouseDown;
            FullDisPlay.MouseMove -= FullDisPlay_MouseMove;
            FullDisPlay.MouseUp -= FullDisPlay_MouseUp;

            // 将 fullDisplay 置于底层
            FullDisPlay.SendToBack();
            fullPicBtn.SendToBack();

            currentFullDisplayPictureBox = null;  // 清除 FullDisplay 的显示关联

        }
        // ----------------主体屏幕处理功能 end----------
        //-----------------右侧屏幕处理功能--------------
        // 通用的拖动处理方法
        private void EnablePanelDragging(System.Windows.Forms.Control panel)
        {
            panel.MouseDown += (sender, e) =>
            {
                isSlideDragging = true;
                dragCursorPoint = Cursor.Position;
                dragControlPoint = panel.Location;
            };

            panel.MouseMove += (sender, e) =>
            {
                if (isSlideDragging)
                {
                    System.Drawing.Point diff = System.Drawing.Point.Subtract(Cursor.Position, new System.Drawing.Size(dragCursorPoint));
                    System.Drawing.Point newLocation = System.Drawing.Point.Add(dragControlPoint, new System.Drawing.Size(diff));

                    // 限制控件只能在 rightPanel 内移动
                    newLocation.X = Math.Max(0, Math.Min(newLocation.X, rightPanel.Width - panel.Width));
                    newLocation.Y = Math.Max(0, Math.Min(newLocation.Y, rightPanel.Height - panel.Height));

                    // 设置控件的位置
                    panel.Location = new System.Drawing.Point(newLocation.X, newLocation.Y);
                }
            };

            panel.MouseUp += (sender, e) =>
            {
                isSlideDragging = false;  // 停止拖动
            };
        }
        private void InitializeZoomAndDragEvents()
        {
            pictureBoxScales = new Dictionary<PictureBox, float>();
            pictureBoxOffsets = new Dictionary<PictureBox, System.Drawing.Point>();

            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                pictureBox.MouseEnter += PictureBox_MouseEnter;
                pictureBox.MouseWheel += PictureBox_MouseWheel;
                pictureBox.MouseDown += PictureBox_MouseDown;
                pictureBox.MouseMove += PictureBox_MouseMoveShow;
                pictureBox.MouseUp += PictureBox_MouseUpShow;
                pictureBoxScales[pictureBox] = 1.0f; // 初始缩放系数为1
                pictureBoxOffsets[pictureBox] = System.Drawing.Point.Empty; // 初始偏移量为0
            }
        }
        private void FullDisPlay_MouseWheel(object sender, MouseEventArgs e)
        {
            if (currentFullDisplayPictureBox != null)
            {
                float scaleFactor = e.Delta > 0 ? 1.1f : 0.9f;
                pictureBoxScales[currentFullDisplayPictureBox] *= scaleFactor; // 更新缩放系数

            }
        }
        private void FullDisPlay_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDraggingFullDisplayImage = true;  // 启用拖动
                fullDisplayDragStartPoint = e.Location;  // 记录鼠标按下的位置
            }
        }

        private void FullDisPlay_MouseMove(object sender, MouseEventArgs e)
        {

            if (isDraggingFullDisplayImage)  // 仅在拖动启用时更新位置
            {
                int offsetX = e.Location.X - fullDisplayDragStartPoint.X;
                int offsetY = e.Location.Y - fullDisplayDragStartPoint.Y;

                // 更新 FullDisplay 的偏移量
                if (currentFullDisplayPictureBox != null)
                {
                    pictureBoxOffsets[currentFullDisplayPictureBox] = new System.Drawing.Point(
                        pictureBoxOffsets[currentFullDisplayPictureBox].X + offsetX,
                        pictureBoxOffsets[currentFullDisplayPictureBox].Y + offsetY
                    );
                }

                fullDisplayDragStartPoint = e.Location;  // 更新起始点
                FullDisPlay.Refresh();  // 刷新显示
            }
        }

        private void FullDisPlay_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDraggingFullDisplayImage = false;  // 停止拖动
            }
        }


        private void PictureBox_MouseEnter(object sender, EventArgs e)
        {
            currentHoverPictureBox = sender as PictureBox;
        }
        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (currentHoverPictureBox != null)
            {
                float scaleFactor = e.Delta > 0 ? 1.1f : 0.9f;
                pictureBoxScales[currentHoverPictureBox] *= scaleFactor; // 更新缩放系数
            }
        }
        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDraggingImage = true;
                dragStartPoint = e.Location;
            }
        }

        private void PictureBox_MouseMoveShow(object sender, MouseEventArgs e)
        {
            if (isDraggingImage && currentHoverPictureBox != null)
            {
                // 计算拖动的偏移量
                int offsetX = e.Location.X - dragStartPoint.X;
                int offsetY = e.Location.Y - dragStartPoint.Y;

                // 更新当前图片的偏移量
                pictureBoxOffsets[currentHoverPictureBox] = new System.Drawing.Point(
                    pictureBoxOffsets[currentHoverPictureBox].X + offsetX,
                    pictureBoxOffsets[currentHoverPictureBox].Y + offsetY
                );

                // 更新拖动起始点
                dragStartPoint = e.Location;
            }
        }

        private void PictureBox_MouseUpShow(object sender, MouseEventArgs e)
        {
            isDraggingImage = false;
        }
        private void InitializeHistLine()
        {
            // 创建默认的 Option 并设置一些默认值
            UIBarOption defaultOption = new UIBarOption();

            defaultOption.Title = new UITitle();
            defaultOption.Title.Text = "默认灰度直方图";

            var defaultSeries = new UIBarSeries();
            defaultSeries.Name = "GrayScale";

            // 设置默认的 X 轴和 Y 轴数据
            int dataCount = 256; // 假设有 256 个灰度级别的数据
            for (int i = 0; i < dataCount; i++)
            {
                // 这里默认给 series 添加初始数据
                defaultSeries.AddData(0); // 假设默认每个灰度级别的值为 0

                if (i % 50 == 0)
                {
                    defaultOption.XAxis.Data.Add(i.ToString());
                }
                else
                {
                    defaultOption.XAxis.Data.Add(""); // 其他位置留空，不显示标签
                }
            }

            // 设置 Y 轴默认值
            defaultOption.YAxisScaleLines.Add(new UIScaleLine() { Color = System.Drawing.Color.Red, Name = "上限", Value = 100 });

            // 将默认系列添加到 Option 中
            defaultOption.Series.Add(defaultSeries);

            // 为 histLine 设置默认 Option
            histLine.SetOption(defaultOption);
        }

        private void DisplayHistogram_Video(Mat image)
        {
            // 将 Bitmap 转换为 OpenCV 的 Mat
            //Mat mat = BitmapConverter.ToMat(image);
            // 转换为灰度图
            if (image.Channels() == 3)
            {
                Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
            }

            // 计算灰度直方图
            Mat hist = new Mat();
            Cv2.CalcHist(new[] { image }, new[] { 0 }, null, hist, 1, new[] { 256 }, new[] { new Rangef(0, 256) });

            // 获取直方图中的最大值
            double minVal, maxVal;
            Cv2.MinMaxLoc(hist, out minVal, out maxVal);

            // 将最大值乘以 1.2 来确定 Y 轴的最大值
            double yAxisMaxValue = maxVal * 1.2;

            // 创建 UIBarOption 以显示直方图
            UIBarOption option = new UIBarOption();
            option.Title = new UITitle();
            option.Title.Text = "灰度直方图";

            var series = new UIBarSeries();
            series.Name = "GrayScale";

            // 将灰度直方图数据添加到 UIBarSeries 中
            for (int i = 0; i < hist.Rows; i++)
            {
                series.AddData(hist.At<float>(i));
            }

            // 添加系列到 Option
            option.Series.Add(series);

            // 设置 X 轴数据，每隔 50 显示一次
            for (int i = 0; i < 256; i++)
            {
                if (i % 50 == 0)
                {
                    option.XAxis.Data.Add(i.ToString());
                }
                else
                {
                    option.XAxis.Data.Add(""); // 其他位置留空，不显示标签
                }
            }

            option.YAxisScaleLines.Add(new UIScaleLine() { Color = System.Drawing.Color.Red, Name = "上限", Value = yAxisMaxValue });

            // 更新 histLine
            histLine.SetOption(option);
        }

        private int GetBitDepthFromComboBox()
        {
            string selectedBitDepth = pixelCombox.SelectedItem?.ToString();
            return selectedBitDepth == "16" ? 16 : (selectedBitDepth == "12" ? 12 : 8);  // 默认处理8位图像
        }
        private void ComputeHistogram(Mat image, out float[] histogram, int bitDepth)
        {
            int binCount;
            int binFactor;

            // 根据 bitDepth 决定 binCount 和 binFactor
            switch (bitDepth)
            {
                case 8:
                    binCount = 256; // 8 位图像使用 256 个 bin
                    binFactor = 1;  // 无需缩放
                    break;
                case 12:
                    binCount = 256; // 12 位图像缩放到 256 个 bin
                    binFactor = 4096 / binCount;  // 将 4096 个值缩放到 256 个 bin
                    break;
                case 16:
                    binCount = 256; // 16 位图像缩放到 256 个 bin
                    binFactor = 65536 / binCount; // 将 65536 个值缩放到 256 个 bin
                    break;
                default:
                    throw new ArgumentException("Unsupported bit depth");
            }

            histogram = new float[binCount];  // 初始化直方图数组

            int width = image.Width;
            int height = image.Height;

            // 创建多个局部直方图
            int numThreads = Environment.ProcessorCount;
            float[][] localHistograms = new float[numThreads][];
            for (int i = 0; i < numThreads; i++)
            {
                localHistograms[i] = new float[binCount];
            }

            // 并行计算直方图
            Parallel.For(0, height, y =>
            {
                int threadIndex = Thread.CurrentThread.ManagedThreadId % numThreads;

                for (int x = 0; x < width; x++)
                {
                    int pixelValue;
                    if (bitDepth == 8)
                    {
                        pixelValue = image.At<byte>(y, x);  // 对于8位图像
                    }
                    else
                    {
                        pixelValue = image.At<ushort>(y, x);  // 对于12或16位图像
                        pixelValue /= binFactor;  // 将像素值按 binFactor 缩小到合适的范围
                    }

                    // 确保像素值在合理范围内
                    if (pixelValue >= 0 && pixelValue < binCount)
                    {
                        localHistograms[threadIndex][pixelValue]++;
                    }
                }
            });

            // 合并局部直方图
            for (int i = 0; i < numThreads; i++)
            {
                for (int j = 0; j < binCount; j++)
                {
                    histogram[j] += localHistograms[i][j];
                }
            }
        }

        private void ShowHistogram(float[] histogram, int binCount, double yAxisMaxValue, int bitDepth)
        {
            // 创建显示的直方图
            UIBarOption option = new UIBarOption();
            option.Title = new UITitle { Text = "灰度直方图" };

            var series = new UIBarSeries { Name = "GrayScale" };

            // 遍历 histogram 数组，确保 binCount 不超出 histogram 的长度
            for (int i = 0; i < binCount && i < histogram.Length; i++)
            {
                series.AddData(histogram[i]);
            }

            option.Series.Add(series);

            int step = 50;
            int xMappingFactor = (bitDepth == 16) ? 256 : 1;

            // 设置 X 轴数据
            for (int i = 0; i < binCount; i++)
            {
                if (i % step == 0)
                {
                    option.XAxis.Data.Add((i * xMappingFactor).ToString());
                }
                else
                {
                    option.XAxis.Data.Add(""); // 其他位置留空，不显示标签
                }
            }

            // 设置 Y 轴最大值
            option.YAxisScaleLines.Add(new UIScaleLine { Color = System.Drawing.Color.Red, Name = "上限", Value = yAxisMaxValue });

            // 更新直方图显示
            histLine.SetOption(option);
        }

        private void DisplayHistogram_live(Mat image)
        {
            // 确保图像是单通道灰度图
            if (image.Channels() == 3)
            {
                Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
            }

            int bitDepth = GetBitDepthFromComboBox();  // 获取当前位深度（8, 12, 16）

            // 计算直方图
            ComputeHistogram(image, out float[] histogram, bitDepth);

            // 获取直方图中的最大值
            double yAxisMaxValue = histogram.Max() * 1.2;

            // 显示直方图
            ShowHistogram(histogram, histogram.Length, yAxisMaxValue, bitDepth);
        }

        private void StretchHistogram(float[] histogram, int bitDepth)
        {
            // 找到直方图的最小值和最大值
            float minValue = histogram.Min();
            float maxValue = histogram.Max();

            // 拉伸直方图到 [0, 255] 范围
            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] = (histogram[i] - minValue) / (maxValue - minValue) * 255;
            }
        }



        // 当 rightPanel 大小变化时自动调整两个面板的宽度
        private void RightPanel_Resize(object sender, EventArgs e)
        {
            // 设置 histogramPanel 的宽度与 rightPanel 的宽度一致
            histogramPanel.Width = rightPanel.Width;

            // 根据宽高比设置 histogramPanel 的高度
            histogramPanel.Height = (int)(histogramPanel.Width * histogramAspectRatio);

            // 设置 histLine 的宽度与 histogramPanel 一致

            //histLine.Height = (int)(histogramPanel.Width * histogramAspectRatio);
            histLine.Width = (int)(histogramPanel.Width * 1.1);
            histLine.Left = histogramPanel.Left - (int)(histogramPanel.Width * 0.05);
            histLine.Top = histSelect.Location.Y + (int)(histSelect.Height * 1.1);



            // 根据相同宽高比设置 histLine 的高度
            histLine.Height = (int)(histLine.Width * histLineRatio);

        }

        private void MiddlePanel_Resize(object sender, EventArgs e)
        {
            // 使用 middlePanel 的宽度和高度的最小值，计算 FullDisplay 的大小
            int fullDisplayWidth = (int)(Math.Min(middlePanel.Width, middlePanel.Height - RestoreBtn.Height) * 0.98);
            FullDisPlay.Width = fullDisplayWidth;
            FullDisPlay.Height = fullDisplayWidth; // 保持正方形

            // 设置 FullDisplay 的位置，使其居中于 middlePanel
            FullDisPlay.Left = (middlePanel.Width - FullDisPlay.Width) / 2;
            FullDisPlay.Top = ((middlePanel.Height + RestoreBtn.Height - FullDisPlay.Height) / 2);

            // 计算每个 PicDisplay 的大小
            int picDisplaySize = (int)(fullDisplayWidth * 0.33); // 每个 PicDisplay 的宽度和高度
            int picSpacing = (int)(0.005 * fullDisplayWidth); // 设置 PicDisplay 的间距


            // 确保 fullPicBtn 已经实例化
            //if (fullPicBtn != null)
            //{
            //    //// 设置 fullPicBtn 的大小和位置，使其位于 FullDisplay 左上角，且边距为 5%
            //    //int fullBtnMargin = (int)(picDisplaySize * 0.05); // 5% 边距
            //    //fullPicBtn.Width = (int)(picDisplaySize * 0.2); // 按钮宽度为 FullDisplay 宽度的 20%
            //    //fullPicBtn.Height = (int)(picDisplaySize * 0.2); // 按钮高度为 FullDisplay 高度的 20%
            //    //fullPicBtn.Left = FullDisPlay.Left;
            //    //fullPicBtn.Top = FullDisPlay.Top;
            //}
            //else
            //{
            //    // 输出调试信息，方便确认问题
            //    Console.WriteLine("fullPicBtn 为空，未正确初始化。");
            //}
            //布局 PicDisplay1-9（九宫格），使其在 FullDisplay 内部居中排列
            for (int i = 0; i < 9; i++)
            {
                var picDisplay = this.Controls.Find($"PicDisplay{i + 1}", true).FirstOrDefault() as PictureBox;
                //var displayBtn = this.Controls.Find($"displayBtn{i + 1}", true).FirstOrDefault() as TriangleButton;

                if (picDisplay != null)
                {
                    // 设置每个 PicDisplay 的大小
                    picDisplay.Width = picDisplaySize;
                    picDisplay.Height = picDisplaySize;

                    // 计算行和列
                    int row = i / 3;
                    int col = i % 3;

                    // 设置 PicDisplay 的位置，使其在 FullDisplay 内部九宫格居中排列
                    picDisplay.Left = FullDisPlay.Left + col * (picDisplaySize + picSpacing);
                    picDisplay.Top = FullDisPlay.Top + row * (picDisplaySize + picSpacing);

                    ////// 设置 displayBtn 的大小和位置，使其在每个 PicDisplay 内部居中
                    //int btnMargin = (int)(picDisplaySize * 0.03); // 5% 边距
                    //displayBtn.Width = (int)(picDisplaySize * 0.15);
                    //displayBtn.Height = (int)(picDisplaySize * 0.15);
                    //displayBtn.Left = picDisplay.Left + btnMargin;
                    //displayBtn.Top = picDisplay.Top + btnMargin;
                }
            }

            LiveBar.Top = FullDisPlay.Top;
            LiveBar.Left = FullDisPlay.Right;
            LiveBar.Height = FullDisPlay.Height;
        }


        //-----------------右侧屏幕处理 end--------------
        //————————状态栏功能——————

        private float lutMin;
        private float lutMax;
        private double BitDepth;
        private void searchCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("searchCamToolStripMenuItem_Click");

            LiveBar.Visible = false;
            // dcamapi_init() may takes for a few seconds
            Cursor.Current = Cursors.WaitCursor;
            //创建一个相机（编号为0，也就是DCAMAPI初始化时找到的第一台相机，经常也是唯一一台相机）的实例
            activeCam = new HisCamera(0);

            Console.WriteLine(activeCam);
            activeCam.SetDCAMIDPROP(DCAMIDPROP.SHADINGCALIB_STABLEMININTENSITY, 4096);
            activeCam.SetDCAMIDPROP(DCAMIDPROP.EXPOSURETIME, 0.001);
            activeCam.SetDCAMIDPROP(DCAMIDPROP.INTERNALLINERATE, 3000);

            //修改PIXELTYPE以及Bitdepth
            activeCam.SetDCAMIDPROP(DCAMIDPROP.IMAGE_PIXELTYPE, (int)DCAM_PIXELTYPE.MONO8);
            activeCam.setBitDepth(8);
            pixelCombox.SelectedIndex = 0;

            MyShowStatus(activeCam.GetModelSN(ref activeCamModelSN));
            devName.Items.Clear();
            devName.Items.Add(activeCamModelSN);
            devName.SelectedIndex = 0;  // 选择第一项


            // 设置曝光时间默认值
            double time = 0.001;
            //activeCam.GetExposureTime(ref time);
            activeCam.GetDCAMIDPROP(DCAMIDPROP.EXPOSURETIME, ref time);
            exposureNum.Value = Convert.ToDecimal(time);

            activeCam.getFrameRate(ref currentFrameRate);
            bottomLabelMiddle.Text = $"currentFrameRate:{currentFrameRate}";

            double lineBundleHeight = 1;
            activeCam.GetDCAMIDPROP(DCAMIDPROP.SENSORMODE_LINEBUNDLEHEIGHT, ref lineBundleHeight);


            double lineRate = 1000;
            activeCam.GetDCAMIDPROP(DCAMIDPROP.INTERNALLINERATE, ref lineRate);


            activeCam.SetDCAMIDPROP(DCAMIDPROP.SHADINGCALIB_STABLESAMPLES, 0);

            MyShowStatus(activeCam.GetDCAMIDPROP(DCAMIDPROP.BITSPERCHANNEL, ref BitDepth));
            lutMin = 0;
            lutMax = (float)(Math.Pow(2, BitDepth) - 1);    //例如，相机是16bit的，那么灰度值最大就是 2^16 - 1 = 65535
            activeCam.GetDCAMIDPROP(DCAMIDPROP.IMAGE_WIDTH, ref shownPicWidth);
            activeCam.GetDCAMIDPROP(DCAMIDPROP.IMAGE_HEIGHT, ref shownPicHeight);
            shownPic = new Bitmap((int)shownPicWidth, (int)shownPicHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            EnableAllControls(funGroup);
            Cursor.Current = Cursors.Default;
        }
        // 应用背景减除到当前帧
        private void ApplyBackgroundSubtractionToFrame(ref Mat currentFrame)
        {
            // 将 backgroundImage 转换为 Mat
            Mat backgroundMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(backgroundImage);

            if (backgroundMat.Size() == currentFrame.Size())
            {
                // 检测并处理 backgroundMat 是否为三通道
                if (backgroundMat.Channels() == 3)
                {
                    Console.WriteLine("Background image is 3 channels (color). Converting to single channel (grayscale).");
                    Cv2.CvtColor(backgroundMat, backgroundMat, ColorConversionCodes.BGR2GRAY); // 转换为灰度图像
                }
                else
                {
                    Console.WriteLine("Background image is already a single channel (grayscale).");
                }
                Cv2.Absdiff(currentFrame, backgroundMat, currentFrame); // 计算当前帧与背景图像的差值
                MyShowStatus("背景减除已应用于当前帧");
            }
            else
            {
                MessageBox.Show("背景图像尺寸与当前帧不匹配，无法应用背景减除。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap ApplyPseudoColor(Bitmap grayImage, System.Drawing.Color color)
        {
            // 创建一个与灰度图像相同大小的彩色图像
            Bitmap pseudoColorImage = new Bitmap(grayImage.Width, grayImage.Height);
            for (int x = 0; x < grayImage.Width; x++)
            {
                for (int y = 0; y < grayImage.Height; y++)
                {
                    // 获取灰度值
                    System.Drawing.Color grayPixel = grayImage.GetPixel(x, y);
                    int grayValue = grayPixel.R;

                    // 根据灰度值和按钮颜色的 RGB 通道进行伪彩色映射
                    int red = (int)(grayValue * (color.R / 255.0));
                    int green = (int)(grayValue * (color.G / 255.0));
                    int blue = (int)(grayValue * (color.B / 255.0));

                    // 将映射后的颜色应用到伪彩色图像中
                    pseudoColorImage.SetPixel(x, y, System.Drawing.Color.FromArgb(red, green, blue));
                }
            }

            return pseudoColorImage;
        }

        private void backgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 如果 currentFrame 不为空，表示当前有图像可以用作背景
            if (originalFrame != null && !originalFrame.Empty())
            {
                // 将 currentFrame 转换为 Bitmap 并保存为背景
                backgroundImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(originalFrame);
                MyShowStatus("背景图像已记录");
            }
            //else if (m_bitmap != null)  // 如果 currentFrame 为空但摄像头的 m_bitmap 可用
            //{
            //    lock (BitmapLock)
            //    {
            //        // 创建摄像头捕获图像的副本
            //        backgroundImage = new Bitmap(m_bitmap);
            //    }
            //    MyShowStatus("摄像头背景图像已记录");
            //}
            else
            {
                MessageBox.Show("没有可用的图像来记录背景。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadIToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // 打开文件对话框以选择图片
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.ksjp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 读取选中的文件的字节数据
                byte[] encryptedData = File.ReadAllBytes(openFileDialog.FileName);

                // 使用 PerformXOR 函数进行解密
                byte[] decryptedData = PerformXOR(encryptedData, 0x5A);

                // 将解密后的数据转换为 MemoryStream
                using (MemoryStream ms = new MemoryStream(decryptedData))
                {
                    // 将 MemoryStream 转换为 Bitmap
                    backgroundImage = new Bitmap(ms);
                }
            }
        }
        private void removeBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 检查是否有背景图像已经记录
            if (backgroundImage == null)
            {
                MessageBox.Show("请先记录背景图像。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 启用或禁用背景减除功能
            isBackgroundSubtractionEnabled = !isBackgroundSubtractionEnabled;
            removeBackgroundToolStripMenuItem.Checked = isBackgroundSubtractionEnabled;

            if (isBackgroundSubtractionEnabled)
            {
                MyShowStatus("背景减除已启用");
            }
            else
            {
                MyShowStatus("背景减除已禁用");
            }
        }

        // 声明全局变量来存储模型
        private static dynamic channelDealModel;
        private static dynamic picDefiniModel1;
        private static dynamic picDefiniModel2;
        private static void LoadModels()
        {
            using (Py.GIL())
            {

                // 加载 channelDeal 模型
                dynamic channelDealModule = Py.Import("lineDraw.channelDeal");
                channelDealModel = channelDealModule.load_model();
                Console.WriteLine("channelDeal 模型加载成功");

                // 加载 picDefini 模型
                dynamic picDefiniModule = Py.Import("enhance.picDefini");
                dynamic result = picDefiniModule.load_model();
                picDefiniModel1 = result[0];
                picDefiniModel2 = result[1];
                Console.WriteLine("picDefini 模型加载成功");
            }
        }

        private void imageProcessingcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("imageProcessingcToolStripMenuItem_Click");

            if (!isVideoPlaying)
            {
                // 设置 liveStatus 为 false
                liveStatus = false;
            }
            else
            {
                stopPlayingToolStripMenuItem_Click(null, null);
            }

            dataShow newForm = new dataShow();
            newForm.Show();

            // 绑定 dataShow 的 FormClosing 事件，当窗口关闭时将 liveStatus 设为 true
            newForm.FormClosing += (s, args) =>
            {
                if (!isVideoPlaying)
                {
                    pushLive_Click(null, null);
                }
                else
                {
                    ProcessSelectFile(selectedFilePath);
                }
            };

            // 创建一个计时器来更新UI
            System.Windows.Forms.Timer uiTimer = new System.Windows.Forms.Timer();
            uiTimer.Interval = 100; // 每100毫秒更新一次UI
            uiTimer.Tick += (s, args) => Application.DoEvents();
            uiTimer.Start();

            try
            {
                using (Py.GIL())
                {
                    // 检查模型是否已加载
                    if (channelDealModel == null)
                    {
                        MessageBox.Show("模型未加载，请先加载模型。");
                        return;
                    }

                    // 将 currentFrame 转换为字节数组
                    byte[] imageData = currentFrame.ToBytes();

                    // 导入 channelDeal 模块
                    dynamic mainModule = Py.Import("lineDraw.channelDeal");

                    // 调用 chanDeal 函数，传入 imageData 和模型
                    dynamic dataRes = mainModule.chanDeal(imageData, channelDealModel);
                    dynamic result = dataRes[0];
                    string tiffPath = dataRes[1];
                    // 停止计时器
                    uiTimer.Stop();

                    // 显示结果
                    newForm.DisplayResult(result.ToString(), tiffPath);
                }
            }
            catch (Exception ex)
            {
                // 停止计时器
                uiTimer.Stop();

                MessageBox.Show($"Error: {ex.Message}");

            }
            finally
            {
                // 确保计时器被停止和释放
                uiTimer.Stop();
                uiTimer.Dispose();
            }
        }

        private void imageEnhanceProcess()
        {
            if (liveStatus)
                pushLive_Click(null, null);
            // 创建一个计时器来更新UI
            System.Windows.Forms.Timer uiTimer = new System.Windows.Forms.Timer();
            uiTimer.Interval = 100; // 每100毫秒更新一次UI
            uiTimer.Tick += (s, args) => Application.DoEvents();
            uiTimer.Start();

            try
            {
                // 检查模型是否已加载
                if (picDefiniModel1 == null || picDefiniModel2 == null)
                {
                    MessageBox.Show("模型未加载，请先加载模型。");
                    return;
                }

                // 获取 PictureBox 中的图像并转换为 PyObject 列表
                List<PyObject> pyImageList = GetImagesFromPictureBoxes();

                using (Py.GIL())
                {
                    // 导入 picDefini 模块
                    dynamic picDefiniModule = Py.Import("enhance.picDefini");

                    // 创建 PyList
                    dynamic pyList = new PyList();
                    foreach (PyObject img in pyImageList)
                    {
                        pyList.Append(img);
                    }

                    // 调用 inference 函数，获取返回的图片列表
                    String result = picDefiniModule.inference(picDefiniModel1, picDefiniModel2, pyList);


                    // 停止计时器
                    uiTimer.Stop();

                    // 将结果显示在 PicDisplay1 - PicDisplay9 中
                    ShowResultImages(result);
                }
            }
            catch (Exception ex)
            {
                // 停止计时器
                uiTimer.Stop();

                // 显示错误信息
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                // 确保计时器被停止和释放
                uiTimer.Stop();
                uiTimer.Dispose();
            }
        }


        private PyObject ImageToPyBytes(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // 将图像保存为 PNG 格式
                image.Save(ms, ImageFormat.Png);
                byte[] imageData = ms.ToArray();

                // 使用 Python 的 builtins.bytes 创建 Python 字节对象
                dynamic builtins = Py.Import("builtins");
                return builtins.bytes(imageData);
            }
        }
        private void ShowResultImages(string imgPath)
        {
            Console.WriteLine($"imgPath:{imgPath}");
            // 从本地加载图片
            Image originalImage = Image.FromFile(imgPath);

            // 获取每个切割块的宽度和高度
            int tileWidth = originalImage.Width / 3;
            int tileHeight = originalImage.Height / 3;

            // 列出所有的 PictureBox 控件
            PictureBox[] pictureBoxes = { PicDisplay1, PicDisplay2, PicDisplay3,
                                  PicDisplay4, PicDisplay5, PicDisplay6,
                                  PicDisplay7, PicDisplay8, PicDisplay9 };

            // 切割图片并将它们加载到 PictureBox 中
            int index = 0;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    // 创建新的 Bitmap，用于存储每一部分
                    Bitmap croppedImage = new Bitmap(tileWidth, tileHeight);

                    using (Graphics g = Graphics.FromImage(croppedImage))
                    {
                        // 定义要从原始图片中切割的矩形
                        Rectangle sourceRect = new Rectangle(col * tileWidth, row * tileHeight, tileWidth, tileHeight);

                        // 定义目标矩形，通常与 sourceRect 大小相同
                        Rectangle destRect = new Rectangle(0, 0, tileWidth, tileHeight);

                        // 将原始图片的一部分绘制到新的 Bitmap 中
                        g.DrawImage(originalImage, destRect, sourceRect, GraphicsUnit.Pixel);
                    }

                    // 将切割后的图片调整大小以适应对应的 PictureBox
                    PictureBox pictureBox = pictureBoxes[index];
                    pictureBox.Image = new Bitmap(croppedImage, pictureBox.Width, pictureBox.Height);
                    pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // 适应 PictureBox 大小

                    index++;
                }
            }

            // 释放原始图片的资源
            originalImage.Dispose();
        }

        private List<PyObject> GetImagesFromPictureBoxes()
        {
            // 创建一个列表，用于存储 PyObject 的图像字节
            List<PyObject> pyImageList = new List<PyObject>();

            // 获取 PictureBox1-9 中的图像
            PictureBox[] pictureBoxes = { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 };
            foreach (PictureBox pb in pictureBoxes)
            {
                if (pb.Image != null)
                {
                    PyObject pyBytes = ImageToPyBytes(pb.Image); // 转换为 PyObject 类型的字节对象
                    pyImageList.Add(pyBytes); // 添加到列表中
                }
            }

            return pyImageList;
        }
        private void removeAutofluorescenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 调用记录背景的方法
            backgroundToolStripMenuItem_Click(sender, e);

            // 调用启用背景减除的方法
            removeBackgroundToolStripMenuItem_Click(sender, e);

            // 可以添加额外的逻辑，例如显示一个消息
            MyShowStatus("自动荧光去除已应用");
        }

        static int GetColumnIndex(string cellReference)
        {
            string columnReference = new String(cellReference.TakeWhile(c => !char.IsDigit(c)).ToArray());
            int columnIndex = 0;
            for (int i = 0; i < columnReference.Length; i++)
            {
                columnIndex *= 26;
                columnIndex += (columnReference[i] - 'A' + 1);
            }
            return columnIndex - 1;
        }

        static List<List<string>> ReadExcelFileToColumnArrays(string filePath)
        {
            List<List<string>> columnArrays = new List<List<string>>();

            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                int maxColumnCount = sheetData.Elements<Row>().Max(r => r.Elements<Cell>().Count());

                for (int i = 0; i < maxColumnCount; i++)
                {
                    columnArrays.Add(new List<string>());
                }

                foreach (Row row in sheetData.Elements<Row>())
                {
                    int currentColumn = 0;
                    foreach (Cell cell in row.Elements<Cell>())
                    {
                        while (currentColumn < GetColumnIndex(cell.CellReference))
                        {
                            columnArrays[currentColumn].Add(string.Empty);
                            currentColumn++;
                        }

                        string cellValue = GetCellValue(spreadsheetDocument, cell);
                        columnArrays[currentColumn].Add(cellValue);
                        currentColumn++;
                    }

                    while (currentColumn < maxColumnCount)
                    {
                        columnArrays[currentColumn].Add(string.Empty);
                        currentColumn++;
                    }
                }
            }

            // 检查所有列的长度是否一致
            if (!AreAllColumnsEqualLength(columnArrays))
            {
                throw new InvalidDataException("Excel 文件中的列长度不一致。");
            }

            return columnArrays;
        }

        static bool AreAllColumnsEqualLength(List<List<string>> columnArrays)
        {
            if (columnArrays.Count == 0)
            {
                return true; // 空文件视为长度一致
            }

            int firstColumnLength = columnArrays[0].Count;
            return columnArrays.All(column => column.Count == firstColumnLength);
        }

        static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue?.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value ?? string.Empty;
            }
        }
        string selectedFilePath;
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("openToolStripMenuItem_Click");
            // 打开文件对话框，允许用户选择文件
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "KSJP Files|*.ksjp|KSJV Files|*.ksjv";
                openFileDialog.Title = "选择一个视频或图像文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    ProcessSelectFile(selectedFilePath);

                    if (!isInitBorderShow)
                        initChangeBtn();
                }
            }
        }

        private void ProcessSelectFile(string selectedFilePath)
        {
            string fileExtension = Path.GetExtension(selectedFilePath).ToLower();

            try
            {
                if (fileExtension == ".ksjp")
                {
                    Console.WriteLine("ksjp");
                    endPlatingVideo();
                    // 处理 .ksjp 图像文件
                    ProcessKsjpFile(selectedFilePath);
                }
                else if (fileExtension == ".ksjv")
                {
                    // 处理 .ksjv 视频文件
                    isVideoPlaying = true;
                    LiveBar.Visible = true;
                    ProcessKsjvFile(selectedFilePath);
                }
                else if (IsImageFile(selectedFilePath))
                {
                    MessageBox.Show("Unable to open this format");
                    //Console.WriteLine("IsImageFile");
                    //endPlatingVideo();
                    //// 如果是图片，使用 OpenCV 读取图像
                    //loadedImage = Cv2.ImRead(selectedFilePath, ImreadModes.Grayscale); // 读取为灰度图像

                    //if (loadedImage.Empty())
                    //{
                    //    MessageBox.Show("无法加载图片文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //    return;
                    //}
                    //currentFrame = loadedImage; // 存储当前帧的图像
                    //ProcessVideoFrames(); // 处理并显示图像
                }
                else if (IsVideoFile(selectedFilePath))
                {
                    MessageBox.Show("Unable to open this format");
                    //isVideoPlaying = true;
                    //LiveBar.Visible = true;
                    //StartVideoPlayback(selectedFilePath); // 启动视频播放

                }

                // 启用保存组中的控件
                EnableAllControls(saveGroup);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取或解密文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        // 处理 KSJP 文件
        private void ProcessKsjpFile(string filePath)
        {
            // 读取 .ksjp 文件的字节数据
            byte[] encryptedData = File.ReadAllBytes(filePath);

            // 解密：使用与加密相同的异或密钥 (例如 0x5A)
            byte[] decryptedData = PerformXOR(encryptedData, 0x5A);

            // 将解密后的字节数组转换为 Bitmap
            using (MemoryStream ms = new MemoryStream(decryptedData))
            {
                Bitmap decryptedImage = new Bitmap(ms);
                loadedImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(decryptedImage); // 将 Bitmap 转换为 Mat
            }

            // 检查是否成功解密和加载图像
            if (loadedImage == null || loadedImage.Empty())
            {
                MessageBox.Show("无法解密或加载图像文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 显示解密后的图片
            currentFrame = loadedImage; // 存储当前帧的图像
            ProcessVideoFrames(); // 处理并显示图像
        }

        // 处理 KSJV 文件
        private void ProcessKsjvFile(string filePath)
        {
            // 读取 .ksjv 文件的字节数据
            byte[] encryptedData = File.ReadAllBytes(filePath);

            // 解密：使用与加密相同的异或密钥 (例如 0x5A)
            byte[] decryptedData = PerformXOR(encryptedData, 0x5A);

            // 将解密后的数据保存为临时视频文件
            string tempVideoPath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(filePath)}.mp4");
            File.WriteAllBytes(tempVideoPath, decryptedData);

            // 启动解密后的视频播放
            StartVideoPlayback(tempVideoPath);
        }

        // 判断文件是否是图片文件
        private bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".bmp", ".jpg", ".jpeg", ".png" };
            string fileExtension = Path.GetExtension(filePath).ToLower();
            return imageExtensions.Contains(fileExtension);
        }

        // 判断文件是否是视频文件
        private bool IsVideoFile(string filePath)
        {
            string[] videoExtensions = { ".mp4", ".avi" };
            string fileExtension = Path.GetExtension(filePath).ToLower();
            return videoExtensions.Contains(fileExtension);
        }

        // 使用 OpenCV 读取视频帧，添加循环播放的逻辑
        private List<Mat> LoadVideoFrames(string videoFilePath, int maxFrames = 100)
        {
            List<Mat> frames = new List<Mat>();
            VideoCapture capture = new VideoCapture(videoFilePath);

            if (!capture.IsOpened())
            {
                MessageBox.Show("无法打开视频文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return frames;
            }

            Mat frame = new Mat();
            int frameCount = 0;
            while (capture.Read(frame) && frameCount < maxFrames)
            {
                if (frame.Empty()) break;
                frames.Add(frame.Clone());
                frameCount++;
            }

            capture.Release();
            return frames;
        }
        // 假设 videoFrames.Count 不为 100，而是任意帧数
        int totalFrames;
        private void StartVideoPlayback(string videoFilePath)
        {
            // 加载视频帧
            videoFrames = LoadVideoFrames(videoFilePath);

            if (videoFrames.Count == 0)
            {
                MessageBox.Show("没有可用的视频帧。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (frameRate <= 0)
            {
                frameRate = 30;  // 设置为默认帧率 30fps
            }
            LiveBar.Maximum = 100; // 设置进度条的最大值为100
            LiveBar.Value = 0;     // 重置进度条

            totalFrames = videoFrames.Count;
            // 设置定时器，间隔为帧率时间（如33ms）
            videoTimer = new System.Windows.Forms.Timer();
            videoTimer.Interval = (int)(1000 / frameRate); // 假设帧率为 frameRate
            videoTimer.Tick += new EventHandler(OnVideoFrameUpdate);
            videoTimer.Start();
        }

        private void OnVideoFrameUpdate(object sender, EventArgs e)
        {
            if (currentFrameIndex >= videoFrames.Count)
            {
                currentFrameIndex = 0; // 循环播放，重置索引
            }

            if (videoFrames.Count > 0 && currentFrameIndex < videoFrames.Count)
            {
                // 显示当前帧
                currentFrame = videoFrames[currentFrameIndex];
                // 保存原始未处理的帧
                originalFrame = currentFrame.Clone();

                // 检查是否启用了背景减除，并确保每一帧都正确应用背景减除
                if (isBackgroundSubtractionEnabled && backgroundImage != null)
                {
                    ApplyBackgroundSubtractionToFrame(ref currentFrame);
                }
                ProcessVideoFrames(); // 更新显示内容
                                      // 计算当前进度百分比
                if (totalFrames > 0)
                {
                    double progressPercentage = (double)currentFrameIndex / totalFrames * 100;
                    //Console.WriteLine($"{progressPercentage}");
                    // 更新进度条
                    LiveBar.Value = (int)progressPercentage;
                }
                currentFrameIndex++;
            }
        }
        //停止播放视频
        private void stopPlayingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 检查视频是否正在播放
            if (videoTimer != null && videoTimer.Enabled)
            {
                // 停止计时器，停止播放视频
                videoTimer.Stop();
                //videoTimer.Dispose();
                //videoTimer = null;
            }

            // 重置视频播放的当前帧索引
            //currentFrameIndex = 0;

            // 清空播放的 PictureBox 或者显示第一帧
            //PicDisplay1.Image = null; // 或者显示一个默认的图像

            // 清理相关视频帧缓存
            //if (videoFrames != null)
            //{
            //    videoFrames.Clear();
            //}

            // 更新UI状态（可以通过状态栏提示或按钮状态）
            MyShowStatus("视频播放已停止");
        }

        private void endPlatingVideo() {

            // 检查视频是否正在播放
            if (videoTimer != null && videoTimer.Enabled)
            {
                // 停止计时器，停止播放视频
                videoTimer.Stop();
                videoTimer.Dispose();
                videoTimer = null;
            }

            //重置视频播放的当前帧索引
           currentFrameIndex = 0;

            //清理相关视频帧缓存
            if (videoFrames != null)
            {
                videoFrames.Clear();
            }
            LiveBar.Visible = false;
        }

        private void SaveCurrentImage()
        {
            logger.LogButtonClick("SaveCurrentImage");
            // 确保原始帧存在
            if (originalFrame == null || originalFrame.Empty())
            {
                MessageBox.Show("没有可保存的原始图像。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 确保当前文件路径（保存路径）存在
            if (string.IsNullOrEmpty(m_savePath))
            {
                MessageBox.Show("请先选择保存目录。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string prefix = preName.Text.Trim(); // 获取 preName TextBox 的内容
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = "image"; // 如果 preName 为空，使用默认前缀
            }
            imageCounter++; // 增加计数器
            string filename = Path.Combine(m_savePath, $"{prefix}_{imageCounter:D4}.ksjp");
            // 获取当前时间戳
            //string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            //string filename = Path.Combine(m_savePath, $"original_frame_{timestamp}.ksjp");  // 使用自定义后缀名

            try
            {
                // 将原始帧转换为 Bitmap
                using (Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(originalFrame))
                {
                    // 将 Bitmap 转换为字节数组
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);  // 先保存为 PNG 格式
                        byte[] imageData = ms.ToArray();  // 获取字节数组

                        // 执行异或操作，使用固定的密钥，例如 0x5A
                        byte[] xorData = PerformXOR(imageData, 0x5A);

                        // 将异或后的数据保存为 .ksjp 文件
                        File.WriteAllBytes(filename, xorData);

                    }
                }

                MyShowStatus($"图像已成功保存：{filename}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存图像时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void currentImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 如果 currentFrame 不为空，表示当前有图像可以用作背景
            if (originalFrame != null && !originalFrame.Empty())
            {
                // 将 currentFrame 转换为 Bitmap 并保存为背景
                backgroundImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(originalFrame);
                MyShowStatus("背景图像已记录");
            }
            //else if (m_bitmap != null)  // 如果 currentFrame 为空但摄像头的 m_bitmap 可用
            //{
            //    lock (BitmapLock)
            //    {
            //        // 创建摄像头捕获图像的副本
            //        backgroundImage = new Bitmap(m_bitmap);
            //    }
            //    MyShowStatus("摄像头背景图像已记录");
            //}
            else
            {
                MessageBox.Show("没有可用的图像来记录背景。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("saveAsToolStripMenuItem_Click");
            // 确保原始帧存在
            if (currentFrame == null || currentFrame.Empty())
            {
                MessageBox.Show("没有可保存的原始图像。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //此处补充代码内容

            // 使用 SaveFileDialog 打开文件选择器
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveFileDialog.Filter = "Image Files|*.png"; // 设置文件类型过滤
                saveFileDialog.DefaultExt = "png"; // 默认后缀为 ksjp
                saveFileDialog.FileName = "image_0001.png"; // 默认文件名

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    m_savePath = Path.GetDirectoryName(saveFileDialog.FileName); // 更新保存目录
                    //preName.Text = Path.GetFileNameWithoutExtension(saveFileDialog.FileName); // 更新文件前缀

                    // 调用保存功能
                    saveToolStripMenuItem_Click(sender, e);
                }
            }

        }

        //异或加密算法
        private byte[] PerformXOR(byte[] data, byte key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key);  // 进行异或操作
            }
            return result;
        }
        //————————状态栏功能 end——————

        //————————工具栏功能——————
        // 用于存储形状和对应的坐标
        private Dictionary<PictureBox, List<ShapeInfo>> drawnShapes = new Dictionary<PictureBox, List<ShapeInfo>>();
        private bool isDragging = false;
        private ShapeInfo selectedShape = null;
        private System.Drawing.Point initialMousePosition;
        // 增加全局变量记录线条起点和点击次数

        // ShapeInfo 类用于存储形状类型和位置
        public class ShapeInfo
        {
            public System.Drawing.Point Location { get; set; }
            public DrawType ShapeType { get; set; }
            public System.Drawing.Color ShapeColor { get; set; }
            public int Width { get; set; }  // 新增宽度属性
            public int Height { get; set; } // 新增高度属性
            public double Length { get; set; } // 新增长度属性，用于线条
            public string TextContent { get; set; }  // 新增文本内容属性
            public int FontSize { get; set; } // 保存字体大小

            public ShapeInfo(System.Drawing.Point location, DrawType shapeType, System.Drawing.Color shapeColor)
            {
                Location = location;
                ShapeType = shapeType;
                ShapeColor = shapeColor;
                Width = 20;  // 默认宽度
                Height = 20; // 默认高度
                Length = 0;  // 初始化长度为 0
                TextContent = string.Empty;  // 初始化为空文本
                FontSize = 12; // 默认字体大小
            }
        }

        public enum DrawType
        {
            Point,
            Rectangle,
            Circle,
            Line,        // 直线
            ArrowLine,    // 带箭头的直线（双箭头）
            SingleArrowLine, // 单箭头直线
            calLine,
            Text  // 新增用于绘制文本的类型
        }
        private System.Drawing.Color currentColor = System.Drawing.Color.Black; // 默认颜色为红色
        private DrawType currentDrawType; // 当前绘制的形状类型
        private System.Drawing.Point? lineStartPoint = null; // 记录线条起点
        private System.Drawing.Point? lineEndPoint = null;   // 实时更新终点用于预览
        private bool isLineDrawing = false; // 标记当前是否正在绘制线条
        private bool isPreviewingLine = false; // 标记是否正在预览线条
        private bool isDrawingEnabled = false;  // 用于标识是否启用绘制功能
        private double pixelLength = 0; // 记录线条长度（像素）
        private PictureBox currentDrawingPictureBox = null;

        //判断当前是否点击
        private bool isPointToolActive = false;
        private bool isCircleToolActive = false;
        private bool isRectangleToolActive = false;
        private bool isLineToolActive = false;
        private bool isSingleArrowToolActive = false;
        private bool isDoubleArrowToolActive = false;
        private bool isPositioningActive = false;
        private bool isTextToolActive = false;
        private bool isCalLineActive = false;
        // 鼠标点击事件，用于绘制、选择或删除形状
        private void DrawFunction(object sender, MouseEventArgs e)
        {
            PictureBox clickedPictureBox = sender as PictureBox;
            if (clickedPictureBox != null)
            {
                // 如果是左键点击，开始绘制或选择形状进行拖动
                if (e.Button == MouseButtons.Left)
                {

                    // 记录当前正在绘制虚线的 PictureBox
                    currentDrawingPictureBox = clickedPictureBox;
                    if (!drawnShapes.ContainsKey(clickedPictureBox))
                    {
                        drawnShapes[clickedPictureBox] = new List<ShapeInfo>();
                    }

                    // 检查是否启用文本工具
                    if (currentDrawType == DrawType.Text)
                    {
                        // 检查是否已有文本框存在，存在的话先移除
                        if (currentTextBox != null)
                        {
                            clickedPictureBox.Controls.Remove(currentTextBox);
                            currentTextBox.Dispose();
                        }

                        // 在点击位置创建文本框
                        textBoxPosition = e.Location;
                        currentTextBox = new TextBox
                        {
                            Location = new System.Drawing.Point(textBoxPosition.Value.X, textBoxPosition.Value.Y),
                            Width = 100,
                            Font = new System.Drawing.Font("Microsoft YaHei", currentFontSize), // 设置字体，支持中文
                            ImeMode = ImeMode.On
                        };

                        clickedPictureBox.Controls.Add(currentTextBox);
                        currentTextBox.Focus();  // 将焦点设置到文本框上
                        currentTextBox.KeyDown += (s, ev) =>
                        {
                            if (ev.KeyCode == Keys.Enter)
                            {
                                // 按下回车键时获取文本框内容并绘制文本
                                string inputText = currentTextBox.Text;

                                // 绘制文本到屏幕上
                                if (!string.IsNullOrEmpty(inputText))
                                {
                                    using (Graphics g = clickedPictureBox.CreateGraphics())
                                    {
                                        using (Brush textBrush = new SolidBrush(currentColor))  // 使用当前的颜色绘制文本
                                        {
                                            g.DrawString(inputText, currentTextBox.Font, textBrush, textBoxPosition.Value);
                                        }
                                    }

                                    // 添加到drawnShapes列表中以便后续重绘
                                    drawnShapes[clickedPictureBox].Add(new ShapeInfo(textBoxPosition.Value, DrawType.Text, currentColor)
                                    {
                                        TextContent = inputText,  // 保存文本内容
                                        Width = inputText.Length * 7, // 计算文本的宽度
                                        Height = 20,  // 默认高度
                                        FontSize = currentFontSize  // 保存当前的字体大小

                                    });
                                }

                                // 移除文本框
                                clickedPictureBox.Controls.Remove(currentTextBox);
                                currentTextBox.Dispose();
                                currentTextBox = null;
                                textBoxPosition = null;
                            }
                        };

                        return;  // 直接返回，防止继续绘制其他形状
                    }

                    // 检查是否启用了长度测量功能
                    if (isLineDrawing && currentDrawType == DrawType.calLine)
                    {

                        if (!isPreviewingLine)
                        {
                            // 第一次点击，记录线条起点
                            lineStartPoint = e.Location;
                            isPreviewingLine = true; // 开始预览
                        }
                        else
                        {
                            // 第二次点击，完成线条绘制和像素长度计算
                            lineEndPoint = e.Location;

                            // 计算像素长度
                            pixelLength = CalculatePixelLength(lineStartPoint.Value, lineEndPoint.Value);

                            // 绘制线条
                            drawnShapes[clickedPictureBox].Add(new ShapeInfo(lineStartPoint.Value, currentDrawType, currentColor)
                            {
                                Width = lineEndPoint.Value.X - lineStartPoint.Value.X,
                                Height = lineEndPoint.Value.Y - lineStartPoint.Value.Y,
                                Length = pixelLength // 保存长度信息
                            });

                            // 重置状态
                            lineStartPoint = null;
                            lineEndPoint = null;
                            isPreviewingLine = false;
                            clickedPictureBox.Invalidate();  // 刷新 PictureBox 以触发重绘
                        }
                        return; // 直接返回，防止其他绘制行为

                    }

                    // 如果启用了十字辅助线，并且单击鼠标，则处理固定逻辑
                    if (isPositioningEnabled)
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            isPositionFixed = !isPositionFixed;  // 切换固定状态

                            if (isPositionFixed)
                            {
                                crossLinePosition = e.Location;  // 固定十字线位置
                                Console.WriteLine("Cross line position fixed at " + crossLinePosition);
                            }
                            else
                            {
                                crossLinePosition = null;  // 取消固定，继续跟随鼠标
                                Console.WriteLine("Cross line position unfixed, following mouse.");
                            }

                            clickedPictureBox.Invalidate();  // 刷新图像显示
                        }
                        return;  // 如果启用了十字辅助线，直接返回，不绘制其他形状
                    }
                    // 判断是否点击了已绘制的形状
                    selectedShape = FindShapeAtPoint(e.Location, clickedPictureBox);
                    if (selectedShape != null)
                    {
                        // 如果点击了形状，准备调整大小
                        isDragging = true;
                        initialMousePosition = e.Location;
                    }
                    else
                    {
                        // 如果是线条或带箭头的线条，处理双击绘制
                        if (currentDrawType == DrawType.Line || currentDrawType == DrawType.ArrowLine || currentDrawType == DrawType.SingleArrowLine)
                        {
                            if (lineStartPoint == null)
                            {
                                // 第一次点击，记录起始点
                                lineStartPoint = e.Location;
                                isPreviewingLine = true; // 开启预览模式
                            }
                            else
                            {
                                // 第二次点击，完成线条绘制
                                System.Drawing.Point endPoint = e.Location;
                                if (lineStartPoint.Value != endPoint)
                                {
                                    drawnShapes[clickedPictureBox].Add(new ShapeInfo((System.Drawing.Point)lineStartPoint, currentDrawType, currentColor)
                                    {
                                        Width = endPoint.X - ((System.Drawing.Point)lineStartPoint).X,
                                        Height = endPoint.Y - ((System.Drawing.Point)lineStartPoint).Y
                                    });
                                }

                                // 重置状态
                                lineStartPoint = null;
                                isPreviewingLine = false;
                                clickedPictureBox.Invalidate(); // 刷新 PictureBox 以触发重绘
                            }
                        }
                        else
                        {
                            // 如果是普通形状（矩形、圆形、点），只需要点击一次即可绘制
                            drawnShapes[clickedPictureBox].Add(new ShapeInfo(new System.Drawing.Point(e.X, e.Y), currentDrawType, currentColor));
                            clickedPictureBox.Invalidate(); // 刷新 PictureBox 以触发重绘
                        }
                    }
                }
                // 右键点击，用于删除形状
                else if (e.Button == MouseButtons.Right)
                {
                    // 查找点击位置的形状
                    ShapeInfo shapeToDelete = FindShapeAtPoint(e.Location, clickedPictureBox);
                    if (shapeToDelete != null)
                    {
                        // 删除形状
                        drawnShapes[clickedPictureBox].Remove(shapeToDelete);
                        clickedPictureBox.Invalidate(); // 刷新 PictureBox 以触发重绘
                    }
                }
            }
        }


        // 鼠标移动事件，用于拖动和放大形状
        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            // 更新 X_loc 和 Y_loc 的坐标显示
            if (pictureBox != null && !isPositionFixed)
            {
                bottomLabelLeft.Text = $"X:{e.X},Y:{e.Y};";
                //X_loc.Text = $"{e.X}";
                //Y_loc.Text = $"{e.Y}";
            }
            if (isLineDrawing && !isPreviewingLine && lineStartPoint.HasValue)
            {
                // 更新线条终点为当前鼠标位置
                lineEndPoint = e.Location;
                // 触发重绘以显示虚线和像素长度
                pictureBox.Invalidate();
            }
            if (isPositioningEnabled && !isPositionFixed)
            {
                // 更新十字线的中心位置为当前鼠标位置
                crossLinePosition = e.Location;
                pictureBox.Invalidate(); // 刷新 PictureBox 以触发重绘
            }

            if (isPreviewingLine && lineStartPoint != null)
            {
                // 实时更新线条终点用于预览
                lineEndPoint = e.Location;
                pictureBox.Invalidate(); // 刷新 PictureBox 以触发重绘
            }
            else if (isDragging && selectedShape != null && pictureBox != null)
            {
                // 计算鼠标移动的距离来调整形状大小
                int deltaX = e.X - initialMousePosition.X;
                int deltaY = e.Y - initialMousePosition.Y;

                if (selectedShape.ShapeType == DrawType.Rectangle || selectedShape.ShapeType == DrawType.Circle)
                {
                    // 更新矩形或圆的宽度和高度
                    selectedShape.Width = Math.Abs(deltaX) * 2; // 假设拖动影响宽度
                    selectedShape.Height = Math.Abs(deltaY) * 2; // 假设拖动影响高度

                    // 保证圆形等宽等高
                    if (selectedShape.ShapeType == DrawType.Circle)
                    {
                        selectedShape.Width = selectedShape.Height = Math.Max(selectedShape.Width, selectedShape.Height);
                    }
                }

                pictureBox.Invalidate(); // 刷新 PictureBox 以触发重绘
            }
        }

        // 鼠标释放事件，用于结束拖动
        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                selectedShape = null; // 结束选择
            }
        }

        // 重新绘制形状，包含拖动放大的更新
        private void RedrawShapes(PictureBox pictureBox, Graphics g)
        {
            if (pictureBox != null && drawnShapes.ContainsKey(pictureBox))
            {
                foreach (var shapeInfo in drawnShapes[pictureBox])
                {
                    using (Pen pen = new Pen(shapeInfo.ShapeColor, 2))
                    {
                        switch (shapeInfo.ShapeType)
                        {
                            case DrawType.Point:
                                int pointSize = 5;
                                g.FillEllipse(new SolidBrush(shapeInfo.ShapeColor), shapeInfo.Location.X - pointSize / 2, shapeInfo.Location.Y - pointSize / 2, pointSize, pointSize);
                                break;
                            case DrawType.Rectangle:
                                g.DrawRectangle(pen, shapeInfo.Location.X - shapeInfo.Width / 2, shapeInfo.Location.Y - shapeInfo.Height / 2, shapeInfo.Width, shapeInfo.Height);
                                break;
                            case DrawType.Circle:
                                g.DrawEllipse(pen, shapeInfo.Location.X - shapeInfo.Width / 2, shapeInfo.Location.Y - shapeInfo.Height / 2, shapeInfo.Width, shapeInfo.Height);
                                break;
                            case DrawType.Line:
                                // 绘制普通直线
                                g.DrawLine(pen, shapeInfo.Location, new System.Drawing.Point(shapeInfo.Location.X + shapeInfo.Width, shapeInfo.Location.Y + shapeInfo.Height));
                                break;
                            case DrawType.SingleArrowLine:
                                // 绘制单箭头直线
                                DrawArrowLine(g, pen, shapeInfo.Location, new System.Drawing.Point(shapeInfo.Location.X + shapeInfo.Width, shapeInfo.Location.Y + shapeInfo.Height), false);
                                break;
                            case DrawType.ArrowLine:
                                // 绘制双箭头直线
                                DrawArrowLine(g, pen, shapeInfo.Location, new System.Drawing.Point(shapeInfo.Location.X + shapeInfo.Width, shapeInfo.Location.Y + shapeInfo.Height), true);
                                break;
                            case DrawType.calLine:
                                // 先绘制普通直线
                                g.DrawLine(pen, shapeInfo.Location, new System.Drawing.Point(shapeInfo.Location.X + shapeInfo.Width, shapeInfo.Location.Y + shapeInfo.Height));

                                // 计算线条的中点
                                System.Drawing.Point middlePoint = new System.Drawing.Point(
                                    shapeInfo.Location.X + shapeInfo.Width / 2,
                                    shapeInfo.Location.Y + shapeInfo.Height / 2
                                );
                                // 绘制线条长度在中点
                                string lengthText = $"{shapeInfo.Length:F2} px";
                                SizeF textSize = g.MeasureString(lengthText, SystemFonts.DefaultFont);
                                using (Brush brightBrush = new SolidBrush(shapeInfo.ShapeColor))
                                {
                                    g.DrawString(lengthText, SystemFonts.DefaultFont, brightBrush, middlePoint.X - textSize.Width / 2, middlePoint.Y - textSize.Height / 2);
                                }
                                break;
                            case DrawType.Text:
                                // 绘制保存的文本
                                // 使用 shapeInfo 的颜色绘制文本
                                using (Brush textBrush = new SolidBrush(shapeInfo.ShapeColor))
                                {
                                    // 创建一个字体对象
                                    using (System.Drawing.Font textFont = new System.Drawing.Font("Microsoft YaHei", shapeInfo.FontSize)) // 使用保存的字体大小
                                    {
                                        g.DrawString(shapeInfo.TextContent, textFont, textBrush, shapeInfo.Location);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            if (pictureBox == currentDrawingPictureBox)
            {
                // 绘制正在预览的线条
                if (isPreviewingLine && lineStartPoint != null && lineEndPoint != null)
                {
                    using (Pen previewPen = new Pen(System.Drawing.Color.Gray, 1))
                    {
                        g.DrawLine(previewPen, (System.Drawing.Point)lineStartPoint, (System.Drawing.Point)lineEndPoint);
                    }
                }
            }


        }

        // 绘制带箭头的线条
        private void DrawArrowLine(Graphics g, Pen pen, System.Drawing.Point start, System.Drawing.Point end, bool isDoubleArrow)
        {
            // 确保 Pen 的 DashStyle 为实线
            pen.DashStyle = DashStyle.Solid;
            // 设置线条终点的箭头
            try
            {
                pen.CustomEndCap = new AdjustableArrowCap(5, 5);  // 5x5 的箭头大小

                // 如果是双箭头，则设置起点的箭头
                if (isDoubleArrow)
                {
                    pen.CustomStartCap = new AdjustableArrowCap(5, 5);
                }

                // 绘制线条
                g.DrawLine(pen, start, end);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"参数错误: {ex.Message}");
            }
            finally
            {
                // 重置 pen 的箭头样式为默认的 LineCap
                pen.CustomEndCap = new AdjustableArrowCap(0, 0); // 设置为空大小的箭头，效果相当于没有箭头
                pen.CustomStartCap = new AdjustableArrowCap(0, 0); // 同样处理起点箭头
            }
        }

        // 检查点是否在有效的坐标范围内（可以根据 PictureBox 或 Graphics 的实际大小调整范围）
        private bool IsPointValid(System.Drawing.Point point)
        {
            return point.X >= 0 && point.X <= 10000 && point.Y >= 0 && point.Y <= 10000; // 假设 10000 是合理的上限
        }


        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;

            Graphics g = e.Graphics;
            // 绘制已经存在的形状
            RedrawShapes(pictureBox, g);
            // 如果正在预览线条，绘制实时变化的虚线和像素长度
            if (isLineDrawing && isPreviewingLine && lineStartPoint.HasValue && lineEndPoint.HasValue)
            {
                using (Pen dashedPen = new Pen(System.Drawing.Color.Gray, 1))
                {
                    if (pictureBox == currentDrawingPictureBox)
                    {

                        dashedPen.DashStyle = DashStyle.Dash;

                        // 绘制虚线
                        g.DrawLine(dashedPen, lineStartPoint.Value, lineEndPoint.Value);
                        //Console.WriteLine($"{lineEndPoint.Value.Y}");
                        // 计算中点
                        System.Drawing.Point middlePoint = new System.Drawing.Point(
                            (lineStartPoint.Value.X + lineEndPoint.Value.X) / 2,
                            (lineStartPoint.Value.Y + lineEndPoint.Value.Y) / 2
                        );

                        // 计算线条的像素长度
                        pixelLength = CalculatePixelLength(lineStartPoint.Value, lineEndPoint.Value);
                        // 绘制像素长度
                        string lengthText = $"{pixelLength:F2} px";
                        SizeF textSize = g.MeasureString(lengthText, SystemFonts.DefaultFont);

                        // 调整文本的位置以防止被鼠标挡住，确保在线条中点稍上方
                        g.DrawString(lengthText, SystemFonts.DefaultFont, Brushes.Black, middlePoint.X - textSize.Width / 2, middlePoint.Y - textSize.Height / 2);

                    }
                }
            }
            // 绘制十字辅助线
            if (isPositioningEnabled && crossLinePosition.HasValue && pictureBox == currentDrawingPictureBox)
            {
                System.Drawing.Point center = crossLinePosition.Value;

                using (Pen dashedPen = new Pen(System.Drawing.Color.Gray, 1))
                {
                    dashedPen.DashStyle = DashStyle.Dash;

                    // 绘制水平和垂直虚线
                    g.DrawLine(dashedPen, new System.Drawing.Point(0, center.Y), new System.Drawing.Point(pictureBox.Width, center.Y));
                    g.DrawLine(dashedPen, new System.Drawing.Point(center.X, 0), new System.Drawing.Point(center.X, pictureBox.Height));
                }
            }
        }

        private void beginDrawFunc()
        {
            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                pictureBox.MouseDown += DrawFunction;
                pictureBox.MouseMove += PictureBox_MouseMove;
                pictureBox.MouseUp += PictureBox_MouseUp;
                pictureBox.Paint += PictureBox_Paint;
            }
        }

        private void DisableDrawing()
        {
            // 注销鼠标事件以禁用绘制
            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                pictureBox.MouseDown -= DrawFunction;
                pictureBox.MouseMove -= PictureBox_MouseMove;
                pictureBox.MouseUp -= PictureBox_MouseUp;
                pictureBox.Paint -= PictureBox_Paint;
            }

            // 重置绘制状态
            lineStartPoint = null;
            isPreviewingLine = false;
            isLineDrawing = false;
        }
        private void applyAllTool_Click(object sender, EventArgs e)
        {
            // 初始化一个列表，用于存储所有图像中的非calLine形状
            List<ShapeInfo> allNonCalLineShapes = new List<ShapeInfo>();

            // 遍历所有 PictureBox，收集它们的非calLine绘制形状
            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                if (drawnShapes.ContainsKey(pictureBox))
                {
                    // 只添加ShapeType不是calLine的形状到共享列表
                    allNonCalLineShapes.AddRange(drawnShapes[pictureBox].Where(shape => shape.ShapeType != DrawType.calLine || shape.ShapeType == DrawType.Text));
                }
            }

            // 遍历所有 PictureBox，将合并后的形状（不包括calLine）应用到每个图像上，并保留其原本的calLine形状
            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                // 如果当前 PictureBox 没有形状信息，则初始化
                if (!drawnShapes.ContainsKey(pictureBox))
                {
                    drawnShapes[pictureBox] = new List<ShapeInfo>();
                }

                // 保留当前 PictureBox 中的calLine形状
                var originalCalLineShapes = drawnShapes[pictureBox].Where(shape => shape.ShapeType == DrawType.calLine || shape.ShapeType == DrawType.Text).ToList();

                // 将非calLine形状添加到当前 PictureBox 中，同时保留原有的calLine形状
                drawnShapes[pictureBox] = originalCalLineShapes.Concat(allNonCalLineShapes.Select(shape =>
                    new ShapeInfo(shape.Location, shape.ShapeType, shape.ShapeColor)
                    {
                        Width = shape.Width,     // 传递宽度
                        Height = shape.Height    // 传递高度
                    })).ToList();

                // 刷新 PictureBox，触发重绘
                pictureBox.Invalidate();
            }

            Console.WriteLine("All non-calLine drawings have been unified and applied to all images, while original calLine shapes are retained.");
        }


        private void cancelDrawTool_Click(object sender, EventArgs e)
        {
            // 清空所有已绘制的形状
            drawnShapes.Clear();

            // 刷新所有 PictureBox，清除绘制内容
            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                pictureBox.Invalidate();  // 触发重新绘制，清除内容
            }

            Console.WriteLine("All drawings have been cleared.");
        }
        private ShapeInfo FindShapeAtPoint(System.Drawing.Point point, PictureBox pictureBox)
        {
            if (drawnShapes.ContainsKey(pictureBox))
            {
                foreach (var shape in drawnShapes[pictureBox])
                {
                    if (IsPointNearShape(point, shape))
                    {
                        Console.WriteLine($"Shape found: {shape.ShapeType}");
                        return shape;
                    }
                }
            }
            Console.WriteLine("No shape found");
            return null;
        }
        // 判断点击的点是否接近形状的边缘，用于调整大小或删除
        private bool IsPointNearShape(System.Drawing.Point clickedPoint, ShapeInfo shapeInfo)
        {
            int threshold = 10; // 定义默认距离阈值，单位是像素
            switch (shapeInfo.ShapeType)
            {
                case DrawType.Point:
                    // 对于点，使用更小的阈值来判断点击是否接近点
                    int pointThreshold = 5;
                    return Math.Abs(clickedPoint.X - shapeInfo.Location.X) <= pointThreshold &&
                           Math.Abs(clickedPoint.Y - shapeInfo.Location.Y) <= pointThreshold;

                case DrawType.Rectangle:
                    return clickedPoint.X >= shapeInfo.Location.X - shapeInfo.Width / 2 &&
                           clickedPoint.X <= shapeInfo.Location.X + shapeInfo.Width / 2 &&
                           clickedPoint.Y >= shapeInfo.Location.Y - shapeInfo.Height / 2 &&
                           clickedPoint.Y <= shapeInfo.Location.Y + shapeInfo.Height / 2;

                case DrawType.Circle:
                    // 使用距离公式判断点击点是否在圆形附近
                    double distance = Math.Sqrt(Math.Pow(clickedPoint.X - shapeInfo.Location.X, 2) + Math.Pow(clickedPoint.Y - shapeInfo.Location.Y, 2));
                    return distance <= shapeInfo.Width / 2 + threshold;

                case DrawType.Line:
                case DrawType.SingleArrowLine:
                case DrawType.ArrowLine:
                case DrawType.calLine:
                    // 对于线条，计算点到线段的距离
                    return IsPointNearLine(clickedPoint, shapeInfo.Location, new System.Drawing.Point(shapeInfo.Location.X + shapeInfo.Width, shapeInfo.Location.Y + shapeInfo.Height), threshold);
                case DrawType.Text:
                    // 检查点击点是否在文本范围内，假设文本区域是矩形
                    SizeF textSize = TextRenderer.MeasureText(shapeInfo.TextContent, new System.Drawing.Font("Microsoft YaHei", shapeInfo.FontSize));
                    return clickedPoint.X >= shapeInfo.Location.X && clickedPoint.X <= shapeInfo.Location.X + textSize.Width &&
                           clickedPoint.Y >= shapeInfo.Location.Y && clickedPoint.Y <= shapeInfo.Location.Y + textSize.Height;

                default:
                    return false;
            }
        }

        // 判断点击点是否在一条线段附近
        private bool IsPointNearLine(System.Drawing.Point p, System.Drawing.Point lineStart, System.Drawing.Point lineEnd, int threshold)
        {
            double distance = DistanceFromPointToLine(p, lineStart, lineEnd);
            return distance <= threshold;
        }

        private void ResetTools()
        {
            // 将所有工具按钮的背景颜色恢复为默认
            toolPointBtn.BackColor = System.Drawing.Color.Transparent;
            toolCircleBtn.BackColor = System.Drawing.Color.Transparent;
            toolRectBtn.BackColor = System.Drawing.Color.Transparent;
            lineDrawTool.BackColor = System.Drawing.Color.Transparent;
            singleArrowTool.BackColor = System.Drawing.Color.Transparent;
            doubleArrowTool.BackColor = System.Drawing.Color.Transparent;
            positionTool.BackColor = System.Drawing.Color.Transparent;  // 重置辅助线按钮的状态
            textToolBtn.BackColor = System.Drawing.Color.Transparent;
            toolCalBtn.BackColor = System.Drawing.Color.Transparent;

            // 禁用绘制功能
            DisableDrawing();
            isDrawingEnabled = false;
            // 停止线条绘制，防止继续绘制线条
            isLineDrawing = false;
            // 禁用十字辅助线
            isPositioningEnabled = false;
            isPositionFixed = false;
            crossLinePosition = null;

        }
        private void EnableDragging()
        {
            isPointToolActive = false;
            isCircleToolActive = false;
            isRectangleToolActive = false;
            isLineToolActive = false;
            isSingleArrowToolActive = false;
            isDoubleArrowToolActive = false;
            isPositioningActive = false;
            isTextToolActive = false;
            isCalLineActive = false;
            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                pictureBox.MouseEnter -= PictureBox_MouseEnter;
                pictureBox.MouseWheel -= PictureBox_MouseWheel;
                pictureBox.MouseDown -= PictureBox_MouseDown;
                pictureBox.MouseMove -= PictureBox_MouseMoveShow;
                pictureBox.MouseUp -= PictureBox_MouseUpShow;
            }
        }
        private void DisableDragging()
        {
            // 注销鼠标事件以禁用绘制
            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                pictureBox.MouseDown -= DrawFunction;
                pictureBox.MouseMove -= PictureBox_MouseMove;
                pictureBox.MouseUp -= PictureBox_MouseUp;
            }
            foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
            {
                pictureBox.MouseEnter += PictureBox_MouseEnter;
                pictureBox.MouseWheel += PictureBox_MouseWheel;
                pictureBox.MouseDown += PictureBox_MouseDown;
                pictureBox.MouseMove += PictureBox_MouseMoveShow;
                pictureBox.MouseUp += PictureBox_MouseUpShow;
            }
        }
        private void toolPointBtn_Click(object sender, EventArgs e)
        {
            if (!isPointToolActive)
            {

                Console.WriteLine($"Point启用");
                EnableDragging();
                isPointToolActive = true;
                ResetTools(); // 重置所有工具

                // 启用绘制点
                isDrawingEnabled = true;
                beginDrawFunc();
                currentDrawType = DrawType.Point;
                toolPointBtn.BackColor = System.Drawing.Color.Green;  // 按钮显示为激活状态
            }
            else
            {
                DisableDragging();
                isPointToolActive = false;
                toolPointBtn.BackColor = System.Drawing.Color.Transparent;
            }

        }
        private void toolCircleBtn_Click(object sender, EventArgs e)
        {
            if (!isCircleToolActive)
            {
                EnableDragging();
                isCircleToolActive = true;

                ResetTools(); // 重置所有工具

                // 启用绘制圆
                isDrawingEnabled = true;
                beginDrawFunc();
                currentDrawType = DrawType.Circle;
                toolCircleBtn.BackColor = System.Drawing.Color.Green;  // 按钮显示为激活状态
            }
            else
            {
                DisableDragging();
                isCircleToolActive = false;
                toolCircleBtn.BackColor = System.Drawing.Color.Transparent;
            }

        }
        private void toolRectBtn_Click(object sender, EventArgs e)
        {
            if (!isRectangleToolActive)
            {
                EnableDragging();
                isRectangleToolActive = true;

                ResetTools(); // 重置所有工具

                // 启用绘制矩形
                isDrawingEnabled = true;
                beginDrawFunc();
                currentDrawType = DrawType.Rectangle;
                toolRectBtn.BackColor = System.Drawing.Color.Green;  // 按钮显示为激活状态
            }
            else
            {
                DisableDragging();
                isRectangleToolActive = false;
                toolRectBtn.BackColor = System.Drawing.Color.Transparent;
            }
        }
        private void lineDrawTool_Click(object sender, EventArgs e)
        {
            if (!isLineToolActive)
            {
                EnableDragging();
                isLineToolActive = true;
                ResetTools(); // 重置所有工具

                // 启用绘制线条
                isDrawingEnabled = true;
                beginDrawFunc();
                currentDrawType = DrawType.Line;
                lineDrawTool.BackColor = System.Drawing.Color.Green;  // 按钮显示为激活状态
            }
            else
            {
                DisableDragging();
                isLineToolActive = false;
                lineDrawTool.BackColor = System.Drawing.Color.Transparent;
            }
        }
        private void singleArrowTool_Click(object sender, EventArgs e)
        {
            if (!isSingleArrowToolActive)
            {

                Console.WriteLine($"Arrow启用");
                EnableDragging();
                isSingleArrowToolActive = true;
                ResetTools(); // 重置所有工具

                // 启用绘制单箭头线
                isDrawingEnabled = true;
                beginDrawFunc();
                currentDrawType = DrawType.SingleArrowLine;
                singleArrowTool.BackColor = System.Drawing.Color.Green;  // 按钮显示为激活状态
            }
            else
            {
                DisableDragging();
                isSingleArrowToolActive = false;
                singleArrowTool.BackColor = System.Drawing.Color.Transparent;
            }
        }
        private void doubleArrowTool_Click(object sender, EventArgs e)
        {
            if (!isDoubleArrowToolActive)
            {
                EnableDragging();
                isDoubleArrowToolActive = true;

                ResetTools(); // 重置所有工具

                // 启用绘制双箭头线
                isDrawingEnabled = true;
                beginDrawFunc();
                currentDrawType = DrawType.ArrowLine;
                doubleArrowTool.BackColor = System.Drawing.Color.Green;  // 按钮显示为激活状态
            }
            else
            {
                DisableDragging();
                isDoubleArrowToolActive = false;
                doubleArrowTool.BackColor = System.Drawing.Color.Transparent;
            }
        }


        private void colorToolSelec_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    currentColor = colorDialog.Color; // 将选择的颜色应用到下划线
                    colorToolSelec.Invalidate(); // 触发重绘以更新下划线颜色
                }
            }
        }

        // 自定义绘制 ToolStripButton 的下划线
        private void colorToolSelec_Paint(object sender, PaintEventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button != null)
            {
                // 绘制下划线的高度和位置
                int underlineHeight = 3; // 下划线的高度
                int underlineY = button.Height - underlineHeight; // 下划线的 Y 坐标

                // 使用当前颜色绘制下划线
                using (Brush brush = new SolidBrush(currentColor))
                {
                    e.Graphics.FillRectangle(brush, 0, underlineY, button.Width, underlineHeight);
                }
            }
        }

        // 计算点到线段的最小距离
        private double DistanceFromPointToLine(System.Drawing.Point p, System.Drawing.Point lineStart, System.Drawing.Point lineEnd)
        {
            double A = p.X - lineStart.X;
            double B = p.Y - lineStart.Y;
            double C = lineEnd.X - lineStart.X;
            double D = lineEnd.Y - lineStart.Y;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = (len_sq != 0) ? (dot / len_sq) : -1;

            double xx, yy;

            if (param < 0)
            {
                xx = lineStart.X;
                yy = lineStart.Y;
            }
            else if (param > 1)
            {
                xx = lineEnd.X;
                yy = lineEnd.Y;
            }
            else
            {
                xx = lineStart.X + param * C;
                yy = lineStart.Y + param * D;
            }

            double dx = p.X - xx;
            double dy = p.Y - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        //定位辅助线
        // 用于控制是否启用定位辅助线
        private bool isPositioningEnabled = false; // 是否启用十字辅助线
        private System.Drawing.Point? crossLinePosition = null; // 十字线的固定位置，如果为null则跟随鼠标移动
        private bool isPositionFixed = false; // 标记十字线是否已经固定
        private void positionTool_Click(object sender, EventArgs e)
        {

            if (!isPointToolActive)
            {
                EnableDragging();
                isPointToolActive = true;

                ResetTools();  // 重置所有工具和状态

                // 启用十字辅助线
                isPositioningEnabled = true;
                isPositionFixed = false; // 取消固定，允许跟随鼠标
                positionTool.BackColor = System.Drawing.Color.Green;  // 按钮显示为激活状态

                // 启用绘制逻辑
                beginDrawFunc();
            }
            else
            {
                DisableDragging();
                isPointToolActive = false;
                isPositioningEnabled = false;  // 禁用十字辅助线
                crossLinePosition = null;      // 清除十字线位置
                positionTool.BackColor = System.Drawing.Color.Transparent;
                // 刷新 PictureBox 以移除十字虚线
                foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
                {
                    pictureBox.Invalidate(); // 触发重绘以清除虚线
                }
            }

        }

        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            if (isLineDrawing)
            {
                if (!isPreviewingLine)
                {
                    // 第一次点击，记录线条起点
                    lineStartPoint = e.Location;
                    isPreviewingLine = true; // 开始预览
                }
                else
                {
                    // 第二次点击，固定线条
                    lineEndPoint = e.Location;

                    // 计算并显示最终的像素长度
                    pixelLength = CalculatePixelLength(lineStartPoint.Value, lineEndPoint.Value);

                    // 绘制最终线条并重置状态
                    drawnShapes[pictureBox].Add(new ShapeInfo(lineStartPoint.Value, DrawType.Line, currentColor)
                    {
                        Width = lineEndPoint.Value.X - lineStartPoint.Value.X,
                        Height = lineEndPoint.Value.Y - lineStartPoint.Value.Y,
                        Length = pixelLength // 保存长度信息
                    });

                    // 重置状态
                    lineStartPoint = null;
                    lineEndPoint = null;
                    isPreviewingLine = false;
                    isLineDrawing = false;
                    pictureBox.Invalidate();  // 触发重绘以显示最终的线条
                }
            }
            if (isPositioningEnabled)
            {
                // 切换固定状态
                isPositionFixed = !isPositionFixed;

                if (isPositionFixed)
                {
                    // 固定当前十字线的位置
                    crossLinePosition = e.Location;
                    Console.WriteLine("Cross line position fixed at " + crossLinePosition);
                }
                else
                {
                    // 取消固定，继续跟随鼠标移动
                    crossLinePosition = null;
                    Console.WriteLine("Cross line position unfixed, following mouse.");
                }

                pictureBox.Invalidate(); // 刷新 PictureBox 以触发重绘
            }
        }

        //计算长度
        private void toolCalBtn_Click(object sender, EventArgs e)
        {
            if (!isCalLineActive)
            {
                EnableDragging();
                isCalLineActive = true;

                ResetTools();  // 重置其他工具的状态

                // 启用线条绘制和像素计算功能
                isLineDrawing = true;
                isPreviewingLine = false;  // 开始预览

                currentDrawType = DrawType.calLine;
                // 注册鼠标事件
                beginDrawFunc();

                // 更新按钮状态
                toolCalBtn.BackColor = System.Drawing.Color.Green;
            }
            else
            {
                DisableDragging();
                isCalLineActive = false;
                toolCalBtn.BackColor = System.Drawing.Color.Transparent;
            }
        }

        // 增加全局变量记录文本框状态
        private TextBox currentTextBox = null;
        private System.Drawing.Point? textBoxPosition = null;  // 记录文本框的位置
        private bool isTextToolEnabled = false;  // 标识是否启用文本输入功能
        // 定义用于字号调节的 ContextMenuStrip
        private ContextMenuStrip fontSizeContextMenu;
        private int currentFontSize = 12;  // 默认字体大小
        private void textToolBtn_Click(object sender, EventArgs e)
        {

            if (!isTextToolActive)
            {
                EnableDragging();
                isTextToolActive = true;

                ResetTools(); // 重置其他工具的状态
                currentDrawType = DrawType.Text;
                isTextToolEnabled = true;  // 启用文本输入功能
                beginDrawFunc();  // 开启鼠标事件
                textToolBtn.BackColor = System.Drawing.Color.Green;  // 按钮显示为激活状态
            }
            else
            {
                DisableDragging();
                isTextToolActive = false;
                textToolBtn.BackColor = System.Drawing.Color.Transparent;
            }
        }
        // 初始化右键菜单
        private void InitializeFontSizeContextMenu()
        {
            fontSizeContextMenu = new ContextMenuStrip();

            // 添加几个常用字号选项
            for (int i = 8; i <= 24; i += 2)
            {
                ToolStripMenuItem fontSizeItem = new ToolStripMenuItem($"{i} pt", null, FontSizeItem_Click);
                fontSizeItem.Tag = i;  // 使用 Tag 存储字号大小
                fontSizeContextMenu.Items.Add(fontSizeItem);
            }
        }

        // 字号菜单项点击事件
        private void FontSizeItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = sender as ToolStripMenuItem;
            if (clickedItem != null)
            {
                currentFontSize = (int)clickedItem.Tag;  // 设置当前字体大小
            }
        }

        private void textToolBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 如果是右键单击，显示字号调节菜单
                fontSizeContextMenu.Show(Cursor.Position);
            }
        }

        private void fillColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            // 模拟左键点击事件
            MouseEventArgs leftClick = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);

            // 调用所有按钮的 ChangeButtonColor 方法
            foreach (var button in displayButtons)
            {
                ChangeButtonColor(button, leftClick);
            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 创建 aboutShow 窗口的实例
            aboutShow aboutWindow = new aboutShow();

            // 显示窗口
            aboutWindow.ShowDialog(); // 使用 ShowDialog() 使其为模态窗口，也可以使用 Show() 作为非模态窗口
        }

        private void loggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string logFilePath = logger.logFilePath;

            // 打开包含日志文件的文件夹
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (logDirectory != null)
            {
                Process.Start("explorer.exe", logDirectory);
            }
        }

        private void smallMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void fullScreanMenuItem_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                int width = this.ClientSize.Width;
                int height = this.ClientSize.Height;
                // 当前为全屏状态，切换为正常窗口
                this.WindowState = FormWindowState.Normal;
                this.fullScreanMenuItem.Image = Properties.Resources.全屏; // 显示全屏图标
                this.Width = (int)(width * 0.7);   // 设置窗体的宽度为
                this.Height = (int)(height * 0.7);  // 设置窗体的高度为
                this.leftPanel.Width = (int)(this.ClientSize.Width * 0.185);
                this.rightPanel.Width = (int)(this.ClientSize.Width * 0.22);
                InitializeEdgePanel();

            }
            else
            {
                // 当前为正常窗口，切换为全屏
                this.WindowState = FormWindowState.Maximized;
                this.fullScreanMenuItem.Image = Properties.Resources.恢复窗口; // 显示恢复图标

                this.leftPanel.Width = (int)(this.ClientSize.Width * 0.185);
                this.rightPanel.Width = (int)(this.ClientSize.Width * 0.22);

                UninitializeEdgePanel();
            }
        }

        private void ExitMenuItem_Click_1(object sender, EventArgs e)
        {
            exitToolStripMenuItem_Click(this, EventArgs.Empty);
            cudaContext.Dispose();
            if (liveStatus)
                liveStatus = false;
            if (activeCam != null)
                activeCam.dev_exit();

            if (isRecording)
            {
                StopRecording();
            }
            // 停止定时器
            if (videoTimer != null)
            {
                videoTimer.Stop();
                videoTimer.Dispose();
            }

            frameRateTimer?.Stop();
            frameRateTimer?.Dispose();
            this.Close();
        }


        private void enhanceBtn_Click(object sender, EventArgs e)
        {
            imageEnhanceProcess();
        }

        private void LiveBar_ValueChanged(object sender, EventArgs e)
        {
            // 检查是否在暂停状态下
            if (videoTimer == null || !videoTimer.Enabled)
            {
                // 计算当前 UIScrollBar 对应的帧索引
                int frameIndex = (int)((double)LiveBar.Value / LiveBar.Maximum * totalFrames);
                // 确保帧索引在有效范围内
                if (frameIndex >= 0 && frameIndex <= videoFrames.Count)
                {
                    Console.WriteLine($"frameIndex:{frameIndex}");
                    currentFrameIndex = frameIndex;
                    currentFrame = videoFrames[currentFrameIndex];

                    // 处理并显示当前帧
                    ProcessVideoFrames();
                }
            }
        }

        private double CalculatePixelLength(System.Drawing.Point start, System.Drawing.Point end)
        {
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        //————————工具栏功能 end——————

    }

}