using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class LiveJsonParser
{
	/****************************************************************************************************/
	static public string Serialize( System.Object root )
	{
		return "{" + SerializeObject( root ) + "}";
	}
	
	/****************************************************************************************************/
	static private string SerializeObject( System.Object root )
	{
		StringBuilder ret = new StringBuilder();
		Type rootType = root.GetType();
		FieldInfo[] memberInfos = rootType.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField );
		
		bool firstInfo = true;
		foreach( FieldInfo info in memberInfos )
		{
			Dictionary<string, bool> attrs = ParseAttribute( info );
			ParseAttribute( info );
			if( attrs["ignore"] )
			{
				continue;
			}
			
			string value = "";
			if( typeof( IList ).IsAssignableFrom( info.FieldType ) )
			{
				value = SerializeList( info.GetValue( root ) );
			}
			else
			{
				value = GetObjStrValue( info.GetValue( root ) );
			}
			
			// assemble json string
			if( !firstInfo )
			{
				ret.Append( "," );
			}
			firstInfo = false;
			ret.Append( "\"" + info.Name + "\":" + value );
		}
		
		return ret.ToString();
	}
	
	/****************************************************************************************************/
	static private string SerializeList( System.Object objList )
	{
		StringBuilder valueList = new StringBuilder();
		bool first = true;
		valueList.Append( "[" );
		foreach( System.Object obj in objList as IList )
		{
			if( !first )
			{
				valueList.Append( "," );
			}
			first = false;
			
			if( typeof( IList ).IsAssignableFrom( obj.GetType() ) )
			{
				SerializeList( obj );
			}
			else
			{
				valueList.Append( GetObjStrValue( obj ) );
			}
		}
		valueList.Append( "]" );
		return valueList.ToString();
	}
	
	/****************************************************************************************************/
	static private string GetObjStrValue( System.Object obj )
	{
		Type objType = obj.GetType();
		if( typeof( IList ).IsAssignableFrom( objType ) )
		{
			return SerializeList( obj );
		}
		else if( typeof( string ).IsAssignableFrom( objType ) )
		{
			return "\"" + obj.ToString() + "\"";
		}
		else if( typeof( bool ).IsAssignableFrom( objType ) )
		{
			return obj.ToString();
		}
		else if( typeof( Vector4 ).IsAssignableFrom( objType ) )
		{
			string ret = "";
			Vector4 v = ( Vector4 )obj;
			for( int i = 0; i < 4; i++ )
			{
				if( float.IsNaN( v[i] ) )
				{
					break;
				}
				ret += "," + v[i];
			}
			return "[" + ret.Substring( 1 ) + "]";
		}
		else if( typeof( Expression ).IsAssignableFrom( objType ) || typeof( MetaObj ).IsAssignableFrom( objType ) )
		{
			return "{" + SerializeObject( obj ) + "}";
		}
		return "";
	}
	
	/****************************************************************************************************/
	static private Dictionary<string, bool> ParseAttribute( FieldInfo info )
	{
		Dictionary<string, bool> ret = new Dictionary<string, bool>();
		ret.Add( "ignore", false );
		ret.Add( "jsonObject", false );
		
		System.Object[] attrs = info.GetCustomAttributes( false );
		foreach( System.Object attr in attrs )
		{
			ret["ignore"] |= (attr.ToString().ToLower().EndsWith( "ignore" ) );
			ret["jsonObject"] |= (attr.ToString().ToLower().EndsWith( "jsonobject" ) );
		}
		return ret;
	}
	
	/****************************************************************************************************/
	static public System.Object Deserialize( System.Object prevObj, System.Object root, string jsonStr )
	{
		jsonStr = jsonStr.Replace( "\t", "" ).Replace( "\n", "" ).Replace( "\r", "" );
		System.Object result = DeserializeObj( root, jsonStr );
        return root;
	}
	
	/****************************************************************************************************/
	static private System.Object DeserializeObj( System.Object root, string jsonStr )
	{
		if( jsonStr.Length > 0 )
		{
			jsonStr = jsonStr.Substring( 1, jsonStr.Length - 2 );
			
			Dictionary<string, string> strObjects = ( Dictionary<string, string> )DeserializeDictionary( typeof( string ), typeof( string ), JsonStrToList( jsonStr, ',' ) );

			if( strObjects == null )
				return null;

			Type rootType = root.GetType();
			FieldInfo[] memberInfos = rootType.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField );
			
			foreach( FieldInfo info in memberInfos )
			{
				if( strObjects.ContainsKey( info.Name ) )
				{
					if( typeof( IList ).IsAssignableFrom( info.FieldType ) )
					{
						string strValue = strObjects[info.Name];
						List<string> jsonList = JsonStrToList( strValue.Substring( 1, strValue.Length - 2 ), ',' );
						
						System.Object value = DeserializeList( info.FieldType.GetGenericArguments()[0], jsonList );
						if( value != null )
						{
							info.SetValue( root, value );
						}
						else
							return null;
					}
					else if( typeof( IDictionary ).IsAssignableFrom( info.FieldType ) )
					{
						Type[] entryType = info.FieldType.GetGenericArguments();
						
						string strValue = strObjects[info.Name];
						List<string> jsonList = JsonStrToList( strValue.Substring( 1, strValue.Length - 2 ), ',' );
						
						System.Object value = DeserializeDictionary( entryType[0], entryType[1], jsonList );
						if( value != null )
						{
							info.SetValue( root, value );
						}
						else
						{
							return null;
						}
					}
					else
					{
						info.SetValue( root, CreateObjectFromType( info.FieldType, strObjects[info.Name] ) );
					}
				}
			}
			return root;
		}
		return null;
	}
	
	/****************************************************************************************************/
	static private System.Object DeserializeDictionary( Type keyType, Type valueType, List<string> strList )
	{
		Type genericClass = typeof( Dictionary<,> );
		Type constructedClass = genericClass.MakeGenericType( keyType, valueType );
		IDictionary ret = ( IDictionary )Activator.CreateInstance( constructedClass );
		foreach( string entry in strList )
		{
			List<string> pairs = JsonStrToList( entry, ':' );
			if( pairs.Count != 2 )
			{
				PrintMessage("Your Expression Set is invalid.") ;
				return null;
			}
			try
			{
				ret.Add( CreateObjectFromType( keyType, pairs[0] ), CreateObjectFromType( valueType, pairs[1] ) );
			}
			catch
			{
				return null;
			}
		}
		return ret;
	}
	
	/****************************************************************************************************/
	static private System.Object DeserializeList( Type type, List<string> strList )
	{
		if( typeof( IList ).IsAssignableFrom( type ) || typeof( IDictionary ).IsAssignableFrom( type ) )
		{
			// Doesn't support list of list or list of dictionary
		}
		else
		{
			Type genericClass = typeof( List<> );
			Type constructedClass = genericClass.MakeGenericType( type );
			IList list = ( IList )Activator.CreateInstance( constructedClass );
			foreach( string entry in strList )
			{
				list.Add( CreateObjectFromType( type, entry ) );
			}
			return list;
		}
		return null;
	}
	
	/****************************************************************************************************/
	static private System.Object CreateObjectFromType( Type type, string value )
	{
		if( typeof( string ).IsAssignableFrom( type ) )
		{
			if( value.StartsWith( "\"" ) && value.EndsWith( "\"" ) )
			{
				return value.Substring( 1, value.Length - 2 );
			}
			return value;
		}
		else if( typeof( bool ).IsAssignableFrom( type ) )
		{
			try
			{
				return Convert.ToBoolean( value );
			}
			catch
			{
				return null;
			}
		}
		else if( typeof( float ).IsAssignableFrom( type ) )
		{
			try
			{
				return Convert.ToSingle( value );
			}
			catch
			{
				return null;
			}
		}
		else if( typeof( Vector4 ).IsAssignableFrom( type ) )
		{
			float[] values = StringToFloats( value.Substring( 1, value.Length - 2 ) );
			Vector4 ret = new Vector4( float.NaN, float.NaN, float.NaN, float.NaN );
			for( int i = 0; i < values.Length; i++ )
			{
				ret[i] = values[i];
			}
			return ret;
		}
		System.Object obj = Activator.CreateInstance( type );
		DeserializeObj( obj, value );
		return obj;
	}
	
	/****************************************************************************************************/
	static private List<string> JsonStrToList( string jsonStr, char separator )
	{
		List<string> ret = new List<string>();
		if( jsonStr.Length > 0 )
		{
			int last = 0;
			Dictionary<char, char> blockMarkers = new Dictionary<char, char>() { { '[', ']' }, { '{', '}' }, { '\"', '\"' } };
			List<char> findCharList = new List<char>();
			findCharList.Add( separator );
			for( int i = 0; i < jsonStr.Length; i++ )
			{
				if( jsonStr[i] == findCharList[0] )
				{
					findCharList.RemoveAt( 0 );
					if( findCharList.Count == 0 )
					{
						ret.Add( jsonStr.Substring( last, i - last ) );
						last = i + 1;
						findCharList.Add( separator );
					}
				}
				else if( blockMarkers.ContainsKey( jsonStr[i] ) )
				{
					findCharList.Insert( 0, blockMarkers[jsonStr[i]] );
				}
			}
			ret.Add( jsonStr.Substring( last, jsonStr.Length - last ) );
		}
		return ret;
	}
	
	/****************************************************************************************************/
	static private float[] StringToFloats( string str )
	{
		try
		{
			string[] comp = str.Split( new char[] { ',' } );
			float[] ret = new float[comp.Length];
			for( int i = 0; i < comp.Length; i++ )
			{
				ret[i] = Convert.ToSingle( comp[i] );
			}
			return ret;
		}
		catch
		{
		}
		return null;
	}
	
	/****************************************************************************************************/
	static private void PrintMessage( string msg )
	{
		Debug.Log( "[Faceware Live] " + msg );
	}
}
