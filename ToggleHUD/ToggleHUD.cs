using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace ToggleHUD;

public class ToggleHUD : ModBehaviour
{
	public static ToggleHUD instance;

	public bool hudEnabled = true;
	public bool targetingEnabled = true;
	public bool reticleEnabled = true;

	public HUDCamera _hudCamera = null;
	public ReferenceFrameGUI _referenceFrameGUI = null;
	public ReticleController reticleController = null;

	public bool _hudCameraState = false;
	public bool _referenceFrameGUIState = false;
	public bool _reticleState = false;

	public override void Configure(IModConfig config)
	{
		base.Configure(config);
		this.hudEnabled = config.GetSettingsValue<bool>("Hud Enabled");
		this.targetingEnabled = config.GetSettingsValue<bool>("Targeting Enabled");
		this.reticleEnabled = config.GetSettingsValue<bool>("Reticle Enabled");
		this.checkHUD();
	}

	private void Awake()
	{
		Harmony.CreateAndPatchAll(typeof(ToggleHUD));
	}

	private void Start()
	{
		ToggleHUD.instance = this;

		// Starting here, you'll have access to OWML's mod helper.
		ModHelper.Console.WriteLine($"My mod {nameof(ToggleHUD)} is loaded!", MessageType.Success);


		// Example of accessing game code.
		LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
		{
			if (loadScene != OWScene.SolarSystem) return;
			ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
			this.checkHUD();
		};
	}
	private void checkHUD()
	{
		updateHud();
		updateTargeting();
		updateReticle();
	}

	public void updateHud()
	{
		if (_hudCamera == null) return;

		if (!hudEnabled)
		{
			_hudCamera._camera.gameObject.SetActive(false);
		}
		else
		{
			_hudCamera._camera.gameObject.SetActive(_hudCameraState);
		}
	}

	public void updateTargeting()
	{
		if (_referenceFrameGUI == null) return;

		if (!targetingEnabled)
		{
			// _referenceFrameGUI._canvas.gameObject.SetActive(false);
			_referenceFrameGUI.SetVisibility(false);
		}
		else
		{
			// _referenceFrameGUI._canvas.gameObject.SetActive(_referenceFrameGUI._showVisuals);
			_referenceFrameGUI.SetVisibility(_referenceFrameGUIState);
		}
	}

	public void updateReticle()
	{
		if (reticleController == null) return;

		if (!reticleEnabled)
		{
			reticleController.gameObject.SetActive(false);
		}
		else
		{
			reticleController.gameObject.SetActive(_reticleState);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(HUDCamera), "Awake")]
	public static void HudAwake(HUDCamera __instance)
	{
		ToggleHUD.instance._hudCamera = __instance;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ReferenceFrameGUI), "Awake")]
	public static void ReferenceFrameGUIAwake(ReferenceFrameGUI __instance)
	{
		ToggleHUD.instance._referenceFrameGUI = __instance;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(HUDCamera), "ActivateHUD")]
	public static void HUDCameraActivated(HUDCamera __instance)
	{
		ToggleHUD.instance._hudCameraState = true;
		ToggleHUD.instance.updateHud();
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(HUDCamera), "DeactivateHUD")]
	public static void HUDCameraDeactivated(HUDCamera __instance)
	{
		ToggleHUD.instance._hudCameraState = false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ReferenceFrameGUI), "SetVisibility")]
	public static void ReferenceFrameGuiActivated(ReferenceFrameGUI __instance)
	{
		ToggleHUD.instance._referenceFrameGUIState = __instance._showVisuals;
		if (!ToggleHUD.instance.targetingEnabled) {
			if (__instance._showVisuals) {
				__instance.SetVisibility(false);
				ToggleHUD.instance._referenceFrameGUIState = true;
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ReticleController), "Awake")]
	public static void GetReticle(ReticleController __instance)
	{
		ToggleHUD.instance.reticleController = __instance;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerCharacterController), "Start")]
	[HarmonyPatch(typeof(ProbeLauncher), "EquipTool")]
	[HarmonyPatch(typeof(ProbeLauncher), "UnequipTool")]
	public static void HandleReticle()
	{
		ToggleHUD.instance.updateReticle();
	}
}

