﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes.SongSelect;
using osum.GameplayElements.Beatmaps;
using osum.GameplayElements;
using osum.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using osum.Graphics.Renderers;
using osum.Support;
using osum.GameplayElements.Scoring;
using osum.GameModes.Play.Components;
using osum.Audio;
using osum.GameplayElements.HitObjects.Osu;
using osum.Graphics.Skins;

namespace osum.GameModes.Play
{
    class Tutorial : Player
    {
        BackButton backButton;
        pText touchToContinueText;

        const int music_offset = 3050;
        const int music_beatlength = 375;

        public override void Initialize()
        {
            Difficulty = Difficulty.None;
            Beatmap = null;

            MainMenu.InitializeBgm();

            base.Initialize();

            touchToContinueText = new pText(osum.Resources.Tutorial.TapToContinue, 30, new Vector2(0, 20), 1, true, Color4.YellowGreen)
            {
                TextBounds = new Vector2(GameBase.BaseSizeFixedWidth.Width * 0.8f, 0),
                Field = FieldTypes.StandardSnapBottomCentre,
                TextShadow = true,
                Bold = true,
                Origin = OriginTypes.BottomCentre
            };

            topMostSpriteManager.Add(touchToContinueText);

            Beatmap = new Beatmap();
            Beatmap.ControlPoints.Add(new ControlPoint(music_offset, music_beatlength, TimeSignatures.SimpleQuadruple, SampleSet.Normal, CustomSampleSet.Default, 100, true, false));

            backButton = new BackButton(delegate { Director.ChangeMode(OsuMode.MainMenu); });
            backButton.Alpha = 0;
            topMostSpriteManager.Add(backButton);

            loadNextSegment();
        }

        public override void Dispose()
        {
            Player.Autoplay = false;
            base.Dispose();
        }

        TutorialSegments currentSegment;
        TutorialSegments nextSegment = TutorialSegments.Introduction_1;

        VoidDelegate currentSegmentDelegate;

        bool touchToContinue = true;
        private void showTouchToContinue(bool showBackButton = true)
        {
            if (touchToContinue)
                return;

            touchToContinue = true;

            GameBase.Scheduler.Add(delegate
            {
                if (showBackButton)
                    backButton.FadeIn(1000, 0.3f);

                touchToContinueText.Transformations.Clear();
                touchToContinueText.Transform(new Transformation(TransformationType.Fade, 1, 0, Clock.ModeTime + 600, Clock.ModeTime + 1400, EasingTypes.In) { LoopDelay = 600, Looping = true });
            }, 400);
        }

        protected override void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            if (touchToContinue && !backButton.IsHovering)
            {
                if (touchToContinueText.Transformations.Count > 0)
                    loadNextSegment();
                return;
            }

            base.InputManager_OnDown(source, point);
        }

        private pText showText(string text, float verticalOffset = 0)
        {
            pText pt = new pText(text, 30, new Vector2(0, verticalOffset), new Vector2(GameBase.BaseSize.Width * 0.9f, 0), 1, true, Color4.White, true)
            {
                Field = FieldTypes.StandardSnapCentre,
                TextAlignment = TextAlignment.Centre,
                Origin = OriginTypes.Centre
            };

            pt.ScaleScalar = 1.4f;
            pt.FadeIn(300);
            pt.ScaleTo(1, 400, EasingTypes.In);
            tutorialSegmentManager.Add(pt);

            return pt;
        }

        public override bool Draw()
        {
            base.Draw();

            tutorialSegmentManager.Draw();

            return true;
        }

        int lastFrameBeat;
        int currentBeat;
        public override void Update()
        {
            if (!AudioEngine.Music.IsElapsing && !Failed)
                AudioEngine.Music.Play();

            lastFrameBeat = currentBeat;
            currentBeat = (Clock.AudioTime - music_offset) / music_beatlength;

            if (currentSegmentDelegate != null) currentSegmentDelegate();

            base.Update();

            tutorialSegmentManager.Update();
        }

        protected override void initializeUIElements()
        {
            //base.initializeUIElements();
        }

        enum TutorialSegments
        {
            None,
            Introduction_1,
            Introduction_2,
            Introduction_3,
            Introduction_4,
            HitCircle_1,
            HitCircle_2,
            HitCircle_3,
            HitCircle_4,
            HitCircle_5,
            HitCircle_6,
            HitCircle_Interact,
            HitCircle_Judge,
            Hold_1,
            Hold_2,
            Hold_Interact,
            Hold_Judge,
            Slider_1,
            Slider_2,
            Slider_3,
            Slider_4,
            Slider_Interact,
            Slider_Judge,
            Spinner_1,
            Spinner_2,
            Spinner_3,
            Spinner_4,
            Spinner_Interact,
            Spinner_Judge,
            Multitouch_1,
            Multitouch_2,
            Multitouch_3,
            Multitouch_Interact,
            Multitouch_Judge,
            Stacked_1,
            Stacked_2,
            Stacked_3,
            Stacked_Interact,
            Stacked_Judge,
            Stream_1,
            Stream_2,
            Stream_3,
            Stream_4,
            Stream_5,
            Healthbar_1,
            Healthbar_2,
            Healthbar_3,
            Healthbar_4,
            Healthbar_5,
            Healthbar_End,
            Score_1,
            Score_2,
            Score_3,
            Score_4,
            Outro,
            End,
        }

        SpriteManager tutorialSegmentManager = new SpriteManager();

        private HitObject sampleHitObject;

        private void loadNextSegment()
        {
            loadSegment(nextSegment);
        }

        private void loadSegment(TutorialSegments segment)
        {
            currentSegment = segment;

            nextSegment = (TutorialSegments)(currentSegment + 1);

            foreach (pDrawable p in tutorialSegmentManager.Sprites)
            {
                p.AlwaysDraw = false;
                p.FadeOut(100);
                p.ScaleTo(0.9f, 400, EasingTypes.In);
            }

            tutorialSegmentManager.Sprites.Clear();

            currentSegmentDelegate = null;

            touchToContinueText.Transformations.Clear();
            touchToContinueText.Alpha = 0;

            touchToContinue = false;

            backButton.FadeOut(200);

            switch (currentSegment)
            {
                case TutorialSegments.Introduction_1:
                    showText(osum.Resources.Tutorial.WelcomeToTheWorldOfOsu);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Introduction_2:
                    showText("osu!stream is a game which requires both rhythmical and positional accuracy.");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Introduction_3:
                    showText("You will need to feel the beat, so make sure you are using headphones or playing in quiet surroundings!");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Introduction_4:
                    showText("Let's start by looking at the different kinds of beats.");
                    showTouchToContinue();
                    break;
                case TutorialSegments.HitCircle_1:
                    resetScore();
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    showText("\"Hit circles\" are the most basic beat in osu!.");
                    showTouchToContinue();
                    break;
                case TutorialSegments.HitCircle_2:
                    {
                        showText("They are made up of the main circle...", -50);

                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(null);

                        sampleHitObject = new HitCircle(HitObjectManager, new Vector2(256, 197), 0, true, 0, HitObjectSoundType.Normal);
                        sampleHitObject.Colour = Color4.OrangeRed;
                        sampleHitObject.ComboNumber = 1;

                        sampleHitObject.Sprites.ForEach(s =>
                        {
                            s.AlwaysDraw = true;
                            s.Transformations.Clear();
                            s.Clocking = ClockTypes.Mode;
                            s.FadeInFromZero(200);
                        });

                        HitCircle c = sampleHitObject as HitCircle;
                        c.SpriteApproachCircle.Alpha = 0;
                        c.SpriteApproachCircle.Transformations.Clear();

                        HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);

                        showTouchToContinue();
                    }
                    break;
                case TutorialSegments.HitCircle_3:
                    {
                        showText("And the approach circle.", -80);

                        HitCircle c = sampleHitObject as HitCircle;
                        c.SpriteHitCircle1.FadeColour(ColourHelper.Darken(c.Colour, 0.3f), 200);
                        c.SpriteApproachCircle.ScaleScalar = 4;
                        c.SpriteApproachCircle.FadeIn(200);

                        showTouchToContinue();
                    }
                    break;
                case TutorialSegments.HitCircle_4:
                    {
                        showText("When the approach circle reaches the border of the main circle...", -80);

                        HitCircle c = sampleHitObject as HitCircle;

                        c.SpriteApproachCircle.Alpha = 1;
                        c.SpriteApproachCircle.ScaleTo(1, 4000);
                        c.SpriteHitCircle1.FadeColour(ColourHelper.Lighten(c.Colour, 0.7f), 4000);

                        currentSegmentDelegate = delegate
                        {
                            if (c.SpriteApproachCircle.Transformations.Count == 0)
                            {
                                if (!touchToContinue)
                                {
                                    AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                                    showText("...you should tap!", 80).Colour = Color4.Yellow;
                                    showTouchToContinue();
                                }

                                pDrawable lastFlash = null;
                                currentSegmentDelegate = delegate
                                {
                                    if (lastFlash == null || lastFlash.Alpha == 0)
                                        lastFlash = c.SpriteApproachCircle.AdditiveFlash(1000, 1).ScaleTo(1.4f, 1000);
                                };
                            }
                        };
                    }
                    break;
                case TutorialSegments.HitCircle_5:
                    {
                        showText("The more accurate your timing, the more points you get!", -90);

                        HitCircle c = sampleHitObject as HitCircle;

                        c.SpriteApproachCircle.FadeOut(200);

                        GameBase.Scheduler.Add(delegate
                        {
                            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                            showText("Good...", 80).FadeOut(1000);
                            c.HitAnimation(ScoreChange.Hit50);
                        }, 2000);

                        GameBase.Scheduler.Add(delegate
                        {
                            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                            AudioEngine.PlaySample(OsuSamples.HitWhistle, SampleSet.Normal);
                            showText("Great!", 90).FadeOut(1000);
                            c.HitAnimation(ScoreChange.Hit100);
                        }, 3000);

                        GameBase.Scheduler.Add(delegate
                        {
                            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                            AudioEngine.PlaySample(OsuSamples.HitFinish, SampleSet.Normal);
                            showText("Perfect!", 100).FadeOut(2000);
                            c.HitAnimation(ScoreChange.Hit300);
                        }, 4000);

                        GameBase.Scheduler.Add(delegate
                        {
                            loadNextSegment();
                        }, 6500);
                    }
                    break;
                case TutorialSegments.HitCircle_6:
                    showText("Okay. Let's give it a shot!\nTry hitting these 8 hit circles.");
                    showTouchToContinue();
                    break;
                case TutorialSegments.HitCircle_Interact:
                    {
                        prepareInteract();
                        HitObjectManager.OnScoreChanged += new ScoreChangeDelegate(hitObjectManager_OnScoreChanged);

                        const int x1 = 100;
                        const int x2 = 512 - 100;
                        const int y1 = 80;
                        const int y2 = 384 - 80;

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 164 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 168 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 172 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 180 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 184 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 188 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.PostProcessing();

                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        currentSegmentDelegate = delegate
                        {
                            if (!touchToContinue && HitObjectManager.AllNotesHit)
                                loadNextSegment();
                        };
                    }
                    break;
                case TutorialSegments.HitCircle_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    if (CurrentScore.countMiss > 2)
                    {
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                        showText("Hmm, looks like we need to practise a bit more. Let's go over this again!");
                        nextSegment = TutorialSegments.HitCircle_1;
                    }
                    else if (CurrentScore.count50 > 1 || CurrentScore.countMiss > 1)
                    {
                        showText("Getting there!\nWatch the approaching circle carefully and listen to the beat. Let's try once more!");
                        nextSegment = TutorialSegments.HitCircle_Interact;
                    }
                    else if (CurrentScore.count100 + CurrentScore.count50 + CurrentScore.countMiss > 0)
                    {
                        showText("That's right!\nFocus on the beat of the song and try to time your taps to get higher accuracy.");
                    }
                    else
                    {
                        showText("Flawless! Great job!");
                    }

                    showTouchToContinue();
                    break;
                case TutorialSegments.Hold_1:
                    {
                        resetScore();
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);
                        Player.Autoplay = true;

                        Clock.ResetManual();
                        Clock.ManualTime = 0;

                        showText("\"Holds\" are like hit circles, but you need to tap...", -110);

                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(null);

                        sampleHitObject = new HoldCircle(HitObjectManager, new Vector2(256, 197), 1000, true, 0, HitObjectSoundType.Normal, 50, 20, null, 800, 10);
                        //arbitrary

                        sampleHitObject.Clocking = ClockTypes.Manual;

                        sampleHitObject.Colour = Color4.White;
                        sampleHitObject.ComboNumber = 1;

                        HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);

                        bool hasShownText = false;

                        currentSegmentDelegate = delegate
                        {
                            Clock.IncrementManual(0.5f);

                            if (sampleHitObject.IsActive && !hasShownText)
                            {
                                showText(osum.Resources.Tutorial.AndHoldUntilTheCircleExplodes, 100);
                                hasShownText = true;
                            }

                            if (Clock.ManualTime > 3000 && !touchToContinue)
                                showTouchToContinue();

                            sampleHitObject.HitAnimation(sampleHitObject.CheckScoring());
                            sampleHitObject.Update();
                        };

                    }
                    break;
                case TutorialSegments.Hold_2:
                    showText("Let's try a few holds!");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Hold_Interact:
                    {
                        prepareInteract();


                        const int x1 = 100;
                        const int x2 = 512 - 100;
                        const int y1 = 80;
                        const int y2 = 384 - 80;

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 168 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 184 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 192 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 196 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 200 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 204 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 208 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 212 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 216 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 220 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);

                        HitObjectManager.PostProcessing();

                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        bool warned = false;

                        currentSegmentDelegate = delegate
                        {
                            if (!touchToContinue && HitObjectManager.AllNotesHit)
                                loadNextSegment();

                            if (Clock.AudioTime > music_offset + 188 * music_beatlength && !warned)
                            {
                                warned = true;
                                pText t = showText("Now with two fingers!");
                                t.Transform(new Transformation(TransformationType.Fade, 1, 0, t.ClockingNow + music_beatlength * 4, t.ClockingNow + music_beatlength * 5));
                            }
                        };
                    }
                    break;
                case TutorialSegments.Hold_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 3 || CurrentScore.count50 > 5)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText("Make sure you hold the notes until they explode! Let's go over the basics again.");
                            nextSegment = TutorialSegments.Hold_1;
                        }
                        else if (CurrentScore.count100 > 0)
                        {
                            showText("Yeah, just like that. Make sure to hold them until they explode!");
                        }
                        else
                        {
                            showText("Perfect.");
                        }

                        showTouchToContinue();
                    }, 500);
                    break;


                case TutorialSegments.Slider_1:
                    {
                        Clock.ResetManual();
                        Player.Autoplay = true;

                        showText("\"Sliders\" are like hit circles,", -80);

                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(null);

                        sampleHitObject = new Slider(HitObjectManager, new Vector2(100, 192), 2000, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 0, 300, new List<Vector2>() { new Vector2(100, 192), new Vector2(400, 192) }, null, 200, 40);


                        sampleHitObject.Colour = TextureManager.DefaultColours[0];
                        sampleHitObject.ComboNumber = 1;

                        sampleHitObject.Clocking = ClockTypes.Manual;

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 1500)
                                Clock.IncrementManual(0.5f);
                            else
                                showTouchToContinue();
                            sampleHitObject.CheckScoring();
                            sampleHitObject.Update();
                        };

                        HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);

                        GameBase.Scheduler.Add(delegate
                        {
                            showText("but extend into tracks.", 80);
                        }, 1000);

                    }
                    break;
                case TutorialSegments.Slider_2:
                    {
                        showText("Touch it like a circle...", -100);

                        GameBase.Scheduler.Add(delegate
                        {
                            showText("then follow the ball with your finger to the end!", 120);
                        }, 1000);

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < sampleHitObject.EndTime + 500)
                                Clock.IncrementManual(0.5f);
                            else
                                showTouchToContinue();

                            sampleHitObject.HitAnimation(sampleHitObject.CheckScoring());
                            sampleHitObject.Update();
                        };

                    }
                    break;
                case TutorialSegments.Slider_3:
                    showText("Some sliders need to be repeated.", -80);

                    Clock.ResetManual();

                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = new HitObjectManager(null);

                    sampleHitObject = new Slider(HitObjectManager, new Vector2(100, 192), 2000, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 2, 300, new List<Vector2>() { new Vector2(100, 192), new Vector2(400, 192) }, null, 200, 40);


                    sampleHitObject.Colour = TextureManager.DefaultColours[0];
                    sampleHitObject.ComboNumber = 1;

                    sampleHitObject.Clocking = ClockTypes.Manual;

                    pText arrowAtEnd = null;

                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 5800)
                            Clock.IncrementManual(0.5f);
                        else if (!touchToContinue)
                        {
                            showTouchToContinue();
                            arrowAtEnd.FadeOut(50);
                            showText("Sometimes you will need to repeat more than once.", 20).Colour = Color4.SkyBlue;
                        }

                        sampleHitObject.CheckScoring();
                        sampleHitObject.Update();
                    };

                    HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);

                    GameBase.Scheduler.Add(delegate
                    {
                        arrowAtEnd = showText("This will be indicated by an arrow at the end.", 120);
                    }, 1000);

                    break;
                case TutorialSegments.Slider_4:
                    showText("Let's try some sliders!");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Slider_Interact:
                    prepareInteract();

                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(50, 92), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 0, 300, new List<Vector2>() { new Vector2(50, 92), new Vector2(200, 70), new Vector2(350, 92) }, null, 200, 300f / 8), Difficulty.Easy);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(512 - 50, 384 - 92), music_offset + 168 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 0, 300, new List<Vector2>() { new Vector2(512 - 50, 384 - 92), new Vector2(512 - 200, 384 - 70), new Vector2(512 - 350, 384 - 92) }, null, 200, 300f / 8), Difficulty.Easy);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(50, 50), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 0, 300, new List<Vector2>() { new Vector2(50, 50), new Vector2(50, 350) }, null, 200, 300f / 8), Difficulty.Easy);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(512 - 50, 50), music_offset + 184 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 2, 300, new List<Vector2>() { new Vector2(512 - 50, 50), new Vector2(512 - 50, 350) }, null, 200, 300f / 8), Difficulty.Easy);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);

                    currentSegmentDelegate = delegate
                    {
                        if (!touchToContinue && HitObjectManager.AllNotesHit)
                            loadNextSegment();
                    };

                    break;
                case TutorialSegments.Slider_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 1 || CurrentScore.count50 > 3)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText("Make sure you follow the ball with your finger! Let's go over the basics again.");
                            nextSegment = TutorialSegments.Slider_1;
                        }
                        else if (CurrentScore.count50 > 2)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText("Make sure you follow the ball with your finger! Let's try once more.");
                            nextSegment = TutorialSegments.Slider_Interact;
                        }
                        else if (CurrentScore.count100 > 0)
                        {
                            showText("Yeah, just like that. Make sure to watch and follow the ball!");
                        }
                        else
                        {
                            showText("Perfect.");
                        }

                        showTouchToContinue();
                    }, 500);
                    break;



                case TutorialSegments.Spinner_1:
                    resetScore();

                    showText("\"Spinners\" are the only beats which are not rhythmical.");
                    showTouchToContinue();
                    break;

                case TutorialSegments.Spinner_2:
                    Player.Autoplay = true;
                    showText("When a spinner appears...", -140);
                    {
                        Spinner sp = null;

                        GameBase.Scheduler.Add(delegate
                        {

                            if (HitObjectManager != null) HitObjectManager.Dispose();
                            HitObjectManager = new HitObjectManager(null);

                            sampleHitObject = new Spinner(HitObjectManager, 0, 9999999, HitObjectSoundType.Normal);

                            sampleHitObject.Sprites.ForEach(s =>
                            {
                                s.AlwaysDraw = true;
                                s.Transformations.Clear();
                                s.Clocking = ClockTypes.Mode;
                                s.FadeInFromZero(500);
                            });

                            sp = sampleHitObject as Spinner;
                            sp.rotationRequirement = 5 * Spinner.sensitivity_modifier;
                            sp.SpriteClear.Transformations.Clear();
                            sp.SpriteSpin.Transformations.Clear();
                            sp.ApproachCircle.Transformations.Clear();

                            HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);
                        }, 400);

                        GameBase.Scheduler.Add(delegate
                        {
                            showText("..you should spin it with your finger until the bars fill!", 80);

                            GameBase.Scheduler.Add(delegate
                            {

                                currentSegmentDelegate = delegate
                                {
                                    sp.Update();

                                    if (!sp.Cleared)
                                    {
                                        sp.velocityFromInputPerMillisecond = 0.02f;
                                        sp.CheckScoring();
                                    }
                                    else
                                    {
                                        sp.StopSound();
                                        showTouchToContinue();
                                    }



                                };
                            }, 700);
                        }, 800);
                    }
                    break;
                case TutorialSegments.Spinner_3:
                    showText("Spin faster for a bonus!", -140);
                    {
                        Spinner sp = sampleHitObject as Spinner;

                        currentSegmentDelegate = delegate
                        {
                            if (sp.BonusScore < 1000)
                            {
                                sp.velocityFromInputPerMillisecond = 0.04f;
                                sp.Update();
                                sp.CheckScoring();
                            }
                            else
                            {
                                if (!touchToContinue)
                                {
                                    showText("But make sure you are ready for the beats after the spinner!", 80);
                                    sp.StopSound();
                                    showTouchToContinue();
                                }
                            }
                        };
                    }
                    break;
                case TutorialSegments.Spinner_4:
                    sampleHitObject.Sprites.ForEach(s => { s.Transformations.Clear(); s.AlwaysDraw = false; });
                    showText("Let's try some spinners!");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Spinner_Interact:
                    prepareInteract();

                    HitObjectManager.Add(new Spinner(HitObjectManager, music_offset + 160 * music_beatlength, music_offset + 164 * music_beatlength, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new Spinner(HitObjectManager, music_offset + 168 * music_beatlength, music_offset + 172 * music_beatlength, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new Spinner(HitObjectManager, music_offset + 176 * music_beatlength, music_offset + 188 * music_beatlength, HitObjectSoundType.Normal), Difficulty);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);

                    currentSegmentDelegate = delegate
                    {
                        if (!touchToContinue && HitObjectManager.AllNotesHit)
                            loadNextSegment();
                    };

                    break;
                case TutorialSegments.Spinner_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 1)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText("Are you actually trying? All you need to do is make circles with your finger! Let's try again...");
                            nextSegment = TutorialSegments.Spinner_Interact;
                        }
                        else if (CurrentScore.count50 > 0 || CurrentScore.count100 > 0 || CurrentScore.countMiss > 0)
                        {
                            showText("You're spinning, but a bit slow. Let's try once more!");
                            nextSegment = TutorialSegments.Spinner_Interact;
                        }
                        else
                        {
                            showText("You spin like a TORNADO!");
                        }

                        showTouchToContinue();
                    }, 500);
                    break;
                case TutorialSegments.Multitouch_1:
                    {
                        Clock.ResetManual();
                        Player.Autoplay = true;

                        showText("Some beats need to be hit at the same time.", -100);

                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(Beatmap);

                        sampleHitObject = new HitCircle(HitObjectManager, new Vector2(128, 180), 1500, true, 0, HitObjectSoundType.Normal);
                        sampleHitObject.ComboNumber = 1;
                        sampleHitObject.Clocking = ClockTypes.Manual;

                        HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                        sampleHitObject = new HitCircle(HitObjectManager, new Vector2(384, 180), 1500, true, 0, HitObjectSoundType.Normal);
                        sampleHitObject.ComboNumber = 1;
                        sampleHitObject.Clocking = ClockTypes.Manual;

                        HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 1100)
                                Clock.IncrementManual(0.5f);
                            else if (!touchToContinue)
                            {
                                showText("This will be denoted by a line connecting the beats.", 120);
                                showTouchToContinue();
                            }
                        };
                    }
                    break;
                case TutorialSegments.Multitouch_2:
                    {

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 2000)
                            {
                                Clock.IncrementManual(0.5f);
                            }
                            else if (!touchToContinue)
                            {
                                showText("Levels are made to be playable with two " + (GameBase.Instance.PlayersUseThumbs ? "thumbs" : "fingers") + ", but you will need to decide which fingers to use for each beat!", 0);
                                showTouchToContinue();
                            }
                        };
                    }
                    break;
                case TutorialSegments.Multitouch_3:
                    showText("Let's try some connected beats!");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Multitouch_Interact:
                    {
                        prepareInteract();

                        const int x1 = 100;
                        const int x15 = 230;
                        const int x2 = 512 - 100;
                        const int x25 = 512 - 230;
                        const int y1 = 80;
                        const int y2 = 384 - 80;

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 160 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 168 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 168 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 176 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 184 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 184 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 192 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x15, y1), music_offset + 192 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 196 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 200 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x25, y1), music_offset + 200 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 204 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 208 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x25, y2), music_offset + 208 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 212 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(256, 192), music_offset + 216 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);

                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        currentSegmentDelegate = delegate
                        {
                            if (!touchToContinue && HitObjectManager.AllNotesHit)
                                loadNextSegment();
                        };
                    }
                    break;
                case TutorialSegments.Multitouch_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss + CurrentScore.count50 > 5)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText("Make sure to touch both circles at the same time. Watch closely!");
                            nextSegment = TutorialSegments.Multitouch_Interact;
                        }
                        else if (CurrentScore.count100 > 2)
                        {
                            showText("Pretty good.");
                        }
                        else
                        {
                            showText("You've mastered it.");
                        }

                        showTouchToContinue();
                    }, 800);
                    break;

                case TutorialSegments.Stacked_1:
                    showText("Beats can also appear in a stack on top of each other.", -100);

                    Clock.ResetManual();
                    Player.Autoplay = true;

                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = new HitObjectManager(Beatmap);

                    sampleHitObject = new HitCircle(HitObjectManager, new Vector2(256, 197), 1500, true, 0, HitObjectSoundType.Normal);
                    sampleHitObject.ComboNumber = 1;
                    sampleHitObject.Clocking = ClockTypes.Manual;

                    HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                    sampleHitObject = new HitCircle(HitObjectManager, new Vector2(256, 197), 2000, false, 0, HitObjectSoundType.Normal);
                    sampleHitObject.ComboNumber = 2;
                    sampleHitObject.Clocking = ClockTypes.Manual;

                    HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);

                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 1300)
                            Clock.IncrementManual(0.5f);
                        else if (!touchToContinue)
                        {
                            showText("Watch for multiple approach circles and tap in time with them.", 120);
                            showTouchToContinue();
                        }
                    };
                    break;
                case TutorialSegments.Stacked_2:
                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 2500)
                            Clock.IncrementManual(0.5f);
                        else if (!touchToContinue)
                        {
                            showText("Hit circles can also be stacked at the beginning of sliders, so watch out for those!");
                            showTouchToContinue();
                        }
                    };
                    break;
                case TutorialSegments.Stacked_3:
                    int i = 0;
                    showText("Let's try a few stacked beats!");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stacked_Interact:
                    {
                        prepareInteract();

                        const int x1 = 100;
                        const int x15 = 230;
                        const int x2 = 512 - 100;
                        const int x25 = 512 - 230;
                        const int y1 = 80;
                        const int y2 = 384 - 80;

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 162 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 168 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 170 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 178 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 180 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 182 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(x1, y2), music_offset + 184 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier,
                            2, 300, new List<Vector2>() { new Vector2(x1 + (x2 - x1) / 2, y2 - 20), new Vector2(x2, y2) }, null, 200, 300f / 8), Difficulty.Easy);

                        Beatmap.StackLeniency = 2;

                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        currentSegmentDelegate = delegate
                        {
                            if (!touchToContinue && HitObjectManager.AllNotesHit)
                                loadNextSegment();
                        };
                    }
                    break;
                case TutorialSegments.Stacked_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 3 || CurrentScore.count50 > 4)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText("Hmm, not quite.. Let's go over stacks again!");
                            nextSegment = TutorialSegments.Stacked_1;
                        }
                        else if (CurrentScore.count50 > 6)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText("Watch the approach circles closely and make sure you hit every note in the stacks!");
                            nextSegment = TutorialSegments.Slider_Interact;
                        }
                        else if (CurrentScore.count100 > 0)
                        {
                            showText("Good job!");
                        }
                        else
                        {
                            showText("Excellent!");
                        }

                        showTouchToContinue();
                    }, 500);
                    break;

                case TutorialSegments.Stream_1:
                    showText("There are three different modes of play.");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stream_2:
                    showText("Stream mode consists of three separate difficulties, otherwise known as 'Streams'.");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stream_3:
                    showText("Reaching higher streams will make gameplay harder, but allow you to get a higher score.");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stream_4:
                    Clock.ResetManual();
                    Player.Autoplay = true;

                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = new HitObjectManager(Beatmap);

                    const int vpos = 240;

                    int hpos = 20;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1350, true, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Easy);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1350, true, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Normal);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1350, true, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    hpos += 100;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1500, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(hpos, vpos), 1550, false, 0, HitObjectSoundType.Normal, CurveTypes.Bezier,
                            2, 300, new List<Vector2>() { new Vector2(hpos + 100, vpos) }, null, 200, 300f / 8) { Clocking = ClockTypes.Manual }, Difficulty.Normal);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(hpos, vpos), 1550, false, 0, HitObjectSoundType.Normal, CurveTypes.Bezier,
                            2, 300, new List<Vector2>() { new Vector2(hpos + 100, vpos) }, null, 200, 300f / 8) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    hpos += 180;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1650, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1650, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Normal);

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos - 40, vpos - 40), 1650, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Easy);

                    hpos += 100;


                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos + 60), 1750, true, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    hpos += 100;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1850, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Easy);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1850, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Normal);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1850, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);



                    //HitObjectManager.Add(h, Difficulty.Hard);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);
                    playfieldBackground.ChangeColour(Difficulty.Easy, false);

                    pText streamTitle = showText("Easy...", -90);

                    Difficulty currentStream = Difficulty.Easy;
                    int lastSecond = 0;
                    bool forwards = true;
                    int startTime = 0;

                    const int delay_between_switches = 1200;

                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 1200)
                        {
                            Clock.IncrementManual(1f);
                            startTime = Clock.Time;
                        }
                        else if ((Clock.Time - startTime) / delay_between_switches != lastSecond)
                        {
                            lastSecond = (Clock.Time - startTime) / delay_between_switches;

                            if (forwards)
                                currentStream = (Difficulty)(currentStream + 1);
                            else
                                currentStream = (Difficulty)(currentStream - 1);

                            if (currentStream == Difficulty.Hard || currentStream == GameplayElements.Difficulty.Easy)
                            {
                                forwards = !forwards;
                                showTouchToContinue();
                            }

                            streamTitle.FadeOut(50);
                            streamTitle = showText(currentStream.ToString() + "...", -90);

                            HitObjectManager.ActiveStream = currentStream;
                            playfieldBackground.ChangeColour(currentStream, true);
                        }

                        //else if (!touchToContinue)
                        //{
                        //    showText("Watch for multiple approach circles and tap in time with them.", 120);
                        //    showTouchToContinue();
                        //}
                    };

                    break;
                case TutorialSegments.Stream_5:
                    HitObjectManager.ActiveStream = Difficulty.Normal;
                    playfieldBackground.ChangeColour(Difficulty.Normal, true);

                    foreach (SpriteManager sm in HitObjectManager.streamSpriteManagers)
                        if (sm != null) sm.ScaleTo(0.5f, 500, EasingTypes.InOut).MoveTo(new Vector2(0,150),500,EasingTypes.In);

                    loadNextSegment();
                    break;
                case TutorialSegments.Healthbar_1:
                    showText("The health bar is located at the top-left of your display.", -120);
                    healthBar = new HealthBar();
                    {
                        pDrawable lastFlash = null;
                        currentSegmentDelegate = delegate
                        {
                            if (lastFlash == null || lastFlash.Alpha == 0)
                                lastFlash = healthBar.s_barBg.AdditiveFlash(1000, 1).ScaleTo(healthBar.s_barBg.ScaleScalar * 1.04f, 1000);
                        };
                    }

                    streamSwitchDisplay = new StreamSwitchDisplay();
                    showTouchToContinue();
                    break;
                case TutorialSegments.Healthbar_2:
                    showText("It will go up or down depending on your performance.", -120);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Healthbar_3:
                    showText("In stream mode gameplay, you can jump to the next stream by filling your health bar.", -120);

                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_STANDARD, false);

                    healthBar.SetCurrentHp(100);
                    {
                        float increaseRate = 0;
                        currentSegmentDelegate = delegate
                        {
                            if (touchToContinue) return;

                            if (healthBar.CurrentHp == 200)
                            {
                                if (increaseRate > 20)
                                {
                                    streamSwitchDisplay.EndSwitch();
                                    HitObjectManager.ActiveStream = Difficulty.Hard;
                                    healthBar.SetCurrentHp(100);
                                    playfieldBackground.ChangeColour(Difficulty.Hard);
                                    showTouchToContinue();
                                }
                                else
                                {
                                    increaseRate += 0.2f;
                                    streamSwitchDisplay.BeginSwitch(true);
                                    playfieldBackground.Move(increaseRate);
                                }
                            }
                            else
                            {
                                healthBar.SetCurrentHp(healthBar.CurrentHp + 1);
                            }
                        };
                    }
                    break;
                case TutorialSegments.Healthbar_4:
                    showText("In a similar manner, if it reaches zero, you will drop down a stream.", -120);
                    {
                        float increaseRate = 0;
                        currentSegmentDelegate = delegate
                        {
                            if (touchToContinue) return;

                            if (healthBar.CurrentHp == 0)
                            {
                                if (increaseRate > 20)
                                {
                                    streamSwitchDisplay.EndSwitch();
                                    HitObjectManager.ActiveStream = Difficulty.Normal;
                                    healthBar.SetCurrentHp(100);
                                    playfieldBackground.ChangeColour(Difficulty.Normal);

                                    showTouchToContinue();
                                }
                                else
                                {
                                    increaseRate += 0.2f;
                                    streamSwitchDisplay.BeginSwitch(false);
                                    playfieldBackground.Move(-increaseRate);
                                }
                            }
                            else
                            {
                                healthBar.SetCurrentHp(healthBar.CurrentHp - 1);
                            }
                        };
                    }
                    break;
                case TutorialSegments.Healthbar_5:
                    showText("If it hits zero on the lowest stream you will fail instantly, so watch out!", -120);

                    HitObjectManager.ActiveStream = Difficulty.Easy;
                    playfieldBackground.ChangeColour(Difficulty.Easy, true);

                    currentSegmentDelegate = delegate
                    {
                        if (playfieldBackground.Velocity == 0)
                            healthBar.SetCurrentHp(healthBar.CurrentHp - 0.5f);
                        if (healthBar.CurrentHp == 0)
                        {

                            if (!touchToContinue)
                            {
                                showTouchToContinue();
                                playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO);
                                showFailSprite();
                                Failed = true;
                                AudioEngine.Music.Pause();
                            }
                        }
                        else if (healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM / 3)
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                    };
                    break;
                case TutorialSegments.Healthbar_End:
                    healthBar.SetCurrentHp(100);
                    hideFailSprite();
                    Failed = false;
                    AudioEngine.Music.Play();
                    healthBar.InitialIncrease = true;
                    currentSegmentDelegate = delegate { if (healthBar.DisplayHp > 20) loadNextSegment(); };
                    break;

                case TutorialSegments.Score_1:
                    scoreDisplay = new ScoreDisplay();
                    {
                        pDrawable lastFlash = null;
                        currentSegmentDelegate = delegate
                        {
                            if (lastFlash == null || lastFlash.Alpha == 0)
                                scoreDisplay.spriteManager.Sprites.ForEach(s => lastFlash = s.AdditiveFlash(1000, 1).ScaleTo(s.ScaleScalar * 1.1f, 1000));
                        };
                    }
                    showText("Scoring is based on a your accuracy and combo.");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Score_2:
                    showText("You can also get score bonuses from reaching higher streams, and for spinning spinners fast!");
                    showTouchToContinue();
                    break;
                case TutorialSegments.Score_3:
                    showText("Your current combo can be seen in the bottom-left corner of the screen.");
                    {
                        GameBase.Scheduler.Add(delegate
                        {
                            comboCounter = new ComboCounter();
                            comboCounter.SetCombo(35);

                            pDrawable lastFlash = null;
                            currentSegmentDelegate = delegate
                            {
                                if (comboCounter.displayCombo == 35)
                                {
                                    if (!touchToContinue)
                                        showTouchToContinue(false);

                                    if (lastFlash == null || lastFlash.Alpha == 0)
                                        comboCounter.spriteManager.Sprites.ForEach(s => lastFlash = s.AdditiveFlash(1000, 1).ScaleTo(s.ScaleScalar * 1.1f, 1000));
                                }
                            };
                        }, 500);
                    }
                    break;
                case TutorialSegments.Score_4:
                    comboCounter.SetCombo(0);
                    showText("Your combo will only show up when you are on a streak!");
                    GameBase.Scheduler.Add(delegate { showTouchToContinue(); }, 1500);
                    break;
            case TutorialSegments.Outro:
                    showText("Congratulations. You now have the skills required to challenge osu!");
                    showTouchToContinue(false);
                    break;
                case TutorialSegments.End:
                    backButton.HandleInput = false;
                    Director.ChangeMode(OsuMode.MainMenu, new FadeTransition(3000, FadeTransition.DEFAULT_FADE_IN));
                    break;

            }
        }

        private void prepareInteract()
        {
            resetScore();
            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_STANDARD, false);

            Player.Autoplay = false;

            Difficulty = Difficulty.Easy;

            if (countdown == null) countdown = new CountdownDisplay();

            firstCountdown = true;
            AudioEngine.Music.SeekTo(58000);
            CountdownResume(music_offset + 160 * music_beatlength, 8);

            loadBeatmap();
        }

        void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            switch (change)
            {
                case ScoreChange.Hit300:
                case ScoreChange.Hit300g:
                case ScoreChange.Hit300k:
                case ScoreChange.Hit300m:
                    showText("Perfect!", 0).FadeOut(1000);
                    break;
                case ScoreChange.Hit100:
                case ScoreChange.Hit100m:
                case ScoreChange.Hit100k:
                    if (Clock.AudioTime < hitObject.StartTime)
                        showText("A bit early..", -60).FadeOut(1000);
                    else
                        showText("A bit late..", 60).FadeOut(1000);
                    break;
                case ScoreChange.Hit50:
                case ScoreChange.Hit50m:
                case ScoreChange.Miss:
                    if (Clock.AudioTime < hitObject.StartTime)
                    {
                        pText t = showText("Very early..", -60);
                        t.TextSize *= 1.4f;
                        t.Colour = Color4.OrangeRed;
                        t.FadeOut(1000);
                    }
                    else
                    {
                        pText t = showText("Very late..", 60);
                        t.TextSize *= 1.4f;
                        t.Colour = Color4.OrangeRed;
                        t.FadeOut(1000);
                    }
                    break;
            }
        }

        protected override void UpdateStream()
        {
        }

        protected override bool CheckForCompletion()
        {
            return false;
        }

    }
}
