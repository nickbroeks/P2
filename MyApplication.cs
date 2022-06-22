using System.Diagnostics;
using OpenTK;

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
		Texture wood;                           // texture to use for rendering
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing
		bool useRenderTarget = true;
		SceneGraph sceneGraph = new SceneGraph();

		// initialize
		public void Init()
		{
			// load a texture
			wood = new Texture("../../assets/wood.jpg");
			// create shaders
			shader = new Shader("../../shaders/vs.glsl", "../../shaders/fs.glsl");
			postproc = new Shader("../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl");
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
		}

		// tick for background surface
		public void Tick()
		{
			screen.Clear( 0 );
			screen.Print( "hello world", 2, 2, 0xffff00 );
		}

		// tick for OpenGL rendering code
		public void RenderGL()
		{
			// measure frame duration
			float frameDuration = timer.ElapsedMilliseconds;
			timer.Reset();
			timer.Start();

			// prepare matrix for vertex shader
			float angle90degrees = PI / 2;
			Matrix4 Tcamera = Matrix4.CreateTranslation( new Vector3( 0, -14.5f, 0 ) ) * Matrix4.CreateFromAxisAngle( new Vector3( 1, 0, 0 ), angle90degrees );
			// update rotation
			a += 0.001f * frameDuration;
			if( a > 2 * PI ) a -= 2 * PI;

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