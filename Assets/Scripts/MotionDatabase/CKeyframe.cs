using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionDatabase
{
	[System.Serializable]
	public class CKeyframe
	{
		public Vector3 rootTranslation;
		public Vector4[] rotations;
        public string action;
        //public List<KeyframeEvent> events;
        public bool isIdle;
        public string annotation;
        public int frameIdx;
    }
}
