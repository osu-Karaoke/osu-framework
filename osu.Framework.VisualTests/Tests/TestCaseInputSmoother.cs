﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Input;
using osu.Framework.Testing;
using System;

namespace osu.Framework.VisualTests.Tests
{
    internal class TestCaseInputSmoother : TestCase
    {
        public override string Description => @"Live optimizing paths.";

        public override void Reset()
        {
            base.Reset();

            const int width = 20;
            Texture gradientTexture = new Texture(width, 1, true);
            byte[] data = new byte[width * 4];
            for (int i = 0; i < width; ++i)
            {
                float brightness = (float)i / (width - 1);
                int index = i * 4;
                data[index + 0] = (byte)(brightness * 255);
                data[index + 1] = (byte)(brightness * 255);
                data[index + 2] = (byte)(brightness * 255);
                data[index + 3] = 255;
            }
            gradientTexture.SetData(new TextureUpload(data));

            SpriteText arc1Text, arc2Text, arc3Text;
            SpriteText drawText;

            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.5f),
                                Children = new Drawable[]
                                {
                                    arc1Text = new SpriteText
                                    {
                                        Text = "Raw Arc",
                                        TextSize = 20,
                                        Colour = Color4.White,
                                    },
                                    new ArcPath(true, new InputSmoother(), gradientTexture, Color4.Green, arc1Text),
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.5f),
                                Children = new Drawable[]
                                {
                                    arc2Text = new SpriteText
                                    {
                                        Text = "Smoothed Arc",
                                        TextSize = 20,
                                        Colour = Color4.White,
                                    },
                                    new ArcPath(false, new InputSmoother(), gradientTexture, Color4.Blue, arc2Text),
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.5f),
                                Children = new Drawable[]
                                {
                                    arc3Text = new SpriteText
                                    {
                                        Text = "Smoothed Raw Arc",
                                        TextSize = 20,
                                        Colour = Color4.White,
                                    },
                                    new ArcPath(true, new InputSmoother
                                        {
                                            SmoothRawInput = true
                                        }, gradientTexture, Color4.Red, arc3Text),
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.5f),
                                Children = new Drawable[]
                                {
                                    drawText = new SpriteText
                                    {
                                        Text = "Custom Smoothed Drawn: Smoothed=0, Raw=0",
                                        TextSize = 20,
                                        Colour = Color4.White,
                                    },
                                    new DrawablePath
                                    {
                                        DrawText = drawText,
                                        RelativeSizeAxes = Axes.Both,
                                        Texture = gradientTexture,
                                        Colour = Color4.White,
                                        InputSmoother = new InputSmoother()
                                        {
                                            SmoothRawInput = true
                                        },
                                    },
                                }
                            },
                        }
                    }
                }
            });
        }

        private class SmoothedPath : Path
        {
            public InputSmoother InputSmoother = new InputSmoother();

            public int NumVertices;

            public int NumRaw;

            public bool AddSmoothedVertex(Vector2 pos)
            {
                NumRaw++;
                if (InputSmoother.AddPosition(pos))
                {
                    AddVertex(pos);
                    NumVertices++;
                    return true;
                }
                return false;
            }
        }

        private class ArcPath : SmoothedPath
        {
            public ArcPath(bool raw, InputSmoother inputSmoother, Texture texture, Color4 colour, SpriteText output)
            {
                InputSmoother = inputSmoother;
                int targetRaw = 256;
                RelativeSizeAxes = Axes.Both;
                Texture = texture;
                Colour = colour;

                for (int i = 0; i < targetRaw; i++)
                {
                    float x = (float) Math.Sin(i / (double) targetRaw * Math.PI * 0.5) * 200 + 50.5f;
                    float y = (float) Math.Cos(i / (double) targetRaw * Math.PI * 0.5) * 200 + 50.5f;
                    if (!raw)
                    {
                        x = (int) x;
                        y = (int) y;
                    }
                    AddSmoothedVertex(new Vector2(x, y));
                }

                output.Text += ": Smoothed=" + NumVertices + ", Raw=" + NumRaw;
            }
        }

        private class DrawablePath : SmoothedPath
        {
            public override bool HandleInput => true;

            public SpriteText DrawText;

            protected override bool OnDragStart(InputState state)
            {
                AddSmoothedVertex(state.Mouse.Position);
                DrawText.Text = "Custom Smoothed Drawn: Smoothed=" + NumVertices + ", Raw=" + NumRaw;
                return true;
            }

            protected override bool OnDrag(InputState state)
            {
                AddSmoothedVertex(state.Mouse.Position);
                DrawText.Text = "Custom Smoothed Drawn: Smoothed=" + NumVertices + ", Raw=" + NumRaw;
                return base.OnDrag(state);
            }
        }
    }
}
