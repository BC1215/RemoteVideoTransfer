using System.Windows.Forms;

namespace RemoteVideoTransfer.TestClient
{
    public partial class FrmShow : Form
    {
        public FrmShow()
        {
            SetStyle(ControlStyles.UserPaint |                      // 控件将自行绘制，而不是通过操作系统来绘制  
     ControlStyles.OptimizedDoubleBuffer |          // 该控件首先在缓冲区中绘制，而不是直接绘制到屏幕上，这样可以减少闪烁  
     ControlStyles.AllPaintingInWmPaint |           // 控件将忽略 WM_ERASEBKGND 窗口消息以减少闪烁  
     ControlStyles.ResizeRedraw |                   // 在调整控件大小时重绘控件  
     ControlStyles.SupportsTransparentBackColor |   // 控件接受 alpha 组件小于 255 的 BackColor 以模拟透明  
     ControlStyles.OptimizedDoubleBuffer,
     true);                                         // 设置以上值为 true  
            UpdateStyles();

            DoubleBuffered = true;
            InitializeComponent();
        }
    }
}
