#define ONLY_USE_TOP
// #define ONLY_USE_Y

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections;

// see https://connect.unity.com/p/updating-your-gui-for-the-iphone-x-and-other-notched-devices

[ExecuteInEditMode]
public class ScreenSafeArea : MonoBehaviour {
	//private readonly Utility.ConditionalDebug DEBUG_SCREENSAFE = new Utility.ConditionalDebug(true, "ScreenSafeArea");

	private DrivenRectTransformTracker drivenRectTransformTracker;

	Rect lastSafeArea = Rect.zero;
	bool insideUpdateDrivenRect = false;

	[SerializeField]
	private bool aggressiveTest = false;
	[SerializeField]
	private bool iPhoneXTest = false;

	void OnValidate() {
#if ONLY_USE_TOP && ONLY_USE_Y
		//Utility_Assert.Unconditional("define at most ONE of ONLY_USE_TOP and ONLY_USE_Y");
#endif
		//Utility_Assert.HasComponent<RectTransform>(this, "ScreenSafeArea adjusts the RectTransform");
	}

	void Awake() {
	}

	void Start() {
		UpdateDrivenRectIfNeeded();
	}

	void Update() {
		UpdateDrivenRectIfNeeded();
	}
	// // //

	void OnDisable() {
		drivenRectTransformTracker.Clear();
		LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());
	}

	void OnEnable() {
		UpdateDrivenRectIfNeeded();
	}

	void OnRectTransformDimensionsChange() {
		UpdateDrivenRectIfNeeded();
	}

	protected void OnTransformParentChanged() {
		UpdateDrivenRectIfNeeded();
	}

	public virtual void SetLayoutHorizontal() { }

	public virtual void SetLayoutVertical() { }

	// // //

	void UpdateDrivenRectIfNeeded() {
		if (insideUpdateDrivenRect)
			return;

		Rect safeArea = Screen.safeArea;
		if (safeArea == Rect.zero) {
			safeArea = new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height));
		}


		if (lastSafeArea == safeArea)
			return;

		lastSafeArea = safeArea;

		insideUpdateDrivenRect = true;

		drivenRectTransformTracker.Clear();

		drivenRectTransformTracker.Add(this, GetComponent<RectTransform>(),
			DrivenTransformProperties.AnchoredPosition
			| DrivenTransformProperties.AnchoredPositionZ
			| DrivenTransformProperties.AnchorMin
			| DrivenTransformProperties.AnchorMax
			| DrivenTransformProperties.SizeDeltaX
			| DrivenTransformProperties.SizeDeltaY
			| DrivenTransformProperties.Pivot
			| DrivenTransformProperties.Scale
			| DrivenTransformProperties.Rotation
			);

		GetComponent<RectTransform>().offsetMin = Vector2.zero;
		GetComponent<RectTransform>().offsetMax = Vector2.zero;

		GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
		GetComponent<RectTransform>().localScale = Vector3.one;
		GetComponent<RectTransform>().localRotation = Quaternion.identity;

		GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;

		Vector2 anchorMin = lastSafeArea.position;
		Vector2 anchorMax = lastSafeArea.position + lastSafeArea.size;
		
#if ONLY_USE_TOP
		anchorMin.x = 0.0f;
		anchorMin.y = 0.0f;
		anchorMax.x = 1.0f;
#elif ONLY_USE_Y
		anchorMin.x = 0.0f;
		anchorMin.y /= Screen.height;
		anchorMax.x = 1.0f;
#else
		anchorMin.x /= Screen.width;
		anchorMin.y /= Screen.height;
		anchorMax.x /= Screen.width;
#endif
		anchorMax.y /= Screen.height;

		GetComponent<RectTransform>().anchorMin = anchorMin;
		GetComponent<RectTransform>().anchorMax = anchorMax;

		insideUpdateDrivenRect = false;
	}
}
