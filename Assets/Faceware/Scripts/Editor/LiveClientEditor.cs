using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;

[CustomEditor(typeof(LiveClient))]
public class LiveClientEditor : Editor
{

	LiveClient FwLive ;
	Texture titleIcon ;
	GUIStyle titleStyle ;

	void OnEnable ()
	{

		titleStyle = new GUIStyle () ;
		titleStyle.fontStyle = FontStyle.Bold ;
		titleStyle.fontSize = 12 ;
		titleStyle.margin = new RectOffset (5, 0, 6, 6);
		titleStyle.normal.textColor = new Color (0.7f, 0.7f, 0.7f);

		FwLive = (LiveClient)target ;

		// Faceware Icon
		string iconPath = Utility.CombinePath( "Assets", "Faceware", "Scripts", "Editor", "Icons" );
		titleIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "LiveClient_DeviceHeader.png" ), typeof( Texture ) );

	}

	public override void OnInspectorGUI(){

		EditorGUILayout.BeginVertical();
		{
			GUI.backgroundColor = new Color(0.8f, 0.85f, 0.9f, 1.0f) ;
			GUILayout.Label( titleIcon, GUILayout.Width (493) );
		
			GUILayout.Label( "Faceware Live Client for Unity", titleStyle );

			// Connect on Play?
			FwLive.ConnectOnPlay = GUILayout.Toggle(FwLive.ConnectOnPlay, "Connect To Live Server On Play") ;
		
			// Server/Port
			FwLive.Server = EditorGUILayout.TextField("Live Server Hostname:", FwLive.Server, GUILayout.Width(491)) ;
			FwLive.Port = EditorGUILayout.IntField("Live Server Port: ", FwLive.Port, GUILayout.Width(491)) ;

			// Character Setup File
			FwLive.ExpressionSetFile = EditorGUILayout.ObjectField("Character Setup File:", FwLive.ExpressionSetFile, typeof(Object), true, GUILayout.Width(490)) ;

			EditorGUILayout.Space () ;

			EditorGUILayout.LabelField( "Need Help?", titleStyle);
			EditorGUILayout.BeginHorizontal();
			{
				GUILayoutOption buttonWidth = GUILayout.Width( 244 );
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
				GUILayoutOption buttonWidth = GUILayout.Width( 492 );
				if( GUILayout.Button( "Get Your 30-Day Free Trial of Faceware Live Server", buttonWidth ) )
				{
					System.Diagnostics.Process.Start( "http://facewaretech.com/products/software/free-trial/" );
				}
			}
			EditorGUILayout.EndHorizontal();
			
		}
		EditorGUILayout.EndVertical ();
	}


}
