using System.Drawing;

namespace MapinfoEditor_KOMIKS
{
    internal class MapRegion
    {
        private readonly int x;
        private readonly int y;
        private STATE state;
        private int drawX;
        private int drawY;
    
        public MapRegion()
        {
            x = 0;
            y = 0;
            state = STATE.DISABLED;
            drawX = 0;
            drawY = 0;
        }
    
        public MapRegion(int x, int y)
        {
            this.x = x;
            this.y = y;
            state = STATE.DISABLED;
            drawX = 0;
            drawY = 0;
        }
    
        public void ToggleState()
        {
            state = state == STATE.ENABLED ? STATE.DISABLED : STATE.ENABLED;
        }
    
        public bool Inside(Point p) => p.X >= drawX && p.X <= drawX + Map.regionDrawSize && p.Y >= drawY && p.Y <= drawY + Map.regionDrawSize;
    
        public int X => x;
        public int Y => y;
    
        public STATE State
        {
            get => state;
            set => state = value;
        }
    
        public int DrawX
        {
            get => drawX;
            set => drawX = value;
        }
    
        public int DrawY
        {
            get => drawY;
            set => drawY = value;
        }
    
        public enum STATE
        {
            ENABLED,
            DISABLED
        }
    }
}
