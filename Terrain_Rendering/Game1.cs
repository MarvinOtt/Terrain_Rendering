using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using FormClosingEventArgs = System.Windows.Forms.FormClosingEventArgs;
using Form = System.Windows.Forms.Form;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Bitmap = System.Drawing.Bitmap;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Image = System.Drawing.Image;
using ImageConverter = System.Drawing.ImageConverter;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxButtons = System.Windows.Forms.MessageBoxButtons;
using DialogResult = System.Windows.Forms.DialogResult;
using System.Runtime;
using System.Text;
using System.Windows;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Terrain_Rendering
{
    #region VertexDeclarations
    public struct VertexPositionColorNormal_noTexCoo
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(sizeof(float) * 6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
        public VertexPositionColorNormal_noTexCoo(Vector3 pos, Vector3 normal, Color color)
        {
            Position = pos;
            Normal = normal;
            Color = color;
        }
    }
    public struct mapvertexdecleration
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 TexWeights;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)

        );
        public mapvertexdecleration(Vector3 pos, Vector3 normal, Vector4 Texweights)
        {
            Position = pos;
            Normal = normal;
            TexWeights = Texweights;
        }
    }
    #endregion
    public class Game1 : Game
    {
        #region Structs
        public struct dreick
        {
            public Vector3 p1, p2, p3;
            public dreick(Vector3 P1, Vector3 P2, Vector3 P3)
            {
                p1 = P1;
                p2 = P2;
                p3 = P3;
            }
            public Vector3 mittelwert()
            {
                return (p1 + p2 + p3) / 3;
            }
        }
        public struct Taste
        {
            public int timer;
        }
        public struct verbindung
        {
            public float länge;
            public float stärke;
            public float dämpfung;
            public int punkt1;
            public int punkt2;
            public verbindung(float Länge, float Stärke, float Dämpfung, int Punkt1, int Punkt2)
            {
                länge = Länge;
                stärke = Stärke;
                dämpfung = Dämpfung;
                punkt1 = Punkt1;
                punkt2 = Punkt2;
            }
        }
        public struct punkt
        {
            public Vector3 pos;
            public Vector3 speed;
            public float masse;
            public float grip;
            public int aktiviert;
            public punkt(Vector3 Pos, Vector3 Speed, float Masse, float Grip)
            {
                pos = Pos;
                speed = Speed;
                masse = Masse;
                grip = Grip;
                aktiviert = 0;
            }
        }

        public struct Box_Style
        {
            public Texture2D texture;
            public SpriteFont font;
            public int width, height, stringoffste_Y, stringoffset_X;

            public Box_Style(Texture2D tex, SpriteFont font)
            {
                texture = tex;
                this.font = font;
                width = tex.Width;
                height = tex.Height / 4;
                stringoffste_Y = (height - (int)font.MeasureString("1234567890,").Y) / 2;
                stringoffset_X = 6;
            }

            public static void Initialize(ContentManager Content)
            {
                Default = new Box_Style(Content.Load<Texture2D>(".\\Box_Styles\\standartboxstyle"), Content.Load<SpriteFont>("Box_font"));
            }

            public static Box_Style Default;
        }
        private static int getDecimalCount(double val)
        {
            int i = 0;
            while (Math.Round(val, i) != val && i < 15)
                i++;
            return i;
        }
        public class Floatbox
        {
            public string currenttext, text;
            public Vector2 pos, size;
            public Box_Style style;
            public Color current_edgecolor1, current_edgecolor2;
            private float timer, min, max;
            private bool aktiv, IsBorder;
            private float value;
            public float wrongstringtimer;

            #region Constructors
            public Floatbox(Box_Style style, Vector2 pos) : this(style, pos, new Vector2(100, 20)) { }

            public Floatbox(Box_Style style, Vector2 pos, float min, float max) : this(style, pos, new Vector2(100, 20), min, max) { }

            public Floatbox(Box_Style style, Vector2 pos, Vector2 size, float min, float max)
            {
                this.pos = pos;
                this.size = size;
                aktiv = false;
                timer = 0;
                value = 0;
                this.min = min;
                this.max = max;
                this.style = style;
                IsBorder = true;
                wrongstringtimer = 0;
            }

            public Floatbox(Box_Style style, Vector2 pos, Vector2 size)
            {
                this.pos = pos;
                this.size = size;
                aktiv = false;
                timer = 0;
                value = 0;
                this.style = style;
                IsBorder = false;
                wrongstringtimer = 0;
            }
            #endregion

            public void Inizialise_Input(float input)
            {
                //this.value = input;
            }

            public void Update(GameTime time, ref float output)
            {
                text = value.ToString();
                if (mousepos.X > pos.X && mousepos.X < pos.X + style.width && mousepos.Y > pos.Y && mousepos.Y < pos.Y + style.height && Mouse.GetState().LeftButton == ButtonState.Pressed && aktiv == false) // Floatbox clicked
                {
                    aktiv = true;
                    currenttext = text;
                }
                if (aktiv) // Textbox ausgewählt
                {
                    currenttext += keyhandler.CurrentInput; // Adding number input
                    if (keyhandler.IsKeyActive(Keys.Back)) // Deleting one number
                        if (currenttext.Length >= 1)
                            currenttext = currenttext.Remove(currenttext.Length - 1);

                    if (keyhandler.IsKeyActive(Keys.Enter))
                    {
                        float OUT;
                        bool state = float.TryParse(currenttext, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InstalledUICulture, out OUT);
                        if (state == true) // Input is a float
                        {
                            if (IsBorder)
                            {
                                if (OUT >= min && OUT <= max) // Input in Bounds
                                {
                                    value = OUT;
                                    output = OUT;
                                    value = getDecimalCount((double)1.1234f);
                                    text = value.ToString();
                                    aktiv = false;
                                }
                                else // Input out of Bounds
                                    wrongstringtimer = 1;
                            }
                            else
                            {
                                value = OUT;
                                output = OUT;
                                text = value.ToString();
                                aktiv = false;
                            }
                        }
                        else // Input is not a float
                            wrongstringtimer = 1;
                    }
                    else if (keyhandler.IsKeyActive(Keys.Escape))
                        aktiv = false;
                }
            }
            public void Draw(SpriteBatch Batch, GameTime time)
            {
                if (wrongstringtimer > 0)
                {
                    // Making Box blink red twice if input was wrong
                    wrongstringtimer += (time.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerSecond) * 60;
                    if (wrongstringtimer % 20 <= 10)
                        Batch.Draw(style.texture, pos, new Rectangle(0, 90, 160, 30), Color.White);
                    else
                        Batch.Draw(style.texture, pos, new Rectangle(0, 60, 160, 30), Color.White);
                    if (wrongstringtimer > 30)
                        wrongstringtimer = 0;
                }
                else if (aktiv)
                    Batch.Draw(style.texture, pos, new Rectangle(0, 60, 160, 30), Color.White);
                else if (mousepos.X > pos.X && mousepos.X < pos.X + style.width && mousepos.Y > pos.Y && mousepos.Y < pos.Y + style.height)
                    Batch.Draw(style.texture, pos, new Rectangle(0, 30, 160, 30), Color.White);
                else if (aktiv == false)
                    Batch.Draw(style.texture, pos, new Rectangle(0, 0, 160, 30), Color.White);

                if (aktiv)
                    Batch.DrawString(style.font, currenttext, new Vector2(pos.X + style.stringoffset_X, pos.Y + style.stringoffste_Y), Color.Black);
                else
                    Batch.DrawString(style.font, text, new Vector2(pos.X + style.stringoffset_X, pos.Y + style.stringoffste_Y), Color.Black);
            }
        }
        #endregion
        static GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static GameSettings settings;
        private static readonly Random r = new Random();
        public static SpriteFont font;
        public static Stopwatch GameWatch;
        menü Menü;
        public float FPS;
        private float[] FPS_werte = new float[150];
        int fps_index;
        public static KeyHandler keyhandler;
        Vector3 rotation;
        Floatbox testbox;
        float updatetime, drawtime;
        public static int Screenwidth = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);
        public static int Screenheight = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);

        public static int modus = 1;
        #region Graphics
        public static Effect Shader, textureshader, shadoweffect, skyboxshader, maptextureshader, godrayeffect, obstacleeffect;
        #region Models
        public static carmodel model;
        public static Model spheremodel, ringmodel, skyboxmodel;
        Matrix[] sphereoriginalmatrix;
        #endregion
        #endregion
        #region Camera
        public static Vector3 camerapos = new Vector3(0, 30, 0), camerarichtung;
        public static BasicEffect cameraeffect;
        Vector2 cameramousepos;
        public int cameratomapdist;
        public static bool camerafreeze;
        int camfreezstop;
        public static Vector3 campos2, camrichtung2;
        int camarabewegen = 0, stopp2;
        float cameraspeed = 2f;
        private float cameraconstspeed = 2f;
        public static Matrix camworld, camview, camprojection;
        public static float camfarplane = 5000000, camnearplane = 5;
        #endregion
        #region Mouse
        Vector3 mouserotationbuffer = new Vector3();
        public static int mausclickedduration;
        public static Vector2 mousepos;
        public System.Drawing.Point mousepointpos;
        #endregion
        #region verbindungen platzieren
        int verbpos1ID;
        Vector3 verbpos2;
        public static int verbmod;
        #endregion
        #region menü
        public static punktfenster Punktfenster;
        #endregion
        #region Map
        public static map Map;
        public static Thread mapthread;
        public skybox Skybox;
        public static RenderTarget2D maintarget;
        public static RenderTarget2D maptarget;
        public static RenderTarget2D heighttarget;
        #endregion
        #region licht
        float sonnenwinkel = (float)Math.PI / 2;
        public static Vector3 ligthdirection;
        public static RenderTarget2D bloomtarget, bloomtarget2, bloomtarget3, bloomtarget4;
        public static Effect blureffecthorizontal, blureffectvertical, combineffect;
        public static Vector4 lightcolor;
        #endregion

        private RenderTarget2D test_rendertarget2D, maintarget2, godraytarget, postprocesstarget;
        public static bool IsSlowerthanrefreshrate;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
                PreferredBackBufferWidth = Screenwidth,
                PreferredBackBufferHeight = Screenheight,
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = false

            };
            IsFixedTimeStep = false;
            Window.IsBorderless = true;
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }
        #region Draw Line
        private static Texture2D pixel;
        private static void CreateThePixel(SpriteBatch spriteBatch)
        {
            pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White });
        }
        public static void DrawLine(SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color)
        {
            DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, 1.0f);
        }
        public static void DrawLine(SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness)
        {
            DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
        }
        public static void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color)
        {
            DrawLine(spriteBatch, point1, point2, color, 1.0f);
        }
        public static void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness)
        {
            // calculate the distance between the two vectors
            float distance = Vector2.Distance(point1, point2);

            // calculate the angle between the two vectors
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }
        public static void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color)
        {
            DrawLine(spriteBatch, point, length, angle, color, 1.0f);
        }
        public static void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness)
        {
            if (pixel == null)
            {
                CreateThePixel(spriteBatch);
            }
            spriteBatch.Draw(pixel, point, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0);
        }
        #endregion

        public static float fade(float t)
        {
            return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        /// 

        protected override void Initialize()
        {
            // S E T T I N G S
            settings = new GameSettings();
            settings.SettingsfromQuality(5);
            settings.IsFixedTimeStep = false;
            settings.IsVsync = false;
            settings.MaxFps = 144;
            settings.Bloom = true;
            settings.water_meshsize = 300;
            //settings.map_drawquality = 1;
            //settings.water_ref_quality = 1;
            //settings.water_map_ref = false;
            //settings.water_skybox_ref = false;
            settings.map_drawdistance = 10000000;
            //settings.water_waves = false;

            SetMaxFPS(settings.MaxFps);
            //this.MaxElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerMillisecond * (1000.0 / 130.0)));
            Form f = Form.FromHandle(Window.Handle) as Form;
            f.Location = new System.Drawing.Point(0, 0);
            if (f != null) { f.FormClosing += f_FormClosing; }
            base.Initialize();
        }


        protected override void LoadContent()
        {
            Box_Style.Initialize(Content);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            CreateThePixel(spriteBatch);
            font = Content.Load<SpriteFont>("NewSpriteFont");
            blureffecthorizontal = Content.Load<Effect>("blureffecthorizontal");
            blureffectvertical = Content.Load<Effect>("blureffectvertical");
            combineffect = Content.Load<Effect>("combinshader");
            godrayeffect = Content.Load<Effect>("GodRayEffect");
            obstacleeffect = Content.Load<Effect>("obstacleeffect");
            GameWatch = new Stopwatch();
            Punktfenster = new punktfenster(Content);
            Menü = new menü(Content, GraphicsDevice, spriteBatch);
            keyhandler = new KeyHandler();
            testbox = new Floatbox(Box_Style.Default, new Vector2(500, 100), 0.25f, 3.0f);

            test_rendertarget2D = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            godraytarget = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
            postprocesstarget = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            maintarget2 = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);

            maptarget = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            maintarget = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Color, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
            heighttarget = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            bloomtarget = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            bloomtarget2 = new RenderTarget2D(GraphicsDevice, (int)(Screenwidth * 0.5f * (1920 / (float)Screenwidth)), (int)(Screenheight * 0.5f * (1080 / (float)Screenheight)), false, SurfaceFormat.Single, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
            bloomtarget3 = new RenderTarget2D(GraphicsDevice, (int)(Screenwidth * 0.5f * (1920 / (float)Screenwidth)), (int)(Screenheight * 0.5f * (1080 / (float)Screenheight)), false, SurfaceFormat.Single, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
            bloomtarget4 = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Single, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
            cameraeffect = new BasicEffect(GraphicsDevice);
            Skybox = new skybox(Content, GraphicsDevice);

            // Projection
            cameratomapdist = 0;
            camfarplane = 5000000;
            camnearplane = 5;
            cameraeffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(120), GraphicsDevice.Viewport.AspectRatio, camnearplane, camfarplane);

            cameraeffect.View = Matrix.CreateLookAt(new Vector3(50, 50, 50), new Vector3(0, 0, 0), Vector3.Up);
            cameraeffect.World = Matrix.Identity;
            graphics.ApplyChanges();

            #region Loading and generating Map and mapsettings
            mapsettings MS = new mapsettings("maptextures", 4, 0);

            MS.load_environment_sand(3, 20);//40, 20
            MS.load_environment_grass(1.5f, 1100, 10.0f, 1000); // 10,280,10,120
            MS.load_environment_dirt(-200, 2000, 50, 80);//0, 800, 20, 80
            MS.load_environment_snow(1000, 160);//200, 160

            Map = new map(GraphicsDevice, Content, 33);
            int power = 14;
            Map.loadmapsettings(power, 0, 1750 * 2, new Vector2(-(float)Math.Pow(2, power) / 2), 1.0f, 0.05f, new Perlin(-1, 1), r.Next(), MS);
            mapthread = new Thread(generatemap);
            mapthread.Start(Map);
            //Map.generateheights();
            //Map.loadheightsfromfile("test1map");
            //Map.generatetexweigths();
            //Map.saveheightstofile("test1map");
            //Map.generatefirstchunk();
            //Map.loadheightsfromfile("test1map");
            //Map.saveheightstofile("test1map");
            //Map.loadmap();
            //Map.freeheights();
            //Map.freetriangles();
            #endregion
            Shader = Content.Load<Effect>("effect1");
            Shader.Parameters["LightDirection"].SetValue(new Vector3(0, 0, -1));
            Shader.Parameters["AmbientIntensity"].SetValue(0.15f);
            Shader.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
            Shader.Parameters["DiffuseIntensity"].SetValue(0.4f);
            Shader.Parameters["DiffuseIntensity2"].SetValue(0.05f);
            Shader.Parameters["DiffuseColor"].SetValue(Color.White.ToVector4());
            Shader.Parameters["SpecularColor"].SetValue(Color.White.ToVector4());
            Shader.Parameters["SpecularIntensity"].SetValue(1f);
            Shader.Parameters["Color"].SetValue(Color.Green.ToVector4());

            camerarichtung = Vector3.UnitX;
            //spheremodel = Content.Load<Model>("skugel");
            //ringmodel = Content.Load<Model>("ring");
            //spheremodel.Bones[0].Transform = spheremodel.Bones[0].Transform * Matrix.CreateScale(10);
            //ringmodel.Bones[0].Transform = ringmodel.Bones[0].Transform * Matrix.CreateScale(1);
            //model = new carmodel(GraphicsDevice);
            //sphereoriginalmatrix = new Matrix[spheremodel.Bones.Count];
            //for (int i = 0; i < spheremodel.Bones.Count; i++)
            //{
            //    sphereoriginalmatrix[i] = spheremodel.Bones[i].Transform;
            //}
            /*model.punktelist.Add(new punkt(new Vector3(0, 150, 0), Vector3.Zero, 1, 1));
            model.punktelist.Add(new punkt(new Vector3(150, 0, 0), Vector3.Zero, 1, 1));
            model.punktelist.Add(new punkt(new Vector3(0, 0, 150), Vector3.Zero, 1, 1));
            model.punktelist.Add(new punkt(new Vector3(0, 0, 0), Vector3.Zero, 1, 1));
            model.punktelist.Add(new punkt(new Vector3(150, 150, 150), Vector3.Zero, 1, 1));
            model.punktelist.Add(new punkt(new Vector3(150, 0, 150), Vector3.Zero, 1, 1));
            model.punktelist.Add(new punkt(new Vector3(150, 150, 0), Vector3.Zero, 1, 1));
            model.punktelist.Add(new punkt(new Vector3(0, 150, 150), Vector3.Zero, 1, 1));
            model.verbindungenlist.Add(new verbindung(150, 1, 1, 3, 0));
            model.verbindungenlist.Add(new verbindung(150, 1, 1, 3, 1));
            model.verbindungenlist.Add(new verbindung(150, 1, 1, 3, 2));

            model.verbindungenlist.Add(new verbindung(150, 1, 1, 4, 5));
            model.verbindungenlist.Add(new verbindung(150, 1, 1, 4, 6));
            model.verbindungenlist.Add(new verbindung(150, 1, 1, 4, 7));

            model.verbindungenlist.Add(new verbindung(150, 1, 1, 1, 5));
            model.verbindungenlist.Add(new verbindung(150, 1, 1, 2, 5));

            model.verbindungenlist.Add(new verbindung(150, 1, 1, 0, 7));
            model.verbindungenlist.Add(new verbindung(150, 1, 1, 2, 7));

            model.verbindungenlist.Add(new verbindung(150, 1, 1, 0, 6));
            model.verbindungenlist.Add(new verbindung(150, 1, 1, 1, 6));*/
            //model.UpdateArray();
            float nn = ComputeGaussian(10);
            float[] array = new float[31];

            /*for (int i = 0; i < 16; i++)
            {
                array[i] = ComputeGaussian(2 * (i - 15) - 0.5f);
            }
            for (int i = 16; i < 31; i++)
            {
                array[i] = ComputeGaussian(2 * (i - 16) + 0.5f);
            }*/
            for (int i = 0; i < 31; i++)
            {
                array[i] = ComputeGaussian(i - 15);
            }

            float sum = 0;
            for (int i = 0; i < 31; i++)
            {
                sum += array[i];
            }
            for (int i = 0; i < 31; i++)
            {
                array[i] /= sum;
            }
            blureffecthorizontal.Parameters["BlurWeights"].SetValue(array);
            blureffectvertical.Parameters["BlurWeights"].SetValue(array);

            godrayeffect.Parameters["Screenwidth"].SetValue(Screenwidth);
            godrayeffect.Parameters["Screenheight"].SetValue(Screenheight);
            godrayeffect.Parameters["Aspectratio"].SetValue(GraphicsDevice.Viewport.AspectRatio);

            // Updating Settings
            Update_Settings();

        }
        protected override void UnloadContent()
        {
        }
        public void generatemap(Object state)
        {
            ((map)state).generateheights();
            ((map)state).generatefirstchunk();
        }
        #region Framerate Functions
        public TimeSpan SetMaxFPS(int fps)
        {
            TimeSpan span = TimeSpan.FromTicks((long)(TimeSpan.TicksPerMillisecond * (1000.0 / (float)fps)));
            TargetElapsedTime = span;
            return span;
        }
        #endregion
        #region String Funktions
        #endregion
        #region FUNKTIONS
        public void f_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Exit();
            Map.Dispose();
            Skybox.Dispose();
            this.GraphicsDevice.Dispose();
            if (mapthread.IsAlive)
                mapthread.Abort();
            Thread.Sleep(100);
            base.Exit();
        }

        public void Update_Settings()
        {
            Map.water.watereffect.Parameters["IsTerraindistortion"].SetValue(settings.water_Terraindistortion);
            Map.water.watereffect.Parameters["IsReflections"].SetValue(settings.water_skybox_ref || settings.water_map_ref);
            Map.water.watereffect.Parameters["IsWaves"].SetValue(settings.water_waves);
            blureffecthorizontal.Parameters["width"].SetValue(Screenwidth);
            blureffectvertical.Parameters["height"].SetValue(Screenheight);
        }

        public Vector2 Spacetoscreen(Vector3 pos)
        {
            Vector3 pos2d = GraphicsDevice.Viewport.Project(pos, cameraeffect.Projection, cameraeffect.View, cameraeffect.World);
            if (pos2d.Z < 1)
            {
                return new Vector2(pos2d.X, pos2d.Y);
            }
            else
            {
                return Vector2.Zero;
            }
        }
        public static void DrawLine3d(GraphicsDevice graphicsdevice, BasicEffect effect, Vector3 pos1, Vector3 pos2)
        {
            DrawLine3d(graphicsdevice, effect, pos1, pos2, Color.White, Color.White);
        }
        public static void DrawLine3d(GraphicsDevice graphicsdevice, BasicEffect effect, Vector3 pos1, Vector3 pos2, Color color1, Color color2)
        {
            effect.LightingEnabled = false;
            effect.EmissiveColor = Color.Transparent.ToVector3();
            var vertices = new[] { new VertexPositionColor(pos1, color1), new VertexPositionColor(pos2, color2) };
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsdevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
            }
        }
        public static void DrawLine3d(GraphicsDevice graphicsdevice, BasicEffect effect, Vector3 pos1, Vector3 pos2, Color color)
        {
            DrawLine3d(graphicsdevice, effect, pos1, pos2, color, color);
        }
        public static void DrawMesh(BasicEffect basiceffect, Model model, Effect shader)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    shader.Parameters["World"].SetValue(basiceffect.World * mesh.ParentBone.Transform);
                    shader.Parameters["View"].SetValue(basiceffect.View);
                    shader.Parameters["Projection"].SetValue(basiceffect.Projection);
                    shader.Parameters["EyePosition"].SetValue(camerapos);
                    shader.Parameters["LightDirection"].SetValue(new Vector3(-1, 0, 0));
                    part.Effect = shader;
                }
                mesh.Draw();

            }
        }
        public static void DrawMesh(BasicEffect basiceffect, Model model)
        {
            Matrix[] transformations = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transformations);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.EmissiveColor = basiceffect.EmissiveColor;
                    effect.LightingEnabled = basiceffect.LightingEnabled;
                    if (effect.LightingEnabled == true)
                    {
                        effect.AmbientLightColor = new Vector3(0.05f, 0.05f, 0.05f);
                        effect.DirectionalLight0.Enabled = false;
                        effect.DirectionalLight1.Enabled = false;
                        effect.DirectionalLight2.Enabled = false;
                        effect.DirectionalLight0.Direction = new Vector3(1, 0, 0);
                        effect.DirectionalLight0.DiffuseColor = Color.LightGoldenrodYellow.ToVector3() * 0.2f;
                        effect.DirectionalLight0.SpecularColor = Color.LightGoldenrodYellow.ToVector3() * 1;
                    }
                    effect.World = transformations[mesh.ParentBone.Index];
                    effect.View = basiceffect.View;
                    effect.Projection = basiceffect.Projection;
                }
                mesh.Draw();
            }
        }
        public static Quaternion GetRotation(Vector3 source, Vector3 dest, Vector3 up)
        {
            float dot = Vector3.Dot(source, dest);

            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                // vector a and b point exactly in the opposite direction, 
                // so it is a 180 degrees turn around the up-axis
                return new Quaternion(up, MathHelper.ToRadians(180.0f));
            }
            if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                // vector a and b point exactly in the same direction
                // so we return the identity quaternion
                return Quaternion.Identity;
            }

            float rotAngle = (float)Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(source, dest);
            rotAxis = Vector3.Normalize(rotAxis);
            return Quaternion.CreateFromAxisAngle(rotAxis, rotAngle);
        }
        public static void MeshPos(ref Model model, Matrix original, int ID, Vector3 pos)
        {
            model.Bones[ID].Transform = original;
            Vector3 oldpos = model.Bones[ID].Transform.Translation;
            model.Bones[ID].Transform *= Matrix.CreateTranslation(pos - oldpos);
        }
        public static void MeshMatrix(ref Model model, Matrix original, int ID, Matrix matrix)
        {
            model.Bones[ID].Transform = original;
            Vector3 oldpos = model.Bones[ID].Transform.Translation;
            model.Bones[ID].Transform *= Matrix.CreateTranslation(Vector3.Zero - oldpos) * matrix;
        }
        public static void MeshMatrix(ref Model model, Matrix original, int ID, Matrix matrix, Vector3 pos)
        {
            model.Bones[ID].Transform = original;
            Vector3 oldpos = model.Bones[ID].Transform.Translation;
            model.Bones[ID].Transform *= matrix * Matrix.CreateTranslation(pos - oldpos);
        }
        public static Vector3 ScreentoSpace(Vector2 mousepos, GraphicsDevice device, int tiefe)
        {
            Matrix viewmatrix = Matrix.CreateLookAt(Vector3.Zero, camerarichtung - camerapos, Vector3.Up);
            Vector3 vec1 = device.Viewport.Unproject(new Vector3(mousepos.X, mousepos.Y, 0), cameraeffect.Projection, viewmatrix, cameraeffect.World);
            Vector3 vec2 = -device.Viewport.Unproject(new Vector3(mousepos.X, mousepos.Y, 1.000015f), cameraeffect.Projection, viewmatrix, Matrix.Identity);
            Vector3 vecrichtung = -Vector3.Normalize(vec2 - vec1);
            return camerapos - vec1 + vecrichtung * tiefe;
        }
        public static ButtonState mouseclick(GameTime time)
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                mausclickedduration++;
            }
            else
            {
                mausclickedduration = 0;
            }
            if (mausclickedduration == 1 || mausclickedduration > 16.666 / time.ElapsedGameTime.Milliseconds * 40)
            {
                return ButtonState.Pressed;
            }
            return ButtonState.Released;
        }
        public static string keytonumber(Keys key)
        {
            if (key == Keys.Enter)
            {
                return "";
            }
            switch (key.ToString())
            {

                case "D0": return "0";
                case "D1": return "1";
                case "D2": return "2";
                case "D3": return "3";
                case "D4": return "4";
                case "D5": return "5";
                case "D6": return "6";
                case "D7": return "7";
                case "D8": return "8";
                case "D9": return "9";
                case "Dot": return ".";
            }
            if (key == Keys.Back)
            {
                return "";
            }
            if (key == Keys.OemComma)
            {
                return "";
            }
            if (key == Keys.Subtract || key == Keys.OemQuestion)
            {
                return "";
            }
            return "";
        }
        public static string updatenumberinput(string text, ref int keypressed)
        {
            bool kommaplaced = false, canminusplaced = false;
            if (text.Contains(","))
                kommaplaced = true;
            if (text.Length == 0)
                canminusplaced = true;
            Keys[] neuekeys = Keyboard.GetState().GetPressedKeys();
            int i = neuekeys.Length - 1;
            if (i >= 0 && keyhandler.IsKeyActive(neuekeys[i]))
            {
                if (neuekeys[i] == Keys.Back && text.Length > 0)
                {
                    text = text.Remove(text.Length - 1, 1);
                }
                else if (kommaplaced == false && text.Length != 0 && neuekeys[i] == Keys.OemComma)
                {
                    text += ",";
                    kommaplaced = true;
                }
                else if (canminusplaced == true && (neuekeys[i] == Keys.OemQuestion || neuekeys[i] == Keys.Subtract))
                {
                    text += "-";
                    canminusplaced = false;
                }
                else if (neuekeys[i] == Keys.Enter)
                {
                    float müll;
                    if (float.TryParse(text, out müll))
                    {
                        keypressed = 1;
                    }
                    else
                        keypressed = 2;
                }
                else if (neuekeys[i] == Keys.Escape)
                {
                    keypressed = -1;
                }
                text += keytonumber(neuekeys[i]);
            }

            return text;
        }
        public static bool keyactiv(int timer)
        {
            if (timer == 1 || timer > 60)
                return true;
            else
                return false;
        }
        public static void DrawRectangle(SpriteBatch Batch, Vector2 pos1, Vector2 pos2, Color color)
        {
            Vector2 size = pos2 - pos1;
            DrawLine(Batch, pos1, new Vector2(pos1.X + size.X - 1, pos1.Y), color);
            DrawLine(Batch, pos1, new Vector2(pos1.X, pos1.Y + size.Y), color);
            DrawLine(Batch, new Vector2(pos2.X - 1, pos2.Y), new Vector2(pos1.X + size.X - 1, pos1.Y), color);
            DrawLine(Batch, pos2, new Vector2(pos1.X, pos1.Y + size.Y), color);
        }
        public void BeginRender3D()
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
        public Vector3 SphereRayIntersection(Vector3 p, Vector3 d, Vector3 SC, double SR)
        {
            Vector3 m = p - SC;
            float b = Vector3.Dot(m, d);
            double c = Vector3.Dot(m, m) - SR * SR;

            // Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
            if (c > 0.0f && b > 0.0f) return Vector3.Zero;
            double discr = b * b - c;

            // A negative discriminant corresponds to ray missing sphere 
            if (discr < 0.0f) return Vector3.Zero;

            // Ray now found to intersect sphere, compute smallest t value of intersection
            float t = (float)(-b - Math.Sqrt(discr));

            // If t is negative, ray started inside sphere so clamp t to zero 
            if (t < 0.0f) t = 0.0f;
            Vector3 q = p + t * d;

            return q;

        }
        #endregion
        #region File Funktions
        public static float[] ConvertByteToFloat(byte[] array)
        {
            float[] floatArr = new float[array.Length / sizeof(float)];
            int index = 0;
            for (int i = 0; i < floatArr.Length; i++)
            {
                floatArr[i] = BitConverter.ToSingle(array, index);
                index += sizeof(float);
            }
            return floatArr;
        }
        public static T[,] toRectangular<T>(T[] flatArray, int width)
        {
            // Break if arraylength is zero
            if (flatArray.Length == 0)
            {
                return null;
            }
            int height = (int)Math.Ceiling(flatArray.Length / (double)width);
            T[,] result = new T[height, width];
            int rowIndex, colIndex;

            for (int index = 0; index < flatArray.Length; index++)
            {
                rowIndex = index / width;
                colIndex = index % width;
                result[rowIndex, colIndex] = flatArray[index];
            }
            return result;
        }
        #endregion
        #region Shader Funktions
        float getstaerke(float minheight, float maxheight, float mingrade, float maxgrade, float grademinwert, float grademaxwert, float istgrade, float height)
        {
            if (height > minheight && height < maxheight)
            {
                float gradevalue = 0;
                float maxmindif = maxgrade - mingrade;
                float maxminwertdif = grademaxwert - grademinwert;
                if (istgrade > mingrade && istgrade < maxgrade)
                {
                    float ministwert = istgrade - mingrade;

                    gradevalue = grademinwert + (ministwert / maxmindif) * maxminwertdif;
                }
                else if (istgrade < mingrade)
                {
                    gradevalue = grademinwert;
                }
                else if (istgrade > maxgrade)
                {
                    gradevalue = grademaxwert;
                }
                float smoothfaktor = 150;
                if (maxheight == 1000)
                {
                    smoothfaktor = 500;
                }
                if (height >= minheight && height <= minheight + smoothfaktor)
                {
                    return ((height - minheight) * (1 / smoothfaktor)) * gradevalue;
                }
                else if (height <= maxheight && height >= maxheight - smoothfaktor)
                {
                    return ((maxheight - height) * (1 / smoothfaktor)) * gradevalue;
                }
                else
                {
                    return 1 * gradevalue;
                }
            }
            else
            {
                return 0;
            }
        }
        #endregion
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            testbox.Inizialise_Input(settings.map_drawquality);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var mouseState = Mouse.GetState(this.Window);
            ButtonState leftmousebutton = mouseclick(gameTime);
            mousepos = new Vector2(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            //int mauspunktID = model.mausaufpunkt(cameraeffect, mousepos);
            bool canbeplaced = Menü.Update(); // Updating Menu
            int mausauffenster = 0;
            if (Punktfenster.aktiv)
                mausauffenster = Punktfenster.Update(gameTime);
            keyhandler.Update();
            if (Map.IsLoaded)
            {
                testbox.Update(gameTime, ref settings.map_drawquality);
                if (Keyboard.GetState().IsKeyDown(Keys.Tab) && stopp2 == 0)
                {
                    Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    stopp2 = 1;
                    if (camarabewegen == 0)
                    {
                        camarabewegen = 1;
                    }
                    else if (camarabewegen == 1)
                    {
                        camarabewegen = 0;
                    }
                }
                if (camarabewegen == 1 && this.IsActive)
                {
                    int changed = 0;
                    float deltax, deltay;
                    deltax = System.Windows.Forms.Cursor.Position.X - cameramousepos.X;
                    deltay = System.Windows.Forms.Cursor.Position.Y - cameramousepos.Y;
                    mouserotationbuffer.X += 0.004f * deltax;
                    mouserotationbuffer.Y += 0.004f * deltay;
                    if (mouserotationbuffer.Y < MathHelper.ToRadians(-88))
                    {
                        mouserotationbuffer.Y = mouserotationbuffer.Y - (mouserotationbuffer.Y - MathHelper.ToRadians(-88));
                    }
                    if (mouserotationbuffer.Y > MathHelper.ToRadians(88))
                    {
                        mouserotationbuffer.Y = mouserotationbuffer.Y - (mouserotationbuffer.Y - MathHelper.ToRadians(88));
                    }
                    if (cameramousepos != mousepos)
                        changed = 1;
                    rotation = new Vector3(-mouserotationbuffer.X, -mouserotationbuffer.Y, 0);
                    if (changed == 1)
                    {
                        System.Windows.Forms.Cursor.Position = mousepointpos;
                        //Mouse.SetPosition((int)cameramousepos.X, (int)cameramousepos.Y);
                    }
                }
                if (Mouse.GetState().RightButton == ButtonState.Pressed && IsActive)
                {
                    if (camarabewegen == 0)
                    {
                        camarabewegen = 1;
                        cameramousepos = mousepos;
                        mousepointpos.X = (int)mousepos.X;
                        mousepointpos.Y = (int)mousepos.Y;
                    }
                }
                if (Mouse.GetState().RightButton == ButtonState.Released && camarabewegen == 1)
                {
                    camarabewegen = 0;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.C) && camfreezstop == 0)
                {
                    camfreezstop = 1;
                    camerafreeze = !camerafreeze;
                }
                if (Keyboard.GetState().IsKeyUp(Keys.C) && camfreezstop == 1)
                {
                    camfreezstop = 0;
                }

                cameraconstspeed = Keyboard.GetState().IsKeyDown(Keys.LeftShift) ? 5 : 0.1f;
                cameraspeed = cameraconstspeed * gameTime.ElapsedGameTime.Ticks / (float)(TimeSpan.TicksPerSecond / 144);

                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    camerapos.Z -= (float)Math.Sin(rotation.X) * cameraspeed;
                    camerapos.X += (float)Math.Cos(rotation.X) * cameraspeed;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    camerapos.Z += (float)Math.Sin(rotation.X) * cameraspeed;
                    camerapos.X -= (float)Math.Cos(rotation.X) * cameraspeed;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    camerapos.Y += cameraspeed;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                {
                    camerapos.Y -= cameraspeed;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    sonnenwinkel += 0.005f;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    sonnenwinkel -= 0.005f;
                }
                sonnenwinkel += 0.000025f;

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    settings.water_ref_quality += 0.005f;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    settings.water_ref_quality -= 0.005f;
                }

                ligthdirection.X = 0;
                ligthdirection.Y = -(float)Math.Sin(sonnenwinkel);
                ligthdirection.Z = (float)Math.Cos(sonnenwinkel);
                if (leftmousebutton == ButtonState.Pressed && modus == 2 && canbeplaced)
                {
                    model.UpdateList();
                    model.punktelist.Add(new punkt(ScreentoSpace(mousepos, GraphicsDevice, 200), Vector3.Zero, 1, 1));
                    model.UpdateArray();
                    Punktfenster.punktübergabe(model.punkte.Length - 1);
                }
                //if (leftmousebutton == ButtonState.Pressed && modus == 3 && canbeplaced)
                //{
                //    if (verbmod == 0)
                //    {
                //        if (mauspunktID != -1)
                //        {
                //            verbpos1ID = mauspunktID;
                //            verbpos2 = ScreentoSpace(mousepos, GraphicsDevice, 1);
                //            verbmod = 1;
                //        }
                //    }
                //    else if (verbmod == 1)
                //    {
                //        if (mauspunktID != -1 && verbpos1ID != mauspunktID)
                //        {
                //            verbpos2 = ScreentoSpace(mousepos, GraphicsDevice, 1);
                //            model.UpdateList();
                //            float länge = (model.punkte[mauspunktID].pos - model.punkte[verbpos1ID].pos).Length();
                //            model.verbindungenlist.Add(new verbindung(länge, 1, 1, mauspunktID, verbpos1ID));
                //            model.UpdateArray();
                //            verbmod = 0;
                //        }
                //    }
                //}
                //if (leftmousebutton == ButtonState.Pressed && mauspunktID != -1 && mausauffenster == 0 && modus == 1)
                //{
                //    Punktfenster.punktübergabe(mauspunktID);
                //    Punktfenster.aktiv = true;
                //    Punktfenster.Update(gameTime);
                //}
                //if (keyhandler.IsKeyActive(Keys.Delete) && mauspunktID != -1 && mausauffenster == 0 && modus == 1)
                //{
                //    model.UpdateList();
                //    List<int> zulöschendeverb = new List<int>();
                //    for (int i = 0; i < model.verbindungenlist.Count; i++)
                //    {
                //        if (model.verbindungenlist[i].punkt1 == mauspunktID || model.verbindungenlist[i].punkt2 == mauspunktID)
                //        {
                //            zulöschendeverb.Add(i);
                //        }
                //    }
                //    for (int i = zulöschendeverb.Count - 1; i >= 0; i--)
                //    {
                //        model.verbindungenlist.RemoveAt(zulöschendeverb[i]);
                //    }
                //    model.punktelist.RemoveAt(mauspunktID);
                //    model.UpdateArray();
                //    for (int i = 0; i < model.verbindungen.Length; i++)
                //    {
                //        if (model.verbindungen[i].punkt1 > mauspunktID)
                //            model.verbindungen[i].punkt1 -= 1;
                //        if (model.verbindungen[i].punkt2 > mauspunktID)
                //            model.verbindungen[i].punkt2 -= 1;
                //    }
                //}

                #region Updating Projection
                if (cameratomapdist == 0 && camerapos.Y > 50000)
                {
                    cameratomapdist = 1;
                    camfarplane = 100000;
                    camnearplane = 10000;
                    cameraeffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(80), GraphicsDevice.Viewport.AspectRatio, camnearplane, camfarplane);
                }
                else if (camerapos.Y < 50000)
                {
                    cameratomapdist = 0;
                    camfarplane = 100000;
                    camnearplane = MathHelper.Max(Map.Smallestdistancetomap / 5.0f, 0.5f);
                    cameraeffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(80), GraphicsDevice.Viewport.AspectRatio, camnearplane, camfarplane);
                }
                #endregion

                #region Updating Camera
                Matrix rotationMatrix = Matrix.CreateRotationY(rotation.X);// * Matrix.CreateRotationX(rotationY);
                Vector3 transformedReference = Vector3.TransformNormal(new Vector3(0, 0, 1000), rotationMatrix);
                Vector3 cameraLookat = camerapos + transformedReference;
                camerarichtung.Y = cameraLookat.Y - (float)Math.Sin(-rotation.Y) * Vector3.Distance(camerapos, cameraLookat);
                camerarichtung.X = cameraLookat.X - (cameraLookat.X - camerapos.X) * (float)(1 - Math.Cos(rotation.Y));
                camerarichtung.Z = cameraLookat.Z - (cameraLookat.Z - camerapos.Z) * (float)(1 - Math.Cos(rotation.Y));

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    var camerablickrichtung = camerapos - camerarichtung;
                    camerablickrichtung = camerablickrichtung / camerablickrichtung.Length();
                    camerapos -= camerablickrichtung * cameraspeed;
                    camerarichtung -= camerablickrichtung * cameraspeed;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    var camerablickrichtung = camerapos - camerarichtung;
                    camerablickrichtung = camerablickrichtung / camerablickrichtung.Length();
                    camerapos += camerablickrichtung * cameraspeed;
                    camerarichtung += camerablickrichtung * cameraspeed;
                }
                if (camerafreeze == false)
                {
                    campos2 = camerapos;
                    camrichtung2 = camerarichtung;
                }
                cameraeffect.View = Matrix.CreateLookAt(camerapos, camerarichtung, Vector3.Up);
                camworld = cameraeffect.World;
                camview = cameraeffect.View;
                camprojection = cameraeffect.Projection;
                #endregion

                Map.Update(gameTime.ElapsedGameTime.Ticks);
            }
            else
            {

            }
            base.Update(gameTime);
            watch.Stop();
            updatetime = watch.ElapsedMilliseconds;
        }

        float ComputeGaussian(float n)
        {
            float theta = 4;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        protected override void Draw(GameTime gameTime)
        {
            GameWatch.Stop();
            FPS = 0;
            foreach (var t in FPS_werte)
            {
                FPS += t;
            }
            FPS /= FPS_werte.Length;
            //FPS_werte[fps_index] = (float)System.Diagnostics.Stopwatch.Frequency / (float)GameWatch.ElapsedTicks;
            FPS_werte[fps_index] = TimeSpan.TicksPerSecond / (float)gameTime.ElapsedGameTime.Ticks;
            fps_index++;
            fps_index = fps_index % FPS_werte.Length;
            GameWatch.Reset();
            GameWatch.Start();

            float aaa = GraphicsDevice.Viewport.AspectRatio;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            BeginRender3D();

            if (Map.IsLoaded)
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                //model.Draw(cameraeffect);
                // Reseting chunk anz
                Map.chunkdrawanz = 0;

                //                           //
                //                           //
                // D R A W I N G   W O R L D //
                //                           //
                //                           //

                if (settings.water_skybox_ref || settings.water_map_ref)
                {
                    #region Generating Reflections

                    // Inizialising mirrored View for Reflections
                    //Matrix reflectionview = Matrix.CreateLookAt(new Vector3(camerapos.X, -camerapos.Y, camerapos.Z), new Vector3(camerarichtung.X, -camerarichtung.Y, camerarichtung.Z), Vector3.Up);
                    //Map.UpdateMatrixes(reflectionview, camworld, camprojection);

                    ////Setting RenderTarget to the reflection Texture
                    //GraphicsDevice.SetRenderTargets(Map.reflectionmap, Map.reflectiondepthmap);
                    //GraphicsDevice.Clear(Color.Transparent);

                    //if (settings.water_map_ref)
                    //    Map.Draw(Matrix.Identity, false);

                    //if (settings.water_skybox_ref)
                    //{
                    //    Skybox.shader.Parameters["output2type"].SetValue(2); // Changing to depth output
                    //    Skybox.Draw(camworld, reflectionview, Matrix.CreateScale(camfarplane) * Matrix.CreateTranslation(new Vector3(camerapos.X, -camerapos.Y, camerapos.Z)), new Vector3(camerapos.X, -camerapos.Y, camerapos.Z));
                    //}
                    //GraphicsDevice.SetRenderTargets(null);
                    #endregion
                }
                // Clearing the Render Targets
                GraphicsDevice.SetRenderTarget(bloomtarget);
                GraphicsDevice.Clear(Color.Transparent);

                // Drawing the Map, Water and Skybox
                GraphicsDevice.SetRenderTargets(maintarget, heighttarget, maptarget);
                GraphicsDevice.Clear(Color.Transparent);

                Map.UpdateMatrixes(camview, camworld, camprojection);
                Map.Draw(Matrix.Identity, false);
                if (settings.Bloom)
                    GraphicsDevice.SetRenderTargets(maintarget, bloomtarget);
                else
                    GraphicsDevice.SetRenderTarget(maintarget);
                Map.DrawWater(cameraeffect);
                Skybox.shader.Parameters["output2type"].SetValue(1); // Changing to bloom output
                Skybox.Draw(camworld, camview, Matrix.CreateScale(camfarplane) * Matrix.CreateTranslation(camerapos), camerapos);
                GraphicsDevice.SetRenderTarget(null);

                if (settings.Bloom)
                {
                    #region Generating Bloom Effect
                    /*GraphicsDevice.SetRenderTarget(bloomtarget2);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin();
                    spriteBatch.Draw(bloomtarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
                    spriteBatch.End();

                    blureffecthorizontal.Parameters["width"].SetValue(1920 / 4);

                    GraphicsDevice.SetRenderTarget(bloomtarget3);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin(SpriteSortMode.Texture, null, null, null, null, blureffecthorizontal, Matrix.Identity);
                    spriteBatch.Draw(bloomtarget2, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spriteBatch.End();

                    blureffectvertical.Parameters["height"].SetValue(1080 / 4);
                    GraphicsDevice.SetRenderTarget(bloomtarget2);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, blureffectvertical, Matrix.Identity);
                    spriteBatch.Draw(bloomtarget3, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spriteBatch.End();

                    GraphicsDevice.SetRenderTarget(bloomtarget4);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin();
                    spriteBatch.Draw(bloomtarget2, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 4.0f, SpriteEffects.None, 0);
                    spriteBatch.End();*/

                    GraphicsDevice.SetRenderTarget(bloomtarget2);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin();
                    spriteBatch.Draw(bloomtarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f * new Vector2(1920 / (float)Screenwidth, 1080 / (float)Screenheight), SpriteEffects.None, 0);
                    spriteBatch.End();

                    blureffecthorizontal.Parameters["width"].SetValue((Screenwidth / 2));
                    GraphicsDevice.SetRenderTarget(bloomtarget3);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin(SpriteSortMode.Texture, null, null, null, null, blureffecthorizontal, Matrix.Identity);
                    spriteBatch.Draw(bloomtarget2, Vector2.Zero, Color.White);
                    spriteBatch.End();

                    blureffectvertical.Parameters["height"].SetValue((Screenheight / 2));
                    GraphicsDevice.SetRenderTarget(bloomtarget4);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, blureffectvertical, Matrix.Identity);
                    spriteBatch.Draw(bloomtarget3, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2 * new Vector2(Screenwidth / 1920.0f, Screenheight / 1080.0f), SpriteEffects.None, 0);
                    spriteBatch.End();

                    //GraphicsDevice.SetRenderTarget(null);

                    GraphicsDevice.SetRenderTarget(maintarget);
                    combineffect.Parameters["tex"].SetValue(bloomtarget4);
                    combineffect.Parameters["SpriteTexture"].SetValue(maintarget);
                    combineffect.Parameters["lightcolor"].SetValue(lightcolor);
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, combineffect, Matrix.Identity);
                    spriteBatch.Draw(maintarget, Vector2.Zero, Color.White);
                    spriteBatch.End();
                    GraphicsDevice.SetRenderTarget(null);
                    #endregion
                }

                // Drawing God Rays

                GraphicsDevice.SetRenderTargets(maintarget2);
                GraphicsDevice.Clear(Color.Transparent);
                //Map.UpdateMatrixes(camview, camworld, Matrix.CreateScale(camfarplane) * Matrix.CreateTranslation(camerapos));
                //Map.Draw(Matrix.Identity, false);
                Skybox.shader.Parameters["output2type"].SetValue(3);
                Skybox.Draw(camworld, camview, Matrix.CreateScale(camfarplane) * Matrix.CreateTranslation(camerapos), camerapos);
                GraphicsDevice.SetRenderTarget(null);

                Vector3 fakesunpos = camerapos + (ligthdirection * 10000.0f);
                fakesunpos = Vector3.Transform(fakesunpos, camview);
                fakesunpos.X *= -1;
                Vector3 TLD = Vector3.Normalize(fakesunpos);

                GraphicsDevice.SetRenderTarget(test_rendertarget2D);
                obstacleeffect.Parameters["transformedLightDirection"].SetValue(TLD);
                obstacleeffect.Parameters["lightmap"].SetValue(maintarget2);
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, obstacleeffect, Matrix.Identity);
                spriteBatch.Draw(maptarget, Vector2.Zero, Color.White);
                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);

                BeginRender3D();

                GraphicsDevice.SetRenderTargets(maintarget);
                Skybox.shader.Parameters["output2type"].SetValue(0);
                Skybox.Draw(camworld, camview, Matrix.CreateScale(camfarplane) * Matrix.CreateTranslation(camerapos), camerapos);
                GraphicsDevice.SetRenderTarget(null);
                
                Vector3 projsun = GraphicsDevice.Viewport.Project(camerapos + (ligthdirection * 10000.0f), camprojection, camview, camworld);

                godrayeffect.Parameters["projsunpos"].SetValue(projsun);
                godrayeffect.Parameters["World"].SetValue(camworld * Matrix.CreateScale(camfarplane) * Matrix.CreateTranslation(camerapos));
                godrayeffect.Parameters["View"].SetValue(camview);
                godrayeffect.Parameters["Projection"].SetValue(Game1.cameraeffect.Projection);
                godrayeffect.Parameters["EyePosition"].SetValue(camerapos);
                godrayeffect.Parameters["LightDirection"].SetValue(Game1.ligthdirection);
                godrayeffect.Parameters["obstaclemap"].SetValue(test_rendertarget2D);
                godrayeffect.Parameters["colormap"].SetValue(maintarget);
                godrayeffect.Parameters["transformedLightDirection"].SetValue(TLD);
                godrayeffect.Parameters["invviewmatrix"].SetValue(Matrix.Invert(camview));

                GraphicsDevice.SetRenderTarget(postprocesstarget);
                GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, godrayeffect, Matrix.Identity);
                spriteBatch.Draw(test_rendertarget2D, Vector2.Zero, Color.White);
                spriteBatch.End();
                
                GraphicsDevice.SetRenderTarget(null);
                

                spriteBatch.Begin();
                //spriteBatch.Draw(maintarget, Vector2.Zero, Color.White);
                //spriteBatch.Draw(Map.heights_rough, Vector2.Zero, Color.White * 0.001f);
                spriteBatch.Draw(postprocesstarget, Vector2.Zero, Color.White * 0.7f);

                //Menü.Draw();
                //TB.Draw(spriteBatch, gameTime);

                #region platzieren
                if (modus == 2)
                {
                    MeshMatrix(ref spheremodel, sphereoriginalmatrix[0], 0, Matrix.CreateTranslation(ScreentoSpace(mousePosition, GraphicsDevice, 200)));
                    DrawMesh(cameraeffect, spheremodel, Shader);
                }
                if (modus == 3)
                {
                    if (verbmod == 1)
                    {
                        verbpos2 = ScreentoSpace(mousePosition, GraphicsDevice, 1);
                        DrawLine3d(GraphicsDevice, cameraeffect, model.punkte[verbpos1ID].pos, verbpos2);
                    }
                }

                #endregion

                if (Punktfenster.aktiv)
                    Punktfenster.Draw(spriteBatch, gameTime);
                var xpos = Spacetoscreen(new Vector3(100, 0, 0));
                var ypos = Spacetoscreen(new Vector3(0, 100, 0));
                var zpos = Spacetoscreen(new Vector3(0, 0, 100));
                if (xpos != Vector2.Zero) { spriteBatch.DrawString(font, "X", xpos, Color.Red); }
                if (ypos != Vector2.Zero) { spriteBatch.DrawString(font, "Y", ypos, Color.Red); }
                if (zpos != Vector2.Zero) { spriteBatch.DrawString(font, "Z", zpos, Color.Red); }
                //spriteBatch.Draw(crosshair, new Vector2(GraphicsDevice.Viewport.Width/2 - 7, GraphicsDevice.Viewport.Height/2 - 7), Color.White);
                spriteBatch.DrawString(font, camerapos.ToString(), new Vector2(100, 100), Color.Red);
                spriteBatch.DrawString(font, GC.CollectionCount(1).ToString(), new Vector2(100, 130), Color.Red);
                spriteBatch.DrawString(font, Map.chunkdrawanz.ToString(), new Vector2(100, 160), Color.Red);
                spriteBatch.DrawString(font, (Map.chunkdrawanz * (Map.chunksize - 1) * (Map.chunksize - 1) * 2).ToString(), new Vector2(100, 190), Color.Red);
                spriteBatch.DrawString(font, "Drawdistance: " + settings.map_drawdistance.ToString(), new Vector2(100, 220), Color.Red);
                spriteBatch.DrawString(font, "Drawquality: " + settings.map_drawquality.ToString(), new Vector2(100, 250), Color.Red);
                spriteBatch.DrawString(font, "Reflectionquality: " + settings.water_ref_quality.ToString(), new Vector2(100, 280), Color.Red);
                spriteBatch.DrawString(font, "FPS: " + ((int)FPS).ToString(), new Vector2(1000, 50), Color.Black);
                spriteBatch.DrawString(font, drawtime.ToString(), new Vector2(100, 310), Color.Black);
                spriteBatch.DrawString(font, gameTime.ElapsedGameTime.Ticks.ToString(), new Vector2(100, 340), Color.Black);
                spriteBatch.DrawString(font, Map.Smallestdistancetomap.ToString(), new Vector2(100, 370), Color.Black);
                //spriteBatch.DrawString(font, TLD.ToString(), new Vector2(100, 400), Color.White);
                //testbox.Draw(spriteBatch, gameTime);
                spriteBatch.End();
            }
            else
            {
                lock (GraphicsDevice)
                {
                    GraphicsDevice.Clear(Color.White);
                    spriteBatch.Begin();
                    spriteBatch.DrawString(font, Map.loadingtype, new Vector2(Screenwidth / 2 - font.MeasureString(Map.loadingtype).X / 2, Screenheight / 2 - font.MeasureString(Map.loadingtype).Y / 2), Color.Black);
                    spriteBatch.DrawString(font, (Map.loadingprogress * 100).ToString("F0"), new Vector2(Screenwidth / 2 - font.MeasureString((Map.loadingprogress * 100).ToString("F0")).X / 2, Screenheight / 2 - font.MeasureString((Map.loadingprogress * 100).ToString("F0")).Y / 2 + 20), Color.Black);
                    spriteBatch.End();
                }
                //Thread.Sleep(450);
            }
            base.Draw(gameTime);
            watch.Stop();
            drawtime = watch.ElapsedMilliseconds;
        }

    }
}
