using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace MotionDatabase
{
  
    [System.Serializable]
    public class CAnimationClip
    {
        public float frame_time;
        public string clipTitle;
        public float[][] poses;
        public string[] jointSequence;
        //public List<KeyframeEvent> events;
        public string skeletonModel;

        public float getFrameTime()
        {
            return frame_time;
        }

        public int GetNumFrames()
        {
            return poses.Length;
        }

        public Vector3 getRootTranslation(int frameIdx)
        {
            Vector3 pos = new Vector3();
            pos.x = poses[frameIdx][0];
            pos.y = poses[frameIdx][1];
            pos.z = poses[frameIdx][2];
            return pos;
        }
        public float[] getPose(int frameId)
        {
            return poses[frameId];
        }

        public void translateFrames(Vector3 translation)
        {
            for (int i = 0; i < poses.Length; i++)
            {
                poses[i][0] += translation.x;
                poses[i][1] += translation.y;
                poses[i][2] += translation.z;
            }
        }

        public void scaleTranslations(float scaleFactor)
        {
            for (int i = 0; i < poses.Length; i++)
            {
                poses[i][0] *= scaleFactor;
                poses[i][1] *= scaleFactor;
                poses[i][2] *= scaleFactor;
            }
        }

        public AnimationClip ToUnityAnimationClip(MGJoint rootJoint, float scaleFactor = 100)
        {
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;
            clip.name = "mg clip " + System.DateTime.Now.ToString("yyyyMMddHHmmssffff");
            clipTitle = clip.name;
            applyRootTranslation(clip, 1.0f / scaleFactor);
            applyJointRotations(rootJoint, clip);
            /*foreach (var e in this.events)
            {
                var evt = new AnimationEvent();
                evt.functionName = e.eventName;
                evt.objectReferenceParameter = GameObject.Find(e.eventTarget);
                evt.time = e.keyframe * this.frame_time;
                clip.AddEvent(evt);
            }*/
            return clip;
        }

        private void applyRootTranslation(AnimationClip clip, float scaleFactor)
        {
            Keyframe[] k_px = new Keyframe[poses.Length];
            Keyframe[] k_py = new Keyframe[poses.Length];
            Keyframe[] k_pz = new Keyframe[poses.Length];
            for (int frameIdx = 0; frameIdx < poses.Length; frameIdx++)
            {
                float t = frame_time * frameIdx;
                k_px[frameIdx] = new Keyframe(t, poses[frameIdx][0] * scaleFactor);
                k_py[frameIdx] = new Keyframe(t, poses[frameIdx][1] * scaleFactor);
                k_pz[frameIdx] = new Keyframe(t, poses[frameIdx][2] * scaleFactor);
            }
            AnimationCurve pos_curve_x = new AnimationCurve(k_px);
            AnimationCurve pos_curve_y = new AnimationCurve(k_py);
            AnimationCurve pos_curve_z = new AnimationCurve(k_pz);
            clip.SetCurve("", typeof(Transform), "localPosition.x", pos_curve_x);
            clip.SetCurve("", typeof(Transform), "localPosition.y", pos_curve_y);
            clip.SetCurve("", typeof(Transform), "localPosition.z", pos_curve_z);

        }

        private void applyJointRotations(MGJoint joint, AnimationClip clip)
        {
            applyJointRotation(joint, clip);
            foreach (MGJoint child in joint.children)
            {
                applyJointRotations(child, clip);
            }
        }

        private void applyJointRotation(MGJoint joint, AnimationClip clip)
        {
            Keyframe[] k_qx = new Keyframe[poses.Length];
            Keyframe[] k_qy = new Keyframe[poses.Length];
            Keyframe[] k_qz = new Keyframe[poses.Length];
            Keyframe[] k_qw = new Keyframe[poses.Length];
            int o = 0;
            for (int frameIdx = 0; frameIdx < poses.Length; frameIdx++)
            {
                float t = frame_time * frameIdx;
                o = joint.indexInJointSequence * 4 + 3;
                k_qx[frameIdx] = new Keyframe(t, poses[frameIdx][o + 1]);
                k_qy[frameIdx] = new Keyframe(t, poses[frameIdx][o + 2]);
                k_qz[frameIdx] = new Keyframe(t, poses[frameIdx][o + 3]);
                k_qw[frameIdx] = new Keyframe(t, poses[frameIdx][o]);
            }
            AnimationCurve rot_curve_x = new AnimationCurve(k_qx);
            AnimationCurve rot_curve_y = new AnimationCurve(k_qy);
            AnimationCurve rot_curve_z = new AnimationCurve(k_qz);
            AnimationCurve rot_curve_w = new AnimationCurve(k_qw);

            clip.SetCurve(joint.relativePath, typeof(Transform), "localRotation.x", rot_curve_x);
            clip.SetCurve(joint.relativePath, typeof(Transform), "localRotation.y", rot_curve_y);
            clip.SetCurve(joint.relativePath, typeof(Transform), "localRotation.z", rot_curve_z);
            clip.SetCurve(joint.relativePath, typeof(Transform), "localRotation.w", rot_curve_w);
        }
    }

    [System.Serializable]
    public class CAnnotationSection
    {
        public int start_idx;
        public int end_idx;
    };
    [System.Serializable]
    public class CLegacyAnnotation
    {

        public List<CAnnotationSection> sections;
    };
    [System.Serializable]
    public class CAnnotation
    {
        public Dictionary<string, List<CAnnotationSection>> sections;
    };

}
