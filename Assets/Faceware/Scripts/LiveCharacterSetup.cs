using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Utility
{
	public static string CombinePath( params string[] list )
	{
		string ret = "";
		foreach( string item in list )
		{
			ret = Path.Combine( ret, item );
		}
		return ret;
	}
}

[Serializable]
class MetaObj
{
	public string Application;
	public string version;

	public MetaObj()
	{
		Application = "";
		version = "";
	}
}

[Serializable]
class Expression
{
	public string Name;
	public string Desc;
	public string Attr;
	public bool InUse;
	public List< Vector4 > Values;

	public Expression()
	{
		Name = "";
		Desc = "";
		Attr = "";
		InUse = false;
		Values = null;
	}
}

[Serializable]
class Data
{
	public MetaObj Meta;
	public List< string > Controls;
	public List< Expression > Expressions;
	public Data()
	{
		Meta = null;
		Controls = null;
		Expressions = null;
	}
}

[Serializable]
public class LiveCharacterSetup
{
	public delegate void ReportErrorHandler( string title, string message );
	public event ReportErrorHandler ReportError;

	static public readonly string translationSuffix = "pos";
	static public readonly string rotationSuffix = "rot";
	static readonly string currentVersion = "1.0";

	Data data;
	string expressionSetTemplateFilename;

	/****************************************************************************************************/
	public LiveCharacterSetup()
	{
		Init();
	}

	/****************************************************************************************************/
	public void Init()
	{
		// load blank expression set
		expressionSetTemplateFilename = Utility.CombinePath( "Assets", "Faceware", "ExpressionSetDefinition_Unity.json" );
		LoadExpressionSet( expressionSetTemplateFilename, false );
	}

	/****************************************************************************************************/
	public void Cleanup()
	{
	}

	/****************************************************************************************************/
	public Dictionary< string, Vector4 > GetNeutralControlValues()
	{
		Dictionary< string, Vector4 > offsets = GetControlValues( "neutral" );
		if( offsets == null || offsets.Count != data.Controls.Count )
		{
			offsets = new Dictionary<string, Vector4>();
			foreach( string control in data.Controls )
			{
				if( control.EndsWith( rotationSuffix ) )
				{
					offsets.Add( control, QuaternionToVector4( Quaternion.identity ) );
				}
				else
				{
					offsets.Add( control, Vector4.zero );
				}
			}
		}
		return offsets;
	}

	/****************************************************************************************************/
	public bool LoadExpressionSet( string file, bool fromGame )
	{
		if( fromGame )
		{
			Load( file );
		}
		else
		{
			StreamReader reader = new StreamReader( file );
			string content = reader.ReadToEnd();
			reader.Close();
			Load( content );
		}

		if( data.Meta.version != currentVersion )
		{
			UpgradeExpressionSet();
		}

		return true;
	}

	/****************************************************************************************************/
	private void Load( string content )
	{
		data = new Data();
		Data result = (Data)LiveJsonParser.Deserialize( null, data, content );

		if( result == null )
		{
			ReportError( "Load Error", "Bad file content." );
			return;
		}

		if( data.Meta.Application != "Live" )
		{
			ReportError( "Load Error", "Bad expression set definition file." );
			return;
		}

		ExpressionAttr_LD11ToRT40();
		ExpressionAttr_LD15ToLD11();
	}

	/****************************************************************************************************/
	private void ReplaceExpressionAttr( string newStr, string oldStr )
	{
		Expression expression = FindExpression( oldStr );
		if( expression != null )
		{
			expression.Attr = newStr;
		}
	}

	/****************************************************************************************************/
	private void ExpressionAttr_LD11ToRT40()
	{
		ReplaceExpressionAttr( "left_frown", "mouth_left_frown" );
	    ReplaceExpressionAttr( "lower_lip_left_down", "lip_lower_left_down" );
	    ReplaceExpressionAttr( "lower_lip_right_down", "lip_lower_right_down" );
	    ReplaceExpressionAttr( "normalFV", "mouth_phoneme_fv" );
	    ReplaceExpressionAttr( "oo_tight", "mouth_phoneme_oh_q" );
	    ReplaceExpressionAttr( "right_frown", "mouth_right_frown" );
	    ReplaceExpressionAttr( "smile_big_left", "mouth_left_smile" );
	    ReplaceExpressionAttr( "smile_big_right", "mouth_right_smile" );
	    ReplaceExpressionAttr( "upper_lip_left_up", "lip_upper_left_up" );
	    ReplaceExpressionAttr( "upper_lip_right_up", "lip_upper_right_up" );
	}

	/****************************************************************************************************/
	private void ExpressionAttr_LD15ToLD11()
	{
	    ReplaceExpressionAttr( "eyes_lookRight", "eyes_rotate_right" );
	    ReplaceExpressionAttr( "eyes_lookLeft", "eyes_rotate_left" );
	    ReplaceExpressionAttr( "eyes_lookDown", "eyes_rotate_down" );
	    ReplaceExpressionAttr( "eyes_lookUp", "eyes_rotate_up" );
	    ReplaceExpressionAttr( "eyes_leftEye_blink", "left_blink" );
	    ReplaceExpressionAttr( "eyes_rightEye_blink", "right_blink" );
	    ReplaceExpressionAttr( "brows_leftBrow_up", "left_brow_up" );
	    ReplaceExpressionAttr( "brows_leftBrow_down", "left_brow_down" );
	    ReplaceExpressionAttr( "brows_rightBrow_up", "right_brow_up" );
	    ReplaceExpressionAttr( "brows_rightBrow_down", "right_brow_down" );
	    ReplaceExpressionAttr( "brows_midBrows_up", "mid_brows_up" );
	    ReplaceExpressionAttr( "brows_midBrows_down", "mid_brows_down" );
	    ReplaceExpressionAttr( "jaw_open", "mouth_open" );
	    ReplaceExpressionAttr( "jaw_right", "jaw_rotate_y_min" );
	    ReplaceExpressionAttr( "jaw_left", "jaw_rotate_y_max" );
	    ReplaceExpressionAttr( "mouth_right", "scrunch_right" );
	    ReplaceExpressionAttr( "mouth_left", "scrunch_left" );
	    ReplaceExpressionAttr( "mouth_leftMouth_smile", "smile_big_left" );
	    ReplaceExpressionAttr( "mouth_rightMouth_smile", "smile_big_right" );
	    ReplaceExpressionAttr( "mouth_leftMouth_frown", "left_frown" );
	    ReplaceExpressionAttr( "mouth_rightMouth_frown", "right_frown" );
	    ReplaceExpressionAttr( "mouth_phoneme_oo", "oo_tight" );
	    ReplaceExpressionAttr( "mouth_upperLip_left_up", "upper_lip_left_up" );
	    ReplaceExpressionAttr( "mouth_upperLip_right_up", "upper_lip_right_up" );
	    ReplaceExpressionAttr( "mouth_lowerLip_left_down", "lower_lip_left_down" );
	    ReplaceExpressionAttr( "mouth_lowerLip_right_down", "lower_lip_right_down" );
	}

	/****************************************************************************************************/
	private void UpgradeExpressionSet()
	{
		// todo:
	}

	/****************************************************************************************************/
	public bool SaveExpressionSet( string filename )
	{
		string jsonStr = LiveJsonParser.Serialize( data );

		// write out json object
		StreamWriter writer = new StreamWriter( filename );
		writer.Write( jsonStr );
		writer.Close();
		return true;
	}

	/****************************************************************************************************/
	private Expression FindExpression( string attr )
	{
		return data.Expressions.Find( expression => expression.Attr == attr );
	}

	/****************************************************************************************************/
	public Dictionary< string, string > GetExpressionNameAttrList()
	{
		Dictionary< string, string > ret = new Dictionary< string, string >();
		foreach( Expression expression in data.Expressions )
		{
			ret.Add( expression.Name, expression.Attr );
		}
		return ret;
	}

	/****************************************************************************************************/
	public Dictionary< string, Vector4 > GetControlValues( string expressionAttr )
	{
		Dictionary< string, Vector4 > ret = new Dictionary<string, Vector4>();
		Expression expression = FindExpression( expressionAttr );
		if( expression != null )
		{
			for( int i = 0; i < data.Controls.Count; i++ )
			{
				ret.Add( data.Controls[i], expression.Values[i] );
			}
		}
		return ret;
	}

	/****************************************************************************************************/
	public bool SetControlValues( string expressionAttr, Dictionary< string, Vector4 > values )
	{
		Expression expression = FindExpression( expressionAttr );
		if( expression == null )
		{
			return false;
		}

		foreach( KeyValuePair< string, Vector4 > kvp in values )
		{
			int i = data.Controls.FindIndex( str => str == kvp.Key );
			if( i >= 0 )
			{
				expression.Values[i] = kvp.Value;
			}
		}
		return true;
	}

	/****************************************************************************************************/
	public List< string > GetControlList()
	{
		return data.Controls;
	}

	/****************************************************************************************************/
	public bool InUse( string expressionAttr )
	{
		Expression expression = FindExpression( expressionAttr );
		if( expression == null )
		{
			return false;
		}
		
		return expression.InUse;
	}

	/****************************************************************************************************/
	public void SetInUse( string expressionAttr, bool value )
	{
		Expression expression = FindExpression( expressionAttr );
		if( expression != null )
		{
			expression.InUse = value;
		}
	}

	/****************************************************************************************************/
	public void AddControls( List< string > controls )
	{
		Vector4 defaultRotationValue = new Vector4( 0, 0, 0, 1 );
		foreach( string control in controls )
		{
			if( !data.Controls.Contains( control ) )
			{
				data.Controls.Add( control );
				if( control.EndsWith( translationSuffix ) )
				{
					foreach( Expression expression in data.Expressions )
					{
						expression.Values.Add( Vector4.zero );
					}
				}
				else if( control.EndsWith( rotationSuffix ) )
				{
					foreach( Expression expression in data.Expressions )
					{
						expression.Values.Add( defaultRotationValue );
					}
				}
				else
				{
					foreach( Expression expression in data.Expressions )
					{
						expression.Values.Add( Vector4.zero );
					}
				}
			}
		}
	}

	/****************************************************************************************************/
	public void RemoveControls( List< string > controls )
	{
		foreach( string control in controls )
		{
			int index = data.Controls.IndexOf( control );
			if( index >= 0 )
			{
				data.Controls.RemoveAt( index );
				foreach( Expression expression in data.Expressions )
				{
					expression.Values.RemoveAt( index );
				}
			}
		}
	}

	/****************************************************************************************************/
	public Dictionary< string, Vector4 > ConstructRigValues( Dictionary< string, float > expressionValues, Dictionary< string, Vector4 > offsets )
	{
		Dictionary< string, Vector4 > ret = new Dictionary<string, Vector4>( offsets );

		// set ret values
		foreach( KeyValuePair< string, float > expressionValue in expressionValues )
		{
			float t = expressionValue.Value;

			if( InUse( expressionValue.Key ) )
			{
				Dictionary< string, Vector4 > controlValues = GetControlValues( expressionValue.Key );
				if( controlValues.Count > 0 )
				{
					foreach( KeyValuePair< string, Vector4 > controlValue in controlValues )
					{
						if( float.IsNaN( controlValue.Value.y ) )
						{
							Vector4 result = ret[controlValue.Key];
							result.x += controlValue.Value.x * t;
							ret[controlValue.Key] = result;
						}
						else if( float.IsNaN( controlValue.Value.w ) )
						{
							ret[controlValue.Key] = ret[controlValue.Key] + ( controlValue.Value - offsets[controlValue.Key] ) * t;
						}
						else
						{
							Quaternion diff = Vector4ToQuaternion( controlValue.Value ) * Quaternion.Inverse( Vector4ToQuaternion( offsets[controlValue.Key] ) );
							ret[controlValue.Key] = QuaternionToVector4( Quaternion.Slerp( Quaternion.identity, diff, t ) * Vector4ToQuaternion( ret[controlValue.Key] ) );
						}
					}
				}
			}
		}

		return ret;
	}
	
	/****************************************************************************************************/
	private Vector4 QuaternionToVector4( Quaternion value )
	{
		return new Vector4( value.x, value.y, value.z, value.w );
	}

	/****************************************************************************************************/
	private Quaternion Vector4ToQuaternion( Vector4 value )
	{
		return new Quaternion( value.x, value.y, value.z, value.w );
	}
}
