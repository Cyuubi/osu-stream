﻿using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;
using osum.GameplayElements;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Input;

namespace osum
{
    class Game : GameWindow
    {
        private SpriteManager sm = new SpriteManager();

        /// <summary>Creates a 1024x768 window with the specified title.</summary>
        public Game()
            : base(Constants.GamefieldDefaultWidth, Constants.GamefieldDefaultHeight, GraphicsMode.Default, "osu!m")
        {
            VSync = VSyncMode.On;
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //GL.Enable(EnableCap.DepthTest);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // enabling and disabling the following block changes nothing
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            //GL.Enable(EnableCap.ColorMaterial);
            //GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Emission);

            /*
            pSprite sprite = new pSprite(SkinManager.Load("puush"), OriginType.TopLeft, new Vector2(50, 200), new Color4(255, 255, 255, 255));
            sprite.Transform(new Transform(new Vector2(50, 200), new Vector2(370, 115), 0, 1000, EasingType.Out));
            sprite.Transform(new Transform(new Vector2(370, 115), new Vector2(350, 120), 1000, 1500, EasingType.In));
            sprite.Transform(new Transform(TransformType.Fade, 0, 1, 0, 1500));
            sprite.Transform(new Transform(Color4.Black, Color4.White, 0, 1000, EasingType.In));
            */

            //HitCircle h = new HitCircle(new Vector2(512, 384), 2500, true);
            //Spinner h = new Spinner(1500, 6000, HitObjectSoundType.Normal);
            //sm.Add(h);

            InputManager.Initialise(Mouse);
            InputManager.MouseClick += mc;
        }

        void mc(object sender, MouseButtonEventArgs e)
        {
            sm.Add(new HitCircle(new Vector2(e.X, e.Y), Clock.Time + 1500, true));
        }

        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, 1024, 768);

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, 1024, 768, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();

            // global clock
            Clock.Update(e.Time);

            sm.Update();
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //ensure the gl context is in the current thread.
            MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color4.MidnightBlue);

            GL.MatrixMode(MatrixMode.Modelview);

            //GL.Viewport(0, 0, Size.Width, Size.Height);
            //Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            //GL.LoadIdentity();
            //unnecessary?

            sm.Draw();
            
            // display
            SwapBuffers();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Game game = new Game())
            {
                game.Run(60);
            }
        }
    }
}