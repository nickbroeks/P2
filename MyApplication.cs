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
		Mesh teapot, floor;                       // a mesh to draw using OpenGL
		const float PI = 3.1415926535f;         // PI
		float a = 0;                            // teapot rotation angle
		Stopwatch timer;                        // timer for measuring frame duration
		Shader shader;                          // shader to use for rendering
		Shader postproc;                        // shader to use for post processing
		Texture wood, white;                           // texture to use for rendering
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing
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
			teapot = new Mesh( "../../assets/teapot.obj", Matrix4.CreateScale(0.5f) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), a ), shader,wood);
			floor = new Mesh( "../../assets/floor.obj", Matrix4.CreateScale(4.0f) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), a), shader, wood );
			sceneGraph.meshes.Add(teapot);
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

			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo_lights);
			GL.BufferData(BufferTarget.ShaderStorageBuffer, (IntPtr)(lights.Length * Marshal.SizeOf(typeof(Light))), lights, BufferUsageHint.StaticDraw );
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, ssbo_lights);
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

			float angle90degrees = PI / 2;
			Tcamera = Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), a)
				* Matrix4.CreateTranslation(new Vector3(0, -4f, -10f));
		}

		// tick for background surface
		public void Tick()
		{
			screen.Clear( 0 );
			screen.Print( "hello world", 2, 2, 0xffff00 );
			HandleInput(Keyboard.GetState());
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
			float frameDuration = timer.ElapsedMilliseconds;
			timer.Reset();
			timer.Start();

			cameraPosition = 10 * new Vector3(-(float)Math.Sin(a),1f, (float)-Math.Cos(a));

			// prepare matrix for vertex shader
			
			Matrix4 Tpot = Matrix4.CreateScale( 0.5f ) * Matrix4.CreateFromAxisAngle( new Vector3( 0, 1, 0 ), 0 );
			Matrix4 Tfloor = Matrix4.CreateScale( 4.0f ) * Matrix4.CreateFromAxisAngle( new Vector3( 0, 1, 0 ), 0 );
			//Matrix4 Tcamera = Matrix4.CreateTranslation( new Vector3( 0, -14.5f, 0 ) ) * Matrix4.CreateFromAxisAngle( new Vector3( 1, 0, 0 ), angle90degrees );
			
			Matrix4 Tview = Matrix4.CreatePerspectiveFieldOfView( 1.2f, 1.3f, .1f, 1000 );


			if( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene to render target
				sceneGraph.Render(Tcamera);
				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene directly to the screen
				sceneGraph.Render(Tcamera);
			}
		}
	}
}