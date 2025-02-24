using SharedComponents.EVE;
using SharedComponents.Utility;
using SharedComponents.WinApiUtil;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVESharpLauncher
{
    internal class ThumbPreviewTab : TabPage
    {
        #region Fields

        private FlowLayoutPanel flowPanel;
        private TabControl parent;
        private MainForm sender;
        private ConcurrentDictionary<EveAccount, ThumbnailPreviewBox> thumbnailDict;
        private CancellationTokenSource thumbnailtokenSource;

        #endregion Fields
        #region Constructors

        public ThumbPreviewTab(MainForm sender, TabControl t) : base()
        {
            this.sender = sender;
            Text = "Preview";
            parent = t;
            flowPanel = new FlowLayoutPanel();
            FormUtil.SetDoubleBuffered(flowPanel);
            flowPanel.AutoScroll = true;
            flowPanel.Dock = DockStyle.Fill;
            Controls.Add(flowPanel);
        }

        #endregion Constructors

        #region Methods

        public void Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == this)
            {
                Debug.WriteLine("Preview tab page selected.");
                if (thumbnailDict == null)
                    thumbnailDict = new ConcurrentDictionary<EveAccount, ThumbnailPreviewBox>();

                if (thumbnailtokenSource == null)
                {
                    thumbnailtokenSource = new CancellationTokenSource();
                    Task.Run(() =>
                    {
                        var ts = thumbnailtokenSource;
                        while (thumbnailtokenSource != null
                               && !thumbnailtokenSource.Token.IsCancellationRequested
                               && thumbnailtokenSource == ts)
                            try
                            {
                                TabPage currentSelectedTab = null;
                                if (this.IsHandleCreated)
                                    Invoke(new Action(() => { currentSelectedTab = parent.SelectedTab; }));

                                if (currentSelectedTab != this)
                                    break;

                                Debug.WriteLine("Update.");
                                foreach (var eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.ToList())
                                    if (eA != null && WinApiUtil.IsValidHWnd((IntPtr)eA.EveHWnd) && eA.EveProcessExists)
                                    {
                                        if (!thumbnailDict.ContainsKey(eA))
                                        {
                                            Debug.WriteLine("Add new ThumbnailPreviewBox to dict.");
                                            thumbnailDict[eA] = new ThumbnailPreviewBox((IntPtr)eA.EveHWnd, this.sender, eA, flowPanel);
                                            Invoke(new Action(() =>
                                            {
                                                flowPanel.Controls.Add(thumbnailDict[eA].PictureBox);
                                            }));
                                            thumbnailDict[eA].StartUpdateTask();
                                        }
                                        else
                                        {
                                            thumbnailDict[eA].StartUpdateTask();
                                        }
                                    }
                                    else
                                    {
                                        if (thumbnailDict.ContainsKey(eA))
                                        {
                                            Debug.WriteLine("Remove ThumbnailPreviewBox from dict.");
                                            thumbnailDict.TryRemove(eA, out var box);
                                            Invoke(new Action(() =>
                                            {
                                                flowPanel.Controls.Remove(box.PictureBox);
                                            }));
                                        }
                                    }

                                thumbnailtokenSource.Token.WaitHandle.WaitOne(2000);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                            }

                        Debug.WriteLine("Update task stopped.");
                    }, thumbnailtokenSource.Token);
                }
            }
            else
            {
                Debug.WriteLine("Preview tab page de-selected.");
                thumbnailtokenSource = null;
                foreach (var cBox in thumbnailDict.Values)
                    Task.Run(() => { cBox.StopUpdateTask(); });
            }
        }

        #endregion Methods
    }

    public class ThumbnailPreviewBox
    {
        #region Fields

        private static ConcurrentDictionary<int, Font> _fontDict = new ConcurrentDictionary<int, Font>();
        private Brush _fgwBrush;
        private Brush _fontBrush;
        private DateTime _lastImageRefresh;
        private DateTime _lastZoom;
        private object _lockZoom;

        private int _previousFontSize;
        private object _taskLock;
        private bool _updateFontSize;
        private CancellationTokenSource cts;
        private EveAccount eA;
        private FlowLayoutPanel flowPanel;
        private RECT rect;
        private MainForm sender;
        private Task t;
        private double _zoom;

        #endregion Fields

        #region Constructors

        public ThumbnailPreviewBox(IntPtr hWnd, MainForm sender, EveAccount eA, FlowLayoutPanel flowPanel)
        {
            this.flowPanel = flowPanel;
            this.eA = eA;
            _zoom = 0.2;
            PictureBox = new PictureBox();
            PictureBox.Paint += PBoxOnPaint;
            PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            rect = WinApiUtil.GetWindowRect(hWnd);
            PictureBox.Width = (int)(rect.Width * _zoom);
            PictureBox.Height = (int)(rect.Height * _zoom);
            RefreshImage();
            this.sender = sender;
            this.sender.MouseWheel += PBoxOnMouseWheel;
            this.flowPanel.MouseWheel += PBoxOnMouseWheel;
            this._lastImageRefresh = DateTime.UtcNow;
            PictureBox.DoubleClick += PBoxOnDoubleClick;
            _fgwBrush = new SolidBrush(Color.FromArgb(255, 57, 255, 118));
            _fontBrush = WinApiUtil.IsWindowAddedToTaskbar(hWnd) ? _fgwBrush : Brushes.White;
            _taskLock = new object();
            _lockZoom = new object();
            StartUpdateTask();
        }

        #endregion Constructors

        #region Destructors

        ~ThumbnailPreviewBox()
        {
            try
            {
                sender.MouseWheel -= PBoxOnMouseWheel;
                flowPanel.MouseWheel -= PBoxOnMouseWheel;
                PictureBox.Paint -= PBoxOnPaint;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                Cache.Instance.Log(e.ToString());
            }
        }

        #endregion Destructors

        #region Properties

        public PictureBox PictureBox { get; }

        #endregion Properties

        #region Methods

        public void RefreshImage()
        {
            if (WinApiUtil.IsValidHWnd((IntPtr)eA.EveHWnd))
            {
                var img = Util.CaptureWindow((IntPtr)eA.EveHWnd);
                _lastImageRefresh = DateTime.UtcNow;
                // release a previous image if it exist
                if (PictureBox.IsHandleCreated)
                {
                    PictureBox.Invoke(new Action(() =>
                    {
                        if (PictureBox.Image != null) PictureBox.Image.Dispose();
                        PictureBox.Image = img;
                    }));
            }
        }
        }

        public void StartUpdateTask()
        {
            lock (_taskLock)
            {
                if (t == null || _lastImageRefresh.AddSeconds(3) <= DateTime.UtcNow)
                {
                    _lastImageRefresh = DateTime.UtcNow;
                    cts = new CancellationTokenSource();
                    t = new Task(() =>
                    {
                        while (!cts.Token.IsCancellationRequested && _lastImageRefresh.AddSeconds(3) > DateTime.UtcNow)
                        {
                            cts.Token.WaitHandle.WaitOne(500);
                            try
                            {
                                if (!WinApiUtil.IsValidHWnd((IntPtr)eA.EveHWnd))
                                {
                                    t = null;
                                    break;
                                }
                                _fontBrush = WinApiUtil.IsWindowAddedToTaskbar((IntPtr)eA.EveHWnd) ? _fgwBrush : Brushes.White;
                                RefreshImage();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                            }
                        }
                        t = null;
                        Debug.WriteLine("Task stopped.");
                    }, cts.Token);
                    t.Start();
                }
            }
        }

        public void StopUpdateTask()
        {
            lock (_taskLock)
            {
                cts.Cancel();
                Debug.WriteLine("Canceled token.");
            }
        }

        private static Font GetFont(int size)
        {
            if (!_fontDict.TryGetValue(size, out var f))
            {
                f = new Font("Tahoma", size);
                Debug.WriteLine($"Adding font size {size} to dict.");
                _fontDict[size] = f;
            }

            return f;
        }

        private Font GetSizedFont(Graphics g, string s, int containerWidth, int containerHeight)
        {
            if (_previousFontSize == 0 || _updateFontSize)
            {
                _updateFontSize = false;
                var maxFontSize = 35;
                var minFontSize = 2;
                Font newFont = null;
                for (var newSize = maxFontSize; newSize >= minFontSize; newSize--)
                {
                    newFont = GetFont(newSize);
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    var format = StringFormat.GenericTypographic;
                    format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                    var sizeNew = g.MeasureString(s, newFont, containerWidth, format);
                    if (containerWidth > Convert.ToInt32(sizeNew.Width) && containerHeight > Convert.ToInt32(sizeNew.Height))
                    {
                        _previousFontSize = newSize;
                        break;
                    }
                }
                return newFont;
            }

            return GetFont(_previousFontSize);
        }

        private void PBoxOnDoubleClick(object sender, EventArgs eventArgs)
        {
            if (eA != null && eA.EveProcessExists)
            {
                var eveHwnd = (IntPtr)eA.EveHWnd;
                if (WinApiUtil.IsWindowAddedToTaskbar(eveHwnd))
                {
                    _fontBrush = Brushes.White;
                    RefreshImage();
                    eA.HideWindows();
                }
                else
                {
                    _fontBrush = _fgwBrush;
                    RefreshImage();
                    eA.ShowWindows();
                }
            }
        }

        private void PBoxOnMouseWheel(object sender, MouseEventArgs mouseEventArgs)
        {
            if (Control.ModifierKeys == Keys.Alt ||
                Control.ModifierKeys == Keys.Control && PictureBox.ClientRectangle.Contains(PictureBox.PointToClient(Control.MousePosition)))
            {

                var z = _zoom;
                ModifyZoom(mouseEventArgs.Delta);
                if (z != _zoom)
                {
                    Task.Run(() =>
                    {
                        _updateFontSize = true;
                        rect = WinApiUtil.GetWindowRect((IntPtr)eA.EveHWnd);
                        PictureBox.Invoke(new Action(() =>
                        {
                            PictureBox.Width = (int)(rect.Width * _zoom);
                            PictureBox.Height = (int)(rect.Height * _zoom);
                        }));
                        RefreshImage();
                    });
            }
        }
        }

        private void ModifyZoom(int delta)
    {
            if (delta == 0)
                return;

            lock (_lockZoom)
        {
                if (_lastZoom.AddMilliseconds(100) > DateTime.UtcNow)
                    return;

                _lastZoom = DateTime.UtcNow;

                if (delta < 0)
        {
                    if (_zoom <= 0.9)
            {
                        _zoom += 0.1;
                        Debug.WriteLine($"Zoom {_zoom}");
                                        }
                                    }
                                    else
                                    {
                    if (_zoom >= 0.2)
                                        {
                        _zoom -= 0.1;
                        Debug.WriteLine($"Zoom {_zoom}");
                                        }
                                    }
                            }
                            }

        private void PBoxOnPaint(object sender, PaintEventArgs paintEventArgs)
        {
            try
            {
                var center = new Point(PictureBox.Width / 2, PictureBox.Height / 2);
                var borderSizeLeft = (int)(40 * _zoom);
                var borderSizeRight = (int)(40 * _zoom);
                var borderSizeTop = (int)(60 * _zoom);
                var borderSizeBot = (int)(80 * _zoom);
                var sizedFont = GetSizedFont(paintEventArgs.Graphics, eA.CharacterName, PictureBox.Width, borderSizeBot);
                var stringSize = paintEventArgs.Graphics.MeasureString(eA.CharacterName, sizedFont);
                var penBot = new Pen(Color.LightSlateGray, borderSizeBot);
                var penLeftRight = new Pen(Color.Black, borderSizeLeft);
                var penTop = new Pen(Color.Black, borderSizeTop);
                paintEventArgs.Graphics.DrawLine(penTop, 0, 0, PictureBox.Width, 0); // top
                paintEventArgs.Graphics.DrawLine(penLeftRight, 0, 0, 0, PictureBox.Height); // left
                paintEventArgs.Graphics.DrawLine(penLeftRight, PictureBox.Width, 0, PictureBox.Width, PictureBox.Height); // right
                paintEventArgs.Graphics.DrawLine(penBot, 0, PictureBox.Height, PictureBox.Width, PictureBox.Height); // bot
                paintEventArgs.Graphics.DrawString(eA.CharacterName, sizedFont, _fontBrush, PictureBox.Width / 2 - stringSize.Width / 2,
                    PictureBox.Height - stringSize.Height + stringSize.Height / 4);
                }
            catch (Exception e)
            {
                Cache.Instance.Log(e.ToString());
                Debug.WriteLine(e);
            }
        }

        #endregion Methods
    }
}