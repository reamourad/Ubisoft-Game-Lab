using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Tutorial
{
    public class VideoPlayer: MonoBehaviour
    {

        [SerializeField] private UnityEngine.Video.VideoPlayer videoPlayer;

        public void ChangeSlide(VideoClip videoClip)
        {
            videoPlayer.clip = videoClip;
            videoPlayer.Prepare();
            videoPlayer.Play();
        }

        public void Stop()
        {
            videoPlayer.Stop();
        }
    }
}