using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.EntityClasses
{
	public class NursingCoral
	{
		public string coral;
		public CountdownTimer timer;

		public NursingCoral()
		{
			// nothing
		}

		public NursingCoral(string _coral, CountdownTimer _timer)
		{
			coral = _coral;
			timer = _timer;
		}

	}
}