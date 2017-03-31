using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

public class LiveNetworking
{
	public TcpClient tcp;
	private NetworkStream stream;
	
	private bool hasNewData;
	private string data;

	private TrackerData trackerData;
	private TrackerData prevTrackerData;

	private string mHost;
	private int mPort;
	private bool reconnect;
	private float reconnectTimer;
	private float timeoutTimer;

	/****************************************************************************************************/
	public LiveNetworking()
	{
		trackerData = new TrackerData();
		prevTrackerData = null;
		reconnect = false;
	}

	/****************************************************************************************************/
	public void Connect( string host, int port )
	{
		Debug.Log ("Connecting...");
		mHost = host;
		mPort = port;
		tcp = new TcpClient( );
		tcp.BeginConnect (mHost, mPort, Connected, tcp);
	}

	public void Connected(IAsyncResult asyncResult)
	{
		Debug.Log ("Connected");
		timeoutTimer = 0.5f;

		TcpClient tcp = (TcpClient) asyncResult.AsyncState;
		tcp.EndConnect(asyncResult);

		if( tcp.Connected )
		{
			stream = tcp.GetStream();	
			StartRead();
		}
	}
	
	/****************************************************************************************************/
	public void Disconnect()
	{
		Debug.Log ("Disconnecting...");
		if( stream != null )
		{
			stream.Close();
			stream = null;
		}
		if( tcp != null )
		{
			tcp.Close();
			tcp = null;
		}
	}


	/****************************************************************************************************/
	public Dictionary< string, float > Update()
	{
		if(reconnect)
		{
			reconnectTimer -= Time.deltaTime;
			if(reconnectTimer <= 0)
			{
				Connect (mHost, mPort);
				reconnect = false;
			}
		}
		else if( tcp != null && tcp.Connected )
		{
			if( hasNewData )
			{
                hasNewData = false;
				timeoutTimer = 0.5f;
				TrackerData result = ( TrackerData )LiveJsonParser.Deserialize( prevTrackerData, trackerData, data );
				if( result != null )
				{
					prevTrackerData = trackerData;
					trackerData = result;
					StartRead();
                    return trackerData.animationValues;
				}
				else if (prevTrackerData != null)
				{
					return prevTrackerData.animationValues;
				}
                else
                {
                    Debug.Log("GRRR");
                    return null;
                }
			}
			else
			{
				timeoutTimer -= Time.deltaTime;
				if(timeoutTimer <= 0)
				{
					Debug.Log ("Lost Connection");
                    Disconnect();
                    reconnect = true;
					timeoutTimer = 0.5f;
				}
			}
		}
		return null;
	}

	/****************************************************************************************************/
	private bool StartRead()
	{
		//Debug.Log ("Reading");
		if( stream != null )
		{
			//Debug.Log ("Stream not null");
			int headerSize = 4;
			byte[] header = new byte[headerSize];
			AsyncCallback headerCB = headerRead =>
			{
				//Debug.Log ("Read Callback");
				stream.EndRead( headerRead );

				int bodySize = BitConverter.ToInt32( header, 0 );
				byte[] body = new byte[bodySize];

                //AsyncCallback bodyCB = bodyRead =>				// windows 8 prefers reading the body to be asynchronous
                //{
                //	stream.EndRead( bodyRead );
                //
                //	data = Encoding.ASCII.GetString( body );
                //	hasNewData = true;
                //	//Debug.Log ("Body?");
                //};
                //stream.BeginRead( body, 0, bodySize, bodyCB, null );
                //Debug.Log ("Read Callback Done");
                stream.Read(body, 0, bodySize);
                data = Encoding.ASCII.GetString(body);
                hasNewData = true;
            };
			hasNewData = false;
			stream.BeginRead( header, 0, headerSize, headerCB, null );
			//Debug.Log ("Read Done");

			return true;
		}
		return false;
	}
}
