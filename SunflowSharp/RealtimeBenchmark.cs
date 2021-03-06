using System;
using SunflowSharp.Core;
using SunflowSharp.Core.Camera;
using SunflowSharp.Core.Display;
using SunflowSharp.Core.Light;
using SunflowSharp.Core.Primitive;
using SunflowSharp.Core.Shader;
using SunflowSharp.Core.Tesselatable;
using SunflowSharp.Image;
using SunflowSharp.Maths;
using SunflowSharp.Systems;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp
{
    public class RealtimeBenchmark : SunflowAPI
    {
        public RealtimeBenchmark(bool showGUI, int threads)
        {
            IDisplay display = /*showGUI ? new FastDisplay() :*/ new FileDisplay(false);
            UI.printInfo(UI.Module.BENCH, "Preparing benchmarking scene ...");
            // settings
            parameter("threads", threads);
            // spawn regular priority threads
            parameter("threads.lowPriority", false);
            parameter("resolutionX", 512);
            parameter("resolutionY", 512);
            parameter("aa.min", -3);
            parameter("aa.max", 0);
            parameter("depths.diffuse", 1);
            parameter("depths.reflection", 1);
            parameter("depths.refraction", 0);
            parameter("bucket.order", "hilbert");
            parameter("bucket.size", 32);
            options(SunflowAPI.DEFAULT_OPTIONS);
            // camera
            Point3 eye = new Point3(30, 0, 10.967f);
            Point3 target = new Point3(0, 0, 5.4f);
            Vector3 up = new Vector3(0, 0, 1);
			parameter("transform", Matrix4.lookAt(eye, target, up));
            parameter("fov", 45.0f);
			camera("camera", "pinhole");
			parameter("camera", "camera");
            options(SunflowAPI.DEFAULT_OPTIONS);
            // geometry
            createGeometry();
            // this first render is not timed, it caches the acceleration data
            // structures and tesselations so they won't be
            // included in the main timing
            UI.printInfo(UI.Module.BENCH, "Rendering warmup frame ...");
            render(SunflowAPI.DEFAULT_OPTIONS, display);
            // now disable all output - and run the benchmark
            UI.set(null);
            Timer t = new Timer();
            t.start();
            float phi = 0;
            int frames = 0;
            while (phi < 4 * Math.PI)
            {
                eye.x = 30 * (float)Math.Cos(phi);
                eye.y = 30 * (float)Math.Sin(phi);
                phi += (float)(Math.PI / 30);
                frames++;
                // update camera
				parameter("transform", Matrix4.lookAt(eye, target, up));
				camera("camera", null);
                render(SunflowAPI.DEFAULT_OPTIONS, display);
            }
            t.end();
            UI.set(new ConsoleInterface());
            UI.printInfo(UI.Module.BENCH, "Benchmark results:");
			UI.printInfo(UI.Module.BENCH, "  * Average FPS:         {0,6:0.00", frames / t.seconds());
			UI.printInfo(UI.Module.BENCH, "  * Total time:          {0}", t);
        }

        private void createGeometry()
        {
            // light source
            parameter("source", new Point3(-15.5945f, -30.0581f, 45.967f));
            parameter("dir", new Vector3(15.5945f, 30.0581f, -45.967f));
            parameter("radius", 60.0f);
			parameter("radiance", null, 3, 3, 3);
            light("light", "directional");

            // gi-engine
            parameter("gi.engine", "fake");
            parameter("gi.fake.sky", null, 0.25f, 0.25f, 0.25f);
            parameter("gi.fake.ground", null, 0.01f, 0.01f, 0.5f);
            parameter("gi.fake.up", new Vector3(0, 0, 1));
            options(DEFAULT_OPTIONS);

            // shaders
			parameter("diffuse", null, 0.5f, 0.5f, 0.5f);
			shader("default", "diffuse");
			parameter("diffuse",  null, 0.5f, 0.5f, 0.5f);
            parameter("shiny", 0.2f);
			shader("refl", "shiny_diffuse");
            // objects

            // teapot
            parameter("subdivs", 10);
            geometry("teapot", "teapot");
            parameter("shaders", "default");
            Matrix4 m = Matrix4.IDENTITY;
            m = Matrix4.scale(0.075f).multiply(m);
            m = Matrix4.rotateZ((float)MathUtils.toRadians(-45f)).multiply(m);
            m = Matrix4.translation(-7, 0, 0).multiply(m);
            parameter("transform", m);
            instance("teapot.instance", "teapot");

            // gumbo
            parameter("subdivs", 10);
			geometry("gumbo", "gumbo");
            m = Matrix4.IDENTITY;
            m = Matrix4.scale(0.5f).multiply(m);
            m = Matrix4.rotateZ((float)MathUtils.toRadians(25f)).multiply(m);
            m = Matrix4.translation(3, -7, 0).multiply(m);
            parameter("shaders", "default");
            parameter("transform", m);
            instance("gumbo.instance", "gumbo");

            // ground plane
            parameter("center", new Point3(0, 0, 0));
            parameter("normal", new Vector3(0, 0, 1));
            geometry("ground", "plane");
            parameter("shaders", "refl");
            instance("ground.instance", "ground");
        }
    }
}