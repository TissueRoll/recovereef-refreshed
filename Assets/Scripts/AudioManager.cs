using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
	public class AudioManager : MonoBehaviour
	{
		public static AudioManager instance;
		public Sound[] sounds;
		private Dictionary<string, int> nameToIndex;

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			} 
			else
			{
				Destroy(gameObject);
				return;
			}

			DontDestroyOnLoad(gameObject);

			for (int i = 0; i < sounds.Length; ++i)
			{
				nameToIndex[sounds[i].name] = i;
				sounds[i].source = gameObject.AddComponent<AudioSource>();
				sounds[i].source.clip = sounds[i].clip;

				sounds[i].source.volume = sounds[i].volume;
				sounds[i].source.pitch = sounds[i].pitch;
				sounds[i].source.loop = sounds[i].loop;
			}
		}

		void Start()
		{
			Play("Theme");
		}

		public void Play (string name)
		{
			if (!nameToIndex.ContainsKey(name))
			{
				print($"{name} sound not found");
				return;
			}
			Sound s = sounds[nameToIndex[name]];
			s.source.Play();
		}

	}
}