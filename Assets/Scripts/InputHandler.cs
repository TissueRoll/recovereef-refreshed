using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
	public class InputHandler : MonoBehaviour
	{
		[SerializeField] private MovementHandler _movementHandler;
		private void Awake()
		{
			
		}
		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void HandleInput()
		{
			_movementHandler.MovementControl();
		}
	}
}