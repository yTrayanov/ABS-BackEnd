using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABS.Data.DynamoDb
{
    public class DynamoDBItem
    {
        private Dictionary<string, AttributeValue> _data = new Dictionary<string, AttributeValue>();

        public DynamoDBItem() { }

        internal DynamoDBItem(Dictionary<string, AttributeValue> data)
        {
            this._data = data;
        }


        public DynamoDBItem MergeData(DynamoDBItem data)
        {
            var dataDict = data.ToDictionary();
            var merged = _data.Union(dataDict).ToDictionary(k => k.Key, v => v.Value);
            return new DynamoDBItem(merged);
        }

        public bool IsEmpty
        {
            get { return _data.Count == 0; }
        }

        public string GetString(string key)
        {
            return _data.GetValueOrDefault(key)?.S;
        }

        public int GetInt32(string key)
        {
            return Convert.ToInt32(_data.GetValueOrDefault(key)?.N);
        }

        public double GetDouble(string key)
        {
            return Convert.ToDouble(_data.GetValueOrDefault(key)?.N);
        }

        public DynamoDBItem GetInnerObjectData(string key)
        {
            return new DynamoDBItem(_data.GetValueOrDefault(key)?.M); 
        }

        public bool GetBoolean(string key)
        {
            return Convert.ToBoolean(_data.GetValueOrDefault(key)?.BOOL);
        }

        public bool PkStartsWith(string prefix)
        {
            return _data.GetValueOrDefault(DynamoDBConstants.PK).S.StartsWith(prefix);
        }

        public void AddPK(string value , string prefix = "")
        {
            AddKeyAttributeValue(DynamoDBConstants.PK, new AttributeValue(prefix + value));
        }
        public void AddSK(string value, string prefix = "")
        {
            if(value != null)
                AddKeyAttributeValue(DynamoDBConstants.SK, new AttributeValue(prefix + value));
        }

        public void AddGSI1(string value, string prefix = "")
        {
            AddKeyAttributeValue(DynamoDBConstants.GSI1, new AttributeValue(prefix + value));
        }
        public void AddGSI2(string value, string prefix = "")
        {
            AddKeyAttributeValue(DynamoDBConstants.GSI2, new AttributeValue(prefix + value));
        }

        public void AddData(Dictionary<string , AttributeValue> value)
        {
            AddKeyAttributeValue(DynamoDBConstants.DATA, BaseObjectValue(value));
        }

        public void AddString(string key, string value)
        {
            AddKeyAttributeValue(key, new AttributeValue(value));
        }

        public void AddNumber(string key, int value)
        {
            AddKeyAttributeValue(key, BaseNumberAttributeValue(Convert.ToString(value)));
        }

        public void AddNumber(string key, double value)
        {
            AddKeyAttributeValue(key, BaseNumberAttributeValue(Convert.ToString(value)));
        }

        internal Dictionary<string , AttributeValue> ToDictionary() =>  _data;
        private void AddKeyAttributeValue(string key, AttributeValue value)
        {
            if (!_data.ContainsKey(key))
                _data.Add(key, value);
            else
                _data[key] = value;
        }

        private AttributeValue BaseNumberAttributeValue(string value)
        {
            var result = new AttributeValue
            {
                N = value
            };

            return result;
        }

        private AttributeValue BaseObjectValue(Dictionary<string , AttributeValue> values)
        {
            var result = new AttributeValue
            {
                M = values
            };

            return result;
        }

    }
}
