using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using System;
using System.ComponentModel;
using System.Threading;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public class WindowHandler : GameWindow
    {
        private readonly Action _onLoad;
        private readonly Action _onClosed;
        private readonly Action<ResizeEventArgs> _onResize;
        private readonly Action<FrameEventArgs> _onUpdate;
        private readonly Action<FrameEventArgs> _onRender;
        private readonly Action<CancelEventArgs> _onClosing;

        public WindowHandler(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings,
            Action onLoad, Action onClosed, Action<CancelEventArgs> onClosing, Action<ResizeEventArgs> onResize, Action<FrameEventArgs> onUpdate, Action<FrameEventArgs> onRender) : base(gameWindowSettings, nativeWindowSettings)
        {
            _onLoad = onLoad;
            _onClosed = onClosed;
            _onResize = onResize;
            _onClosing = onClosing;
            _onUpdate = onUpdate;
            _onRender = onRender;
        }

        protected override void OnLoad()
        {
            IsFocused = true;
            PrimitiveMeshes.Init();
            _onLoad?.Invoke();
        }

        protected override void OnClosed()
        {
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            RenderDataHandler.ClearAllRenderData();
            _onClosed?.Invoke();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _onResize?.Invoke(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            _onUpdate?.Invoke(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Graphics.ClearQueue();
            InputManager.UpdateInput(this);

            _onRender?.Invoke(args);

            SwapBuffers();

            if (args.Time < 1000d / RenderFrequency)
            {
                Thread.Sleep((int)(Math.Round(1000 / RenderFrequency - args.Time) * 0.9));
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _onClosing?.Invoke(e);
        }
    }
}
