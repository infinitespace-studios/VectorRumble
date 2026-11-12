using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VectorRumble
{
#if DEBUG || PERF_OVERLAY
    /// <summary>
    /// A small on-screen readout of frame time, CPU time, garbage collection and draw calls, for finding out
    /// where the time goes on a real device. It is only compiled into Debug builds, or into any build made with
    /// <c>-p:PerfOverlay=true</c> (use that to profile a Release build, which is what players run). Everywhere
    /// else this class is an empty stub, so the calls to it cost nothing and need no #if.
    /// </summary>
    /// <remarks>
    /// F3 shows and hides it. A line summarising each few seconds also goes to the console (logcat on Android,
    /// tagged "[perf]"), so a run can be read back without a screenshot.
    /// </remarks>
    internal sealed class PerfOverlay
    {
        // how often the figures are refreshed, and how many refreshes between lines in the console
        const double WindowSeconds = 1.0;
        const int LogEveryWindows = 5;

        static readonly double TicksToMs = 1000.0 / Stopwatch.Frequency;

        readonly StringBuilder text = new StringBuilder(256);
        readonly string[] lines = new string[5];

        public bool Visible { get; set; } = true;

        // the small font is drawn at its own size: scaling a pixel font down turns it to mush
        SpriteFont font;

        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("Fonts/retroSmall");
        }

        // timing of the current window
        long windowStart = Stopwatch.GetTimestamp();
        long lastFrameStart;
        long sectionStart;
        int frames;
        double frameMsSum, frameMsMax;
        double updateMsSum, drawMsSum;

        // GC and graphics, over the current window
        int gc0, gc1, gc2;
        long allocatedBytes;
        long drawCalls, primitives, sprites;

        // the finished window, which is what is shown
        double fps, frameMsAvg, frameMsWorst, updateMsAvg, drawMsAvg;
        int gcGen0, gcGen1, gcGen2;
        double allocKbPerSec, callsPerFrame, primsPerFrame, spritesPerFrame;
        double heapMb;
        int windows;

        public PerfOverlay()
        {
            gc0 = GC.CollectionCount(0);
            gc1 = GC.CollectionCount(1);
            gc2 = GC.CollectionCount(2);
            allocatedBytes = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < lines.Length; i++)
                lines[i] = string.Empty;
        }

        public void HandleInput(InputState input)
        {
            if (input.IsNewKeyPress(Keys.F3))
                Visible = !Visible;
        }

        public void BeginUpdate()
        {
            sectionStart = Stopwatch.GetTimestamp();
        }

        public void EndUpdate()
        {
            updateMsSum += (Stopwatch.GetTimestamp() - sectionStart) * TicksToMs;
        }

        public void BeginDraw()
        {
            long now = Stopwatch.GetTimestamp();
            if (lastFrameStart != 0)
            {
                double ms = (now - lastFrameStart) * TicksToMs;
                frameMsSum += ms;
                if (ms > frameMsMax)
                    frameMsMax = ms;
            }
            lastFrameStart = now;
            sectionStart = now;
        }

        /// <summary>Call once the game has been drawn, before the overlay itself is.</summary>
        public void EndDraw(GraphicsDevice device)
        {
            drawMsSum += (Stopwatch.GetTimestamp() - sectionStart) * TicksToMs;
            frames++;

            GraphicsMetrics metrics = device.Metrics;
            drawCalls += metrics.DrawCount;
            primitives += metrics.PrimitiveCount;
            sprites += metrics.SpriteCount;
            device.Metrics = new GraphicsMetrics();

            double elapsed = (Stopwatch.GetTimestamp() - windowStart) * TicksToMs / 1000.0;
            if (elapsed >= WindowSeconds)
                CloseWindow(elapsed);
        }

        void CloseWindow(double elapsed)
        {
            // frame times are measured between frames, so there is one fewer of them than frames
            int measured = Math.Max(frames - 1, 1);
            fps = frames / elapsed;
            frameMsAvg = frameMsSum / measured;
            frameMsWorst = frameMsMax;
            updateMsAvg = updateMsSum / Math.Max(frames, 1);
            drawMsAvg = drawMsSum / Math.Max(frames, 1);
            callsPerFrame = drawCalls / (double)Math.Max(frames, 1);
            primsPerFrame = primitives / (double)Math.Max(frames, 1);
            spritesPerFrame = sprites / (double)Math.Max(frames, 1);

            int c0 = GC.CollectionCount(0), c1 = GC.CollectionCount(1), c2 = GC.CollectionCount(2);
            gcGen0 = c0 - gc0;
            gcGen1 = c1 - gc1;
            gcGen2 = c2 - gc2;
            gc0 = c0;
            gc1 = c1;
            gc2 = c2;

            long allocated = GC.GetAllocatedBytesForCurrentThread();
            allocKbPerSec = (allocated - allocatedBytes) / 1024.0 / elapsed;
            allocatedBytes = allocated;
            heapMb = GC.GetTotalMemory(false) / (1024.0 * 1024.0);

            lines[0] = string.Format("{0:0} fps  frame {1:0.0} ms  worst {2:0.0} ms", fps, frameMsAvg, frameMsWorst);
            lines[1] = string.Format("cpu: update {0:0.00} ms  draw {1:0.00} ms", updateMsAvg, drawMsAvg);
            lines[2] = string.Format("gc/s: g0 {0}  g1 {1}  g2 {2}", gcGen0, gcGen1, gcGen2);
            lines[3] = string.Format("alloc {0:0} KB/s  heap {1:0.0} MB", allocKbPerSec, heapMb);
            lines[4] = string.Format("gpu: {0:0.0} draws  {1:0} prims  {2:0} sprites /frame",
                callsPerFrame, primsPerFrame, spritesPerFrame);

            if (++windows % LogEveryWindows == 0)
                Console.WriteLine("[perf] " + string.Join(" | ", lines));

            windowStart = Stopwatch.GetTimestamp();
            frames = 0;
            frameMsSum = frameMsMax = updateMsSum = drawMsSum = 0;
            drawCalls = primitives = sprites = 0;
        }

        /// <summary>Draws the readout, in the game's own units, in the top left corner.</summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D blank)
        {
            if (!Visible || font == null)
                return;

            int lineHeight = font.LineSpacing;
            int width = 0;
            for (int i = 0; i < lines.Length; i++)
                width = Math.Max(width, (int)Math.Ceiling(font.MeasureString(lines[i]).X));

            // whole pixels only, so the glyphs stay sharp
            Point origin = new Point(12, 12);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(blank, new Rectangle(origin.X - 6, origin.Y - 6,
                width + 12, lineHeight * lines.Length + 12), new Color(0, 0, 0, 190));
            for (int i = 0; i < lines.Length; i++)
            {
                // a worst frame over about 33 ms (a dropped frame at 30 fps) shows red
                Color color = (i == 0 && frameMsWorst > 33.0) ? Color.OrangeRed : Color.Lime;
                spriteBatch.DrawString(font, lines[i], new Vector2(origin.X, origin.Y + i * lineHeight), color);
            }
            spriteBatch.End();
        }
    }
#else
    /// <summary>The overlay is compiled out of this build; see the other half of this file.</summary>
    internal sealed class PerfOverlay
    {
        public bool Visible { get; set; }
        public void LoadContent(ContentManager content) { }
        public void HandleInput(InputState input) { }
        public void BeginUpdate() { }
        public void EndUpdate() { }
        public void BeginDraw() { }
        public void EndDraw(GraphicsDevice device) { }
        public void Draw(SpriteBatch spriteBatch, Texture2D blank) { }
    }
#endif
}
