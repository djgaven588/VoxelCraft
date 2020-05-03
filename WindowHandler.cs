using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using System;

namespace VoxelCraft
{
    public class WindowHandler : GameWindow
    {
        private Action _onLoad;
        private Action _onClosed;
        private Action<ResizeEventArgs> _onResize;
        private Action<FrameEventArgs> _onUpdate;
        private Action<FrameEventArgs> _onRender;

        public WindowHandler(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, 
            Action onLoad, Action onClosed, Action<ResizeEventArgs> onResize, Action<FrameEventArgs> onUpdate, Action<FrameEventArgs> onRender) : base(gameWindowSettings, nativeWindowSettings)
        {
            _onLoad = onLoad;
            _onClosed = onClosed;
            _onResize = onResize;
            _onUpdate = onUpdate;
            _onRender = onRender;
        }

        protected override void OnLoad()
        {
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
            _onRender?.Invoke(args);
            SwapBuffers();
        }
    }
}
