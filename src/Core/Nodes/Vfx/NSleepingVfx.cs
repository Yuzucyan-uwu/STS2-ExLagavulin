using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace EXLagavulin.Sts2.Core.Nodes.Vfx;

public partial class NSleepingVfx : Node2D
{
	private static readonly StringName _direction = new StringName("direction");

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_sleeping");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _burstParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _continuousParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private GpuParticles2D? _zParticles;

	[Export(PropertyHint.None, "")]
	private LocalizedTexture? _localizedZTexture;

	private CancellationTokenSource? _cts;

	public static NSleepingVfx? Create(Vector2 targetTalkPosition, bool goingRight = true)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSleepingVfx nSleepingVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NSleepingVfx>(PackedScene.GenEditState.Disabled);
		nSleepingVfx.GlobalPosition = targetTalkPosition;
		nSleepingVfx.SetFloatingDirection(goingRight);
		if (nSleepingVfx != null && nSleepingVfx._zParticles != null && nSleepingVfx._localizedZTexture != null && nSleepingVfx._localizedZTexture.TryGetTexture(out Texture2D texture))
		{
			nSleepingVfx._zParticles.Texture = texture;
		}
		return nSleepingVfx;
	}

	public override void _Ready()
	{
		Play();
	}

	public override void _ExitTree()
	{
		_cts?.Cancel();
	}

	private void Play()
	{
		foreach (GpuParticles2D burstParticle in _burstParticles)
		{
			burstParticle.Restart();
		}
		foreach (GpuParticles2D continuousParticle in _continuousParticles)
		{
			continuousParticle.Restart();
			continuousParticle.Emitting = true;
		}
	}

	public void SetFloatingDirection(bool goingRight)
	{
		_zParticles.ProcessMaterial = (ParticleProcessMaterial)_zParticles.ProcessMaterial.Duplicate();
		_zParticles.ProcessMaterial.Set(_direction, new Vector3(goingRight ? 0.5f : (-0.5f), -1f, 0f));
	}

	public void Stop()
	{
		TaskHelper.RunSafely(Stopping());
	}

	private async Task Stopping()
	{
		_cts = new CancellationTokenSource();
		foreach (GpuParticles2D continuousParticle in _continuousParticles)
		{
			continuousParticle.Emitting = false;
		}
		await Cmd.Wait(5f, _cts.Token);
		this.QueueFreeSafely();
	}
}
