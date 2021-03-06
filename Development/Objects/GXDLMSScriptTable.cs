//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
//
// More information of Gurux products: http://www.gurux.org
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Text;
using Gurux.DLMS;
using System.ComponentModel;
using System.Xml.Serialization;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Internal;

namespace Gurux.DLMS.Objects
{
    /// <summary>
    /// Script table objects contain a table of script entries. Each entry consists of a script identifier
    /// and a series of action specifications.
    /// </summary>
    public class GXDLMSScriptTable : GXDLMSObject, IGXDLMSBase
    {
        /// <summary> 
        /// Constructor.
        /// </summary> 
        public GXDLMSScriptTable()
            : base(ObjectType.ScriptTable)
        {
            Scripts = new List<KeyValuePair<int, GXDLMSScriptAction>>();
        }

        /// <summary> 
        /// Constructor.
        /// </summary> 
        /// <param name="ln">Logical Name of the object.</param>
        public GXDLMSScriptTable(string ln)
            : base(ObjectType.ScriptTable, ln, 0)
        {
            Scripts = new List<KeyValuePair<int, GXDLMSScriptAction>>();
        }

        /// <summary> 
        /// Constructor.
        /// </summary> 
        /// <param name="ln">Logical Name of the object.</param>
        /// <param name="sn">Short Name of the object.</param>
        public GXDLMSScriptTable(string ln, ushort sn)
            : base(ObjectType.ScriptTable, ln, 0)
        {
            Scripts = new List<KeyValuePair<int, GXDLMSScriptAction>>();
        }

        [XmlIgnore()]                
        public List<KeyValuePair<int, GXDLMSScriptAction>> Scripts
        {
            get;
            set;
        }

        /// <inheritdoc cref="GXDLMSObject.GetValues"/>
        public override object[] GetValues()
        {
            return new object[] { LogicalName, Scripts };
        }

        #region IGXDLMSBase Members


        byte[][] IGXDLMSBase.Invoke(object sender, int index, Object parameters)
        {
            throw new ArgumentException("Invoke failed. Invalid attribute index.");            
        }

        int[] IGXDLMSBase.GetAttributeIndexToRead()
        {
            List<int> attributes = new List<int>();
            //LN is static and read only once.
            if (string.IsNullOrEmpty(LogicalName))
            {
                attributes.Add(1);
            }
            //Scripts
            if (!base.IsRead(2))
            {
                attributes.Add(2);
            }           
            return attributes.ToArray();
        }

        /// <inheritdoc cref="IGXDLMSBase.GetNames"/>
        string[] IGXDLMSBase.GetNames()
        {
            return new string[] { Gurux.DLMS.Properties.Resources.LogicalNameTxt, "Scripts" };
        }

        int IGXDLMSBase.GetAttributeCount()
        {
            return 2;
        }

        int IGXDLMSBase.GetMethodCount()
        {
            return 1;
        }

        override public DataType GetDataType(int index)
        {
            if (index == 1)
            {
                return DataType.OctetString;                
            }
            if (index == 2)
            {
                return DataType.Array;                
            }   
            throw new ArgumentException("GetDataType failed. Invalid attribute index.");
        }

        object IGXDLMSBase.GetValue(int index, int selector, object parameters)
        {
            if (index == 1)
            {
                return this.LogicalName;
            }
            if (index == 2)
            {
                int cnt = Scripts.Count;
                List<byte> data = new List<byte>();
                data.Add((byte) DataType.Array);
                //Add count            
                GXCommon.SetObjectCount(cnt, data);
                if (cnt != 0)
                {
                    foreach (var it in Scripts)
                    {
                        data.Add((byte) DataType.Structure);
                        data.Add(2); //Count
                        GXCommon.SetData(data, DataType.UInt16, it.Key); //Script_identifier:
                        data.Add((byte)DataType.Array);
                        data.Add(5); //Count
                        GXDLMSScriptAction tmp = it.Value;
                        GXCommon.SetData(data, DataType.Enum, tmp.Type); //service_id
                        GXCommon.SetData(data, DataType.UInt16, tmp.ObjectType); //class_id
                        GXCommon.SetData(data, DataType.OctetString, tmp.LogicalName); //logical_name
                        GXCommon.SetData(data, DataType.Int8, tmp.Index); //index
                        GXCommon.SetData(data, GXCommon.GetValueType(tmp.Parameter), tmp.Parameter); //parameter
                    }
                }
                return data.ToArray();
            }            
            throw new ArgumentException("GetValue failed. Invalid attribute index.");
        }

        void IGXDLMSBase.SetValue(int index, object value)
        {
            if (index == 1)
            {
                if (value is string)
                {
                    LogicalName = value.ToString();
                }
                else
                {
                    LogicalName = GXDLMSClient.ChangeType((byte[])value, DataType.OctetString).ToString();
                }
            }
            else if (index == 2)
            {
                Scripts.Clear();
                //Fix Xemex bug here.
                //Xemex meters do not return array as they shoul be according standard.
                if (value is Object[] && ((Object[])value).Length != 0)
                {
                    if (((Object[])value)[0] is Object[])
                    {
                        foreach (Object[] item in (Object[])value)
                        {
                            int script_identifier = Convert.ToInt32(item[0]);
                            foreach (Object[] arr in (Object[])item[1])
                            {
                                GXDLMSScriptAction it = new GXDLMSScriptAction();
                                it.Type = (GXDLMSScriptActionType)Convert.ToInt32(arr[0]);
                                it.ObjectType = (ObjectType)Convert.ToInt32(arr[1]);
                                it.LogicalName = GXDLMSClient.ChangeType((byte[])arr[2], DataType.OctetString).ToString();
                                it.Index = Convert.ToInt32(arr[3]);
                                it.Parameter = arr[4];
                                Scripts.Add(new KeyValuePair<int, GXDLMSScriptAction>(script_identifier, it));
                            }
                        }
                    }
                    else //Read Xemex meter here.
                    {
                        int script_identifier = Convert.ToInt32(((Object[])value)[0]);
                        Object[] arr = (Object[])((Object[])value)[1];
                        {
                            GXDLMSScriptAction it = new GXDLMSScriptAction();
                            it.Type = (GXDLMSScriptActionType)Convert.ToInt32(arr[0]);
                            it.ObjectType = (ObjectType)Convert.ToInt32(arr[1]);
                            it.LogicalName = GXDLMSClient.ChangeType((byte[])arr[2], DataType.OctetString).ToString();
                            it.Index = Convert.ToInt32(arr[3]);
                            it.Parameter = arr[4];
                            Scripts.Add(new KeyValuePair<int, GXDLMSScriptAction>(script_identifier, it));
                        }
                    }
                }
            }            
            else
            {
                throw new ArgumentException("SetValue failed. Invalid attribute index.");
            }
        }
        #endregion
    }
}
