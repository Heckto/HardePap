using Game1.DataContext;
using Game1.GameObjects.Graphics.Effects;
using Game1.GameObjects.Graphics.PostProcessing;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Game1.GameObjects.Graphics.PostProcessing
{
	public class BloomPostProcessor : PostProcessor
	{
        int i = 0;
		/// <summary>
		/// the settings used by the bloom and blur shaders. If changed, you must call setBloomSettings for the changes to take effect.
		/// </summary>
		public BloomSettings Settings
		{
			get => _settings;
			set => SetBloomSettings(value);
		}

		/// <summary>
		/// scale of the internal RenderTargets. For high resolution renders a half sized RT is usually more than enough. Defaults to 1.
		/// </summary>
		public float RenderTargetScale
		{
			get => _renderTargetScale;
			set
			{
				if (_renderTargetScale != value)
				{
					_renderTargetScale = value;
					UpdateBlurEffectDeltas();
				}
			}
		}

		float _renderTargetScale = 1f;
		BloomSettings _settings;

		Effect _bloomExtractEffect;
		Effect _bloomCombineEffect;
		GaussianBlurEffect _gaussianBlurEffect;

		// extract params
		EffectParameter _bloomExtractThresholdParam;

		// combine params
		EffectParameter _bloomIntensityParam,
			_bloomBaseIntensityParam,
			_bloomSaturationParam,
			_bloomBaseSaturationParam,
			_bloomBaseMapParm;


		public BloomPostProcessor(GameContext context) : base(context)
		{

			_settings = BloomSettings.PresetSettings[3];
            LoadContent(context.content);
		}

		public void LoadContent(ContentManager content)
		{
            //base.OnAddedToScene(scene);
            _bloomExtractEffect = content.Load<Effect>("Effects/bloomExtract2");
            _bloomCombineEffect = content.Load<Effect>("Effects/bloomCombine2");

            _gaussianBlurEffect = new GaussianBlurEffect(game_context.graphics, content.Load<Effect>("Effects/gaussianBlur"));

            _bloomExtractThresholdParam = _bloomExtractEffect.Parameters["_bloomThreshold"];

            _bloomIntensityParam = _bloomCombineEffect.Parameters["_bloomIntensity"];
            _bloomBaseIntensityParam = _bloomCombineEffect.Parameters["_baseIntensity"];
            _bloomSaturationParam = _bloomCombineEffect.Parameters["_bloomSaturation"];
            _bloomBaseSaturationParam = _bloomCombineEffect.Parameters["_baseSaturation"];
            _bloomBaseMapParm = _bloomCombineEffect.Parameters["_baseMap"];

            SetBloomSettings(_settings);
        }

		public void Unload(ContentManager content)
		{            
            //content.Unload(_bloomExtractEffect);
            //content.Unload(_bloomCombineEffect);
            //content.UnloadUnloadEffect(_gaussianBlurEffect);

			base.Unload();
		}

		/// <summary>
		/// sets the settings used by the bloom and blur shaders
		/// </summary>
		/// <param name="settings">Settings.</param>
		public void SetBloomSettings(BloomSettings settings)
		{
			_settings = settings;

			_bloomExtractThresholdParam.SetValue(_settings.Threshold);

			_bloomIntensityParam.SetValue(_settings.Intensity);
			_bloomBaseIntensityParam.SetValue(_settings.BaseIntensity);
			_bloomSaturationParam.SetValue(_settings.Saturation);
			_bloomBaseSaturationParam.SetValue(_settings.BaseSaturation);

			_gaussianBlurEffect.BlurAmount = _settings.BlurAmount;
		}

        

		/// <summary>
		/// updates the Effect with the new vertical and horizontal deltas
		/// </summary>
		void UpdateBlurEffectDeltas()
		{
            
			var sceneRenderTargetSize = game_context.graphics.Viewport;
			_gaussianBlurEffect.HorizontalBlurDelta = 1f / (sceneRenderTargetSize.X * _renderTargetScale);
			_gaussianBlurEffect.VerticalBlurDelta = 1f / (sceneRenderTargetSize.Y * _renderTargetScale);
		}
        
		public override void Process(SpriteBatch sb, RenderTarget2D source, RenderTarget2D destination)
		{
            

			// aquire two rendertargets for the bloom processing. These can be scaled via renderTargetScale in order to minimize fillrate costs. Reducing
			// the resolution in this way doesn't hurt quality, because we are going to be blurring the bloom images in any case.
			var sceneRenderTargetSize = game_context.graphics.Viewport;


            var renderTarget1 = new RenderTarget2D(game_context.graphics, sceneRenderTargetSize.Width, sceneRenderTargetSize.Height, false, SurfaceFormat.Color, DepthFormat.None);
            DrawFullscreenQuad(sb, source, renderTarget1, _bloomExtractEffect);


            
            //var renderTarget1 = RenderTarget.GetTemporary((int) (sceneRenderTargetSize.X * RenderTargetScale),
            //(int) (sceneRenderTargetSize.Y * RenderTargetScale), DepthFormat.None);
            var renderTarget2 = new RenderTarget2D(game_context.graphics, sceneRenderTargetSize.Width, sceneRenderTargetSize.Height);
            //        var renderTarget2 = RenderTarget.GetTemporary((int) (sceneRenderTargetSize.X * RenderTargetScale),
            //(int) (sceneRenderTargetSize.Y * RenderTargetScale), DepthFormat.None);
            //source.SaveAsPng(new FileStream(@"C:\Users\Heckto\Desktop\check\ass.png", FileMode.Create), source.Width, source.Height);
            // Pass 1: draw the scene into rendertarget 1, using a shader that extracts only the brightest parts of the image.

            renderTarget1.SaveAsPng(new FileStream(@"C:\Users\Heckto\Desktop\output\ass" + i + ".png", FileMode.Create), renderTarget1.Width, renderTarget1.Height);
            i++;
            // Pass 2: draw from rendertarget 1 into rendertarget 2, using a shader to apply a horizontal gaussian blur filter.
            _gaussianBlurEffect.PrepareForHorizontalBlur();
			DrawFullscreenQuad(sb,renderTarget1, renderTarget2, _gaussianBlurEffect);

			// Pass 3: draw from rendertarget 2 back into rendertarget 1, using a shader to apply a vertical gaussian blur filter.
			_gaussianBlurEffect.PrepareForVerticalBlur();
			DrawFullscreenQuad(sb,renderTarget2, renderTarget1, _gaussianBlurEffect);

            // Pass 4: draw both rendertarget 1 and the original scene image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            game_context.graphics.SamplerStates[1] = SamplerState.LinearClamp;
			_bloomBaseMapParm.SetValue(source);

			DrawFullscreenQuad(sb,renderTarget1, destination, _bloomCombineEffect);
            renderTarget1.Dispose();
            renderTarget2.Dispose();

            //RenderTarget.ReleaseTemporary(renderTarget1);
			//RenderTarget.ReleaseTemporary(renderTarget2);
		}
	}
}