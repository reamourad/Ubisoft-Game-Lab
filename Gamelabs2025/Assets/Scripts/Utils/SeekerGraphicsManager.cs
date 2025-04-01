using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

namespace Utils
{
    public class SeekerGraphicsManager : NetworkBehaviour
    {
        [SerializeField] private List<MeshRenderer> meshRenderers;
        [SerializeField] private List<SkinnedMeshRenderer> skMeshRenderers;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Application.isPlaying && Application.isEditor)
            {
                if(skMeshRenderers == null)
                    skMeshRenderers = new List<SkinnedMeshRenderer>();
                if(meshRenderers == null)
                    meshRenderers = new List<MeshRenderer>();
                
                if(meshRenderers != null && meshRenderers.Count <= 0)
                    meshRenderers = GetComponentsInChildren<MeshRenderer>().ToList();
                
                if(skMeshRenderers != null && skMeshRenderers.Count <= 0)
                    skMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            }
        }
        
        public void SetRendererEnabled(bool isEnabled)
        {
            foreach (var mRenderer in meshRenderers)
            {
                mRenderer.enabled = isEnabled;
            }
                
            foreach (var skmRenderer in skMeshRenderers)
            {
                skmRenderer.enabled = isEnabled;
            }
        }
    }
}