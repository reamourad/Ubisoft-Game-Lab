using System;
using System.Collections.Generic;
using FishNet.Object;
using HighlightPlus;
using UnityEngine;

namespace Utils
{
    public class GhostHighlighterRegisterer : NetworkBehaviour
    {
        private static Dictionary<GameObject, HighlightEffect> highlightEffects = new Dictionary<GameObject, HighlightEffect>();

        public static HighlightEffect GetHighlightEffect(GameObject gameObject)
        {
            if(highlightEffects.ContainsKey(gameObject))
                return highlightEffects[gameObject];
            
            return null;
        }

        public override void OnStartClient()
        {
            highlightEffects.Add(gameObject,GetComponent<HighlightEffect>());
        }
    }
}