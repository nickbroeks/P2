using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Template
{
	class MyApplication
	{
		// member variables
		public Surface screen;                  // background surface for printing etc.
		const float PI = 3.1415926535f;         // PI
		float a = 0;                            // teapot rotation angle
		Stopwatch timer;                        // timer for measuring frame duration
		Shader shader;                          // shader to use for rendering
		Shader postproc;                        // shader to use for post processing
		Texture wood, white;                           // texture to use for rendering
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing
		Mesh plane, teapot;
		bool useRenderTarget = true;
		public int uniform_amblight;
		public int uniform_camposition;
		public int ssbo_lights;
		public Vector4 ambientLight;
		public Vector3 cameraPosition;
		Light[] lights;
		Matrix4 Tcamera;
		SceneGraph sceneGraph = new SceneGraph();

		// initialize
		public void Init()
		{
			ambientLight = new Vector4(0.2f, 0.2f, 0.2f, 0);
			lights = new Light[] {
				new Light(new Vector3(8, 8, 8), new Vector4(1f, 0f, 0f, 0f)),
				new Light(new Vector3(8, 8, -8), new Vector4(0f, 1f, 0f, 1f)),
				new Light(new Vector3(-8, 8, 8), new Vector4(0f, 0f, 1f, 1f))
			};
			// load a texture
			wood = new Texture("../../assets/wood.jpg");
			// create shaders
			shader = new Shader("../../shaders/vs.glsl", "../../shaders/fs.glsl");
			postproc = new Shader("../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl");
			white = new Texture("../../assets/white.jpg");
			// load teapot
			teapot = new Mesh( "../../assets/teapot.obj", Matrix4.CreateTranslation(0, 0, 0) * Matrix4.CreateScale(0.2f), shader, white);
			Mesh floor = new Mesh( "../../assets/floor.obj", Matrix4.CreateScale(4.0f), shader, wood );
			plane = new Mesh("../../assets/paper_airplane.obj", Matrix4.CreateScale(2f) * Matrix4.CreateTranslation(0.3f, 0.1f, 0), shader, white);
			teapot.Children.Add(plane);
			floor.Children.Add(teapot);
			sceneGraph.meshes.Add(floor);

			// initialize stopwatch
			timer = new Stopwatch();
			timer.Reset();
			timer.Start();

			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			quad = new ScreenQuad();

			uniform_amblight = GL.GetUniformLocation(shader.programID, "ambLight");
			uniform_camposition = GL.GetUniformLocation(shader.programID, "vCamPosition");
			ssbo_lights = GL.GenBuffer();
			GL.UseProgram(shader.programID);
			GL.Uniform4(uniform_amblight, ambientLight);
			GL.Uniform3(uniform_camposition, ref cameraPosition);
			LoadLights();
			Tcamera = Matrix4.CreateTranslation(new Vector3(0, -4f, -10f));
		}

		public void LoadLights()
        {
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo_lights);
			GL.BufferData(BufferTarget.ShaderStorageBuffer, (IntPtr)(lights.Length * Marshal.SizeOf(typeof(Light))), lights, BufferUsageHint.StaticDraw);
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, ssbo_lights);
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
		}
		// tick for background surface
		public void Tick()
		{
			screen.Clear( 0 );
			screen.Print( "hello world", 2, 2, 0xffff00 );
			HandleInput(Keyboard.GetState());
			a += 0.02f;
			teapot.ModelMatrix = Matrix4.CreateTranslation(0, 5*(float)Math.Sin(5*a), 0) * Matrix4.CreateScale(0.2f);
			plane.ModelMatrix = Matrix4.CreateScale(200f) * Matrix4.CreateTranslation(20f, 10f, 0) * Matrix4.CreateRotationY(a);
		}
		/// <summary>
		/// Method that gets the input values and passes the used values to the raytracer
		/// </summary>
		/// <param name="keyboard">The current keyboard state</param>
		/// <param name="mouse">The current mouse state</param>
		public void HandleInput(KeyboardState keyboard)
		{
			float angle = 0.04f;
			if (keyboard.IsAnyKeyDown) {
				if (keyboard.IsKeyDown(Key.W)) {
					Tcamera *= Matrix4.CreateTranslation(0, 0, 0.1f);
				}
				if (keyboard.IsKeyDown(Key.A)) {
					Tcamera *= Matrix4.CreateTranslation(0.1f, 0, 0);
				}
				if (keyboard.IsKeyDown(Key.S)) {
					Tcamera *= Matrix4.CreateTranslation(0, 0, -0.1f);
				}
				if (keyboard.IsKeyDown(Key.D)) {
					Tcamera *= Matrix4.CreateTranslation(-0.1f, 0, 0);
				}
				if (keyboard.IsKeyDown(Key.Q)) {
					Tcamera *= Matrix4.CreateRotationY(-angle);
				}
				if (keyboard.IsKeyDown(Key.E)) {
					Tcamera *= Matrix4.CreateRotationY(angle);
				}
				
			}
		}
		// tick for OpenGL rendering code
		public void RenderGL()
		{
			// measure frame duration
			timer.Reset();
			timer.Start();


			if( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene to render target
				sceneGraph.Render(shader, Tcamera);
				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene directly to the screen
				sceneGraph.Render(shader, Tcamera);
			}
		}
	}
}