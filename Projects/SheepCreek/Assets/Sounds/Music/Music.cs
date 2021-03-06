﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// https://www.youtube.com/watch?time_continue=167&v=oLeAsRZ3e3I
/// </summary>
public class Music : MonoBehaviour {
	public List<AudioMixerSnapshot> levels;

	public int bpm;

	public int fadeIn;
	public int fadeOut;

	public int introFade;

	private float transitionIn;
	private float transitionOut;

	private float quarterNote;

	private int level;


	public void Start () {
		quarterNote = 60.0f / bpm;
		transitionIn = quarterNote * fadeIn;
		transitionOut = quarterNote * fadeOut;

		level = 1;

		// Fade in the music.
		levels[level].TransitionTo(quarterNote * introFade);
	}


	public void increaseLevel() {
		if(level < levels.Count - 1) {
			level += 1;

			levels[level].TransitionTo(transitionIn);
		}
	}

	public void decreaseLevel() {
		// Don't go back down to the silent level.
		if(level > 1) {
			level -= 1;

			levels[level].TransitionTo(transitionOut);
		}
	}
}
