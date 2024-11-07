using DocumentFormat.OpenXml.Office.CustomXsn;
using Python.Runtime;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace csAcq4
{
    public partial class dataShow : Form
    {
        public dataShow()
        {
            InitializeComponent();
            picShow.MouseWheel += PicShow_MouseWheel;
            picShow.MouseClick += PicShow_MouseClick;
            picShow.Paint += PicShow_Paint;
        }

        private List<Bitmap> imageList = new List<Bitmap>(); // 用于存储已转换好的图片
        private int currentImageIndex = 0; // 当前显示的图像索引
        int width = 40;  // 假设图片宽度固定为 40 列
        int height = 40; // 假设图片高度固定为 40 行

        private List<(Rectangle Rect, Color Color, List<int> Data)> selectedRectangles = new List<(Rectangle, Color, List<int>)>();
        private const int RectangleSize = 10;
        private Random random = new Random();
        private string multiTiffPath;
        //随机生成颜色
        private Color[] colors = new Color[]
        {
            Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple,
            Color.Orange, Color.Cyan, Color.Magenta, Color.LimeGreen, Color.Navy
        };
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
                        e.Item.ForeColor = Color.Black;
                    }
                    else
                    {
                        // 未选中时前景色为白色
                        e.Item.ForeColor = Color.White;
                    }
                }
                else
                {
                    // 子菜单项始终保持黑色
                    e.Item.ForeColor = Color.Black;
                }
            }
        }

        // Step 1: 解析并显示结果
        public void DisplayResult(string formattedString, string tiffPath)
        {
            multiTiffPath = tiffPath;
            // 解析数据生成图片列表
            ParseImageData(formattedString);

            // 显示第一张图片
            ShowImage(picShow, 0);

            picScroller.Maximum = imageList.Count - 1;
            InitializeRoiLineChart();
        }

        // 解析传入的字符串并生成图像列表
        public void ParseImageData(string finalString)
        {
            imageList.Clear(); // 清空现有的图像列表，确保是新的数据

            // 按照 '|' 分割数组，得到每个数组
            string[] allImagesData = finalString.Split('|');

            foreach (string imageData in allImagesData)
            {
                // 按照 ';' 分割行，得到每张图片的行数据
                string[] rows = imageData.Split(';');
                int[,] pixelData = new int[height, width];

                for (int i = 0; i < height; i++)
                {
                    // 每行数据通过 ',' 分割获取每列的灰度值
                    string[] pixelValues = rows[i].Split(',');

                    // 确保行数据的长度符合预期
                    if (pixelValues.Length != width)
                    {
                        MessageBox.Show($"第 {i} 行数据长度不正确，预期宽度为 {width}");
                        return;
                    }

                    // 解析并存储每个像素的灰度值
                    for (int j = 0; j < width; j++)
                    {
                        if (int.TryParse(pixelValues[j], out int grayValue))
                        {
                            pixelData[i, j] = grayValue;
                        }
                        else
                        {
                            MessageBox.Show($"第 {i} 行的第 {j} 列数据无法解析为整数");
                            return;
                        }
                    }
                }

                // 生成 Bitmap 图像
                Bitmap image = CreateBitmapFromPixelData(pixelData);
                imageList.Add(image); // 将生成的图片加入列表
            }
        }

        // 创建 Bitmap 图像
        public Bitmap CreateBitmapFromPixelData(int[,] pixelData)
        {
            Bitmap image = new Bitmap(width, height);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int grayValue = pixelData[i, j];
                    Color color = Color.FromArgb(grayValue, grayValue, grayValue); // 灰度图
                    image.SetPixel(j, i, color);
                }
            }
            return image;
        }

        // 显示指定索引的图像
        public void ShowImage(PictureBox picShow, int imageIndex)
        {
            if (imageIndex < 0 || imageIndex >= imageList.Count)
            {
                MessageBox.Show("图像索引超出范围");
                return;
            }

            // 获取指定索引的图像并显示
            picShow.Image = imageList[imageIndex];
            picShow.SizeMode = PictureBoxSizeMode.Zoom;
            currentImageIndex = imageIndex; // 更新当前图像索引
        }

        private void PicShow_MouseWheel(object sender, MouseEventArgs e)
        {
            // 根据滚轮方向调整 picScroller 的值
            if (e.Delta > 0)
            {
                // 向上滚动，减小值
                picScroller.Value = Math.Max(0, picScroller.Value - 1);
                currentImageIndex = picScroller.Value; // 循环切换图片
                ShowImage(picShow, currentImageIndex);
            }
            else
            {
                // 向下滚动，增加值
                picScroller.Value = Math.Min(picScroller.Maximum, picScroller.Value + 1);
                currentImageIndex = picScroller.Value; // 循环切换图片
                ShowImage(picShow, currentImageIndex);
            }
        }
        private void picScroller_ValueChanged(object sender, EventArgs e)
        {
            currentImageIndex = picScroller.Value; // 循环切换图片
            ShowImage(picShow, currentImageIndex);
        }
        private void PicShow_MouseClick(object sender, MouseEventArgs e)
        {
            if (roiCheck.Checked && imageList.Count > 0)
            {
                Point clickPoint = e.Location;
                int imageX = (int)((float)clickPoint.X / picShow.Width * width);
                int imageY = (int)((float)clickPoint.Y / picShow.Height * height);

                if (imageX >= 0 && imageX < width && imageY >= 0 && imageY < height)
                {
                    Rectangle newRect = new Rectangle(
                        clickPoint.X - RectangleSize / 2,
                        clickPoint.Y - RectangleSize / 2,
                        RectangleSize,
                        RectangleSize);

                    bool removed = false;
                    for (int i = selectedRectangles.Count - 1; i >= 0; i--)
                    {
                        if (selectedRectangles[i].Rect.Contains(clickPoint))
                        {
                            selectedRectangles.RemoveAt(i);
                            removed = true;
                            break;
                        }
                    }

                    if (!removed)
                    {
                        Color newColor = GetNextColor();

                        List<int> pointData = ExtractPointData(imageX, imageY);
                        selectedRectangles.Add((newRect, newColor, pointData));
                    }

                    picShow.Invalidate();
                    UpdateRoiLineChart();
                }
            }
        }
        private void InitializeRoiLineChart()
        {
            UILineOption option = new UILineOption();
            option.ToolTip.Visible = true;
            option.Title = new UITitle();
            option.Title.Text = "ROI Data";
            option.XAxis.Name = "Image Index";
            option.YAxis.Name = "Pixel Value";

            // 如果没有选中任何 ROI，显示一条提示信息
            var series = new UILineSeries("No ROI Data", Color.Gray)
            {
                Symbol = UILinePointSymbol.None,
                SymbolSize = 4,
                SymbolLineWidth = 2
            };
            // 在图表中显示一个空系列
            option.AddSeries(series);

            // 设置X轴范围
            option.XAxis.SetRange(0, imageList.Count - 1);

            // 设置Y轴范围（假设像素值在0-255之间）
            option.YAxis.SetRange(0, 255);
            roiLineChart.SetOption(option);
        }
        private void UpdateRoiLineChart()
        {
            UILineOption option = new UILineOption();
            option.ToolTip.Visible = true;
            option.Title = new UITitle();
            option.Title.Text = "ROI Data";
            option.XAxis.Name = "Image Index";
            option.YAxis.Name = "Pixel Value";
            // 清除图表中的所有系列（这将删除旧的数据）
            option.Series.Clear();
            if (selectedRectangles.Count == 0)
            {
                // 如果没有选中任何 ROI，显示一条提示信息
                var series = new UILineSeries("No ROI Data", Color.Gray)
                {
                    Symbol = UILinePointSymbol.None,
                    SymbolSize = 4,
                    SymbolLineWidth = 2
                };
                // 在图表中显示一个空系列
                option.AddSeries(series);
            }
            else
            {
                for (int i = 0; i < selectedRectangles.Count; i++)
                {
                    var (rect, color, data) = selectedRectangles[i];
                    var series = new UILineSeries($"ROI {i + 1}", color);
                    series.Symbol = UILinePointSymbol.None;
                    series.SymbolSize = 4;
                    series.SymbolLineWidth = 2;

                    for (int j = 0; j < data.Count; j++)
                    {
                        series.Add(j, data[j]);
                    }

                    option.AddSeries(series);  // 使用 AddSeries 方法添加系列
                }
            }

            // 设置X轴范围
            option.XAxis.SetRange(0, imageList.Count - 1);

            // 设置Y轴范围（假设像素值在0-255之间）
            option.YAxis.SetRange(0, 255);

            roiLineChart.SetOption(option);
        }

        private void PicShow_Paint(object sender, PaintEventArgs e)
        {
            foreach (var (rect, color, _) in selectedRectangles)
            {
                using (Pen pen = new Pen(color, 2))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        private Color GetNextColor()
        {
            return Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        }

        private List<int> ExtractPointData(int imageX, int imageY)
        {

            List<Point> rawPointData = new List<Point>();
            for (int i = 0; i < imageList.Count; i++)
            {
                Color pixelColor = imageList[i].GetPixel(imageX, imageY);
                int grayValue = pixelColor.R; // 假设是灰度图，R、G、B值相同
                rawPointData.Add(new Point(i, grayValue));
            }

            List<int> resultValues;
            if (smoothCheck.Checked)
            {
                int smoothFactor = (int)numericUpDown1.Value;
                // 使用 linearSmooth5 平滑数据并传入平滑系数
                List<Point> smoothedData = linearSmooth5(rawPointData, smoothFactor);
                resultValues = smoothedData.Select(p => p.Y).ToList();
            }
            else
            {
                // 若不启用平滑，直接返回原始数据
                resultValues = rawPointData.Select(p => p.Y).ToList();
            }

            return resultValues;
        }
        //private List<int> ExtractPointData(int imageX, int imageY)
        //{
        //    List<Point> rawPointData = new List<Point>();
        //    for (int i = 0; i < imageList.Count; i++)
        //    {
        //        Color pixelColor = imageList[i].GetPixel(imageX, imageY);
        //        int grayValue = pixelColor.R; // 假设是灰度图，R、G、B值相同
        //        rawPointData.Add(new Point(i, grayValue));
        //    }

        //    // 使用 linearSmooth5 平滑数据
        //    List<Point> smoothedData = linearSmooth5(rawPointData);

        //    // 将平滑后的数据提取出灰度值
        //    List<int> smoothedValues = smoothedData.Select(p => p.Y).ToList();
        //    return smoothedValues;
        //}

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (selectedRectangles.Count == 0)
            {
                MessageBox.Show("没有数据可保存，请先添加数据。");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(sfd.FileName))
                    {
                        // 动态生成表头，每个 ROI 使用 3 列，并在每个 ROI 之后加一个空列
                        for (int i = 0; i < selectedRectangles.Count; i++)
                        {
                            writer.Write($"ROI {i + 1} Name,ROI {i + 1} Index,ROI {i + 1} Data,"); // 加一个空列
                        }
                        writer.WriteLine(); // 换行写入数据

                        // 找出所有 ROI 中数据列表的最大长度
                        int maxDataCount = selectedRectangles.Max(r => r.Item3.Count);

                        // 开始逐行写入每个 ROI 的数据
                        for (int j = 0; j < maxDataCount; j++)
                        {
                            for (int i = 0; i < selectedRectangles.Count; i++)
                            {
                                var (rect, color, data) = selectedRectangles[i];
                                string lineName = $"ROI {i + 1}";

                                // 判断当前行是否有数据，否则填充空值
                                if (j < data.Count)
                                {
                                    writer.Write($"{lineName},{j},{data[j]},"); // 数据后加一个空列
                                }
                                else
                                {
                                    writer.Write(",,,"); // 保留空列
                                }
                            }
                            writer.WriteLine(); // 每写完一行，换行
                        }
                    }
                    MessageBox.Show("数据点已成功保存到CSV文件。");
                }
            }
        }


        private void chartSaveToolClick(object sender, EventArgs e)
        {
            if (selectedRectangles.Count == 0)
            {
                MessageBox.Show("没有数据可保存，请先添加数据。");
                return;
            }

            string filePath = Path.Combine(Environment.CurrentDirectory, "chartData.csv"); // 设置保存路径和文件名
            using (StreamWriter writer = new StreamWriter(filePath)) // 默认覆盖同名文件
            {
                // 动态生成表头，每个 ROI 使用 3 列，并在每个 ROI 之后加一个空列
                for (int i = 0; i < selectedRectangles.Count; i++)
                {
                    writer.Write($"ROI {i + 1} Name,ROI {i + 1} Index,ROI {i + 1} Data,"); // 加一个空列
                }
                writer.WriteLine(); // 换行写入数据

                // 找出所有 ROI 中数据列表的最大长度
                int maxDataCount = selectedRectangles.Max(r => r.Item3.Count);

                // 开始逐行写入每个 ROI 的数据
                for (int j = 0; j < maxDataCount; j++)
                {
                    for (int i = 0; i < selectedRectangles.Count; i++)
                    {
                        var (rect, color, data) = selectedRectangles[i];
                        string lineName = $"ROI {i + 1}";

                        // 判断当前行是否有数据，否则填充空值
                        if (j < data.Count)
                        {
                            writer.Write($"{lineName},{j},{data[j]},"); // 数据后加一个空列
                        }
                        else
                        {
                            writer.Write(",,,"); // 保留空列
                        }
                    }
                    writer.WriteLine(); // 每写完一行，换行
                }
            }
            MessageBox.Show("数据点已成功保存到文件：chartData.csv");
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 提取文件夹路径
            string folderPath = Path.GetDirectoryName(multiTiffPath);

            // 检查文件夹路径是否存在
            if (Directory.Exists(folderPath))
            {
                // 打开文件夹
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                Console.WriteLine($"指定的文件夹不存在：{folderPath}");
            }
            //// 获取当前路径
            //string currentPath = Environment.CurrentDirectory;

            //// 构造保存的文件路径
            //string filePath = Path.Combine(currentPath, "all_images.raw");

            //// 调用保存方法
            //SaveAllImagesAsSingleRaw(filePath);
        }

        // 将所有图片保存为一张RAW文件
        //public void SaveAllImagesAsSingleRaw(string filePath)
        //{
        //    using (FileStream fs = new FileStream(filePath, FileMode.Create))
        //    {
        //        foreach (var image in imageList)
        //        {
        //            for (int y = 0; y < image.Height; y++)
        //            {
        //                for (int x = 0; x < image.Width; x++)
        //                {
        //                    Color pixelColor = image.GetPixel(x, y);
        //                    byte grayValue = pixelColor.R; // 假设是灰度图
        //                    fs.WriteByte(grayValue);
        //                }
        //            }
        //        }
        //    }
        //    MessageBox.Show("所有图片已成功保存为一张RAW文件！");
        //}

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "tiff files (*.tiff)|*.tiff";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    try
                    {
                        // 检查源文件是否存在
                        if (File.Exists(multiTiffPath))
                        {
                            // 检查目标路径的目录是否存在，不存在则创建
                            string destinationDir = Path.GetDirectoryName(filePath);
                            if (!Directory.Exists(destinationDir))
                            {
                                Directory.CreateDirectory(destinationDir);
                            }

                            // 移动文件
                            File.Move(multiTiffPath, filePath);
                            multiTiffPath = filePath;
                        }
                        else
                        {
                            Console.WriteLine("源文件不存在！");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"移动文件时发生错误：{ex.Message}");
                    }
                }
            }
        }
        string imageJPath; // 替换为 ImageJ.exe 的实际路径
        private string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        private void showInImageJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 设置 ImageJ 可执行文件的路径
            // 加载持久化保存的 ImageJ 路径
            LoadImageJPath();
            // 获取当前路径
            //string currentPath = Environment.CurrentDirectory;

            //// 构造保存的文件路径
            //string filePath = Path.Combine(currentPath, "all_images.raw");
            //using (FileStream fs = new FileStream(filePath, FileMode.Create))
            //{
            //    foreach (var image in imageList)
            //    {
            //        for (int y = 0; y < image.Height; y++)
            //        {
            //            for (int x = 0; x < image.Width; x++)
            //            {
            //                Color pixelColor = image.GetPixel(x, y);
            //                byte grayValue = pixelColor.R; // 假设是灰度图
            //                fs.WriteByte(grayValue);
            //            }
            //        }
            //    }
            //}

            // 创建并启动 Process 来运行 ImageJ

            Console.WriteLine($"multiTiffPath:{multiTiffPath}");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = imageJPath,
                Arguments = $"\"{multiTiffPath}\"", // 将图像路径作为参数传递
                UseShellExecute = true // 设置为 true 以便打开外部程序
            };

            try
            {
                Process process = Process.Start(startInfo);
                Console.WriteLine("ImageJ 已启动并运行宏文件。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动 ImageJ 时出错: {ex.Message}");
            }
        }

        private List<Point> linearSmooth5(List<Point> Data, double smoothFactor)
        {
            List<Point> result = new List<Point>();
            var count = Data.Count;
            if (count <= 5)
            {
                for (int i = 0; i <= count - 1; i++)
                {
                    result.Add(Data[i]);
                }
            }
            else
            {
                // 使用传入的平滑系数计算平滑值
                double y = (smoothFactor * Data[0].Y + (smoothFactor - 1.0) * Data[1].Y + Data[2].Y - Data[4].Y) / (smoothFactor + 1.0);
                result.Add(new Point(Data[0].X, (int)y));

                y = ((smoothFactor + 1.0) * Data[0].Y + smoothFactor * Data[1].Y + 2 * Data[2].Y + Data[3].Y) / (2 * (smoothFactor + 1.0));
                result.Add(new Point(Data[1].X, (int)y));

                for (int i = 2; i <= count - 3; i++)
                {
                    y = (Data[i - 2].Y + Data[i - 1].Y + Data[i].Y + Data[i + 1].Y + Data[i + 2].Y) / 5.0;
                    result.Add(new Point(Data[i].X, (int)y));
                }

                y = ((smoothFactor + 1.0) * Data[count - 1].Y + smoothFactor * Data[count - 2].Y + 2 * Data[count - 3].Y + Data[count - 4].Y) / (2 * (smoothFactor + 1.0));
                result.Add(new Point(Data[count - 2].X, (int)y));

                y = (smoothFactor * Data[count - 1].Y + (smoothFactor - 1.0) * Data[count - 2].Y + Data[count - 3].Y - Data[count - 5].Y) / (smoothFactor + 1.0);
                result.Add(new Point(Data[count - 1].X, (int)y));
            }
            return result;
        }

        private void imageJPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 打开文件对话框以选择 imagej.exe
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable Files|imagej.exe";
            openFileDialog.Title = "选择 ImageJ 可执行文件";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 记录选中的文件路径
                imageJPath = openFileDialog.FileName;

                // 保存路径到本地文件
                SaveImageJPath(imageJPath);

                // 显示保存成功的信息
                MessageBox.Show($"ImageJ 路径已保存：{imageJPath}", "路径保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 保存路径到本地文件
        private void SaveImageJPath(string path)
        {
            try
            {
                File.WriteAllText(configFilePath, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存路径时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // 从本地文件加载路径
        private void LoadImageJPath()
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    imageJPath = File.ReadAllText(configFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载路径时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataShow_Load(object sender, EventArgs e)
        {

            // 设置主菜单的渲染器
            menuStrip1.Renderer = new CustomToolStripRenderer();

            // 对所有子菜单项应用自定义渲染器
            ApplyCustomRendererToSubMenus(menuStrip1);
        }
        // 递归地应用渲染器到所有子菜单项
        private void ApplyCustomRendererToSubMenus(ToolStrip menuStrip)
        {
            foreach (ToolStripItem item in menuStrip.Items)
            {
                ApplyCustomRendererToMenuItem(item);
            }
        }

        private void ApplyCustomRendererToMenuItem(ToolStripItem item)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                menuItem.DropDown.Renderer = new CustomToolStripRenderer(); // 设置子菜单的渲染器

                // 递归调用，对子菜单项应用渲染器
                foreach (ToolStripItem subItem in menuItem.DropDownItems)
                {
                    ApplyCustomRendererToMenuItem(subItem);
                }
            }
        }
        private void execPy_Click(object sender, EventArgs e)
        {
            // 显示文件选择窗口
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Python Files (*.py)|*.py|All Files (*.*)|*.*";
                openFileDialog.Title = "选择要执行的 Python 文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 获取用户选择的文件路径
                    string pythonFilePath = openFileDialog.FileName;

                    try
                    {
                        // 读取 Python 文件内容
                        string pythonCode = File.ReadAllText(pythonFilePath);

                        using (Py.GIL())
                        {
                            // 将 Python 代码加载为模块
                            dynamic pyModule = PyModule.FromString("temp_module", pythonCode);

                            // 调用 calData 函数并获取返回值
                            dynamic result = pyModule.calData();

                            // 显示结果
                            MessageBox.Show(result.ToString(), "结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 显示错误信息
                        MessageBox.Show($"执行 Python 代码时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

    }
}
