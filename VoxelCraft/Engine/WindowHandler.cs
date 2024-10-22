﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.ComponentModel;
using System.Threading;
using VoxelCraft.Engine.Rendering.Standard;
using VoxelCraft.Rendering;
using VoxelCraft.Rendering.Standard;

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

        public int Width;
        public int Height;

        public WindowHandler(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings,
            Action onLoad, Action onClosed, Action<CancelEventArgs> onClosing, Action<ResizeEventArgs> onResize, Action<FrameEventArgs> onUpdate, Action<FrameEventArgs> onRender) : base(gameWindowSettings, nativeWindowSettings)
        {
            _onLoad = onLoad;
            _onClosed = onClosed;
            _onResize = onResize;
            _onClosing = onClosing;
            _onUpdate = onUpdate;
            _onRender = onRender;

            Width = nativeWindowSettings.Size.X;
            Height = nativeWindowSettings.Size.Y;
        }

        protected override void OnLoad()
        {
            // As of OpenTK 4.0.0 pre9.2, this is required and may be changed in the future.
            // See: https://github.com/opentk/opentk/pull/1096
            // Issue: https://github.com/opentk/opentk/issues/1118
            MakeCurrent();

            Debug.Log($"Using OpenGL {GL.GetInteger(GetPName.MajorVersion)}.{GL.GetInteger(GetPName.MinorVersion)}");
            IsFocused = true;
            InputManager.Initialize(this);
            InputManager.ChangeMouseState(true);
            PrimitiveMeshes.Init();
            StandardFonts.Init();
            StandardMaterials.Init();
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
            Width = e.Width;
            Height = e.Height;
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
