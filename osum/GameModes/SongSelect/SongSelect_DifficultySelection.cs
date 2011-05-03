﻿using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using osum.Audio;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.GameModes.SongSelect;
using OpenTK.Graphics;
using osum.GameModes.Play.Components;
using osum.Graphics.Drawables;
using osum.GameplayElements;
using System.Threading;
using osum.Graphics.Renderers;

namespace osum.GameModes
{
    public partial class SongSelectMode : GameMode
    {
        private pSpriteCollection spritesDifficultySelection = new pSpriteCollection();

        private pSprite s_ModeButtonStream;

        private pSprite s_ModeArrowLeft;
        private pSprite s_ModeArrowRight;
        private pSprite s_ModeButtonEasy;
        private pSprite s_ModeButtonExpert;
        private pText s_ModeDescriptionText;

        /// <summary>
        /// True when expert mode is not yet unlocked for the current map.
        /// </summary>
        bool mapRequiresUnlock
        {
            get { return false; }
        }

        private void showDifficultySelection()
        {
            if (s_ModeButtonStream == null)
            {
                Vector2 border = new Vector2(4, 4);

                int ypos = 140;
                float spacing = border.X;

                Vector2 buttonSize = new Vector2((GameBase.BaseSize.Width - spacing * 4) / 3f, 100);

                float currX = spacing;

                float yOffset = -40;

                s_ModeArrowLeft = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_arrow), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-150, yOffset), 0.45f, true, Color4.White);
                s_ModeArrowLeft.OnHover += delegate { s_ModeArrowLeft.ScaleTo(1.2f, 100, EasingTypes.In); };
                s_ModeArrowLeft.OnHoverLost += delegate { s_ModeArrowLeft.ScaleTo(1f, 100, EasingTypes.In); };
                s_ModeArrowLeft.OnClick += onSelectPreviousMode;

                spritesDifficultySelection.Add(s_ModeArrowLeft);

                s_ModeArrowRight = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_arrow), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(150, yOffset), 0.45f, true, Color4.DarkGray);
                s_ModeArrowRight.OnHover += delegate { s_ModeArrowRight.ScaleTo(1.2f, 100, EasingTypes.In); };
                s_ModeArrowRight.OnHoverLost += delegate { s_ModeArrowRight.ScaleTo(1f, 100, EasingTypes.In); };
                s_ModeArrowRight.OnClick += onSelectNextMode;

                s_ModeArrowRight.Rotation = 1;
                spritesDifficultySelection.Add(s_ModeArrowRight);

                s_ModeButtonStream = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_stream), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.4f, true, Color4.White) { Offset = new Vector2(0, yOffset), HandleClickOnUp = true };
                s_ModeButtonStream.OnClick += onModeButtonClick;
                spritesDifficultySelection.Add(s_ModeButtonStream);

                s_ModeButtonEasy = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_easy), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.4f, true, Color4.White) { Offset = new Vector2(-mode_button_width, yOffset), HandleClickOnUp = true };
                s_ModeButtonEasy.OnClick += onModeButtonClick;
                spritesDifficultySelection.Add(s_ModeButtonEasy);

                s_ModeButtonExpert = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_expert), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.4f, true, mapRequiresUnlock ? Color4.Gray : Color4.White) { Offset = new Vector2(mode_button_width, yOffset), HandleClickOnUp = true };
                s_ModeButtonExpert.OnClick += onModeButtonClick;
                spritesDifficultySelection.Add(s_ModeButtonExpert);

                tabController = new pTabController();
                tabController.Add(OsuTexture.songselect_tab_bar_play);
                tabController.Add(OsuTexture.songselect_tab_bar_rank);
                tabController.Add(OsuTexture.songselect_tab_bar_other);

                spritesDifficultySelection.Add(tabController.Sprites);

                s_ModeDescriptionText = new pText(string.Empty, 30, new Vector2(0, 55), new Vector2(GameBase.BaseSize.Width, 96), 1, true, Color4.White, true) { Field = FieldTypes.StandardSnapCentre, Origin = OriginTypes.Centre };
                s_ModeDescriptionText.TextAlignment = TextAlignment.Centre;

                spritesDifficultySelection.Add(s_ModeDescriptionText);

                spriteManager.Add(spritesDifficultySelection);
                spritesDifficultySelection.Sprites.ForEach(s => s.Alpha = 0);
            }

            //preview has finished loading.
            State = SelectState.DifficultySelect;

            foreach (pDrawable s in SelectedPanel.Sprites)
                s.MoveTo(new Vector2(0, 0), 500, EasingTypes.InDouble);

            tabController.Sprites.ForEach(s => s.Transform(new Transformation(new Vector2(0, -100), new Vector2(0, -100), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In)));
            tabController.Sprites.ForEach(s => s.Transform(new Transformation(new Vector2(0, 0), new Vector2(0, BeatmapPanel.PANEL_HEIGHT), Clock.ModeTime + 400, Clock.ModeTime + 1000, EasingTypes.In)));

            spritesDifficultySelection.Sprites.ForEach(s => s.FadeIn(200));

            s_Header.Transform(new Transformation(Vector2.Zero, new Vector2(0, -59), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            s_Header.Transform(new Transformation(TransformationType.Rotation, s_Header.Rotation, 0.03f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));

            s_Footer.Transform(new Transformation(new Vector2(-60, -105), Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            s_Footer.Transform(new Transformation(TransformationType.Rotation, 0.04f, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));

            updateModeSelectionArrows();
        }


        void onModeButtonClick(object sender, EventArgs e)
        {
            pDrawable d = sender as pDrawable;
            if (d == null) return;

            Difficulty newDifficulty;

            if (sender == s_ModeButtonEasy)
                newDifficulty = Difficulty.Easy;
            else if (sender == s_ModeButtonExpert)
                newDifficulty = Difficulty.Expert;
            else
                newDifficulty = Difficulty.Normal;

            if (newDifficulty == Player.Difficulty)
                onStartButtonPressed(sender, e);
            else
            {
                Player.Difficulty = newDifficulty;
                updateModeSelectionArrows();
            }
        }

        void onSelectPreviousMode(object sender, EventArgs e)
        {
            switch (Player.Difficulty)
            {
                case Difficulty.Normal:
                    Player.Difficulty = Difficulty.Easy;
                    break;
                case Difficulty.Expert:
                    Player.Difficulty = Difficulty.Normal;
                    break;
            }

            updateModeSelectionArrows();
        }

        void onSelectNextMode(object sender, EventArgs e)
        {
            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    Player.Difficulty = Difficulty.Normal;
                    break;
                case Difficulty.Normal:
                    Player.Difficulty = Difficulty.Expert;
                    break;
            }

            updateModeSelectionArrows();
        }

        const float mode_button_width = 300;
        private pSprite s_TabBarPlay;
        private pSprite s_TabBarRank;
        private pDrawable s_TabBarOther;
        private pTabController tabController;

        /// <summary>
        /// Updates the states of mode selection arrows depending on the current mode selection.
        /// </summary>
        private void updateModeSelectionArrows()
        {
            bool hasPrevious = false;
            bool hasNext = false;

            if (Player.Difficulty == Difficulty.Expert && mapRequiresUnlock)
            {
                Player.Difficulty = Difficulty.Normal;
                //todo: show an alert that this needs an unlock.
            }

            string text = null;

            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    hasNext = true;
                    difficultySelectOffset = mode_button_width;
                    text = "Toned-down difficulty. You can't fail.";
                    break;
                case Difficulty.Normal:
                    hasPrevious = true;
                    hasNext = !mapRequiresUnlock;
                    difficultySelectOffset = 0;
                    text = "Standard triple-stream gameplay. Difficulty changes based on your performance.";
                    break;
                case Difficulty.Expert:
                    hasPrevious = true;
                    difficultySelectOffset = -mode_button_width;
                    text = "Unlockable challenge.";
                    break;
            }

            s_ModeArrowLeft.Colour = hasPrevious ? Color4.White : Color4.DarkGray;
            s_ModeArrowRight.Colour = hasNext ? Color4.White : Color4.DarkGray;

            if (s_ModeDescriptionText.Text != text)
            {
                pSprite clone = s_ModeDescriptionText.Clone();
                clone.FadeOut(200);
                clone.AlwaysDraw = false;
                spriteManager.Add(clone); 

                s_ModeDescriptionText.Text = text;
                s_ModeDescriptionText.Alpha = 0;
                s_ModeDescriptionText.FadeInFromZero(200);
            }
        }

        private void leaveDifficultySelection(object sender, EventArgs args)
        {
            State = SelectState.SongSelect;

            InitializeBgm();

            GameBase.Scheduler.Add(delegate
            {
                foreach (BeatmapPanel p in panels)
                {
                    p.s_BackingPlate.HandleInput = true;

                    foreach (pDrawable d in p.Sprites)
                        d.FadeIn(200);
                }

                spritesDifficultySelection.Sprites.ForEach(s => s.FadeOut(50));

                s_Header.Transform(new Transformation(s_Header.Position, Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Header.Transform(new Transformation(TransformationType.Rotation, s_Header.Rotation, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));

                s_Footer.Transform(new Transformation(s_Footer.Position, new Vector2(-60, -105), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Footer.Transform(new Transformation(TransformationType.Rotation, 0, 0.04f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            }, true);
        }

        private void onStartButtonPressed(object sender, EventArgs args)
        {
            if (State == SelectState.Starting)
                return;

            State = SelectState.Starting;

            if (Player.Difficulty != Difficulty.Easy) s_ModeButtonEasy.FadeOut(200);
            if (Player.Difficulty != Difficulty.Normal) s_ModeButtonStream.FadeOut(200);
            if (Player.Difficulty != Difficulty.Expert) s_ModeButtonExpert.FadeOut(200);

            s_ModeArrowLeft.FadeOut(200);
            s_ModeArrowRight.FadeOut(200);

            GameBase.Scheduler.Add(delegate
            {
                Director.ChangeMode(OsuMode.SongSelect);
            }, 900);
        }
    }
}