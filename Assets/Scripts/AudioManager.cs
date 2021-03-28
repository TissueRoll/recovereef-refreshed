using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
	public class AudioManager : MonoBehaviour
	{
		public static AudioManager instance = null;
		public Sound[] sounds;
		private Dictionary<string, int> nameToIndex;
		private List<int> music;
		private List<int> sfx;

		public float musicVolume = 1f;
		public float sfxVolume = 1f;

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

			nameToIndex = new Dictionary<string, int>();
			music = new List<int>();
			sfx = new List<int>();

			for (int i = 0; i < sounds.Length; ++i)
			{
				nameToIndex[sounds[i].name] = i;
				if (sounds[i].type == "Music")
				{
					music.Add(i);
				}
				else
				{
					sfx.Add(i);
				}
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

		public void AdjustMusicVolume()
		{
			foreach (int i in music)
			{
				sounds[i].source.volume = musicVolume;
			}
		}

		public void AdjustSFXVolume()
		{
			foreach (int i in sfx)
			{
				sounds[i].source.volume = sfxVolume;
			}
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