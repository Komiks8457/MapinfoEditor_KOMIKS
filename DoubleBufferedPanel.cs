using System.ComponentModel;
using System.Windows.Forms;

namespace MapinfoEditor_KOMIKS
{
    public sealed class DoubleBufferedPanel : Panel
    {
        private IContainer components = (IContainer) null;
        
        public DoubleBufferedPanel()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
              components.Dispose();

            base.Dispose(disposing);
        }
        
        private void InitializeComponent() => components = (IContainer) new Container();
    }
}
