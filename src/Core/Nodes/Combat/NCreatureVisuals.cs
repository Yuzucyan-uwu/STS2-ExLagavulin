using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

public partial class NCreatureVisuals : Node2D
{
	private static readonly StringName _overlayInfluence = new StringName("overlay_influence");

	private static readonly StringName _h = new StringName("h");

	private static readonly StringName _tint = new StringName("tint");

	private const double _baseLiquidOverlayDuration = 1.0;

	private Node2D _body;

	private Node2D? _phobiaModeBody;

	private float _hue = 1f;

	private double _liquidOverlayTimer;

	private Material? _savedNormalMaterial;

	private ShaderMaterial? _currentLiquidOverlayMaterial;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	public Control Bounds { get; private set; }

	public Marker2D IntentPosition { get; private set; }

	public Marker2D OrbPosition { get; private set; }

	public Marker2D? TalkPosition { get; private set; }

	private bool IsSpineNode
	{
		get
		{
			if (GodotObject.IsInstanceValid(_body))
			{
				return _body.GetClass() == "SpineSprite";
			}
			return false;
		}
	}

	public bool HasSpineAnimation => SpineBody != null;

	public bool IsUsingPhobiaModeBody => _phobiaModeBody == GetCurrentBody();

	public MegaSprite? SpineBody { get; private set; }

	public SpineAnimationAccess SpineAnimation => new SpineAnimationAccess(SpineBody);

	public Marker2D VfxSpawnPosition { get; private set; }

	public float DefaultScale { get; set; } = 1f;

	public Node2D GetCurrentBody()
	{
		Node2D phobiaModeBody = _phobiaModeBody;
		if (phobiaModeBody == null || !phobiaModeBody.Visible)
		{
			return _body;
		}
		return _phobiaModeBody;
	}

	public override void _Ready()
	{
		_body = GetNode<Node2D>("%Visuals");
		_phobiaModeBody = GetNodeOrNull<Node2D>("%PhobiaModeVisuals");
		Bounds = GetNode<Control>("%Bounds");
		IntentPosition = GetNode<Marker2D>("%IntentPos");
		VfxSpawnPosition = GetNode<Marker2D>("%CenterPos");
		OrbPosition = (HasNode("%OrbPos") ? GetNode<Marker2D>("%OrbPos") : IntentPosition);
		TalkPosition = (HasNode("%TalkPos") ? GetNode<Marker2D>("%TalkPos") : null);
		if (IsSpineNode)
		{
			SpineBody = new MegaSprite(_body);
			if (SpineBody.GetSkeleton()?.GetData() == null)
			{
				GD.PushWarning($"Spine skeleton data failed to load for {base.Name}, disabling spine animation.");
				SpineBody = null;
			}
		}
		_savedNormalMaterial = null;
		_currentLiquidOverlayMaterial = null;
		UpdatePhobiaMode();
	}

	public override void _EnterTree()
	{
		_cts = new CancellationTokenSource();
		NGame.Instance?.Connect(NGame.SignalName.PhobiaModeToggled, Callable.From(UpdatePhobiaMode));
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
		NGame.Instance?.Disconnect(NGame.SignalName.PhobiaModeToggled, Callable.From(UpdatePhobiaMode));
	}

	private void UpdatePhobiaMode()
	{
		if (_phobiaModeBody != null)
		{
			_phobiaModeBody.Visible = SaveManager.Instance.PrefsSave.PhobiaMode;
			_body.Visible = !_phobiaModeBody.Visible;
		}
	}

	public void SetUpSkin(MonsterModel model)
	{
		if (SpineBody != null)
		{
			MegaSkeleton skeleton = SpineBody.GetSkeleton();
			if (skeleton != null)
			{
				model.SetupSkins(SpineBody, skeleton);
			}
		}
	}

	public void SetScaleAndHue(float scale, float hue)
	{
		DefaultScale = scale;
		base.Scale = Vector2.One * scale;
		_hue = hue;
		if (!Mathf.IsEqualApprox(hue, 0f) && SpineBody != null)
		{
			Material normalMaterial = SpineBody.GetNormalMaterial();
			ShaderMaterial shaderMaterial;
			if (normalMaterial == null)
			{
				Material material = (ShaderMaterial)PreloadManager.Cache.GetMaterial("res://materials/vfx/hsv.tres");
				shaderMaterial = (ShaderMaterial)material.Duplicate();
				SpineBody.SetNormalMaterial(shaderMaterial);
			}
			else
			{
				shaderMaterial = (ShaderMaterial)normalMaterial;
			}
			shaderMaterial.SetShaderParameter(_h, hue);
		}
	}

	public bool IsPlayingHurtAnimation()
	{
		return SpineAnimation.GetCurrentTrack()?.GetAnimation().GetName().Equals("hurt") ?? false;
	}

	public void TryApplyLiquidOverlay(Color tint)
	{
		if (_currentLiquidOverlayMaterial != null)
		{
			_currentLiquidOverlayMaterial.SetShaderParameter(_tint, tint);
			_liquidOverlayTimer = 1.0;
		}
		else
		{
			TaskHelper.RunSafely(ApplyLiquidOverlayInternal(tint));
		}
	}

	private async Task ApplyLiquidOverlayInternal(Color tint)
	{
		if (SpineBody != null)
		{
			_savedNormalMaterial = SpineBody.GetNormalMaterial();
			Material material = (ShaderMaterial)PreloadManager.Cache.GetMaterial("res://materials/vfx/potion/potion_liquid_overlay.tres");
			_currentLiquidOverlayMaterial = (ShaderMaterial)material.Duplicate();
			_currentLiquidOverlayMaterial.SetShaderParameter(_tint, tint);
			_currentLiquidOverlayMaterial.SetShaderParameter(_h, _hue);
			_currentLiquidOverlayMaterial.SetShaderParameter(_overlayInfluence, 1f);
			SpineBody.SetNormalMaterial(_currentLiquidOverlayMaterial);
			_liquidOverlayTimer = 1.0;
			while (_liquidOverlayTimer > 0.0)
			{
				double num = (1.0 - _liquidOverlayTimer) / 1.0;
				_currentLiquidOverlayMaterial.SetShaderParameter(_overlayInfluence, 1.0 - num);
				_liquidOverlayTimer -= GetProcessDeltaTime();
				await this.AwaitProcessFrame(_cts.Token);
			}
			SpineBody.SetNormalMaterial(_savedNormalMaterial);
			_currentLiquidOverlayMaterial = null;
		}
	}
}
