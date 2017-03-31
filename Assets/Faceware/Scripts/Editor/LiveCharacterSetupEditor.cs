using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class LiveCharacterSetupEditor : EditorWindow
{
	[Serializable]
	class SerializeData
	{
		public bool selectControlSelectAll;
		public bool sortSelectControl;
		public bool addedControlSelectAll;
		public bool sortAddedControl;
		public List< string > controlSelectionKeys;
		public List< bool > controlSelectionValues;
		public List< string > addedControlKeys;
		public List< bool > addedControlValues;
		public SerializeData()
		{
			controlSelectionKeys = new List<string>();
			controlSelectionValues = new List<bool>();
			addedControlKeys = new List<string>();
			addedControlValues = new List<bool>();
		}
	}

	LiveCharacterSetup charSetupData = new LiveCharacterSetup();
	SerializeData serializeData = new SerializeData();

	Dictionary< string, bool > controlSelection = new Dictionary<string, bool>();
	Dictionary< string, bool > addedControls = new Dictionary<string, bool>();

	List< UnityEngine.Object > sceneControlObjectList = new List<UnityEngine.Object>();

	string charSetupFilename = "";

	bool selectControlSelectAll = false;
	bool sortSelectControl = false;
	bool addedControlSelectAll = false;
	bool sortAddedControl = false;

	Vector2 mainScrollPos;
	Vector2 expressionScrollPos;
	Vector2 selectedControlScrollPos;
	Vector2 addedControlScrollPos;

	GUIStyle titleStyle;
	GUIStyle headingStyle ;
	GUIStyle scrollviewBgStyle ;
	GUIStyle exprSetBgStyle ;
	GUIStyle exprBgStyle ;

	Texture titleIcon;
	Texture helpIcon;
	Texture yesIcon;
	Texture noIcon;
	Texture scrollBgIcon ;
	Texture exprSetBgIcon ;


	/****************************************************************************************************/
	[MenuItem( "Window/Faceware/Character Setup" )]
	/****************************************************************************************************/
	public static void ShowWindow()
	{
		EditorWindow.GetWindow( typeof( LiveCharacterSetupEditor ), false, "Faceware" );
	}

	/****************************************************************************************************/
	void Awake()
	{
		string iconPath = Utility.CombinePath( "Assets", "Faceware", "Scripts", "Editor", "Icons" );

		titleIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "LiveClient_DeviceHeader.png" ), typeof( Texture ) );
		helpIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "Help_16.png" ), typeof( Texture ) );
		yesIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "Ok_16.png" ), typeof( Texture ) );
		noIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "No_16.png" ), typeof( Texture ) );
		scrollBgIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "scrollviewBg.png" ), typeof( Texture ) );
		exprSetBgIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "exprSetBg.png" ), typeof( Texture ) );
	}

	/****************************************************************************************************/
	public void OnEnable()
	{
		charSetupData.ReportError += delegate(string title, string message)
		{
			EditorUtility.DisplayDialog( title, message, "OK" );
		};

		selectControlSelectAll = serializeData.selectControlSelectAll;
		sortSelectControl = serializeData.sortSelectControl;
		addedControlSelectAll = serializeData.addedControlSelectAll;
		sortAddedControl = serializeData.sortAddedControl;
		controlSelection.Clear();
		for( int i = 0; i < serializeData.controlSelectionKeys.Count; i++ )
		{
			controlSelection.Add( serializeData.controlSelectionKeys[i], serializeData.controlSelectionValues[i] );
		}
		addedControls.Clear();
		for( int i = 0; i < serializeData.addedControlKeys.Count; i++ )
		{
			addedControls.Add( serializeData.addedControlKeys[i], serializeData.addedControlValues[i] );
		}
	}

	/****************************************************************************************************/
	public void OnDisable()
	{
		serializeData.selectControlSelectAll = selectControlSelectAll;
		serializeData.sortSelectControl = sortSelectControl;
		serializeData.addedControlSelectAll = addedControlSelectAll;
		serializeData.sortAddedControl = sortAddedControl;
		serializeData.controlSelectionKeys = new List<string>( controlSelection.Keys );
		serializeData.controlSelectionValues = new List<bool>( controlSelection.Values );
		serializeData.addedControlKeys = new List<string>( addedControls.Keys );
		serializeData.addedControlValues = new List<bool>( addedControls.Values );
	}

	/****************************************************************************************************/
	public void OnDestroy()
	{
		if( EditorUtility.DisplayDialog( "Save Character Setup File", "Would you like to save your Character Setup file before exiting?", "Yes", "No" ) )
		{
			SaveExpressionSet();
		}
	}

	/****************************************************************************************************/
	public void OnGUI()
	{
		titleStyle = new GUIStyle (EditorStyles.label);
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.fontSize = 12;
		titleStyle.alignment = TextAnchor.LowerLeft;

		headingStyle = new GUIStyle (EditorStyles.label);
		headingStyle.fontStyle = FontStyle.Bold;
		headingStyle.fontSize = 11;
		headingStyle.alignment = TextAnchor.LowerLeft;
		
		scrollviewBgStyle = new GUIStyle ();
		scrollviewBgStyle.normal.background = scrollBgIcon as Texture2D ;
		scrollviewBgStyle.margin = new RectOffset (5, 0, 4, 0);

		exprSetBgStyle = new GUIStyle ();
		exprSetBgStyle.normal.background = exprSetBgIcon as Texture2D ;
		exprSetBgStyle.margin = new RectOffset (5, 0, 0, 0);
		exprSetBgStyle.padding = new RectOffset (1, 1, 1, 1);

		exprBgStyle = new GUIStyle ();
		exprBgStyle.normal.background = exprSetBgIcon as Texture2D ;
		exprBgStyle.margin = new RectOffset (1, 0, 1, 1);

		EditorGUILayout.BeginVertical( GUILayout.Height( 800 ) );
		{
			GUI.backgroundColor = new Color(0.8f, 0.85f, 0.9f, 1.0f) ;
			GUILayout.Label( titleIcon, GUILayout.Width (498) );
			CreateFileGUI();
			CreateControlSetupGUI();
			CreateExpressionSetGUI();
			CreateSaveGUI();
			CreateHelpGUI();
		}
		EditorGUILayout.EndVertical();
	}

	/****************************************************************************************************/
	private string SaveExpressionSet()
	{
		string filename = EditorUtility.SaveFilePanel( "Save Character Setup", "", "", "json" );
		if( filename != null && filename != "" )
		{
			charSetupData.SaveExpressionSet( filename );
		}
		return filename;
	}

	/****************************************************************************************************/
	private void CreateFileGUI()
	{
		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.LabelField("Faceware Live Client for Unity - Character Setup", titleStyle, GUILayout.ExpandHeight( true ) );
			EditorGUILayout.LabelField( "Current File: " + charSetupFilename );
			EditorGUILayout.BeginHorizontal();
			{
				GUILayoutOption buttonWidth = GUILayout.Width( 162 );
				if( GUILayout.Button( "New", buttonWidth ) )
				{
					charSetupData.Init();
					controlSelection.Clear();
					addedControls.Clear();
					charSetupFilename = "";
					sceneControlObjectList.Clear();
				}
				if( GUILayout.Button( "Open...", buttonWidth ) )
				{
					string filename = EditorUtility.OpenFilePanel( "Open Character Setup", "", "json" );
					if( filename != null && filename != "" )
					{
						if( charSetupData.LoadExpressionSet( filename, false ) )
						{
							charSetupFilename = filename;
							controlSelection.Clear();
							InitAddedControls();
							sceneControlObjectList = LiveUnityInterface.GetControls( charSetupData.GetControlList() );
							LiveJsonParser.Serialize( charSetupData );
						}
					}
				}
				if( GUILayout.Button( "Save As...", buttonWidth ) )
				{
					charSetupFilename = SaveExpressionSet() ?? "";
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		CreateSpace (2);

	}
	
	/****************************************************************************************************/
	private void CreateControlSetupGUI()
	{
		GUILayoutOption scrollViewWidth = GUILayout.Width( 240 );
		GUILayoutOption scrollViewBtnWidth = GUILayout.Width( 242 );
		GUILayoutOption scrollViewHeight = GUILayout.Height( 200 );

		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.LabelField( "Step 1: Control Setup", titleStyle, GUILayout.ExpandHeight( true ) );
			if( GUILayout.Button( "Get Selected Scene Objects", GUILayout.Width( 494 ) ) )
			{
				InitSelectControlList();
				sortSelectControl = false;
				selectControlSelectAll = false;
			}
			CreateSpace (1);
			EditorGUILayout.BeginHorizontal( GUILayout.Width( 500 ) );
			{
				EditorGUILayout.BeginVertical();
				{
					EditorGUILayout.LabelField( "Selected Objects:", headingStyle );
					if( GUILayout.Button( "Add Controls", scrollViewBtnWidth ) )
					{
						AddControls();
						sceneControlObjectList = LiveUnityInterface.GetControls( charSetupData.GetControlList() );
						sortAddedControl = false;
						addedControlSelectAll = false;
					}
					bool checkSelectAll;
					bool checkSort;
					EditorGUILayout.BeginHorizontal();
					{
						checkSelectAll = GUILayout.Toggle( selectControlSelectAll, "Select All" );
						checkSort = GUILayout.Toggle( sortSelectControl, "Sort A-Z" );
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(scrollviewBgStyle, GUILayout.Width (200)) ;
					{
						selectedControlScrollPos = EditorGUILayout.BeginScrollView( selectedControlScrollPos, false, false, scrollViewWidth, scrollViewHeight );
						PopulateControlScrollView( controlSelection, selectControlSelectAll != checkSelectAll, checkSelectAll, checkSort );
						EditorGUILayout.EndScrollView();
					}
					EditorGUILayout.EndHorizontal();

					sortSelectControl = checkSort;
					selectControlSelectAll = checkSelectAll;
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical();
				{
					EditorGUILayout.LabelField( "Added Controls:", headingStyle );
					if( GUILayout.Button( "Remove Controls", scrollViewBtnWidth ) )
					{
						RemoveControls();
						sceneControlObjectList = LiveUnityInterface.GetControls( charSetupData.GetControlList() );
					}
					bool checkSelectAll;
					bool checkSort;
					EditorGUILayout.BeginHorizontal();
					{
						checkSelectAll = GUILayout.Toggle( addedControlSelectAll, "Select All" );
						checkSort = GUILayout.Toggle( sortAddedControl, "Sort A-Z" );
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(scrollviewBgStyle, GUILayout.Width (200)) ;
					{
						addedControlScrollPos = EditorGUILayout.BeginScrollView( addedControlScrollPos, false, false, scrollViewWidth, scrollViewHeight );
						PopulateControlScrollView( addedControls, addedControlSelectAll != checkSelectAll, checkSelectAll, checkSort );
						EditorGUILayout.EndScrollView();
					}
					EditorGUILayout.EndHorizontal();

					sortAddedControl = checkSort;
					addedControlSelectAll = checkSelectAll;
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		CreateSpace (2);
	}

	/****************************************************************************************************/
	private void PopulateControlScrollView( Dictionary< string, bool > controls, bool checkAllChanged, bool checkAll, bool sort )
	{
		EditorGUILayout.BeginVertical();
		{
			if( checkAllChanged )
			{
				if( sort )
				{
					SortedList< string, bool > display = new SortedList< string, bool >( controls );
					foreach( KeyValuePair< string, bool > kvp in display )
					{
						controls[kvp.Key] = GUILayout.Toggle( checkAll, kvp.Key );
					}
				}
				else
				{
					Dictionary<string, bool> display = new Dictionary<string, bool>( controls );
					foreach( KeyValuePair< string, bool > kvp in display )
					{
						controls[kvp.Key] = GUILayout.Toggle( checkAll, kvp.Key );
					}
				}
			}
			else
			{
				if( sort )
				{
					SortedList< string, bool > display = new SortedList< string, bool >( controls );
					foreach( KeyValuePair< string, bool > kvp in display )
					{
						controls[kvp.Key] = GUILayout.Toggle( kvp.Value, kvp.Key );
					}
				}
				else
				{
					Dictionary<string, bool> display = new Dictionary<string, bool>( controls );
					foreach( KeyValuePair< string, bool > kvp in display )
					{
						controls[kvp.Key] = GUILayout.Toggle( kvp.Value, kvp.Key );
					}
				}
			}
		}
		EditorGUILayout.EndVertical();
	}

	/****************************************************************************************************/
	private void CreateExpressionSetGUI()
	{
		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.LabelField( "Step 2: Expression Set", titleStyle, GUILayout.ExpandHeight( true ) );

			EditorGUILayout.BeginHorizontal(exprSetBgStyle, GUILayout.Width(492)) ;
			{
				expressionScrollPos = GUILayout.BeginScrollView( expressionScrollPos, false, true, GUILayout.Width( 491 ), GUILayout.Height( 300 ) );
				EditorGUILayout.BeginVertical();
				GUILayoutOption buttonWidth = GUILayout.Width( 95 );
				foreach( KeyValuePair< string, string > kvp in charSetupData.GetExpressionNameAttrList() )
				{
					EditorGUILayout.BeginHorizontal(exprBgStyle);
					{
						bool hasData = charSetupData.InUse( kvp.Value );
						GUILayout.Label( hasData ? yesIcon : noIcon, GUILayout.Width( 20 ) );
						EditorGUILayout.LabelField( kvp.Key, GUILayout.Width( 205 ) );
						if( GUILayout.Button( hasData ? "Update Pose" : "Save Pose", buttonWidth ) )
						{
							bool updateExpression = true;
							if( hasData )
							{
								updateExpression = EditorUtility.DisplayDialog( "Update Pose", "Are you sure you want to overwrite this Pose?", "Yes", "No" );
							}
							if( updateExpression )
							{
								List< string > controls = charSetupData.GetControlList();
								if( controls.Count > 0 )
								{
									charSetupData.SetControlValues( kvp.Value, LiveUnityInterface.GetControlValues( controls ) );
									charSetupData.SetInUse( kvp.Value, true );
								}
							}
						}
						if( GUILayout.Button( "Show Saved", buttonWidth ) )
						{
							Undo.RecordObjects( sceneControlObjectList.ToArray(), ( "Set '" + kvp.Key + "' Expression" ) );
							LiveUnityInterface.ApplyControlValues( charSetupData.GetControlValues( kvp.Value ) );
						}
						if( GUILayout.Button( helpIcon, EditorStyles.miniButton ) )
						{
							System.Diagnostics.Process.Start( "http://support.facewaretech.com/entries/37471737#" + kvp.Value );
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			GUILayout.EndScrollView();

			CreateSpace(1) ;
			if( GUILayout.Button( "Reset Character to 'Neutral'", GUILayout.Width( 496 ) ) )
			{
				LiveUnityInterface.ApplyControlValues( charSetupData.GetControlValues( "neutral" ) );
			}
		}
		EditorGUILayout.EndVertical();
		CreateSpace (2);
	}

	/****************************************************************************************************/
	private void CreateSaveGUI()
	{
		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.LabelField( "Step 3: Save Your Character Setup File", titleStyle, GUILayout.ExpandHeight( true ) );
			if( GUILayout.Button( "Save As...", GUILayout.Width ( 496 ) ) )
			{
				charSetupFilename = SaveExpressionSet() ?? "";
			}
		}
		EditorGUILayout.EndVertical() ;
		CreateSpace (2);
	}
	/****************************************************************************************************/
	private void CreateHelpGUI()
	{
		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.LabelField( "Need Help?", titleStyle, GUILayout.ExpandHeight( true ) );
			EditorGUILayout.BeginHorizontal();
			{
				GUILayoutOption buttonWidth = GUILayout.Width( 246 );
				if( GUILayout.Button( "Live Client for Unity - User Guide", buttonWidth ) )
				{
					System.Diagnostics.Process.Start( "http://support.facewaretech.com/forums/23050447-Live-Tutorials-and-Articles" );
				}
				if( GUILayout.Button( "Visit www.facewaretech.com", buttonWidth ) )
				{
					System.Diagnostics.Process.Start( "http://www.facewaretech.com/" );
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			{
				GUILayoutOption buttonWidth = GUILayout.Width( 496 );
				if( GUILayout.Button( "Get Your 30-Day Free Trial of Faceware Live Server", buttonWidth ) )
				{
					System.Diagnostics.Process.Start( "http://facewaretech.com/products/software/free-trial/" );
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
	}



	/****************************************************************************************************/
	private void InitSelectControlList()
	{
		List< string > controlList = new List<string>();
		if( Selection.activeGameObject != null )
		{
			foreach( GameObject selected in Selection.gameObjects )
			{
				List< string > newControls = LiveUnityInterface.GetControls( selected );
				foreach( string ctrl in newControls )
				{
					if( !controlList.Contains( ctrl ) )
					{
						controlList.Add( ctrl );
					}
				}
			}
		}

		controlSelection.Clear();
		foreach( string control in controlList )
		{
			controlSelection.Add( control, false );
		}
	}

	/****************************************************************************************************/
	private void InitAddedControls()
	{
		addedControls.Clear();
		foreach( string control in charSetupData.GetControlList() )
		{
			addedControls.Add( control, false );
		}
	}

	/****************************************************************************************************/
	private void AddControls()
	{
		List< string > newControls = new List<string>();
		foreach( KeyValuePair< string, bool > kvp in controlSelection )
		{
			if( kvp.Value )
			{
				if( !addedControls.ContainsKey( kvp.Key ) )
				{
					newControls.Add( kvp.Key );
					addedControls.Add( kvp.Key, false );
				}
			}
		}
		charSetupData.AddControls( newControls );
	}

	/****************************************************************************************************/
	private void RemoveControls()
	{
		List< string > controls = new List<string>();
		foreach( KeyValuePair< string, bool > kvp in addedControls )
		{
			if( kvp.Value )
			{
				controls.Add( kvp.Key );
			}
		}
		foreach( string control in controls )
		{
			addedControls.Remove( control );
		}
		charSetupData.RemoveControls( controls );
	}

	/****************************************************************************************************/
	private void CreateSpace(int numSpaces)
	{
		for(int i = 0; i < numSpaces; i++)
			EditorGUILayout.Space() ;
	}
		
}
