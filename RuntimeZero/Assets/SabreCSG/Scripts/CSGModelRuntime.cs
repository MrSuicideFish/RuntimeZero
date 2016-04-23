using UnityEngine;
using System.Collections;

namespace Sabresaurus.SabreCSG
{
	public class CSGModelRuntime : MonoBehaviour
	{
	    // Use this for initialization
	    void Start()
	    {	
#if !RUNTIME_CSG
			Transform meshGroup = transform.FindChild("MeshGroup");
			if(meshGroup != null)
			{
				// Reanchor the meshes to the root
				meshGroup.parent = null;
			}

			// Remove this game object
			Destroy (this.gameObject);	
#endif
	    }
	}
}