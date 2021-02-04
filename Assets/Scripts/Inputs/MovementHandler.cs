using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Inputs
{
	public class MovementHandler : MonoBehaviour
	{
		[SerializeField] private CodeMonkey.MonoBehaviours.CameraFollow cameraFollow;
		private float zoom;
		private Vector3 cameraFollowPosition;
		private Vector3 savedCameraPosition;
		private bool edgeScrollingEnabled = false;
		private void Awake()
		{

		}
		// Use this for initialization
		void Start()
		{
			zoom = GameManager.instance.globalVarContainer.globals[GameManager.instance.Level].zoom;
			cameraFollowPosition = cameraFollow.transform.position;
			cameraFollow.Setup(() => cameraFollowPosition, () => zoom);
			cameraFollow.enabled = true;
		}

		// Update is called once per frame
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				edgeScrollingEnabled = !edgeScrollingEnabled;
				//FeedbackDialogue("Edge scrolling is " + (edgeScrollingEnabled ? "enabled!" : "disabled!"), globalVarContainer.globals[level].feedbackDelayTime);
			}
		}

		public void MovementControl()
		{
			MoveCameraWASD(25f);
			if (edgeScrollingEnabled) MoveCameraMouseEdge(25f, 10f);
			ZoomKeys(1.0f);
			ClampCamera();
		}

		private void MoveCameraWASD(float moveAmount)
		{
			if (Input.GetKey(KeyCode.W))
			{
				cameraFollowPosition.y += moveAmount * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.A))
			{
				cameraFollowPosition.x -= moveAmount * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.S))
			{
				cameraFollowPosition.y -= moveAmount * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.D))
			{
				cameraFollowPosition.x += moveAmount * Time.deltaTime;
			}
		}

		private void MoveCameraMouseEdge(float moveAmount, float edgeSize)
		{
			if (Input.mousePosition.x > Screen.width - edgeSize)
			{
				cameraFollowPosition.x += moveAmount * Time.deltaTime;
			}
			if (Input.mousePosition.x < edgeSize)
			{
				cameraFollowPosition.x -= moveAmount * Time.deltaTime;
			}
			if (Input.mousePosition.y > Screen.height - edgeSize)
			{
				cameraFollowPosition.y += moveAmount * Time.deltaTime;
			}
			if (Input.mousePosition.y < edgeSize)
			{
				cameraFollowPosition.y -= moveAmount * Time.deltaTime;
			}
		}

		private void ZoomKeys(float zoomChangeAmount)
		{
			if (Input.GetKey(KeyCode.Q))
			{
				zoom -= zoomChangeAmount * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.E))
			{
				zoom += zoomChangeAmount * Time.deltaTime;
			}

			if (Input.mouseScrollDelta.y > 0)
			{
				zoom -= zoomChangeAmount;
			}
			if (Input.mouseScrollDelta.y < 0)
			{
				zoom += zoomChangeAmount;
			}

			zoom = Mathf.Clamp(zoom, 1f, 5f);
		}

		private void ClampCamera()
		{
			cameraFollowPosition = new Vector3(
				Mathf.Clamp(cameraFollowPosition.x, -11.25f, 11.25f),
				Mathf.Clamp(cameraFollowPosition.y, -18.75f, 18.75f),
				cameraFollowPosition.z
			);
		}
	}
}