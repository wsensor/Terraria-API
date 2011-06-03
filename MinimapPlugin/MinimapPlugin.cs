﻿using System;
using System.Drawing;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using Color = Microsoft.Xna.Framework.Color;

namespace MinimapPlugin
{
    public class MinimapPlugin : TerrariaPlugin
    {
        public override string Name
        {
            get { return "Minimap"; }
        }

        public override Version Version
        {
            get { return new Version(2, 0); }
        }

        public override Version APIVersion
        {
            get { return new Version(1, 1); }
        }

        public override string Author
        {
            get { return "High / Jaex"; }
        }

        public override string Description
        {
            get { return "Its a minimap, what do you think?"; }
        }

        private WorldRenderer rend = null;
        private InputManager input = new InputManager();
        private Texture2D minimap = null;
        private Texture2D chest;
        private Thread renderthread;
        private MinimapSettings settings;
        private MinimapForm settingsForm;

        public MinimapPlugin(Main main)
            : base(main)
        {
            settings = new MinimapSettings
            {
                MinimapWidth = 400,
                MinimapHeight = 200,
                MinimapZoom = 1.0f,
                PositionOffsetX = 0,
                PositionOffsetY = 0,
                MinimapPosition = MinimapPosition.LeftBottom,
                MinimapPositionOffset = 20,
                MinimapTransparency = 1.0f,
                ShowSky = true
            };
        }

        public override void Initialize()
        {
            renderthread = new Thread(RenderMap);
            renderthread.Start();
            GameHooks.OnLoadContent += GameHooks_OnLoadContent;
            GameHooks.OnUpdate += GameHooks_OnUpdate;
            DrawHooks.OnEndDraw += DrawHooks_OnEndDraw;
        }

        public override void DeInitialize()
        {
            renderthread = null;
            GameHooks.OnLoadContent -= GameHooks_OnLoadContent;
            GameHooks.OnUpdate -= GameHooks_OnUpdate;
            DrawHooks.OnEndDraw -= DrawHooks_OnEndDraw;
        }

        private void GameHooks_OnLoadContent(ContentManager obj)
        {
            // chest = BitmapToTexture(Game.GraphicsDevice, Properties.Resources.chest);
        }

        private void GameHooks_OnUpdate(GameTime obj)
        {
            if (Game.IsActive)
            {
                input.Update();

                if (input.IsKeyUp(Keys.F5, true))
                {
                    if (rend == null)
                    {
                        rend = new WorldRenderer(Main.tile, Main.maxTilesX, Main.maxTilesY) { SurfaceY = (int)Main.worldSurface };
                    }
                    else
                    {
                        rend = null;
                    }
                }
                else if (input.IsKeyUp(Keys.F6, true))
                {
                    if (settingsForm == null || settingsForm.IsDisposed)
                    {
                        settingsForm = new MinimapForm(settings);
                    }

                    settingsForm.Show();
                    settingsForm.BringToFront();
                }
            }
        }

        private void DrawHooks_OnEndDraw(SpriteBatch arg1)
        {
            if (Game.IsActive && rend != null && minimap != null && !Main.playerInventory)
            {
                Vector2 position;

                if (settings.MinimapPosition == MinimapPosition.RightBottom)
                {
                    position = new Vector2(Main.screenWidth - minimap.Width - settings.MinimapPositionOffset,
                        Main.screenHeight - minimap.Height - settings.MinimapPositionOffset);
                }
                else
                {
                    position = new Vector2(settings.MinimapPositionOffset, Main.screenHeight - minimap.Height - settings.MinimapPositionOffset);
                }

                Game.spriteBatch.Draw(minimap, position, new Color(1, 1, 1, settings.MinimapTransparency));
                // DrawPlayers();
            }
        }

        private void RenderMap()
        {
            while (renderthread != null)
            {
                if (rend != null)
                {
                    int curx = (int)(Main.player[Main.myPlayer].position.X / 16) + settings.PositionOffsetX;
                    int cury = (int)(Main.player[Main.myPlayer].position.Y / 16) + settings.PositionOffsetY;
                    int width = settings.MinimapWidth;
                    int height = settings.MinimapHeight;

                    int[,] img = rend.GenerateMinimap(curx, cury, width, height, settings.MinimapZoom, settings.ShowSky);

                    minimap = IntsToTexture(Game.GraphicsDevice, img, width, height);
                }

                Thread.Sleep(33);
            }
        }

        private static Texture2D BitmapToTexture(GraphicsDevice gd, Bitmap img)
        {
            int width = img.Width;
            int height = img.Height;
            int[,] ints = new int[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ints[x, y] = img.GetPixel(x, y).ToArgb();
                }
            }
            return IntsToTexture(gd, ints, width, height);
        }

        private static Texture2D IntsToTexture(GraphicsDevice gd, int[,] img, int width, int height)
        {
            Texture2D ret = new Texture2D(gd, width, height);
            int[] ints = new int[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int c = img[x, y];
                    int a = c >> 24;
                    int b = c >> 16 & 0xFF;
                    int g = c >> 8 & 0xFF;
                    int r = c & 0xFF;
                    ints[(y * width) + x] = (a << 24) | (r << 16) | (g << 8) | b;
                }
            }
            ret.SetData(ints);
            return ret;
        }

        /*private void DrawPlayers()
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                if (Main.player[i].active)
                {
                    int mex = (int)(Main.player[Main.myPlayer].position.X / 16);
                    int mey = (int)(Main.player[Main.myPlayer].position.Y / 16);
                    int targetx = (int)(Main.player[i].position.X / 16);
                    int targety = (int)(Main.player[i].position.Y / 16);

                    if (targetx < mex - 100)
                        continue;
                    if (targetx > mex + 100)
                        continue;
                    if (targety < mey - 100)
                        continue;
                    if (targety > mey + 100)
                        continue;

                    targetx = targetx - mex + 100;
                    targety = targety - mey + 100;

                    targetx -= Main.player[i].width / 2;
                    targety -= Main.player[i].height;

                    Game.spriteBatch.Draw(chest, new Vector2(Main.screenWidth - minimap.Width + targetx, Main.screenHeight - minimap.Height + targety), Color.White);
                }
            }
        }*/
    }
}