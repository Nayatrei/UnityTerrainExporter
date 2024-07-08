/// <summary>
/// Dvornik
/// </summary>
using System;
using System.IO;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

/// <summary>
/// Split terrain.
/// </summary>
public class CsTerrainController : EditorWindow {

	//string num = "1";
	float terrainRaiseAmount;
	private int lodLevel = 50;
	List<TerrainData> terrainData = new List<TerrainData>();
	List<GameObject> terrainGo = new List<GameObject>();

	private Terrain selectedTerrain;

	[HideInInspector] public List<float[,]> undoHeights = new List<float[,]>();

	Terrain originalTerrain;
	
	const int terrainsCount = 1;		
	
	// Add submenu
    [MenuItem("Tools/Terrain/Terrain Tools")]
	static void Init()
    {
		
		// Get existing open window or if none, make a new one:
		CsTerrainController window = (CsTerrainController)EditorWindow.GetWindow(typeof(CsTerrainController));
		
		window.autoRepaintOnSceneChange = true;
       	window.titleContent.text = "Terrain Controller";
       	window.Show();
							
			
	}
	void OnGUI()
    {

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Modify Terrain Height", EditorStyles.boldLabel);

		EditorGUILayout.Separator();


		GUILayout.Label("Terrain Raise Amount: " + terrainRaiseAmount.ToString("0.00"));
		terrainRaiseAmount = EditorGUILayout.Slider(terrainRaiseAmount, -100, 100);

		if (GUILayout.Button("Raise Terrain"))
		{

			if (Selection.activeGameObject == null)
			{
				Debug.LogWarning("No terrain was selected");
				return;
			}

			originalTerrain = Selection.activeGameObject.GetComponent(typeof(Terrain)) as Terrain;
			TerrainData terrainData = originalTerrain.terrainData;
			int heightmapWidth = terrainData.heightmapResolution;
			int heightmapHeight = terrainData.heightmapResolution;
			float[,] currentHeights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
			undoHeights.Add(currentHeights);
			float[,] newHeights = new float[heightmapWidth, heightmapHeight];
			float terrainHeight = terrainData.size.y;
			for (int y = 0; y < heightmapWidth; y++)
			{
				for (int x = 0; x < heightmapHeight; x++)
				{
					newHeights[y, x] = Mathf.Clamp01(currentHeights[y, x] + (terrainRaiseAmount / terrainHeight));
				}
			}
			terrainData.SetHeights(0, 0, newHeights);

		}

		if (GUILayout.Button("Undo Raise"))
		{
			if (Selection.activeGameObject == null)
			{
				Debug.LogWarning("No terrain was selected");
				return;
			}

			// get last heights
			float[,] newHeights = undoHeights[undoHeights.Count - 1];

			// apply to terrain
			originalTerrain.terrainData.SetHeights(0, 0, newHeights);

			// remove from list
			undoHeights.RemoveAt(undoHeights.Count - 1);

			Debug.Log("Undo Heights completed");


		}

		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Clone Terrain", EditorStyles.boldLabel);
		EditorGUILayout.Separator();

		if (GUILayout.Button("Clone Terrain",GUILayout.Height(30)))
        {			
			
			CopyIt();							
		}

		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Terrain to Mesh Tool", EditorStyles.boldLabel);
		EditorGUILayout.Separator();
		selectedTerrain = EditorGUILayout.ObjectField("Selected Terrain", selectedTerrain, typeof(Terrain), true) as Terrain;


		EditorGUILayout.LabelField("Export Tree as GameObjects", EditorStyles.boldLabel);
		EditorGUILayout.Separator();

		if (GUILayout.Button("Convert Trees"))
		{
			if (selectedTerrain != null)
			{
				ConvertTrees();
			}
			else
			{
				EditorUtility.DisplayDialog("Error", "Please select a terrain first.", "OK");
			}
		}

		GUILayout.Label("Convert Terrain to Mesh", EditorStyles.boldLabel);

		lodLevel = EditorGUILayout.IntSlider("Level of Detail (1-100)", lodLevel, 1, 100);

		if (GUILayout.Button("Generate FBX"))
		{
			if (selectedTerrain != null)
			{
				GenerateAndExportTerrainMesh();
			}
			else
			{
				EditorUtility.DisplayDialog("Error", "Please select a terrain first.", "OK");
			}
		}

		if (GUILayout.Button("Export SplatMaps",GUILayout.Height(30)))
        {			
			
			ExportSplat();							
		}
												
	}
			
	void CopyIt()
	{
		
		if ( Selection.activeGameObject == null )
		{
			Debug.LogWarning("No terrain was selected");
			return;
		}
		
		
		originalTerrain = Selection.activeGameObject.GetComponent(typeof(Terrain)) as Terrain;		
		
		if ( originalTerrain == null )
		{
			Debug.LogWarning("Current selection is not a terrain");
			return;
		}
						
			TerrainData selectedTerrainData = new TerrainData();
			GameObject targetTerrainObject = Terrain.CreateTerrainGameObject( selectedTerrainData );
		
			targetTerrainObject.name = originalTerrain.name + " " + DateTime.Now.ToString("MMdd");
			
			terrainData.Add( selectedTerrainData );
			terrainGo.Add ( targetTerrainObject );
			
			Terrain newTerrain = targetTerrainObject.GetComponent(typeof(Terrain)) as Terrain;								
			newTerrain.terrainData = selectedTerrainData;

			AssetDatabase.CreateAsset(selectedTerrainData, "Assets/" + newTerrain.name+ ".asset");

			//Copy Initial Data

			newTerrain.terrainData.terrainLayers = originalTerrain.terrainData.terrainLayers;
			newTerrain.terrainData.detailPrototypes = originalTerrain.terrainData.detailPrototypes;
			newTerrain.terrainData.treePrototypes = originalTerrain.terrainData.treePrototypes;
			newTerrain.basemapDistance = originalTerrain.basemapDistance;			
			newTerrain.shadowCastingMode = originalTerrain.shadowCastingMode;
			newTerrain.detailObjectDensity = originalTerrain.detailObjectDensity;
			newTerrain.detailObjectDistance = originalTerrain.detailObjectDistance;
			newTerrain.heightmapMaximumLOD = originalTerrain.heightmapMaximumLOD;
			newTerrain.heightmapPixelError = originalTerrain.heightmapPixelError;
			newTerrain.treeBillboardDistance = originalTerrain.treeBillboardDistance;
			newTerrain.treeCrossFadeLength = originalTerrain.treeCrossFadeLength;
			newTerrain.treeDistance = originalTerrain.treeDistance;
			newTerrain.treeMaximumFullLODCount = originalTerrain.treeMaximumFullLODCount;

			//Copy Height data

			Vector3 originalPosition = originalTerrain.GetPosition();
			targetTerrainObject.transform.position = new Vector3( targetTerrainObject.transform.position.x,targetTerrainObject.transform.position.y,targetTerrainObject.transform.position.z); 	
			targetTerrainObject.transform.position = new Vector3( targetTerrainObject.transform.position.x + originalPosition.x,targetTerrainObject.transform.position.y + originalPosition.y,targetTerrainObject.transform.position.z + originalPosition.z);
			
			selectedTerrainData.heightmapResolution = originalTerrain.terrainData.heightmapResolution;							
			
			selectedTerrainData.size = new Vector3( originalTerrain.terrainData.size.x,originalTerrain.terrainData.size.y,originalTerrain.terrainData.size.z);
			
			float[,] originalTerrainHeightPoints = originalTerrain.terrainData.GetHeights(0,0, originalTerrain.terrainData.heightmapResolution, originalTerrain.terrainData.heightmapResolution );
			
			float[,] copiedHeightPoints = new float[ originalTerrain.terrainData.heightmapResolution,originalTerrain.terrainData.heightmapResolution];
			
			int terrainEndXPos = 0;
			int terrainEndYPos = 0;
			
			terrainEndXPos = terrainEndYPos = originalTerrain.terrainData.heightmapResolution;

			for ( int x=0;x< terrainEndXPos;x++)
			{	
				for ( int y=0;y< terrainEndYPos;y++)
				{
					float pointHeight = originalTerrainHeightPoints[x,y];	
					copiedHeightPoints[x ,y ] = pointHeight;		
				}
			}

			newTerrain.terrainData.SetHeights( 0,0, copiedHeightPoints );
								
			selectedTerrainData.alphamapResolution = originalTerrain.terrainData.alphamapResolution;													
			
			float[,,] storeSplat = originalTerrain.terrainData.GetAlphamaps(0,0, originalTerrain.terrainData.alphamapResolution, originalTerrain.terrainData.alphamapResolution );			

			float[,,] pasteSplat = new float[ originalTerrain.terrainData.alphamapResolution,originalTerrain.terrainData.alphamapResolution, originalTerrain.terrainData.alphamapLayers];
									
			terrainEndXPos = terrainEndYPos = originalTerrain.terrainData.alphamapResolution;
			
			//Copy Alpha data

			for (int alpha=0; alpha < originalTerrain.terrainData.alphamapLayers; alpha++)
			{				
				for ( int x=0;x< terrainEndXPos;x++)
				{	
					for ( int y=0;y< terrainEndYPos;y++)
					{
						float pointHeight = storeSplat[x,y, alpha];	
						pasteSplat[x ,y, alpha] = pointHeight;
					}
				}			
			}

			newTerrain.terrainData.SetAlphamaps( 0,0, pasteSplat );

			//Copy Detail data
				
			selectedTerrainData.SetDetailResolution( originalTerrain.terrainData.detailResolution, 8 );													
						
		for ( int detLay=0; detLay< originalTerrain.terrainData.detailPrototypes.Length; detLay++)
		{ 								
			int[,] storeDetail = originalTerrain.terrainData.GetDetailLayer(0,0, originalTerrain.terrainData.detailResolution, originalTerrain.terrainData.detailResolution, detLay );			
	
			int[,] pasteDetail = new int[ originalTerrain.terrainData.detailResolution, originalTerrain.terrainData.detailResolution];

			terrainEndXPos = terrainEndYPos = originalTerrain.terrainData.detailResolution;

			for (int x = 0; x < terrainEndXPos; x++)
			{
				for (int y = 0; y < terrainEndYPos; y++)
				{
					int ph = storeDetail[x, y];
					pasteDetail[x, y] = ph;
				}
			}
				newTerrain.terrainData.SetDetailLayer( 0,0, detLay, pasteDetail );
		}

			//Copy Tree Data

			for( int t=0; t< originalTerrain.terrainData.treeInstances.Length;t++)
			{
					
				TreeInstance treeInstance = originalTerrain.terrainData.treeInstances[t];				
	
				if (treeInstance.position.x > 0f &&	treeInstance.position.x < 0.5f && treeInstance.position.z > 0f &&	treeInstance.position.z < 0.5f)
				{
					treeInstance.position = new Vector3( treeInstance.position.x, treeInstance.position.y, treeInstance.position.z);
					newTerrain.AddTreeInstance( treeInstance );												
				}
				
			}											
			AssetDatabase.SaveAssets();
	}

	private void ConvertTrees()
	{

		if (Selection.activeGameObject == null)
		{
			Debug.LogWarning("No terrain was selected");
			return;
		}


		TreeInstance[] treeInstances = selectedTerrain.terrainData.treeInstances;
		Dictionary<string, GameObject> parentGroups = new Dictionary<string, GameObject>();

		for (int i = 0; i < treeInstances.Length; i++)
		{
			TreeInstance treeInstance = treeInstances[i];
			TreePrototype treePrototype = selectedTerrain.terrainData.treePrototypes[treeInstance.prototypeIndex];

			// Create or find the parent group for this type of tree
			string groupName = treePrototype.prefab.name + "_Group";
			if (!parentGroups.ContainsKey(groupName))
			{
				GameObject groupObject = new GameObject(groupName);
				parentGroups[groupName] = groupObject;
			}

			GameObject treeObject = Instantiate(treePrototype.prefab);
			treeObject.transform.position = Vector3.Scale(treeInstance.position, selectedTerrain.terrainData.size) + selectedTerrain.transform.position;
			treeObject.transform.localScale = new Vector3(treeInstance.widthScale, treeInstance.heightScale, treeInstance.widthScale);
			treeObject.transform.rotation = Quaternion.AngleAxis(treeInstance.rotation * Mathf.Rad2Deg, Vector3.up);
			treeObject.transform.parent = parentGroups[groupName].transform; // Set the parent of the treeObject
		}

		// Optional: Clear all trees from terrain after conversion
		selectedTerrain.terrainData.treeInstances = new TreeInstance[0];
		EditorUtility.SetDirty(selectedTerrain);
		selectedTerrain.Flush();
	}

	private void GenerateAndExportTerrainMesh()
	{
		GameObject meshObj = GenerateTerrainMesh();
		string path = EditorUtility.SaveFilePanel("Save Mesh as FBX", "", meshObj.name, "fbx");

		if (!string.IsNullOrEmpty(path))
		{
			// Convert path to relative for Unity
			path = FileUtil.GetProjectRelativePath(path);

			// Export the GameObject as an FBX file
			ModelExporter.ExportObject(path, meshObj);
			Debug.Log("Mesh exported to: " + path);

			// Clean up the GameObject after export
			DestroyImmediate(meshObj);
		}
	}

	private GameObject GenerateTerrainMesh()
	{
		TerrainData terrainData = selectedTerrain.terrainData;
		int w = terrainData.heightmapResolution;
		int h = terrainData.heightmapResolution;
		Vector3 size = terrainData.size;
		int resolution = Mathf.Clamp((int)(lodLevel / 100f * w), 1, w);
		float[,] heights = terrainData.GetHeights(0, 0, w, h);

		Vector3[] vertices = new Vector3[resolution * resolution];
		int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
		Vector2[] uv = new Vector2[resolution * resolution];

		// Create vertices and UVs
		for (int i = 0; i < resolution; i++)
		{
			for (int j = 0; j < resolution; j++)
			{
				int index = i * resolution + j;
				float x = (float)j / (resolution - 1) * size.x;
				float z = (float)i / (resolution - 1) * size.z;
				float y = heights[(int)(i * ((float)w / resolution)), (int)(j * ((float)h / resolution))] * size.y;
				vertices[index] = new Vector3(x, y, z);
				uv[index] = new Vector2((float)j / (resolution - 1), (float)i / (resolution - 1));
			}
		}

		// Create triangles
		int triIndex = 0;
		for (int i = 0; i < resolution - 1; i++)
		{
			for (int j = 0; j < resolution - 1; j++)
			{
				int topLeft = i * resolution + j;
				int topRight = topLeft + 1;
				int bottomLeft = (i + 1) * resolution + j;
				int bottomRight = bottomLeft + 1;

				triangles[triIndex++] = topLeft;
				triangles[triIndex++] = bottomLeft;
				triangles[triIndex++] = topRight;
				triangles[triIndex++] = topRight;
				triangles[triIndex++] = bottomLeft;
				triangles[triIndex++] = bottomRight;
			}
		}

		// Create a new mesh
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		mesh.RecalculateNormals();

		// Create a GameObject to hold the mesh
		GameObject meshObj = new GameObject("Terrain Mesh");
		MeshFilter mf = meshObj.AddComponent<MeshFilter>();
		mf.mesh = mesh;
		MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
		mr.material = new Material(Shader.Find("Standard"));

		return meshObj;
	}


	void ExportSplat()
	{
		TerrainData terrainData = selectedTerrain.terrainData;
		Texture2D[] splatmapTextures = terrainData.alphamapTextures;

		for (int i = 0; i < splatmapTextures.Length; i++)
		{
			Texture2D splatmap = splatmapTextures[i];
			string path = Path.Combine("Assets", selectedTerrain.name + "_Splatmap_" + i + ".tga");

			// Check if the file exists to handle overwriting properly
			if (File.Exists(path))
			{
				Debug.Log("Overwriting existing file: " + path);
			}

			byte[] bytes = splatmap.EncodeToTGA();
			File.WriteAllBytes(path, bytes);
			Debug.Log("Splatmap " + i + " exported to: " + path);
		}

		AssetDatabase.Refresh();  // Refresh the AssetDatabase to show new files
	}
}
