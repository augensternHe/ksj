using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Python.Runtime;

using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using OpenCvSharp;

using Hamamatsu.DCAM4;
using Hamamatsu.subacq4;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using System.Collections.Generic;
using Sunny.UI;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;
using static csAcq4.Form1;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using Sunny.UI.Win32;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.NVRTC;
using System.Text;
using ManagedCuda.VectorTypes;
using System.Collections.Concurrent;


namespace csAcq4
{
    public partial class Form1 : Form
    {

        private Logger logger;//日志记录
        public Form1()
        {
            InitializeComponent();

            this.Font = new System.Drawing.Font("Segoe UI", this.Font.Size); // 设置窗体的默认字体
            {
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
            };

            InitializePython();
            m_image = new MyImage();
            m_lut = new MyLut();
            BitmapLock = new object();

            // 初始时禁用 funGroup 和 saveGroup 中的所有控件
            DisableAllControls(funGroup);
            DisableAllControls(saveGroup);
            InitializeDisplayBtnEvents();//屏幕按钮鼠标点击事件
            //InitializePictureBoxEvents();//屏幕点击事件
            InitializesmallPicShowEvents();//小屏点击事件

            InitializeFontSizeContextMenu();  // 初始化文本字号选择右键菜单
            // 实例化 logger
            logger = new Logger();

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
        }
        private void InitializeDisplayBtnEvents()
        {

            // 为所有按钮添加点击事件
            displayBtn1.MouseDown += new MouseEventHandler(ChangeButtonColor);
            displayBtn2.MouseDown += new MouseEventHandler(ChangeButtonColor);
            displayBtn3.MouseDown += new MouseEventHandler(ChangeButtonColor);
            displayBtn4.MouseDown += new MouseEventHandler(ChangeButtonColor);
            displayBtn5.MouseDown += new MouseEventHandler(ChangeButtonColor);
            displayBtn6.MouseDown += new MouseEventHandler(ChangeButtonColor);
            displayBtn7.MouseDown += new MouseEventHandler(ChangeButtonColor);
            displayBtn8.MouseDown += new MouseEventHandler(ChangeButtonColor);
            displayBtn9.MouseDown += new MouseEventHandler(ChangeButtonColor);
        }

        private void InitializePictureBoxEvents(MouseEventHandler newEventHandler)
        {
            // 如果上次绑定的事件处理函数不为空，先解绑上次的事件处理函数
            if (lastBoundHandler != null)
            {
                UnInitializePictureBoxEvents(lastBoundHandler);
            }

            // 绑定新的事件处理函数到所有 PictureBox
            PicDisplay1.MouseClick += newEventHandler;
            PicDisplay2.MouseClick += newEventHandler;
            PicDisplay3.MouseClick += newEventHandler;
            PicDisplay4.MouseClick += newEventHandler;
            PicDisplay5.MouseClick += newEventHandler;
            PicDisplay6.MouseClick += newEventHandler;
            PicDisplay7.MouseClick += newEventHandler;
            PicDisplay8.MouseClick += newEventHandler;
            PicDisplay9.MouseClick += newEventHandler;

            // 更新 lastBoundHandler，记录本次绑定的处理函数
            lastBoundHandler = newEventHandler;
        }

        // 解绑指定的事件处理函数
        private void UnInitializePictureBoxEvents(MouseEventHandler handlerToUnbind)
        {
            PicDisplay1.MouseClick -= handlerToUnbind;
            PicDisplay2.MouseClick -= handlerToUnbind;
            PicDisplay3.MouseClick -= handlerToUnbind;
            PicDisplay4.MouseClick -= handlerToUnbind;
            PicDisplay5.MouseClick -= handlerToUnbind;
            PicDisplay6.MouseClick -= handlerToUnbind;
            PicDisplay7.MouseClick -= handlerToUnbind;
            PicDisplay8.MouseClick -= handlerToUnbind;
            PicDisplay9.MouseClick -= handlerToUnbind;
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

        private enum FormStatus
        {
            Startup,                // After startup or camapi_uninit()
            Initialized,            // After dcamapi_init() or dcamdev_close()
            Opened,                 // After dcamdev_open() or dcamcap_stop() without any image
            Acquiring,              // After dcamcap_start()
            AcquiringSoftwareTrigger,
            Acquired                // After dcamcap_stop() with image
        }

        private MyDcam mydcam;
        private MyDcamApi mydcamapi;
        private MyDcamWait mydcamwait;
        private MyImage m_image;
        private MyLut m_lut;
        private readonly object BitmapLock;
        private Mat originalFrame;  // 用于存储原始未处理的图像帧
        private struct MyLut
        {
            public int camerabpp;          // camera bit per pixel.  This sample code only support MONO.
            public int cameramax;

            public int inmax;
            public int inmin;
        };

        private int m_indexCamera = 0;          // DCAM设备编码
        private int m_nFrameCount;              // DCAM 捕获分配缓冲区的帧数
        private FormStatus m_formstatus;        // 指示当前表单状态。设置时使用 MyFormStatus() 函数
        private Thread m_threadCapture;         // 用于在捕获期间监控更新的帧
        private Bitmap m_bitmap;                // 用于在此窗体中显示的位图数据
        private bool m_cap_stopping = false;    // 开始捕获时重置为 false，停止捕获时设置为 true

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
        private bool isVideoPlaying=false;
        private void InitializePython()
        {
            string pythonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "python-embed");
            Environment.SetEnvironmentVariable("PYTHONHOME", pythonPath);
            Environment.SetEnvironmentVariable("PYTHONPATH", Path.Combine(pythonPath, "Lib\\site-packages"));

            // 添加这行
            Runtime.PythonDLL = Path.Combine(pythonPath, "python311.dll");  // 替换 X 为你的 Python 版本，例如 python39.dll

            PythonEngine.PythonHome = pythonPath;
            PythonEngine.Initialize();
        }
        // 初始化定时器
        private void InitializeFrameRateTimer()
        {
            frameRateTimer = new System.Windows.Forms.Timer();
            frameRateTimer.Interval = 1000; // 每秒更新一次
            frameRateTimer.Tick += FrameRateTimer_Tick;
            frameRateTimer.Start();
        }

        // ---------------- 私有类----------------
        // ---------------- 图片操作类----------------
        private class MyImage
        {
            public DCAMBUF_FRAME bufframe;
            public MyImage()
            {
                bufframe = new DCAMBUF_FRAME(0);
            }
            public int width { get { return bufframe.width; } }
            public int height { get { return bufframe.height; } }
            public DCAM_PIXELTYPE pixeltype { get { return bufframe.type; } }
            public bool isValid()
            {
                if (width <= 0 || height <= 0 || pixeltype == DCAM_PIXELTYPE.NONE)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            public void clear()
            {
                bufframe.width = 0;
                bufframe.height = 0;
                bufframe.type = DCAM_PIXELTYPE.NONE;
            }
            public void set_iFrame(int index)
            {
                bufframe.iFrame = index;
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

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Initialize form status
            MyFormStatus_Startup();
            m_nFrameCount = 3;

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
            m_lut = new MyLut
            {
                camerabpp = 16, // 或其他适当的值
                cameramax = (1 << 16) - 1,
                inmax = (1 << 16) - 1,
                inmin = 0
            };
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

            // 为 PicDisplay1 到 PicDisplay9 添加 MouseEnter 和 MouseLeave 事件
            for (int i = 1; i <= 9; i++)
            {
                var picDisplay = this.Controls.Find($"PicDisplay{i}", true).FirstOrDefault() as PictureBox;
                if (picDisplay != null)
                {
                    picDisplay.MouseEnter += PicDisplay_MouseEnter;
                    picDisplay.MouseLeave += PicDisplay_MouseLeave;
                }
            }

            middlePanel.Resize += MiddlePanel_Resize;
            MiddlePanel_Resize(null, null);
            initCudaProceImage();
        }
        // 初始化 CUDA 上下文和内核，只执行一次
        private void initCudaProceImage()
        {
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
                int newValue = (int)(yValue * originalValue);
                newValue = max(0, min(newValue, 255));

                outputImage[idx] = static_cast<unsigned char>(newValue);
            }
        }";

            CudaRuntimeCompiler rtc = new CudaRuntimeCompiler(cudaKernelCode, "ProcessImageKernel");
            rtc.Compile(new string[] { });
            CUmodule cudaModule = cudaContext.LoadModulePTX(rtc.GetPTX());
            processImageKernel = new CudaKernel("ProcessImageKernel", cudaModule, cudaContext);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyThreadCapture_Abort();                // abort capturing thread if exist
            exitToolStripMenuItem_Click(this, EventArgs.Empty);

            if (mydcam != null)                     // close device if exist
            {
                mydcam.dev_close();
                mydcam = null;
            }

            if (!IsMyFormStatus_Startup())          // uninitialize if necessary
            {
                MyDcamApi.uninit();
            }
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
        private void MyFormStatus(FormStatus status)
        {
            Boolean isStartup = (status == FormStatus.Startup);
            Boolean isInitialized = (status == FormStatus.Initialized);
            Boolean isOpened = (status == FormStatus.Opened);
            Boolean isAcquiring = (status == FormStatus.Acquiring) || (status == FormStatus.AcquiringSoftwareTrigger);
            Boolean isAcquired = (status == FormStatus.Acquired);
            Boolean isAcquiringSoftwareTrigger = (status == FormStatus.AcquiringSoftwareTrigger);
            //PushInit.Enabled = isStartup;
            //PushOpen.Enabled = isInitialized;

            if (isInitialized || isOpened)
            {
                // acquisition is not starting
                MyThreadCapture_Abort();
            }

            m_formstatus = status;
        }
        private void MyFormStatus_Startup() { MyFormStatus(FormStatus.Startup); }
        private void MyFormStatus_Initialized() { MyFormStatus(FormStatus.Initialized); }
        private void MyFormStatus_Opened() { MyFormStatus(FormStatus.Opened); }
        private void MyFormStatus_Acquiring() { MyFormStatus(FormStatus.Acquiring); }
        private void MyFormStatus_AcquiringSoftwareTrigger() { MyFormStatus(FormStatus.AcquiringSoftwareTrigger); }
        private void MyFormStatus_Acquired() { MyFormStatus(FormStatus.Acquired); }

        private Boolean IsMyFormStatus_Startup() { return (m_formstatus == FormStatus.Startup); }
        private Boolean IsMyFormStatus_Initialized() { return (m_formstatus == FormStatus.Initialized); }
        private Boolean IsMyFormStatus_Opened() { return (m_formstatus == FormStatus.Opened); }
        private Boolean IsMyFormStatus_Acquiring() { return (m_formstatus == FormStatus.Acquiring) || (m_formstatus == FormStatus.AcquiringSoftwareTrigger); }
        private Boolean IsMyFormStatus_Acquired() { return (m_formstatus == FormStatus.Acquired); }
        private void MyShowStatus(string text) { bottomLabelRight.Text = text; }
        private void MyShowStatusOK(string text) { MyShowStatus("OK: " + text); }
        private void MyShowStatusNG(string text, DCAMERR err)
        {
            MyShowStatus(String.Format("NG: 0x{0:X8}:{1},failed", (int)err, text));
        }
        //------------------状态判断 end---------------

        // ---------------- 设备读取播放功能 ----------------
        private void pushLive_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("pushLive_Click");

            m_image = new MyImage();
            if (mydcam == null)
            {
                MyFormStatus_Initialized();     // FormStatus should be Initialized.
                return;                         // internal error
            }
            string text = "";
            if (IsMyFormStatus_Opened())
            {
                // if FormStatus is Opened, DCAM buffer is not allocated.
                // So call dcambuf_alloc() to prepare capturing.

                text = string.Format("dcambuf_alloc({0})", m_nFrameCount);
                // allocate frame buffer
                if (!mydcam.buf_alloc(m_nFrameCount))
                {

                    // allocation was failed
                    return;                     // Fail: dcambuf_alloc()
                }

                // Success: dcambuf_alloc()
            }

            // start acquisition
            m_cap_stopping = false;
            mydcam.m_capmode = DCAMCAP_START.SEQUENCE;    // continuous capturing.  continuously acqusition will be done
            if (!mydcam.cap_start())
            {

                // acquisition was failed. In this sample, frame buffer is also released.
                mydcam.buf_release();           // release unnecessary buffer in DCAM
                MyFormStatus_Opened();          // change dialog FormStatus to Opened
                return;                         // Fail: dcamcap_start()
            }

            // Success: dcamcap_start()
            // acquisition has started

            if (text.Length > 0)
            {
                text += " && ";
            }

            MyDcamProp prop = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERSOURCE);
            double v = 0;
            prop.getvalue(ref v);

            if (v == DCAMPROP.TRIGGERSOURCE.SOFTWARE)
            {
                MyFormStatus_AcquiringSoftwareTrigger(); // change dialog FormStatus to AcquiringSoftwareTrigger
            }
            else
            {
                MyFormStatus_Acquiring();           // change dialog FormStatus to Acquiring
            }
            MyThreadCapture_Start();            // start monitoring thread
            // 启用 saveGroup 中的控件
            EnableAllControls(saveGroup);

            InitializeFrameRateTimer();

            pushLive.Enabled = false;
            stopLive.Enabled = true;
            pixelCombox.Enabled = false;
            isVideoPlaying = false;
        }

        private void stopLive_Click(object sender, EventArgs e)
        {
            DCAMERR err = dcamcap.stop(mydcam.m_hdcam);
            if (err.failed())
            {
                MessageBox.Show("停止播放失败");
                return;
            }
            stopLive.Enabled = false;
            pushLive.Enabled = true;

            MyShowStatusOK("播放已停止");
        }

        private void UpdateBitsPerChannel()
        {
            DCAMPROP_ATTR attr = new DCAMPROP_ATTR(DCAMIDPROP.BITSPERCHANNEL);
            // 获取属性信息
            DCAMERR err = dcamprop.getattr(mydcam.m_hdcam, ref attr);
            if (err.failed())
            {
                MessageBox.Show($"获取位深度属性失败: {err.text()}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 清空下拉菜单
            pixelCombox.Items.Clear();
            //Console.WriteLine($"{attr.valuemin}----{attr.valuemax}");
            // 遍历可用的位深度选项
            double bitDepthValue = attr.valuemin;
            bool has8BitOption = false;
            //double currentValue = 0;

            //err = dcamprop.getvalue(mydcam.m_hdcam, DCAMIDPROP.BITSPERCHANNEL, ref currentValue);
            //Console.WriteLine($"当前：{currentValue}");
            while (bitDepthValue <= attr.valuemax)
            {
                if (!err.failed())  // 如果成功设置该值
                {
                    int bitDepth = (int)bitDepthValue;

                    // 检查是否已经包含8-bit
                    if (bitDepth == 8)
                    {
                        has8BitOption = true;
                        pixelCombox.Items.Insert(0, bitDepth);  // 确保8-bit在第一个位置
                    }
                    else
                    {
                        pixelCombox.Items.Add(bitDepth);  // 添加到下拉框中
                    }
                }

                bitDepthValue += attr.valuestep;  // 递增以遍历所有可能的位深度
            }

            // 如果没有8-bit选项，则添加8-bit
            if (!has8BitOption)
            {
                pixelCombox.Items.Insert(0, 8);
            }
            pixelCombox.SelectedItem = 8;
            //// 将当前位深度设置为默认的8-bit
            //pixelCombox.SelectedItem = 8;

            //// 设置默认的8-bit为实际的位深度
            //err = dcamprop.setvalue(mydcam.m_hdcam, DCAMIDPROP.IMAGE_PIXELTYPE, (int)DCAM_PIXELTYPE.MONO8);
            ////err = dcamprop.setvalue(mydcam.m_hdcam, DCAMIDPROP.BITSPERCHANNEL, 8);
            //if (err.failed())
            //{
            //    MessageBox.Show($"默认设置位深度为8-bit失败: {err.text()}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }


        private void pixelCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedBitDepth = (int)pixelCombox.SelectedItem;
            MyShowStatusOK($"成功将位深度设置为 {selectedBitDepth}-bit");
            //if (pixelCombox.SelectedItem == null) return;

            //int selectedBitDepth = (int)pixelCombox.SelectedItem;
            //DCAMERR err;

            //// 根据选择的位深度设置相应的 PIXELTYPE
            //switch (selectedBitDepth)
            //{
            //    case 8:
            //        err = dcamprop.setvalue(mydcam.m_hdcam, DCAMIDPROP.IMAGE_PIXELTYPE, (int)DCAM_PIXELTYPE.MONO8);
            //        break;
            //    case 12:
            //        err = dcamprop.setvalue(mydcam.m_hdcam, DCAMIDPROP.IMAGE_PIXELTYPE, (int)DCAM_PIXELTYPE.MONO16);
            //        break;
            //    case 16:
            //        err = dcamprop.setvalue(mydcam.m_hdcam, DCAMIDPROP.IMAGE_PIXELTYPE, (int)DCAM_PIXELTYPE.MONO16);
            //        break;
            //    default:
            //        return; // 不支持的位深度
            //}

            //// 检查是否设置 PIXELTYPE 成功
            //if (err.failed())
            //{
            //    MessageBox.Show($"设置像素类型失败: {err.text()}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //// 设置位深度属性为用户选择的位深度
            //err = dcamprop.setvalue(mydcam.m_hdcam, DCAMIDPROP.BITSPERCHANNEL, selectedBitDepth);
            //Console.WriteLine($"err:{err.text()}");
            //if (err.failed())
            //{
            //    MessageBox.Show($"设置位深度为 {selectedBitDepth}-bit 失败: {err.text()}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //else
            //{
            //    Console.WriteLine($"成功将位深度设置为 {selectedBitDepth}-bit");
            //}
        }

        private void UpdateExposureTime()
        {
            DCAMPROP_ATTR attr = new DCAMPROP_ATTR(DCAMIDPROP.EXPOSURETIME);

            // 获取属性信息
            DCAMERR err = dcamprop.getattr(mydcam.m_hdcam, ref attr);
            if (err.failed())
            {
                MessageBox.Show($"获取曝光时间属性失败: {err.text()}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 获取最小值、最大值和步进值
            double minExposureTime = attr.valuemin;
            double maxExposureTime = attr.valuemax;
            double stepExposureTime = attr.valuestep;

            // 获取当前设置的曝光时间
            double currentExposureTime = 0;
            err = dcamprop.getvalue(mydcam.m_hdcam, DCAMIDPROP.EXPOSURETIME, ref currentExposureTime);
            if (err.failed())
            {
                MessageBox.Show($"获取当前曝光时间失败: {err.text()}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            exposureNum.Minimum = (decimal)minExposureTime;
            exposureNum.Maximum = (decimal)maxExposureTime;
            exposureNum.Value = (decimal)currentExposureTime;
        }

        private void UpdateFrameRate()
        {

            DCAMPROP_ATTR attr = new DCAMPROP_ATTR(DCAMIDPROP.INTERNALFRAMERATE);

            // 获取属性信息
            DCAMERR err = dcamprop.getattr(mydcam.m_hdcam, ref attr);
            if (err.failed())
            {
                MessageBox.Show($"获取帧率属性失败: {err.text()}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
                        
            err = dcamprop.getvalue(mydcam.m_hdcam, DCAMIDPROP.INTERNALFRAMERATE, ref currentFrameRate);
            if (err.failed())
            {
                MessageBox.Show($"获取当前曝光时间失败: {err.text()}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bottomLabelMiddle.Text = $"currentFrameRate:{(decimal)currentFrameRate}";
        }

        private void MyUpdateDisplay()
        {
            if (m_bitmap != null)
            {
                Image oldImg = PicDisplay1.Image;
                //PicDisplay1.Image = new Bitmap(m_bitmap);//原始整张图显示方案
                Mat matFrame = OpenCvSharp.Extensions.BitmapConverter.ToMat(m_bitmap);//九宫格切割
                currentFrame = matFrame;
                // 保存当前的原始帧
                originalFrame = matFrame.Clone();
                // 使用Task.Run在后台执行异步函数，不会阻塞主线程
                Task.Run(async () =>
                {
                    await ProcessVideoFramesAsync();
                });
                //PicDisplay1.Refresh();
                if (oldImg != null)
                    oldImg.Dispose();
            }
            else
            {
                PicDisplay1.Image = null;
            }
        }

        private delegate void MyDelegate_SnapCaptureFinished();
        private void MyUpdatePicture()
        {
            if (m_image.isValid())
            {
                Rectangle rc = new Rectangle(0, 0, m_image.width, m_image.height);
                lock (BitmapLock)
                {
                    Bitmap oldBitmap = m_bitmap;

                    m_bitmap = new Bitmap(m_image.width, m_image.height, PixelFormat.Format24bppRgb);
                    SUBACQERR err = subacq.copydib(ref m_bitmap, m_image.bufframe, ref rc, m_lut.inmax, m_lut.inmin, m_lut.camerabpp);
                    
                    if (oldBitmap != null)
                        oldBitmap.Dispose();
                }

            }

            if (InvokeRequired)
            {
                // worker thread calls this function
                // only display drawing needs to be called from Main Thread
                Invoke(new MyDelegate_UpdateDisplay(MyUpdateDisplay));
            }
            else
            {
                MyUpdateDisplay();
            }

            if (isRecording && videoWriter != null && videoWriter.IsOpened())
            {
                double actualFrameRate = GetActualFrameRate();
                using (Mat frame = OpenCvSharp.Extensions.BitmapConverter.ToMat(m_bitmap))
                {
                    for (int i = 0; i < frameRate / actualFrameRate; i++)
                    {
                        videoWriter.Write(frame);
                    }
                }
            }
        }
        private void MySnapCaptureFinished()
        {
            if (InvokeRequired)
            {
                // worker thread calls this function
                Invoke(new MyDelegate_SnapCaptureFinished(MySnapCaptureFinished), null);
                return;
            }
            MyFormStatus_Acquired();            // change dialog FormStatus to Acquired
        }

        private void OnThreadCapture()
        {
            bool bContinue = true;

            using (mydcamwait = new MyDcamWait(ref mydcam))
            {
                while (bContinue)
                {
                    DCAMWAIT eventmask = DCAMWAIT.CAPEVENT.FRAMEREADY | DCAMWAIT.CAPEVENT.STOPPED;
                    DCAMWAIT eventhappened = DCAMWAIT.NONE;
                    if (mydcamwait.start(eventmask, ref eventhappened))
                    {
                        if (eventhappened & DCAMWAIT.CAPEVENT.FRAMEREADY)
                        {
                            int iNewestFrame = 0;
                            int iFrameCount = 0;

                            if (mydcam.cap_transferinfo(ref iNewestFrame, ref iFrameCount))
                            {
                                m_image.set_iFrame(iNewestFrame);
                                if (mydcam.buf_lockframe(ref m_image.bufframe))
                                {

                                    // 更新并显示图像
                                    MyUpdatePicture();
                                }
                            }
                        }

                        if (eventhappened & DCAMWAIT.CAPEVENT.STOPPED)
                        {
                            bContinue = false;
                            if (m_cap_stopping == false && mydcam.m_capmode == DCAMCAP_START.SNAP)
                            {
                                // in this condition, cap_stop() happens automatically, so update the main dialog
                                MySnapCaptureFinished();
                            }
                        }
                    }
                    else
                    {
                        if (mydcamwait.m_lasterr == DCAMERR.TIMEOUT)
                        {
                            // nothing to do
                        }
                        else
                        if (mydcamwait.m_lasterr == DCAMERR.ABORT)
                        {
                            bContinue = false;
                        }
                    }
                }
            }
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
        private readonly object cudaLock = new object(); // 用于确保线程安全
        private BlockingCollection<Mat> frameQueue = new BlockingCollection<Mat>(); // 阻塞队列

        // 专门处理 CUDA 操作的线程
        private void StartProcessingThread()
        {
            Task.Run(() =>
            {
                while (!frameQueue.IsCompleted)
                {
                    Mat frame;
                    if (frameQueue.TryTake(out frame, Timeout.Infinite))
                    {
                        lock (cudaLock) // 确保线程安全
                        {
                            cudaContext.SetCurrent(); // 确保当前 CUDA 上下文已经被激活
                            ProcessImageWithCuda(frame); // 处理图像
                        }
                    }
                }
            });
        }

        // 将帧加入队列供处理线程处理
        public void EnqueueFrameForProcessing(Mat frame)
        {
            frameQueue.Add(frame);
        }

        public Mat ProcessImageWithCuda(Mat img)
        {
            lock (cudaLock)
            {
                // 绑定 CUDA 上下文到当前线程
                cudaContext.SetCurrent();

                if (img.Empty() || img.Type() != MatType.CV_8UC1)
                    throw new ArgumentException("图像应为灰度图且不为空。");

                int width = img.Cols;
                int height = img.Rows;
                int blockWidth = width / 8;
                int blockHeight = height / 8;

                Mat processedImg = new Mat(height, width, MatType.CV_8UC1);

                for (int by = 0; by < 8; by++)
                {
                    for (int bx = 0; bx < 8; bx++)
                    {
                        Rect roi = new Rect(bx * blockWidth, by * blockHeight, blockWidth, blockHeight);
                        Mat block = new Mat(img, roi);

                        Mat processedBlock = ProcessBlockWithCuda(block, blockWidth, blockHeight);
                        processedBlock.CopyTo(new Mat(processedImg, roi));
                    }
                }

                return processedImg;
            }
        }

        private Mat ProcessBlockWithCuda(Mat block, int blockWidth, int blockHeight)
        {
            cudaContext.SetCurrent(); // 确保当前线程绑定 CUDA 上下文

            int numPixels = blockWidth * blockHeight;
            int centerX = blockWidth / 2;
            int centerY = blockHeight / 2;

            byte[] inputArray = new byte[numPixels];
            Marshal.Copy(block.Data, inputArray, 0, numPixels);

            using (CudaDeviceVariable<byte> deviceInputImage = new CudaDeviceVariable<byte>(numPixels))
            using (CudaDeviceVariable<byte> deviceOutputImage = new CudaDeviceVariable<byte>(numPixels))
            {
                deviceInputImage.CopyToDevice(inputArray);

                uint threadsPerBlockX = 16;
                uint threadsPerBlockY = 16;
                uint blocksPerGridX = (uint)((blockWidth + threadsPerBlockX - 1) / threadsPerBlockX);
                uint blocksPerGridY = (uint)((blockHeight + threadsPerBlockY - 1) / threadsPerBlockY);

                processImageKernel.BlockDimensions = new ManagedCuda.VectorTypes.dim3(threadsPerBlockX, threadsPerBlockY, 1);
                processImageKernel.GridDimensions = new ManagedCuda.VectorTypes.dim3(blocksPerGridX, blocksPerGridY, 1);

                // 执行 CUDA 内核
                processImageKernel.Run(deviceInputImage.DevicePointer, deviceOutputImage.DevicePointer, blockWidth, blockHeight, centerX, centerY);

                byte[] outputArray = new byte[numPixels];
                deviceOutputImage.CopyToHost(outputArray);

                Mat processedBlock = new Mat(blockHeight, blockWidth, MatType.CV_8UC1);
                Marshal.Copy(outputArray, 0, processedBlock.Data, numPixels);

                return processedBlock;
            }
        }


        // 主函数：处理视频帧
        private async Task ProcessVideoFramesAsync()
        {
            groupedFrames = new Dictionary<char, Mat>();

            if (currentFrame == null || currentFrame.Empty())
            {
                MessageBox.Show("没有可用的图像帧进行处理。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 异步处理帧
            await Task.Run(() =>
            {
                // 1. 转换为灰度图像
                ConvertFrameToGrayscale(ref currentFrame);

                // 2. 应用背景减除
                if (isBackgroundSubtractionEnabled && backgroundImage != null)
                {
                    ApplyBackgroundSubtractionToFrame(ref currentFrame); // 可以进一步优化为CUDA版本
                }

                // 3. 使用CUDA处理图像
                currentFrame = ProcessImageWithCuda(currentFrame);

                // 4. 处理裁剪与拼接
                PerformCroppingAndStitching();
            });

            // 处理完后显示结果
            ShowGroupFrames();
        }

        // 背景减除函数
        private void ApplyBackgroundSubtractionToFrame(ref Mat currentFrame)
        {
            // 将 backgroundImage 转换为 Mat
            Mat backgroundMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(backgroundImage);

            if (backgroundMat.Size() == currentFrame.Size())
            {
                // 检查是否为三通道图像
                if (backgroundMat.Channels() == 3)
                {
                    Console.WriteLine("背景图像是三通道 (彩色)，正在转换为单通道 (灰度)。");
                    Cv2.CvtColor(backgroundMat, backgroundMat, ColorConversionCodes.BGR2GRAY); // 转换为灰度图像
                }
                else
                {
                    Console.WriteLine("背景图像已经是单通道 (灰度)。");
                }
                // 计算差异并更新当前帧
                Cv2.Absdiff(currentFrame, backgroundMat, currentFrame);
                MyShowStatusOK("背景减除已应用于当前帧");
            }
            else
            {
                MessageBox.Show("背景图像尺寸与当前帧不匹配，无法应用背景减除。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 裁剪与拼接操作
        private void PerformCroppingAndStitching()
        {
            List<string> xCoords = columnArrays[0];
            List<string> yCoords = columnArrays[1];
            List<string> widths = columnArrays[2];
            List<string> heights = columnArrays[3];
            List<string> positions = columnArrays[4];

            int outputWidth = int.Parse(widths[0]) * 6;
            int outputHeight = int.Parse(heights[0]) * 6;

            // 使用Parallel.For进行并行裁剪与拼接
            Parallel.For(0, xCoords.Count, i =>
            {
                int x = int.Parse(xCoords[i]);
                int y = int.Parse(yCoords[i]);
                int w = int.Parse(widths[i]);
                int h = int.Parse(heights[i]);

                // 裁剪当前帧的指定区域
                Rect roi = new Rect(x, y, w, h);
                Mat croppedFrame = new Mat(currentFrame, roi);
                string position = positions[i];
                char groupLetter = position.Last();
                int gridPosition = int.Parse(position.Substring(0, position.Length - 1));

                // 计算拼接的行和列位置
                int row = (gridPosition - 1) / 6;
                int col = (gridPosition - 1) % 6;
                Rect gridRoi = new Rect(col * w, row * h, w, h);

                lock (groupedFrames)
                {
                    // 初始化拼接图像
                    if (!groupedFrames.ContainsKey(groupLetter))
                    {
                        groupedFrames[groupLetter] = new Mat(outputHeight, outputWidth, MatType.CV_8UC1, Scalar.All(0));
                    }

                    // 检查目标矩形是否超出拼接图像的范围
                    if (gridRoi.X + gridRoi.Width <= groupedFrames[groupLetter].Cols &&
                        gridRoi.Y + gridRoi.Height <= groupedFrames[groupLetter].Rows)
                    {
                        // 将裁剪的图像复制到拼接图像中
                        croppedFrame.CopyTo(groupedFrames[groupLetter][gridRoi]);
                    }
                }
            });
        }

        // 显示拼接结果
        private void ShowGroupFrames()
        {
            PictureBox[] pictureBoxes = { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 };
            PictureBox[] smallPicBoxes = { smallShow1, smallShow2, smallShow3, smallShow4, smallShow5, smallShow6, smallShow7, smallShow8, smallShow9 };

            // 异步更新显示
            Task.Run(() =>
            {
                for (int i = 0; i < pictureBoxes.Length; i++)
                {
                    int index = i;
                    char groupLetter = (char)('A' + index);

                    if (groupedFrames.ContainsKey(groupLetter))
                    {
                        Mat histMat = groupedFrames[groupLetter];

                        // 缩放图片
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

                        // 应用伪彩色或直接显示
                        Bitmap pseMap = pseudoColorEnabled.ContainsKey(index) && pseudoColorEnabled[index] && pseudoColorButtonColors.ContainsKey(index)
                            ? ApplyPseudoColor(bitmap, pseudoColorButtonColors[index])
                            : bitmap;

                        pictureBoxes[index].Invoke((MethodInvoker)(() =>
                        {
                            pictureBoxes[index].Image = pseMap;
                            pictureBoxes[index].Refresh();
                        }));

                        smallPicBoxes[index].Invoke((MethodInvoker)(() =>
                        {
                            //smallPicBoxes[index].Image = bitmap;
                            //smallPicBoxes[index].Refresh();
                        }));
                    }
                }
            });
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
                pictureBoxScales[currentHoverPictureBox] *= scaleFactor;
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
        private double GetActualFrameRate()
        {
            double actualFrameRate = 0;
            DCAMERR err = dcamprop.getvalue(mydcam.m_hdcam, DCAMIDPROP.INTERNALFRAMERATE, ref actualFrameRate);
            if (err.failed())
            {
                actualFrameRate = 30; // 如果获取失败，使用默认值
            }
            return actualFrameRate;
        }
        // Capturing Thread helper functions
        private void MyThreadCapture_Start()
        {
            m_threadCapture = new Thread(new ThreadStart(OnThreadCapture));

            m_threadCapture.IsBackground = true;
            m_threadCapture.Start();
        }
        void MyThreadCapture_Abort()
        {
            if (m_threadCapture != null)
            {
                if (mydcamwait != null)
                    mydcamwait.abort();

                m_threadCapture.Abort();
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

            // 更新相机的曝光时间
            DCAMERR err = dcamprop.setvalue(mydcam.m_hdcam, DCAMIDPROP.EXPOSURETIME, newExposureTime);
            if (err.failed())
            {
                MessageBox.Show($"Failed to update camera exposure time: {err.text()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MyShowStatusOK($"Camera exposure time updated to: {newExposureTime}");
            }
        }
        // 定时器的 Tick 事件处理程序
        private void FrameRateTimer_Tick(object sender, EventArgs e)
        {
            if (mydcam == null)
            {
                MyShowStatusOK("Camera not initialized.");
                return; // 提前返回，防止空引用错误
            }

            double currentFrameRate = 0;
            DCAMERR err = dcamprop.getvalue(mydcam.m_hdcam, DCAMIDPROP.INTERNALFRAMERATE, ref currentFrameRate);

            if (err.failed())
            {
                MyShowStatusOK("Failed to retrieve frame rate.");
            }
            else
            {
                bottomLabelMiddle.Text = $"currentFrameRate:{(decimal)currentFrameRate}";
            }
        }

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

        private void SaveCurrentImage()
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
                MyShowStatusOK($"图像成功保存：{filename}");
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
        }

        private async void shootStart_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("shootStart_Click");
            int delay = (int)delayVal.Value * 1000; // 将秒转换为毫秒
            int interval = (int)periodVal.Value * 1000; // 将秒转换为毫秒
            int totalShots = (int)numVal.Value;

            if (totalShots <= 0)
            {
                MessageBox.Show("请设置有效的拍摄次数。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 禁用开始按钮，防止重复点击
            shootStart.Enabled = false;

            // 创建取消令牌源
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                if (delay > 0)
                {
                    MyShowStatusOK($"连拍将在 {delay / 1000} 秒后开始");
                    await Task.Delay(delay, cancellationTokenSource.Token);
                }

                for (int i = 0; i < totalShots; i++)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    SaveCurrentImage();
                    //Console.WriteLine($"已拍摄 {i + 1}/{totalShots} 张");

                    if (i < totalShots - 1) // 不是最后一张时才等待
                    {
                        await Task.Delay(interval, cancellationTokenSource.Token);
                    }
                }

                MyShowStatusOK($"连拍完成！共拍摄 {totalShots} 张照片。");
            }
            catch (OperationCanceledException)
            {
                MyShowStatusOK("连拍已取消。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连拍过程中发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 重新启用开始按钮
                shootStart.Enabled = true;
                cancellationTokenSource.Dispose();
            }
        }

        // 添加一个取消连拍的方法
        private void shootCancel_Click(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }

        //--------视频录制---------------
        private string recordedVideoPath; // 用于存储录制的视频路径

        private void StartRecording()
        {
            if (m_bitmap == null)
            {
                MessageBox.Show("No image available for recording.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(m_savePath))
            {
                MessageBox.Show("Please select a save directory first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frameWidth = m_bitmap.Width;
            frameHeight = m_bitmap.Height;
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
            MyShowStatusOK($"Started recording video: {recordedVideoPath}");
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
            MyShowStatusOK("Video recording stopped");

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
            m_cap_stopping = true;
            if (mydcam != null)
            {
                mydcam.cap_stop();
            }
            MyThreadCapture_Abort();

            if (mydcam != null)
            {
                mydcam.buf_release();
            }

            MyFormStatus_Opened();
            MyShowStatusOK("视频显示已停止");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("exitToolStripMenuItem_Click");
            // 停止视频显示
            if (IsMyFormStatus_Acquiring())
            {
                StopVideoDisplay();
                Console.WriteLine("StopVideoDisplay");
            }
            // 关闭相机
            if (mydcam != null)
            {
                CloseCameraDevice();
                Console.WriteLine("CloseCameraDevice");
            }
            if (!MyDcamApi.uninit())
            {
                MyShowStatusNG("dcamapi_uninit()", MyDcamApi.m_lasterr);
                return;                         // Fail: dcamapi_uninit()
            }

            // Success: dcamapi_uninit()

            MyShowStatusOK("dcamapi_uninit()");

            // 禁用两个控件组
            DisableAllControls(funGroup);
            DisableAllControls(saveGroup);

            // 重置表单状态
            MyFormStatus_Startup();

            MyShowStatusOK("已退出相机操作");
        }

        // ---------------- 左侧窗体图片存储 end-----------

        // ----------------主体屏幕处理功能----------
        // 处理屏幕点击事件
        private void PicDisplay_MouseEnter(object sender, EventArgs e)
        {
            // 获取触发事件的 PictureBox 控件
            PictureBox picDisplay = sender as PictureBox;

            if (picDisplay != null)
            {
                // 提取 PicDisplay 的最后一个数字
                string lastDigit = picDisplay.Name.Substring(picDisplay.Name.Length - 1);

                // 拼接对应的 displayBtn 名称
                string btnName = "displayBtn" + lastDigit;

                // 查找对应的 displayBtn 控件
                UISymbolButton displayBtn = this.Controls.Find(btnName, true).FirstOrDefault() as UISymbolButton;

                // 设置 displayBtn 的 FillColor 为 FillColor2
                if (displayBtn != null)
                {
                    displayBtn.FillColor = displayBtn.FillColor2;
                }
                else
                {
                    MessageBox.Show($"未找到按钮: {btnName}");
                }
            }
        }

        private void PicDisplay_MouseLeave(object sender, EventArgs e)
        {
            // 获取触发事件的 PictureBox 控件
            PictureBox picDisplay = sender as PictureBox;

            if (picDisplay != null)
            {
                // 提取 PicDisplay 的最后一个数字
                string lastDigit = picDisplay.Name.Substring(picDisplay.Name.Length - 1);

                // 拼接对应的 displayBtn 名称
                string btnName = "displayBtn" + lastDigit;

                // 查找对应的 displayBtn 控件
                UISymbolButton displayBtn = this.Controls.Find(btnName, true).FirstOrDefault() as UISymbolButton;

                // 恢复 displayBtn 的 FillColor 为 Silver
                if (displayBtn != null)
                {
                    displayBtn.FillColor = System.Drawing.Color.Silver;
                }
                else
                {
                    MessageBox.Show($"未找到按钮: {btnName}");
                }
            }
        }
        private void PictureBox_Click(object sender, EventArgs e)
        {
            PictureBox clickedPictureBox = sender as PictureBox;

            if (clickedPictureBox != null)
            {
                // 获取 PictureBox 的名称
                string pictureBoxName = clickedPictureBox.Name;

                // 提取最后一位数字
                string lastDigit = pictureBoxName.Substring(pictureBoxName.Length - 1);

                // 拼接成按钮的名称
                string btnName = "displayBtn" + lastDigit;

                // 查找按钮控件
                UISymbolButton btn = this.Controls.Find(btnName, true).FirstOrDefault() as UISymbolButton;

                // 获取按钮的颜色
                if (btn != null)
                {
                    System.Drawing.Color btnColor = btn.SymbolColor;
                    // 输出或使用颜色
                    fullPicBtn.SymbolColor = btnColor;
                }
                else
                {
                    MessageBox.Show("未找到按钮: " + btnName);
                }

                // 记录当前 FullDisplay 显示的 PictureBox
                currentFullDisplayPictureBox = clickedPictureBox;
                Console.WriteLine(clickedPictureBox.Name);
                // 将点击的PictureBox中的图像显示在fullDisplay中
                FullDisPlay.Image = clickedPictureBox.Image;
                // 设置fullDisplay的SizeMode为StretchImage以填充控件
                FullDisPlay.SizeMode = PictureBoxSizeMode.StretchImage;
                FullDisPlay.Refresh();

                // 将 fullDisplay 置于顶层
                FullDisPlay.BringToFront();
                fullPicBtn.BringToFront();
            }
        }

        private void smallPic_Click(object sender, EventArgs e)
        {
            PictureBox clickedPictureBox = sender as PictureBox;

            if (clickedPictureBox != null)
            {
                // 提取 Name 中的数字部分
                string smallNumber = Regex.Match(clickedPictureBox.Name, @"\d+").Value;
                String picShowName = "PicDisplay" + smallNumber;
                // 将点击的PictureBox中的图像显示在fullDisplay中
                System.Windows.Forms.Control foundControl = this.Controls.Find(picShowName, true).FirstOrDefault();

                if (foundControl != null && foundControl is PictureBox)
                {
                    // 将对应 PictureBox 的 Image 赋值给 fullDisplay
                    PictureBox picShow = foundControl as PictureBox;
                    FullDisPlay.Image = picShow.Image;

                    // 设置 fullDisplay 的 SizeMode 为 StretchImage 以充满控件
                    FullDisPlay.SizeMode = PictureBoxSizeMode.StretchImage;

                    // 将 fullDisplay 置于顶层
                    FullDisPlay.BringToFront();
                    fullPicBtn.BringToFront();

                }
                else
                {
                    MessageBox.Show("找不到控件或控件不是 PictureBox");
                }
            }
        }

        // 获取当前选中的 PictureBox
        private PictureBox GetSelectedPictureBox(string pictureBoxName)
        {
            System.Windows.Forms.Control[] controls = this.Controls.Find(pictureBoxName, true);
            if (controls.Length > 0 && controls[0] is PictureBox)
            {
                return controls[0] as PictureBox;
            }
            return null;
        }

        private void ChangeButtonColor(object sender, MouseEventArgs e)
        {
            // 判断是否为左键点击
            if (e.Button == MouseButtons.Left)
            {
                if (sender is UISymbolButton button)
                {
                    dynamic btn = button;
                    System.Drawing.Color buttonColor = btn.SymbolColor;
                    // 获取 RGB 值
                    //int red = buttonColor.R;
                    //int green = buttonColor.G;
                    //int blue = buttonColor.B;

                    // 提取 Name 中的数字部分
                    string number = System.Text.RegularExpressions.Regex.Match(btn.Name, @"\d+").Value;
                    int numberInt = int.Parse(number) - 1; // 按钮数字
                    // 拼接成对应的值，例如 "disPlay" + 数字
                    String showName = "PicDisplay" + number;

                    // 切换伪彩色状态
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
                    MyShowStatusOK(status);
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
                        if (sender is UISymbolButton button)
                        {
                            // 假设按钮有自定义属性 SymbolColor 和 SymbolPressColor
                            dynamic btn = button;
                            btn.SymbolColor = selectedColor;
                            btn.SymbolPressColor = selectedColor;
                        }
                    }
                }
            }
        }

        private void recoveryBtn_Click(object sender, EventArgs e)
        {
            // 清除 fullDisplay 的显示内容
            FullDisPlay.Image = null;
            // 将 fullDisplay 置于底层
            FullDisPlay.SendToBack();
            fullPicBtn.SendToBack();

            currentFullDisplayPictureBox = null;  // 清除 FullDisplay 的显示关联
                        
        }
        private void uiSwitch1_ValueChanged(object sender, bool value)
        {
            if (value)
            {
                Console.WriteLine("begin");
                // 启用屏幕点击功能
                InitializePictureBoxEvents(PictureBox_Click);
                foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
                {
                    pictureBox.MouseEnter -= PictureBox_MouseEnter;
                    pictureBox.MouseWheel -= PictureBox_MouseWheel;
                    pictureBox.MouseDown -= PictureBox_MouseDown;
                    pictureBox.MouseMove -= PictureBox_MouseMoveShow;
                    pictureBox.MouseUp -= PictureBox_MouseUpShow;
                }
                // 为 FullDisplay 添加缩放和拖动事件
                FullDisPlay.MouseWheel += FullDisPlay_MouseWheel;
                FullDisPlay.MouseDown += FullDisPlay_MouseDown;
                FullDisPlay.MouseMove += FullDisPlay_MouseMove;
                FullDisPlay.MouseUp += FullDisPlay_MouseUp;
            }
            else
            {
                // 禁用屏幕点击功能
                UnInitializePictureBoxEvents(PictureBox_Click);
                // 取消 FullDisplay 的所有事件绑定，防止缩放、拖动等操作
                FullDisPlay.MouseWheel -= FullDisPlay_MouseWheel;
                FullDisPlay.MouseDown -= FullDisPlay_MouseDown;
                FullDisPlay.MouseMove -= FullDisPlay_MouseMove;
                FullDisPlay.MouseUp -= FullDisPlay_MouseUp;
                foreach (var pictureBox in new[] { PicDisplay1, PicDisplay2, PicDisplay3, PicDisplay4, PicDisplay5, PicDisplay6, PicDisplay7, PicDisplay8, PicDisplay9 })
                {
                    pictureBox.MouseEnter += PictureBox_MouseEnter;
                    pictureBox.MouseWheel += PictureBox_MouseWheel;
                    pictureBox.MouseDown += PictureBox_MouseDown;
                    pictureBox.MouseMove += PictureBox_MouseMoveShow;
                    pictureBox.MouseUp += PictureBox_MouseUpShow;
                }
            }
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
        private void DisplayHistogram_live(Mat image)
        {
            // 确保图像是单通道灰度图
            if (image.Channels() == 3)
            {
                Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
            }

            // 计算8位图的直方图
            int binCount = 256;
            Rangef range = new Rangef(0, 256);
            int step = 50;

            float[] histogram = new float[binCount];
            int width = image.Width;
            int height = image.Height;

            // 使用并行计算直方图
            int numThreads = Environment.ProcessorCount;
            int rowsPerThread = height / numThreads;

            Parallel.For(0, numThreads, threadIndex =>
            {
                int startRow = threadIndex * rowsPerThread;
                int endRow = (threadIndex == numThreads - 1) ? height : startRow + rowsPerThread;

                float[] localHist = new float[binCount];

                for (int y = startRow; y < endRow; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pixelValue = image.At<byte>(y, x); // 使用byte读取8位的像素值
                        localHist[pixelValue]++;
                    }
                }

                // 累加局部直方图结果
                lock (histogram)
                {
                    for (int i = 0; i < binCount; i++)
                    {
                        histogram[i] += localHist[i];
                    }
                }
            });

            // 获取直方图中的最大值
            double yAxisMaxValue = histogram.Max() * 1.2;

            // 创建 UIBarOption 以显示直方图
            UIBarOption option = new UIBarOption();
            option.Title = new UITitle { Text = "灰度直方图" };

            var series = new UIBarSeries { Name = "GrayScale" };

            // 将灰度直方图数据添加到 UIBarSeries 中
            foreach (var value in histogram)
            {
                series.AddData(value);
            }

            // 添加系列到 Option
            option.Series.Add(series);

            // 确定 X 轴的映射范围
            string selectedBitDepth = pixelCombox.SelectedItem?.ToString();
            int xMappingFactor = 1;
            if (selectedBitDepth == "12")
            {
                xMappingFactor = 4096 / 256; // 将X轴从8位图的0-255映射到12位图的0-4095
                step = 800 / xMappingFactor;
            }
            else if (selectedBitDepth == "16")
            {
                xMappingFactor = 65536 / 256; // 将X轴从8位图的0-255映射到16位图的0-65535
                step = 12800 / xMappingFactor;
            }

            // 设置 X 轴数据，按照映射后的范围进行显示
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

            // 设置 Y 轴上限为最大值的1.2倍
            option.YAxisScaleLines.Add(new UIScaleLine { Color = System.Drawing.Color.Red, Name = "上限", Value = yAxisMaxValue });

            // 更新直方图显示
            histLine.SetOption(option);
        }

        // 当 rightPanel 大小变化时自动调整两个面板的宽度
        private void RightPanel_Resize(object sender, EventArgs e)
        {
            // 设置 histogramPanel 的宽度与 rightPanel 的宽度一致
            histogramPanel.Width = rightPanel.Width;

            // 根据宽高比设置 histogramPanel 的高度
            histogramPanel.Height = (int)(histogramPanel.Width * histogramAspectRatio);

            // 设置 histLine 的宽度与 histogramPanel 一致
            histLine.Width = histogramPanel.Width;

            // 根据相同宽高比设置 histLine 的高度
            histLine.Height = (int)(histLine.Width * histLineRatio);

        }

        private void MiddlePanel_Resize(object sender, EventArgs e)
        {
            // 使用 middlePanel 的宽度和高度的最小值，计算 FullDisplay 的大小
            int fullDisplayWidth = (int)(Math.Min(middlePanel.Width, middlePanel.Height) * 0.9);
            FullDisPlay.Width = fullDisplayWidth;
            FullDisPlay.Height = fullDisplayWidth; // 保持正方形

            // 设置 FullDisplay 的位置，使其居中于 middlePanel
            FullDisPlay.Left = (middlePanel.Width - FullDisPlay.Width) / 2;
            FullDisPlay.Top = (middlePanel.Height - FullDisPlay.Height) / 2;

            // 计算每个 PicDisplay 的大小
            int picDisplaySize = (int)(fullDisplayWidth * 0.3); // 每个 PicDisplay 的宽度和高度
            int picSpacing = (int)(0.05 * fullDisplayWidth); // 设置 PicDisplay 的间距

            // 设置 fullPicBtn 的大小和位置，使其位于 FullDisplay 左上角，且边距为 5%
            int fullBtnMargin = (int)(picDisplaySize * 0.05); // 5% 边距
            fullPicBtn.Width = (int)(picDisplaySize * 0.2); // 按钮宽度为 FullDisplay 宽度的 20%
            fullPicBtn.Height = (int)(picDisplaySize * 0.2); // 按钮高度为 FullDisplay 高度的 20%
            fullPicBtn.Left = FullDisPlay.Left + fullBtnMargin;
            fullPicBtn.Top = FullDisPlay.Top + fullBtnMargin;

            // 布局 PicDisplay1-9（九宫格），使其在 FullDisplay 内部居中排列
            for (int i = 0; i < 9; i++)
            {
                var picDisplay = this.Controls.Find($"PicDisplay{i + 1}", true).FirstOrDefault() as PictureBox;
                var displayBtn = this.Controls.Find($"displayBtn{i + 1}", true).FirstOrDefault() as UISymbolButton;

                if (picDisplay != null && displayBtn != null)
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

                    // 设置 displayBtn 的大小和位置，使其在每个 PicDisplay 内部居中
                    int btnMargin = (int)(picDisplaySize * 0.03); // 5% 边距
                    displayBtn.Width = (int)(picDisplaySize * 0.15);
                    displayBtn.Height = (int)(picDisplaySize * 0.15);
                    displayBtn.Left = picDisplay.Left + btnMargin;
                    displayBtn.Top = picDisplay.Top + btnMargin;
                }
            }
            // 设置 videoProgressBar 的宽度和位置
            videoProgressBar.Width = fullDisplayWidth;            // 宽度与 FullDisplay 一致
            videoProgressBar.Left = FullDisPlay.Left;             // 左对齐 FullDisplay
            videoProgressBar.Top = FullDisPlay.Bottom + 5;        // 紧贴 FullDisplay 下方，可以增加 5 像素间距
        }


        //-----------------右侧屏幕处理 end--------------
        //————————状态栏功能——————
        private void searchCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("searchCamToolStripMenuItem_Click");
            // dcamapi_init() may takes for a few seconds
            Cursor.Current = Cursors.WaitCursor;

            if (!MyDcamApi.init())
            {
                MyShowStatusNG("相机读取失败:", MyDcamApi.m_lasterr);
                MessageBox.Show($"相机读取失败\n{MyDcamApi.m_lasterr}");
                Cursor.Current = Cursors.Default;
                return;                         // Fail: dcamapi_init()
            }
            // Success: dcamapi_init()
            MyShowStatusOK("dcamapi_init()");
            MyFormStatus_Initialized();
            Cursor.Current = Cursors.Default;

            //开始打开相机
            if (mydcam != null)
            {
                MyFormStatus_Initialized();     // FormStatus should be Initialized.
                return;                         // internal error
            }

            // dcamdev_open() may takes for a few seconds
            Cursor.Current = Cursors.WaitCursor;

            MyDcam aMyDcam = new MyDcam();
            if (!aMyDcam.dev_open(m_indexCamera))
            {
                MyShowStatusNG("dcamdev_open()", aMyDcam.m_lasterr);
                aMyDcam = null;
                Cursor.Current = Cursors.Default;
                return;                         // Fail: dcamdev_open()
            }

            // Success: dcamdev_open()
            mydcam = aMyDcam;					// store MyDcam instance
            string cameraModel = mydcam.dev_getstring(DCAMIDSTR.MODEL);
            MyShowStatusOK("dcamdev_open()");
            MyFormStatus_Opened();              // change dialog FormStatus to Opened
            devName.Items.Clear();
            devName.Items.Add(cameraModel);
            devName.SelectedIndex = 0;  // 选择第一项

            if (mydcam != null)
            {
                try
                {
                    UpdateBitsPerChannel();
                    UpdateExposureTime();
                    UpdateFrameRate();
                    // 启用 funGroup 中的控件
                    EnableAllControls(funGroup);
                }
                catch (Exception ex)
                {
                    MyShowStatusOK($"获取相机参数时发生错误: {ex.Message}");
                }
            }
            Cursor.Current = Cursors.Default;

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


        private void CloseCameraDevice()
        {
            if (mydcam == null)
            {
                MyShowStatus("Internal Error: mydcam is null");
                MyFormStatus_Initialized();     // FormStatus should be Initialized.
                return;                         // internal error
            }

            MyThreadCapture_Abort();            // abort capturing thread if exist

            if (!mydcam.dev_close())
            {
                MyShowStatusNG("dcamdev_close()", mydcam.m_lasterr);
                return;                         // Fail: dcamdev_close()
            }

            // Success: dcamdev_close()

            mydcam = null;

            MyFormStatus_Initialized();         // change dialog FormStatus to Initialized

        }

        private void backgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 如果 currentFrame 不为空，表示当前有图像可以用作背景
            if (currentFrame != null && !currentFrame.Empty())
            {
                // 将 currentFrame 转换为 Bitmap 并保存为背景
                backgroundImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(currentFrame);
                MyShowStatusOK("背景图像已记录");
            }
            else if (m_bitmap != null)  // 如果 currentFrame 为空但摄像头的 m_bitmap 可用
            {
                lock (BitmapLock)
                {
                    // 创建摄像头捕获图像的副本
                    backgroundImage = new Bitmap(m_bitmap);
                }
                MyShowStatusOK("摄像头背景图像已记录");
            }
            else
            {
                MessageBox.Show("没有可用的图像来记录背景。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadIToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // 打开文件对话框以选择图片
            OpenFileDialog openFileDialog = new OpenFileDialog();
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
                MyShowStatusOK("背景减除已启用");
            }
            else
            {
                MyShowStatusOK("背景减除已禁用");
            }
        }

        private void imageProcessingcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("imageProcessingcToolStripMenuItem_Click");
            dataShow newForm = new dataShow();
            newForm.Show();

            // 创建一个计时器来更新UI
            System.Windows.Forms.Timer uiTimer = new System.Windows.Forms.Timer();
            uiTimer.Interval = 100; // 每100毫秒更新一次UI
            uiTimer.Tick += (s, args) => Application.DoEvents();
            uiTimer.Start();

            try
            {
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    string pythonScriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "1");
                    sys.path.append(pythonScriptsPath);
                    // 将 currentFrame 转换为字节数组
                    byte[] imageData = currentFrame.ToBytes();
                    dynamic mainModule = Py.Import("channelDeal");
                    dynamic result = mainModule.chanDeal(imageData);  // 调用 main.py 中的 main 函数
                    // 将字符串解析为二维数组

                    // 停止计时器
                    uiTimer.Stop();
                    // 隐藏等待指示器并显示结果

                    newForm.DisplayResult(result.ToString());
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
        private void removeAutofluorescenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 调用记录背景的方法
            backgroundToolStripMenuItem_Click(sender, e);

            // 调用启用背景减除的方法
            removeBackgroundToolStripMenuItem_Click(sender, e);

            // 可以添加额外的逻辑，例如显示一个消息
            MyShowStatusOK("自动荧光去除已应用");
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

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

            logger.LogButtonClick("openToolStripMenuItem_Click");
            // 打开文件对话框，允许用户选择文件
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png|Video Files|*.mp4;*.avi|KSJP Files|*.ksjp|KSJV Files|*.ksjv|All Files|*.*";
                openFileDialog.Title = "选择一个视频或图像文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    string fileExtension = Path.GetExtension(selectedFilePath).ToLower();

                    try
                    {
                        if (fileExtension == ".ksjp")
                        {
                            // 处理 .ksjp 图像文件
                            ProcessKsjpFile(selectedFilePath);
                        }
                        else if (fileExtension == ".ksjv")
                        {
                            // 处理 .ksjv 视频文件
                            isVideoPlaying = true;
                            ProcessKsjvFile(selectedFilePath);
                        }
                        else if (IsImageFile(selectedFilePath))
                        {
                            // 如果是图片，使用 OpenCV 读取图像
                            loadedImage = Cv2.ImRead(selectedFilePath, ImreadModes.Grayscale); // 读取为灰度图像

                            if (loadedImage.Empty())
                            {
                                MessageBox.Show("无法加载图片文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            currentFrame = loadedImage; // 存储当前帧的图像
                                                        // 使用Task.Run在后台执行异步函数，不会阻塞主线程
                            Task.Run(async () =>
                            {
                                await ProcessVideoFramesAsync();
                            });
                        }
                        else if (IsVideoFile(selectedFilePath))
                        {
                            isVideoPlaying = true;
                            StartVideoPlayback(selectedFilePath); // 启动视频播放
                            
                        }

                        // 启用保存组中的控件
                        EnableAllControls(saveGroup);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"读取或解密文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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
                                        // 使用Task.Run在后台执行异步函数，不会阻塞主线程
            Task.Run(async () =>
            {
                await ProcessVideoFramesAsync();
            });
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
                // 使用Task.Run在后台执行异步函数，不会阻塞主线程
                Task.Run(async () =>
                {
                    await ProcessVideoFramesAsync();
                });
                if (totalFrames > 0)
                {
                    double progressPercentage = (double)currentFrameIndex / totalFrames * 100;
                    //Console.WriteLine($"{progressPercentage}");
                    // 更新进度条
                    videoProgressBar.Value = (int)progressPercentage;
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
                videoTimer.Dispose();
                videoTimer = null;
            }

            // 重置视频播放的当前帧索引
            currentFrameIndex = 0;

            // 清空播放的 PictureBox 或者显示第一帧
            PicDisplay1.Image = null; // 或者显示一个默认的图像

            // 清理相关视频帧缓存
            if (videoFrames != null)
            {
                videoFrames.Clear();
            }

            // 更新UI状态（可以通过状态栏提示或按钮状态）
            MyShowStatusOK("视频播放已停止");
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("saveToolStripMenuItem_Click");
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

            // 获取当前时间戳
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = Path.Combine(m_savePath, $"original_frame_{timestamp}.ksjp");  // 使用自定义后缀名

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

                MyShowStatusOK($"图像已成功保存：{filename}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存图像时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void currentImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 如果 currentFrame 不为空，表示当前有图像可以用作背景
            if (currentFrame != null && !currentFrame.Empty())
            {
                // 将 currentFrame 转换为 Bitmap 并保存为背景
                backgroundImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(currentFrame);
                MyShowStatusOK("背景图像已记录");
            }
            else if (m_bitmap != null)  // 如果 currentFrame 为空但摄像头的 m_bitmap 可用
            {
                lock (BitmapLock)
                {
                    // 创建摄像头捕获图像的副本
                    backgroundImage = new Bitmap(m_bitmap);
                }
                MyShowStatusOK("摄像头背景图像已记录");
            }
            else
            {
                MessageBox.Show("没有可用的图像来记录背景。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.LogButtonClick("saveAsToolStripMenuItem_Click");
            // 确保原始帧存在
            if (originalFrame == null || originalFrame.Empty())
            {
                MessageBox.Show("没有可保存的原始图像。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "KSJP Files|*.ksjp|All Files|*.*";
                saveFileDialog.Title = "选择保存路径";
                saveFileDialog.FileName = $"original_frame_{DateTime.Now:yyyyMMdd_HHmmss}.ksjp";  // 默认文件名带.ksjp后缀

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
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
                                File.WriteAllBytes(saveFileDialog.FileName, xorData);
                            }
                        }

                        MyShowStatusOK($"图像已成功保存：{saveFileDialog.FileName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存图像时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
        private void EnableDragging() {
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
        private void DisableDragging() {
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
            if (!isSingleArrowToolActive) {

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
            ChangeButtonColor(displayBtn1, leftClick);
            ChangeButtonColor(displayBtn2, leftClick);
            ChangeButtonColor(displayBtn3, leftClick);
            ChangeButtonColor(displayBtn4, leftClick);
            ChangeButtonColor(displayBtn5, leftClick);
            ChangeButtonColor(displayBtn6, leftClick);
            ChangeButtonColor(displayBtn7, leftClick);
            ChangeButtonColor(displayBtn8, leftClick);
            ChangeButtonColor(displayBtn9, leftClick);

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

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            cudaContext.Dispose();
        }
        private void btnToggleFullScreen_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                // 当前为全屏状态，切换为正常窗口
                this.WindowState = FormWindowState.Normal;
                this.fullScreanMenuItem.Image = Properties.Resources.全屏; // 显示全屏图标
            }
            else
            {
                // 当前为正常窗口，切换为全屏
                this.WindowState = FormWindowState.Maximized;
                this.fullScreanMenuItem.Image = Properties.Resources.恢复窗口; // 显示恢复图标
            }
        }


        private void smallMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
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