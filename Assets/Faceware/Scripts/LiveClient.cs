using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

class TrackerData
{
	public Dictionary<string, float> animationValues;
	public TrackerData()
	{
		animationValues = new Dictionary<string, float>();
	}
}

public class LiveClient : MonoBehaviour
{
	private LiveNetworking network = new LiveNetworking();
	private LiveCharacterSetup data = new LiveCharacterSetup();
	private bool charSetupLoaded = false;

	private Dictionary< string, Vector4 > controlOffsets;

	private Dictionary< string, float > expressionValues;
	public Dictionary< string, float > ExpressionValues { get { return expressionValues; } }

	public bool ConnectOnPlay = true;
	public string Server = "localhost";
	public int Port = 802 ;
	public UnityEngine.Object ExpressionSetFile;

	/****************************************************************************************************/
	void Start()
	{
		if( ConnectOnPlay )
		{
			charSetupLoaded = LoadCharSetupFile( ExpressionSetFile.ToString() );
			if( charSetupLoaded )
			{
				List< string > missingCtrlList = LiveUnityInterface.ValidateControls( data.GetControlList() );
				if( missingCtrlList.Count > 0 )
				{
					string msg = "These controls are not in your scene:\n";
					foreach( string ctrl in missingCtrlList )
					{
						msg += ctrl;
						msg += "\n";
					}
					PrintError( msg );
				}
				controlOffsets = data.GetNeutralControlValues();
			}

			network.Connect( Server, Port );
			ConnectOnPlay = false ;
			PrintMessage( "LiveClient started." );
			PrintMessage( charSetupLoaded ? "Loaded Character Setup file." : "No Character Setup file loaded." );
			//PrintMessage( connected ? "Connected to Live Server." : "Could not connect to Live Server.  Check your hostname/port and make sure that Live Server is streaming data." );
		}
	}

	/****************************************************************************************************/
	void Update()
	{
		if( charSetupLoaded )
		{
			expressionValues = network.Update();

			if( expressionValues != null && expressionValues.Count > 0 )
			{
				Dictionary< string, Vector4 > rigValues = data.ConstructRigValues( expressionValues, controlOffsets );
				LiveUnityInterface.ApplyControlValues( rigValues );
			}
		}
	}

	/****************************************************************************************************/
	void OnApplicationQuit()
	{
		network.Disconnect();
		PrintMessage( "LiveClient stopped." );
	}

	/****************************************************************************************************/
	public LiveClient()
	{
		// nothing to do
	}

	/****************************************************************************************************/
	public void Init()
	{
		// nothing to do
	}

	/****************************************************************************************************/
	public bool LoadCharSetupFile( string filename )
	{
		return data.LoadExpressionSet( filename, true );
	}

	/****************************************************************************************************/
	public void Animate( bool value )
	{
		Debug.Log ("This Happens");
		Debug.Break ();
		ConnectOnPlay = value;
		if( ConnectOnPlay )
		{
			network.Connect( Server, Port );
		}
		else
		{
			network.Disconnect();
		}
	}

	/****************************************************************************************************/
	private void PrintMessage( string msg )
	{
		Debug.Log( "[Faceware Live] " + msg );
	}
	
	/****************************************************************************************************/
	private void PrintError( string msg )
	{
		Debug.LogError( "[Faceware Live] " + msg );
	}
}
