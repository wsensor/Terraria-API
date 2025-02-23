﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using XNAHelpers;

namespace ItemPlugin
{
    /// <summary>
    /// F9 = Show item editor form
    /// </summary>
    [APIVersion(1, 7)]
    public class ItemPlugin : TerrariaPlugin
    {
        public override string Name
        {
            get { return "Item Editor"; }
        }

        public override Version Version
        {
            get { return new Version(1, 0); }
        }

        public override string Author
        {
            get { return "Jaex"; }
        }

        public override string Description
        {
            get { return "Give/Edit items"; }
        }

        private ItemForm itemForm;

        public ItemPlugin(Main game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            GameHooks.LoadContent += GameHooks_LoadContent;
            GameHooks.Update += GameHooks_Update;
        }

        public override void DeInitialize()
        {
            GameHooks.LoadContent -= GameHooks_LoadContent;
            GameHooks.Update -= GameHooks_Update;
        }

        private void GameHooks_LoadContent(ContentManager obj)
        {
            itemForm = new ItemForm();
        }

        private void GameHooks_Update(GameTime gameTime)
        {
            if (Game.IsActive)
            {
                if (InputManager.IsKeyPressed(Keys.F9) && itemForm != null)
                {
                    itemForm.Visible = !itemForm.Visible;
                }
            }
        }
    }
}